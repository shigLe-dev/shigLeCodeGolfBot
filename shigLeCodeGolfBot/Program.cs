using paizaIOSharp;
using System.Text.Json.Nodes;
using System.Configuration;
using Discord;
using Discord.WebSocket;

namespace shigLeCodeGolfBot;

class Program
{
    DiscordSocketClient client;
    HttpClient httpClient;
    public Dictionary<ulong, Func<SocketSlashCommand, Task>> commands = new Dictionary<ulong, Func<SocketSlashCommand, Task>>();
    public Dictionary<ulong, CodeGolf> codeGolfs = new Dictionary<ulong, CodeGolf>();

    public async Task MainAsync()
    {
        httpClient = new HttpClient();
        client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.All
        });
        client.Log += LogAsync;
        client.Ready += OnReady;
        client.MessageReceived += OnMessage;
        client.SlashCommandExecuted += OnSlashCommand;
        await client.LoginAsync(TokenType.Bot, ConfigurationManager.AppSettings["token"]);
        await client.StartAsync();
        await Task.Delay(Timeout.Infinite);
    }

    private async Task OnReady()
    {
        Console.WriteLine("OnReady");

        await SetCommand(1031157559404548107);
        await SetCommand(811964339375308890);
    }

    private async Task SetCommand(ulong guildId)
    {
        var guild = client.GetGuild(guildId);

        try
        {
            commands[(await guild.CreateApplicationCommandAsync(BuildCreateCodeGolfCommand())).Id] = OnCreateCodeGolfCommand;
            commands[(await guild.CreateApplicationCommandAsync(BuildRemoveCodeGolfCommand())).Id] = OnRemoveCodeGolfCommand;
            commands[(await guild.CreateApplicationCommandAsync(BuildShowAllCodeGolfCommand())).Id] = OnShowAllCodeGolfCommand;
            commands[(await guild.CreateApplicationCommandAsync(BuildAddPlayerCommand())).Id] = OnAddPlayerCommand;
            commands[(await guild.CreateApplicationCommandAsync(BuildChangePlayerTeamCommand())).Id] = OnChangePlayerTeamCommand;
            commands[(await guild.CreateApplicationCommandAsync(BuildShowAllPlayerCommand())).Id] = OnShowAllPlayerCommand;
            commands[(await guild.CreateApplicationCommandAsync(BuildRunProgramCommand())).Id] = OnRunProgramCommand;
        }
        catch (System.Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    #region BuildCommand
    private SlashCommandProperties BuildCreateCodeGolfCommand()
    {
        var guildCommand = new SlashCommandBuilder()
            .WithName("create_codegolf")
            .WithDescription("このコマンドを使用することで、新たにCodeGolfを始めることが出来ます。")
            .AddOption("name", ApplicationCommandOptionType.String, "CodeGolfの名前を設定してください。", isRequired: true)
            .AddOption("settingsurl", ApplicationCommandOptionType.String, "設定を記述しているファイルへのURLを設定してください。", isRequired: true);
        return guildCommand.Build();
    }

    private SlashCommandProperties BuildRemoveCodeGolfCommand()
    {
        var guildCommand = new SlashCommandBuilder();

        guildCommand.WithName("remove_codegolf");
        guildCommand.WithDescription("このコマンドを使用することで、既存のCodeGolfを削除することが出来ます。");

        return guildCommand.Build();
    }

    private SlashCommandProperties BuildShowAllCodeGolfCommand()
    {
        var guildCommand = new SlashCommandBuilder();

        guildCommand.WithName("show_allcodegolf");
        guildCommand.WithDescription("このコマンドを使用することで、サーバー内のすべてのCodeGolfを表示することが出来ます。");

        return guildCommand.Build();
    }

    private SlashCommandProperties BuildAddPlayerCommand()
    {
        SlashCommandBuilder builder = new SlashCommandBuilder()
            .WithName("add_player")
            .WithDescription("このコマンドを使用することで、現在このスレッドで実行されているCodeGolfにプレイヤーを追加することが出来ます。")
            .AddOption("user", ApplicationCommandOptionType.User, "追加するUser", isRequired: true)
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("team")
                .WithDescription("追加するTeam")
                .WithType(ApplicationCommandOptionType.Integer)
                .WithRequired(true)
                .AddChoice("Red", 0)
                .AddChoice("Blue", 1));
        return builder.Build();
    }

    private SlashCommandProperties BuildChangePlayerTeamCommand()
    {
        return new SlashCommandBuilder()
            .WithName("change_team")
            .WithDescription("このコマンドを使用することで、参加中のプレイヤーのチームを変更することが出来ます。")
            .AddOption("user", ApplicationCommandOptionType.User, "変更するUser", isRequired: true)
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("team")
                .WithDescription("変更先のTeam")
                .WithType(ApplicationCommandOptionType.Integer)
                .WithRequired(true)
                .AddChoice("Red", 0)
                .AddChoice("Blue", 1))
            .Build();
    }

    private SlashCommandProperties BuildShowAllPlayerCommand()
    {
        return new SlashCommandBuilder()
            .WithName("show_allplayer")
            .WithDescription("このコマンドを使用することで、このCodeGolfに参加しているPlayerを表示することが出来ます。").Build();
    }

    private SlashCommandProperties BuildRunProgramCommand()
    {
        SlashCommandBuilder builder = new SlashCommandBuilder();
        SlashCommandOptionBuilder optionBuilder = new SlashCommandOptionBuilder()
            .WithName("language")
            .WithType(ApplicationCommandOptionType.Integer)
            .WithDescription("実行する言語を指定してください。")
            .WithRequired(true);

        for (int i = 0; i < CodeGolfLanguage.Languages.Count; i++)
        {
            optionBuilder.AddChoice(CodeGolfLanguage.Languages[i], i);
        }

        builder.WithName("run")
            .WithDescription("このコマンドを使用することで、プログラムを実行することが出来ます。")
            .AddOption(optionBuilder)
            .AddOption("code", ApplicationCommandOptionType.String, "実行するプログラムを記述してください。", isRequired: true)
            .AddOption("input", ApplicationCommandOptionType.String, "実行時に入力される文字列を記述してください。", isRequired: false);

        return builder.Build();
    }
    #endregion

    #region OnCommand
    private async Task OnCreateCodeGolfCommand(SocketSlashCommand command)
    {
        IThreadChannel threadChannel;
        CodeGolfSettings settings = null;
        string name = (string)command.Data.Options.ToArray()[0];
        string settingsUrl = (string)command.Data.Options.ToArray()[1];

        // 設定ファイル読み込み関係
        try
        {
            settings = CodeGolfSettings.Parse(JsonNode.Parse(httpClient.GetAsync(settingsUrl).Result.Content.ReadAsStringAsync().Result));
        }
        catch (System.Exception e)
        {
            Console.WriteLine(e.Message);
            await command.RespondAsync($"設定ファイルの読み込みに失敗しました。");
            return;
        }
        // スレッド生成関係
        try
        {
            threadChannel = await CreateThread("CodeGolf", command.Channel as ITextChannel);
        }
        catch (System.Exception)
        {
            await command.RespondAsync($"スレッドの生成に失敗しました。");
            return;
        }
        await threadChannel.SendMessageAsync("CodeGolfの始まりです。");
        await command.RespondAsync($"{command.User.Mention}さんがCodeGolfを開始しました。");
        codeGolfs[threadChannel.Id] = new CodeGolf(name, threadChannel, command.User.Id, settingsUrl, settings);
        Console.WriteLine(codeGolfs[threadChannel.Id].name);
    }

    private async Task OnRemoveCodeGolfCommand(SocketSlashCommand command)
    {
        ulong threadId = command.ChannelId ?? 0;
        ulong userId = command.User.Id;

        // CodeGolfを実行しているスレッドか
        if (!codeGolfs.TryGetValue(threadId, out var codeGolf))
        {
            await command.RespondAsync($"このスレッドでは実行されていません。");
            return;
        }
        // コマンドを実行したuserがownerUserか
        if (codeGolf.ownerUserId != userId)
        {
            await command.RespondAsync($"権限がありません。");
            return;
        }

        // 削除
        codeGolfs.Remove(threadId);
        await command.RespondAsync($"CodeGolf ID: {threadId} を終了しました。");
    }

    private async Task OnShowAllCodeGolfCommand(SocketSlashCommand command)
    {
        EmbedBuilder builder = new EmbedBuilder();

        foreach (var codeGolf in codeGolfs.Values)
        {
            builder.AddField(codeGolf.name, $"owner : {client.GetUser(codeGolf.ownerUserId).Mention}", false);
        }

        if (codeGolfs.Count == 0)
        {
            await command.RespondAsync("実行中のCodeGolfは見つかりませんでした。");
            return;
        }

        await command.RespondAsync("CodeGolfの一覧", embed: builder.Build());
    }

    private async Task OnAddPlayerCommand(SocketSlashCommand command)
    {
        SocketGuildUser user = (SocketGuildUser)command.Data.Options.ToArray()[0].Value;
        ulong userId = user.Id;
        CodeGolfTeam team = (CodeGolfTeam)((long)command.Data.Options.ToArray()[1].Value);

        // そのCodeGolfが存在するか調べる
        if (!codeGolfs.TryGetValue(command.Channel.Id, out var codeGolf))
        {
            await command.RespondAsync("そのスレッドではCodeGolfを実行していません。");
            return;
        }
        // 権限があるか調べる
        if (codeGolf.ownerUserId != command.User.Id)
        {
            await command.RespondAsync("権限がありません。");
            return;
        }
        // すでに存在していないか調べる
        if (codeGolf.players.ContainsKey(userId))
        {
            await command.RespondAsync($"{user.Mention}はすでに参加しています。チームを変更する場合は、ChangeTeamコマンドを実行してください。");
            return;
        }

        await command.RespondAsync($"{user.Mention}を{team.ToString()}に追加しました。");
        codeGolf.AddPlayer(new CodeGolfPlayer(userId, team));
    }

    private async Task OnChangePlayerTeamCommand(SocketSlashCommand command)
    {
        ulong userId = ((SocketGuildUser)command.Data.Options.ToArray()[0].Value).Id;
        CodeGolfTeam team = (CodeGolfTeam)((long)command.Data.Options.ToArray()[1].Value);

        // そのCodeGolfが存在するか調べる
        if (!codeGolfs.TryGetValue(command.Channel.Id, out var codeGolf))
        {
            await command.RespondAsync("そのスレッドではCodeGolfを実行していません。");
            return;
        }
        // 権限があるか調べる
        if (codeGolf.ownerUserId != command.User.Id)
        {
            await command.RespondAsync("権限がありません。");
            return;
        }
        // すでに参加しているか調べる
        if (!codeGolf.players.ContainsKey(userId))
        {
            await command.RespondAsync($"{command.User.Mention}はこのCodeGolfに参加していません。参加させる場合は、AddPlayerコマンドを使用してください。");
            return;
        }

        // チームを変更
        await command.RespondAsync($"{client.GetUser(userId).Mention}のTeamを{team.ToString()}に変更しました。");
        codeGolf.players[userId].ChangeTeam(team);
    }

    private async Task OnShowAllPlayerCommand(SocketSlashCommand command)
    {
        // そのCodeGolfが存在するか調べる
        if (!codeGolfs.TryGetValue(command.Channel.Id, out var codeGolf))
        {
            await command.RespondAsync("このスレッドではCodeGolfを実行していません。");
            return;
        }

        // プレイヤーが0の場合すぐ返す
        if (codeGolf.players.Count == 0)
        {
            await command.RespondAsync("参加中のプレイヤーはいません。");
            return;
        }

        EmbedBuilder builder = new EmbedBuilder();

        builder.AddField("Red", codeGolf.players.Values.Count(p => p.team == CodeGolfTeam.Red));
        builder.AddField("Blue", codeGolf.players.Values.Count(p => p.team == CodeGolfTeam.Blue));

        await command.RespondAsync("すべてのプレイヤー", embed: builder.Build());

        foreach (var player in codeGolf.players.Values)
        {
            await command.Channel.SendMessageAsync(embed: new EmbedBuilder()
                .WithColor(player.team == CodeGolfTeam.Red ? Color.Red : Color.Blue)
                .WithAuthor(client.GetUser(player.userId))
                .Build());
        }
    }

    private async Task OnRunProgramCommand(SocketSlashCommand command)
    {
        string language = CodeGolfLanguage.Languages[(int)(long)command.Data.Options.ToArray()[0].Value];
        string code = ((string)command.Data.Options.ToArray()[1]);
        string input = "";
        if (command.Data.Options.Count >= 3)
        {
            input = ((string)command.Data.Options.ToArray()[2]);
        }

        Result result = new Result();

        try
        {
            result = PaizaIO.Run(code, language, input);
        }
        catch (System.Exception)
        {
            await command.RespondAsync("実行できませんでした。");
            return;
        }

        await command.RespondAsync($"Language:```{language}```\nCode:```{language}\n{code}\n```\nResult:``` {result.stdOut}```\nError:``` {result.stdError}```\nBuildError:``` {result.buildStdError}```");
    }
    #endregion

    private Task OnMessage(SocketMessage message)
    {
        Console.WriteLine("{0} {1}:{2}", message.Channel.Name, message.Author.Username, message);

        // botの場合すぐ返す
        if (message.Author.IsBot) return Task.CompletedTask;

        return Task.CompletedTask;
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log);
        return Task.CompletedTask;
    }

    private Task<IThreadChannel> CreateThread(string name, ITextChannel? channel)
    {
        return channel?.CreateThreadAsync(
            name: name,
            autoArchiveDuration: ThreadArchiveDuration.OneDay,
            invitable: false,
            type: ThreadType.PublicThread
        );
    }

    private async Task OnSlashCommand(SocketSlashCommand command)
    {
        if (commands.TryGetValue(command.CommandId, out var commandFunc))
        {
            await commandFunc(command);
        }
        else
        {
            await command.RespondAsync("Error");
        }
    }

    public static void Main(string[] args) => new Program().MainAsync().Wait();
}

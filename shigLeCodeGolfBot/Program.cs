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
        }
        catch (System.Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

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
        // すでに存在していないか調べる
        if (codeGolf.players.ContainsKey(userId))
        {
            await command.RespondAsync($"{command.User.Mention}はすでに参加しています。チームを変更する場合は、ChangeTeamコマンドを実行してください。");
            return;
        }

        await command.RespondAsync($"{command.User.Mention}を{team.ToString()}に追加しました。");
        codeGolf.AddPlayer(new CodeGolfPlayer(userId, team));
    }

    public static void Main(string[] args) => new Program().MainAsync().Wait();
}

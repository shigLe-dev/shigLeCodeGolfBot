using paizaIOSharp;
using System.Configuration;
using Discord;
using Discord.WebSocket;

namespace shigLeCodeGolfBot;

class Program
{
    DiscordSocketClient client;
    public Dictionary<ulong, Func<SocketSlashCommand, Task>> commands = new Dictionary<ulong, Func<SocketSlashCommand, Task>>();
    public Dictionary<ulong, CodeGolf> codeGolfs = new Dictionary<ulong, CodeGolf>();

    public async Task MainAsync()
    {
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
        string name = "";
        string settingsUrl = "";
        try
        {
            threadChannel = await CreateThread("CodeGolf", command.Channel as ITextChannel);
            await threadChannel.SendMessageAsync("CodeGolfの始まりです。");
            name = (string)command.Data.Options.ToArray()[0];
            settingsUrl = (string)command.Data.Options.ToArray()[1];

        }
        catch (System.Exception)
        {
            await command.RespondAsync($"生成に失敗しました。");
            return;
        }
        await command.RespondAsync($"{command.User.Mention}さんがCodeGolfを開始しました。");
        codeGolfs[threadChannel.Id] = new CodeGolf(name,threadChannel.Id, command.User.Id, settingsUrl);
        Console.WriteLine(codeGolfs[threadChannel.Id].name);
    }

    private async Task OnRemoveCodeGolfCommand(SocketSlashCommand command)
    {
        ulong threadId = command.ChannelId ?? 0;
        ulong userId = command.User.Id;

        // CodeGolfを実行しているスレッドか
        if (!codeGolfs.TryGetValue(threadId, out var codeGolf))
        {
            await command.RespondAsync($"ERROR: このスレッドでは実行されていません。");
            return;
        }
        // コマンドを実行したuserがownerUserか
        if (codeGolf.ownerUserId != userId)
        {
            await command.RespondAsync($"ERROR: 権限がありません。");
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

    public static void Main(string[] args) => new Program().MainAsync().Wait();
}

using paizaIOSharp;
using System.Configuration;
using Discord;
using Discord.WebSocket;

namespace shigLeCodeGolfBot;

class Program
{
    DiscordSocketClient client;
    public Dictionary<ulong, Func<SocketSlashCommand,Task>> commands = new Dictionary<ulong, Func<SocketSlashCommand,Task>>();

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

        commands[await SetCommand(1031157559404548107)] = OnCodeGolfCommand;
        commands[await SetCommand(811964339375308890)] = OnCodeGolfCommand;
    }

    private async Task<ulong> SetCommand(ulong guildId)
    {
        var guild = client.GetGuild(guildId);

        var guildCommand = new SlashCommandBuilder();
        guildCommand.WithName("codegolf");
        guildCommand.WithDescription("このコマンドを使用することで、新たにCodeGolfを始めることが出来ます。");

        try
        {
            return (await guild.CreateApplicationCommandAsync(guildCommand.Build())).Id;
        }
        catch (System.Exception e)
        {
            Console.WriteLine(e.Message);
        }

        return 0;
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

    private async Task OnCodeGolfCommand(SocketSlashCommand command)
    {
        await command.RespondAsync($"{command.User.Mention}さんがCodeGolfを開始しました。");
        await CreateThread("CodeGolf", command.Channel as ITextChannel).Result.SendMessageAsync("CodeGolfの始まりです。");
    }

    public static void Main(string[] args) => new Program().MainAsync().Wait();
}

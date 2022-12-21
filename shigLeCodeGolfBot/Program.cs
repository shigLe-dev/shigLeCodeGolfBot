using paizaIOSharp;
using System.Configuration;
using Discord;
using Discord.WebSocket;

namespace shigLeCodeGolfBot;

class Program
{
    DiscordSocketClient client;

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

        await SetCommand();
    }

    private async Task SetCommand(){
        var guild = client.GetGuild(1031157559404548107);

        var guildCommand = new SlashCommandBuilder();
        guildCommand.WithName("codegolf");
        guildCommand.WithDescription("このコマンドを使用することで、新たにCodeGolfを始めることが出来ます。");

        try
        {
            await guild.CreateApplicationCommandAsync(guildCommand.Build());
        }
        catch (System.Exception e)
        {
            Console.WriteLine(e.Message);
        }
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

    private Task CreateThread(string name, ITextChannel? channel)
    {
        channel?.CreateThreadAsync(
            name: name,
            autoArchiveDuration: ThreadArchiveDuration.OneDay,
            invitable: false,
            type: ThreadType.PublicThread
        ).Wait();

        return Task.CompletedTask;
    }

    private async Task OnSlashCommand(SocketSlashCommand command){
        await CreateThread("test", command.Channel as ITextChannel);
        await command.RespondAsync("スレッド作った");
    }

    public static void Main(string[] args) => new Program().MainAsync().Wait();
}

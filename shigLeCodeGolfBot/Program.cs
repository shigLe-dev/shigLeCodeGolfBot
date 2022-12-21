using paizaIOSharp;
using System.Configuration;
using Discord;
using Discord.WebSocket;

namespace shigLeCodeGolfBot;

class Program
{
    public async Task MainAsync()
    {
        var client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.All
        });
        client.Log += LogAsync;
        client.Ready += OnReady;
        client.MessageReceived += OnMessage;
        await client.LoginAsync(TokenType.Bot, ConfigurationManager.AppSettings["token"]);
        await client.StartAsync();
        await Task.Delay(Timeout.Infinite);
    }

    private Task OnReady()
    {
        Console.WriteLine("OnReady");
        return Task.CompletedTask;
    }

    private Task OnMessage(SocketMessage message)
    {
        Console.WriteLine("{0} {1}:{2}", message.Channel.Name, message.Author.Username, message);

        // botの場合すぐ返す
        if (message.Author.IsBot) return Task.CompletedTask;

        // テストで返す
        message.Channel.SendMessageAsync("てぇぇぇすとぉぉぉでぇすねぇぇぇぇええええ！！！！");
        CreateThread("testThread", message.Channel as ITextChannel);

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
            autoArchiveDuration: ThreadArchiveDuration.OneWeek,
            invitable: false,
            type: ThreadType.PublicThread
        ).Wait();

        return Task.CompletedTask;
    }

    public static void Main(string[] args) => new Program().MainAsync().Wait();
}

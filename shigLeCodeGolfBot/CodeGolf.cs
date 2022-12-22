using Discord;

namespace shigLeCodeGolfBot;

public class CodeGolf
{
    public readonly string name;
    public readonly ulong id;
    public readonly ulong ownerUserId;
    public readonly string settingsUrl;
    public readonly CodeGolfSettings settings;
    private readonly IThreadChannel thread;
    private readonly List<CodeGolfPlayer> players;

    public CodeGolf(string name, IThreadChannel thread, ulong ownerUserId, string settingsUrl, CodeGolfSettings settings)
    {
        this.name = name;
        this.id = thread.Id;
        this.ownerUserId = ownerUserId;
        this.settingsUrl = settingsUrl;
        this.settings = settings;
        this.thread = thread;
        this.players = new List<CodeGolfPlayer>();
    }

    public void AddPlayer(ulong userId)
    {
        players.Add(new CodeGolfPlayer(userId, CodeGolfTeam.Red));
    }
}

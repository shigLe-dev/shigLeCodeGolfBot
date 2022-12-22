using Discord;
using System.Collections.ObjectModel;

namespace shigLeCodeGolfBot;

public class CodeGolf
{
    public readonly string name;
    public readonly ulong id;
    public readonly ulong ownerUserId;
    public readonly string settingsUrl;
    public readonly CodeGolfSettings settings;
    public ReadOnlyDictionary<ulong, CodeGolfPlayer> players => _players.ToDictionary(p => p.userId).AsReadOnly();
    private readonly IThreadChannel thread;
    private readonly List<CodeGolfPlayer> _players;

    public CodeGolf(string name, IThreadChannel thread, ulong ownerUserId, string settingsUrl, CodeGolfSettings settings)
    {
        this.name = name;
        this.id = thread.Id;
        this.ownerUserId = ownerUserId;
        this.settingsUrl = settingsUrl;
        this.settings = settings;
        this.thread = thread;
        this._players = new List<CodeGolfPlayer>();
    }

    public void AddPlayer(CodeGolfPlayer player)
    {
        _players.Add(player);
    }
}

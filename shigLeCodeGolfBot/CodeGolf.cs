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

    public CodeGolf(string name, IThreadChannel thread, ulong ownerUserId, string settingsUrl, CodeGolfSettings settings)
    {
        this.name = name;
        this.id = thread.Id;
        this.ownerUserId = ownerUserId;
        this.settingsUrl = settingsUrl;
        this.settings = settings;
        this.thread = thread;
    }
}

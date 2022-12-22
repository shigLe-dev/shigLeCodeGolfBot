namespace shigLeCodeGolfBot;

public class CodeGolf
{
    public readonly string name;
    public readonly ulong threadId;
    public readonly ulong ownerUserId;
    public readonly string settingsUrl;

    public CodeGolf(string name, ulong threadId, ulong ownerUserId, string settingsUrl)
    {
        this.name = name;
        this.threadId = threadId;
        this.ownerUserId = ownerUserId;
        this.settingsUrl = settingsUrl;
    }
}
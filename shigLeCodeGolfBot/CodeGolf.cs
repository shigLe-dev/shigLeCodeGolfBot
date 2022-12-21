namespace shigLeCodeGolfBot;

public class CodeGolf
{
    public readonly ulong threadId;
    public readonly ulong ownerUserId;

    public CodeGolf(ulong threadId, ulong ownerUserId)
    {
        this.threadId = threadId;
        this.ownerUserId = ownerUserId;
    }
}
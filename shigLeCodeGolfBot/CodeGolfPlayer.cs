namespace shigLeCodeGolfBot;

public class CodeGolfPlayer
{
    public readonly ulong userId;
    public CodeGolfTeam team;

    public CodeGolfPlayer(ulong userId, CodeGolfTeam team)
    {
        this.userId = userId;
        this.team = team;
    }

    public void ChangeTeam(CodeGolfTeam team)
    {
        this.team = team;
    }
}

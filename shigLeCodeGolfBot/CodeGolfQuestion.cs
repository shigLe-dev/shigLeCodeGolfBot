using System.Collections.ObjectModel;

namespace shigLeCodeGolfBot;

public class CodeGolfQuestion
{
    public readonly string sentence;
    public readonly string language;
    public ReadOnlyCollection<CodeGolfCase> cases => _cases.AsReadOnly();
    private List<CodeGolfCase> _cases;

    public CodeGolfQuestion(string sentence, string language, CodeGolfCase[] cases)
    {
        _cases = new List<CodeGolfCase>(cases);
        this.sentence = sentence;
        this.language = language;
    }
}

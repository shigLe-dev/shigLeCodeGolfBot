using System.Collections.ObjectModel;
using System.Text.Json.Nodes;

namespace shigLeCodeGolfBot;

public class CodeGolfQuestion
{
    public readonly string sentence;
    public readonly string language;
    public ReadOnlyCollection<CodeGolfCase> cases;

    private CodeGolfQuestion(string sentence, string language, CodeGolfCase[] cases)
    {
        this.cases = cases.AsReadOnly();
        this.sentence = sentence;
        this.language = language;
    }

    public static CodeGolfQuestion Parse(JsonNode node)
    {
        string sentence = ((string?)node["sentence"]?.AsValue()) ?? "";
        string language = ((string?)node["language"]?.AsValue()) ?? "";
        CodeGolfCase[] cases = node["cases"].AsArray().Select(n => CodeGolfCase.Parse(n)).ToArray();

        return new CodeGolfQuestion(sentence, language, cases);
    }
}

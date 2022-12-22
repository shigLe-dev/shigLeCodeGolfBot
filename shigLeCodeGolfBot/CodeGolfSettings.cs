using System.Text.Json.Nodes;
using System.Collections.ObjectModel;

namespace shigLeCodeGolfBot;

public class CodeGolfSettings
{
    public readonly string name;
    public readonly int width;
    public readonly int height;
    public ReadOnlyCollection<CodeGolfQuestion> questions;

    private CodeGolfSettings(string name, int width, int height, CodeGolfQuestion[] questions)
    {
        this.name = name;
        this.width = width;
        this.height = height;
        this.questions = questions.AsReadOnly();
    }

    public static CodeGolfSettings Parse(JsonNode rootNode)
    {
        string name = ((string?)rootNode["name"].AsValue()) ?? "";
        int width = ((int)rootNode["width"].AsValue());
        int height = ((int)rootNode["height"].AsValue());
        CodeGolfQuestion[] questions = rootNode["questions"].AsArray().Select(n => CodeGolfQuestion.Parse(n)).ToArray();

        return new CodeGolfSettings(name, width, height, questions);
    }
}

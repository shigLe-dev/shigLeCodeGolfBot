using System.Text.Json.Nodes;

namespace shigLeCodeGolfBot;

public class CodeGolfCase{
    public readonly bool isExample;
    public readonly string input;
    public readonly string output;

    private CodeGolfCase(bool isExample, string input, string output){
        this.isExample = isExample;
        this.input = input;
        this.output = output;
    }

    public static CodeGolfCase Parse(JsonNode node)
    {
        bool isExample = (bool)node["isExample"]?.AsValue();
        string input = ((string?)node["input"]?.AsValue());
        string output = ((string?)node["output"]?.AsValue());

        return new CodeGolfCase(isExample, input, output);
    }
}

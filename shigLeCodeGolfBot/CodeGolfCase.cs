namespace shigLeCodeGolfBot;

public class CodeGolfCase{
    public readonly bool isExample;
    public readonly string input;
    public readonly string output;

    public CodeGolfCase(bool isExample, string input, string output){
        this.isExample = isExample;
        this.input = input;
        this.output = output;
    }
}

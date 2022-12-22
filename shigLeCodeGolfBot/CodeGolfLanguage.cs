using System.Collections.ObjectModel;

namespace shigLeCodeGolfBot;

public static class CodeGolfLanguage
{
    public static readonly ReadOnlyCollection<string> Languages;

    static CodeGolfLanguage()
    {
        Languages = new List<string>(){
            "c",
            "kotlin",
            "csharp",
            "go",
            "haskell",
            "erlang",
            "perl",
            "python",
            "python3",
            "ruby",
            "php",
            "bash",
            "javascript",
            "cobol",
            "fsharp",
            "d",
            "clojure",
            "elixir",
            "rust",
            "commonlisp",
            "brainfuck",
        }.AsReadOnly();
    }
}
// See https://aka.ms/new-console-template for more information

Console.WriteLine("Hello, World!");

var grammar = new StringGrammar(new Dictionary<char, string>() { { 'A', "AB" }, { 'B', "A" } });
var turtle_grammar = new StringGrammar(new Dictionary<char, string>() { { 'F', "F+F-F-F+F" } });
var tree_grammar = new StringGrammar(new Dictionary<char, string>() { { 'F', "F[-F]F+[+F][F]" } });

var expand_by_times = grammar.ExpandByTimes("A", 7);
Console.WriteLine(expand_by_times);
Console.WriteLine("ABAABABAABAABABAABABAABAABABAABAAB");
var times = turtle_grammar.ExpandByTimes("F", 3);
Console.WriteLine(times);

public record StringGrammar(Dictionary<char, string> Rules)
{
    public string Expand(string input)
        => string.Concat(input.Select(ch => Rules.TryGetValue(ch, out var result) ? result : ch.ToString()));

    public string ExpandByTimes(string input, int times)
    {
        var cur = input;
        for (int i = 0; i < times; i++)
        {
            cur = Expand(cur);
        }

        return cur;
    }
}
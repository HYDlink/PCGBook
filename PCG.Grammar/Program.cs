// See https://aka.ms/new-console-template for more information

using PCG.Grammar;
using QuikGraph;
using QuikGraph.Graphviz;
using QuikGraph.Graphviz.Dot;

void TestStringGrammar()
{
    var grammar = new StringGrammar(new Dictionary<char, string>() { { 'A', "AB" }, { 'B', "A" } });
    var turtle_grammar = new StringGrammar(new Dictionary<char, string>() { { 'F', "F+F-F-F+F" } });
    var tree_grammar = new StringGrammar(new Dictionary<char, string>() { { 'F', "F[-F]F+[+F][F]" } });

    var expand_by_times = grammar.ExpandByTimes("A", 7);
    Console.WriteLine(expand_by_times);
    Console.WriteLine("ABAABABAABAABABAABABAABAABABAABAAB");
    var times = turtle_grammar.ExpandByTimes("F", 3);
    Console.WriteLine(times);
}

void TestGraphGrammar()
{
    var graph = new List<TaggedEdge<Vertex, GraphvizColor>>
    {
        new(new Vertex(0, "a"), new Vertex(2, "B"), GraphvizColor.Black),
        new(new Vertex(1, "A"), new Vertex(2, "B"), GraphvizColor.Black),
        new(new Vertex(1, "A"), new Vertex(0, "b"), GraphvizColor.Black),
    }.ConstructGraph();
    // graph.MyToGraphviz().ExportDotToSvg("Test", "svg");

    var new_edges = new List<TaggedEdge<Vertex, GraphvizColor>>
    {
        new(new(2, "A"), new(4, "B"), GraphvizColor.Black),
        new(new(4, "B"), new(3, "b"), GraphvizColor.Black),
        new(new(3, "b"), new(1, "a"), GraphvizColor.Black),
    };
    var old_edges = new List<TaggedEdge<Vertex, GraphvizColor>>
    {
        new(new(1, "A"), new(2, "B"), GraphvizColor.Black)
    };
    var graph_grammar = new GraphGrammar(
        old_edges,
        new_edges,
        new()
        {
            { new(1, "A"), new(1, "a") },
            { new(2, "B"), new(2, "A") },
        }
    );
    // new_edges.ConstructGraph().MyToGraphviz().ExportDotToSvg("NewEdges", "svg");
    graph_grammar.ReplaceGraph(graph).MyToGraphviz().ExportDotToSvg("Replaced", "svg");
}

// TestStringGrammar();
TestGraphGrammar();

public record StringGrammar(Dictionary<char, string> Rules)
{
    public string Expand(string input)
        => string.Concat(input.Select(ch => Rules.TryGetValue(ch, out var result) ? result : ch.ToString()));

    public string ExpandByTimes(string input, int times)
    {
        var cur = input;
        for (int i = 0; i < times; i++)
            cur = Expand(cur);

        return cur;
    }
}
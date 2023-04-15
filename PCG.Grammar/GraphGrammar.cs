using System.Diagnostics;
using QuikGraph;
using QuikGraph.Graphviz;
using QuikGraph.Graphviz.Dot;

namespace PCG.Grammar;

// 使用 TaggedEdge 来方便 Debug
using Edge = TaggedEdge<Vertex, GraphvizColor>;
using Graph = UndirectedGraph<Vertex, TaggedEdge<Vertex, GraphvizColor>>;

public record struct Vertex(int Index, string Type)
{
    public override string ToString()
        => (Index == 0 ? "" : $"{Index}:") + Type;
}

public record GraphGrammar(List<Edge> Formal, List<Edge> New, Dictionary<Vertex, Vertex> ReservedVertex)
{
    public Graph ReplaceGraph(Graph input)
    {
        var replaced_vertices = new HashSet<Vertex>();
        var new_graph = input.Clone();

        bool IsNewEdge(Edge edge) =>
            replaced_vertices.Contains(edge.Source) || replaced_vertices.Contains(edge.Target);

        bool IsOldEdge(Edge edge) => !IsNewEdge(edge);

        bool IsCorrespondVertex(Vertex left, Vertex right) => left.Type == right.Type;

        bool IsCorrespondEdge(Edge left, Edge right)
            => IsCorrespondVertex(left.Source, right.Source)
               && IsCorrespondVertex(left.Target, right.Target);

        do
        {
            var edges = new_graph.Edges.Where(IsOldEdge).ToList();
            if (!edges.Any())
                break;

            // 找到所有和 Formal 匹配的 Edges，并且不能重复，如果不存在全部的匹配，那么退出
            var correspond_edges = new List<Edge>();
            foreach (var edge in Formal)
            {
                var cor_edge = edges.FirstOrDefault(e => IsCorrespondEdge(e, edge));
                if (cor_edge is null)
                    break;
                correspond_edges.Add(cor_edge);
            }

            var has_all_correspond_edges = correspond_edges.Count == Formal.Count;
            if (!has_all_correspond_edges) break;

            // 替换
            // 记录可以保存的顶点，记录图中连接到顶点的边，删掉图中所有的边，然后创建新的节点
            var reserved_to_source_edges = correspond_edges
                .Where(e => ReservedVertex.ContainsKey(e.Source))
                .SelectMany(e => new_graph.AdjacentEdges(e.Source))
                .Where(e => !correspond_edges.Contains(e))
                .Distinct().ToList();
            var new_source_edges = reserved_to_source_edges
                .Select(e => new Edge(ReservedVertex[e.Source], e.Target, GraphvizColor.Chocolate));

            // 记录可以保存的顶点，记录图中连接到顶点的边，删掉图中所有的边，然后创建新的节点
            var reserved_to_target_edges = correspond_edges
                .Where(e => ReservedVertex.ContainsKey(e.Target))
                .SelectMany(e => new_graph.AdjacentEdges(e.Target))
                .Where(e => !correspond_edges.Contains(e))
                .Distinct().ToList();
            var new_target_edges = reserved_to_target_edges
                .Select(e => new Edge(e.Source, ReservedVertex[e.Target], GraphvizColor.Chocolate));

            // Debug
            // correspond_edges.ForEach(e => e.Tag = GraphvizColor.Aquamarine);
            // reserved_to_source_edges.ForEach(e => e.Tag = GraphvizColor.Brown);
            // reserved_to_target_edges.ForEach(e => e.Tag = GraphvizColor.Gold);
            // return new_graph;
            
            foreach (var edge in correspond_edges.Concat(reserved_to_source_edges).Concat(reserved_to_target_edges))
            {
                new_graph.RemoveEdge(edge);
            }
            
            foreach (var edge in correspond_edges.Concat(reserved_to_source_edges).Concat(reserved_to_target_edges))
            {
                new_graph.RemoveEdge(edge);
            }
            foreach (var vertex in correspond_edges.GetAllVertices())
            {
                new_graph.RemoveVertex(vertex);
            }
            
            foreach (var vertex in New.GetAllVertices())
            {
                replaced_vertices.Add(vertex);
                new_graph.AddVertex(vertex);
            }
            
            foreach (var edge in New.Concat(new_source_edges).Concat(new_target_edges))
            {
                new_graph.AddEdge(edge);
            }
        } while (true);

        return new_graph;
    }
}

public static class Utilities
{
    public static Graph ConstructGraph(this IEnumerable<Edge> edges)
    {
        var graph = new Graph();
        foreach (var edge in edges) graph.AddVerticesAndEdge(edge);

        return graph;
    }

    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        foreach (var element in enumerable)
        {
            action(element);
        }
    }

    public static IEnumerable<Vertex> GetAllVertices(this IEnumerable<Edge> edges)
        => edges.Select(e => e.Source)
            .Concat(edges.Select(e => e.Target))
            .Distinct();

    public static string MyToGraphviz(this Graph graph)
        => graph.ToGraphviz(algorithm =>
        {
            algorithm.FormatVertex += (sender, args)
                =>
            {
                args.VertexFormat.Label = args.Vertex.ToString();
                args.VertexFormat.Shape = GraphvizVertexShape.Box;
            };
            algorithm.FormatEdge += (sender, args)
                => args.EdgeFormat.StrokeColor = args.Edge.Tag;
        });

    public static void ExportDotToSvg(this string dot, string name, string format)
    {
        var dot_file = $"{name}.dot";
        var bat_file = $"{name}.bat";
        var output_file = $"{name}.{format}";
        File.WriteAllText(dot_file, dot);


        var bat =
            $"dot -T{format} {dot_file} -o {output_file}";


        File.WriteAllText(bat_file, bat);
        var result = Process.Start(bat_file);
        result.WaitForExit();


        // File.Delete(dot_file);
        File.Delete(bat_file);
        OpenFile(output_file);
    }

    public static void OpenFile(string filename)
    {
        var start_info = new ProcessStartInfo(filename)
            { UseShellExecute = true };
        Process.Start(start_info);
    }
}
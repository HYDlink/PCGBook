namespace PCG.Maze;

public partial class MazeGenerator
{
    public class DisjointUnion<TValue>
    {
        private Dictionary<TValue, TValue> Parent = new();

        public TValue Find(TValue value)
        {
            var cur = value;
            while (Parent.TryGetValue(cur, out var cur_parent))
                cur = cur_parent;

            return cur;
        }

        public bool IsSameSet(TValue left, TValue right) => Equals(Find(left), Find(right));

        public void Union(TValue lhs, TValue rhs)
        {
            var l_root = Find(lhs);
            var r_root = Find(rhs);
            if (Equals(l_root, r_root)) return;
            Parent[l_root] = r_root;
        }
    }

    /// <summary>
    /// 无关边缘顶点的保存顺序
    /// 保证即便另一个 AdjacentEdge 是以相反的 Right, Left 顺序存储的话，依然能够 Equals，并且有同样的 HashCode
    /// </summary>
    /// <param name="Left"></param>
    /// <param name="Right"></param>
    /// <typeparam name="TValue"></typeparam>
    public record AdjacentEdge<TValue>(TValue Left, TValue Right)
    {
        public virtual bool Equals(AdjacentEdge<TValue>? other)
        {
            if (other is null) return false;
            return Equals(Left, other.Left) && Equals(Right, other.Right)
                   || Equals(Left, other.Right) && Equals(Right, other.Left);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Left, Right) + HashCode.Combine(Right, Left);
        }

        public override string ToString()
        {
            return $"{Left} - {Right}";
        }
    }
}
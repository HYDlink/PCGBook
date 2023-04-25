namespace PCG.CellularAutomata;

public enum NeighborType
{
    Square,
    Cross,
}

public abstract record CARule
{
    public abstract bool GetNewValue(CaveCA caveCa, int x, int y, out int result);
}

public record CellCountRule(int CurCell, int NewCell,
    int AroundCell, NeighborType NeighborType, int Count) : CARule
{
    public override bool GetNewValue(CaveCA caveCa, int x, int y, out int result)
    {
        var cell = caveCa.GetCell(x, y);
        result = cell;
        if (CurCell != SpecialCell.Any && cell != CurCell) return false;
        var count = caveCa.GetAllNeighbors(NeighborType, x, y).Count(c => c == AroundCell);
        
        var success = count >= Count;
        result = success ? NewCell : cell;
        return success;
    }
}

public record SpecificSquareNeighborRule(int[,] From, int To) : CARule //, int[,] To)
{
    public override bool GetNewValue(CaveCA caveCa, int x, int y, out int result)
    {
        var validX = x > 0 && x < caveCa.Width - 1;
        var validY = y > 0 && y < caveCa.Height - 1;
        if (validX && validY)
        {
            var curNeighbors = new int[,]
            {
                { caveCa.GetCell(x - 1, y + 1), caveCa.GetCell(x, y + 1), caveCa.GetCell(x + 1, y + 1) },
                { caveCa.GetCell(x - 1, y), caveCa.GetCell(x, y), caveCa.GetCell(x + 1, y) },
                { caveCa.GetCell(x - 1, y - 1), caveCa.GetCell(x, y - 1), caveCa.GetCell(x + 1, y - 1) },
            };
            if (curNeighbors.Cast<int>().SequenceEqual(From.Cast<int>()))
            {
                result = To;
                return true;
            }
        }

        result = caveCa.GetCell(x, y);
        return false;
    }
}
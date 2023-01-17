namespace Logic;

public readonly struct Offset
{
    public int RowOffset { get; }
    public int ColumnOffset { get; }

    public bool IsZero => RowOffset == 0 && ColumnOffset == 0;

    internal Offset(int rowOffset, int columnOffset)
    {
        RowOffset = rowOffset;
        ColumnOffset = columnOffset;
    }
}

public readonly struct Move
{
    public Position From { get; }
    public Position To { get; }

    public Move(Position from, Position to)
    {
        From = from;
        To = to;
    }
}

public readonly struct Position
{
    public int Row { get; }
    public int Column { get; }

    public bool IsValid => Row is >= 1 and <= 8 && Column is >= 1 and <= 8;

    public Position(int row, int column)
    {
        Row = row;
        Column = column;
    }

    public static Position operator +(Position pos, Offset offset) => new(pos.Row + offset.RowOffset, pos.Column + offset.ColumnOffset);
    public static Position operator -(Position pos, Offset offset ) => new(pos.Row - offset.RowOffset, pos.Column - offset.ColumnOffset);

    public static bool operator ==(Position pos0, Position pos1) => pos0.Row == pos1.Row && pos0.Column == pos1.Column;
    public static bool operator !=(Position pos0, Position pos1) => !(pos0 == pos1);

    public static implicit operator Position(string str)
    {
        if (str.Length != 2)
            throw new InvalidOperationException();

        var columnChar = str[0];
        var column = char.IsUpper(columnChar) ? columnChar - 'A' + 1 : columnChar - 'a' + 1;
        var row = int.Parse(str[1].ToString());
        return new Position(row, column);
    }

    public override string ToString() => $"{(char) ('a' - 1 + Column)}{Row}";

    public override bool Equals(object obj) => obj is Position other && this == other;
    public override int GetHashCode() => HashCode.Combine(Row, Column);
}
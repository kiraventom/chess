using System.Data;

namespace Logic;

public readonly struct Offset
{
    public int RowOffset { get; }
    public int ColumnOffset { get; }

    public bool IsZero => RowOffset == 0 && ColumnOffset == 0;

    public Offset(int rowOffset, int columnOffset)
    {
        RowOffset = rowOffset;
        ColumnOffset = columnOffset;
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

    public static bool operator ==(Position pos0, Position pos1)
    {
        return pos0.Row == pos1.Row && pos0.Column == pos1.Column;
    }

    public static bool operator !=(Position pos0, Position pos1)
    {
        return !(pos0 == pos1);
    }

    public static implicit operator Position(string str)
    {
        if (str.Length != 2)
            throw new InvalidOperationException();

        var columnChar = str[0];
        var column = columnChar - 'A' + 1;
        var row = int.Parse(str[1].ToString());
        return new Position(row, column);
    }
}
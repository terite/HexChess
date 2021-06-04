using System;

[Serializable]
public struct Index
{
    public int row;
    public int col;
    public Index(int rank, char file)
    {
        if (rank < 1 || rank > 10)
            throw new ArgumentOutOfRangeException(nameof(rank), "Rank must be between 1-10 inclusive");
        if (file < 'A' || file > 'I')
            throw new ArgumentOutOfRangeException(nameof(file), "File must be between A-I inclusive");

        bool tallFile = file == 'B' || file == 'D' || file == 'F' || file == 'H';

        if (rank == 10 && !tallFile)
            throw new ArgumentOutOfRangeException(nameof(file), "Only valid rank 10 files are B, D, F, H");

        this.col = file switch {
            'A' => 0,
            'B' => 0,
            'C' => 1,
            'D' => 1,
            'E' => 2,
            'F' => 2,
            'G' => 3,
            'H' => 3,
            'I' => 4,
            _ => '?'
        };

        var startingRow = tallFile ? 0 : 1;
        this.row = startingRow + ((rank - 1) * 2);

    }
    public Index(int row, int col)
    {
        this.row = row;
        this.col = col;
    }

    public string GetKey() => $"{GetLetter()}{GetNumber()}";

    public int GetNumber() => (row / 2) + 1;

    public string GetLetter()
    {
        bool isEven = row % 2 == 0;

        return col switch {
            0 when !isEven => "A", 0 when isEven => "B",
            1 when !isEven => "C", 1 when isEven => "D",
            2 when !isEven => "E", 2 when isEven => "F",
            3 when !isEven => "G", 3 when isEven => "H",
            4 => "I", _ => ""
        };
    }

    public override string ToString() => $"{row}, {col} ({GetKey()})";

    public override bool Equals(object obj) => 
        obj is Index index &&
        row == index.row &&
        col == index.col;

    public override int GetHashCode()
    {
        int hashCode = -1720622044;
        hashCode = hashCode * -1521134295 + row.GetHashCode();
        hashCode = hashCode * -1521134295 + col.GetHashCode();
        return hashCode;
    }

    public static bool operator ==(Index a, Index b) => a.row == b.row && a.col == b.col;
    public static bool operator !=(Index a, Index b) => !(a==b);

}
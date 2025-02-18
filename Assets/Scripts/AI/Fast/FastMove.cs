using Extensions;

public readonly struct FastMove
{
    public static readonly FastMove Invalid = new FastMove(FastIndex.Invalid, FastIndex.Invalid, MoveType.None);

    public readonly FastIndex start;
    public readonly FastIndex target;
    public readonly MoveType moveType;
    public readonly FastPiece promoteTo;

    public FastMove(HexAIMove move)
    {
        this.start = FastIndex.FromByte(move.start.ToByte());
        this.target = FastIndex.FromByte(move.target.ToByte());
        this.moveType = move.moveType;
        this.promoteTo = move.promoteTo.ToFastPiece();
    }
    public FastMove(Index start, Index target, MoveType moveType)
    {
        this.start = FastIndex.FromByte(start.ToByte());
        this.target = FastIndex.FromByte(target.ToByte());
        this.moveType = moveType;
        this.promoteTo = FastPiece.Pawn;
    }
    public FastMove(Index start, Index target, MoveType moveType, FastPiece promoteTo)
    {
        this.start = FastIndex.FromByte(start.ToByte());
        this.target = FastIndex.FromByte(target.ToByte());
        this.moveType = moveType;
        this.promoteTo = promoteTo;
    }

    public FastMove(FastIndex start, FastIndex target, MoveType moveType)
    {
        this.start = start;
        this.target = target;
        this.moveType = moveType;
        this.promoteTo = FastPiece.Pawn;
    }

    public FastMove(FastIndex start, FastIndex target, MoveType moveType, FastPiece promoteTo)
    {
        this.start = start;
        this.target = target;
        this.moveType = moveType;
        this.promoteTo = promoteTo;
    }

    public HexAIMove ToHexMove()
    {
        return new HexAIMove(
            Index.FromByte(start.HexId),
            Index.FromByte(target.HexId),
            moveType,
            promoteTo.ToPiece()
        );
    }

    public override string ToString()
    {
        string promoteMsg = promoteTo == FastPiece.Pawn ? string.Empty : $"to {promoteTo}";
        return $"{moveType}({start} -> {target}){promoteMsg}";
    }

    public static explicit operator FastMove(HexAIMove move) => new FastMove(move);
    public static explicit operator HexAIMove(FastMove move) => move.ToHexMove();
}

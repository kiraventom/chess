using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Logic;
using Logic.Pieces;

namespace GUI.Utils;

public class PawnPromoter
{
    private readonly ChangeTracker _changeTracker;
    private readonly Board _board;
    private readonly Dictionary<Position, PromotionPiece> _promotionPieces = new();

    private readonly ManualResetEvent _handle = new(false);

    public bool IsPromoting => _promotingPawn is not null;
    private PromotionPiece _promotionPiece;
    private Pawn _promotingPawn;

    public PawnPromoter(ChangeTracker changeTracker, Board board)
    {
        _changeTracker = changeTracker;
        _board = board;
    }

    public void BeginPromoting(Pawn pawn)
    {
        _promotingPawn = pawn;

        var rowOffset = pawn.Color == PieceColor.White ? -1 : 1;

        var promotionPieceRow = pawn.Position.Row;
        foreach (var promotionPiece in Enum.GetValues<PromotionPiece>())
        {
            var pos = new Position(promotionPieceRow, pawn.Position.Column);
            promotionPieceRow += rowOffset;

            _changeTracker.RegisterPromotion(pos, promotionPiece, pawn.Color);
            _promotionPieces.Add(pos, promotionPiece);
        }

        for (var row = 1; row <= 8; row++)
        {
            for (var col = 1; col <= 8; col++)
            {
                var pos = new Position(row, col);
                if (_promotionPieces.ContainsKey(pos))
                    continue;

                _changeTracker.RegisterField(_board[pos], true, false, true);
            }
        }
    }

    public async Task<PromotionPiece> GetPiece()
    {
        await Task.Run(() => _handle.WaitOne());
        return _promotionPiece;
    }

    public void HandleClick(Position position)
    {
        if (_promotionPieces.ContainsKey(position))
        {
            _promotionPiece = _promotionPieces[position];
            _promotingPawn = null;

            _handle.Set();
        }
    }

    public void EndPromoting()
    {
        _promotingPawn = null;
        _promotionPiece = default;
        _handle.Reset();
    }
}
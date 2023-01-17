using System.Text;
using Logic.GameLog;
using Logic.Pieces;

namespace GUI.ViewModels.GameLogReader;

public class GameLogTurnReader : BaseNotifier
{
    private string _asString;
    public GameLogTurn GameLogTurn { get; }

    public string AsString
    {
        get => _asString;
        private set => SetAndRaise(ref _asString, value);
    }

    public GameLogTurnReader(GameLogTurn gameLogTurn)
    {
        GameLogTurn = gameLogTurn;
        AsString = GameLogTurnToString(GameLogTurn);
    }

    public void Update() => AsString = GameLogTurnToString(GameLogTurn);

    private static string GameLogTurnToString(GameLogTurn turn)
    {
        StringBuilder sb = new();
        sb.Append(turn.Number).Append('.').Append(' ');

        var whiteMoveSb = GameLogMoveToString(turn.WhiteMove);
        sb.Append(whiteMoveSb).Append(' ');

        if (turn.BlackMove is not null)
        {
            var blackMoveSb = GameLogMoveToString(turn.BlackMove);
            sb.Append(blackMoveSb).Append(' ');
        }

        return sb.ToString();
    }

    private static StringBuilder GameLogMoveToString(GameLogMove move)
    {
        StringBuilder sb = new();

        if (move.IsCastle)
        {
            sb.Append(move.Move.To.Column == 7 ? "0-0" : "0-0-0");
        }
        else
        {
            var pieceStr = move.Piece switch
            {
                King => "K",
                Queen => "Q",
                Bishop => "B",
                Knight => "N",
                Rook => "R",
                Pawn => string.Empty
            };

            sb.Append(pieceStr);

            if (move.IsTake)
                sb.Append(move.Piece is Pawn ? move.Move.From.ToString()[0] : 'x');

            sb.Append(move.Piece is Pawn && move.IsTake ? move.Move.To.ToString()[0] : move.Move.To.ToString());
        }

        if (move.IsMate)
            sb.Append('#');
        else if (move.IsCheck)
            sb.Append('+');

        return sb;
    }
}
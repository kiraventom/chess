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

    private static StringBuilder GameLogMoveToString(GameLogMove turn)
    {
        StringBuilder sb = new();

        if (turn.IsCastle)
        {
            sb.Append(turn.Move.To.Column == 7 ? "0-0" : "0-0-0");
        }
        else
        {
            var pieceStr = turn.Piece switch
            {
                King => "K",
                Queen => "Q",
                Bishop => "B",
                Knight => "N",
                Rook => "R",
                Pawn => string.Empty
            };

            sb.Append(pieceStr);

            if (turn.IsTake)
                sb.Append(turn.Piece is Pawn ? turn.Move.From.ToString()[0] : 'x');

            sb.Append(turn.Piece is Pawn && turn.IsTake ? turn.Move.To.ToString()[0] : turn.Move.To.ToString());
        }

        if (turn.IsMate)
            sb.Append('#');
        else if (turn.IsCheck)
            sb.Append('+');

        return sb;
    }
}
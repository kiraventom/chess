namespace Logic.GameLog;

public class GameLogTurn
{
    public int Number { get; }
    public GameLogMove WhiteMove { get; internal init;}
    public GameLogMove BlackMove { get; internal set; }

    public GameLogMove LastMove => BlackMove ?? WhiteMove;

    public GameLogTurn(int number)
    {
        Number = number;
    }
}
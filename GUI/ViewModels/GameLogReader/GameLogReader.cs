using System.Collections.ObjectModel;
using System.Linq;
using Logic.GameLog;

namespace GUI.ViewModels.GameLogReader;

public class GameLogReader : BaseNotifier
{
    public ObservableCollection<GameLogTurnReader> Turns { get; } = new();

    public void AddOrUpdateTurn(GameLogTurn turn)
    {
        var lastTurnWrapper = Turns.LastOrDefault();
        if (lastTurnWrapper is null || lastTurnWrapper.GameLogTurn != turn)
            Turns.Add(new GameLogTurnReader(turn));
        else
            lastTurnWrapper.Update();
    }

    public void Clear()
    {
        Turns.Clear();
    }
}
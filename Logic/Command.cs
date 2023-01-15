using System.Windows.Input;

namespace Logic;

public class Command : ICommand
{
    public Command(Action action)
    {
        _action = action;
    }

    public Command(Action action, Func<bool> predicate)
    {
        _action = action;
        _predicate = predicate;
    }

    private readonly Action _action;
    private readonly Func<bool> _predicate;

    public bool CanExecute(object parameter) => _predicate?.Invoke() ?? true;

    public void Execute(object parameter) => _action.Invoke();

    public event EventHandler CanExecuteChanged;
}
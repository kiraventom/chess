using System.Collections.Generic;
using System.Linq;
using Logic;

namespace GUI;

public class ChangeTracker
{
    private readonly HashSet<Position> _changes = new();

    public void Register(Position pos) => _changes.Add(pos);

    public IEnumerable<Position> GetAll()
    {
        var changes = _changes.ToList();
        _changes.Clear();
        return changes;
    }
}
using SkiaSharp;

namespace Common;

public interface IChangeLayer
{
    void Paint(SKCanvas canvas, float mod);
}

public class Change
{
    public IReadOnlyCollection<IChangeLayer> Layers { get; }

    public Change(IReadOnlyCollection<IChangeLayer> layers)
    {
        Layers = layers;
    }
}

public class ChangeTracker
{
    private readonly HashSet<Change> _changes = new();

    public void Register(Change change) => _changes.Add(change);

    public IEnumerable<Change> GetAll()
    {
        var changes = _changes.ToList();
        _changes.Clear();
        return changes;
    }
}
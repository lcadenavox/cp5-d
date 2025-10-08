using cp5_d.Models;

namespace cp5_d.Services;

public class InMemoryRepository<T> : IRepository<T> where T : class, IEntity, new()
{
    private readonly List<T> _items = new();
    private int _nextId = 1;

    public IEnumerable<T> GetAll() => _items.OrderBy(i => i.Id);

    public T? GetById(int id) => _items.FirstOrDefault(i => i.Id == id);

    public T Add(T entity)
    {
        entity.Id = _nextId++;
        _items.Add(entity);
        return entity;
    }

    public void Update(T entity)
    {
        var existing = GetById(entity.Id);
        if (existing is null) return;
        var index = _items.IndexOf(existing);
        _items[index] = entity;
    }

    public void Delete(int id)
    {
        var existing = GetById(id);
        if (existing is null) return;
        _items.Remove(existing);
    }
}

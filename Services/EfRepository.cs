using cp5_d.Data;
using cp5_d.Models;
using Microsoft.EntityFrameworkCore;

namespace cp5_d.Services;

public class EfRepository<T> : IRepository<T> where T : class, IEntity
{
    private readonly ApplicationDbContext _db;
    private readonly DbSet<T> _set;

    public EfRepository(ApplicationDbContext db)
    {
        _db = db;
        _set = _db.Set<T>();
    }

    public IEnumerable<T> GetAll() => _set.AsNoTracking().ToList();

    public T? GetById(int id) => _set.Find(id);

    public T Add(T entity)
    {
        _set.Add(entity);
        _db.SaveChanges();
        return entity;
    }

    public void Update(T entity)
    {
        _set.Update(entity);
        _db.SaveChanges();
    }

    public void Delete(int id)
    {
        var e = GetById(id);
        if (e is null) return;
        _set.Remove(e);
        _db.SaveChanges();
    }
}

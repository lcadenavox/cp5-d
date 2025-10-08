using cp5_d.Models;

namespace cp5_d.Services;

public interface IRepository<T> where T : class, IEntity
{
    IEnumerable<T> GetAll();
    T? GetById(int id);
    T Add(T entity);
    void Update(T entity);
    void Delete(int id);
}

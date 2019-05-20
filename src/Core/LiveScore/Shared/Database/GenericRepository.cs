namespace LiveScore.Shared.Database
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public interface IGenericRepository<T> where T : class
    {
        Task<List<T>> GetAll();

        Task<T> GetById(object id);

        Task<IEnumerable<T>> Find(Func<T, bool> predicate);

        Task Insert(T obj);

        Task InsertRange(List<T> objects);

        void Update(T obj);

        void Delete(object id);

        Task Save();
    }

    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly ScoreAppContext context;
        private readonly DbSet<T> table;

        public GenericRepository(ScoreAppContext context)
        {
            this.context = context;
            table = context.Set<T>();
        }

        public async Task<IEnumerable<T>> Find(Func<T, bool> predicate)
        {
            return await Task.FromResult(table.Where(predicate));
        }

        public Task<List<T>> GetAll()
        {
            return table.ToListAsync();
        }

        public Task<T> GetById(object id)
        {
            return table.FindAsync(id);
        }

        public Task Insert(T obj)
        {
            return table.AddAsync(obj);
        }

        public Task InsertRange(List<T> objects)
        {
            return table.AddRangeAsync(objects);
        }

        public void Update(T obj)
        {
            table.Attach(obj);
            context.Entry(obj).State = EntityState.Modified;
        }

        public void Delete(object id)
        {
            T existing = table.Find(id);
            table.Remove(existing);
        }

        public Task Save()
        {
            return context.SaveChangesAsync();
        }
    }
}

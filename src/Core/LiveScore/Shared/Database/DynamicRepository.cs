namespace LiveScore.Shared.Database
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public interface IDynamicRepository
    {
        Task<List<T>> GetAll<T>() where T : class;

        Task<T> GetById<T>(object id) where T : class;

        Task<IEnumerable<T>> Find<T>(Func<T, bool> predicate) where T : class;

        Task Insert<T>(T obj) where T : class;

        Task InsertRange<T>(List<T> objects) where T : class;

        void Update<T>(T obj) where T : class;

        void Delete<T>(object id) where T : class;

        Task Save();
    }

    public class DynamicRepository : IDynamicRepository
    {
        private readonly ScoreAppContext context;
        //private readonly DbSet<T> table;

        public DynamicRepository(ScoreAppContext context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<T>> Find<T>(Func<T, bool> predicate) where T : class
        {
            var table = context.Set<T>();
            return await Task.FromResult(table.Where(predicate));
        }

        public Task<List<T>> GetAll<T>() where T : class
        {
            var table = context.Set<T>();
            return table.ToListAsync();
        }

        public Task<T> GetById<T>(object id) where T : class
        {
            var table = context.Set<T>();
            return table.FindAsync(id);
        }

        public Task Insert<T>(T obj) where T : class
        {
            var table = context.Set<T>();
            return table.AddAsync(obj);
        }

        public Task InsertRange<T>(List<T> objects) where T : class
        {
            var table = context.Set<T>();
            return table.AddRangeAsync(objects);
        }

        public void Update<T>(T obj) where T : class
        {
            var table = context.Set<T>();
            table.Attach(obj);
            context.Entry(obj).State = EntityState.Modified;
        }

        public void Delete<T>(object id) where T : class
        {
            var table = context.Set<T>();
            T existing = table.Find(id);
            table.Remove(existing);
        }

        public Task Save()
        {
            return context.SaveChangesAsync();
        }
    }
}

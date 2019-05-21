namespace LiveScore.Shared.Database
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;

    public interface IDynamicRepository : IDisposable
    {
        Task<List<T>> GetAll<T>() where T : class;

        Task<T> GetById<T>(object id) where T : class;

        Task<List<T>> Find<T>(Func<T, bool> predicate) where T : class;

        Task<T> First<T>(Expression<Func<T, bool>> predicate) where T : class;

        Task<T> Last<T>(Expression<Func<T, bool>> predicate) where T : class;

        Task Insert<T>(T obj) where T : class;

        Task InsertRange<T>(IEnumerable<T> objects) where T : class;

        void Update<T>(T obj) where T : class;

        void Delete<T>(object id) where T : class;

        Task<int> Save();
    }

    public class DynamicRepository : IDynamicRepository
    {
        private readonly ScoreAppContext context;

        public DynamicRepository(ScoreAppContext context)
        {
            this.context = context;
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public async Task<List<T>> Find<T>(Func<T, bool> predicate) where T : class
        {
            return await Task.FromResult(context.Set<T>().Where(predicate).ToList());
        }

        public Task<T> First<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return context.Set<T>().FirstOrDefaultAsync(predicate);
        }

        public Task<T> Last<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return context.Set<T>().LastOrDefaultAsync(predicate);
        }

        public Task<List<T>> GetAll<T>() where T : class
        {
            return context.Set<T>().ToListAsync();
        }

        public Task<T> GetById<T>(object id) where T : class
        {
            return context.Set<T>().FindAsync(id);
        }

        public Task Insert<T>(T obj) where T : class
        {
            return context.Set<T>().AddAsync(obj);
        }

        public Task InsertRange<T>(IEnumerable<T> objects) where T : class
        {
            return context.Set<T>().AddRangeAsync(objects);
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

        public Task<int> Save()
        {
            return context.SaveChangesAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Cleanup
        }

        ~DynamicRepository()
        {
            Dispose(false);
        }
    }
}

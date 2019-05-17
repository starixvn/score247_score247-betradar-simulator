namespace LiveScore.Entity
{
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public interface GenericRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        Task<T> GetById(object id);
        Task Insert(T obj);
        void Update(T obj);
        void Delete(object id);
        Task Save();
    }

    public class GenericRepositoryImpl<T> : GenericRepository<T> where T : class
    {
        private readonly ScoreAppContext context;
        private readonly DbSet<T> table;
       
        public GenericRepositoryImpl(ScoreAppContext context)
        {
            this.context = context;
            table = context.Set<T>();
        }
        public IEnumerable<T> GetAll()
        {
            return table.ToList();
        }
        public Task<T> GetById(object id)
        {
            return table.FindAsync(id);
        }
        public Task Insert(T obj)
        {
            return table.AddAsync(obj);
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

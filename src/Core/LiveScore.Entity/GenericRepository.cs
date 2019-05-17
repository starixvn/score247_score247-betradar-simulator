namespace LiveScore.Entity
{
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Linq;

    public interface GenericRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        T GetById(object id);
        void Insert(T obj);
        void Update(T obj);
        void Delete(object id);
        void Save();
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
        public T GetById(object id)
        {
            return table.Find(id);
        }
        public void Insert(T obj)
        {
            table.Add(obj);
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
        public void Save()
        {
            context.SaveChanges();
        }
    }
}

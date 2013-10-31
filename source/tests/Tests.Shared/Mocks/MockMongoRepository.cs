using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTimer.Tests.Shared.Mocks
{
    using MongoRepository;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class MockMongoRepositority<T> : IRepository<T>
        where T : IEntity
    {
        private IList<T> _collection { get; set; }
        public MockMongoRepositority()
        {
            _collection = new List<T>();
        }

        public MockMongoRepositority(IList<T> basecollection)
        {
            _collection = basecollection;
        }

        public void Add(IEnumerable<T> entities)
        {
            foreach (var e in entities)
                this.Add(e);
        }

        public T Add(T entity)
        {
            entity.Id = Guid.NewGuid().ToString();
            _collection.Add(entity);
            return entity;
        }

        public MongoDB.Driver.MongoCollection<T> Collection
        {
            get { throw new NotImplementedException(); }
        }

        public long Count()
        {
            return _collection.Count();
        }

        public void Delete(System.Linq.Expressions.Expression<Func<T, bool>> criteria)
        {
            var entitiesToDelete = _collection.AsQueryable().Where(criteria);
            foreach (var entity in entitiesToDelete)
                this.Delete(entity);
        }

        public void Delete(T entity)
        {
            this.Delete(entity.Id);
        }

        public void Delete(string id)
        {
            _collection.Remove(this.GetById(id));
        }

        public void DeleteAll()
        {
            _collection.Clear();
        }

        public bool Exists(System.Linq.Expressions.Expression<Func<T, bool>> criteria)
        {
            return _collection.AsQueryable().Any(criteria);
        }

        public T GetById(string id)
        {
            return _collection.First(x => x.Id == id);
        }

        public T GetSingle(System.Linq.Expressions.Expression<Func<T, bool>> criteria)
        {
            return _collection.AsQueryable().FirstOrDefault(criteria);
        }

        public void RequestDone()
        {
            throw new NotImplementedException();
        }

        public IDisposable RequestStart()
        {
            throw new NotImplementedException();
        }

        public void Update(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                this.Update(entity);
            }
        }

        public T Update(T entity)
        {
            this.Delete(entity.Id);
            _collection.Add(entity);
            return entity;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        public Type ElementType
        {
            get { return typeof(T); }
        }

        public System.Linq.Expressions.Expression Expression
        {
            get { return _collection.AsQueryable<T>().Expression; }
        }

        public IQueryProvider Provider
        {
            get { return _collection.AsQueryable<T>().Provider; }
        }
    }
}

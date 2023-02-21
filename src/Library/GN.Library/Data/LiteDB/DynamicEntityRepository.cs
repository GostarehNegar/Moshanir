
using GN.Library.Data;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using System.Linq;
using GN.Library.Shared.Entities;

namespace GN.Library.Data.Lite
{
    public class DynamicEntityLiteDBRepository<TEntity> : IDynamicEntityRepository<TEntity> where TEntity : DynamicEntity, new()
    {
        private string connectionString;
        private string collectionName;
        static DynamicEntityLiteDBRepository()
        {
            var mapper = BsonMapper.Global;
            mapper.Entity<TEntity>();
            //.Id(x => x.UniqueId);
        }
        public DynamicEntityLiteDBRepository()
        {
            var folder = Path.GetFullPath(
                Path.Combine(
                    Path.GetDirectoryName(this.GetType().Assembly.Location), "data"));
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            var fileName = Path.Combine(folder, $"{typeof(TEntity).Name}.db");
            this.connectionString = $"Filename={fileName};Connection=shared";
            this.collectionName = typeof(TEntity).Name;
        }
        private LiteDB.LiteDatabase GetDatabase()
        {
            for (int i = 0; i < 8; i++)
            {
                try
                {
                    return new LiteDB.LiteDatabase(this.connectionString);
                }
                catch { }
                System.Threading.Thread.Sleep(500);
            }
            return null;
        }

        private ILiteCollection<TEntity> GetCollection(LiteDatabase db)
        {
            return db.GetCollection<TEntity>();
        }
        public void Upsert(TEntity entity)
        {
            using (var db = this.GetDatabase())
            {
                var collection = this.GetCollection(db).Upsert(entity.Id, entity);
            }

        }
        public TEntity GetById(string id)
        {
            using (var db = this.GetDatabase())
            {
                var collection = this.GetCollection(db);
                return collection.FindById(id);

            }
        }

        public TEntity GetOrAddOrUpdate(string id, Action<TEntity> init)
        {
            TEntity result = null;
            using (var db = this.GetDatabase())
            {
                var collection = this.GetCollection(db);
                result = collection.FindById(id);
                if (init != null)
                {
                    result = result ?? new TEntity() { Id = id };
                    init(result);
                    collection.Upsert(result);
                }

            }
            return result;

        }

        public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate, int skip = 0, int limit = int.MaxValue)
        {
            using (var db = this.GetDatabase())
            {
                var collection = this.GetCollection(db);
                return collection.Find(predicate, skip, limit).ToArray();
            }
        }

        public void DeleteById(string id)
        {
            using (var db = this.GetDatabase())
            {
                var collection = this.GetCollection(db);
                collection.Delete(id);
            }
        }
    }
}

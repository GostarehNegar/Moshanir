using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Entities
{
    public class PackedDynamicEntityReference
    {
        public string I { get; set; }
        public string L { get; set; }
        public string N { get; set; }
        public long? T { get; set; }

    }
    public class DynamicEntityReference
    {
        public string Id { get; set; }
        public string LogicalName { get; set; }
        public string Name { get; set; }
        public PackedDynamicEntityReference Pack(long? time = null)
        {
            return new PackedDynamicEntityReference
            {
                I = this.Id,
                L = this.LogicalName,
                N=this.Name,
                T = time

            };
        }

    }

    public class DynamicEntity
    {
        public class Schema
        {
            public const string Description = "description";
            public const string ModiefiedOn = "modifiedon";
            public const string CreatedOn = "createdon";
            public const string TimeStamp = "$timestamp";
            public const string StateCode = "statecode";
            public const string StatusCode = "statuscode";
            public const string ModifiedOn = "modifiedon";
            public const string CreatedBy = "createdby";
            public const string ModifiedBy = "modifiedby";
            public const string Owner = "ownerid";
            public const string OwningUser = "owninguser";
            public const string OwningTeam = "owningteam";
            public const string OwningBuisnessUnit = "owningbusinessunit";
        }
        public DynamicEntity()
        {
            LogicalName = GetType().FullName;
            this.Time = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds();
        }
        public string UniqueId => $"{LogicalName}-{Id}";
        public string Id { get; set; }
        public string LogicalName { get; set; }
        public virtual long Time { get; set; }
        public virtual string Name { get => GetAttributeValue("name"); set => SetAttributeValue("name", value); }
        public DynamicAttributeCollection Attributes { get; set; } = new DynamicAttributeCollection();

        public DynamicEntityCollection RelatedObjects { get; set; } = new DynamicEntityCollection();
        public void SetAttributeValue(string key, string value)
        {
            this.Attributes = this.Attributes ?? new DynamicAttributeCollection();
            Attributes.AddOrUpdate(key, value);
        }
        public void SetAttributeValue(string key, object value)
        {
            this.Attributes = this.Attributes ?? new DynamicAttributeCollection();
            Attributes.AddOrUpdate(key, value);
        }
        public T GetAttributeValue<T>(string key)
        {
            this.Attributes = this.Attributes ?? new DynamicAttributeCollection();
            return Attributes.TryGetValue<T>(key, out var _t) ? _t : default;
        }
        public string GetAttributeValue(string key)
        {
            return GetAttributeValue<string>(key);
        }

        public void RemoveAttribute(string key)
        {
            this.Attributes = this.Attributes ?? new DynamicAttributeCollection();
            this.Attributes.TryRemove(key, out _);
        }
        public T GetId<T>()
        {
            if (this.Id == null)
            {
                return default;
            }
            if (typeof(T) == typeof(string))
            {
                return (T)(object)(this.Id);
            }
            if (typeof(T) == typeof(Guid))
            {
                return (T)(object)(Guid.TryParse(this.Id, out var _id) ? _id : Guid.Empty);
            }
            if (typeof(T) == typeof(Guid?))
            {
                return (T)(object)(Guid.TryParse(this.Id, out var _id) ? _id : (Guid?)null);
            }
            if (typeof(T) == typeof(int))
            {
                return (T)(object)(int.TryParse(this.Id, out var _id) ? _id : 0);
            }
            if (typeof(T) == typeof(int?))
            {
                return (T)(object)(int.TryParse(this.Id, out var _id) ? _id : (int?)null);
            }
            if (typeof(T) == typeof(long))
            {
                return (T)(object)(long.TryParse(this.Id, out var _id) ? _id : 0);
            }
            if (typeof(T) == typeof(long?))
            {
                return (T)(object)(long.TryParse(this.Id, out var _id) ? _id : (long?)null);
            }
            return default(T);

        }
        /*
        public DynamicPropertyCollection Properties { get; set; } = new DynamicPropertyCollection();
        public void SetPropertyValue(string key, string value)
        {
            Properties.AddOrUpdate(key, value);
        }
        public void SetPropertyValue(string key, object value)
        {
            Properties.AddOrUpdate(key, value);
        }
        public T GetPropertyValue<T>(string key)
        {
            return Properties.TryGetValue<T>(key, out var _t) ? _t : default;
        }
        public string GetPropertyValue(string key)
        {
            return GetPropertyValue<string>(key);
        }
        */
        public void AddObject(string key, DynamicEntity value)
        {
            this.RelatedObjects = this.RelatedObjects ?? new DynamicEntityCollection();
            this.RelatedObjects.AddOrUpdate(key, value, (a, b) => value);
        }
        public T GetObject<T>(string key) where T : DynamicEntity, new()
        {
            this.RelatedObjects = this.RelatedObjects ?? new DynamicEntityCollection();
            return this.RelatedObjects.TryGetValue(key, out var _r) ? _r.Cast<T>() : null;
        }
        public void RemoveObject(string key)
        {
            this.RelatedObjects = this.RelatedObjects ?? new DynamicEntityCollection();
            this.RelatedObjects.TryRemove(key, out _);
        }
        public void Merge(DynamicEntity other)
        {
            foreach (var att in other.Attributes)
            {
                SetAttributeValue(att.Key, att.Value);
            }
            //foreach (var att in other.Properties)
            //{
            //    SetPropertyValue(att.Key, att.Value);
            //}
            foreach (var ent in other.RelatedObjects)
            {
                AddObject(ent.Key, ent.Value);
            }

        }
        public T Cast<T>() where T : DynamicEntity, new()
        {
            var ret = new T
            {
                Id = Id,
                LogicalName = LogicalName,
                Time = this.Time,
                Attributes = Attributes,
                //Properties = Properties,
                RelatedObjects = RelatedObjects,
                //Name = Name
            };
            ret.Init();
            return ret;
        }
        public T To<T>() where T : DynamicEntity
        {
            var ret1 = Activator.CreateInstance<T>();


            ret1.Id = Id;
            ret1.LogicalName = LogicalName;
            ret1.Time = this.Time;
            ret1.Attributes = new DynamicAttributeCollection(Attributes);
            ret1.RelatedObjects = new DynamicEntityCollection(this.RelatedObjects);
            //ret1.Name = Name;
            ret1.Init();
            return ret1;
            //var ret = new T
            //{
            //    Id = Id,
            //    LogicalName = LogicalName,
            //    Time = this.Time,
            //    Attributes = new DynamicAttributeCollection(Attributes),
            //    //Properties = new DynamicPropertyCollection(Properties),
            //    RelatedObjects = new DynamicEntityCollection(this.RelatedObjects)
            //};
            //ret.Init();
            // return ret;

        }

        public virtual void Init()
        {

        }
        public override string ToString()
        {
            return $"{LogicalName} ({Id})";
        }
        public DynamicEntityReference ToEntityReference()
        {
            return new DynamicEntityReference
            {
                Id = this.Id,
                LogicalName = this.LogicalName,
                Name = this.Name
            };
        }
    }

    public class XrmDynamicEntity : DynamicEntity
    {

        public virtual string Description { get => GetAttributeValue(Schema.Description); set => SetAttributeValue(Schema.Description, value); }
        public DateTime? ModifiedOnEx { get => GetAttributeValue<DateTime?>(Schema.ModiefiedOn); set => SetAttributeValue(Schema.ModiefiedOn, value); }
    }
    //    public class DynamicEntity
    //    {
    //        public class Schema
    //        {
    //            public const string Description = "description";
    //            public const string ModiefiedOn = "modifiedon";
    //        }
    //        public DynamicEntity()
    //        {
    //            //Id = Guid.Empty.ToString();// Guid.NewGuid().ToString();
    //            LogicalName = GetType().FullName;
    //        }
    //        public string UniqueId => $"{LogicalName}-{Id}";
    //        public string Id { get; set; }
    //        public string LogicalName { get; set; }
    //        public virtual string Name { get => GetAttributeValue("name"); set => SetAttributeValue("name", value); }
    //        public virtual string Description { get => GetAttributeValue(Schema.Description); set => SetAttributeValue(Schema.Description, value); }
    //        public DateTime? ModifiedOnEx { get => GetAttributeValue<DateTime?>(Schema.ModiefiedOn); set => SetAttributeValue(Schema.ModiefiedOn, value); }
    //        public DynamicAttributeCollection Attributes { get; set; } = new DynamicAttributeCollection();
    //        public Dictionary<string, DynamicEntity> RelatedObjects { get; set; } = new Dictionary<string, DynamicEntity>();
    //        public void SetAttributeValue(string key, string value)
    //        {
    //            Attributes.AddOrUpdate(key, value);
    //        }
    //        public void SetAttributeValue(string key, object value)
    //        {
    //            Attributes.AddOrUpdate(key, value);
    //        }
    //        public T GetAttributeValue<T>(string key)
    //        {
    //            return Attributes.TryGetValue<T>(key, out var _t) ? _t : default;
    //        }
    //        public string GetAttributeValue(string key)
    //        {
    //            return GetAttributeValue<string>(key);
    //        }
    //        public void Merge(DynamicEntity other)
    //        {
    //            foreach (var att in other.Attributes)
    //            {
    //                SetAttributeValue(att.Key, att.Value);
    //            }
    //        }
    //        public T To<T>() where T : DynamicEntity, new()
    //        {
    //            return new T
    //            {
    //                Id = Id,
    //                LogicalName = LogicalName,
    //                Attributes = new DynamicAttributeCollection(Attributes)
    //            };
    //        }

    //        public override string ToString()
    //        {
    //            return $"{LogicalName} ({Id})";
    //        }
    //    }
}

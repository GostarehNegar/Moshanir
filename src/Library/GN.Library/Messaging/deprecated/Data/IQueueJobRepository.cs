using GN.Library.Data;
using GN.Library.Data.Internal;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Messaging.Data
{
    public interface IQueueJobRepository
    {
    }
    class QueueJobRepository : DocumentRepository<MessageQueueData>, IQueueJobRepository
    {
        public QueueJobRepository(string connectionString = null) : base(connectionString)
        {

        }
        public QueueJobRepository(ILiteDbContext context) : base(context)
        {

        }
        public QueueJobRepository(LiteDatabase db) : base(db)
        {

        }



    }
}

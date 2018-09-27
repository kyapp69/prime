using System;
using LiteDB;
using Prime.Base;
using Prime.Core;

namespace Prime.Scratch.Data
{
    public class DataTestModel : IModelBase
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [Bson]
        public string Name { get; set; }

        [Bson]
        public string[] Phones { get; set; }

        [Bson]
        public bool IsActive { get; set; }

        [Bson]
        public DateTime DateUtc { get; set; }

        [Bson]
        public DataTestSub DataTestSub { get; set; }
    }
}
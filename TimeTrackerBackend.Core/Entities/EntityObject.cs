using System;
using System.ComponentModel.DataAnnotations;
using TimeTrackerBackend.Core.Contracts;

namespace TimeTrackerBackend.Core.Entities
{
    public class EntityObject : IEntityObject
    {
        [Key]
        public Guid Id { get; set; }

        [Timestamp]
        public byte[] RowVersion
        {
            get;
            set;
        }
    }
}

namespace LiveScore.Shared.Database.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class DatabaseEntity
    {
        [Key]
        public string Id { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime ModifiedTime { get; set; }
    }
}

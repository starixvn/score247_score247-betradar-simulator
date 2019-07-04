namespace LiveScore.Soccer.Database.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class DatabaseEntity
    {
        [Key]
        public string Id { get; set; }

        public DateTimeOffset CreatedTime { get; set; }

        public DateTimeOffset ModifiedTime { get; set; }
    }
}

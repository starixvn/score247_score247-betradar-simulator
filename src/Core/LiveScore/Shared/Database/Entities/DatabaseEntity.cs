namespace LiveScore.Shared.Database.Entities
{
    using System;

    public class DatabaseEntity
    {
        public string Id { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime ModifiedTime { get; set; }
    }
}

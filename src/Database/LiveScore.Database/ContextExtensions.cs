namespace LiveScore.Shared.Database
{
    using Microsoft.EntityFrameworkCore;

    public static class ContextExtensions
    {
        public static string GetTableName<T>(this DbContext context) where T : class
        {
            var mapping = context.Model.FindEntityType(typeof(T)).Relational();
            var tableName = mapping.TableName;
            return tableName;
        }
    }
}

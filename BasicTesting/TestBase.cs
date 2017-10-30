using Microsoft.EntityFrameworkCore;

namespace EntityTesting3.BasicTesting
{
   public class TestBase
   {
      protected void CreateDb()
      {
         using(var dbContext = new BasicContext())
         {
            dbContext.Database.EnsureCreated();
         }
      }

      protected void DropDb()
      {
         using(var dbContext = new BasicContext())
         {
            dbContext.Database.EnsureDeleted();
         }
      }

      protected void ResetDb()
      {
         DropDb();
         CreateDb();
      }

      protected bool DoesDatabaseExist(string databaseName)
      {
         using(var dbContext = new RootContext())
         using(var command = dbContext.Database.GetDbConnection().CreateCommand())
         {
            command.CommandText = $"select exists (select 1 from pg_database where datname = '{databaseName}')";
            dbContext.Database.OpenConnection();
            return (bool)command.ExecuteScalar();
         }         
      }

      protected bool DoesTableExist(string tableName)
      {
         using(var dbContext = new BasicContext())
         using(var command = dbContext.Database.GetDbConnection().CreateCommand())
         {
            command.CommandText = $"select exists (select 1 from pg_tables where tablename = '{tableName}')";
            dbContext.Database.OpenConnection();
            return (bool)command.ExecuteScalar();
         }
      }      
   }
}

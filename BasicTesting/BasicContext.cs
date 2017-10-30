using System;
using Microsoft.EntityFrameworkCore;

namespace EntityTesting3.BasicTesting
{
   public class BasicContext : DbContext
   {
      public DbSet<Author> Authors { get; set; }
      public DbSet<Book> Books { get; set; }
      public DbSet<Address> Addresses { get; set; }

      protected override void OnConfiguring(DbContextOptionsBuilder ob)
      {
         ob.UseNpgsql("User ID=eftest;Host=localhost;Port=5432;Database=eftest;Pooling=true;");
      }
   }

   public class RootContext : DbContext
   {
      protected override void OnConfiguring(DbContextOptionsBuilder ob)
      {
         ob.UseNpgsql("User ID=postgres;Host=localhost;Port=5432;Database=postgres;Pooling=true;");
      }      
   }
}

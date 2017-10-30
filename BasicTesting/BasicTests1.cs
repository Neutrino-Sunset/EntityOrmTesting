using System.Linq;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EntityTesting3.BasicTesting.BasicTests1
{
   public class Author
   {
      public int AuthorId { get; set; }
      public string Name { get; set; }
      public Address Address { get; set; }
   }

   public class Address
   {
      public int AddressId { get; set; }
      public string Postcode { get; set; }
      public Author Author { get; set; }
      public int AuthorId { get; set; }
   }

   public class Publisher
   {
      public int PublisherId { get; set; }
      public string Name { get; set; }
      public Address Address { get; set; }
   }

   public class Context : DbContext
   {
      public DbSet<Author> Authors { get; set; }
      public DbSet<Publisher> Publishers { get; set; }
      public DbSet<Address> Addresses { get; set; }

      protected override void OnConfiguring(DbContextOptionsBuilder ob)
      {
         ob.UseNpgsql("User ID=eftest;Host=localhost;Port=5432;Database=eftest;Pooling=true;");
      }
   }

   /*public*/ class Tests
   {
      // Create a simple principle/dependent relationship.
      [Fact]
      public void Test1()
      {
         // Arrange.
         using(var dc = new Context())
         {
            dc.Database.EnsureDeleted();
            dc.Database.EnsureCreated();
         }

         // Act.
         using(var dc = new Context())
         {
            // Add Author and Address.
            var address1 = new Address() { Postcode = "1234" };
            var author = new Author() { Name = "Bob", Address = address1 };
            dc.Authors.Add(author);
            dc.SaveChanges();
         }

         // Assert.
         using(var dc = new Context())
         {
            // Verify Author and Addresses.
            Assert.Equal(1, dc.Authors.Count());
            Assert.Equal("Bob", dc.Authors.First().Name);
            Assert.Equal(1, dc.Addresses.Count());
            Assert.Equal("1234", dc.Authors.Include(a => a.Address).First().Address.Postcode);
         }

         // Delete Author test for cascade delete of Address.
         using(var dc = new Context())
         {
            Author author = dc.Authors.Include(a => a.Address).First();
            dc.Authors.Remove(author);
            dc.SaveChanges();

            // FAIL. Doesn't delete the address.
            Assert.Equal(0, dc.Addresses.Count());
         }
      }
   }
}

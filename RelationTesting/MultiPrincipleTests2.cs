using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

// In this test the dependent defines explicit FK properties only on grounds that navigating from dependent to principle
// is unlikely to be very useful and it will keep the code simpler.
namespace EntityTesting3.BasicTesting.MultiPrincipleTests2
{
   public class Author
   {
      public int AuthorId { get; set; }
      public string Name { get; set; }
      public Address Address { get; set; }
   }

   public class Publisher
   {
      public int PublisherId { get; set; }
      public string Name { get; set; }
      public Address Address { get; set; }
   }

   public class Address
   {
      public int AddressId { get; set; }

      public int? AuthorId { get; set; }
      public int? PublisherId { get; set; }

      public string Postcode { get; set; }
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

      protected override void OnModelCreating(ModelBuilder modelBuilder)
      {
         modelBuilder.Entity<Author>()
            .HasOne(e => e.Address)
            .WithOne()
            .HasForeignKey<Address>(e => e.AuthorId)
            .OnDelete(DeleteBehavior.Cascade);

         modelBuilder.Entity<Publisher>()
            .HasOne(e => e.Address)
            .WithOne()
            .HasForeignKey<Address>(e => e.PublisherId)
            .OnDelete(DeleteBehavior.Cascade);            
      }

      public void ResetDb()
      {
         Database.EnsureDeleted();
         Database.EnsureCreated();
      }
   }


   /*public*/ class MultiPrincipleTestClass
   {
      [Fact]
      public void CreateOneOfEach()
      {
         // Arrange.
         using(var dbc = new Context())
         {
            dbc.ResetDb();
         }

         // Act.
         using(var dbc = new Context())
         {
            // Add Author and Address.
            var address1 = new Address() { Postcode = "1234" };
            var author = new Author() { Name = "Bob", Address = address1 };
            dbc.Authors.Add(author);

            // Add Publisher and Address.
            var address2 = new Address() { Postcode = "2345" };
            var publisher = new Publisher() { Name = "Fred", Address = address2 };
            dbc.Publishers.Add(publisher);

            dbc.SaveChanges();
         }

         // Assert.
         using(var dbc = new Context())
         {
            // Verify Author, Publisher and Addresses.
            Assert.Equal(1, dbc.Authors.Count());
            Assert.Equal("Bob", dbc.Authors.First().Name);
            Assert.Equal(1, dbc.Publishers.Count());
            Assert.Equal("Fred", dbc.Publishers.First().Name);
            Assert.Equal(2, dbc.Addresses.Count());
            Assert.Equal("1234", dbc.Authors.Include(a => a.Address).First().Address.Postcode);
            Assert.Equal("2345", dbc.Publishers.Include(p => p.Address).First().Address.Postcode);
         }         
      }

      // Disconnected update.
      [Fact]
      public void Update()
      {
         // Arrange.
         using(var dbc = new Context())
         {
            dbc.ResetDb();

            // Add Author.
            var author = new Author() { Name = "Bob" };
            dbc.Authors.Add(author);
            dbc.SaveChanges();
         }

         using(var dbc = new Context())
         {
            // Verify Author.
            Assert.Equal(1, dbc.Authors.Count());
            Assert.Equal("Bob", dbc.Authors.First().Name);
            Assert.Equal(0, dbc.Addresses.Count());
         }

         // Act.
         using(var dbc = new Context())
         {
            Author author = new Author() { AuthorId = 1, Name = "Fred" };
            dbc.Authors.Update(author);
            dbc.SaveChanges();
         }

         // Assert.
         using(var dbc = new Context())
         {
            // Verify Author, Publisher and Addresses.
            Assert.Equal(1, dbc.Authors.Count());
            Assert.Equal("Fred", dbc.Authors.First().Name);
            Assert.Equal(0, dbc.Addresses.Count());
         }
      }

      [Fact]
      public void UpdateNavigationProperty()
      {
         // Arrange.
         using(var dbc = new Context())
         {
            dbc.ResetDb();

            // Add Author and Address.
            var address1 = new Address() { Postcode = "1234" };
            var author = new Author() { Name = "Bob", Address = address1 };
            dbc.Authors.Add(author);
            dbc.SaveChanges();
         }

         using(var dbc = new Context())
         {
            // Verify Author and Address.
            Assert.Equal(1, dbc.Authors.Count());
            Assert.Equal("Bob", dbc.Authors.First().Name);
            Assert.Equal(1, dbc.Addresses.Count());
            Assert.Equal("1234", dbc.Authors.Include(a => a.Address).First().Address.Postcode);
         }

         // Act.
         Author disconnectedAuthor = null;

         using(var dbc = new Context())
         {
            disconnectedAuthor = dbc.Authors.AsNoTracking().Include(a => a.Address).First();
         }

         disconnectedAuthor.Name = "Fred";
         disconnectedAuthor.Address.Postcode = "2345";

         using(var dbc = new Context())
         {
            // Calling update on either the context or the entity both work.
            //dbc.Update(disconnectedAuthor);
            dbc.Authors.Update(disconnectedAuthor);
            dbc.SaveChanges();
         }

         // Assert.
         using(var dbc = new Context())
         {
            // Verify Author and Address.
            Assert.Equal(1, dbc.Authors.Count());
            Assert.Equal("Fred", dbc.Authors.First().Name);
            Assert.Equal(1, dbc.Addresses.Count());
            Assert.Equal("2345", dbc.Authors.Include(a => a.Address).First().Address.Postcode);
         }         
      }

      [Fact]
      public void TestCascadeDelete()
      {
         // Arrange.
         using(var dbc = new Context())
         {
            dbc.ResetDb();

            // Add Author and Address.
            var address1 = new Address() { Postcode = "1234" };
            var author = new Author() { Name = "Bob", Address = address1 };
            dbc.Authors.Add(author);

            // Add Publisher and Address.
            var address2 = new Address() { Postcode = "2345" };
            var publisher = new Publisher() { Name = "Fred", Address = address2 };
            dbc.Publishers.Add(publisher);

            dbc.SaveChanges();
         }

         using(var dbc = new Context())
         {
            // Verify Author, Publisher and Addresses.
            Assert.Equal(1, dbc.Authors.Count());
            Assert.Equal("Bob", dbc.Authors.First().Name);
            Assert.Equal(1, dbc.Publishers.Count());
            Assert.Equal("Fred", dbc.Publishers.First().Name);
            Assert.Equal(2, dbc.Addresses.Count());
            Assert.Equal("1234", dbc.Authors.Include(a => a.Address).First().Address.Postcode);
            Assert.Equal("2345", dbc.Publishers.Include(p => p.Address).First().Address.Postcode);
         }         

         // Act.

         Author disconnectedAuthor = null;

         using(var dbc = new Context())
         {
            disconnectedAuthor = dbc.Authors.First();
         }

         using(var dbc = new Context())
         {
            dbc.Remove(disconnectedAuthor);
            dbc.SaveChanges();
         }

         // Assert.
         using(var dbc = new Context())
         {
            // Verify Author, Publisher and Addresses.
            Assert.Equal(0, dbc.Authors.Count());
            Assert.Equal(1, dbc.Publishers.Count());
            Assert.Equal("Fred", dbc.Publishers.First().Name);
            Assert.Equal(1, dbc.Addresses.Count());
            Assert.Equal("2345", dbc.Publishers.Include(p => p.Address).First().Address.Postcode);            
         }
      }
   }
}

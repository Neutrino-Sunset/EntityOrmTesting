using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Xunit;


namespace EntityTesting3.BasicTesting
{
   /*public*/ class BasicTests : TestBase
   {
      [Fact]
      public void DatabaseCreation()
      {
         // Arrange.
         DropDb();
         Assert.False(DoesDatabaseExist("eftest"));

         // Act.
         CreateDb();

         // Assert.
         string[] tableNames = 
         {
            "Authors", "Books", "Addresses"
         };
         Assert.True(tableNames.All(DoesTableExist));
      }

      // Demonstrates that calling Update is not required when changing the value of an entity returned from the current
      // DbContext.
      [Fact]
      public void TestUpdate()
      {
         // Arrange.
         ResetDb();

         using(var context = new BasicContext())
         {
            Author[] authors = new[]
            {
               new Author { Name = "Bob"},
               new Author { Name = "Fred" }
            };
            context.Authors.AddRange(authors);
            context.SaveChanges();            
         }

         using(var context = new BasicContext())
         {
            Author a = context.Authors.FirstOrDefault(x => x.Name == "Bob");
            Assert.NotNull(a);
         }

         // Act.
         using(var context = new BasicContext())
         {
            Author a = context.Authors.First(x => x.Name == "Bob");
            a.Name = "Bill";
            context.SaveChanges();
         }

         // Assert.
         using(var context = new BasicContext())
         {
            Author a = context.Authors.FirstOrDefault(x => x.Name == "Bill");
            Assert.NotNull(a);
         }
      }

      // Demonstrates that modifications are not reflected in the current DbContext until SaveChanges is called.
      [Fact]
      public void TestSaveChanges()
      {
         // Arrange.
         ResetDb();

         using(var context = new BasicContext())
         {
            Author[] authors = new[]
            {
               new Author { Name = "Bob"},
               new Author { Name = "Fred" }
            };
            context.Authors.AddRange(authors);
            context.SaveChanges();            
         }

         using(var context = new BasicContext())
         {
            Author a = context.Authors.FirstOrDefault(x => x.Name == "Bob");
            Assert.NotNull(a);
         }

         // Act.
         using(var context = new BasicContext())
         {
            Author a = context.Authors.First(x => x.Name == "Bob");
            a.Name = "Bill";

            Author a1 = context.Authors.FirstOrDefault(x => x.Name == "Bill");
            Assert.Null(a1);
         }

         // Assert.
      }

      // Calling ToArray on a returned entity collection doesn't detach it from being tracked.
      [Fact]
      public void TestToArrayTracking()
      {
         // Arrange.
         ResetDb();

         using(var context = new BasicContext())
         {
            Author[] authors = new[]
            {
               new Author { Name = "Bob"},
               new Author { Name = "Fred" }
            };
            context.Authors.AddRange(authors);
            context.SaveChanges();            
         }

         using(var context = new BasicContext())
         {
            Author a = context.Authors.FirstOrDefault(x => x.Name == "Bob");
            Assert.NotNull(a);
         }
         
         // Act.
         using(var context = new BasicContext())
         {
            Author[] authors = context.Authors.ToArray();
            authors[0].Name = "Bill";
            context.SaveChanges();

            Author a1 = context.Authors.FirstOrDefault(x => x.Name == "Bill");
            Assert.NotNull(a1);
         }         
      }

      [Fact]
      public void TestEnumerate()
      {
         // Arrange.
         ResetDb();

         using(var context = new BasicContext())
         {
            Author[] authors = new[]
            {
               new Author { Name = "Bob"},
               new Author { Name = "Fred" }
            };
            context.Authors.AddRange(authors);

            Book[] books = new[]
            {
               new Book { Title = "Book 1" },
               new Book { Title = "Book 2" }
            };

            context.Books.AddRange(books);
            context.SaveChanges();            
         }

         using(var context = new BasicContext())
         {
            Author a = context.Authors.FirstOrDefault(x => x.Name == "Bob");
            Assert.NotNull(a);
         }

         // Act.
         bool exception = false;

         using(var context = new BasicContext())
         {
            foreach(Author a in context.Authors)
            {
               try
               {
                  string title = context.Books.First().Title;
               }
               catch(Exception)
               {
                  exception = true;
               }
            }
         }

         // Assert.
         Assert.True(exception);
      }

      // Demonstrates that any tracked entity instance can be removed even if other references to the entity exist.
      [Fact]
      public void TestRemove1()
      {
         // Arrange.
         ResetDb();

         using(var context = new BasicContext())
         {
            Author[] authors = new[]
            {
               new Author { Name = "Bob"},
               new Author { Name = "Fred" }
            };
            context.Authors.AddRange(authors);
            context.SaveChanges();            
         }

         using(var context = new BasicContext())
         {
            Author a = context.Authors.FirstOrDefault(x => x.Name == "Bob");
            Assert.NotNull(a);
         }

         // Act.
         using(var context = new BasicContext())
         {
            Author a = context.Authors.First(x => x.Name == "Bob");
            Author b = context.Authors.First(x => x.Name == "Bob");
            context.Authors.Remove(a);
         }
      }

      // Demonstrates that the optimisation of creating a stub object in order to remove an object without having to
      // obtain an instance from the DB first, does not work if a tracked instance exists.
      [Fact]
      public void TestRemove2()
      {
         // Arrange.
         ResetDb();

         using(var context = new BasicContext())
         {
            Author[] authors = new[]
            {
               new Author { Name = "Bob"},
               new Author { Name = "Fred" }
            };
            context.Authors.AddRange(authors);
            context.SaveChanges();            
         }

         using(var context = new BasicContext())
         {
            Author a = context.Authors.FirstOrDefault(x => x.Name == "Bob");
            Assert.NotNull(a);
         }

         // Act.
         bool exception = false;
         using(var context = new BasicContext())
         {
            Author a = context.Authors.First(x => x.Name == "Bob");
            Author b = new Author();
            b.AuthorId = a.AuthorId;
            try
            {
            context.Authors.Remove(b);
            }
            catch(Exception)
            {
               exception = true;
            }
         }

         // Assert.
         Assert.True(exception);
      }

      [Fact]
      public void TestTransaction()
      {
         // Arrange.
         ResetDb();

         using(var context = new BasicContext())
         {
            using(var transaction = context.Database.BeginTransaction())
            {
               Author[] authors = new[]
               {
                  new Author { Name = "Bob"},
                  new Author { Name = "Fred" }
               };
               context.Authors.AddRange(authors);
               context.SaveChanges();
               //transaction.Commit();
            }
         }

         // Assert.
         using(var context = new BasicContext())
         {
            Assert.Equal(0, context.Authors.Count());
         }
      }

      [Fact]
      public void IsUncommittedTransactionVisibleInOtherContext()
      {
         // Arrange.
         ResetDb();

         // Act.

         // Add a record using one DbContext in a transaction.
         var cn1 = new BasicContext();
         var tr = cn1.Database.BeginTransaction();
         cn1.Authors.Add(new Author { Name = "Bob" });
         cn1.SaveChanges();

         // Check whether that change is visible in the DB to another DbContext before it is committed.
         using(var cn2 = new BasicContext())
         {
            Author a = cn2.Authors.FirstOrDefault(x => x.Name == "Bob");
            Assert.Null(a);
         }

         tr.Dispose();
         cn1.Dispose();
      }
   }
}

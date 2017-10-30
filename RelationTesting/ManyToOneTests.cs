
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace EntityTesting3.RelationTesting.ManyToOneTests
{
   public class Parent
   {
      public int ParentId { get; set; }
      public string Data1 { get; set; }
      public List<Child> Children { get; set; }
   }

   public class Child
   {
      public int ChildId { get; set; }
      public string Data1 { get; set; }
      //public int ParentId { get; set; }
      public Parent Parent { get; set; }
   }

   public class Context : DbContext
   {
      public DbSet<Parent> Parents { get; set; }
      public DbSet<Child> Childs { get; set; }

      protected override void OnConfiguring(DbContextOptionsBuilder ob)
      {
         ob.UseNpgsql("User ID=eftest;Host=localhost;Port=5432;Database=eftest;Pooling=true;");
      }

      protected override void OnModelCreating(ModelBuilder mb)
      {
         mb.Entity<Child>()
            .HasOne(c => c.Parent)
            .WithMany(p => p.Children)
            .OnDelete(DeleteBehavior.SetNull);
      }

      public void ResetDb()
      {
         Database.EnsureDeleted();
         Database.EnsureCreated();
      }
   }

   /*public*/ class ManyToOneTestClass
   {
      [Fact]
      public void Create()
      {
         // Arrange.
         using(var dbc = new Context())
         {
            dbc.ResetDb();
         }

         // Act.
         using(var dbc = new Context())
         {
            List<Child> children = new List<Child> {
               new Child() { Data1 = "Data1" },
               new Child() { Data1 = "Data2" }
            };

            Parent parent = new Parent() { Data1 = "Data1", Children = children };
            dbc.Add(parent);
            dbc.SaveChanges();
         }

         // Assert.
         using(var dbc = new Context())
         {
            Assert.Equal(1, dbc.Parents.Count());
            Assert.Equal(2, dbc.Childs.Count());
         }
      }

      [Fact]
      public void CreateChildOnly()
      {
         // Arrange.
         using(var dbc = new Context())
         {
            dbc.ResetDb();
         }

         // Arrange.
         using(var dbc = new Context())
         {
            List<Child> children = new List<Child> {
               new Child() { Data1 = "Data1" },
               new Child() { Data1 = "Data2" }
            };

            dbc.Childs.AddRange(children);
            dbc.SaveChanges();
         }
         
         // Assert.
         using(var dbc = new Context())
         {
            Assert.Equal(2, dbc.Childs.Count());
         }
      }

      [Fact]
      public void CascadeDelete()
      {
         // Arrange.
         using(var dbc = new Context())
         {
            dbc.ResetDb();
            List<Child> children = new List<Child> {
               new Child() { Data1 = "Data1" },
               new Child() { Data1 = "Data2" }
            };

            Parent parent = new Parent() { Data1 = "Data1", Children = children };
            dbc.Add(parent);
            dbc.SaveChanges();
         }

         // Act.
         using(var dbc = new Context())
         {
            Parent parent = dbc.Parents.Include(p => p.Children).First();
            //parent.Children.Clear();
            dbc.Remove(parent);

            // Using a navigation property only relation this will fail throwing an exception complaining of violating
            // contraint "FK_Childs_Parents_ParentId" on table "Childs".
            // Adding explicit ParentId foreign key to Child deleting the parent then works, but also cascade deletes
            // all the children by default.
            // If you retrieve all the children with the parent, then clear the children out of the parent before
            // removing the parent, then it works and the children are left behind.
            // The behaviour of leaving the children behind when the parent is deleted can be achieved automatically by
            // using an 'OnDelete' clause in OnModelCreating.
            dbc.SaveChanges();
         }

         // Assert.
         using(var dbc = new Context())
         {
            Assert.Equal(0, dbc.Parents.Count());
            Assert.Equal(2, dbc.Childs.Count());
         }
      }

      // Deleting a child automatically removes it from the parent.
      [Fact]
      public void DeleteChild()
      {
         // Arrange.
         using(var dbc = new Context())
         {
            dbc.ResetDb();
            List<Child> children = new List<Child> {
               new Child() { Data1 = "Data1" },
               new Child() { Data1 = "Data2" }
            };

            Parent parent = new Parent() { Data1 = "Data1", Children = children };
            dbc.Add(parent);
            dbc.SaveChanges();
         }

         using(var dbc = new Context())
         {
            Assert.Equal(1, dbc.Parents.Count());
            Assert.Equal(2, dbc.Childs.Count());
            Assert.Equal(2, dbc.Parents.Include(p => p.Children).First().Children.Count());
         }         

         // Act.
         using(var dbc = new Context())
         {
            Child child = dbc.Childs.First();
            dbc.Remove(child);
            dbc.SaveChanges();            
         }

         // Assert.
         using(var dbc = new Context())
         {
            Assert.Equal(1, dbc.Parents.Count());
            Assert.Equal(1, dbc.Childs.Count());
            Assert.Equal(1, dbc.Parents.Include(p => p.Children).First().Children.Count());
         }         
      }
   }
}

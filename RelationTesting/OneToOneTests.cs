using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace EntityTesting3.BasicTesting.OneToOneTests
{
   public class Parent
   {
      public int ParentId { get; set; }

      public Child Child { get; set; }

      public string Data1 { get; set; }
      public string Data2 { get; set; }      
   }

   public class Child
   {
      public int ChildId { get; set; }
      
      public int ParentId { get; set; }
      public Parent Parent { get; set; }

      public string Data1 { get; set; }
      public string Data2 { get; set; }
   }

   public class Context : DbContext
   {
      public DbSet<Parent> Parents { get; set; }
      public DbSet<Child> Childs { get; set; }

      protected override void OnConfiguring(DbContextOptionsBuilder ob)
      {
         ob.UseNpgsql("User ID=eftest;Host=localhost;Port=5432;Database=eftest;Pooling=true;");
      }

      protected override void OnModelCreating(ModelBuilder modelBuilder)
      {}

      public void ResetDb()
      {
         Database.EnsureDeleted();
         Database.EnsureCreated();
      }
   }

   /*public*/ class OneToOneTestClass
   {
      [Fact]
      public void TestCreate()
      {
         // Arrange.
         using(var dbc = new Context())
         {
            dbc.ResetDb();
            Child child = new Child() { Data1 = "Data1", Data2 = "Data2" };
            Parent parent = new Parent() { Child = child, Data1 = "Data1", Data2 = "Data2" };
            dbc.Add(parent);
            dbc.SaveChanges();
         }

         // Assert.
         using(var dbc = new Context())
         {
            Assert.Equal(1, dbc.Parents.Count());
            Assert.Equal(1, dbc.Childs.Count());
            Parent parent = dbc.Parents.Include(p => p.Child).First();
            Assert.Equal("Data1", parent.Data1);
            Assert.Equal("Data1", parent.Child.Data1);
            Assert.Equal(parent.ParentId, parent.Child.ParentId);
         }
      }

      // Cascade delete works even if you only delete the parent.
      [Fact]
      public void TestCascadeDelete()
      {
         // Arrange.
         using(var dbc = new Context())
         {
            dbc.ResetDb();
            Child child = new Child() { Data1 = "Data1", Data2 = "Data2" };
            Parent parent = new Parent() { Child = child, Data1 = "Data1", Data2 = "Data2" };
            dbc.Add(parent);
            dbc.SaveChanges();
         }

         using(var dbc = new Context())
         {
            Assert.Equal(1, dbc.Parents.Count());
            Assert.Equal(1, dbc.Childs.Count());
            Parent parent = dbc.Parents.Include(p => p.Child).First();
            Assert.Equal(parent.ParentId, parent.Child.ParentId);
         }

         // Act.
         using(var dbc = new Context())
         {
            Parent parent = dbc.Parents.First();
            dbc.Remove(parent);
            dbc.SaveChanges();
         }

         // Assert.
         using(var dbc = new Context())
         {
            Assert.Equal(0, dbc.Parents.Count());
            Assert.Equal(0, dbc.Childs.Count());
         }         
      }

      //[Fact]
      public void TestConnectedUpdateParent()
      {
         // Arrange.
         using(var dbc = new Context())
         {
            dbc.ResetDb();
            Child child = new Child() { Data1 = "Data1", Data2 = "Data2" };
            Parent parent = new Parent() { Child = child, Data1 = "Data1", Data2 = "Data2" };
            dbc.Add(parent);
            dbc.SaveChanges();
         }

         // Act.
         using(var dbc = new Context())
         {
            Parent parent = dbc.Parents.Include(p => p.Child).First();

            // Updating a single field of a connected entity generates SQL that updates only that column.
            parent.Data1 = "Data1_modified";

            // When setting a field to its current value Entity recognises that it has not changed and so does not
            // update it. Given a modified disconnected entity, it should be possible to retrieve that entity from the
            // database, map the modified entity onto it (perhaps using AutoMapper), then update, causing only the
            // modified fields to be updated. Retrieving the entity from the database would cause an additional
            // round trip though so possibly not worth it.
            parent.Data2 = "Data2";
            dbc.SaveChanges();
         }

         // Assert.
         using(var dbc = new Context())
         {
            Assert.Equal(1, dbc.Parents.Count());
            Assert.Equal(1, dbc.Childs.Count());
            Parent parent = dbc.Parents.First();
            Assert.Equal("Data1_modified", parent.Data1);
         }
      }

      [Fact]
      public void TestConnectedUpdateChild()
      {
         // Arrange.
         using(var dbc = new Context())
         {
            dbc.ResetDb();
            Child child = new Child() { Data1 = "Data1", Data2 = "Data2" };
            Parent parent = new Parent() { Child = child, Data1 = "Data1", Data2 = "Data2" };
            dbc.Add(parent);
            dbc.SaveChanges();
         }

         // Act.
         using(var dbc = new Context())
         {
            Parent parent = dbc.Parents.Include(p => p.Child).First();
            // In a connected scenario updating a field on a navigation property generates SQL that updates only that
            // table.
            parent.Child.Data1 = "Data1_modified";
            dbc.SaveChanges();
         }

         // Assert.
         using(var dbc = new Context())
         {
            Assert.Equal(1, dbc.Parents.Count());
            Assert.Equal(1, dbc.Childs.Count());
            Parent parent = dbc.Parents.Include(p => p.Child).First();
            Assert.Equal("Data1_modified", parent.Child.Data1);
         }
      }

      // Updating a disconnected graph when only the parent has changed updates the entire graph.
      [Fact]
      public void TestDisconnectedUpdateParent()
      {
         // Arrange.
         using(var dbc = new Context())
         {
            dbc.ResetDb();
            Child child = new Child() { Data1 = "Data1", Data2 = "Data2" };
            Parent parent = new Parent() { Child = child, Data1 = "Data1", Data2 = "Data2" };
            dbc.Add(parent);
            dbc.SaveChanges();
         }

         // Act.
         Parent disconnectedParent = null;

         using(var dbc = new Context())
         {
            disconnectedParent = dbc.Parents.Include(p => p.Child).First();
         }

         disconnectedParent.Data1 = "Data1_modified";

         using(var dbc = new Context())
         {
            // Updating a disconnected entity updates every field in the entire object graph.
            dbc.Update(disconnectedParent);
            dbc.SaveChanges();
         }

         // Assert.
         using(var dbc = new Context())
         {
            Assert.Equal(1, dbc.Parents.Count());
            Assert.Equal(1, dbc.Childs.Count());
            Parent parent = dbc.Parents.First();
            Assert.Equal("Data1_modified", parent.Data1);
         }         
      }

      // Updating a disconnected graph when only the child has changed also updates the entire graph.
      [Fact]
      public void TestDisconnectedUpdateChild()
      {
         // Arrange.
         using(var dbc = new Context())
         {
            dbc.ResetDb();
            Child child = new Child() { Data1 = "Data1", Data2 = "Data2" };
            Parent parent = new Parent() { Child = child, Data1 = "Data1", Data2 = "Data2" };
            dbc.Add(parent);
            dbc.SaveChanges();
         }

         // Act.
         Parent disconnectedParent = null;

         using(var dbc = new Context())
         {
            disconnectedParent = dbc.Parents.Include(p => p.Child).First();
         }

         disconnectedParent.Child.Data1 = "Data1_modified";

         using(var dbc = new Context())
         {
            // Updating a disconnected entity updates every field in the entire object graph.
            dbc.Update(disconnectedParent);
            dbc.SaveChanges();
         }

         // Assert.
         using(var dbc = new Context())
         {
            Assert.Equal(1, dbc.Parents.Count());
            Assert.Equal(1, dbc.Childs.Count());
            Parent parent = dbc.Parents.Include(p => p.Child).First();
            Assert.Equal("Data1_modified", parent.Child.Data1);
         }         
      }

      // Setting a child navigation property to null does not disassociate the child element from the parent because the
      // association is maintained by the ParentId foreign key in the child.
      [Fact]
      public void TestDisconnectedUpdateParentNullChild()
      {
         // Arrange.
         using(var dbc = new Context())
         {
            dbc.ResetDb();
            Child child = new Child() { Data1 = "Data1", Data2 = "Data2" };
            Parent parent = new Parent() { Child = child, Data1 = "Data1", Data2 = "Data2" };
            dbc.Add(parent);
            dbc.SaveChanges();
         }

         // Act.
         Parent disconnectedParent = null;

         using(var dbc = new Context())
         {
            disconnectedParent = dbc.Parents.Include(p => p.Child).First();
         }

         disconnectedParent.Data1 = "Data1_modified";
         disconnectedParent.Child = null;

         using(var dbc = new Context())
         {
            // Updating a disconnected entity with navigation properties set to null only updates the parent object.
            dbc.Update(disconnectedParent);
            dbc.SaveChanges();
         }

         // Assert.
         using(var dbc = new Context())
         {
            // The navigation property still works.
            Assert.Equal(1, dbc.Parents.Count());
            Assert.Equal(1, dbc.Childs.Count());
            Parent parent = dbc.Parents.Include(p => p.Child).First();
            Assert.Equal("Data1", parent.Child.Data1);
            dbc.Remove(parent);
            dbc.SaveChanges();
         }

         using(var dbc = new Context())
         {
            // Cascade delete also works.
            Assert.Equal(0, dbc.Parents.Count());
            Assert.Equal(0, dbc.Childs.Count());            
         }
      }
   }
}

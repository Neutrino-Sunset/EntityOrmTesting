
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace EntityTesting3.RelationTesting.ManyToOneTests1
{
   // A many to one relation between User (principle) and Projects (dependents).
   // Removing a Project should just remove it from the Users set of Projects.
   // Removing a User should just set related Projects.Author to null.
   public class User
   {
      public int UserId { get; set; }
      public string Name { get; set; }
      public List<Project> Projects { get; set; }
   }

   public class Project
   {
      public int ProjectId { get; set; }
      public string Name { get; set; }
      public User Author { get; set; }
   }

   public class Context : DbContext
   {
      public DbSet<User> Users { get; set; }
      public DbSet<Project> Projects { get; set; }

      protected override void OnConfiguring(DbContextOptionsBuilder ob)
      {
         ob.UseNpgsql("User ID=eftest;Host=localhost;Port=5432;Database=eftest;Pooling=true;");
      }

      protected override void OnModelCreating(ModelBuilder mb)
      {
         // mb.Entity<Project>()
         //    .HasOne(c => c.User)
         //    .WithMany(p => p.Projects)
         //    .OnDelete(DeleteBehavior.SetNull);
      }

      public void ResetDb()
      {
         Database.EnsureDeleted();
         Database.EnsureCreated();
      }
   }

   public class ManyToOneTests1Class
   {
      // Ensure the entity definitions exhibit the correct relational behaviour.
      [Fact]
      public void CreateRelation()
      {
         // Arrange.
         using(var dbc = new Context())
         {
            dbc.ResetDb();
         }

         // Act.
         using(var dbc = new Context())
         {
            List<Project> projects = new List<Project> {
               new Project() { Name = "Project1" },
               new Project() { Name = "Project2" }
            };

            User user = new User() { Name = "User1", Projects = projects };
            dbc.Add(user);
            dbc.SaveChanges();
         }

         // Assert.
         using(var dbc = new Context())
         {
            Assert.Equal(1, dbc.Users.Count());
            Assert.Equal(2, dbc.Projects.Count());
            Assert.Equal(2, dbc.Users.Include(u => u.Projects).First().Projects.Count());
            Assert.Equal("User1", dbc.Projects.Include(p => p.Author).First().Author.Name);
         }
      }

      [Fact]
      public void AddRelation()
      {
         // Arrange.
         using(var dbc = new Context())
         {
            dbc.ResetDb();
            List<Project> projects = new List<Project> {
               new Project() { Name = "Project1" },
               new Project() { Name = "Project2" }
            };

            User user = new User() { Name = "User1", Projects = projects };
            dbc.Add(user);
            dbc.SaveChanges();
         }

         using(var dbc = new Context())
         {
            Assert.Equal(1, dbc.Users.Count());
            Assert.Equal(2, dbc.Projects.Count());
            Assert.Equal(2, dbc.Users.Include(u => u.Projects).First().Projects.Count());
            Assert.Equal("User1", dbc.Projects.Include(p => p.Author).First().Author.Name);
         }

         // Act.
         using(var dbc = new Context())
         {
            User user = dbc.Users.First();
            Project newProject = new Project() { Name = "Project3" };
            newProject.Author = user;
            dbc.Add(newProject);
            dbc.SaveChanges();
         }

         // Assert.
         using(var dbc = new Context())
         {
            Assert.Equal(1, dbc.Users.Count());
            Assert.Equal(3, dbc.Projects.Count());
            Assert.Equal(3, dbc.Users.Include(u => u.Projects).First().Projects.Count());
            Assert.Equal("User1", dbc.Projects.Include(p => p.Author).Last().Author.Name);
         }
      }

      // A Project can be created with a null Author.
      [Fact]
      public void CreateProjectOnly()
      {
         // Arrange.
         using(var dbc = new Context())
         {
            dbc.ResetDb();
         }

         // Arrange.
         using(var dbc = new Context())
         {
            List<Project> projects = new List<Project> {
               new Project() { Name = "Project1" },
               new Project() { Name = "Project2" }
            };

            dbc.Projects.AddRange(projects);
            dbc.SaveChanges();
         }
         
         // Assert.
         using(var dbc = new Context())
         {
            Assert.Equal(2, dbc.Projects.Count());
         }
      }

      [Fact]
      public void CascadeDelete()
      {
         // Arrange.
         using(var dbc = new Context())
         {
            dbc.ResetDb();
            List<Project> projects = new List<Project> {
               new Project() { Name = "Project1" },
               new Project() { Name = "Project2" }
            };

            User user = new User() { Name = "User1", Projects = projects };
            dbc.Add(user);
            dbc.SaveChanges();
         }

         // Act.
         using(var dbc = new Context())
         {
            User user = dbc.Users.Include(p => p.Projects).First();
            //user.Projects.Clear();
            dbc.Remove(user);

            // Using a navigation property only relation this will fail throwing an exception complaining of violating
            // contraint "FK_Childs_Parents_ParentId" on table "Projects".
            // Adding explicit ParentId foreign key to Project deleting the user then works, but also cascade deletes
            // all the projects by default.
            // If you retrieve all the projects with the user, then clear the projects out of the user before
            // removing the user, then it works and the projects are left behind.
            // The behaviour of leaving the projects behind when the user is deleted can be achieved automatically by
            // using an 'OnDelete' clause in OnModelCreating.
            dbc.SaveChanges();
         }

         // Assert.
         using(var dbc = new Context())
         {
            Assert.Equal(0, dbc.Users.Count());
            Assert.Equal(2, dbc.Projects.Count());
         }
      }

      // Deleting a project automatically removes it from the user.
      [Fact]
      public void DeleteProject()
      {
         // Arrange.
         using(var dbc = new Context())
         {
            dbc.ResetDb();
            List<Project> projects = new List<Project> {
               new Project() { Name = "Project1" },
               new Project() { Name = "Project2" }
            };

            User user = new User() { Name = "User1", Projects = projects };
            dbc.Add(user);
            dbc.SaveChanges();
         }

         using(var dbc = new Context())
         {
            Assert.Equal(1, dbc.Users.Count());
            Assert.Equal(2, dbc.Projects.Count());
            Assert.Equal(2, dbc.Users.Include(p => p.Projects).First().Projects.Count());
         }         

         // Act.
         using(var dbc = new Context())
         {
            Project project = dbc.Projects.First();
            dbc.Remove(project);
            dbc.SaveChanges();            
         }

         // Assert.
         using(var dbc = new Context())
         {
            Assert.Equal(1, dbc.Users.Count());
            Assert.Equal(1, dbc.Projects.Count());
            Assert.Equal(1, dbc.Users.Include(p => p.Projects).First().Projects.Count());
         }         
      }
   }
}

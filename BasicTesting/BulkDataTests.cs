using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EntityTesting3;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EntityTesting3.BasicTesting
{
   [TestCaseOrderer("EntityTesting3.PriorityOrderer", "EntityTesting3")]
   /*public*/ class BulkDataTests : TestBase
   {
      // In memory create 50k nested collection.
      [Fact, TestPriority(1)]
      public void InMemoryBulkTest()
      {
         Console.WriteLine("Starting InMemoryBulkTest");
         Stopwatch sw = new Stopwatch();
         sw.Start();

         List<Author> authors = new List<Author>();
         List<Book> books = new List<Book>();

         for(int i = 0; i < 1000; ++i)
         {
            Author a = new Author();
            a.Name = "Author" + i;
            authors.Add(a);

            for(int j = 0; j < 50; ++j)
            {
               Book b = new Book();
               b.Author = a;
               b.Title = "Book" + j;
               books.Add(b);
            }
         }

         Console.WriteLine("Completed in: " + sw.Elapsed.TotalMilliseconds + "ms");
         Console.WriteLine();          
      }

      [Fact, TestPriority(2)]
      public void SingleDbContextBulkTest()
      {
         // Arrange.
         ResetDb();

         // Act.
         Console.WriteLine("Starting SingleDbContextBulkTest");
         Stopwatch sw = new Stopwatch();
         sw.Start();

         using(var context = new BasicContext())
         {
            for(int i = 0; i < 100; ++i)
            {
               Author a = new Author();
               a.Name = "Author" + i;
               context.Authors.Add(a);
               context.SaveChanges();

               for(int j = 0; j < 50; ++j)
               {
                  Book b = new Book();
                  b.Author = a;
                  b.Title = "Book" + j;
                  context.Books.Add(b);
                  context.SaveChanges();
               }
            }
         }

         Console.WriteLine("Completed: " + sw.Elapsed.TotalSeconds + "s");         
         Console.WriteLine();

         // Assert.
         Assert.True(BulkTestAsserts());               
      }

      // Bulk data entry. Naive implementation.
      // Insertion of large object graph into DB takes too long to be useful. So this test provides an indication of at
      // which point data insertion starts to slow.
      // Times each entry.
      // Logs the first time.
      // Logs the 10th time.
      // Logs the next time that takes 2x as long as the 10th time and exits.
      [Fact, TestPriority(3)]
      public void SingleDbContextSlowdownTest()
      {
         // Arrange.
         ResetDb();

         // Act.
         Console.WriteLine("Starting SingleDbContextSlowdownTest");
         Stopwatch sw = new Stopwatch();
         TimeSpan cutoff = new TimeSpan(24, 0, 0);  // 24 hrs.

         using(var context = new BasicContext())
         {
            for(int i = 0; i < 1000; ++i)
            {
               sw.Restart();
               Author a = new Author();
               a.Name = "Author" + i;
               context.Authors.Add(a);
               context.SaveChanges();

               for(int j = 0; j < 50; ++j)
               {
                  Book b = new Book();
                  b.Author = a;
                  b.Title = "Book" + j;
                  context.Books.Add(b);
                  context.SaveChanges();
               }

               TimeSpan elapsed = sw.Elapsed;

               if(i == 10)
               {
                  cutoff = new TimeSpan(elapsed.Ticks * 2);
               }

               if(i == 0 || i == 10 || elapsed > cutoff)
               {
                  OutputTime(i, elapsed);
               }

               if(elapsed > cutoff)
               {
                  Console.WriteLine();          
                  break;
               }
            }
         }
      }
      
      [Fact, TestPriority(4)]
      public void SingleDbContextNoTrackingBulkTest()
      {
         // Arrange.
         ResetDb();

         // Act.
         Console.WriteLine("Starting SingleDbContextNoTrackingBulkTest");
         Stopwatch sw = new Stopwatch();
         sw.Start();

         using(var context = new BasicContext())
         {
            context.ChangeTracker.AutoDetectChangesEnabled = false;
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            for(int i = 0; i < 100; ++i)
            {
               Author a = new Author();
               a.Name = "Author" + i;
               context.Authors.Add(a);
               context.SaveChanges();

               for(int j = 0; j < 50; ++j)
               {
                  Book b = new Book();
                  b.Author = a;
                  b.Title = "Book" + j;
                  context.Books.Add(b);
                  context.SaveChanges();
               }
            }
         }

         Console.WriteLine("Completed: " + sw.Elapsed.TotalSeconds + "s");         
         Console.WriteLine();

         // Assert.
         Assert.True(BulkTestAsserts());         
      }
      
      [Fact, TestPriority(5)]
      public void SingleDbContextNoTrackingSlowdownTest()
      {
         // Arrange.
         ResetDb();

         // Act.
         Console.WriteLine("Starting SingleDbContextNoTrackingSlowdownTest");
         Stopwatch sw = new Stopwatch();
         TimeSpan cutoff = new TimeSpan(24, 0, 0);  // 24 hrs.

         using(var context = new BasicContext())
         {
            context.ChangeTracker.AutoDetectChangesEnabled = false;
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            for(int i = 0; i < 1000; ++i)
            {
               sw.Restart();
               Author a = new Author();
               a.Name = "Author" + i;
               context.Authors.Add(a);
               context.SaveChanges();

               for(int j = 0; j < 50; ++j)
               {
                  Book b = new Book();
                  b.Author = a;
                  b.Title = "Book" + j;
                  context.Books.Add(b);
                  context.SaveChanges();
               }

               TimeSpan elapsed = sw.Elapsed;

               if(i == 10)
               {
                  cutoff = new TimeSpan(elapsed.Ticks * 2);
               }

               if(i == 0 || i == 10 || elapsed > cutoff)
               {
                  OutputTime(i, elapsed);
               }

               if(elapsed > cutoff)
               {
                  Console.WriteLine();          
                  break;
               }
            }
         }
      }

      [Fact, TestPriority(6)]
      public void MultipleDbContextBulkTest()
      {
         // Arrange.
         ResetDb();

         // Act.
         Console.WriteLine("Starting MultipleDbContextBulkTest");
         Stopwatch sw = new Stopwatch();
         sw.Start();

         for(int i = 0; i < 100; ++i)
         {
            using(var context = new BasicContext())
            {
               Author a = new Author();
               a.Name = "Author" + i;
               context.Authors.Add(a);
               context.SaveChanges();

               for(int j = 0; j < 50; ++j)
               {
                  Book b = new Book();
                  b.Author = a;
                  b.Title = "Book" + j;
                  context.Books.Add(b);
                  context.SaveChanges();
               }
            }
         }

         Console.WriteLine("Completed: " + sw.Elapsed.TotalSeconds + "s");         
         Console.WriteLine();

         // Assert.
         Assert.True(BulkTestAsserts());              
      }

      [Fact, TestPriority(7)]
      public void MultipleDbContextSlowdownTest()
      {
         // Arrange.
         ResetDb();

         // Act.
         Console.WriteLine("Starting MultipleDbContextCheckSlowdown");
         Stopwatch sw = new Stopwatch();
         TimeSpan cutoff = new TimeSpan(24, 0, 0);  // 24 hrs.

         for(int i = 0; i < 1000; ++i)
         {
            using(var context = new BasicContext())
            {            
               sw.Restart();
               Author a = new Author();
               a.Name = "Author" + i;
               context.Authors.Add(a);
               context.SaveChanges();

               for(int j = 0; j < 50; ++j)
               {
                  Book b = new Book();
                  b.Author = a;
                  b.Title = "Book" + j;
                  context.Books.Add(b);
                  context.SaveChanges();
               }
            }

            TimeSpan elapsed = sw.Elapsed;

            if(i == 10)
            {
               cutoff = new TimeSpan(elapsed.Ticks * 2);
            }

            if(i == 0 || i == 10 || i == 999 || elapsed > cutoff)
            {
               OutputTime(i, elapsed);
            }

            if(elapsed > cutoff)
            {
               break;
            }
         }

         Console.WriteLine();
      }

      [Fact, TestPriority(8)]
      public void NestedDbContextBulkTest()
      {
         // Arrange.
         ResetDb();

         // Act.
         Console.WriteLine("Starting NestedDbContextBulkTest");
         Stopwatch sw = new Stopwatch();
         sw.Start();

         for(int i = 0; i < 100; ++i)
         {
            using(var context = new BasicContext())
            {
               Author a = new Author();
               a.Name = "Author" + i;
               context.Authors.Add(a);
               context.SaveChanges();

               for(int j = 0; j < 50; ++j)
               {
                  using(var context1 = new BasicContext())
                  {                  
                     Book b = new Book();
                     // This gives duplicate key errors.
                     context1.Attach(a);
                     b.Author = a;
                     b.Title = "Book" + j;
                     context1.Books.Add(b);
                     context1.SaveChanges();
                  }
               }
            }
         }

         Console.WriteLine("Completed: " + sw.Elapsed.TotalSeconds + "s");         
         Console.WriteLine();

         // Assert.
         Assert.True(BulkTestAsserts());                      
      }

      [Fact, TestPriority(9)]
      public void NestedDbContextSlowdownTest()
      {
         // Arrange.
         ResetDb();

         // Act.
         Console.WriteLine("Starting NestedDbContextCheckSlowdown");
         Stopwatch sw = new Stopwatch();
         TimeSpan cutoff = new TimeSpan(24, 0, 0);  // 24 hrs.

         for(int i = 0; i < 1000; ++i)
         {
            using(var context = new BasicContext())
            {            
               sw.Restart();
               Author a = new Author();
               a.Name = "Author" + i;
               context.Authors.Add(a);
               context.SaveChanges();

               for(int j = 0; j < 50; ++j)
               {
                  using(var context1 = new BasicContext())
                  {            
                     Book b = new Book();
                     context1.Attach(a);
                     b.Author = a;
                     b.Title = "Book" + j;
                     context1.Books.Add(b);
                     context1.SaveChanges();
                  }
               }
            }

            TimeSpan elapsed = sw.Elapsed;

            if(i == 10)
            {
               cutoff = new TimeSpan(elapsed.Ticks * 2);
            }

            if(i == 0 || i == 10 || i == 999 || elapsed > cutoff)
            {
               OutputTime(i, elapsed);
            }

            if(elapsed > cutoff)
            {
               break;
            }
         }

         Console.WriteLine();
      }

      [Fact, TestPriority(10)]
      public void BatchedBulkTest()
      {
         // Arrange.
         ResetDb();

         // Act.
         Console.WriteLine("Starting BatchedBulkTest");
         Stopwatch sw = new Stopwatch();
         sw.Start();

         List<Author> authors = new List<Author>();
         List<Book> books = new List<Book>();
         for(int i = 0; i < 100; ++i)
         {
            Author a = new Author();
            a.Name = "Author" + i;
            authors.Add(a);

            for(int j = 0; j < 50; ++j)
            {
               Book b = new Book();
               b.Author = a;
               b.Title = "Book" + j;
               books.Add(b);
            }
         }

         using(var context = new BasicContext())
         {
            context.Books.AddRange(books);
            context.SaveChanges();
         }         

         Console.WriteLine("Completed: " + sw.Elapsed.TotalSeconds + "s");         
         Console.WriteLine();

         // Assert.
         Assert.True(BulkTestAsserts());                
      }

      // Bulk data entry. Naive implementation.
      // Insertion of large object graph into DB takes too long to be useful. So this test provides an indication of at
      // which point data insertion starts to slow.
      // Times each entry.
      // Logs the first time.
      // Logs the 10th time.
      // Logs the next time that takes 2x as long as the 10th time and exits.
      [Fact, TestPriority(11)]
      public void BatchedOperationsSlowdownTest()
      {
         // Arrange.
         ResetDb();

         // Act.
         Console.WriteLine("Starting BatchedOperationsSlowdownTest");
         Stopwatch sw = new Stopwatch();
         TimeSpan cutoff = new TimeSpan(24, 0, 0);  // 24 hrs.

         using(var context = new BasicContext())
         {
            for(int i = 0; i < 1000; ++i)
            {
               sw.Restart();
               Author a = new Author();
               a.Name = "Author" + i;
               context.Authors.Add(a);
               context.SaveChanges();

               for(int j = 0; j < 50; ++j)
               {
                  Book b = new Book();
                  b.Author = a;
                  b.Title = "Book" + j;
                  context.Books.Add(b);
                  context.SaveChanges();
               }

               TimeSpan elapsed = sw.Elapsed;

               if(i == 10)
               {
                  cutoff = new TimeSpan(elapsed.Ticks * 2);
               }

               if(i == 0 || i == 10 || elapsed > cutoff)
               {
                  OutputTime(i, elapsed);
               }

               if(elapsed > cutoff)
               {
                  Console.WriteLine();          
                  break;
               }
            }
         }
      }

      [Fact, TestPriority(12)]
      public void BatchedNoTrackingBulkTest()
      {
         // Arrange.
         ResetDb();

         // Act.
         Console.WriteLine("Starting BatchedNoTrackingBulkTest");
         Stopwatch sw = new Stopwatch();
         sw.Start();

         List<Author> authors = new List<Author>();
         List<Book> books = new List<Book>();
         for(int i = 0; i < 100; ++i)
         {
            Author a = new Author();
            a.Name = "Author" + i;
            authors.Add(a);

            for(int j = 0; j < 50; ++j)
            {
               Book b = new Book();
               b.Author = a;
               b.Title = "Book" + j;
               books.Add(b);
            }
         }

         using(var context = new BasicContext())
         {
            context.ChangeTracker.AutoDetectChangesEnabled = false;
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            context.Books.AddRange(books);
            context.SaveChanges();
         }         

         Console.WriteLine("Completed: " + sw.Elapsed.TotalSeconds + "s");         
         Console.WriteLine();

         // Assert.
         Assert.True(BulkTestAsserts());       
      }
      
      private void OutputTime(int cycle, TimeSpan elapsed)
      {
         Console.WriteLine("Cycle: " + cycle + ", Time: " + elapsed.TotalMilliseconds + "ms");
      }

      private bool BulkTestAsserts()
      {
         bool ok = true;
         using(var context = new BasicContext())
         {
            ok = ok && context.Books.Count() == 5000;
            ok = ok && context.Authors.Count() == 100;
         }

         return ok;
      }
   }
}

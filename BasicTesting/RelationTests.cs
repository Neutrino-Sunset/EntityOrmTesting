using System.Linq;
using Xunit;

namespace EntityTesting3.BasicTesting
{
   /*public*/ class RelationTests : TestBase
   {
      [Fact]
      public void CreateOneToOneRelation()
      {
         // Arrange.
         ResetDb();

         // Act.
         using(var context = new BasicContext())
         {
            var author = new Author();
            author.Name = "Bob";
            var address = new Address();
            address.StreetName = "1 Bob St";
            address.Postcode = "1234";
            author.Address = address;

            context.Authors.Add(author);
            context.SaveChanges();            
         }

         // Assert.
         using(var context = new BasicContext())
         {
            Assert.Equal(1, context.Authors.Count());
            Assert.Equal("Bob", context.Authors.First().Name);
            Assert.Equal(1, context.Addresses.Count());
            Assert.Equal("1234", context.Addresses.First().Postcode);
         }
      }
   }
}

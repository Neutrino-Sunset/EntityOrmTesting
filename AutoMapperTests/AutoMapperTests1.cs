using AutoMapper;
using Xunit;

namespace EntityTesting3.AutoMapperTests.AutoMapperTests1
{
   public class Parent
   {
      public string Data1 { get; set; }
      public int Data2 { get; set; }
      public Child Child { get; set; }
   }

   public class Child
   {
      public string Data1 { get; set; }
      public int Data2 { get; set; }
   }

   public class AutoMapperTests1Class
   {
      [Fact]
      public void BasicTest1()
      {
         // Arrange.
         var mc = new MapperConfiguration(
            cfg => cfg.CreateMap<Parent, Parent>()
         );
         var mapper = mc.CreateMapper();

         Parent p1 = new Parent() { Data1 = "Foo", Data2 = 42 };

         // Act.
         Parent p2 = new Parent();
         mapper.Map<Parent, Parent>(p1, p2);

         // Assert.
         Assert.Equal("Foo", p2.Data1);
         Assert.Equal(42, p2.Data2);
         Assert.Null(p2.Child);
      }

      // For an object reference default AutoMapper behaviour is to copy a reference from source.Child to dest.Child.
      // This is rarely useful.
      [Fact]
      public void HierarchyTest1()
      {
         // Arrange.
         var mc = new MapperConfiguration(
            cfg => cfg.CreateMap<Parent, Parent>()
         );
         var mapper = mc.CreateMapper();

         Child c1 = new Child() { Data1 = "Child_Foo", Data2 = 43 };
         Parent source = new Parent() { Data1 = "Foo", Data2 = 42, Child = c1 };

         // Act.
         Parent dest = new Parent();
         dest = mapper.Map<Parent, Parent>(source, dest);

         // Assert.
         Assert.Equal("Foo", dest.Data1);
         Assert.Equal(42, dest.Data2);
         Assert.True(source.Child == dest.Child);
      }

      // For a child object to be mapped just include a mapping for it.
      [Fact]
      public void HierarchyTest2()
      {
         // Arrange.
         var mc = new MapperConfiguration(cfg => {
            cfg.CreateMap<Child, Child>();
            cfg.CreateMap<Parent, Parent>();
         });
         var mapper = mc.CreateMapper();

         Parent source = new Parent() { Data1 = "Foo", Data2 = 42 };
         source.Child = new Child() { Data1 = "Child_Foo", Data2 = 43 };

         // Act.
         Parent dest = new Parent();
         dest.Child = new Child();
         dest = mapper.Map<Parent, Parent>(source, dest);

         // Assert.
         Assert.Equal("Foo", dest.Data1);
         Assert.Equal(42, dest.Data2);
         Assert.False(source.Child == dest.Child);
         Assert.Equal(source.Child.Data1, dest.Child.Data1);
         Assert.Equal(source.Child.Data2, dest.Child.Data2);
      }
   }
}

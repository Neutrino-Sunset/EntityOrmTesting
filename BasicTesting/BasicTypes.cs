using System;
using System.Collections.Generic;

namespace EntityTesting3.BasicTesting
{
   public class Author
   {
      public int AuthorId { get; set; }
      public string Name { get; set; }
      public Address Address { get; set; }
      public ICollection<Book> Books { get; set; } = new List<Book>();
   }

   // One to many relation with Author.
   // No explicit foreign key is required, Entity will identify the principle/dependent side of the relation. Presumably
   // the fact that the navigation property in the principle is a collection makes this obvious.
   public class Book
   {
      public int BookId { get; set; }
      public string Title { get; set; }
      public Author Author { get; set; }
   }

   // One to one relation with Author implemented as nullable reference property.
   // Entity _requires_ the dependent side to declare an explicit foreign key.
   // This enforces a one-to-one constraint on the dependent side, i.e. an Address cannot be added with an invalid
   // AuthorId. However an Author _can_ be created with a null Address, so in fact this only enforces a
   // 'zero or one'-to-one relation instead of a true one-to-one relation.
   // Unfortunately you cannot also add an explicit foreign key to the principle (Author) and force this to be a true
   // one-to-one relation, because then you are back in the position of Entity being unable to identify the dependent.
   public class Address
   {
      public int AddressId { get; set; }
      public string StreetName { get; set; }
      public string Postcode { get; set; }
      public int AuthorId { get; set; }
      public Author Author { get; set; }
   }
}

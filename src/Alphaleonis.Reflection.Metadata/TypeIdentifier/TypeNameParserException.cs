using System;
using System.Linq;

namespace Alphaleonis.Reflection.Metadata
{
   /// <summary>Exception thrown when an invalid type name is parsed by a <see cref="TypeIdentifier"  /></summary>
   [Serializable]
   public class TypeNameParserException : Exception
   {
      /// <summary>Default constructor.</summary>
      public TypeNameParserException() { }

      /// <summary>Constructor.</summary>
      /// <param name="message">The message.</param>
      public TypeNameParserException(string message) : base(message) { }

      /// <summary>Constructor.</summary>
      /// <param name="message">The message.</param>
      /// <param name="inner">The inner exception or <see langword="null"/>.</param>
      public TypeNameParserException(string message, Exception inner) : base(message, inner) { }

      /// <summary>Specialised constructor for use only by derived class.</summary>
      /// <param name="info">The information.</param>
      /// <param name="context">The context.</param>
      protected TypeNameParserException(
       System.Runtime.Serialization.SerializationInfo info,
       System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
   }
}

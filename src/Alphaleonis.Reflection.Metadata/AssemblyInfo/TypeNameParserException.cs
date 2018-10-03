using System;
using System.Linq;

namespace Alphaleonis.Reflection.Metadata
{
   [Serializable]
   public class TypeNameParserException : Exception
   {
      public TypeNameParserException() { }
      public TypeNameParserException(string message) : base(message) { }
      public TypeNameParserException(string message, Exception inner) : base(message, inner) { }
      protected TypeNameParserException(
       System.Runtime.Serialization.SerializationInfo info,
       System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
   }
}

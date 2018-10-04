using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Alphaleonis.Reflection.Metadata
{
   /// <summary>Values that represent the type of a <see cref="TypeSpecifier" /></summary>
   public enum TypeSpecifierKind
   {
      /// <summary>Indicates a pointer specifier (*)</summary>
      Pointer,
      /// <summary>Indicates a reference specifier (&amp;).</summary>
      Reference,

      /// <summary>Indicates an array specifier, e.g. ([], [,,]).</summary>
      Array
   }
}

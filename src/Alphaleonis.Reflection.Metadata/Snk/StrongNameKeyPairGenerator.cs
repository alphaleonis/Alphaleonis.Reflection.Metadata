﻿using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace Alphaleonis.Reflection.Metadata
{
   /// <summary>Utility class that can be used to generate a strong name key pair (.snk) usable for signing an assembly.</summary>
   public static class StrongNameKeyPairGenerator
   {
      /// <summary>Creates strong name key pair.</summary>
      /// <exception cref="ArgumentException">Thrown when one or more arguments have unsupported or
      /// illegal values.</exception>
      /// <param name="keySize">The key size. Must be between 384 and 16384 and be a multiple
      /// of 8.  The default is 1024.</param>
      /// <returns>A new array containing the strong name key pair.</returns>
      public static byte[] CreateStrongNameKeyPair(int keySize = 1024)
      {
         if ((keySize % 8) != 0)
         {
            throw new ArgumentException("Key size must be between 384 and 16384, and be a multiple of 8.", nameof(keySize));
         }

         using (RSACryptoServiceProvider provider = new RSACryptoServiceProvider(keySize, new CspParameters { KeyNumber = 2 }))
         {
            return provider.ExportCspBlob(!provider.PublicOnly);
         }
      }

      /// <summary>Creates strong name key pair.</summary>
      /// <exception cref="ArgumentException">Thrown when one or more arguments have unsupported or
      /// illegal values.</exception>
      /// <param name="filePath">Full path to the file to write the strong name key pair to.</param>
      /// <param name="keySize">(Optional) The key size. Must be between 384 and 16384 and be a multiple
      /// of 8.  The default is 1024.</param>
      public static void CreateStrongNameKeyFile(string filePath, int keySize = 1024)
      {
         var data = CreateStrongNameKeyPair(keySize);
         using (var outputStream = File.Open(filePath, FileMode.Create, FileAccess.Write))
         {
            outputStream.Write(data, 0, data.Length);
         }
      }
   }
}

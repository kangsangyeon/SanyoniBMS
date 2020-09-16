using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

namespace SanyoniLib.SystemHelper
{

    public static class TextHelper
    {

        /// <summary
        /// Get File's Encoding
        /// </summary>
        /// <param name="filename">The path to the file
        public static Encoding GetEncoding(string filename)
        {
            // This is a direct quote from MSDN:
            // The CurrentEncoding value can be different after the first
            // call to any Read method of StreamReader, since encoding
            // autodetection is not done until the first call to a Read method.

            using (var reader = new StreamReader(filename, Encoding.Default, true))
            {
                if (reader.Peek() >= 0) // you need this!
                    reader.Read();

                return reader.CurrentEncoding;
            }
        }

    }

}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace matadd
{
    class Program
    {
        static void Main(string[] args)
        {
            string inPath;
            string outPath;

            StringStream inStream;
            StringBuilder outSb = new StringBuilder(10000);

            int matsOffsetNum;

            int readPosition = 0;
            bool inPartsSection = false;
            bool inMaterialsAttr = false;

            int partsElementMarker = 0;
            int materialsMarker = 0;

            Console.WriteLine("Aircraft Material Offset Tool");
            Console.WriteLine("by hpgbproductions");
            Console.WriteLine();

            Console.WriteLine("Enter input filename:");
            inPath = Console.ReadLine();
            inPath = RemoveQuotes(inPath);
            inStream = new StringStream(File.ReadAllText(inPath));
            Console.WriteLine();

            Console.Write("Enter number of materials to offset: ");
            matsOffsetNum = int.Parse(Console.ReadLine());

            Console.WriteLine("Enter output filename:");
            outPath = Console.ReadLine();
            outPath = RemoveQuotes(outPath);
            
            while (!inPartsSection)
            {
                char c = inStream.PeekChar();
                if (c == '<')
                {
                    partsElementMarker = inStream.pos;
                }
                if (inStream.pos - partsElementMarker == 7 && inStream.str.Substring(partsElementMarker, 7) == "<Parts>")
                {
                    inPartsSection = true;
                }

                c = inStream.ReadChar();
                outSb.Append(c);
            }

            while (inPartsSection)
            {
                char c = inStream.PeekChar();
                if (c == '<')
                {
                    partsElementMarker = inStream.pos;
                }
                if (inStream.pos - partsElementMarker == 8 && inStream.str.Substring(partsElementMarker, 8) == "</Parts>")
                {
                    inPartsSection = false;
                }

                if (c == 'm')
                {
                    materialsMarker = inStream.pos;
                }
                if (inStream.pos - materialsMarker == 11 && inStream.str.Substring(materialsMarker, 11) == "materials=\"")
                {
                    // True when the peeked char is the first number
                    inMaterialsAttr = true;
                }

                if (inMaterialsAttr)
                {
                    // Between the " of the materials attribute, exclusive
                    if (char.IsDigit(c))
                    {
                        int newMaterial = inStream.ReadInt() + matsOffsetNum;
                        outSb.Append(newMaterial);
                    }
                    else if (c == '\"')
                    {
                        inMaterialsAttr = false;
                    }
                    else if (c == '-')
                    {
                        // Don't change negative numbers
                        // Write initial negative sign
                        c = inStream.ReadChar();
                        outSb.Append(c);

                        // Write the digits
                        int material = inStream.ReadInt();
                        outSb.Append(material);
                    }
                    else // comma ','
                    {
                        c = inStream.ReadChar();
                        outSb.Append(c);
                    }
                }

                if (!inMaterialsAttr)
                {
                    c = inStream.ReadChar();
                    outSb.Append(c);
                }
            }

            while (!inStream.EndOfString)
            {
                char c = inStream.ReadChar();
                outSb.Append(c);
            }

            File.WriteAllText(outPath, outSb.ToString());

            Console.WriteLine();
            Console.WriteLine("Complete");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        public class StringStream
        {
            public readonly string str;
            public int pos;

            public StringStream(string s)
            {
                str = s;
                pos = 0;
            }

            /// <summary>
            /// Reads the next character from the string and advances the position by one.
            /// </summary>
            public char ReadChar()
            {
                if (pos < str.Length)
                {
                    char c = str[pos];
                    pos++;
                    return c;
                }
                else
                {
                    return '\0';
                }
            }

            /// <summary>
            /// Reads the following numeric characters and advances the position.
            /// </summary>
            public int ReadInt()
            {
                List<char> numberChars = new List<char>();
                while (char.IsDigit(PeekChar()))
                {
                    numberChars.Add(ReadChar());
                }
                return int.Parse(string.Join("", numberChars));
            }

            /// <summary>
            /// Gets the next character from the string without advancing the position counter.
            /// </summary>
            public char PeekChar()
            {
                if (pos < str.Length)
                {
                    char c = str[pos];
                    return c;
                }
                else
                {
                    return '\0';
                }
            }

            /// <summary>
            /// True if the current position is after the last available index
            /// </summary>
            public bool EndOfString
            {
                get
                {
                    return pos >= str.Length;
                }
            }
        }

        static string RemoveQuotes(string str)
        {
            if (str[0] == '\"' && str[str.Length - 1] == '\"')
            {
                str = str.Substring(1, str.Length - 2);
            }
            return str;
        }
    }
}

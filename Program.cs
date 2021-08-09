using Priority_Queue;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Asm2
{
    class Program
    {
        static int ReadNext(int len, StringReader reader)
        {
            if(reader.Peek() == -1)
            {
                return 0;
            }
            int res = 0;
            int i = 0;
            for(; i < len && reader.Peek() != -1; ++i)
            {
                res <<= 1;
                res |= (reader.Read() - '0');
            }
            return res << (len - i);
        }

        static string HuffmanDecode(string[] lines)
        {
            int n = int.Parse(lines[0]);

            var heap = new SimplePriorityQueue<KeyValuePair<int, string>, int>();
            var len = 0;

            //find max code length L
            for(int i = 1; i <= n; ++i)
            {
                var line = lines[i];
                string[] items = line.Split(" ");
                var codelen = items[1].Length;
                if(codelen > len)
                {
                    len = codelen;
                }
            }

            //sort table by code, ascending
            for (int i = 1; i <= n; ++i)
            {
                var line = lines[i];
                string[] items = line.Split(null);
                heap.Enqueue(new KeyValuePair<int, string>(char.Parse(items[0]), items[1]), Convert.ToInt32(items[1].PadRight(len, '0'), 2));
            }

            var entryCount = 1 << len;

            var HuffDec = new int[entryCount, 2];

            int j = 0;

            //build table
            while(heap.Count > 0)
            {
                var entry = heap.First;
                var symbol = entry.Key;
                var code = heap.GetPriority(entry);
                var codelen = entry.Value.Length;

                heap.Dequeue();

                int k = 0;

                do
                {
                    HuffDec[k + j, 0] = symbol;
                    HuffDec[k + j, 1] = codelen;
                    ++k;
                } while (heap.Count > 0 && heap.GetPriority(heap.First) > code + k);

                j += k;

            }
            //build table fill remaining entries
            while(j < entryCount)
            {
                HuffDec[j, 0] = HuffDec[j - 1, 0];
                HuffDec[j, 1] = HuffDec[j - 1, 1];
                ++j;
            }

            var encoded = lines[lines.Length - 1];

            var mask = (1 << len) - 1;

            string content = "";
            //use table to decode the content
            using (var reader = new StringReader(encoded))
            {
                int x = ReadNext(len, reader);
                int bitwrote = 0;
                while(bitwrote < encoded.Length)
                {
                    content += (char)HuffDec[x, 0];
                    var codelen = HuffDec[x, 1];
                    bitwrote += codelen;
                    x <<= codelen;
                    var nextbits = ReadNext(codelen, reader);
                    x |= nextbits;
                    x &= mask;
                }

            }

            return content;
        }

        static string LZWEncode(string[] lines, out string dictionary)
        {
            var content = lines[0];

            int n = int.Parse(lines[1]);

            var table = new Dictionary<string, int>();
            
            int i = 0;

            for (; i < n; ++i)
            {
                var line = lines[i + 2];
                string[] items = line.Split(null);
                table.Add(items[1], i);
            }

            var result = "";

            using(var reader = new StringReader(content))
            {
                string s = "" + (char)reader.Read();

                while(reader.Peek() != -1)
                {
                    char c = (char)reader.Read();

                    if (table.ContainsKey(s + c))
                    {
                        s += c;
                    }
                    else
                    {
                        result += table[s];
                        table[s + c] = i++;
                        s = "" + c;
                    }

                }
                result += table[s];
            }

            dictionary = "";
            foreach (var entry in table)
            {
                dictionary += $"{entry.Value} : {entry.Key}\n";
            }
            return result;
        }

        static List<string[]> FileGetTestCases(string filename, bool isLZW)
        {
            string[] lines = File.ReadAllLines(filename);
            
            List<string[]> testCases = new List<string[]>();
            int casei = 0;
            if (isLZW)
            {
                for(int i = 0; i < lines.Length;)
                {
                    var content = lines[i++];
                    var linenum = int.Parse(lines[i]);
                    testCases.Add(new string[linenum + 2]);
                    testCases[casei][0] = content;
                    testCases[casei][1] = lines[i++];
                    for (int j = 0; j < linenum; ++j, ++i)
                    {
                        testCases[casei][j + 2] = Regex.Replace(lines[i], @"\s+", " ");
                    }
                    ++casei;
                }
            }
            else
            {
                for (int i = 0; i < lines.Length;)
                {
                    var linenum = int.Parse(lines[i]);
                    testCases.Add(new string[linenum + 2]);
                    testCases[casei][0] = lines[i++];
                    int j = 0;
                    for (; j < linenum; ++j, ++i)
                    {
                        testCases[casei][j + 1] = Regex.Replace(lines[i], @"\s+", " ");
                    }
                    testCases[casei][j + 1] = lines[i++];
                    ++casei;
                }
            }
            return testCases;
        }

        static void Main(string[] args)
        {
            var method = args[0].Trim().ToLowerInvariant();
            var filename = args[1];
            
            Console.WriteLine($"Processing file: {filename}...");
            if(method == "huffman")
            {
                var cases = FileGetTestCases(filename, false);
                int i = 1;
                foreach (var c in cases)
                {
                    Console.WriteLine($"Test case: {i}");
                    var content = HuffmanDecode(c);
                    Console.WriteLine("Decoded content:");
                    Console.WriteLine(content);
                    ++i;
                }
            } 
            else if (method == "lzw")
            {
                var cases = FileGetTestCases(filename, true);
                int i = 1;

                foreach(var c in cases)
                {
                    Console.WriteLine($"Test case: {i}");
                    string dictionary;
                    var content = LZWEncode(c, out dictionary);
                    Console.WriteLine("Output Sequence:");
                    Console.WriteLine(content + "\n");
                    Console.WriteLine("Dictionary:\n");
                    Console.WriteLine(dictionary);
                    ++i;
                }
            }
            
            
        }
    }
}

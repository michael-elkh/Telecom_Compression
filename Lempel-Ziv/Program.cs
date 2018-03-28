using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lempel_Ziv
{
	class Program
	{
		static Byte[] Get_Bytes_From_String(String input)
		{
			Byte[] Table = new Byte[input.Length];
			for (int i = 0; i < input.Length; i++)
			{
				Table[i] = Convert.ToByte(input[i]);
			}
			return Table;
		}
		static List<Int32> LZW_Compress(String input)
		{
			//https://www.geeksforgeeks.org/lzw-lempel-ziv-welch-compression-technique/
			List<int> output = new List<int>();
			Dictionary<String, Int32> Table = new Dictionary<string, int>();
			for (int i = 0; i < 256; i++)
			{
				Table.Add(((Char)i).ToString(), i);
			}

			string p = input[0].ToString();
			Char c;
			string concat;
			int cmpt = 256;
			for (int i = 1; i < input.Length; i++)
			{
				c = input[i];
				concat = p + c.ToString();
				if (Table.ContainsKey(concat))
				{
					p = concat;
				}else
				{
					output.Add(Table[p]);
					Table.Add(concat, cmpt);
					cmpt++;
					p = c.ToString();
				}
			}
			output.Add(Table[p]);

			return output;
		}
		static string LZW_Uncompress(List<Int32> input)
		{
			Dictionary<Int32,String> Table = new Dictionary<int, String>();
			int i;
			for (i = 0; i < 256; i++)
			{
				Table.Add(i,((Char)i).ToString());
			}

			string output;

			int old = input.First();
			int next;
			string s, c = String.Empty;
			input.Remove(input.First());
			output = Table[old];
			while (input.Count > 0)
			{
				next = input.First();
				input.Remove(input.First());
				if(!Table.ContainsKey(next))
				{
					s = Table[old];
					s += c;
				}else
				{
					s = Table[next];
				}
				output += s;
				c = s[0].ToString();
				Table.Add(i, Table[old] + c);
				i++;
				old = next;
			}
			return output;
		}
		static void Main(string[] args)
		{
			LZW_Compress("BABAABAAA");
			Console.WriteLine(	LZW_Uncompress(LZW_Compress("BABAABAAA")));
		}
	}
}

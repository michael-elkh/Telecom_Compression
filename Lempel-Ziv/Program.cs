using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Lempel_Ziv
{
	class Program
	{
		const String FILE = "C:\\Users\\micha\\Desktop\\big.txt.lzw";
		static Byte[] Get_Bytes_From_String(String input)
		{
			Byte[] Table = new Byte[input.Length];
			for (int i = 0; i < input.Length; i++)
			{
				Table[i] = Convert.ToByte(input[i]);
			}
			return Table;
		}
		static KeyValuePair<int,List<Int32>> LZW_Compress(String input)
		{
			//https://www.geeksforgeeks.org/lzw-lempel-ziv-welch-compression-technique/
			List<int> output = new List<int>();
			FileStream file = new FileStream(FILE,FileMode.Open, FileAccess.Read);
			Dictionary<String, Int32> Table = new Dictionary<string, int>();
			for (int i = 0; i < 256; i++)
			{
				Table.Add(((Char)i).ToString(), i);
			}

			string previous = ((Char)file.ReadByte()).ToString();
			Char current;
			string concat;
			int cmpt = 255;
			for (int i = 1; i < file.Length; i++)
			{
				current = (Char)file.ReadByte();
				concat = previous + current.ToString();
				if (Table.ContainsKey(concat))
				{
					previous = concat;
				}else
				{
					output.Add(Table[previous]);
					cmpt++;
					Table.Add(concat, cmpt);
					previous = current.ToString();
				}
			}
			output.Add(Table[previous]);
			KeyValuePair<int, List<Int32>> res;
			res = new KeyValuePair<int, List<int>>((int)Math.Floor(Math.Log(output.Max(), 2)) + 1, output);
			file.Close();
			return res;
		}
		static List<Boolean> Get_List(int Value, int Nb_Byte = 8)
		{
			List<Boolean> result = new List<Boolean>(new Boolean[Nb_Byte]);

			//Récupération de la valeur bit par bit, via décalage.
			for (int i = Nb_Byte - 1; i >= 0; i--)
			{
				result[i] = (Value % 2 == 1);
				Value >>= 1; //Décalage d'un bit vers la gauche.
			}
			return result;
		}
		static Byte Get_Byte(List<Boolean> Boolean_List)
		{
			Byte result = 0;
			Int32 i = Boolean_List.Count - 1;
			foreach (Boolean item in Boolean_List)
			{
				//Somme les puissances de 2.
				result += (Byte)(item ? (Math.Pow(2, i)) : 0);
				i--;
			}

			return result;
		}
		static void Write(KeyValuePair<int, List<int>> input)
		{
			FileStream file = new FileStream(FILE+".lzw",FileMode.OpenOrCreate,FileAccess.Write);
			List<Boolean> buffer = new List<bool>();
			foreach (int element in input.Value)
			{
				buffer.AddRange(Get_List(element, input.Key));
				while(buffer.Count >= 8)
				{
					file.WriteByte(Get_Byte(buffer.GetRange(0,8)));
					buffer.RemoveRange(0, 8);
				}
			}
			file.Close();
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
			FileStream f = new FileStream(FILE + ".test", FileMode.OpenOrCreate, FileAccess.Write);
			for (int i = 0; i < 256; i++)
			{
				f.WriteByte((Byte)i);
			}
			f.Close();
			KeyValuePair<int, List<int>> compressed = LZW_Compress("BABAABAAA");
			GC.Collect();
			Write(compressed);
			//Console.WriteLine(LZW_Uncompress(LZW_Compress("BABAABAAA").Value));
		}
	}
}

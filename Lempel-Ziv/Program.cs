using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace Lempel_Ziv
{
	class Program
	{
		const String FILE = "C:\\Users\\micha\\Desktop\\Test_Files\\TheSonnets.txt";
		static Int32 Get_NbBits(Int32 value)
		{
			return (int)Math.Log(value, 2) + 1;
		}
		static List<Boolean> Get_List(int Value, int Nb_Bits = -1)
		{
			//Si le nombre bits n'est pas précisé, je le fixe au minimum.
			Nb_Bits = Nb_Bits < 0 ? (int)Math.Log(Value, 2) + 1 : Nb_Bits;
			List<Boolean> result = new List<Boolean>(new Boolean[Nb_Bits]);

			//Récupération de la valeur bit par bit, via décalage.
			for (int i = Nb_Bits - 1; i >= 0; i--)
			{
				result[i] = Value % 2 == 1;
				Value >>= 1; //Décalage d'un bit vers la gauche.
			}
			return result;
		}
		static Byte Get_Byte(List<Boolean> Boolean_List)
		{
			return (Byte)Get_Int(Boolean_List);
		}
		static Int32 Get_Int(List<Boolean> Boolean_List)
		{
			Int32 result = 0;
			Int32 i = Boolean_List.Count - 1;
			foreach (Boolean item in Boolean_List)
			{
				//Somme les puissances de 2.
				result += item ? (Int32)(Math.Pow(2, i)) : 0;
				i--;
			}

			return result;
		}
		static List<Int32> Read(String Path)
		{
			Byte[] input = File.ReadAllBytes(Path);
			Int32 nbbits = input[0];
			List<Int32> data = new List<int>();
			List<Boolean> buffer = new List<bool>();
			for (int i = 1; i < input.Length; i++)
			{
				buffer.AddRange(Get_List(input[i], 8));
				while (buffer.Count >= nbbits)
				{
					int value = Get_Int(buffer.GetRange(0, nbbits));
					buffer.RemoveRange(0, nbbits);
					if (value == 0)
					{
						nbbits++;
					}
					else
					{
						data.Add(value);
					}
				}
			}

			return data;
		}
		static void Write(String Path, List<int> input)
		{
			List<Byte> output = new List<Byte>();
			List<Boolean> buffer = new List<bool>();
			int current = Get_NbBits(input[0]);
			buffer.AddRange(Get_List(current, 8));
			for (int i = 0; i < input.Count; i++)
			{
				while(Get_NbBits(input[i]) > current)
				{
					//Séparateur
					buffer.AddRange(Get_List(0, current));

					current++;
				}
				buffer.AddRange(Get_List(input[i], current));
				while (buffer.Count >= 8)
				{
					output.Add(Get_Byte(buffer.GetRange(0, 8)));
					buffer.RemoveRange(0, 8);
				}
			}

			if (buffer.Count > 0)
			{
				while (buffer.Count < 8)
				{
					buffer.Add(false);
				}
				output.Add(Get_Byte(buffer.GetRange(0, 8)));
				buffer.RemoveRange(0, 8);
			}
			File.WriteAllBytes(Path, output.ToArray());
		}
		static List<Int32> LZW_Compress(String Path)
		{
			//https://www.geeksforgeeks.org/lzw-lempel-ziv-welch-compression-technique/
			List<int> output = new List<int>();
			Byte[] input = File.ReadAllBytes(Path);
			Dictionary<String, Int32> Table = new Dictionary<string, int>();
			for (int i = 1; i <= 256; i++)
			{
				Table.Add(((Char)(i-1)).ToString(), i);
			}

			string previous = ((Char)input[0]).ToString();
			Char current;
			string concat;
			int cmpt = 256;
			for (int i = 1; i < input.Length; i++)
			{
				current = (Char)input[i];
				concat = previous + current.ToString();
				if (Table.ContainsKey(concat))
				{
					previous = concat;
				}
				else
				{
					output.Add(Table[previous]);
					cmpt++;
					Table.Add(concat, cmpt);
					previous = current.ToString();
				}
			}
			output.Add(Table[previous]);
			return output;
		}
		static void LZW_Uncompress(List<Int32> input, String Path)
		{
			List<Char> output = new List<Char>();
			Dictionary<Int32, String> Table = new Dictionary<int, String>();
			int i;
			for (i = 1; i <= 256; i++)
			{
				Table.Add(i, ((Char)(i-1)).ToString());
			}

			int old = input.First();
			int next;
			string s, c = String.Empty;
			input.Remove(input.First());
			output.AddRange(Table[old]);
			while (input.Count > 0)
			{
				next = input.First();
				input.Remove(input.First());
				if (!Table.ContainsKey(next))
				{
					s = Table[old];
					s += c;
				}
				else
				{
					s = Table[next];
				}
				output.AddRange(s);
				c = s[0].ToString();
				Table.Add(i, Table[old] + c);
				i++;
				old = next;
			}

			File.WriteAllBytes(Path, Encoding.ASCII.GetBytes(output.ToArray()));
		}
		static void Analyse(List<int> Data)
		{
			List<int>[] bench = new List<int>[Get_NbBits(Data.Count)];
			for (int i = 0; i < bench.Length; i++)
			{
				bench[i] = new List<int>();
			}
			int pos = 0;
			int value = 0;
			for (int i = 0; i < Data.Count; i++)
			{
				value = Math.Abs(Data[i]);
				pos = (int)Math.Log(value, 2);
				bench[pos].Add(value);
			}
			Double total = 0;
			Double current = 0;
			Console.WriteLine("Total : " + Data.Count.ToString("N0") + " - " + Math.Log(Data.Count, 2));
			for (int i = 0; i < bench.Length; i++)
			{
				current = Math.Round((Double)bench[i].Count * 100 / Data.Count, 2);
				total += current;
				Console.WriteLine("Nb bits : " + (i + 1) + " - Pourcentage : " + current);
			}
			Console.WriteLine("Total : " + total);
		}
		static void Main(string[] args)
		{
			List<int> compressed = LZW_Compress(FILE);
			Write(FILE + ".lzw", compressed);
			compressed.Clear();

			GC.Collect();
			List<Int32> decompressed = Read(FILE + ".lzw");
			LZW_Uncompress(decompressed, FILE.Substring(0, FILE.Length - 4) + ".original");
		}
	}
}
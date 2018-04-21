﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace Lempel_Ziv
{
	class Program
	{
		const String FILE = "C:\\Users\\micha\\Desktop\\Test_Files\\TheSonnets.txt";
		static Byte[] Get_Bytes_From_String(String input)
		{
			Byte[] Table = new Byte[input.Length];
			for (int i = 0; i < input.Length; i++)
			{
				Table[i] = Convert.ToByte(input[i]);
			}
			return Table;
		}
		static KeyValuePair<int, List<Int32>> LZW_Compress(String File)
		{
			//https://www.geeksforgeeks.org/lzw-lempel-ziv-welch-compression-technique/
			List<int> output = new List<int>();
			FileStream file = new FileStream(File, FileMode.Open, FileAccess.Read);
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
			KeyValuePair<int, List<Int32>> res;
			res = new KeyValuePair<int, List<int>>((int)Math.Log(output.Max(), 2) + 1, output);
			file.Close();
			return res;
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
		static void Write(String File, KeyValuePair<int, List<int>> input)
		{
			FileStream file = new FileStream(File + ".lzw", FileMode.OpenOrCreate, FileAccess.Write);
			List<Boolean> buffer = new List<bool>();
			buffer.AddRange(Get_List(input.Key, 8));
			foreach (int element in input.Value)
			{
				buffer.AddRange(Get_List(element, input.Key));
				while (buffer.Count >= 8)
				{
					file.WriteByte(Get_Byte(buffer.GetRange(0, 8)));
					buffer.RemoveRange(0, 8);
				}
			}

			if(buffer.Count > 0)
			{
				while(buffer.Count < 8)
				{
					buffer.Add(false);
				}
				file.WriteByte(Get_Byte(buffer.GetRange(0, 8)));
				buffer.RemoveRange(0, 8);
			}
			file.Close();
		}
		static List<Int32> Read(String File)
		{
			FileStream filereading = new FileStream(File, FileMode.Open, FileAccess.Read);
			Int32 nbbits = (Int32)filereading.ReadByte();
			List<Int32> input = new List<int>();
			List<Boolean> buffer = new List<bool>();
			for (int i = 1; i < filereading.Length; i++)
			{
				buffer.AddRange(Get_List(filereading.ReadByte(), 8));
				while(buffer.Count >= nbbits)
				{
					input.Add(Get_Int(buffer.GetRange(0, nbbits)));
					buffer.RemoveRange(0, nbbits);
				}
			}

			return input;
		}
		static void LZW_Uncompress(List<Int32> input, String File)
		{
			FileStream output = new FileStream(File, FileMode.OpenOrCreate, FileAccess.Write);
			Dictionary<Int32, String> Table = new Dictionary<int, String>();
			int i;
			for (i = 0; i < 256; i++)
			{
				Table.Add(i, ((Char)i).ToString());
			}

			string buffer;

			int old = input.First();
			int next;
			string s, c = String.Empty;
			input.Remove(input.First());
			buffer = Table[old];
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
				buffer += s;
				c = s[0].ToString();
				Table.Add(i, Table[old] + c);
				i++;
				old = next;

				if(buffer.Length >= 65536)
				{
					output.Write(Encoding.ASCII.GetBytes(buffer.ToCharArray(),0, 65536),0,65536);
					buffer = buffer.Substring(65536);
				}
			}

			output.Write(Encoding.ASCII.GetBytes(buffer.ToCharArray(), 0, buffer.Length), 0, buffer.Length);
		}
		static void Main(string[] args)
		{
			KeyValuePair<int, List<int>> compressed = LZW_Compress(FILE);
			GC.Collect();
			Write(FILE, compressed);
			List<Int32> decompressed = Read(FILE + ".lzw");
			LZW_Uncompress(decompressed, FILE.Substring(0, FILE.Length - 4) + ".original");


			//Console.WriteLine(LZW_Uncompress(LZW_Compress("BABAABAAA").Value));
		}
	}
}
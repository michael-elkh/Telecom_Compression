using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCorrecteur
{
	class Program
	{
		/// <summary>
		/// Cette focntion permet de calculer le nombre de bits nécessaire pour stocker l'information
		/// </summary>
		/// <param name="value"> La valeur à analyser</param>
		/// <returns>nombre de bits necessaire</returns>
		static Int32 Get_NbBits(Int32 value)
		{
			return (int)Math.Log(value, 2) + 1;
		}
		/// <summary>
		/// Cette fonction donne le code binaire d'un nombre
		/// </summary>
		/// <param name="Value">nombre de bits nécessaire</param>
		/// <param name="Nb_Bits">taille du codage</param>
		/// <returns>^retourne une liste de bits</returns>
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
		/// <summary>
		/// Cette fonction transforme une liste de booléen en octets
		/// </summary>
		/// <param name="Boolean_List">Liste de bits</param>
		/// <returns>Retourne un octet correspondant</returns>
		static Byte Get_Byte(List<Boolean> Boolean_List)
		{
			return (Byte)Get_Int(Boolean_List);
		}
		/// <summary>
		/// Cette fonction transforme une liste de booléen en liste d'entiers
		/// </summary>
		/// <param name="Boolean_List">Liste de booléen</param>
		/// <returns>Retourne la valeur associée</returns>
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
		static List<Boolean> Bits_From_Bytes(List<Byte> Input)
		{
			List<Boolean> output = new List<bool>();
			for (int i = 0; i < Input.Count; i++)
			{
				output.AddRange(Get_List(Input[i],8));
			}
			return output;
		}
		static List<Byte> Bytes_From_Bits(List<Boolean> Input)
		{
			List<Byte> output = new List<Byte>();
			for (int i = 0; i < Input.Count; i+=8)
			{
				output.Add(Get_Byte(Input.GetRange(i, 8)));
			}
			return output;
		}
		static Boolean[,] Matrix_From_List(List<Boolean> Input, Int32 rows = 256, Int32 columns = 256)
		{
			if(Input.Count != rows*columns)
			{
				throw new Exception();
			}
			Boolean[,] matrix = new Boolean[rows, columns];
			Int32 nbItems = Input.Count;
			for (int i = 0; i < nbItems; i++)
			{
				matrix[i / columns, i % columns] = Input[i];
			}
			return matrix;
		}	
		static List<Boolean> List_From_Matrix(Boolean[,] Input)
		{
			List<Boolean> output = new List<Boolean>();
			for (int i = 0; i < Input.GetLength(0); i++)
			{
				for (int j = 0; j < Input.GetLength(1); j++)
				{
					output.Add(Input[i, j]);
				}
			}
			return output;
		}
		static Boolean[,] Compute_Parity(Boolean[,] Input)
		{
			Int32 rows = Input.GetLength(0) + 1;
			Int32 columns = Input.GetLength(1) + 1;
			Boolean[,] output = new Boolean[rows, columns];

			Boolean ParityY = false;
			Boolean[] ParityX = new Boolean[columns-1];
			for (int i = 0; i < rows; i++)
			{
				if (i != rows - 1)
				{
					for (int j = 0; j < columns; j++)
					{
						if (j != columns - 1)
						{
							output[i, j] = Input[i, j];
							ParityX[j] = output[i, j] ^ ParityX[j];
							ParityY = output[i, j] ^ ParityY;
						}
						else
						{
							output[i, j] = ParityY;
						}
					}
				}
				else
				{
					for (int j = 0; j < ParityX.Length; j++)
					{
						output[i, j] = ParityX[j];
					}
				}
			}
			return output;
		}
		static void Main(string[] args)
		{
			int i = 0;
		}
	}
}
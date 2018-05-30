using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace Lempel_Ziv
{
    class Program
    {
        /// <summary>
		/// Formatte une valeur flotante en chaîne de caractères.
		/// </summary>
		/// <param name="Value">Valeur à formatter</param>
		/// <returns>Valeur formattée</returns>
        static String Double_ToString(Double Value)
        {
            if ((0.001 <= Value && Value < 1000) || Value == 0)
            {
                return Math.Round(Value, 3).ToString();
            }
            else
            {
                return Value.ToString("0.000e0");
            }
        }
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
        /// <summary>
        /// Cette fonction lis un fichier encodé en LZW
        /// </summary>
        /// <param name="Path">Chemin du fichier</param>
        /// <returns>Retourne une liste d'index de dictionnaire</returns>
        static List<Int32> Read(String Path)
        {
            Byte[] input = File.ReadAllBytes(Path);
            Int32 nbbits = input[0]; //Recupère la taille du premier index
            List<Int32> data = new List<int>();
            List<Boolean> buffer = new List<bool>();
            for (int i = 1; i < input.Length; i++)
            {
                buffer.AddRange(Get_List(input[i], 8)); //Transforme un octet en liste de bits
                                                        //Si la taille du buffer de bits est égale nbbits de l'index en cours
                while (buffer.Count >= nbbits)
                {
                    int value = Get_Int(buffer.GetRange(0, nbbits)); //Récupère l'index
                    buffer.RemoveRange(0, nbbits); //Vide la partie déja convertie du buffer
                    if (value == 0)
                    {
                        nbbits++;//Si la valeur est 0 alors le nombre de bits pour les index suivants augmente
                    }
                    else
                    {
                        data.Add(value);
                    }
                }
            }

            return data;
        }
        /// <summary>
        /// Cette fonction écrit un fichier encodé avec LZW
        /// </summary>
        /// <param name="Path">Chemin du fichier</param>
        /// <param name="input">Liste des index du dictionnaire LZW</param>
        static void Write(String Path, List<int> input)
        {
            List<Byte> output = new List<Byte>();
            List<Boolean> buffer = new List<bool>();
            int current = Get_NbBits(input[0]);
            buffer.AddRange(Get_List(current, 8));//Stocke la taille du premier index
            for (int i = 0; i < input.Count; i++)
            {
                while (Get_NbBits(input[i]) > current)
                {
                    //Séparateur
                    buffer.AddRange(Get_List(0, current));
                    current++;
                }
                buffer.AddRange(Get_List(input[i], current));
                //Groupe les bits par 8 pour les transformer en octets
                while (buffer.Count >= 8)
                {
                    output.Add(Get_Byte(buffer.GetRange(0, 8)));
                    buffer.RemoveRange(0, 8);
                }
            }

            //Complète le dernier octet, avec des zéros
            if (buffer.Count > 0)
            {
                while (buffer.Count < 8)
                {
                    buffer.Add(false);
                }
                output.Add(Get_Byte(buffer.GetRange(0, 8)));
                buffer.RemoveRange(0, 8);
            }
            //Écrit dans le fichier
            File.WriteAllBytes(Path, output.ToArray());
        }
        /// <summary>
        /// Cette fonction compresse le fichier en LZW
        /// </summary>
        /// <param name="Path">Chemin du fichier</param>
        /// <returns>Retourne la liste des index du dictionnaire LZW</returns>
        static List<Int32> LZW_Compress(String Path)
        {
            //https://www.geeksforgeeks.org/lzw-lempel-ziv-welch-compression-technique/
            List<int> output = new List<int>();
            Byte[] input = File.ReadAllBytes(Path);
            Byte[] convBuff = new Byte[1];
            Dictionary<String, Int32> Table = new Dictionary<string, int>();
            //Rempli le dictionnaire LZW par les premiers 256 octets
            for (int i = 1; i <= 256; i++)
            {
                //Ces deux lignes de code permettent de traiter n'importe quel type de données, en évitant des problèmes d'encodage.
                convBuff[0] = (Byte)(i-1);
                Table.Add(Encoding.GetEncoding(1252).GetString(convBuff), i);
            }

            convBuff[0] = (Byte)input[0];
            String previous = Encoding.GetEncoding(1252).GetString(convBuff);
            Char current;
            string concat;
            int cmpt = 256;//Index courant dans la table
            for (int i = 1; i < input.Length; i++)
            {
                //Ces deux lignes de code permettent de traiter n'importe quel type de données, en évitant des problèmes d'encodage.
                convBuff[0] = (Byte)input[i];
                current = Encoding.GetEncoding(1252).GetChars(convBuff)[0]; //Récupère le premier caractère
                concat = previous + current.ToString();//Concatène avec le chaîne précédente
                if (Table.ContainsKey(concat))//Si la concaténation existe 
                {
                    previous = concat;//La concaténation devient la chaîne précdente
                }
                else
                {
                    output.Add(Table[previous]); //Ajoute l'index de l'élément précédent à liste des index
                    cmpt++;
                    Table.Add(concat, cmpt); //Rajoute la concaténation à la table
                    previous = current.ToString();
                }
            }
            output.Add(Table[previous]); //Ajoute le dernier élément dans la liste des index
            return output;
        }
        /// <summary>
        /// Cette fonction décompresse un fichier encodé en LZW
        /// </summary>
        /// <param name="input">Liste des index du dictionnaire LZW</param>
        /// <param name="Path">Chemin du fichier</param>
        static void LZW_Uncompress(List<Int32> input, String Path)
        {
            List<Char> output = new List<Char>();
            Dictionary<Int32, String> Table = new Dictionary<int, String>();
            Byte[] convBuff = new Byte[1];
            int cmpt = 1;//Index courant dans la table
            for (; cmpt <= 256; cmpt++) //Rempli le dictionnaire LZW par les premiers 256 octets
            {
                //Ces deux lignes de code permettent de traiter n'importe quel type de données, en évitant des problèmes d'encodage.
                convBuff[0] = (Byte)(cmpt - 1);
                Table.Add(cmpt, Encoding.GetEncoding(1252).GetString(convBuff));
            }

            int prevIndex = input.First();
            int currIndex;
            string previous = String.Empty;
            char currChar = '\0';
            input.Remove(input.First());
            output.AddRange(Table[prevIndex]);
            while (input.Count > 0)
            {
                currIndex = input.First();//Avance la position
                input.Remove(input.First());
                if (!Table.ContainsKey(currIndex)) //Si la table ne contient pas l'index courrant
                {
                    previous = Table[prevIndex]; //Récupère la chaîne de caractères précédente 
                    previous += currChar; //Concatène le caractère courant à la chaîne précédente
                }
                else
                {
                    previous = Table[currIndex]; //Déplace la fenêtre
                }
                output.AddRange(previous);//Ajoute l'élément décodé à la sortie
                currChar = previous[0];
                Table.Add(cmpt, Table[prevIndex] + currChar);//Ajoute la concaténation à la table
                cmpt++;
                prevIndex = currIndex;
            }

            //Écrit le fichier décodé (Encodage : Windows-1252)
            File.WriteAllBytes(Path, Encoding.GetEncoding(1252).GetBytes(output.ToArray()));
        }
        static void Main(string[] args)
        {
            String FilePath = "";
            Boolean loop = true;
            while (loop)
            {
                Console.Write("Veuillez entrer un nom de fichier : ");
                FilePath = Console.ReadLine();
                if (!File.Exists(FilePath))
                {
                    Console.WriteLine("Erreur : Ce fichier n'existe pas.");
                }
                else
                {
                    loop = false;
                }
            }

            List<int> compressed_list = LZW_Compress(FilePath);
            Write(FilePath + ".lzw", compressed_list);
            compressed_list.Clear();
            GC.Collect();

            Console.WriteLine("Le fichier a été compréssé, nous allons maintenant le décompresser.");
            FileInfo original = new FileInfo(FilePath);
            FileInfo compressed = new FileInfo(FilePath + ".lzw");
            Console.WriteLine("Taux de compression effectif : " + Double_ToString((Double)(compressed.Length) / original.Length * 100) + "%");

            List<Int32> decompressed = Read(FilePath + ".lzw");
            LZW_Uncompress(decompressed, FilePath.Substring(0, FilePath.Length - 4) + ".original");
            Console.WriteLine("Le fichier a été décompressé, chemin du fichier décompressé : " + FilePath.Substring(0, FilePath.Length - original.Extension.Length) + ".original");
            Console.ReadKey();
        }
    }
}
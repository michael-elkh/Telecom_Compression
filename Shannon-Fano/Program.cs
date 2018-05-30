using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace ShannonFano
{
    class Program
    {
        /// <summary>
        /// Permet d'associé une probabilité et un encodage en vue d'un stockage dans un dictionnaire.
        /// </summary>
        class Shannon_Element
        {
            public Double Probability;
            public List<Boolean> Encoding;

            /// <summary>
            /// Constructeur de l'objet.
            /// </summary>
            /// <param name="Prob">Probabilité d'apparition</param>
            public Shannon_Element(Double Prob)
            {
                Encoding = new List<Boolean>();
                this.Probability = Prob;
            }
            /// <summary>
            /// Cette fonction retourne la valeur de l'objet sous la forme d'une chaîne de caractères.
            /// </summary>
            /// <returns>Chaîne de caractères "Probabilité : Codage binaire"</returns>
            public override String ToString()
            {
                return Double_ToString(Probability) + " : " + List_ToString(Encoding);
            }
        }
        /// <summary>
        /// Transforme une liste de bits en chaîne de caractères.
        /// </summary>
        /// <param name="Boolean_List">Liste de bits</param>
        /// <returns>Chaine de la forme "01010101"</returns>
        static String List_ToString(IEnumerable<Boolean> Boolean_List)
        {
            String buffer = "";
            foreach (Boolean bit in Boolean_List)
            {
                buffer += bit ? "1" : "0";
            }
            return buffer;
        }
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
        /// Transforme une liste de bits en entier non-signé sur 8 bits.
        /// </summary>
        /// <param name="Boolean_List">Liste de bits</param>
        /// <returns>Valeur en octet</returns>
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
        /// <summary>
        /// Transforme un entier non-signé sur 8 bits en une liste de bits.
        /// </summary>
        /// <param name="Value">Valeur en octet</param>
        /// <returns>Liste de bits</returns>
        static List<Boolean> Get_List(Byte Value)
        {
            List<Boolean> result = new List<Boolean>(new Boolean[8]);

            //Récupération de la valeur bit par bit, via décalage.
            for (int i = 7; i >= 0; i--)
            {
                result[i] = (Value % 2 == 1);
                Value >>= 1; //Décalage d'un bit vers la gauche.
            }
            return result;
        }
        /// <summary>
        /// Calcul l'ensemble des probabilités pour chaque octet du fichier.
        /// </summary>
        /// <param name="Path">Chemin du fichier</param>
        /// <returns>Dictionnaire, clé = octet, valeur = probabilitée d'apparition</returns>
        static Dictionary<Byte, Double> Get_Probabilities(String Path)
        {
            Dictionary<Byte, Double> result = new Dictionary<Byte, Double>();
            FileStream read_Stream = new FileStream(Path, FileMode.Open, FileAccess.Read);
            Byte temp;

            //Compte les occurences pour chaque octet.
            for (int i = 0; i < read_Stream.Length; i++)
            {
                temp = (Byte)read_Stream.ReadByte();
                if (result.ContainsKey(temp))
                {
                    result[temp]++;
                }
                else
                {
                    result.Add(temp, 1);
                }
            }

            foreach (Byte element in result.Keys.ToList())
            {
                result[element] /= read_Stream.Length;
            }
            //Tri le dictionnaire par ordre décroissant des probabilités.
            result = result.OrderByDescending(key => key.Value).ToDictionary(x => x.Key, y => y.Value);

            read_Stream.Close();
            return result;
        }
        /// <summary>
        /// Calcul l'entropie du fichier.
        /// </summary>
        /// <param name="Probabilities">Dictionnaire, clé = octet, valeur = probabilitée d'apparition</param>
        /// <returns>Entropie</returns>
        static Double Get_Entropy(Dictionary<Byte, Double> Probabilities)
        {
            Double entropy = 0.0;

            foreach (KeyValuePair<Byte, Double> element in Probabilities)
            {
                entropy += element.Value * Math.Log(element.Value, 2);
            }

            return -entropy;
        }
        /// <summary>
        /// Calcul la redondance de la source.
        /// </summary>
        /// <param name="Probabilities">Dictionnaire, clé = octet, valeur = probabilitée d'apparition</param>
        /// <returns>Redondance de la source</returns>
        static Double Get_Source_Redundancy(Dictionary<Byte, Double> Probabilities)
        {
            return Math.Log(Probabilities.Count, 2) - Get_Entropy(Probabilities);
        }
        /// <summary>
        /// Calcul la redondance de la résiduelle.
        /// </summary>
        /// <param name="Probabilities">Dictionnaire, clé = octet, valeur = probabilitée d'apparition</param>
        /// <returns>Redondance résiduelle</returns>
        static Double Get_Residual_Redundancy(Dictionary<Byte, Double> Probabilities, Double Average_Bits_By_Byte)
        {
            return Average_Bits_By_Byte - Get_Entropy(Probabilities);
        }
        /// <summary>
        /// Calcul du taux moyen de bits par octet.
        /// </summary>
        /// <param name="Shannon_Tree">Dictionnaire, clé = octet, valeur = noeud d'arbre Shannon-Fano</param>
        /// <returns>Taux moyen de bits par octet</returns>
        static Double Get_Average_Bits_By_Byte(Dictionary<Byte, Shannon_Element> Shannon_Tree)
        {
            Double average_Bits_By_Bytes = 0.0;
            foreach (KeyValuePair<Byte, Shannon_Element> item in Shannon_Tree)
            {
                average_Bits_By_Bytes += item.Value.Probability * item.Value.Encoding.Count;
            }
            return average_Bits_By_Bytes;
        }
        /// <summary>
        /// Génère un arbre de Shannon-Fano, stocké d'un dictionnaire.
        /// </summary>
        /// <param name="Probabilities">Dictionnaire, clé = octet, valeur = probabilitée d'apparition</param>
        /// <returns>Dictionnaire, clé = octet, valeur = noeud d'arbre Shannon-Fano</returns>
        static Dictionary<Byte, Shannon_Element> Get_Shannon_Tree(Dictionary<Byte, Double> Probabilities)
        {
            Dictionary<Byte, Shannon_Element> result = new Dictionary<Byte, Shannon_Element>();
            //Cas particulier, où il n'exite qu'une seule valeur d'octet.
            if (Probabilities.Count == 1)
            {
                result.Add(Probabilities.Keys.First(), new Shannon_Element(Probabilities.Values.First()));
                result.First().Value.Encoding.Add(false); //Je code l'octet avec un simple 0.
                return result;
            }

            foreach (KeyValuePair<Byte, Double> element in Probabilities)
            {
                result.Add(element.Key, new Shannon_Element(element.Value));
            }

            return Split(result);
        }
        /// <summary>
        /// (Récursive) Génération récursive de l'arbre, parcours GD.
        /// </summary>
        /// <param name="Shannon_Tree">Dictionnaire, clé = octet, valeur = noeud d'arbre Shannon-Fano</param>
        /// <returns>Dictionnaire, clé = octet, valeur = noeud d'arbre Shannon-Fano</returns>
        static Dictionary<Byte, Shannon_Element> Split(Dictionary<Byte, Shannon_Element> Shannon_Tree)
        {
            //Condition d'arrêt de la récursivité.
            if (Shannon_Tree.Count <= 1)
            {
                return Shannon_Tree;
            }

            Double half_Probability = 0.0;
            foreach (KeyValuePair<Byte, Shannon_Element> item in Shannon_Tree)
            {
                half_Probability += item.Value.Probability;
            }
            half_Probability /= 2;

            Double sum = 0.0;
            Dictionary<Byte, Shannon_Element> left = new Dictionary<Byte, Shannon_Element>();
            Dictionary<Byte, Shannon_Element> right = new Dictionary<Byte, Shannon_Element>();

            foreach (KeyValuePair<Byte, Shannon_Element> element in Shannon_Tree)
            {
                //Divise les valeurs en deux parties, tant que la somme est inférieure ou égale à la demi-probabilité on insère à gauche. 
                if (sum + element.Value.Probability >= half_Probability)
                {
                    //Si le résultat de la somme plus la valeur actuelle moins la demi-probabilité est plus petit que,
                    //la demi-probabilité moins la somme on insère dans l'arbre de gauche.
                    //
                    //Exemple : pour p0 = 0,6, p1 = p2 = 0.2,
                    //Au début la somme est nulle, la valeur actuelle est p1 et la demi-probabilié = 0,5, donc on compare et on obtient :
                    //0 + 0.6 - 0.5 = 0,1 < 0,5 - 0 = 0,5
                    //Donc la première valeur est insérée dans l'arbre de gauche.
                    //Pour la suivante on a 0.6 + 0,2 - 0.5 = 0,3 > 0,5 - 0,6 = -0,1, donc on insère à droite etc...
                    if ((sum + element.Value.Probability - half_Probability) < (half_Probability - sum))
                    {
                        element.Value.Encoding.Add(false);
                        left.Add(element.Key, element.Value);
                    }
                    else
                    {
                        element.Value.Encoding.Add(true);
                        right.Add(element.Key, element.Value);
                    }
                }
                else
                {
                    element.Value.Encoding.Add(false);
                    left.Add(element.Key, element.Value);
                }
                sum += element.Value.Probability;
            }

            //Appel récursif sur les arbres gauches et droits.
            left = Split(left);
            right = Split(right);

            //Fusion des branches.
            foreach (KeyValuePair<Byte, Shannon_Element> element in right)
            {
                left.Add(element.Key, element.Value);
            }
            return left;
        }
        /// <summary>
        /// Génère le rembourrage de fin de fichier.
        /// Le rembourrage est composé de deux octets, le premier est la liste de bits complétée de 0, et le deuxième le nombre de bits ajouté.
        /// </summary>
        /// <param name="Boolean_List">Liste de bits</param>
        /// <returns>Liste de bits</returns>
        static List<Boolean> Padd(List<Boolean> Boolean_List)
        {
            //S'il la liste est vide je renvoie, la taille 0, sous forme d'une liste de bits.
            if (Boolean_List.Count == 0)
            {
                return Get_List(0);
            }
            else
            {
                List<Boolean> result = Boolean_List.ToList(); //DeepCopy

                while (result.Count < 8)
                {
                    result.Add(false);
                }
                //J'ajoute la nombre de bits rajouté. sur 8 bits non signés.
                result.AddRange(Get_List((Byte)(8 - Boolean_List.Count)));

                return result;
            }
        }
        /// <summary>
        /// Supprime le rembourrage de fin de fichier, voir la fonction Padd.
        /// </summary>
        /// <param name="Data">Liste des bits (rembourrée)</param>
        /// <returns>Liste des bits (sans rembourrage)</returns>
        static List<Boolean> Unpadd(List<Boolean> Data)
        {
            //Je récupère la taille du rembourrage.
            Byte Pad_size = Get_Byte(Data.GetRange(Data.Count - 8, 8));
            return (Data.GetRange(0, Data.Count - (Pad_size + 8)));
        }
        /// <summary>
        /// Sérialisation de l'arbre de décodage de Shannon-Fano pour le stockage.
        /// </summary>
        /// <param name="Data">Dictionnaire, clé = octet, valeur = noeud d'arbre Shannon-Fano</param>
        /// <returns>Tableau d'octets</returns>
        static Byte[] Serialize_Shannon(Dictionary<Byte, Shannon_Element> Data)
        {
            List<Byte> buffer = new List<Byte>();

            List<Boolean> temp;
            Byte nb_Bits;
            foreach (KeyValuePair<Byte, Shannon_Element> element in Data)
            {
                buffer.Add(element.Key);
                //Encodage du codage de Shannon-Fano sur 4 bits, 3 bits pour la taille et un 1 pour indiquer la fin d'encodage.
                for (int i = 0; i < element.Value.Encoding.Count; i += 4)
                {
                    //Calcul du nombre de bits à écrire.
                    nb_Bits = (Byte)(i + 4 <= element.Value.Encoding.Count ? 4 : element.Value.Encoding.Count - i);
                    temp = element.Value.Encoding.GetRange(i, nb_Bits);
                    //Ajout du padding
                    for (int j = 0; j < 4 - nb_Bits; j++)
                    {
                        temp.Add(false);
                    }
                    //Ajout de la taille du padding
                    temp.AddRange(Get_List(nb_Bits).GetRange(5, 3));
                    //Si on a inséré tous les bits on insère la fin de codage.
                    temp.Add(nb_Bits + i == element.Value.Encoding.Count);
                    buffer.Add(Get_Byte(temp));
                }
            }

            return buffer.ToArray();
            /*
			Optionnel, je l'ai retiré, car utiliser une librairie de compression dans un programme qui fait de la compression, 
			je ne trouve pas cela très intéressant.

			//Je compresse l'arbre de décodage
			MemoryStream output = new MemoryStream();
			using (System.IO.Compression.DeflateStream dstream = new System.IO.Compression.DeflateStream(output, System.IO.Compression.CompressionLevel.Optimal))
			{
				dstream.Write(buffer.ToArray(), 0, buffer.Count);
			}

			return output.ToArray();
			*/
        }
        /// <summary>
        /// Désérialisation de l'arbre de décodage de Shannon-Fano.
        /// </summary>
        /// <param name="Data">Tableau d'octets</param>
        /// <returns>Dictionnaire, clé = octet, valeur = noeud d'arbre Shannon-Fano</returns>
        static Dictionary<String, Byte> Deserialize_Shannon(Byte[] Data)
        {
            /*
			Optionnel, je l'ai retiré, car utiliser une librairie de compression dans un programme qui fait de la compression, 
			je ne trouve pas cela très intéressant.
			
			//Je décompresse l'arbre de décodage
			MemoryStream input = new MemoryStream(Data);
			MemoryStream output = new MemoryStream();
			using (System.IO.Compression.DeflateStream dstream = new System.IO.Compression.DeflateStream(input, System.IO.Compression.CompressionMode.Decompress))
			{
				dstream.CopyTo(output);
			}
			Data = output.ToArray();
			
			*/

            Dictionary<String, Byte> Shannon_Tree = new Dictionary<String, Byte>();
            Int32 I = 0;
            Byte tmp_Byte = 0;
            String tmp_Str;
            List<Boolean> tmp_Bits;
            Boolean loop;

            while (I < Data.Length)
            {
                tmp_Byte = Data[I++]; //Je récupère l'octet associé à l'encodage. 
                tmp_Str = "";
                loop = true;
                while (loop)
                {
                    tmp_Bits = Get_List(Data[I]);
                    //Je récupère les bits pertinents.
                    foreach (Boolean element in tmp_Bits.GetRange(0, Get_Byte(tmp_Bits.GetRange(4, 3))))
                    {
                        tmp_Str += element ? '1' : '0';
                    }
                    //Je vérifie si j'ai atteint la fin de l'encodage.
                    if (Data[I] % 2 == 1)
                    {
                        loop = false;
                    }
                    I++;
                }
                Shannon_Tree.Add(tmp_Str, tmp_Byte);
            }

            return Shannon_Tree;
        }
        /// <summary>
        /// Compresse un fichier, à l'aide de l'encodage de Shannon-Fano.
        /// </summary>
        /// <param name="Filepath">Chemin du fichier</param>
        static void Compress(String Filepath)
        {
            Dictionary<Byte, Shannon_Element> Shannon_Tree = Get_Shannon_Tree(Get_Probabilities(Filepath));
            FileStream Source, Destination;
            Source = new FileStream(Filepath, FileMode.Open, FileAccess.Read);
            Destination = new FileStream(Filepath + ".shannon", FileMode.Create, FileAccess.Write);

            //J'insère l'arbre de décodage au début du fichier.
            Byte[] Decoding_Tree = Serialize_Shannon(Shannon_Tree);
            //J'encode la taille de l'arbre sur 16 bits non signés.
            Destination.WriteByte((Byte)(Decoding_Tree.Length >> 8));
            Destination.WriteByte((Byte)((Decoding_Tree.Length << 8) >> 8));
            Destination.Write(Decoding_Tree, 0, Decoding_Tree.Length);

            List<Boolean> buffer = new List<Boolean>();
            for (int i = 0; i < Source.Length; i++)
            {
                //Je récupère l'encodage de l'octet lu.
                buffer.AddRange(Shannon_Tree[(Byte)Source.ReadByte()].Encoding);
                //J'écrit par paquets de 8.
                while (buffer.Count >= 8)
                {
                    Destination.WriteByte(Get_Byte(buffer.GetRange(0, 8)));
                    buffer.RemoveRange(0, 8);
                }
            }
            Source.Close();

            //Je complète les bits restants.
            buffer = Padd(buffer);
            while (buffer.Count != 0)
            {
                Destination.WriteByte(Get_Byte(buffer.GetRange(0, 8)));
                buffer.RemoveRange(0, 8);
            }
            Destination.Close();
        }
        /// <summary>
        /// Décompresse un fichier compresser à l'aide de l'encodage de Shannon-Fano.
        /// </summary>
        /// <param name="Filepath">Chemin du fichier</param>
        static void Decompress(String Filepath)
        {
            FileStream Source, Destination;
            Source = new FileStream(Filepath, FileMode.Open, FileAccess.Read);
            //Pour le fichier de destination je supprime l'extension ".shannon".
            Destination = new FileStream(Filepath.Substring(0, Filepath.Length - 8) + ".original", FileMode.Create, FileAccess.Write);

            //Je récupère l'arbre de décodage.
            UInt16 Tree_Size = (UInt16)(Source.ReadByte() * 256);
            Tree_Size += (UInt16)Source.ReadByte();
            Byte[] Serialized_Tree = new Byte[Tree_Size];
            Source.Read(Serialized_Tree, 0, Tree_Size);
            Dictionary<String, Byte> Shannon_Tree = Deserialize_Shannon(Serialized_Tree);

            List<Boolean> buffer = new List<Boolean>();
            String tmp_Str;
            for (int i = (int)Source.Position; i < Source.Length - 2; i++)
            {
                buffer.AddRange(Get_List((Byte)Source.ReadByte()));
                //Je cherche dans le dictionnaire en regroupants les bits 1 à 1.
                //Ex : J'ai 0101 0101 cherche si mon arbre de décodage contient 0,
                //si ce n'est pas le cas je cherche avec 01, puis avec 010, etc...
                for (int j = 1; j <= buffer.Count; j++)
                {
                    tmp_Str = List_ToString(buffer.GetRange(0, j));
                    if (Shannon_Tree.ContainsKey(tmp_Str))
                    {
                        Destination.WriteByte(Shannon_Tree[tmp_Str]);
                        buffer.RemoveRange(0, j);
                        j = 0;
                    }
                }
            }

            //Je traîte les deux derniers octets différemment.
            buffer.AddRange(Get_List((Byte)Source.ReadByte()));
            buffer.AddRange(Get_List((Byte)Source.ReadByte()));
            buffer = Unpadd(buffer);
            Source.Close();

            for (int j = 1; j <= buffer.Count; j++)
            {
                tmp_Str = List_ToString(buffer.GetRange(0, j));
                if (Shannon_Tree.ContainsKey(tmp_Str))
                {
                    Destination.WriteByte(Shannon_Tree[tmp_Str]);
                    buffer.RemoveRange(0, j);
                    j = 0;
                }
            }
            Destination.Close();
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

            Dictionary<Byte, Double> Data = Get_Probabilities(FilePath);

            Console.WriteLine("Entropie : " + Double_ToString(Get_Entropy(Data)));
            Console.WriteLine("Rendondance de la source : " + Double_ToString(Get_Source_Redundancy(Data)));
            Console.WriteLine("Taux de compression maximal théorique : " + Double_ToString(Get_Entropy(Data) / Math.Log((new FileInfo(FilePath)).Length, 2) * 100) + "%");

            Dictionary<Byte, Shannon_Element> Shan = new Dictionary<byte, Shannon_Element>();


            Shan = Get_Shannon_Tree(Data);
            Console.WriteLine("Encodage : ");
            foreach (KeyValuePair<Byte, Shannon_Element> item in Shan)
            {
                Console.WriteLine(" - " + item.Key + " : " + item.Value.ToString());
            }
            Double Average_Bits_By_Byte = Get_Average_Bits_By_Byte(Shan);
            Console.WriteLine("Rendondance résiduelle : " + Double_ToString(Get_Residual_Redundancy(Data, Average_Bits_By_Byte)));

            Compress(FilePath);
            FileInfo Source = new FileInfo(FilePath);
            FileInfo Destination = new FileInfo(FilePath + ".shannon");
            Console.WriteLine("Taux de compression effectif : " + Double_ToString((Double)(Destination.Length) / Source.Length * 100) + "%");
            Decompress(FilePath + ".shannon");
            Console.WriteLine("Le fichier a été décompressé, chemin du fichier décompressé : " + FilePath + ".original");
            Console.ReadKey();
        }
    }
}
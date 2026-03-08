using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PrecessadorAnomalo
{
    internal class Compilador
    {
        public string SourceCode;

        private List<variavel> varlist = new List<variavel>();

        private List<variavel> LABList = new List<variavel>();

        private List<(string, byte)> LAB_Index = new List<(string, byte)>();

        private int indexprogram = 0;

        public byte[] MachineCode = new byte[128];

        public Compilador(string SourceCode)
        {
            this.SourceCode = SourceCode;
        }


        private static string[] mnemonicos =
        {
            "LDA","LDX","LDY","STA","STX","STY","AND","EOR",
            "ORA","ADD","SUB","INC","DEC","ASR","ASL","JMP",
            "BCS","BCC","BEQ","BNE","BMI","BPL","BVS","BVC",
            "CLC","CVL","SEC","BRK","NOP"
        };
        public static string[] mnemoEspscial =
        {
            "VAR","LAB"
        };
        public static char[] OPeraChar =
        {
            '0','A','X','Y','#','$'
        };
        //public static string TestSouceCode = "LDA #16 \r\n VAR VarTest \r\n INC A \r\n BRK 0 ";
        //public static string TestSouceCode = "VAR Test \r\n LDA #16 \r\n STA $Test \r\n LDY $Test \r\n ORA A \n\r BRK 0";

        class variavel
        {
            public int index, Valor;
            public string Nome;

            public variavel(string Nome, int Valor, int index)
            {
                this.Valor = Valor;
                this.Nome = Nome;
                this.index = index;
            }

        }
       
        public void Compiler()
        {

            string[] tokkens = Tokkentonize(SourceCode);

            for (int i = 0; i < tokkens.Length; i++)
            {
                string tok = tokkens[i];

                int MnemoIndex = Array.IndexOf(mnemonicos, tok.Substring(0, 3));
                int MnEspIndex = Array.IndexOf(mnemoEspscial, tok.Substring(0, 3));
                int Opchaindex = Array.IndexOf(OPeraChar, tok[3]);
                bool IsLAB = tok.EndsWith(':');

                if (MnemoIndex == -1 && MnEspIndex == -1 && !IsLAB) throw new Exception($"mnemônico desconhecido na lina {i}, {tok.Substring(0, 3)}");
                if (MnemoIndex >= 0 && Opchaindex == -1 && !IsLAB) throw new Exception($"Operante desconhecido na lina {i}, : {tok[3]}");


                // ---------------------- início da "Compilação" ------------------------------------
                if (MnemoIndex >= 0 && MnEspIndex == -1 && !IsLAB)  
                {
                    MachineCode[indexprogram] = (byte)((MnemoIndex << 3) | (byte)Opchaindex); indexprogram++;  // caso o mnemônico esteja na mnemonicos[]

                    if (Opchaindex > 3)                                                                        // caso a instrução precisse de um operador
                    {
                        string substring = tok.Substring(4);

                        bool Substring_Is_Numeric = ISNureric(substring);
                        bool Substring_Is_Hexadec = substring.IndexOf("0x") >= 0;
                        bool Substring_Is_Variavel = varlist.Exists(X => X.Nome == substring);
                        bool IS_A_Something = !Substring_Is_Numeric && !Substring_Is_Hexadec && !Substring_Is_Variavel; 

                        if (Substring_Is_Numeric)
                        {
                            MachineCode[indexprogram] = Convert.ToByte(substring); indexprogram++;
                        }

                        if (Substring_Is_Hexadec)
                        {
                            MachineCode[indexprogram] = HexadecimalCOnvert(substring); indexprogram++;
                        }

                        if (Substring_Is_Variavel)
                        {
                            var abacaxi = varlist.FirstOrDefault(X => X.Nome == substring);
                            MachineCode[indexprogram] = (byte)abacaxi.index; indexprogram++;
                        }

                        if (IS_A_Something)
                        {
                            MachineCode[indexprogram] = 0;
                            LAB_Index.Add((substring, (byte)indexprogram));
                        }

                    }
                
            
                }
                // ----------------------------- Criação de Variaveis ----------------------------------------
                if (MnemoIndex == -1 && MnEspIndex >= 0 && !IsLAB) if (MnEspIndex == 0)
                {
                        createVar(tok); 
                }
                
                if (MnemoIndex == -1 && MnEspIndex == -1 && IsLAB)                                          // É aqui que o bagulho complica
                {
                    LABList.Add(new variavel(tok.Replace(":",""), indexprogram, 0));
                }

            }
              //------------------------------- definições das Jump Functions ----------------------------------
            
           foreach(var iten in LAB_Index) // determina o endereço das jump com base na lista de LAb_Index e LABList
           {
                    if (!LABList.Exists(x => x.Nome == iten.Item1)) throw new Exception($"variavel não encontrada:`{iten.Item1}");
                         else
                         {
                         MachineCode[iten.Item2] = (byte)LABList.FirstOrDefault(X => X.Nome == iten.Item1).Valor;
                         }
           }
            if (indexprogram > (MachineCode.Length - (LABList.Count + varlist.Count))) throw new Exception("código transpassou limite definido e sobrescreveu endereços de variáveis");

               
         

            void createVar(string Localtokken)
            {
                int memorylocalvar = MachineCode.Length - (varlist.Count + 1);
                
                int num = Localtokken.IndexOf("=");
                int Commenint = num == -1 ? Localtokken.Length : num;
                
                string VarName =  Localtokken.Substring(3, Commenint - 3);
                string VArValor = num == -1? "0" : Localtokken.Substring(Commenint + 1);
                bool IsHexadecimal = VArValor.IndexOf("0x") != -1;

                varlist.Add(new variavel(VarName, 0, memorylocalvar));

                if (IsHexadecimal)
                {
                    MachineCode[memorylocalvar] = HexadecimalCOnvert(VArValor);
                }
                if (!IsHexadecimal) 
                {
                    MachineCode[memorylocalvar] = Convert.ToByte(VArValor);
                }
            }
            bool ISNureric(string Number)
            {
                int internum;
                return Int32.TryParse(Number, out internum);   
            }
            
        }
        public static string[] Tokkentonize(string Sourcecode)
        {
            List<string> InternListTokken = new List<string>();
            string[] interTokken = Sourcecode.Replace(" ","").Split("\r\n"); //remove os espaços em branco e divide a string Sourcecode em linhas
            
            foreach(string item in interTokken)
            {
               int num = item.IndexOf("//");
               int Commenint = num == -1 ? item.Length : num;  // remove qualquer texto depis de "//"
                 
               if (item != string.Empty) InternListTokken.Add(item.Substring(0, Commenint)); 
                
            }

            return InternListTokken.ToArray();

        }
        public static byte HexadecimalCOnvert(string target) // 0xXX
        {
            string interastring = target.Substring(2);
            int OUTvalor = Convert.ToInt32(interastring, 16);

            if (OUTvalor > 255) throw new Exception("Valor superior a 255");

            return (byte)OUTvalor;

        }


    }
}

    


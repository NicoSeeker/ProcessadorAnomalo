using System.Diagnostics;
using System;
using System.Text;

namespace PrecessadorAnomalo
{
    internal class Program
    {
        static void Main(string[] args)
        {
           
            byte[] MachineCode = new byte[128];
            string exemploaPath = Environment.CurrentDirectory + "\\exemplos\\";

            if (args.Length != 0)
            {
                if (args[0].EndsWith(".txt")) // compila e executa um programa
                {
                    if (File.Exists(exemploaPath + args[0]))
                    {
                        string SourceCode = File.ReadAllText(exemploaPath + args[0]);
                        Compilador Abobora = new Compilador(SourceCode);

                        Abobora.Compiler();

                        MachineCode = Abobora.MachineCode;
                        File.WriteAllBytes(exemploaPath + args[0].Replace(".txt",".bct"), Abobora.MachineCode);

                    }
                    else throw new Exception($"Arquivo de texto não encontrado, {exemploaPath + args[0]}");
                }
                if (args[0].EndsWith(".bct")) // só executa o programa
                {
                    if (File.Exists(exemploaPath + args[0]))
                    {
                        MachineCode = File.ReadAllBytes(exemploaPath + args[0]);
                    }
                    else throw new Exception($"Programa não encontrado, {exemploaPath + args[0]}");
                }
            }
            else             // compila e executa o programa padrão: ASM_de_pobre.txt
            {
                string SourceCode = File.ReadAllText(Environment.CurrentDirectory + "\\ASM_de_pobre.txt");
                Compilador Abobora = new Compilador(SourceCode);

                Abobora.Compiler();

                MachineCode = Abobora.MachineCode;
            }

            _Memory rom = new _Memory(MachineCode);
            _6502_Bizonho _6502 = new _6502_Bizonho(rom);

            while ((_6502.FG & 0x10) != 0x10)
            {
                Console.Clear();
                Console.WriteLine("----------------------------------------------");
                Console.WriteLine("Registradores   Acumulador:{0}   X:{1}   Y:{2}", _6502.A, _6502.X, _6502.Y);
                Console.WriteLine("Program Counter: {0}          Flags:{1} ", _6502.PC, Convert.ToString(_6502.FG, 2));
                Console.WriteLine("\nInstrução atual: {0}     |     {1}\n", Convert.ToString(rom.Ram[_6502.PC], 16), ConvertMachineCodeToNmemo(rom.Ram[_6502.PC], rom.Ram[_6502.PC + 1]));
                memoryLust(rom.Ram);
                

                _6502.ExecuteIstruction();
                Console.ReadKey();
            }
        }
        public static string ConvertMachineCodeToNmemo(byte machineCode, byte Oper)
        {

              string[] Nmemonicos =
              {
                 "LDA","LDX","LDY","STA","STX","STY","AND","EOR",
                 "ORA","ADD","SUB","INC","DEC","ASR","ASL","JMP",
                 "BCS","BCC","BEQ","BNE","BMI","BPL","BVS","BVC",
                 "CLC","CVL","SEC","BRK","NOP"
              };
              char[] OPeraChar =
              {
                 '0','A','X','Y','#','$'//,'@','*'
              };

            string outvalor = $"{Nmemonicos[machineCode >> 3]} {OPeraChar[machineCode & 7]}";
           
            if ((machineCode & 7) > 3)
            {
                outvalor += Oper.ToString();
            }

            return outvalor;
        }
        public static void memoryLust(byte[] Arraydebytes)
        {
            IEnumerable<byte[]> InternalLust = Arraydebytes.Chunk(16);
            StringBuilder SBR = new StringBuilder();
            int hexColuna = 0;

            SBR.AppendLine("   00_01_02_03_04_05_06_07_08_09_0A_0B_0C_0D_0E_0F");
            foreach (byte[] b in InternalLust)
            {
                SBR.AppendLine(Convert.ToString(hexColuna, 16) + "| " + BitConverter.ToString(b));
                hexColuna ++;
            }
            Console.WriteLine(SBR.ToString());
        } 

        }
    }



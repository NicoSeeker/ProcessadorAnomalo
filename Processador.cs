using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrecessadorAnomalo
{ 
        class _Memory
        {
            public byte[] Ram;
            public _Memory(byte[] Memory)
            {
                Ram = Memory;
            }

            public void SetRam(byte adress, byte Valor) => Ram[adress] = Valor;
            public byte ReadRam(byte adress) => Ram[adress];
            public byte ReadPoi(byte index) => Ram[Ram[index]];

        }
        internal class _6502_Bizonho
        {
            public byte A, X, Y, PC, FG;                // FG =  Negative | Overflow | ____ | Stop  | decimal | Interrupt | ZeroFlag | Carry
            private Func<byte>[] Operando;              //          8     |    7     |   6  |  5    |    4    |     3     |     2    |   1
            public _Memory memory;

            private byte[] JMPFunction = new byte[] { 0x0F, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17 };

            public _6502_Bizonho(_Memory Ram)
            {
                memory = Ram;
                Operando = new Func<byte>[]                  // Um array de funções, não é todo dia que se ve isso
                {
                () => 0,                                 // vazio                                                            0b00000_000
                () => A,                                 // Realtivo ao acummulador                                          0b00000_001
                () => X,                                 // Relativo ao registrador X                                        0b00000_010    
                () => Y,                                 // Relativo ao registrador Y                                        0b00000_011                                       
                () => memory.ReadRam((byte)(PC + 1)),    // Realtivo ao próximo byte da memoria        #absolute             0b00000_100
                () => memory.ReadPoi((byte)(PC + 1))     // Realativo ao byte que o ponteiro aponta    $PageZero             0b00000_101
                };
            }
            public void ExecuteIstruction()
            {
                byte instrução = memory.ReadRam(PC);
                byte Oppers = ((byte)(instrução & 0x07));
                byte Function = (byte)((instrução & 0xF8) >> 3);


                Action[] Fun = new Action[]                  // Olha outro aqui 
                {
                () => {A = Operando[Oppers](); SetFlagNZ(A);},                    // 0  LDA  0b00000_000
                () => {X = Operando[Oppers](); SetFlagNZ(X);},                    // 1  LDX, 0b00001_000
                () => {Y = Operando[Oppers](); SetFlagNZ(Y);},                    // 2  LDY, 0b00010_000
                () => {memory.SetRam(Operando[Oppers](), A);},                    // 3  STA, 0b00011_000
                () => {memory.SetRam(Operando[Oppers](), X);},                    // 4  STX, 0b00100_000
                () => {memory.SetRam(Operando[Oppers](), Y);},                    // 5  STY, 0b00101_000
                () => {A &= Operando[Oppers](); SetFlagNZ(A);},                   // 6  AND, 0b00110_000
                () => {A ^= Operando[Oppers](); SetFlagNZ(A);},                   // 7  EOR, 0b00111_000
                () => {A |= Operando[Oppers](); SetFlagNZ(A);},                   // 8  ORA, 0b01000_000
                () => {A = ASO(A, Operando[Oppers](), false);},                   // 9  ADD, 0b01001_000
                () => {A = ASO(A, Operando[Oppers](), true );},                   // 10 SUB, 0b01010_000
                () => {INCDEC(Operando[Oppers](),false, Oppers);},                // 11 INC, 0b01011_000
                () => {INCDEC(Operando[Oppers](),true,  Oppers);},                // 12 DEC, 0b01100_000
                () => {SHIFT (Operando[Oppers](),false, Oppers);},                // 13 ASR, 0b01101_000
                () => {SHIFT (Operando[Oppers](),true , Oppers);},                // 14 ASL, 0b01110_000
                () => {PC = Operando[Oppers]();              },                   // 15 JMP, 0b01111_000           JUMP
                () => {JUM( Operando[Oppers](),(FG & 0x01) == 0x01); },           // 16 BCS, 0b10000_000           Jump se a carry flag estiver ativa
                () => {JUM( Operando[Oppers](),(FG & 0x01) != 0x01); },           // 17 BCC, 0b10001_000           JUMP se a carry flag estiver limpa
                () => {JUM( Operando[Oppers](),(FG & 0x02) == 0x02); },           // 18 BEQ, 0b10010_000           Jump se a zero flag estiver ativa
                () => {JUM( Operando[Oppers](),(FG & 0x02) != 0x02); },           // 19 BNE, 0b10011_000           Jump se a Zero flag estiver limpa
                () => {JUM( Operando[Oppers](),(FG & 0x80) == 0x80); },           // 20 BMI, 0b10100_000           Jump se a negative flag estiver ativa
                () => {JUM( Operando[Oppers](),(FG & 0x80) != 0x80); },           // 21 BPL, 0b10101_000           Jump se a negative flag estiver limpa
                () => {JUM( Operando[Oppers](),(FG & 0x40) == 0x40); },           // 22 BVS, 0b10110_000           Jump se a Overflow flag estiver ativa 
                () => {JUM( Operando[Oppers](),(FG & 0x40) != 0x40); },           // 23 BVC, 0b10111_000           Jump se a Overflow flag estiver limpa
                () => {if ((FG & 0x01) == 0X01) FG ^= 0x01; },                    // 24 CLC, 0b11000_000           Limpa a carry flag
                () => {if ((FG & 0x40) == 0x40) FG ^= 0x40; },                    // 25 CVL, 0b11001_000           Limpa a OverflowFlag
                () => {if ((FG & 0x01) != 0x01) FG ^= 0x01; },                    // 26 SEC, 0b11010_000           Ativa a carryflag
                () => {if ((FG & 0x10) != 0x10) FG ^= 0x10; },                    // 27 BRK, 0b11011_000           Ativa a BreakFLag (fim do programa)
                () => {                                     }                     // 28 NOP, 0b11100_000           NOP funcition, faz nada, função vagabunda

                };
                Fun[Function]();

                if ((Oppers > 3) && !JMPFunction.Contains<byte>(Function)) PC += 2;
                if ((Oppers <= 3) && !JMPFunction.Contains<byte>(Function)) PC++;
            }

            // --------------------- SETFLAG -----------------------------------------------------------
            private void SetFlagNZ(byte Target)
            {
                if ((Target >= 0x80 && FG <= 0x80) || (Target <= 0x80 & FG >= 0x80)) FG ^= 0x80;                          //verifica se o valor é maior ou igual a 0x80, seta ela se Target >= 0x80 e a N flag está desativada. OU se target < 0x80 e a N flag está ativa

                if ((((Target == 0) && ((FG & 0x02) != 0x02)) || ((Target != 0x02) && (FG & 0x02) == 0x02))) FG ^= 0x02;  // mesmo caso da N flag, mas verifica se Target == 0;
            }

            // --------------------- Operação de Soma E Subtração ---------------------------------------
            private byte ASO(byte b1, byte b2, bool sub)
            {
                b2 = sub ? (byte)(b2 ^ 0xFF) : b2;                                                            // se a função se sub estriver ativa, B2 ^ 0xFF vai inverter os bits, resultando em uma subtração
                short temp = (short)(b1 + b2 + (FG & 0x01));
                if (((temp > 0xFF) && ((FG & 1) != 1) || temp < 0xFF) && ((FG & 1) == 1)) FG ^= 1;            // seta o carryBit se TEMP > 255;  FG ^= 0b00000001 
                if ((sub && (b2 > b1) && (FG & 1) == 0) || ((sub && (b1 > b2) && (FG & 1) != 1))) FG ^= 1;    // seta o carryBit se b2 < b1;
                if (((b1 ^ b2) & (b1 ^ temp) & 0x80) != 0) FG ^= 0x40;                                        // seta o Overflow  FG ^= 0b01000000

                return (byte)(temp & 0xFF);
            }

            // --------------------- Operação de Incremento e decrimento (sla como escreve) --------------
            private void INCDEC(byte Adress, bool DEC, byte Func)  // INC & DEC, somente com ponteiros 
            {
                byte valor = Func < 4 ? Adress : memory.ReadRam(Adress); // gambiarra
                valor = DEC ? (byte)(valor - 1) : (byte)(valor + 1);
                SetFlagNZ(valor);

                if (Func == 1) A = valor;
                if (Func == 2) X = valor;
                if (Func == 3) Y = valor;
                if (Func == 5) memory.SetRam(Adress, valor);

            }

            // --------------------- OPerações Lógicas de Shift --------------------------------------------
            private void SHIFT(byte Adress, bool LEFT, byte Func)  // somente com ponteiros, acho que vai dar merda usar um ASR #XX
            {
                byte Valor = Func < 4 ? Adress : memory.ReadRam(Adress);
                int TEMP = (Valor << 8);                 // 0x0000XX00   int criado para verificar Overflow caso (int & 0xFF) != 0 || (int & 0xFF0000) != 0
                TEMP = LEFT ? (TEMP << 1) : (TEMP >> 1);

                bool IsOverFlow = ((TEMP & 0xFF0000) != 0) || ((TEMP & 0xFF) != 0);
                SetFlagNZ((byte)((Valor & 0XFF00) >> 8));

                if (IsOverFlow && ((FG & 0x40) != 0x40) || (!IsOverFlow && ((FG & 0x40) == 0x40))) FG ^= 0x40;

                if (Func == 1) A = (byte)((TEMP & 0xFF00) >> 8);
                if (Func == 2) X = (byte)((TEMP & 0xFF00) >> 8);
                if (Func == 3) Y = (byte)((TEMP & 0xFF00) >> 8);
                if (Func == 5) memory.SetRam(Adress, (byte)((TEMP & 0xFF00) >> 8));

            }
            private void JUM(byte Adress, bool aboboa)
            {
                if (aboboa) PC = Adress;
                else PC += 2;
            }



        }
    }




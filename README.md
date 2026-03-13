# PrecessadorAnomalo

	Projeto criado apenas para estudo, embora funcional está repleto de bugs. Não tenho muito conhecimento na programação.

	O "PrecessadorAnomalo" é um processador/interpretador && compilador baseado no 6502, ele é capaz de "compilar"
	um assembly em um código que posteriormente é processado.

# "Compilação"

		Para Compilar um arquivo de texto para o arquivo executável do programa use o .\PrecessadorAnomalo nome_do_arquivo.txt 
	"lembrando que esse arquivo tem que estar na pasta exemplos do projeto".
		Uma vez compilado o arquivo vai gerar um .bct (basic compiled text) que pode ser executado sem a necessidade de recompilação
    usando o .\PrecessadorAnomalo nome_do_arquivo.btc
	Caso não for dado um argumento de entrada, o programa vai compilar/executar o arquivo ASM_de_pobre.txt na raiz do projeto.

# Limitações & funções 

	Os programas não podem ter mais de 256 bytes, já que é usado somente um byte para o endereçamento;
	Não existe a stack;

	O compilador pode gerar Variáveis usando o mnemônico VAR: VAR (nome da variável) = (valor da variável)

	VAR var1         // para criar uma variável com valor zero;
	VAR var1 = 16    // para criar uma variável com um valor númerico;
	var var1 = 0x10  // para criar uma variável com um valor em hexadecimal;

	O compilador pode criar label para as Jump Functions:

	LOOP:            // Criação de label

	JMP #LOOP        // Jump para o endereço da label

	Tudo depois de "//" é ignorado pelo compilador

# Mnemônico e operadores

	Todos os Mnemônico precisão ter um operador, mesmo que seja instruções com BRK ou CLC 
	(que normalmente não precisão ter um operador)

	'0' // vazio
	'A' // Relativo ao acumulador
	'X' // Relativo ao registrador X
	'Y' // Relativo ao registrador Y
	'#' // Valor absoluto relativo ao próximo endereço na memória  #absolute
	'$' // relativo a um ponteiro                                  $PageZero

	LDA, carrega um valor no registrador A
	LDX, carrega um valor no registrador X
	LDY, carrega um valor no registrador Y

	STA, Salva o valor de A na memória
	STX, Salva o valor de X na memória
	STY, Salva o valor de Y na memória

	AND, Faz um AND lógico entre o acumulador e algum valor
	EOR, Faz um EOR lógico entre o acumulador e algum valor
	ORA, Faz um ORA lógico entre o acumulador e algum valor
	ADD, Soma entre o acumulador e algum valor
	SUB, Subtração entre o acumulador e algum valor

	INC, Incrementa algum valor
	DEC, Decrementa algum valor

	ASR, Arithmetic Shift Right, valor >> 1
	ASL, Arithmetic Shift Left,  valor << 1

	JMP, Jump para algum lugar da memória
	BCS, Jump se a carry flag estiver ativa
	BCC, JUMP se a carry flag estiver limpa
	BEQ, Jump se a zero flag estiver ativa
	BNE, Jump se a Zero flag estiver limpa
	BMI, Jump se a negative flag estiver ativa
	BPL, Jump se a negative flag estiver limpa
	BVS, Jump se a Overflow flag estiver ativa
	BVC, Jump se a Overflow flag estiver limpa

	CLC, Limpa a carry flag
	CVL, Limpa a OverflowFlag
	SEC, Ativa a carryflag
	BRK, Ativa a BreakFLag (fim do programa)
	NOP, NOP funcition, faz nada


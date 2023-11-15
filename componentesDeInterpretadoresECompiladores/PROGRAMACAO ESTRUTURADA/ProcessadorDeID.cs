using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

using parser;
using Util;
using parser.ProgramacaoOrentadaAObjetos;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

using System.Globalization;
using parser.textoFormatado;
using System.Reflection.Emit;
using System.Text.RegularExpressions;

namespace parser
{

    /// <summary>
    /// sistema de compilação de código orquidea.
    /// </summary>
    public class ProcessadorDeID : BuildInstrucoes
    {

        /// <summary>
        /// codigo de um processador id. lista de tokens para compilar
        /// </summary>
        public List<string> codigo;
        /// <summary>
        /// escopo do processador.
        /// </summary>
        public Escopo escopo;
        /// <summary>
        /// linguagem contendo classes, métodos, propriedades, funções, variáveis, e operadores.
        /// </summary>
        private static LinguagemOrquidea lng = null;




        /// <summary>
        ///  assinatura de uma função compiladora de uma instrucao da linguagem.
        /// </summary>
        /// <param name="sequencia">texto contendo uma sequencia possivel de commpilar.</param>
        /// <param name="escopo">contexto onde o texto está.</param>
        /// <returns></returns>
        public delegate Instrucao MetodoTratador(UmaSequenciaID sequencia, Escopo escopo);


        /// <summary>
        /// lista de métodos tratadores para ordenação.
        /// </summary>
        public static List<MetodoTratadorOrdenacao> tratadores = new List<MetodoTratadorOrdenacao>();






        /// <summary>
        /// lista de instruções construidas na compilação.
        /// </summary>
        private List<Instrucao> instrucoes = new List<Instrucao>();


        // guarda as sequencias já mapeadas e resumidas.
        private static List<List<string>> sequenciasJaMapeadas = new List<List<string>>(); 


        // tipos de acesso a propriedades e metodos.
        private static List<string> acessorsValidos = new List<string>() { "public", "private", "protected" };


        public GerenciadorWrapper gerenciador = new GerenciadorWrapper();


        /// <summary>
        /// lista de identifcadores de instanciacao de wrappers objects.
        /// </summary>
        private static List<string> tokensIdenficacaoWrappers = null;



        public List<Instrucao> GetInstrucoes()
        {
            return this.instrucoes;
        }

        public List<MetodoTratadorOrdenacao> GetHandlers()
        {
            return ProcessadorDeID.tratadores;
        }



        public ProcessadorDeID(string code):base(new List<string>())
        {
            this.codigo = new Tokens(code).GetTokens();

            // inicializa os headers.
            Expressao.InitHeaders(code);
            // inicializa o mapeamento de sequencias id resumidas, afim de processamento de sequencias e instruções.
            this.InitMapeamento();
            // carrega os nomes que identificam uma instanciacao id.
            this.GetIdentifyWrappers();
            if (lng == null)
                lng = LinguagemOrquidea.Instance();

      
            this.instrucoes = new List<Instrucao>();
            this.escopo = new Escopo(codigo);

            
            
        }
        /// <summary>
        /// construtor. Extrai e constroi instruções para serem consumidas em um programaVM.
        /// O codigo de um escopo é unido ao codigo de outros escopos antereriores.
        /// </summary>
        public ProcessadorDeID(List<string> code) : base(code)
        {
            // inicializa os headers.
            Expressao.InitHeaders(code);
            // inicializa as sequencias id mapeadas resumidas.
            this.InitMapeamento();
            // inicializa a lista de identificadoes de instanciacao wrappers.
            this.GetIdentifyWrappers();


            // instancia a linguagem padrão..
            if (lng == null)
            {
                lng = LinguagemOrquidea.Instance();
            }
                

            this.instrucoes = new List<Instrucao>();
            codigo = new List<string>();
            codigo.AddRange(code);

            this.escopo = new Escopo(code); // obtém um escopo para o processador de sequencias ID.
            

        } 



        /// <summary>
        /// obtem uma lista de nomes de idenficação para uma instanciação wrapper.
        /// </summary>
        /// <returns></returns>
        private void GetIdentifyWrappers()
        {
            if (ProcessadorDeID.tokensIdenficacaoWrappers == null)
            {
                ProcessadorDeID.tokensIdenficacaoWrappers= new List<string>();
                List<WrapperData> wrappers = WrapperData.GetAllTypeWrappers();
                foreach (WrapperData w in wrappers)
                {
                    tokensIdenficacaoWrappers.AddRange(w.getNamesIDWrapperData());
                }

               
            }
            
        }


        public static void LoadHandler(MetodoTratador umTratador, string patternResumedOfSequence)
        {
            tratadores.Add(new MetodoTratadorOrdenacao(umTratador, patternResumedOfSequence));
            
        } 


        private void InitMapeamento()
        {
            if ((tratadores == null) || (tratadores.Count == 0))
            {

               
                //__________________________________________________________________________________________________________________________________
                //          DEFINIÇÃO DE SEQUENCIAS ID.
                /// Lembrando que um ID pode ser um único elemento ou uma expressão, que é um conjunto de elementos.

                // definição sequências ID Estruturadas.
                string rgx_chamadaAFuncaoSemRetornoESemParametros = "id ( )";
                string rgx_chamadaAFuncaoCOMRetornoCOMParametros = "id ( exprss";


                string rgx_definicaoDeVariavelComAtribuicaoDeChamadaDeFuncao = "id id = id (";
                string rgx_definicaoDeVariavelComAtribuicaoDeChamadaDeMetodo = "id id = id . id (";
                string rgx_definicaoComTipoDefinidoEOperadorUnarioPosOrdem = "operador id";
                string rgx_definicaoComTipoDefinidoEOperadorUnarioPreOrdem = "id operador";
                string rgx_definicaoVariavelNaoEstaticaSemAtribuicao = "id id ;";




                
                // definicao de funções estruturadas, e defnição de funções.
                string rgx_definicaoDeFuncaoComRetornoComUmOuMaisParametrosComCorpoOuSemCorpo = "id id ( id id";
                string rgx_definicaoDeFuncaoComRetornoSemParametrosSemCorpo = "id id ( ) ;";
                string rgx_defnicaoDeFuncaoComRetornoSemParametrosComCorpo = "id id ( )";





                // definição sequências POO.
             
                string rgx_inicializacaoPropriedadeEstaticaComAtribuicao = "id static id id = id";
                string rgx_inicializacaoPropriedadeEstaticaSemAtribuicao = "id static id id";

                string rgx_inicializacaoPropriedadeEAtribuicao = "id id id = id ;";
                string rgx_inicializacaoPropriedadeSemAtribuicao = "id id id ;";






                // sequencias de propriedades/chamada de metodos, aninhados.
                string rgx_propriedadesEncadeadasComOuSemAtribuicao = "id . id  = exprss";
                string rgx_chamadaAMetodoSemParametros = "id . id ( ) ;";
                string rgx_chamadaAMetodoComParametros = "id . id ( exprss";
                string rgx_chamadaDeMetodoComAtribuicao = "id id = id . id (";
                string rgx_chamadaDeMetodoComAtribuicaoSemInicializacao = "id = id . id (";
                string rgx_chamadaAFuncaoComParametrosESemRetorno = "id ( exprss";
                string rgx_chamadaAFuncaoComAtribuicaoComParametros = "id id =  id ( exprss";
                string rgx_chamadaAFuncaoComAtribuicaoComParametrosSemInicializacao = "id =  id ( exprss";




                // definicao de metodos POO.
                string rgx_definicaoDeMetodoComParametrosComCorpoOuSemCorpo = "id id id ( id";
                string rgx_definicaoMetodoSemParametrosComOuSemCorpo = "id id id ( )";



            
                // sequencia aspecto. 
                string rgx_definicaoAspecto = "aspecto ( id id";





          
                // seuencias de importação de classes da classe base.
                string rgx_Importer = "importer ( id . id ) ;";
                string rgx_moduleLibrary = "module id ;";

                // sequencias de instrução da linguagem.
                string rgx_CreateNewObject = "id id = create (";
                string rgx_CreateNewObjectSemInicializacao = "id = create (";
                string rgx_ConstrutorUp = "id . construtorUP ( exprss";
                string rgx_DefinicaoInstrucaoWhileComBlocoOuSemBloco = "while ( exprss )";
                string rgx_DefinicaoInstrucaoForComBlocoOuSemBloco = "for ( id = exprss ; exprss ; exprss )";
                string rgx_DefinicaoInstrucaoForComBlocoOuSemBlocoComAtribuicao = "for ( id id = exprss ; exprss ; exprss )";
                string rgx_DefinicaoInstrucaoIfComBlocoOuSemBlocoSemElse = "if ( exprss )";
                string rgx_DefinicaoInstrucaoIfComBlocoComElse = "if ( exprss ) bloco else bloco";
                string rgx_DefinicaoInstrucaoSetVar = "SetVar ( id )";
                string rgx_DefinicaoInstrucaoGetObjeto = "id GetObjeto ( id )";



                string rgx_DefinicaoInstrucaoBreak = "continue";
                string rgx_DefinicaoInstrucaoContinue = "break";
                string rgx_DefinicaoInstrucaoReturn = "return exprss;";



                // sequencias de definicao de operadores binarios, unarios.
                string rgx_DefinicaoDeOperadorBinario = "operador id id ( id id , id id ) prioridade id metodo id;";
                string rgx_DefinicaoDeOperadorUnario = "operador id id ( id id ) prioridade id metodo id ;";


                string rgx_AtribuicaoDeVariavelSemDefinicao = "id = exprss ;";
                string rgx_definicaoDeVariavelNaoEsttaticaComAtribuicao = "id id = exprss ;";
                string rgx_DefinicaoInstrucaoCasesOfUse = "casesOfUse id : { ( case exprss exprss ) : {";














                //____________________________________________________________________________________________________________________________
                // CARREGA OS METODO TRATADORES E AS SEQUENCIAS DE ID ASSOCIADAS.


                // SEQUENCIAS OPERACAO
                LoadHandler(OperacaoUnarioPosOrder, rgx_definicaoComTipoDefinidoEOperadorUnarioPosOrdem);
                LoadHandler(OperacaoUnarioPreOrder, rgx_definicaoComTipoDefinidoEOperadorUnarioPreOrdem);

                // SEQUENCIAS ESTRUTURADAS.
                LoadHandler(Atribuicao, rgx_definicaoVariavelNaoEstaticaSemAtribuicao);
                LoadHandler(Atribuicao, rgx_definicaoDeVariavelComAtribuicaoDeChamadaDeFuncao);

                LoadHandler(Atribuicao, rgx_definicaoDeVariavelNaoEsttaticaComAtribuicao);
                LoadHandler(Atribuicao, rgx_chamadaAFuncaoSemRetornoESemParametros);


                // propriedades estaticas.
                LoadHandler(AtribuicaoEstatica, rgx_inicializacaoPropriedadeEstaticaComAtribuicao);
                LoadHandler(AtribuicaoEstatica, rgx_inicializacaoPropriedadeEstaticaSemAtribuicao);
                LoadHandler(BuildInstrucaoDefinicaoDePropriedadeAninhadas, rgx_propriedadesEncadeadasComOuSemAtribuicao);


                // propriedaedes nao estaticas.
                LoadHandler(Atribuicao, rgx_inicializacaoPropriedadeEAtribuicao);
                LoadHandler(Atribuicao, rgx_inicializacaoPropriedadeSemAtribuicao);





                LoadHandler(BuildInstrucaoDefinicaoDePropriedadeAninhadas, rgx_definicaoDeVariavelComAtribuicaoDeChamadaDeMetodo);
                LoadHandler(ChamadaMetodo, rgx_chamadaAMetodoSemParametros);
                LoadHandler(ChamadaMetodo, rgx_chamadaAMetodoComParametros);
                LoadHandler(ChamadaFuncao, rgx_chamadaAFuncaoComParametrosESemRetorno);
                LoadHandler(ChamadaFuncao, rgx_chamadaAFuncaoCOMRetornoCOMParametros);



                LoadHandler(ChamadaDeMetodoComAtribuicao, rgx_chamadaDeMetodoComAtribuicao);
                LoadHandler(ChamadaDeMetodoComAtribuicao, rgx_chamadaDeMetodoComAtribuicaoSemInicializacao);
                LoadHandler(ChamadaDeMetodoComAtribuicao, rgx_chamadaAFuncaoComAtribuicaoComParametros);
                LoadHandler(ChamadaDeMetodoComAtribuicao, rgx_chamadaAFuncaoComAtribuicaoComParametrosSemInicializacao);



                // SEQUENCIAS INTEROPABILIDADE:
                LoadHandler(BuildInstrucaoImporter, rgx_Importer);

                // sequencias definição de métodos.
                LoadHandler(BuildDefinicaoDeMetodo, rgx_definicaoDeMetodoComParametrosComCorpoOuSemCorpo);
                LoadHandler(BuildDefinicaoDeMetodo, rgx_definicaoMetodoSemParametrosComOuSemCorpo);

                // sequencias definicao de funcoes.
                LoadHandler(BuildDefinicaoDeFuncao, rgx_definicaoDeFuncaoComRetornoComUmOuMaisParametrosComCorpoOuSemCorpo);
                LoadHandler(BuildDefinicaoDeFuncao, rgx_definicaoDeFuncaoComRetornoSemParametrosSemCorpo);
                LoadHandler(BuildDefinicaoDeFuncao, rgx_defnicaoDeFuncaoComRetornoSemParametrosComCorpo);

                // sequencias de instruções da linguagem,
                LoadHandler(BuildInstrucaoWhile, rgx_DefinicaoInstrucaoWhileComBlocoOuSemBloco);
                LoadHandler(BuildInstrucaoFor, rgx_DefinicaoInstrucaoForComBlocoOuSemBloco);
                LoadHandler(BuildInstrucaoFor, rgx_DefinicaoInstrucaoForComBlocoOuSemBlocoComAtribuicao);
                LoadHandler(BuildInstrucaoIFsComOuSemElse, rgx_DefinicaoInstrucaoIfComBlocoOuSemBlocoSemElse);
                LoadHandler(BuildInstrucaoIFsComOuSemElse, rgx_DefinicaoInstrucaoIfComBlocoComElse);
                LoadHandler(BuildInstrucaoBreak, rgx_DefinicaoInstrucaoBreak);
                LoadHandler(BuildInstrucaoContinue, rgx_DefinicaoInstrucaoContinue);
                LoadHandler(BuildInstrucaoReturn, rgx_DefinicaoInstrucaoReturn);

                LoadHandler(BuildInstrucaoCreate, rgx_CreateNewObject);
                LoadHandler(BuildInstrucaoCreate, rgx_CreateNewObjectSemInicializacao);

                LoadHandler(Atribuicao, rgx_AtribuicaoDeVariavelSemDefinicao);


           

                LoadHandler(BuildInstrucaoGetObjeto, rgx_DefinicaoInstrucaoGetObjeto);
                LoadHandler(BuildInstrucaoSetVar, rgx_DefinicaoInstrucaoSetVar);
                LoadHandler(BuildInstrucaoOperadorBinario, rgx_DefinicaoDeOperadorBinario);
                LoadHandler(BuildInstrucaoOperadorUnario, rgx_DefinicaoDeOperadorUnario);
                LoadHandler(BuildInstrucaoCasesOfUse, rgx_DefinicaoInstrucaoCasesOfUse);
                LoadHandler(BuildInstrucaoConstrutorUP, rgx_ConstrutorUp);
                LoadHandler(BuildInstrucaoModule, rgx_moduleLibrary);


                // programacao orientada a aspectos.
                LoadHandler(BuildDefinicaoDeAspecto, rgx_definicaoAspecto);



                // ordena a lista de métodos tratadores, pelo cumprimento de seus testes de sequencias ID.            
                ProcessadorDeID.MetodoTratadorOrdenacao.ComparerMetodosTratador comparer = new MetodoTratadorOrdenacao.ComparerMetodosTratador();
                if (tratadores == null)
                {
                    tratadores = new List<MetodoTratadorOrdenacao>();
                }
                tratadores.Sort(comparer);

                tratadores[5].metodo = ChamadaDeMetodoComAtribuicao;
             

            } // if tratadores

        } // InitMapeamento()
        private void ProcessaAspectos()
        {
            // faz uma varredura nas classes, encontrando objetos e/ou metodos monitorados, para inserir  aspectos no codigo.
            if (lng.Aspectos != null)
            {
                for (int x = 0; x < lng.Aspectos.Count; x++)
                    lng.Aspectos[x].AnaliseAspecto(escopo);
            }
        }

      
        public void Compilar()
        {

            // obtem os tokens do codigo para compilar.
            List<string> tokens = new Tokens(codigo).GetTokens();


            // calcula uma vez os headers, para a compilação total do codigo. os headers auxilia no processamento de classes,
            // evitando instanciação posterior a atribuição de propriedades, metodos, operadores.

            InitExpressao(tokens);
           


            // faz a compilação dos tokens da entrada do processador.
            this.CompileEscopos(this.escopo, tokens); 

            // faz a compilação dos aspectos.
            this.ProcessaAspectos();

           
        }


        /// <summary>
        /// faz a compilacao de um escopo.
        /// </summary>
        private void CompileEscopos(Escopo escopo, List<string> tokens)
        {


            int umToken = 0;
            while (umToken < tokens.Count)
            {

                if (((umToken + 1) < tokens.Count) &&
                    (acessorsValidos.Find(k => k.Equals(tokens[umToken])) != null) &&
                    ((tokens[umToken + 1].Equals("class")) ||
                    (tokens[umToken + 1].Equals("interface"))))
                {
                    try
                    {
                        // PROCESSAMENTO DE INTERFACE.
                        string classeOuInterface = tokens[umToken + 1];
                        if (classeOuInterface == "interface")
                        {

                            List<string> tokensDaInterface = UtilTokens.GetCodigoEntreOperadores(umToken, "{", "}", tokens);


                            ExtratoresOO extratorDeClasses = new ExtratoresOO(escopo, tokensDaInterface);
                            Classe umaInterface = extratorDeClasses.ExtraiUmaInterface();
                            Escopo.nomeClasseCurrente = umaInterface.GetNome();
                            if (extratorDeClasses.MsgErros.Count > 0)
                                this.escopo.GetMsgErros().AddRange(extratorDeClasses.MsgErros);

                            if (umaInterface != null)
                            {

                                umToken += umaInterface.tokensDaClasse.Count;
                                continue;
                            }

                        }
                        else
                        // PROCESSAMENTO DE CLASSE.
                        if (classeOuInterface == "class")
                        {



                            List<string> tokensDaClasse = UtilTokens.GetCodigoEntreOperadores(umToken, "{", "}", tokens);
                            ExtratoresOO extratorDeClasses = new ExtratoresOO(escopo, tokensDaClasse);


                            Classe umaClasse = extratorDeClasses.ExtaiUmaClasse(Classe.tipoBluePrint.EH_CLASSE);
                            if (umaClasse != null)
                            {
                                Escopo.nomeClasseCurrente = umaClasse.GetNome();
                                umToken += umaClasse.tokensDaClasse.Count; // +1 do acessor (public, private, protected).

                                if (extratorDeClasses.MsgErros.Count > 0)
                                    this.escopo.GetMsgErros().AddRange(extratorDeClasses.MsgErros); // retransmite erros na extracao da classe, para a mensagem de erros do escopo.
                                continue;

                            } // if

                        }
                    }
                    catch
                    {
                        UtilTokens.WriteAErrorMensage("Erro na extracao de tokens de uma classe ou interface, verifique a sintaxe das especificacoes de classe.", tokens, escopo);
                        return;
                    }
                }
                else
                if (lng.IsID(tokens[umToken]) || (lng.isTermoChave(tokens[umToken])))
                {


                    
                    UmaSequenciaID sequenciaCurrente = UmaSequenciaID.ObtemUmaSequenciaID(umToken, tokens, codigo); // obtem a sequencia  seguinte.



                    // IDENTIFICA UMA INSTANCIACAO WRAPPER DATA.
                    string textSequencia = Utils.OneLineTokens(sequenciaCurrente.tokens);
                    bool isFound = false;
                    for (int i = 0; i < tokensIdenficacaoWrappers.Count; i++)
                    {
                        if (textSequencia.Contains(tokensIdenficacaoWrappers[i]))
                        {
                            isFound = true;
                            break;
                        }
                    }




                    // PROCESSAMENTO DE OBJETOS WRAPPER DATA OBJECTS, INSTANCIACAO.
                    if (isFound)
                    {
                        Expressao exprssIntanciacaoWrapper = Expressao.ProcessamentoDeInstanciacaoWrappersObjectData(sequenciaCurrente.tokens.ToArray(), escopo);
                        if (exprssIntanciacaoWrapper != null)
                        {
                            UmaSequenciaID sequenciaCreateWrapperObjects = new UmaSequenciaID(exprssIntanciacaoWrapper.tokens.ToArray(), this.codigo);
                            Instrucao instrucaoChamadaDeMetodo = ChamadaMetodo(sequenciaCreateWrapperObjects, escopo);
                            if (instrucaoChamadaDeMetodo != null)
                            {
                                this.instrucoes.Add(instrucaoChamadaDeMetodo);
                                umToken += sequenciaCurrente.tokens.Count;
                                continue;
                            }
                            else
                            {
                                UtilTokens.WriteAErrorMensage("sequencia de tokens: " + sequenciaCurrente + " nao reconhecida, verifique a sintaxe e tambem COLOCANDO PONTO-E-VIGULA NO FINAL DESTA INSTRUÇÃO, E NAS ANTERIORES TAMBÉM:  ", sequenciaCurrente.tokens, escopo);
                                return;
                            }
                        }
                        
                    }

                    
                    // IDENTIFICA UMA ANOTAÇÃO WRAPPER DATA, PARA GetElement, SetElement.
                    bool isFoundWrapperObject = false;
                    if ((escopo.tabela.objetos != null) && (escopo.tabela.objetos.Count > 0))
                    {
                        
                        if (sequenciaCurrente.tokens != null)
                        {
                            for (int i = 0; i < sequenciaCurrente.tokens.Count; i++)
                            {
                                if (escopo.tabela.objetos.FindIndex(k => k.nome == sequenciaCurrente.tokens[i] && k.isWrapperObject == true) != -1)
                                {
                                    isFoundWrapperObject=true;
                                    break;
                                }
                            }
                        }
                    }

                    // PROCESSAMENTO DE GET/SET ELEMENT DE OBJETOS WRAPPER.
                    if (isFoundWrapperObject)
                    {
                        Expressao exprssWithWrapperObjects = new Expressao(sequenciaCurrente.tokens.ToArray(), escopo);
                        if (exprssWithWrapperObjects != null)
                        {
                            Instrucao instrucaoWrapper = new Instrucao(ProgramaEmVM.codeExpressionValid, new List<Expressao>() { exprssWithWrapperObjects }, new List<List<Instrucao>>(), escopo);
                            if (instrucaoWrapper != null) 
                            {
                                this.instrucoes.Add(instrucaoWrapper);
                                umToken += sequenciaCurrente.tokens.Count;
                                continue;
                            }
                        }
                        else
                        {   // continua o processamento: a expressao contem wrapper object, mas nao como o 1o. token.
                            umToken += 1;
                            continue;
                        }

                    }
                    


                    if ((sequenciaCurrente!=null) &&  (sequenciaCurrente.tokens.Count==1) && (sequenciaCurrente.tokens[0].Trim(' ') ==";"))
                    {
                        umToken += 1;
                        continue;
                    }

                    if (sequenciaCurrente == null)
                    {
                        // continua o processamento, para verificar se há mais erros no codigo orquidea.
                        UtilTokens.WriteAErrorMensage("error, tokens sequence not match: " + Utils.OneLineTokens(sequenciaCurrente.tokens), sequenciaCurrente.tokens, escopo);  
                    }



                    // obtém o indice de metodo tratador.
                    MatchSequencias(sequenciaCurrente, escopo);






                    if (sequenciaCurrente.indexHandler == -1)
                    {
                        Instrucao instrucaoExpressaoCorreta = BuildExpressaoValida(sequenciaCurrente, escopo); // a sequencia id pode ser uma expressao correta, há build para expressoes corretas.
                        if (instrucaoExpressaoCorreta != null)
                        {
                            this.instrucoes.Add(instrucaoExpressaoCorreta); // a sequencia id é uma expressao correta, processa e adiciona a instrucao de expressao correta.
                            umToken += sequenciaCurrente.tokens.Count;
                            continue;
                        }

                        // trata de problemas de sintaxe da sequencia currente, emitindo uma mensagem de erro.
                        UtilTokens.WriteAErrorMensage("intern error in compilador, error in tokens sequence, sequence: " + Utils.OneLineTokens(sequenciaCurrente.tokens) + ". ", sequenciaCurrente.tokens, escopo);


                        umToken += sequenciaCurrente.tokens.Count;
                        continue;  // continua, para capturar mais erros em outras sequencias currente.   
                    }
                    else
                    {

                        try
                        {
                            // chamada do método tratador para processar a costrução de escopos, da 0sequencia de entrada.
                            Instrucao instrucaoTratada = tratadores[sequenciaCurrente.indexHandler].metodo(sequenciaCurrente, escopo);
                            if ((instrucaoTratada != null) && (instrucaoTratada.code != -1))
                                this.instrucoes.Add(instrucaoTratada);
                            else
                            if (instrucaoTratada == null)
                            {
                                Instrucao instrucaoExpressaoCorreta = BuildExpressaoValida(sequenciaCurrente, escopo); // a sequencia id pode ser uma expressao correta, há build para expressoes corretas.
                                if ((instrucaoExpressaoCorreta != null) && (instrucaoExpressaoCorreta.code != -1))
                                {
                                    this.instrucoes.Add(instrucaoExpressaoCorreta); // a sequencia id é uma expressao correta, processa e adiciona a instrucao de expressao correta.
                                    umToken += sequenciaCurrente.tokens.Count;
                                    continue;
                                }
                                else
                                {
                                    UtilTokens.WriteAErrorMensage("sequencia de tokens: " + sequenciaCurrente + " nao reconhecida, verifique a sintaxe e tambem COLOCANDO PONTO-E-VIGULA NO FINAL DESTA INSTRUÇÃO, E NAS ANTERIORES TAMBÉM:  ", sequenciaCurrente.tokens, escopo);
                                }
                                    
                            }
                            umToken += sequenciaCurrente.tokens.Count; // atualiza o iterator de tokens, consumindo os tokens que foram utilizados no processamento da seuencia id currente.
                            continue;

                        }
                        catch
                        {
                            UtilTokens.WriteAErrorMensage("Erro de compilacao da sequencia: " + sequenciaCurrente.ToString() + ". Verifique a sintaxe, com terminos de fim de instrucao, tambem. ", tokens, escopo);
                            return;
                        }
                    } // if tokensSequenciaOriginais.Count>0

                } // if linguagem.VerificaSeEID()
                umToken++;
            } // while

        }// CompileEcopos()


        private static void InitExpressao(List<string> tokensClasses)
        {
            if (Expressao.classesDaLinguagem == null)
            {

                // constroi os headers de Expressao.
                Expressao.headers = new FileHeader();
                List<Classe> classesDaLinguagem = LinguagemOrquidea.Instance().GetClasses();
                List<HeaderClass> headersDaLinguagem = new List<HeaderClass>();
                Expressao.headers.ExtractHeadersClassFromClassesOrquidea(classesDaLinguagem, headersDaLinguagem);


                Expressao.headers.ExtractCabecalhoDeClasses(tokensClasses);
                Expressao.headers.cabecalhoDeClasses.AddRange(headersDaLinguagem);
            }
        }








        public void MatchSequencias(UmaSequenciaID sequencia, Escopo escopo)
        {

            


            string str_sequencia = Utils.OneLineTokens(sequencia.tokens);
            str_sequencia = UtilTokens.FormataEntrada(str_sequencia);


            TextExpression textExpression = new TextExpression();
            string patthernResumido = textExpression.FormaPatternResumed(str_sequencia);
            List<string> tokensResumed = new Tokens(patthernResumido).GetTokens();


            for (int seqMapeada = 0; seqMapeada < tratadores.Count; seqMapeada++)
            {
              
                if ((sequencia.tokens.Count >= tratadores[seqMapeada].tokens.Count) && (tratadores[seqMapeada].regex_sequencia.IsMatch(str_sequencia)))   
                {
                    sequencia.indexHandler = seqMapeada;
                    MatchBlocos(sequencia, escopo);
                    return;
                }
            } // seqMapeada

            // adiciona o indice -1, pois não encontrou uma sequencia mapeada, nem expressão complexa.
            sequencia.indexHandler = -1;

        } 



      

        /// <summary>
        /// a ideia deste método é identificar blocos, e colocar os tokens de blocos, na sequencia de entrada.
        /// </summary>
        private static void MatchBlocos(UmaSequenciaID sequencia, Escopo escopo)
        {
            int indexSearchBlocks = sequencia.tokens.IndexOf("{");

            while (indexSearchBlocks != -1)
            {
                // retira um bloco a partir dos tokens sem modificações (originais).
                List<string> umBloco = UtilTokens.GetCodigoEntreOperadores(indexSearchBlocks, "{", "}", sequencia.tokens);
                // encontrou um bloco de tokens, adiciona à sequencia de entrada.
                if ((umBloco != null) && (umBloco.Count > 0))
                {
                    umBloco.RemoveAt(0);
                    umBloco.RemoveAt(umBloco.Count - 1);
                    sequencia.sequenciasDeBlocos.Add(new List<UmaSequenciaID>() { new UmaSequenciaID(umBloco.ToArray(), escopo.codigo) });
                } // if
                indexSearchBlocks = sequencia.tokens.IndexOf("{", indexSearchBlocks + 1);
            } // while
        } // MatchBlocos()



        protected Instrucao OperacaoUnarioPreOrder(UmaSequenciaID sequencia, Escopo escopo)
        {
            //   OP. ID
            if (sequencia == null)
            {
                return null;
            }
            if ((lng.IsID(sequencia.tokens[0]) && (lng.VerificaSeEhOperadorUnario(sequencia.tokens[1]))))
            {


                string nomeObjeto = sequencia.tokens[1];
                Objeto v = escopo.tabela.GetObjeto(nomeObjeto, escopo);

                if (v == null)
                    UtilTokens.WriteAErrorMensage("objeto: " + nomeObjeto + "  inexistente.", sequencia.tokens, escopo);


                string nomeOperador = sequencia.tokens[0];
                Operador umOperador = Operador.GetOperador(nomeOperador, v.GetTipo(), "UNARIO", lng);

                if (umOperador == null)
                    UtilTokens.WriteAErrorMensage("operador: " + nomeOperador + "  não é unário.", sequencia.tokens, escopo);



                bool isFoundClass = false;
                foreach (Classe umaClasse in escopo.tabela.GetClasses())

                    if (umaClasse.GetNome() == nomeOperador)
                    {
                        isFoundClass = true;
                        break;
                    } //if
                if (!isFoundClass)
                {

                    UtilTokens.WriteAErrorMensage("operador: " + nomeObjeto + "  inexistente. linha: ", sequencia.tokens, escopo);
                    return null;
                }
                else
                {
                    escopo.tabela.AdicionaExpressoes(escopo, new Expressao(new string[] { nomeOperador + " " + nomeObjeto }, escopo));
                    return new Instrucao(ProgramaEmVM.codeOperadorUnario, new List<Expressao>(), new List<List<Instrucao>>());
                }
            } // if
            return null;
        } // OperacaoUnarioPreOrder()

        protected Instrucao OperacaoUnarioPosOrder(UmaSequenciaID sequencia, Escopo escopo)
        {
            // ID OPERADOR

            if ((lng.IsID(sequencia.tokens[1]) && (lng.VerificaSeEhOperadorUnario(sequencia.tokens[0]))))
            {

                string nomeObjeto = sequencia.tokens[1];
                Objeto v = escopo.tabela.GetObjeto(nomeObjeto, escopo);

                if (v == null)
                    UtilTokens.WriteAErrorMensage("objeto: " + nomeObjeto + "  inexistente.", sequencia.tokens, escopo);

                string nomeOperador = sequencia.tokens[0];
                Operador umOperador = Operador.GetOperador(nomeOperador, v.GetTipo(), "UNARIO", lng);
                if (umOperador.GetTipo() != "OPERADOR UNARIO")
                    UtilTokens.WriteAErrorMensage("operador: " + nomeOperador + "  não é unário.", sequencia.tokens, escopo);

                bool isFoundClass = false;
                foreach (Classe umaClasse in escopo.tabela.GetClasses())

                    if (umaClasse.GetNome() == nomeOperador)
                    {
                        isFoundClass = true;
                        break;
                    } //if
                if (!isFoundClass)
                    UtilTokens.WriteAErrorMensage("operador: " + nomeObjeto + "  inexistente.", sequencia.tokens, escopo);
                else
                {
                    escopo.tabela.AdicionaExpressoes(escopo, new Expressao(new string[] { nomeObjeto + " " + nomeOperador }, escopo));
                    return new Instrucao(ProgramaEmVM.codeOperadorUnario, new List<Expressao>(), new List<List<Instrucao>>());
                }
            } // if
            return null;
        } // OperacaoUnarioPreOrder()

     
        protected Instrucao BuildExpressaoValida(UmaSequenciaID sequencia, Escopo escopo)
        {
            if (sequencia.tokens == null)
                return null;
            Expressao expressaoCorreta = new Expressao(sequencia.tokens.ToArray(), escopo);
            if ((expressaoCorreta != null) && (expressaoCorreta.Elementos.Count > 0))
            {
                // adiciona a expressao correta, para a lista de expressoes do escopo, para fins de otimização.
                escopo.tabela.AdicionaExpressoes(escopo, expressaoCorreta);

                // cria a instrucao para a expressao correta.
                Instrucao instrucaoExpressaoCorreta = new Instrucao(ProgramaEmVM.codeExpressionValid, new List<Expressao>() { expressaoCorreta }, new List<List<Instrucao>>());
                return instrucaoExpressaoCorreta;
            }
            return null;
        }
        protected Instrucao Atribuicao(UmaSequenciaID sequencia, Escopo escopo)
        {


            bool isAlreadyCreated = false;
            string nomeObjetoAtribuicao = "";
            string tipoObjetoAtribuicao = "";
            object valorObjeto = "";



            /// estrutura de dados para atribuicao:
            /// 0- Elemento[0]: tipo do objeto.
            /// 1- Elemento[1]: nome do objeto.
            /// 2- Elemento[2]: se tiver propriedades/metodos aninhados: expressao de aninhamento. Se não tiver, ExpressaoElemento("") ".
            /// 3- expressao da atribuicao ao objeto/vetor. (se nao tiver: ExpressaoELemento("")
            /// 4- indice de enderacamento de atribuicao a um elemento de um vetor.

          

            bool com_acessor = false;
            string acessorObjetoAtribuicao = "";
            int offsetAcessor = 0;
            if (acessorsValidos.IndexOf(sequencia.tokens[0]) != -1)
            {
                acessorObjetoAtribuicao = sequencia.tokens[0];
                com_acessor = true;
                offsetAcessor = 1;
            }
            else
            {
                acessorObjetoAtribuicao = "private";
                com_acessor = false;
                offsetAcessor = 0;
            }

            // COM ACESSOR.
            if (com_acessor) 
            {
                tipoObjetoAtribuicao = sequencia.tokens[1];
                nomeObjetoAtribuicao = sequencia.tokens[2];
            }
            else
            // SEM ACESSOR.
            if (!com_acessor)
            {
                
                tipoObjetoAtribuicao = sequencia.tokens[0];
                int indexHeader = Expressao.headers.cabecalhoDeClasses.FindIndex(k => k.nomeClasse == tipoObjetoAtribuicao);
                // CASO DE PROPRIEDADE SEM INSTANCIACAO, OU PROPRIEDADE DE CLASSE, DENTRO DA CLASSE.
                if (indexHeader == -1)
                {
                    nomeObjetoAtribuicao = sequencia.tokens[0];
                    Objeto objJaExistente = escopo.tabela.GetObjeto(nomeObjetoAtribuicao, escopo);
                    if (objJaExistente == null)
                    {
                        tipoObjetoAtribuicao = Escopo.nomeClasseCurrente;
                    }
                    else
                    {
                        tipoObjetoAtribuicao = objJaExistente.tipo;
                    }
                    
                    
                }
                else
                {
                    nomeObjetoAtribuicao = sequencia.tokens[1];
                }
                
            }

            if ((offsetAcessor + 1) < sequencia.tokens.Count) 
            {
                Objeto objJaInstanciado = escopo.tabela.GetObjeto(sequencia.tokens[0 + offsetAcessor + 1], escopo);
                // O PRIMEIRO TOKEN DA SEQUENCIA É UM NOME DE PROPRIEADE JA INSTANCIADA.
                if (objJaInstanciado != null)
                {
                    tipoObjetoAtribuicao = objJaInstanciado.tipo;
                    nomeObjetoAtribuicao = objJaInstanciado.nome;
                    isAlreadyCreated = true;
                }
            }
            



            int indexOperadorIgual = sequencia.tokens.IndexOf("=");
            int indexOperadorComma = sequencia.tokens.IndexOf(";");
            if (indexOperadorComma == -1)
            {
                indexOperadorComma = sequencia.tokens.Count - 1;
            }
            if ((sequencia.tokens.Count >= 4) && (sequencia.tokens.Contains("=")))
            {

                
                if ((indexOperadorIgual + 2 < sequencia.tokens.Count) && (sequencia.tokens[indexOperadorIgual + 2] == ";")) 
                {
                    valorObjeto = sequencia.tokens[indexOperadorIgual + 1];
                    
                }
                else
                {
                    valorObjeto = null;
                }
            }
            else
            {
                valorObjeto = null;
            }

            // obtem a expressao de atribuicao, que guarda o calculo do valor a ser atribuido à variável.
            Expressao expressaoAtribuicao = null;
            if (indexOperadorIgual != -1)
            {
                List<string> tokensValorAtribuicao = sequencia.tokens.GetRange(indexOperadorIgual + 1, sequencia.tokens.Count - (indexOperadorIgual + 1));
                expressaoAtribuicao = new Expressao(tokensValorAtribuicao.ToArray(), escopo);

                if ((expressaoAtribuicao != null) &&
                    (expressaoAtribuicao.Elementos != null) &&
                    (expressaoAtribuicao.Elementos.Count > 0) &&
                    (expressaoAtribuicao.Elementos[0].GetType() == typeof(ExpressaoNumero)) && (valorObjeto != null)) 
                {
                    ExpressaoNumero exprss = (ExpressaoNumero)expressaoAtribuicao.Elementos[0];
                    valorObjeto= exprss.ConverteParaNumero(valorObjeto.ToString(), escopo);
                 
                }
                else
                if ((expressaoAtribuicao != null) &&
                   (expressaoAtribuicao.Elementos != null) &&
                   (expressaoAtribuicao.Elementos.Count > 0) &&
                   (expressaoAtribuicao.Elementos[0].GetType() == typeof(ExpressaoObjeto)))
                {
                    ExpressaoObjeto exprss = (ExpressaoObjeto)expressaoAtribuicao.Elementos[0];
                    valorObjeto = exprss.objectCaller.valor;

                }

            }
            else
            {
                expressaoAtribuicao = new Expressao();
            }


      
                


            Expressao exprrsCabecalho = new Expressao();
            exprrsCabecalho.Elementos.Add(new ExpressaoElemento(tipoObjetoAtribuicao));
            exprrsCabecalho.Elementos.Add(new ExpressaoElemento(nomeObjetoAtribuicao));
            exprrsCabecalho.Elementos.Add(new ExpressaoElemento(""));
            exprrsCabecalho.Elementos.Add(expressaoAtribuicao);


            


            Instrucao instrucaoAtribuicao = new Instrucao(ProgramaEmVM.codeAtribution, new List<Expressao>(), new List<List<Instrucao>>());
            instrucaoAtribuicao.expressoes.Add(exprrsCabecalho);
            escopo.tabela.AdicionaExpressoes(escopo, exprrsCabecalho);




            Objeto objCreated = new Objeto(acessorObjetoAtribuicao, tipoObjetoAtribuicao, nomeObjetoAtribuicao, valorObjeto);
            if (!isAlreadyCreated) 
            {
                // registra o objeto, no mesmo escopo onde o objeto está, sem busca de escopos acima.
                escopo.tabela.RegistraObjeto(objCreated);

            }


        
            return instrucaoAtribuicao;
        }


        protected Instrucao BuildInstrucaoDefinicaoDePropriedadeAninhadas(UmaSequenciaID sequencia, Escopo escopo)
        {
            // processamento de expressoes que envolvem expressoesAninhadas, como "objA.a= obj.a+x";

            Expressao aninhadas = new Expressao(sequencia.tokens.ToArray(), escopo);


            if (aninhadas == null)
            {
                UtilTokens.WriteAErrorMensage("expressao de propriedades aninhadas nao reconhecida.", sequencia.tokens, escopo);
                return null;
            }
            string tipoObjeto = sequencia.tokens[0];
            string nomeObjeto = sequencia.tokens[1];

            // registra a expressao, para fins de otimização sobre o valor da expressão.
            escopo.tabela.AdicionaExpressoes(escopo, aninhadas); 
          
            Instrucao instrucaoPropriedade = new Instrucao(ProgramaEmVM.codeExpressionValid, new List<Expressao>(), new List<List<Instrucao>>());


            instrucaoPropriedade.expressoes.Add(aninhadas);


               



            return instrucaoPropriedade;
        }



        protected Instrucao AtribuicaoEstatica(UmaSequenciaID sequencia, Escopo escopo)
        {
            string acessorDaVariavel = sequencia.tokens[0];
            Classe tipoDaVariavel = RepositorioDeClassesOO.Instance().GetClasse(sequencia.tokens[2]);
            if (tipoDaVariavel == null)
            {
                UtilTokens.WriteAErrorMensage("tipo da variavel estatica nao definida anteriormente.", sequencia.tokens, escopo);
                return null;
            }

            string nomeDaVariavel = sequencia.tokens[3];

            if (tipoDaVariavel.GetPropriedade(nomeDaVariavel) != null)
            {

                UtilTokens.WriteAErrorMensage("propriedade estatica ja definida anteriormente.", sequencia.tokens, escopo);
                return null;
            }

            Objeto propriedadeEstatica = new Objeto(acessorDaVariavel, tipoDaVariavel.GetNome(), nomeDaVariavel, null, escopo, (Boolean)true);
            propriedadeEstatica.isStatic = true;


            Classe classeRegistrada = RepositorioDeClassesOO.Instance().GetClasse(tipoDaVariavel.nome);

            if (classeRegistrada != null)
            {
                if (classeRegistrada.propriedadesEstaticas.Find(k => k.GetNome().Equals(propriedadeEstatica.GetNome())) == null)
                    classeRegistrada.propriedadesEstaticas.Add(propriedadeEstatica);
            }

            Expressao exprssAtribuicaoDePropriedadeEstatica = new Expressao(sequencia.tokens.ToArray(), escopo);
            if (exprssAtribuicaoDePropriedadeEstatica != null)
            {
                SetValorNumero(propriedadeEstatica, exprssAtribuicaoDePropriedadeEstatica, escopo);
            }
            else
            {
                exprssAtribuicaoDePropriedadeEstatica = new Expressao();
            }

                

            Expressao exprssDeclaracaoPropriedadeEstatica = new Expressao();
            exprssDeclaracaoPropriedadeEstatica.Elementos.Add(new ExpressaoElemento(tipoDaVariavel.GetNome()));
            exprssDeclaracaoPropriedadeEstatica.Elementos.Add(new ExpressaoElemento(nomeDaVariavel));
            exprssDeclaracaoPropriedadeEstatica.Elementos.Add(new ExpressaoElemento(""));
            exprssDeclaracaoPropriedadeEstatica.Elementos.Add(exprssAtribuicaoDePropriedadeEstatica);
            exprssAtribuicaoDePropriedadeEstatica.Elementos.Add(new ExpressaoElemento("estatica"));

            Instrucao instrucaoAtribuicao = new Instrucao(ProgramaEmVM.codeAtribution, new List<Expressao>(), new List<List<Instrucao>>());

           
            if (exprssAtribuicaoDePropriedadeEstatica == null)
            {
                UtilTokens.WriteAErrorMensage("sequencia nao completamente definida ainda.", sequencia.tokens, escopo);
                return null;
            }

            if (exprssAtribuicaoDePropriedadeEstatica.Elementos.Count > 0)
            {
                // registra as expressoes de atribuicao no escopo, para fins de otimização.
                escopo.tabela.AdicionaExpressoes(escopo, exprssAtribuicaoDePropriedadeEstatica); 
            }
                


            instrucaoAtribuicao.expressoes.AddRange(new List<Expressao>() { exprssDeclaracaoPropriedadeEstatica, exprssAtribuicaoDePropriedadeEstatica });

            return instrucaoAtribuicao;


        }




        internal static void SetValorNumero(Objeto v, Expressao expressaoNumero, Escopo escopo)
        {
            if (expressaoNumero == null)
                return;

            string possivelNumero = expressaoNumero.ToString().Replace(" ", "");

            if (Expressao.Instance.IsTipoInteiro(possivelNumero))
                v.SetValorObjeto(int.Parse(possivelNumero)); // seta o valor previamente, pois em modo de depuracao, é necessario este valor. Em um programa, a atribuicao é feito pela instrucao atribuicao.
            else
            if (Expressao.Instance.IsTipoFloat(possivelNumero))
                v.SetValorObjeto(float.Parse(possivelNumero)); // seta o valor previamente, pois em modo de depuracao, é necessario este valor. Em um programa, a atribuicao é feito pela instrucao atribuicao.
            else
            if (Expressao.Instance.IsTipoDouble(possivelNumero))
                v.SetValorObjeto(double.Parse(possivelNumero));
        }

        ///______________________________________________________________________________________________________________________________________________________
        /// MÉTODOS TRATADORES DE CHAMADA DE FUNÇÃO.
        private Instrucao ChamadaFuncao(UmaSequenciaID sequencia, Escopo escopo)
        {
            // ID ( ID )

            string nomeFuncao = sequencia.tokens[0];

            Expressao exprssChamadaDeFuncao = new Expressao(sequencia.tokens.ToArray(), escopo);
            if (exprssChamadaDeFuncao.Elementos[0].GetType() != typeof(ExpressaoChamadaDeMetodo))
            {
                UtilTokens.WriteAErrorMensage("chamada de funcao invalido!", sequencia.tokens, escopo);
                return null;
            }

            Metodo funcaoCompativel = ((ExpressaoChamadaDeMetodo)exprssChamadaDeFuncao).funcao;
            List<Expressao> expressoesParametros = ((ExpressaoChamadaDeMetodo)exprssChamadaDeFuncao).Elementos[0].Elementos;

            ExpressaoChamadaDeMetodo exprssFuncaoChamada = (ExpressaoChamadaDeMetodo)exprssChamadaDeFuncao;

            Expressao exprssDefinicaoDaChamada = new Expressao();
            exprssDefinicaoDaChamada.Elementos.Add(new ExpressaoElemento("chamada"));
            exprssDefinicaoDaChamada.Elementos.Add(exprssFuncaoChamada);


            // registra as expessoes parametros, no escopo, para fins de otimização sobre o valor.
            escopo.tabela.AdicionaExpressoes(escopo, expressoesParametros.ToArray());
            escopo.tabela.AdicionaExpressoes(escopo, exprssFuncaoChamada);


            Instrucao instrucaoChamada = new Instrucao(ProgramaEmVM.codeCallerFunction, new List<Expressao>() { exprssDefinicaoDaChamada }, new List<List<Instrucao>>());
            return instrucaoChamada;
        } // ChamadaFuncaoSemRetornoEComParametros()


        protected Instrucao ChamadaMetodo(UmaSequenciaID sequencia, Escopo escopo)
        {

            
            //  ID . ID ( ID )";
            Expressao expressaoPrincipal = new Expressao(sequencia.tokens.ToArray(), escopo);
            if ((expressaoPrincipal == null) || (expressaoPrincipal.Elementos.Count == 0) || (expressaoPrincipal.Elementos[0].GetType() != typeof(ExpressaoChamadaDeMetodo)))
            {
                UtilTokens.WriteAErrorMensage("erro em chamada de metodo.", sequencia.tokens, escopo);
                return null;
            }



           

            // adiciona as expressoes da chamada de metodo, para fins de otimização.
            for (int indexExprss = 0; indexExprss < expressaoPrincipal.Elementos.Count; indexExprss++) 
            {
                // ex: metodo1(x,y), sendo expressao.elementos[0] a expressao da chamada, e expressao.Elementos[0].Elementos os parametros.
                List<Expressao> expressoesParametros = ((ExpressaoChamadaDeMetodo)expressaoPrincipal.Elementos[indexExprss]).parametros;

                // forma a estrutura de dados que contem os dados da chamada do metodo: chamadas de metodos aninhados, propriedades aninhadas.
                ExpressaoChamadaDeMetodo exprssDefinicaoDaChamada = (ExpressaoChamadaDeMetodo)expressaoPrincipal.Elementos[indexExprss];
     
                // inclui na funcionalidade de otimização de expressoes, as expressões parâmetros, para fins de otimização.
                escopo.tabela.AdicionaExpressoes(escopo, expressoesParametros.ToArray());
                escopo.tabela.AdicionaExpressoes(escopo, expressaoPrincipal);
            }


            // constroi a instrução chamada de método.
            Instrucao instrucaoChamadaDeMetodo = new Instrucao(ProgramaEmVM.codeCallerMethod, new List<Expressao>() { expressaoPrincipal }, new List<List<Instrucao>>());



            // retorna apenas a ultima chamada de metodo, as demais já foram inseridas na lista de instruções feitas pelo compilador.
            return instrucaoChamadaDeMetodo;

        } 




        protected Instrucao ChamadaDeMetodoComAtribuicao(UmaSequenciaID sequencia, Escopo escopo)
        {
            Expressao expss = new Expressao(sequencia.tokens.ToArray(), escopo);
            int indexSignalEquals = sequencia.tokens.IndexOf("=");
            if ((indexSignalEquals == -1) || (indexSignalEquals + 1 >= sequencia.tokens.Count))
            {
                UtilTokens.WriteAErrorMensage("bad format in calling expression", sequencia.tokens, escopo);
                return null;
            }


            Instrucao instrucaoAtribuicao = Atribuicao(sequencia, escopo);
            return instrucaoAtribuicao;
        }

        protected Instrucao BuildDefinicaoDeMetodo(UmaSequenciaID sequencia, Escopo escopo)
        {
            string acessor = null;
            string tipoRetornoMetodo = null;
            string nomeMetodo = null;

            if (ProcessadorDeID.acessorsValidos.Find(k => k.Equals(sequencia.tokens[0])) != null)
            {
                acessor = sequencia.tokens[0];
                tipoRetornoMetodo = sequencia.tokens[1];
                nomeMetodo = sequencia.tokens[2];
            } // if


            List<Objeto> parametrosDoMetodo = new List<Objeto>();

            int indexParentesAbre = sequencia.tokens.FindIndex(k => k == "(");
            if (indexParentesAbre == -1)
            {
                UtilTokens.WriteAErrorMensage("funcao sem interface de parametros, parte entre ( e ), exemplo deveria ser FuncaoA(){} e encontrou FuncaoA{}", sequencia.tokens, escopo);
                return null;
            }
            // constroi os parâmetros da definição da função.
            ExtraiParametrosDaFuncao(sequencia, parametrosDoMetodo, indexParentesAbre, escopo);



            // constroi a função.
            Metodo umMetodoComCorpo = new Metodo();
            umMetodoComCorpo.nome = nomeMetodo;
            umMetodoComCorpo.tipoReturn = tipoRetornoMetodo;
            umMetodoComCorpo.acessor = acessor;
            umMetodoComCorpo.escopo = new Escopo(sequencia.tokens);



            if (parametrosDoMetodo.Count > 0)
                umMetodoComCorpo.parametrosDaFuncao = parametrosDoMetodo.ToArray();



            // REGISTRA OS PARÂMETROS DA FUNÇÃO, COMO UMA ATRIBUICAO, O QUE É LOGICO POIS A DEFINICAO DE PARAMETROS É UMA ATRIBUICAO!!!
            for (int x = 0; x < parametrosDoMetodo.Count; x++)
            {
                Objeto objParametro = new Objeto("private", parametrosDoMetodo[x].GetTipo(), parametrosDoMetodo[x].GetNome(), null);
                escopo.tabela.GetObjetos().Add(objParametro);
            }

            // registra a função no escopo pai.
            escopo.tabela.RegistraFuncao(umMetodoComCorpo);
            
         

            Instrucao instrucoesCorpoDaFuncao = new Instrucao(ProgramaEmVM.codeDefinitionFunction, null, null);

            if (sequencia.tokens.Contains("{"))
            {
                ProcessadorDeID processadorBloco = null;

                // constroi o bloco de instruções do corpo do método.
                this.BuildBloco(0, sequencia.tokens, ref umMetodoComCorpo.escopo, instrucoesCorpoDaFuncao, ref processadorBloco);

                // o escopo do método é o escopo do bloco de instruções do corpo do método!
                umMetodoComCorpo.escopo = processadorBloco.escopo.Clone();

                umMetodoComCorpo.instrucoesFuncao.AddRange(instrucoesCorpoDaFuncao.blocos[0]);
            }

            // RETIRA OS PARÂMETROS DA FUNÇÃO, o que é logico tambem, pois o escopo da funcao ja foi copiado,paa a funcao, e a variavel parametro nao tem que ficar no escopo principal.
            for (int x = 0; x < parametrosDoMetodo.Count; x++)
                escopo.tabela.RemoveObjeto(parametrosDoMetodo[x].GetNome());



            return instrucoesCorpoDaFuncao;

        }
        protected Instrucao BuildDefinicaoDeFuncao(UmaSequenciaID sequencia, Escopo escopo)
        {

            string tipoRetornoFuncao = null;
            string nomeFuncao = null;
            string acessorFuncao = null;
            if (acessorsValidos.Contains(sequencia.tokens[0]))
            {
                acessorFuncao = sequencia.tokens[0];
                tipoRetornoFuncao = sequencia.tokens[1];
                nomeFuncao = sequencia.tokens[2];
                if (nomeFuncao == "(")
                {
                    nomeFuncao = tipoRetornoFuncao;
                    tipoRetornoFuncao = "void"; // não tem tipo de retorno, o retorno é vazio.
                }
            }
            else
            {

                acessorFuncao = "protected";
                tipoRetornoFuncao = sequencia.tokens[0];
                nomeFuncao = sequencia.tokens[1];

            }
            List<Objeto> parametrosDoMetodo = new List<Objeto>();

            int indexParentesAbre = sequencia.tokens.FindIndex(k => k == "(");
            if (indexParentesAbre == -1)
            {
                UtilTokens.WriteAErrorMensage("funcao sem interface de parametros, parte entre ( e ), exemplo deveria ser FuncaoA(){} e encontrou FuncaoA{}", sequencia.tokens, escopo);
                return null;
            }
            // constroi os parâmetros da definição da função.
            ExtraiParametrosDaFuncao(sequencia, parametrosDoMetodo, indexParentesAbre, escopo);

            // REGISTRA OS PARÂMETROS DA FUNÇÃO, COMO UMA ATRIBUICAO, O QUE É LOGICO POIS A DEFINICAO DE PARAMETROS É UMA ATRIBUICAO!!!
            for (int x = 0; x < parametrosDoMetodo.Count; x++)
                escopo.tabela.GetObjetos().Add(new Objeto("private", parametrosDoMetodo[x].GetTipo(), parametrosDoMetodo[x].GetNome(), null));

            // constroi a função.
            Metodo umaFuncaoComCorpo = new Metodo();
            umaFuncaoComCorpo.nome = nomeFuncao;
            umaFuncaoComCorpo.tipoReturn = tipoRetornoFuncao;
            umaFuncaoComCorpo.acessor = acessorFuncao;
            umaFuncaoComCorpo.escopo = new Escopo(sequencia.tokens);

            if (parametrosDoMetodo.Count > 0)
                umaFuncaoComCorpo.parametrosDaFuncao = parametrosDoMetodo.ToArray();

            umaFuncaoComCorpo.escopo = new Escopo(sequencia.tokens); // cria o escopo da funcao.
            escopo.tabela.RegistraFuncao(umaFuncaoComCorpo);
            umaFuncaoComCorpo.escopo.tabela.RegistraFuncao(umaFuncaoComCorpo); // o escopo da função registra a função!.



            UtilTokens.LinkEscopoPaiEscopoFilhos(escopo, umaFuncaoComCorpo.escopo);  // monta a hierarquia de escopos.



            Instrucao instrucaoDefinicaoDeMetodo = new Instrucao(ProgramaEmVM.codeDefinitionFunction, null, null);


            ProcessadorDeID processadorBloco = null;
            this.BuildBloco(0, sequencia.tokens, ref umaFuncaoComCorpo.escopo, instrucaoDefinicaoDeMetodo, ref processadorBloco); // constroi o bloco de instruções, retornado o bloco de instrucoes, e o processador id do bloco.



            // retira os parâmetros da funcao, do escopo, o que é logico tambem, pois o escopo da funcao ja foi copiado,paa a funcao, e a variavel parametro nao tem que ficar no escopo principal.
            for (int x = 0; x < parametrosDoMetodo.Count; x++)
                escopo.tabela.RemoveObjeto(parametrosDoMetodo[x].GetNome());

            umaFuncaoComCorpo.instrucoesFuncao.AddRange(instrucaoDefinicaoDeMetodo.blocos[0]);
            return instrucaoDefinicaoDeMetodo;

        } // BuildDefinicaoDeFuncao()

        protected Instrucao BuildDefinicaoDeAspecto(UmaSequenciaID sequencia, Escopo escopo)
        {
            /// template: aspecto NameId typeInsertId (TipoOjbeto:string, string, NomeMetodo: string ) { funcaoCorte(Objeto x){}}.
            int indexNameAspect = 1;
            string nomeAspecto = sequencia.tokens[indexNameAspect];

            int indexTypeInsert = sequencia.tokens.IndexOf(nomeAspecto) + 1;
            List<string> tiposInsercao = new List<string>() { "before", "after", "all" };
            if (tiposInsercao.IndexOf(sequencia.tokens[indexTypeInsert]) == -1)
            {
                UtilTokens.WriteAErrorMensage("tipo de insercao invalido, esperado: before, after ou all", sequencia.tokens, escopo);
                return null;
            }

            string typeInserction = sequencia.tokens[indexTypeInsert];

            int indexStartInterface = sequencia.tokens.IndexOf("(");
            if (indexStartInterface == -1)
            {
                UtilTokens.WriteAErrorMensage("instrucao aspecto com erros de sintaxe.", sequencia.tokens, escopo);
                return null;
            }
            List<string> tokensInterface = UtilTokens.GetCodigoEntreOperadores(indexStartInterface, "(", ")", sequencia.tokens);
            if ((tokensInterface == null) || (tokensInterface.Count == 0))
            {
                UtilTokens.WriteAErrorMensage("instrucao aspecto com erros de sintaxe, interface de parametros nao construida corretamente", sequencia.tokens, escopo);
                return null;
            }


            int indexTypeObjectName = tokensInterface.IndexOf("(") + 1;
            if (indexTypeObjectName < 0)
            {
                UtilTokens.WriteAErrorMensage("instrucao aspecto com erros de sintaxe, interface de parametros nao construida corretamente", sequencia.tokens, escopo);
                return null;
            }


            string tipoObjetoAMonitorar = tokensInterface[indexTypeObjectName];

            string metodoAMonitorar = null;

            int indexMethodName = indexTypeObjectName + 2; // +1 do typeObject, +1 do operador virgula.
            if ((indexMethodName >= 2) && (indexMethodName < tokensInterface.Count))
                metodoAMonitorar = tokensInterface[indexMethodName];



            int indexStartInstructionsAspect = sequencia.tokens.IndexOf("{");
            if (indexStartInstructionsAspect == -1)
            {
                UtilTokens.WriteAErrorMensage("instrucao aspecto com erros de sintaxe, sem definicao do bloco de instruções que compoe o aspecto.", sequencia.tokens, escopo);
                return null;
            }


            List<string> tokensDaFuncaoCorte = UtilTokens.GetCodigoEntreOperadores(indexStartInstructionsAspect, "{", "}", sequencia.tokens);
            if ((tokensDaFuncaoCorte == null) || (tokensDaFuncaoCorte.Count == 0))
            {
                UtilTokens.WriteAErrorMensage("instrucao aspecto com erros de sintaxe, erro na descrição do bloco de instruções que compoe o aspecto.", sequencia.tokens, escopo);
                return null;
            }
            tokensDaFuncaoCorte.RemoveAt(0);
            tokensDaFuncaoCorte.RemoveAt(tokensDaFuncaoCorte.Count - 1);




            ProcessadorDeID processador = new ProcessadorDeID(tokensDaFuncaoCorte);
            processador.Compilar();

            Metodo funcaoCorte = null;
            if ((processador.escopo.tabela.GetFuncoes() != null) && (processador.escopo.tabela.GetFuncoes().Count == 1))
            {
                funcaoCorte = processador.escopo.tabela.GetFuncoes()[0];
                if ((funcaoCorte.parametrosDaFuncao == null) || (funcaoCorte.parametrosDaFuncao.Length != 1))
                {
                    UtilTokens.WriteAErrorMensage("a funcao de corte deve conter um parametro somente, e do tipo Objeto.", sequencia.tokens, escopo);
                    return null;
                }
            }
            Random rnd = new Random();
            List<Expressao> expressoesDaInstrucao = new List<Expressao>();
            Objeto obj_caller = new Objeto("public", funcaoCorte.nomeClasse, "objTemp" + rnd.Next(100000), null);
            expressoesDaInstrucao.Add(new ExpressaoChamadaDeMetodo(obj_caller, funcaoCorte, new List<Expressao>()));
            expressoesDaInstrucao.Add(new ExpressaoElemento(tipoObjetoAMonitorar));


            Aspecto aspecto = new Aspecto(nomeAspecto, tipoObjetoAMonitorar, metodoAMonitorar, funcaoCorte, escopo, Aspecto.TypeAlgoritmInsertion.ByObject, typeInserction);
            lng.Aspectos.Add(aspecto);

            return new Instrucao(-1, null, null);

        }

        protected static void ExtraiParametrosDaFuncao(UmaSequenciaID sequencia, List<Objeto> parametrosDoMetodo, int indexParentesAbre, Escopo escopo)
        {
            if (indexParentesAbre != -1)
            {

                int start = indexParentesAbre;
                List<string> tokensParametros = UtilTokens.GetCodigoEntreOperadores(start, "(", ")", sequencia.tokens);
                tokensParametros.RemoveAt(0);
                tokensParametros.RemoveAt(tokensParametros.Count - 1);

                if (tokensParametros.Count > 0)
                {
                    int indexToken = 0;
                    int pilhaInteiropParenteses = 0;

                    while (((indexToken + 1) < tokensParametros.Count) && (tokensParametros[indexToken] != "{"))
                    {

                        // exmemplo do codigo seguinte: funcaoA(), uma funcao sem parmetros.
                        if ((tokensParametros[indexToken] == "(") && (tokensParametros[indexToken + 1] == ")"))
                            break;

                        // o token é um parênteses fecha?
                        if (tokensParametros[indexToken] == ")")
                        {
                            pilhaInteiropParenteses--;
                            if (pilhaInteiropParenteses == 0)
                                break;
                        }
                        //  o  tokens é um parenteses abre?
                        if (tokensParametros[indexToken] == "(") // verifica se 
                        {
                            pilhaInteiropParenteses++;
                        }
                        // inicializa um parâmetro da função/método.
                        Objeto umParametro = new Objeto("private", tokensParametros[indexToken], tokensParametros[indexToken + 1], null, escopo, false);
                        parametrosDoMetodo.Add(umParametro); // adiciona o parametro construido.
                        indexToken += 3; // 1 token para o nome do parametro; 1 token para o tipo do parametro, 1 token para a vigula.

                    } // while
                } // if

            } // if tokenParametros.Count
        }

        protected Instrucao BuildInstrucaoOperadorBinario(UmaSequenciaID sequencia, Escopo escopo)
        {

            /// operador ID ID ( ID ID, ID ID ) prioridade ID meodo ID ;
            if ((sequencia.tokens[0].Equals("operador")) &&
               (lng.IsID(sequencia.tokens[1])) &&
               (lng.IsID(sequencia.tokens[2])) &&
               (sequencia.tokens[3] == "(") &&
               (lng.IsID(sequencia.tokens[4])) &&
               (lng.IsID(sequencia.tokens[5])) &&
               (sequencia.tokens[6] == ",") &&
               (lng.IsID(sequencia.tokens[7])) &&
               (lng.IsID(sequencia.tokens[8])) &&
               (sequencia.tokens[9] == ")") &&
               (sequencia.tokens[10] == "prioridade") &&
               (lng.IsID(sequencia.tokens[11])) &&
               (sequencia.tokens[12] == "metodo") &&
               (lng.IsID(sequencia.tokens[13]) &&
               ((sequencia.tokens[14] == ";"))))
            {
                string nomeClasseOperadorETipoDeRetorno = sequencia.tokens[1];
                string nomeOperador = sequencia.tokens[2];
                string nomeMetodoOperador = sequencia.tokens[13];
                string tipoOperando1 = sequencia.tokens[4];
                string tipoOperando2 = sequencia.tokens[7];

                string nomeOperando1 = sequencia.tokens[5];
                string nomeOperando2 = sequencia.tokens[8];

                List<Metodo> metodos = escopo.tabela.GetFuncao(nomeMetodoOperador).FindAll(k => k.nome.Equals(nomeMetodoOperador));
                Metodo funcaoOPeradorEncontrada = null;
                foreach (Metodo umaFuncaoDeOperador in metodos)
                {
                    if ((umaFuncaoDeOperador.parametrosDaFuncao.Length == 2) && (umaFuncaoDeOperador.tipoReturn.Equals(nomeClasseOperadorETipoDeRetorno)))
                    {
                        funcaoOPeradorEncontrada = umaFuncaoDeOperador;
                        break;
                    }
                }
                if (funcaoOPeradorEncontrada == null)
                {

                    UtilTokens.WriteAErrorMensage("Funcao para Operador nao encontrada, tipos de parametros nao encontrados, ou classe e retorno nao encontrado.", sequencia.tokens, escopo);
                    return null;
                }


                if (RepositorioDeClassesOO.Instance().ExisteClasse(tipoOperando1))
                {

                    UtilTokens.WriteAErrorMensage("Erro na definição do operador binario" + nomeOperador + ", tipo do operando: " + tipoOperando1 + " nao existente", sequencia.tokens, escopo);
                    return null;
                }



                if (RepositorioDeClassesOO.Instance().ExisteClasse(tipoOperando2))
                {

                    UtilTokens.WriteAErrorMensage("Erro na definição do operador binario: " + nomeOperador + ", tipo do operando: " + tipoOperando2 + " nao existente", sequencia.tokens, escopo);
                    return null;
                }


                int prioridade = -1;
                try
                {
                    prioridade = int.Parse(sequencia.tokens[11]);
                    if (prioridade < -1)
                    {

                        UtilTokens.WriteAErrorMensage("prioridade: " + prioridade + " não valida para o operador: " + nomeOperador, sequencia.tokens, escopo);
                        return null;
                    }
                } //try
                catch
                {
                    UtilTokens.WriteAErrorMensage("prioridade: " + sequencia.tokens[11] + " não valida para o operador: " + nomeOperador, sequencia.tokens, escopo);
                    return null;
                } // catch

                List<Expressao> expressaoElementosOperador = new List<Expressao>();
                expressaoElementosOperador.Add(new ExpressaoElemento(nomeClasseOperadorETipoDeRetorno));
                expressaoElementosOperador.Add(new ExpressaoElemento(nomeOperador));

                expressaoElementosOperador.Add(new ExpressaoElemento(tipoOperando1));
                expressaoElementosOperador.Add(new ExpressaoElemento(nomeOperando1));

                expressaoElementosOperador.Add(new ExpressaoElemento(tipoOperando2));
                expressaoElementosOperador.Add(new ExpressaoElemento(nomeOperando2));


                expressaoElementosOperador.Add(new ExpressaoElemento(prioridade.ToString()));

                Metodo fnc = escopo.tabela.GetFuncao(nomeMetodoOperador, nomeClasseOperadorETipoDeRetorno, escopo);
                Random rnd = new Random();
                Objeto obj_caller = new Objeto("private", fnc.nomeClasse, "obj_tmp_" + rnd.Next(1000000), null);
                Objeto obj_parametro1 = new Objeto("private", nomeOperando1, "obj_params_1_" + rnd.Next(100000), null);
                Objeto obj_parametro2 = new Objeto("private", nomeOperando1, "obj_params_2_" + rnd.Next(100000), null);

                ExpressaoObjeto expssParams1 = new ExpressaoObjeto(obj_parametro1);
                ExpressaoObjeto expssParams2 = new ExpressaoObjeto(obj_parametro2);

                expressaoElementosOperador.Add(new ExpressaoChamadaDeMetodo(obj_caller, fnc, new List<Expressao>() { expssParams1, expssParams2 }));

                Objeto operandoA = new Objeto("private", tipoOperando1, nomeOperando1, null, escopo, false);
                Objeto operandoB = new Objeto("private", tipoOperando2, nomeOperando2, null, escopo, false);

                Instrucao instrucaoOperadorBinario = new Instrucao(ProgramaEmVM.codeOperadorBinario, expressaoElementosOperador, new List<List<Instrucao>>());
                Operador opNovo = new Operador(nomeClasseOperadorETipoDeRetorno, nomeOperador, prioridade, new Objeto[] { operandoA, operandoB }, "BINARIO", funcaoOPeradorEncontrada.InfoMethod, escopo);

                escopo.tabela.GetOperadores().Add(opNovo);
                Classe classe = RepositorioDeClassesOO.Instance().GetClasse(nomeClasseOperadorETipoDeRetorno);
                if (classe != null)
                    classe.GetOperadores().Add(opNovo);
                LinguagemOrquidea.operadoresBinarios.Add(opNovo);

                return instrucaoOperadorBinario;
            }
            return null;
        } // DefinicaoDeUmOperadorBinario()

        protected Instrucao BuildInstrucaoOperadorUnario(UmaSequenciaID sequencia, Escopo escopo)
        {



            if ((sequencia.tokens[0] == "operador") &&
                (lng.IsID(sequencia.tokens[1])) &&
                (lng.IsID(sequencia.tokens[2])) &&
                (sequencia.tokens[3] == "(") &&
                (lng.IsID(sequencia.tokens[4])) &&
                (lng.IsID(sequencia.tokens[5])) &&
                (sequencia.tokens[6] == ")") &&
                (sequencia.tokens[7] == "prioridade") &&
                (lng.IsID(sequencia.tokens[8])) &&
                (sequencia.tokens[9] == "metodo") &&
                (lng.IsID(sequencia.tokens[10])) &&
                (sequencia.tokens[11] == ";"))
            {
                string tipoRetornoDoOperador = sequencia.tokens[1];
                if (RepositorioDeClassesOO.Instance().ExisteClasse(tipoRetornoDoOperador))
                {
                    UtilTokens.WriteAErrorMensage("tipo: " + tipoRetornoDoOperador + " de retorno do operador nao existente.", sequencia.tokens, escopo);
                    return null;
                }



                string nomeOperador = sequencia.tokens[2];

                string tipoOperando1 = sequencia.tokens[4];
                string nomeOperando1 = sequencia.tokens[5];
                string nomeDaFuncaoQueImplementaOperador = sequencia.tokens[9];

                // valida a prioridade do operador;
                int prioridade = -100;
                try
                {
                    prioridade = int.Parse(sequencia.tokens[8]);
                } //try
                catch
                {
                    UtilTokens.WriteAErrorMensage("prioridade: " + sequencia.tokens[8] + " nao valida para operador unario: " + nomeOperador, sequencia.tokens, escopo);
                    return null;
                } // catch

                if (prioridade <= -100)
                {
                    UtilTokens.WriteAErrorMensage("prioridade: " + prioridade + "nao valida para o operador unario: " + nomeOperador, sequencia.tokens, escopo);
                    return null;
                }

                // tenta obter uma classe compatível com o tipo de operação (tipo do operador= o tipo do operando1.
                Classe classTipoOperando1 = RepositorioDeClassesOO.Instance().GetClasse(tipoOperando1);
                if (classTipoOperando1 == null)
                {
                    UtilTokens.WriteAErrorMensage("tipo do operando: " + tipoOperando1 + " não existente para o operador unario: " + nomeOperador, sequencia.tokens, escopo);
                    return null;
                } // if

                List<Expressao> expressaoOperador = new List<Expressao>();
                expressaoOperador.Add(new ExpressaoElemento(tipoRetornoDoOperador));
                expressaoOperador.Add(new ExpressaoElemento(nomeOperador));
                expressaoOperador.Add(new ExpressaoElemento(tipoOperando1));
                expressaoOperador.Add(new ExpressaoElemento(nomeOperando1));
                expressaoOperador.Add(new ExpressaoElemento(prioridade.ToString()));
                Random rnd = new Random();
                Objeto obj_parametro = new Objeto("public", tipoOperando1, "obj_" + rnd.Next(100000), null);
                Objeto obj_caller = new Objeto("public", tipoRetornoDoOperador, "obj_caller_" + rnd.Next(10000), null);
                Metodo fncOperadorUnario = escopo.tabela.GetFuncao(nomeDaFuncaoQueImplementaOperador, tipoOperando1, escopo);
                
                expressaoOperador.Add(new ExpressaoChamadaDeMetodo(obj_caller, fncOperadorUnario, new List<Expressao>() { new ExpressaoObjeto(obj_parametro) }));



                Instrucao instrucaoOperadorUnario = new Instrucao(ProgramaEmVM.codeOperadorUnario, expressaoOperador, new List<List<Instrucao>>());

                Objeto operandoA = new Objeto("private", tipoOperando1, nomeOperando1, null, escopo, false);

                Operador opNovo = new Operador(tipoRetornoDoOperador, nomeOperador, prioridade, new Objeto[] { operandoA }, "UNARIO", escopo.tabela.GetFuncao(nomeDaFuncaoQueImplementaOperador, tipoRetornoDoOperador, escopo).InfoMethod, escopo);
                escopo.tabela.GetOperadores().Add(opNovo);

                Classe classe = RepositorioDeClassesOO.Instance().GetClasse(tipoRetornoDoOperador);
                if (classe != null)
                    classe.GetOperadores().Add(opNovo);

                LinguagemOrquidea.operadoresUnarios.Add(opNovo);

                return instrucaoOperadorUnario;

            } // if
            return null;
        } 

   

        private bool IguaisExpressoes(Expressao exp1, Expressao exp2)
        {
            if ((exp1 == null) && (exp2 == null))
                return true;
            if ((exp1 == null) && (exp2 != null))
                return false;
            if ((exp1 != null) && (exp2 == null))
                return false;

            if (exp1.tokens.Count != exp2.tokens.Count)
                return false;

            for (int umToken = 0; umToken < exp1.tokens.Count; umToken++)
                if (exp1.tokens[umToken] != exp2.tokens[umToken])
                    return false;

            return true;
        }

        //______________________________________________________________________________________________________________________






        public class MetodoTratadorOrdenacao
        {
            public MetodoTratador metodo
            {
                get;
                set;
            }

        

            public List<string> tokens
            {
                get;
                set;
            }

            public string patterResumed
            { 
                get;
                set;
            }

            public Regex regex_sequencia
            {
                get;
                set;
            }

            private static TextExpression textExprss;
            public MetodoTratadorOrdenacao(MetodoTratador umMetodo, string patternResumedDaSequencia)
            {
                if (textExprss == null)
                    textExprss = new TextExpression();
                this.metodo = umMetodo;

                this.patterResumed = textExprss.FormaPatternResumed(patternResumedDaSequencia);
                this.regex_sequencia = new Regex(textExprss.FormaExpressaoRegularGenerica(this.patterResumed));
                this.tokens = new Tokens(patternResumedDaSequencia).GetTokens();


            }

            public override string ToString()
            {
                string str = "";
                str += patterResumed;
                return str;
            }

           
            /// <summary>
            /// ordena decrescentemente pelo cumprimento da sequencia do metodo tratador,
            /// e priorizando sequencias que contem termos-chaves de comando.
            /// </summary>
            public class ComparerMetodosTratador : IComparer<MetodoTratadorOrdenacao>
            {
                private static List<string> termos_chaves=new List<string>();

                public ComparerMetodosTratador()
                {
                    if (termos_chaves == null)
                    {
                        termos_chaves = TextExpression.GetTodosTermosChavesIniciais();
                    }
                    termos_chaves.Add(".");    
                }

                private bool ContainsTermoChave(MetodoTratadorOrdenacao x)
                {
                    List<string> tokens = x.tokens;
                    for (int i = 0; i < tokens.Count; i++)
                    {
                        if (termos_chaves.IndexOf(tokens[i]) != -1)
                        {
                            return true;
                        }
                    }
                    return false;
                }
                public int Compare(MetodoTratadorOrdenacao ?x, MetodoTratadorOrdenacao ?y)
                {
                    int c1 = x.patterResumed.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Length;
                    int c2 = y.patterResumed.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Length;

                    

                    if ((ContainsTermoChave(x)) && (!ContainsTermoChave(y)))
                        return -1;
                    if ((!ContainsTermoChave(x)) && (ContainsTermoChave(y)))
                        return +1;
                  

                    if (c1 > c2)
                        return -1;
                    if (c1 < c2)
                        return +1;
                    return 0;
                } // Compare()
            }
        }

        /// <summary>
        /// verifica se um dos tokens da sequencia é um wrapper object.
        /// </summary>
        /// <param name="sequenciaCurrente">sequencia com tokens.</param>
        /// <param name="escopo">contexto da sequencia.</param>
        /// <returns>[true] se há wrapper object na sequencia.</returns>
        private bool HasWrapperObjectInSequence(UmaSequenciaID sequenciaCurrente, Escopo escopo)
        {

            for (int x = 0; x < sequenciaCurrente.tokens.Count; x++)
            {
                string nomeObjeto = sequenciaCurrente.tokens[x];
                if (escopo.tabela.GetObjeto(nomeObjeto, escopo) != null)
                {
                    if (escopo.tabela.GetObjeto(nomeObjeto, escopo).isWrapperObject)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public new class Testes : SuiteClasseTestes
        {
            

            public Testes() : base("testes para classe ProcessadorID")
            {
            }


            public void TestePromptWrite(AssercaoSuiteClasse assercao)
            {
                Expressao.InitHeaders("");
                LinguagemOrquidea lng = LinguagemOrquidea.Instance();
                try
                {
                    Classe prompt= RepositorioDeClassesOO.Instance().GetClasse("Prompt");
                    List<Metodo> fnc = prompt.GetMetodo("xWrite");
                    for (int i=0;i<fnc.Count; i++)
                    {
                        if (fnc[i].parametrosDaFuncao.Length == 2)
                        {
                            assercao.IsTrue(fnc[i].parametrosDaFuncao[1].isMultArgument);
                            return;
                        }
                    }
                    
                }
                catch (Exception ex)
                {
                    string codeMsg= ex.Message; 
                    assercao.IsTrue(false, "TESTE FALHOU");
                }
            }



            public void TesteExpressaoValida(AssercaoSuiteClasse assercao)
            {
                string pathProgramaFatorialRecursivo = "programasTestes\\programaFatorialRecursivo.txt";
                ParserAFile parser = new ParserAFile(pathProgramaFatorialRecursivo);

                ProcessadorDeID compilador = new ProcessadorDeID(parser.GetTokens());
                compilador.Compilar();


            }    
            public void TesteIF_Else(AssercaoSuiteClasse assercao)
            {
                string codigo = "int a=1; int b=5; if (a>b){a=3*b;} else {a=6;}";
                ProcessadorDeID processador = new ProcessadorDeID(codigo);
                processador.Compilar();

                try
                {
                    assercao.IsTrue(
                        processador.instrucoes.Count == 3 &&
                        processador.instrucoes[2].code == ProgramaEmVM.codeIfElse, codigo);
                }
                catch (Exception ex)
                {
                    string codeMessage = ex.Message;
                    assercao.IsTrue(false, "TESTE FALHOU");
                }
            }

            public void TesteCasesOfUse_2(AssercaoSuiteClasse assercao)
            {
                // sintaxe: casesOfUse id : ( case operador exprss ) : {} (case operador exprss): {}...
                string codigo = "int b=1; int a=5; casesOfUse b: (case  < a): { b = 1};";
                ProcessadorDeID processador = new ProcessadorDeID(codigo);
                processador.Compilar();
                try
                {
                    assercao.IsTrue(
                        processador.instrucoes.Count == 3 &&
                        processador.instrucoes[2].code == ProgramaEmVM.codeCasesOfUse, codigo);
                }
                catch (Exception ex)
                {
                    string codeMessage = ex.Message;
                }

            }


    
            public void TesteInstanciacaoWrapperObjects(AssercaoSuiteClasse assercao)
            {
                string codigoClasse1 = "public class classeWrapper { public int metodoB(){ int[] vetor1[15]; vetor1[0]=1; }};";
                ProcessadorDeID compilador= new ProcessadorDeID(codigoClasse1);
                compilador.Compilar();
                try
                {
                    List<Metodo> metodos = RepositorioDeClassesOO.Instance().GetClasse("classeWrapper").GetMetodos();
                    assercao.IsTrue(metodos[0].instrucoesFuncao.Count == 2, codigoClasse1);
                    assercao.IsTrue(metodos[0].instrucoesFuncao[0].code == 25, codigoClasse1);
                    assercao.IsTrue(metodos[0].instrucoesFuncao[1].code == ProgramaEmVM.codeExpressionValid, codigoClasse1);

                }
                catch (Exception e) 
                {
                    string msgError = e.Message;
                    assercao.IsTrue(false, "TESTE FALHOU");
                }
            }

            public void TesteIfElse(AssercaoSuiteClasse assercao)
            {
                string pathFile = @"programasTestes\programaFatorialRecursivo.txt";
                ParserAFile parser = new ParserAFile(pathFile);
                ProcessadorDeID compilador = new ProcessadorDeID(parser.GetTokens());
                compilador.Compilar();

                List<Metodo> metodos = RepositorioDeClassesOO.Instance().GetClasse("classeFatorial").metodos; 

                try
                {
                    assercao.IsTrue(metodos[1].instrucoesFuncao.Count == 1);
                }
                catch (Exception e)
                {
                    string errorMessage = e.Message;
                    assercao.IsTrue(false, "TESTE FALHOU");
                }
                
            }

            public void TestesFuncoes(AssercaoSuiteClasse assercao)
            {
                string codigoClasseA = "public class classeA { public classeA() { int b=metodoA();  };  public int metodoA(){ int x =1; x=x+1;}; }; int a=1; int b=2;";
                ProcessadorDeID compilador = new ProcessadorDeID(codigoClasseA);
                compilador.Compilar();


                Classe classe1 = RepositorioDeClassesOO.Instance().GetClasse("classeA");
                List<Metodo> metodosClasse1 = classe1.metodos;

                try
                {
                    assercao.IsTrue(metodosClasse1[0].instrucoesFuncao.Count == 1 && metodosClasse1[0].instrucoesFuncao[0].expressoes.Count == 1);
                }catch(Exception e)
                {
                    string messageError = e.Message;
                    assercao.IsTrue(false, "TESTE FALHOU");
                }


            }


            public void TestePrintTratadores(AssercaoSuiteClasse assercao)
            {
                string codigoClasseA = "public class classeA { public classeA() { int b=6;  };  public int metodoA(){ int x =1; x=x+1;}; }; int a=1; int b=2;";
                ProcessadorDeID compilador = new ProcessadorDeID(codigoClasseA);
                compilador.Compilar();

                List<MetodoTratadorOrdenacao> handlers = compilador.GetHandlers();
                for (int i = 0; i < handlers.Count; i++)
                {
                    System.Console.WriteLine((i + 1).ToString() + "-  " + handlers[i].patterResumed + "    regex: " + handlers[i].regex_sequencia.ToString());
                }

                System.Console.ReadLine();

            }

            public void TesteCompilarCorpoDeMetodos(AssercaoSuiteClasse assercao)
            {
                string codigoClasseA = "public class classeA { public classeA() { int b=6;  };  public int metodoA(){ int x =1; x=x+1;}; }; int a=1; int b=2;";
                ProcessadorDeID compilador = new ProcessadorDeID(codigoClasseA);
                compilador.Compilar();

                try
                {
                    Instrucao instrucaoMetodo1 = RepositorioDeClassesOO.Instance().GetClasse("classeA").GetMetodos()[0].instrucoesFuncao[0];
                    assercao.IsTrue(RepositorioDeClassesOO.Instance().GetClasse("classeA").GetMetodos()[0].instrucoesFuncao.Count == 1, codigoClasseA);
                    assercao.IsTrue(RepositorioDeClassesOO.Instance().GetClasse("classeA").GetMetodos()[1].instrucoesFuncao.Count == 2, codigoClasseA);

                }
                catch (Exception e)
                {
                    string codeMessage = e.Message;
                    assercao.IsTrue(false, "Teste Falhou");
                }



            }


            public void TestesPequenosProgramas(AssercaoSuiteClasse assercao)
            {
                string pathProgram = @"programasTestes\programContagens.txt";
                ParserAFile program = new ParserAFile(pathProgram);



                ProcessadorDeID compilador = new ProcessadorDeID(program.GetTokens());
                compilador.Compilar();

                try
                {
                    assercao.IsTrue(true, "programa rodado sem erros fatais.");
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }




            }

   
            public void TesteInstanciacaoVariavel(AssercaoSuiteClasse assercao)
            {
                string codigo_0_1 = "int x=1;";
                string codigo_0_2 = "int x=1; int y=5;";
                string codigo0_3 = "int x=1+1;int y=5; x=x+y;";

                Escopo escopo1 = new Escopo(codigo_0_1 + codigo_0_2 + codigo0_3 + codigo0_3);

                ProcessadorDeID compilador = new ProcessadorDeID(codigo_0_1+ codigo_0_2);
                compilador.Compilar();

                try
                {
                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("x", compilador.escopo) != null, codigo_0_1);
                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("y", compilador.escopo) != null, codigo_0_2);
                }
                catch
                {
                    assercao.IsTrue(false, "falha na compilacao.");
                }
            }


 
            public void TesteCompilarUmaClasse(AssercaoSuiteClasse assercao)
            {

        

                string codigoClasseA = "public class classeA { public classeA() { };  public int metodoA(){}; }";
                ProcessadorDeID compilador = new ProcessadorDeID(codigoClasseA);
                compilador.Compilar();


                try
                {
                   
                    assercao.IsTrue(RepositorioDeClassesOO.Instance().GetClasse("classeA").GetMetodos().Count == 2);

                }
                catch (Exception e)
                {
                    string codeMessage = e.Message;
                    assercao.IsTrue(false, "Teste Falhou");
                }
                


            }




            public void Atribuicao(AssercaoSuiteClasse assercao)
            {


                string codigo = "int a=2; int b=1; int c= 3*b;";
            
                ProcessadorDeID processador = new ProcessadorDeID(codigo);
                processador.Compilar();

                try
                {
                    assercao.IsTrue(processador.escopo.tabela.GetObjeto("a", processador.escopo) != null, codigo);
                    assercao.IsTrue(processador.escopo.tabela.GetObjeto("b", processador.escopo) != null, codigo);
                    assercao.IsTrue(processador.escopo.tabela.GetObjeto("c", processador.escopo) != null, codigo);

                }
                catch (Exception e)
                {
                    string codeMessage = e.Message;
                    assercao.IsTrue(false, "Teste Falhou");
                }
                
            }
            

            public void TesteCompilacaoPropriedade(AssercaoSuiteClasse assercao)
            {

                Expressao.headers = null;

                // codigo das classes do cenario de teste.
                string codigoClasseE = "public class classeE { public classeE() { }; public int propriedade1 ;  }";

                List<string> codigoTeste = new Tokens(new List<string>() { codigoClasseE }).GetTokens();


                ProcessadorDeID compilador = new ProcessadorDeID(codigoTeste);
                compilador.Compilar();

                try
                {
                    assercao.IsTrue(
                       RepositorioDeClassesOO.Instance().GetClasse("classeE").GetPropriedade("propriedade1") != null, codigoClasseE);

                }
                catch (Exception e)
                {
                    string msgError= e.Message;
                    assercao.IsTrue(false, "TESTE FALHOU");
                }

            }




            public void TesteCasesOfUse(AssercaoSuiteClasse assercao)
            {
               
                // sintaxe: casesOfUse id : ( case operador exprss ) : {} (case operador exprss): {}...
                string codigo = "int b=1; int a=5; casesOfUse a: (case < b): { a = 2; };";
                ProcessadorDeID processador = new ProcessadorDeID(codigo);
                processador.Compilar();
                try
                {
                    assercao.IsTrue(processador.instrucoes.Count >= 3, codigo);
                }
                catch(Exception ex)
                {
                    string codeMessage = ex.Message;
                    assercao.IsTrue(false, "TESTE FALHOU.");
                }
                
            }


   


            public void TesteChamadaDeObjetoImportado(AssercaoSuiteClasse assercao)
            {
                CultureInfo.CurrentCulture = CultureInfo.CurrentCulture; // para compatibilizar os numeros float como: 1.0.

                /// ID ID = create ( ID , ID ) --> exemplo: int m= create(1,1).
                /// importer ( nomeAssembly).

                string codigoImportar = "importer (ParserLinguagemOrquidea.exe);";
                ProcessadorDeID compilador = new ProcessadorDeID(codigoImportar);
                compilador.Compilar();

                try
                {
                    assercao.IsTrue(RepositorioDeClassesOO.Instance().GetClasses().Count > 8, codigoImportar);
                }
                catch (Exception ex)
                {
                    string codeMessage = ex.Message;
                    assercao.IsTrue(false, "TESTE FALHOU.");
                }
                
            }

            public void TesteIF(AssercaoSuiteClasse assercao)
            {
                string codigo = "int a=1; int b=5; if (a<b){a=3*b;}";
                ProcessadorDeID compilador = new ProcessadorDeID(codigo);

                compilador.Compilar();

                try
                {
                    assercao.IsTrue(
                        compilador.instrucoes.Count == 3 &&
                        compilador.instrucoes[2].code == ProgramaEmVM.codeIfElse, codigo);
                }
                catch(Exception e)
                {
                    string codeMessage = e.Message;
                    assercao.IsTrue(false, "TESTE FALHOU");
                }
                




            }


            public void TesteWhile(AssercaoSuiteClasse assercao)
            {
                string codigo = "int x=1; int dx=5; while (x<=4){dx=dx+1; x=x+1;}";
                ProcessadorDeID processador = new ProcessadorDeID(codigo);
                processador.Compilar();

                try
                {
                    assercao.IsTrue(
                        processador.instrucoes.Count == 3 &&
                        processador.instrucoes[2].code == ProgramaEmVM.codeWhile, codigo);
                }
                catch (Exception ex)
                {
                    string codeMessage = ex.Message;
                    assercao.IsTrue(false, "TESTE FALHOU");
                }
                
            }






            public void TesteFor(AssercaoSuiteClasse assercao)
            {
                string codigo = "int a=0; int b=5; for (int x=0;x< 3;x++) {a= a+ x;};";

                ProcessadorDeID processador = new ProcessadorDeID(codigo);
                processador.Compilar();

                try
                {
                    assercao.IsTrue(
                    processador.instrucoes[2].expressoes.Count == 3 &&
                    processador.instrucoes[1].expressoes.Count == 1 &&
                    processador.instrucoes[0].expressoes.Count == 1, codigo);
                }
                catch (Exception ex)
                {
                    string codeMessage = ex.Message;
                    assercao.IsTrue(false, "TESTE FALHOU");
                }
                

            }

        }

    } // class ProcessadorDeID

} // namespace

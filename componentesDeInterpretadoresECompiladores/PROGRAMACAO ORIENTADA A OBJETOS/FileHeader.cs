using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using System.Text;
using System.Threading.Tasks;

using parser;
using parser.ProgramacaoOrentadaAObjetos;
using ParserLinguagemOrquidea.Wrappers;
using Wrappers;

namespace parser
{
    public class FileHeader
    {



        /// <summary>
        /// lista de erros encontrada na compilacao do codigo de programador.
        /// </summary>
        public List<string> errorsFound = new List<string>();



        /// <summary>
        /// contem definicoes de wrapper data objects.
        /// </summary>
        public static List<WrapperData> wrapperDefinition;

        /// <summary>
        /// contem as definicoes de classes extraidas.
        /// </summary>
        public List<HeaderClass> cabecalhoDeClasses = new List<HeaderClass>();

        public FileHeader()
        {
            this.cabecalhoDeClasses = new List<HeaderClass>();
            FileHeader.wrapperDefinition = new List<WrapperData>() {
                new WrapperDataVector(),
                new WrapperDataMatriz(),
                new WrapperDataDictionaryText(),
                new WrapperDataJaggedArray()};

        }


        public void ExtractCabecalhoDeClasses(List<string> all_tokens)
        {
            int offset = 0;
            while (offset < all_tokens.Count)
            {



                int indexTokenClass = all_tokens.IndexOf("class", offset);
                if (indexTokenClass == -1)
                {
                    // fim de processamento de classes, nao encontrou mais tokens de identificacao de classe.
                    return;
                }

                int indexCorpoClasse = all_tokens.IndexOf("{", offset);
                if (indexCorpoClasse == -1)
                {

                    errorsFound.Add("sintaxe error: body tokens class not found.");
                    return;

                }
                string nomeClasse = "";
                if (indexTokenClass + 1 < all_tokens.Count)
                {
                    nomeClasse = all_tokens[indexTokenClass + 1];
                }
                else
                {
                    errorsFound.Add("sintaxe error: class namme not found.");
                    return;
                }



                if ((all_tokens != null) && (all_tokens.Count > 0) && (offset < all_tokens.Count)) 
                {

                    HeaderClass headerClasse = new HeaderClass(all_tokens, nomeClasse, offset);
                    if (!headerClasse.hasErrors)
                    {

                        if ((headerClasse.tokensDaClasse != null) && (headerClasse.tokensDaClasse.Count > 0))
                        {
                            offset += headerClasse.tokensDaClasse.Count;
                        }
                        else
                        {
                            offset++; // nao entrar em loop infinito.
                        }
                        cabecalhoDeClasses.Add(headerClasse);

                    }
                }
            }

            this.MetodoxWrite();
        }
        public void ExtractCabecalhoDeClasses(string codigo)
        {
            List<string> tokensDoCodigo = new Tokens(codigo).GetTokens();
            ExtractCabecalhoDeClasses(tokensDoCodigo);
        }


        /// <summary>
        /// caso singular de acerto de parâmetros.
        /// </summary>
        public void MetodoxWrite()
        {
            HeaderClass classeHeader = this.cabecalhoDeClasses.Find(k => k.nomeClasse == "Prompt");
            if (classeHeader != null)
            {

                HeaderMethod funcoesHeaderDaClasse = classeHeader.methods.Find(k => k.name == "xWriter");
                if (funcoesHeaderDaClasse != null)
                {
                    classeHeader.methods.Remove(funcoesHeaderDaClasse);
                    funcoesHeaderDaClasse.parameters[1].isMultArgument = true;
                    classeHeader.methods.Add(funcoesHeaderDaClasse);

                }

            }
        }

        /// <summary>
        /// extrai header de classes com o codigo orquidea.
        /// </summary>
        /// <param name="classesDoRepositorio">classes armazendas.</param>
        /// <param name="headersClasse">headers de classes a serem extraidos.</param>
        public void ExtractHeadersClassFromClassesOrquidea(List<Classe> classesDoRepositorio, List<HeaderClass> headersClasse)
        {
            cabecalhoDeClasses = new List<HeaderClass>();
            int indexOffseet = 0;
            if (classesDoRepositorio != null)
                for (int x = 0; x < classesDoRepositorio.Count; x++)
                {

                    Classe umaClasse = classesDoRepositorio[x];
                    HeaderClass umHeaderClasse = new HeaderClass(umaClasse.tokensDaClasse, umaClasse.nome, indexOffseet);
                    string nomeClasse = umaClasse.nome;


                    if (umaClasse.GetPropriedades() != null)
                    {
                        List<Objeto> property = umaClasse.GetPropriedades();


                        for (int i = 0; i < property.Count; i++)
                        {


                            // instancia um header property e inicializa seus dados.
                            HeaderProperty headerPropriedade = new HeaderProperty();


                            headerPropriedade.acessor = property[i].GetAcessor();
                            headerPropriedade.className = property[i].GetTipo();
                            headerPropriedade.name = property[i].GetNome();
                            headerPropriedade.tokens = new List<string>();
                      
                            umHeaderClasse.properties.Add(headerPropriedade);

                        }
                    }
                    if (umaClasse.GetMetodos() != null)
                    {

                        List<Metodo> methods = umaClasse.GetMetodos();

                        for (int i = 0; i < methods.Count; i++)
                        {
                            HeaderMethod headerMetodo = new HeaderMethod(nomeClasse, methods[i].nome);


                            headerMetodo.acessor = methods[i].acessor;
                            headerMetodo.className = umaClasse.nome;
                            headerMetodo.name = methods[i].nome;
                            headerMetodo.typeReturn = methods[i].tipoReturn;

                            ExtractParametersFromClass(methods[i], headerMetodo);

                            headerMetodo.tokens = new List<string>();
                          

                            umHeaderClasse.methods.Add(headerMetodo);
                        }
                    }
                    if (umaClasse.GetOperadores() != null)
                    {
                        List<Operador> operators = umaClasse.GetOperadores();

                        for (int i = 0; i < operators.Count; i++)
                        {
                            HeaderOperator headerOperador = new HeaderOperator(operators[i].nomeClasse);
                            headerOperador.className = operators[i].nomeClasse;
                            headerOperador.name = operators[i].nome;



                            if (operators[i].parametrosDaFuncao.Length == 2)
                            {
                                headerOperador.tipoDoOperador = HeaderOperator.typeOperator.binary;
                            }
                            else
                            if (operators[i].parametrosDaFuncao.Length == 1)
                            {
                                // a distinção entre operador unario pre e pos é determinado na colocação do operador na expressão em que está.
                                headerOperador.tipoDoOperador = HeaderOperator.typeOperator.unary_pos;
                            }
                            else
                            {
                                headerOperador.tipoDoOperador = HeaderOperator.typeOperator.binary;
                            }






                            Objeto[] parametrosMetodo = operators[i].parametrosDaFuncao;

                            if (parametrosMetodo != null)
                                for (int j = 0; j < parametrosMetodo.Length; j++)
                                {
                                    string umOperandoTipo = parametrosMetodo[j].GetTipo();
                                    headerOperador.operands.Add(umOperandoTipo);
                                }

                            umHeaderClasse.operators.Add(headerOperador);

                        }

                    }


                    headersClasse.Add(umHeaderClasse);
                    cabecalhoDeClasses.Add(umHeaderClasse);
                }
        }

        /// <summary>
        /// extrai parametros de uma classe base orquidea.
        /// </summary>
        /// <param name="metodo">metodo com parametros.</param>
        /// <param name="headerMetodo">header para incluir os parametros.</param>
        private static void ExtractParametersFromClass(Metodo metodo, HeaderMethod headerMetodo)
        {

            if (metodo.parametrosDaFuncao != null)
            {
                List<Objeto> parametros = metodo.parametrosDaFuncao.ToList<Objeto>();
                if (parametros != null)
                    for (int k = 0; k < parametros.Count; k++)
                    {
                        string nomeParametro = parametros[k].GetNome();
                        string tipoParametro = parametros[k].GetTipo();
                        string acessor = parametros[k].GetAcessor();

                        HeaderProperty propertyParameter = new HeaderProperty();
                        propertyParameter.acessor = acessor;
                        propertyParameter.className = tipoParametro;
                        propertyParameter.name = nomeParametro;

                        headerMetodo.parameters.Add(propertyParameter);
                    }
            }
            else
                headerMetodo.parameters = new List<HeaderProperty>();
        }


        private bool IsAcessor(string token)
        {
            return token == "public" || token == "private" || token == "protected";
        }

        public class Testes : SuiteClasseTestes
        {
            public Testes() : base("testes classe file headers")
            {

            
            
            
            }
            public void TesteFixarParametrosDeMetodo(AssercaoSuiteClasse assercao)
            {
                string codigoClasseA = "public class classeA {public int variavel1=5; public int variavel2;  public int metodoB(int x, int y) {int b=3;} ; public classeA() { int b=2;};  }";
                List<string> tokensClasse = new Tokens(new List<string>() { codigoClasseA }).GetTokens();

                FileHeader headers = new FileHeader();
                headers.ExtractCabecalhoDeClasses(tokensClasse);

                try
                {
                    assercao.IsTrue(true, "execucao de extracao de headers class feito sem erros fatais.");
                    assercao.IsTrue(headers.cabecalhoDeClasses[0].methods.Count == 2);

                    assercao.IsTrue(headers.cabecalhoDeClasses[0].properties.Count == 2);
                    assercao.IsTrue(headers.cabecalhoDeClasses[0].methods[0].parameters.Count == 2);

                    assercao.IsTrue(headers.cabecalhoDeClasses[0].methods[1].name == "classeA");
                    assercao.IsTrue(headers.cabecalhoDeClasses[0].methods[1].parameters.Count == 0);


                    assercao.IsTrue(headers.cabecalhoDeClasses[0].properties[0].name == "variavel1");
                    assercao.IsTrue(headers.cabecalhoDeClasses[0].properties[1].name == "variavel2");

                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }


            }
            public void TesteParametroMultiArgumento2(AssercaoSuiteClasse assercao)
            {
                string codigoClasse = "public class classeA { public int propriedadeA = 1;  public classeA(){ int x=1; } ;public int metodoB(double x, ! int[] y){ return 5;} ;};";
                FileHeader headers = new FileHeader();
                headers.ExtractCabecalhoDeClasses(codigoClasse);

                try
                {
                    assercao.IsTrue(headers.cabecalhoDeClasses[0].methods[1].parameters[1].isMultArgument);
                }
                catch (Exception ex)
                {
                    string erroMessage = ex.Message;
                    assercao.IsTrue(false, "TESTE FALHOU");
                }
            }

            public void TesteMetodosParametrosComChamadaDeFuncao(AssercaoSuiteClasse assercao)
            {
                string codigo_classeA_0_0 = "public class classeA { public int propriedadeA; public int metodoB(int funcao(int x), int a) { funcao(1);} ; public classeA(){int x=1; };  public int metodoA(int y){int x=2;}; };";
                FileHeader fileHeader = new FileHeader();
                fileHeader.ExtractCabecalhoDeClasses(codigo_classeA_0_0);

                try
                {
                    assercao.IsTrue(fileHeader.cabecalhoDeClasses[0].methods[0].parameters[0].name == "funcao");
                }
                catch(Exception ex)
                {
                    string codeMsg = ex.Message;
                    assercao.IsTrue(false, "TESTE FALHOU");
                }
            }

            public void TesteMuitasClassesLinguagemOrquidea(AssercaoSuiteClasse assercao)
            {

             string codigoClasseX = "public class classeX { public classeX() { int y; }  public int metodoX(int x, int y) { int x; }; };";
              string codigoClasseA = "public class classeA { public classeA() { int x=1; }  public int metodoA(){ int y= 1; }; };";



               string codigoClasseC = "public class classeC { public int propriedadeC; public classeC() { int x =0; }  };";
                string codigoClasseD = "public class classeD {  public int propriedadeD;  public classeD() { int y=0; } };";


                //string code = codigoClasseX + codigoClasseA + codigoClasseC+ codigoClasseD;

                FileHeader fileHeader = new FileHeader();
                fileHeader.ExtractCabecalhoDeClasses(codigoClasseX + codigoClasseA + codigoClasseD+ codigoClasseC);


                try
                {
                    assercao.IsTrue(fileHeader.cabecalhoDeClasses.Count == 4);
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    assercao.IsTrue(false, "TESTE FALHOU");
                }
                
            }
            public void TesteParameterMethod(AssercaoSuiteClasse assercao)
            {
                string parameterMethod = "int funcao5(int x)";
                string classeTeste = "public class classeMethodParams{ public int x=1; public int metodoB("
                    + parameterMethod + ",int y){int b=8;};}";

                FileHeader fileHeader = new FileHeader();
                fileHeader.ExtractCabecalhoDeClasses(classeTeste);

                try
                {
                    assercao.IsTrue(true, "feito sem erros fatais");
                }
                catch (Exception e)
                {
                    string msgError = e.Message;

                }

            }

            public void TesteInstrucoesForaDeClasses(AssercaoSuiteClasse assercao)
            {
                string pathProgramaFatorialRecursivo = "programasTestes\\programaFatorialRecursivo.txt";
                ParserAFile parser = new ParserAFile(pathProgramaFatorialRecursivo);
                List<string> tokensTeste = parser.GetTokens();

                try
                {
                    FileHeader headers = new FileHeader();
                    headers.ExtractCabecalhoDeClasses(tokensTeste);

                    assercao.IsTrue(headers.cabecalhoDeClasses[0].methods.Count == 2);
                }
                catch (Exception ex)
                {
                    string messageError = ex.Message;
                    assercao.IsTrue(false, "TESTE FALHOU");
                }


            }

            public void TesteTokensDoMetodo(AssercaoSuiteClasse assercao)
            {
                string codigoClasse = "public class classeA { protected int propriedadeA = 1;  protected int metodoB(double x, int y){ return 5;}; public classeA(){ int x=1; }};";
                FileHeader headers = new FileHeader();
                headers.ExtractCabecalhoDeClasses(codigoClasse);
                try
                {
                    assercao.IsTrue(headers.cabecalhoDeClasses[0].methods[0].tokens.Count== 3, codigoClasse);
                    assercao.IsTrue(headers.cabecalhoDeClasses[0].methods[1].tokens.Count == 5, codigoClasse);
                    
                }
                catch (Exception ex)
                {
                    string erroMessage = ex.Message;
                    assercao.IsTrue(false, "TESTE FALHOU");
                }
            }
            public void TesteParametroVector(AssercaoSuiteClasse assercao)
            {
                string codigoClasse = "public class classeA { public int propriedadeA = 1 ; public int metodoB( DictionaryText d1{ string }, ! Vector y, JaggedArray j1, Matriz m1){ return 5;} ;};";
                FileHeader headers = new FileHeader();
                headers.ExtractCabecalhoDeClasses(codigoClasse);

                try
                {
                    assercao.IsTrue(headers.cabecalhoDeClasses[0].methods[0].parameters[0].tipoElemento == "string");
                    assercao.IsTrue(headers.cabecalhoDeClasses[0].methods[0].parameters[1].isMultArgument);
                    assercao.IsTrue(headers.cabecalhoDeClasses[0].methods[0].parameters[1].tipoElemento == "Object");
                    assercao.IsTrue(headers.cabecalhoDeClasses[0].methods[0].parameters[2].tipoElemento == "Object");
                    assercao.IsTrue(headers.cabecalhoDeClasses[0].methods[0].parameters[3].tipoElemento== "double");  
                }
                catch (Exception ex)
                {
                    string erroMessage = ex.Message;
                    assercao.IsTrue(false, "TESTE FALHOU");
                }
            }




            public void TesteParametroMultiArgumento(AssercaoSuiteClasse assercao)
            {
                string codigo = "public class classeA { public int propriedadeA;  public classeA(){ int x=2;} public int metodoB(double x, ! int [ ]  y) {Prompt.sWrite(\"HelloWorld\");}}";
                FileHeader headers = new FileHeader();
                headers.ExtractCabecalhoDeClasses(codigo);

                try
                {
                    assercao.IsTrue(
                        headers.cabecalhoDeClasses[0].methods[1].parameters[1].isMultArgument &&
                        ((HeaderProperty)headers.cabecalhoDeClasses[0].methods[1].parameters[1]).tipoElemento == "int", codigo);
                }
                catch
                {
                    assercao.IsTrue(false, "teste falhou.");
                }
            }


            public void TestePropriedadeProtected(AssercaoSuiteClasse assercao)
            {
                string codigoClasse = "public class classeA { protected int propriedadeA = 1;  protected int metodoB(double x, ! int[] y){ return 5;}; public classeA(){ int x=1; }};";
                FileHeader headers = new FileHeader();
                headers.ExtractCabecalhoDeClasses(codigoClasse);
                try
                {
                    assercao.IsTrue(headers.cabecalhoDeClasses[0].properties[0].acessor == "protected", codigoClasse);
                    assercao.IsTrue(headers.cabecalhoDeClasses[0].methods[1].acessor == "protected", codigoClasse);
                }
                catch (Exception ex)
                {
                    string erroMessage = ex.Message;
                    assercao.IsTrue(false, "TESTE FALHOU");
                }
            }

            public void TestExtractionHeadersOfClassesRepository(AssercaoSuiteClasse assercao)
            {
                List<Classe> classesBasicas = LinguagemOrquidea.Instance().GetClasses();
                List<HeaderClass> headersExtraidos = new List<HeaderClass>();

                try
                {
                    FileHeader fileHeader = new FileHeader();
                    fileHeader.ExtractHeadersClassFromClassesOrquidea(classesBasicas, headersExtraidos);
                    assercao.IsTrue(fileHeader.cabecalhoDeClasses.Count > 5);
                }
                catch (Exception e)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + e.Message);
                }

            }

            public void TesteMetodoParametro(AssercaoSuiteClasse assercao)
            {
                string codigoClasseA = "public class classeA {public int metodoB(int metodoParametro(int x, int y)) {} ; }";
                List<string> tokensClasse = new Tokens(new List<string>() { codigoClasseA }).GetTokens();

                FileHeader headers = new FileHeader();
                headers.ExtractCabecalhoDeClasses(tokensClasse);

                try
                {
                    assercao.IsTrue(headers.cabecalhoDeClasses[0].methods[0].parameters[0].name == "metodoParametro", codigoClasseA);
                }
                catch (Exception e)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + e.Message);
                }

            }
            
            public void TesteParametrosPropriedadesWrapperObject(AssercaoSuiteClasse assercao)
            {
                // wrapper object Vector: id[] id (como parametro).

                string codigoClasseA = "public class classeA {" +
                 "public int metodoB(int[] x, ! int y) {};" + "public int[] variavel5;" + "public Vector variavel1; " + " }";
                List<string> allTokens = new Tokens(new List<string>() { codigoClasseA }).GetTokens();
                FileHeader header2 = new FileHeader();
                header2.ExtractCabecalhoDeClasses(allTokens);

                try
                {
                    assercao.IsTrue(header2.cabecalhoDeClasses[0].properties[0].className == "Vector", codigoClasseA);
                    assercao.IsTrue(header2.cabecalhoDeClasses[0].methods[0].parameters[0].className == "Vector", codigoClasseA);
                    assercao.IsTrue(header2.cabecalhoDeClasses[0].methods[0].name == "metodoB", codigoClasseA);
                }
                catch (Exception e)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + e.Message);
                }
            }


            public void TesteComposicaoHerancaDeseranca(AssercaoSuiteClasse assercao)
            {
                string codigoClasse = "public class classeA: + classeB, -classeC { public int variavel1=5; " + "public int metodoB(int x, int y) {} ; }";

                List<string> allTokens = new Tokens(new List<string>() { codigoClasse }).GetTokens();
                FileHeader header2= new FileHeader();
                header2.ExtractCabecalhoDeClasses(allTokens);
                try
                {
                    assercao.IsTrue((header2.cabecalhoDeClasses[0].heranca[0] == "classeB") && (header2.cabecalhoDeClasses[0].deseranca[0] == "classeC"), codigoClasse);
                }
                catch(Exception e)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + e.Message);
                }
            }


  
            public void TesteComposicaoOperadores(AssercaoSuiteClasse assercao)
            {
                // sintaxe de operador:
                //  operador ID ID ( ID , ID) prioridade ID;
                string codigoClasseA = "public class classeA {" +
                    "operador int mais(int,int) prioridade 1 {int x=5; } ; "+"public int variavel1=5; " + "public int metodoB(int x, int y) {} ; }";

                List<string> tokensClasse = new Tokens(new List<string>() { codigoClasseA }).GetTokens();

                FileHeader headers = new FileHeader();
                headers.ExtractCabecalhoDeClasses(tokensClasse);
            }

     


   

     

    


            public void TestePropriedadesEstaticas(AssercaoSuiteClasse assercao)
            {
                string codigo = "public class classeA { public static int propriedadeA;  public classeA(){ } public int metodoB(){Prompt.sWrite(\"HelloWorld\");}}";
                FileHeader headers = new FileHeader();
                headers.ExtractCabecalhoDeClasses(codigo);

                try
                {
                    assercao.IsTrue(headers.cabecalhoDeClasses[0].properties[0].isStatic == true, codigo);
                }
                catch (Exception e)
                {
                    assercao.IsTrue(false, "falha no teste:" + e.Message);
                }
            }


  


            public void TestesGetHeadersConstrutor(AssercaoSuiteClasse assercao)
            {
                string codigoTeste = "public class classeA { public classeA ( int x ) { x = x + 1 } }";
                List<string> tokensTeste = new Tokens(new List<string>() { codigoTeste }).GetTokens();

                FileHeader arquivoCabecalho = new FileHeader();
                arquivoCabecalho.ExtractCabecalhoDeClasses(tokensTeste);

                try
                {
                    assercao.IsTrue(arquivoCabecalho.cabecalhoDeClasses[0].methods[0].name == "classeA");

                }
                catch (Exception e)
                {
                    assercao.IsTrue(false, "teste falhou" + e.Message);
                }

            }





            public void TestesGetHeaderProperty(AssercaoSuiteClasse assercao)
            {

                string codigoTeste = "public class classeA { public int umaPropriedade ; }";
                List<string> tokensTeste = new Tokens(new List<string>() { codigoTeste }).GetTokens();

                FileHeader arquivoCabecalho = new FileHeader();
                arquivoCabecalho.ExtractCabecalhoDeClasses(tokensTeste);


                assercao.IsTrue(arquivoCabecalho != null && arquivoCabecalho.cabecalhoDeClasses != null && arquivoCabecalho.cabecalhoDeClasses.Count == 1);


            }



  
            public void TestesGetHeadersMethod(AssercaoSuiteClasse assercao)
            {
                string codigoTeste = "class classeA { int umMethodo ( int x ) { x = x + 1 } public int doisMetodos(int x, int y) { x = x + 1 ; } }";
                List<string> tokensTeste = new Tokens(new List<string>() { codigoTeste }).GetTokens();

                FileHeader arquivoCabecalho = new FileHeader();
                arquivoCabecalho.ExtractCabecalhoDeClasses(tokensTeste);

                try
                {
                    assercao.IsTrue(arquivoCabecalho.cabecalhoDeClasses.Count == 1);
                }
                catch (Exception)
                {
                    assercao.IsTrue(false, "teste falhou");
                }


            }




            public void TestesGetHeadersProprerties(AssercaoSuiteClasse assercao)
            {
                string codigoTeste = "class classeA { int umaPropriedade ; public int duasPropriedades ; int tresPropriedades ; }";

                List<string> tokensTeste = new Tokens(new List<string>() { codigoTeste }).GetTokens();

                FileHeader arquivoCabecalho = new FileHeader();
                arquivoCabecalho.ExtractCabecalhoDeClasses(tokensTeste);

                try
                {
                    assercao.IsTrue(arquivoCabecalho.cabecalhoDeClasses[0].properties.Count == 3);
                }
                catch (Exception e)
                {
                    assercao.IsTrue(false, "teste falhou:  " + e.Message);
                }

            }
            public void TestesGetHeaderMethod(AssercaoSuiteClasse assercao)
            {
                string codigoTeste = "class classeA { int umMethodo ( int x ) { x = x + 1 } }";
                List<string> tokensTeste = new Tokens(new List<string>() { codigoTeste }).GetTokens();

                FileHeader arquivoCabecalho = new FileHeader();
                arquivoCabecalho.ExtractCabecalhoDeClasses(tokensTeste);

                assercao.IsTrue((arquivoCabecalho != null) && (arquivoCabecalho.cabecalhoDeClasses != null) && arquivoCabecalho.cabecalhoDeClasses.Count == 1);

            }





            public void TestExtractionHeadersFromAssembly(AssercaoSuiteClasse assercao)
            {
                ImportadorDeClasses importador = new ImportadorDeClasses(@"ParserLinguagemOrquidea.exe");
                importador.ImportAClassFromAssembly("FileHeader");



                List<Classe> classesImportadas = RepositorioDeClassesOO.Instance().GetClasses();
                List<HeaderClass> headersClasses = new List<HeaderClass>();


                FileHeader fileHeader = new FileHeader();
                fileHeader.ExtractHeadersClassFromClassesOrquidea(classesImportadas, headersClasses);

                assercao.IsTrue(headersClasses != null && classesImportadas != null && headersClasses.Count == classesImportadas.Count);
            }









        }
    }



    public class HeaderClass
    {

        public string nomeClasse;

        /// <summary>
        /// nome e tokens de classes herdadas.
        /// </summary>
        public List<string> heranca = new List<string>();
        /// <summary>
        /// nome e tokens de classes deserdadas.
        /// </summary>
        public List<string> deseranca = new List<string>();




        /// <summary>
        /// tokens do corpo da classe.
        /// </summary>
        public List<string> tokensDaClasse = new List<string>();


        /// <summary>
        /// se true, hove erros na compilacao header.
        /// </summary>
        public bool hasErrors = false;


        /// <summary>
        /// lista de propriedades da classe.
        /// </summary>
        public List<HeaderProperty> properties = new List<HeaderProperty>();
        /// <summary>
        /// lista de metodos da classe.
        /// </summary>
        public List<HeaderMethod> methods = new List<HeaderMethod>();

        /// <summary>
        /// lista de operadores da classe.
        /// </summary>
        public List<HeaderOperator> operators = new List<HeaderOperator>();


        /// <summary>
        /// construtor, inicializa os tokens da classe, construindo os tokens classificados pelo seu tipo.
        /// </summary>
        public HeaderClass(List<string> alltokokens, string nomeClasse, int offset)
        {
            this.nomeClasse = nomeClasse;
            this.properties = new List<HeaderProperty>();
            this.methods = new List<HeaderMethod>();
            this.operators = new List<HeaderOperator>();

            this.heranca = new List<string>();
            this.deseranca = new List<string>();

            this.tokensDaClasse = alltokokens;

            
            int indexTokenClass = alltokokens.IndexOf("class", offset);
            int indexBodyClass = alltokokens.IndexOf("{", offset);

            int indexStartHeranca = alltokokens.IndexOf(":", offset);

            List<string> cabecalhoHeranca = new List<string>();
            if ((indexStartHeranca>=0) && (indexBodyClass-indexStartHeranca)>1)
            {
                // PROCESSAMENTO DE HERANCA/DESERANCA.
                cabecalhoHeranca = alltokokens.GetRange(indexStartHeranca, (indexBodyClass - indexStartHeranca) + 1 - 1); //+1 porque é contador, nao indice, -1 porque eh ate o token anterior a "{".
                if ((cabecalhoHeranca != null) && (cabecalhoHeranca.Count > 0))
                {
                    BuildHerancaDeseranca(cabecalhoHeranca);
                }
            }


            int offsetAcessor = 0;
            if ((indexTokenClass - 1 >= 0) && (GetAcessor(alltokokens[indexTokenClass - 1]) != null))
            {
                offsetAcessor = 1;
            }

            // colocar aqui o codigo para obter os nomes de classes herdadas e deserdadas.

            List<string> tokens = UtilTokens.GetCodigoEntreOperadores(indexBodyClass, "{", "}", alltokokens);
            if ((tokens != null) && (tokens.Count > 0))
            {
                this.tokensDaClasse = alltokokens.GetRange(offset, tokens.Count + cabecalhoHeranca.Count + offsetAcessor + 2); //+1 do token "class" e +1 do token nome, +offsetAcessor.
            }

            // nao esquecer do operador ponto-e-virgula no final da definicao da classe, delimitando de outras classes, no compilador.
            if ((this.tokensDaClasse.Count < alltokokens.Count) && (alltokokens[tokensDaClasse.Count] == ";")) 
            {
                this.tokensDaClasse.Add(";");
            }

            if (!ExtractAClass())
            {
                this.hasErrors = true;
            }

        }

        /// <summary>
        /// extrai nomes de classes herdadas, deserdadas.
        /// </summary>
        /// <param name="tokensCabecalho">tokens da interface de heranca.</param>
        private void BuildHerancaDeseranca(List<string>tokensCabecalho)
        {
            if ((tokensCabecalho == null) || (tokensCabecalho.Count > 0))
            {
                int i = 0;
                if (tokensCabecalho[0] == ":")
                {
                    i += 1;
                }
                string currentSinalHeranca= "";
                string tokenCurr = "";
                
                while (i < tokensCabecalho.Count)
                {
                    tokenCurr = tokensCabecalho[i];
                    if (tokenCurr == "+")
                    {
                        currentSinalHeranca = "+";
                    }
                    else
                    if (tokenCurr == "-")
                    {
                        currentSinalHeranca = "-";
                    }
                    else
                    if ((HeaderClass.IsID(tokenCurr)) && (currentSinalHeranca != ""))
                    {
                        if (currentSinalHeranca == "+")
                        {
                            this.heranca.Add(tokenCurr);
                            i += 1;
                            continue;
                        }
                        else
                        if (currentSinalHeranca == "-")
                        {
                            this.deseranca.Add(tokenCurr);
                            i += 1;
                            continue;
                        }
                        
                    }

                    i++;
                }
            }
        }
        
        /// <summary>
        /// extrai propriedades, metodos, operadores, da classe currente.
        /// </summary>
        public bool ExtractAClass()
        {

			int i = this.tokensDaClasse.IndexOf("{");
            string tokenCURR = "";
            if (i < 0)
            {
                return false;
            }
            i++; //1o. token do corpo da classe.

            List<string> tokensID = new List<string>();
            
            bool isStatic = false;
            bool isMultArgument = false;

            string acessorCurr = "";
            while (i < this.tokensDaClasse.Count)
            {
                tokenCURR = this.tokensDaClasse[i];

                // PROCESSAMENTO DE PARAMETROS MULTI-ARGUMENTOS.
                if (tokenCURR == "!")
                {
                    isMultArgument= true;
                    i += 1;
                    continue;
                }
                else
                // PROCESSAMENTO  DO ACESSOR CURRENTE.
                if ((tokenCURR == "public") || (tokenCURR == "private") || (tokenCURR == "protected"))
                {
                    acessorCurr = tokenCURR;
                }
                else
                // ENCONTROU UM TOKEN  static.
                if (tokenCURR == "static")
                {
                    isStatic = true;
                }
                else
                if (IsID(tokenCURR))
                {
                    tokensID.Add(tokenCURR);

                    // PROCESSAMENTO DE OPERADOR.
                    if (tokenCURR == "operador")
                    {
                        HeaderOperator operadorHeader = HeaderOperator.ValidaOperador(this.nomeClasse, tokensDaClasse, i);
                        if (operadorHeader != null)
                        {   // encontrou um operador.

                            this.operators.Add(operadorHeader);
                            i += operadorHeader.tokens.Count + operadorHeader.bodyOperator.Count + 2; // +2 do operadores de bloco de corpo de metodo.

                            // atualizacao da malha de tokens, caso haja ou token virgula apos a definicao da instrucao operador.
                            if ((i < tokensDaClasse.Count) && (tokensDaClasse[i] == ";")) 
                            {
                                i += 1;
                            }
                            acessorCurr = "";
                            isStatic = false;
                            isMultArgument = false;
                            tokensID.Clear();
                            continue;
                        }
                        else
                        {
                            // tratar aqui caso de nao validacao, mesmo com o token [operador].
                            return false;
                            
                        }
                    }
                    else
                    // PROCESSAMENTO DE WRAPPERS OBJECT.
                    if ((tokensID.Count == 1) && (i + 1 < tokensDaClasse.Count) && (tokensDaClasse[i + 1] != "("))
                    {
                        HeaderProperty property = null;
                        // codificar aqui o processo de wrapper object. faltando definir os tokens de propriedade.
                        if ((BuildParametrosWrapperObject(ref i, tokensDaClasse, ref property)) && (property != null)) 
                        {
                            property.isMultArgument = isMultArgument;
                            property.isStatic = isStatic;
                            this.properties.Add(property);
                            acessorCurr = "";
                            isStatic = false;
                            isMultArgument = false;
                            tokensID.Clear();
                            continue;
                        }
                        
                        
                    }
                    else
                    // PROCESSAMENTO DE METODO SEM TIPO DE RETORNO.
                    if ((tokensID.Count == 1) && (i + 1 < tokensDaClasse.Count) && (tokensDaClasse[i + 1] == "(")) 
                    {
                        if (acessorCurr == "")
                        {
                            acessorCurr = "protected";
                        }
                        string nameMethod = tokensID[0];
                        string classMethod = nomeClasse;

                        int indexStartParams = tokensDaClasse.IndexOf("(", i);
                        if (indexStartParams >= 0)
                        {
                            List<string> tokensDaInterfaceParametros = UtilTokens.GetCodigoEntreOperadores(indexStartParams, "(", ")", this.tokensDaClasse);
                            if (tokensDaInterfaceParametros.Count < 2)
                            {
                                hasErrors = true;
                                return false;
                            }
                            int indexBodyMethod = tokensDaClasse.IndexOf("{", indexStartParams);
                            List<string> tokensDoCorpoDoMetodo = UtilTokens.GetCodigoEntreOperadores(indexBodyMethod, "{", "}", this.tokensDaClasse);


                            HeaderMethod umMetodo = new HeaderMethod(classMethod, nameMethod);
                            umMetodo.typeReturn = "void";
                            umMetodo.BuildParmeters(tokensDaInterfaceParametros, 0);
                            umMetodo.BuildBodyMethod(tokensDoCorpoDoMetodo);
                            umMetodo.isStatic = isStatic;
                            umMetodo.acessor = acessorCurr;

                            // adiciona o metodo para a lista de metodos encontrados.
                            this.methods.Add(umMetodo);

                            // faz a contagem de tokens do metodo: 1 do  nome do metodo +tokens da interface de parametros, + tokens do corpo do metodo;
                            // e +2 dos operadores bracas do corpo do metodo.
                            i += 1 + tokensDaInterfaceParametros.Count + tokensDoCorpoDoMetodo.Count + 2;

                            if (acessorCurr != "")
                            {
                                i++;
                            }

                            if ((i < this.tokensDaClasse.Count) && (this.tokensDaClasse[i] == ";"))
                            {
                                i++;
                            }

                            tokensID.Clear();
                            acessorCurr = "";
                            isStatic = false;
                            isMultArgument = false;
                            continue;

                        }
                        else
                        {
                            // relatar aqui a sintaxd de erro: sem parenteses de interface.

                            return false;
                        }

                    }

                    if (tokensID.Count == 2)
                    {
                        if (acessorCurr == "")
                        {
                            acessorCurr = "protected";
                        }
                        string nomePropriedade = tokensID[1];
                        string nomeClassePropriedade = tokensID[0];

                        // PROCESSAMENTO DE PROPRIEDADE NORMAL
                        if ((this.tokensDaClasse[i + 1] == ";") || (this.tokensDaClasse[i + 1] == "="))
                        {

                            // PROPRIEDADE NORMAL SEM ATRIBUICAO.
                            if (this.tokensDaClasse[i + 1] == ";")
                            {

                                HeaderProperty property = new HeaderProperty(
                                    acessorCurr, nomePropriedade, nomeClassePropriedade, isStatic, null, null);
                                property.isMultArgument = isMultArgument;
                                property.isStatic = isStatic;

                                this.properties.Add(property);

                                tokensID.Clear();
                                acessorCurr = "";
                                isStatic = false;
                                isMultArgument = false;

                                if (tokensDaClasse[i + 1] == ";")
                                {
                                    i += 1;
                                }
                                i += 1;


                                continue;
                            }
                            else
                            // PROCESSAMENTO DE PROPRIEDADE NORMAL, MAS COM ATRIBUICAO.
                            if (this.tokensDaClasse[i + 1] == "=")
                            {
                                bool isOperatorPontoEVirgulaPresente = false;
                                int indexStartAtribuicao = i + 2;
                                int indexEndAtribuicao = this.tokensDaClasse.IndexOf(";");
                                List<string> atribuicao = new List<string>();
                                if ((indexEndAtribuicao != -1) && (indexEndAtribuicao > indexStartAtribuicao)) 
                                {
                                    atribuicao = this.tokensDaClasse.GetRange(indexStartAtribuicao, indexEndAtribuicao - indexStartAtribuicao + 1);
                                    if ((atribuicao != null) && (atribuicao.Count + 1 < this.tokensDaClasse.Count))
                                    {
                                        // remove o operador ponto-e-virgula da atribuicao.
                                        if (atribuicao[atribuicao.Count - 1] == ";")
                                        {
                                            atribuicao.RemoveAt(atribuicao.Count - 1);
                                            isOperatorPontoEVirgulaPresente = true;
                                        }
                                    }
                                }
                               
                                HeaderProperty property = new HeaderProperty(
                                    acessorCurr, nomePropriedade, nomeClassePropriedade, isStatic, null, null);

                                property.isMultArgument = isMultArgument;
                                property.tokensAtribuicao = atribuicao;
                                property.isStatic= isStatic;

                                this.properties.Add(property);

                                tokensID.Clear();
                                acessorCurr = "";
                                isStatic = false;
                                isMultArgument = false;

                                if (atribuicao != null)
                                {
                                    i += 1 + atribuicao.Count + 1;
                                    if (isOperatorPontoEVirgulaPresente)
                                    {
                                        i += 1;
                                    }
                                }
                                else
                                {
                                    i += 1;
                                }
                                continue;
                            }
                        }
                        else
                        {    // PROCESSAMENTO DE UM METODO, COM TIPO DE RETORNO E PARAMETROS,SE TIVER.
                            if (this.tokensDaClasse[i + 1] == "(")
                            {

                                int indexBodyMethod = this.tokensDaClasse.IndexOf("{", i);
                                List<string> tokensInterfaceParametros = UtilTokens.GetCodigoEntreOperadores(i + 1, "(", ")", this.tokensDaClasse);
                                List<string> tokensDoCorpoDoMetodo = UtilTokens.GetCodigoEntreOperadores(indexBodyMethod, "{", "}", this.tokensDaClasse);

                                if (tokensInterfaceParametros.Count < 2)
                                {
                                    hasErrors = true;
                                    return false;
                                }
                                if (tokensInterfaceParametros != null)
                                {
                                    string nomeMetodo = tokensID[1];
                                    string classeMetodo = nomeClasse;

                                    HeaderMethod umMetodo = new HeaderMethod(classeMetodo, nomeMetodo);
                                    umMetodo.typeReturn = tokensID[0];
                                    umMetodo.isStatic = isStatic;
                                    umMetodo.acessor = acessorCurr;

                                    // extrai os parametros do metoodo.
                                    umMetodo.BuildParmeters(tokensInterfaceParametros, 0);
                                    umMetodo.BuildBodyMethod(tokensDoCorpoDoMetodo);

                                    // adiciona o metodo para a lista de metodos da classe.
                                    this.methods.Add(umMetodo);



                                    // +1 do tipo retorno, +1 do nome do metodo, + tokens da interface, + tokens do corpo, +2 dos operadores bracas..
                                    i += 2 + tokensInterfaceParametros.Count + tokensDoCorpoDoMetodo.Count;

                                    if (acessorCurr != "")
                                    {
                                        i++;
                                    }

                                    if ((i < this.tokensDaClasse.Count) && (this.tokensDaClasse[i] == ";"))
                                    {
                                        i++;
                                    }

                                    tokensID.Clear();
                                    acessorCurr = "";
                                    isStatic = false;
                                    isMultArgument = false;


                                    continue;
                                }

                            }
                        }
                    }
                }

                i++;
            }

            return true;

        }
        private string GetAcessor(string tokenAcessor)
        {
            if ((tokenAcessor == "public") || (tokenAcessor == "private") || (tokenAcessor == "protected"))
            {
                return tokenAcessor;
            }
            else
            { return null; }
        }


        private int GetCountBodyMethod(int i)
        {
            int indexStartBodyMethod = this.tokensDaClasse.IndexOf("{", i);
            int countTokensBody = 0;
            if (indexStartBodyMethod >= 0)
            {
                List<string> tokensBody = UtilTokens.GetCodigoEntreOperadores(indexStartBodyMethod, "{", "}", this.tokensDaClasse);
                if (tokensBody != null)
                {
                    countTokensBody += tokensBody.Count;
                }

            }

            return countTokensBody;
        }

        /// <summary>
        /// constroi parametros WrapperData Object, com mais de 2 ids de definicao.
        /// caso encontre, atualiza automaticamente o indice de tokens.
        /// </summary>
        /// <param name="indexTOKEN">indice currente da malha de tokens.</param>
        /// <param name="tokensProperty">tokens da propriedade, delimitados.</param>
        /// <param name="property">parametro/propriedade wrapper object, retornado.</param>
        /// <returns>[true] se localizaou um wrapper object, [false] se nao.</returns>
        public static bool BuildParametrosWrapperObject(ref int indexTOKEN, List<string> tokensProperty, ref HeaderProperty property)
        {

            if ((tokensProperty==null) || (tokensProperty.Count==0))
            {
                return false;
            }

            property = new HeaderProperty();


            List<string> tokens = tokensProperty.ToList<string>();

            for (int i = 0; i < FileHeader.wrapperDefinition.Count; i++)
            {

                // PROCESSAMENTO DE WRAPPER PARAMETERS.
                if (FileHeader.wrapperDefinition[i].isThisTypeWrapperParameter(tokens, indexTOKEN) != null)
                {
                    // obtem os tokens recortados a partir do indice x de processamento, evitando confusão de 1+ wrapper objeccts na
                    // mesma lista de parâmetros.


                    int countTokensWrapperDefinition = 0;

                    List<string> tokensWrapper = FileHeader.wrapperDefinition[i].isThisTypeWrapperParameter(tokens, indexTOKEN);
                    // obtem o tipo de elemento do wrapper data parametro.
                    property.tipoElemento = FileHeader.wrapperDefinition[i].GetTipoElemento(tokensWrapper, ref countTokensWrapperDefinition);

                    // obtem o nome e a classe do wrapper data parametro.
                    property.name = FileHeader.wrapperDefinition[i].GetNameWrapperObject(tokensWrapper, 0);
                    property.className = FileHeader.wrapperDefinition[i].GetTipo();


                    
                    // atualiza o indice da malha de tokens.
                    indexTOKEN += countTokensWrapperDefinition;
                    // remove o operador ponto-e-virguala, se houver.
                    if ((indexTOKEN<tokens.Count) && (tokens[indexTOKEN]==";"))
                    {
                        indexTOKEN += 1;
                    }
                    return true;
                }
            }
            return false;

        }

        private void CompoeHeranca(HeaderClass classe, FileHeader headers)
        {
            List<string> classesHeranca = classe.heranca;


            if ((classesHeranca != null) && (classesHeranca.Count > 0))
                for (int x = 0; x < classesHeranca.Count; x++)
                {
                    HeaderClass headerHerdado = headers.cabecalhoDeClasses.Find(k => k.nomeClasse == classesHeranca[x]);
                    if (headerHerdado != null)
                    {
                        if (headerHerdado.properties != null)
                        {
                            for (int i = 0; i < headerHerdado.properties.Count; i++)
                                if ((headerHerdado.properties[i].acessor == "public") || (headerHerdado.properties[i].acessor == "protected"))
                                {
                                    HeaderProperty propriedadeHerdada = headerHerdado.properties[i];

                                    // compoe o nome longo da propriedade, padrão da linguagem orquidea.
                                    propriedadeHerdada.name = headerHerdado.properties[i].className + "." + headerHerdado.properties[i].name;


                                    if (classe.properties == null)
                                        classe.properties = new List<HeaderProperty>();





                                    classe.properties.Add(propriedadeHerdada);

                                }
                        }

                        if (headerHerdado.methods != null)
                        {
                            for (int i = 0; i < headerHerdado.methods.Count; i++)
                                if ((headerHerdado.methods[i].acessor == "public") || (headerHerdado.methods[i].acessor == "protected"))
                                {
                                    HeaderMethod metodoHerdado = headerHerdado.methods[i];

                                    // compoe o nome longo do metodo, padrão da linguagem orquidea.
                                    metodoHerdado.name = metodoHerdado.className + "." + headerHerdado.methods[i].name;



                                    if (classe.methods == null)
                                        classe.methods = new List<HeaderMethod>();





                                    classe.methods.Add(metodoHerdado);
                                }
                        }




                        if (headerHerdado.operators != null)
                        {
                            for (int i = 0; i < headerHerdado.operators.Count; i++)
                            {
                                HeaderOperator operadorHerdado = headerHerdado.operators[i];

                                if (classe.operators == null)
                                    classe.operators = new List<HeaderOperator>();


                                classe.operators.Add(operadorHerdado);
                            }
                        }
                    }
                }
        }

        private void CompoeDeseranca(HeaderClass classe, FileHeader headers)
        {
            if ((classe.deseranca != null) && (classe.deseranca.Count > 0))
            {
                for (int x = 0; x < classe.deseranca.Count; x++)
                {
                    HeaderClass headerDeserdado = headers.cabecalhoDeClasses.Find(k => k.nomeClasse == classe.deseranca[x]);

                    if (headerDeserdado != null)
                    {


                        // remove propriedades deserdadas.
                        if ((classe.properties != null) && (headerDeserdado.properties != null))
                            for (int i = 0; i < headerDeserdado.properties.Count; i++)
                            {


                                int indexPropriedadeDeserdada = classe.properties.FindIndex(
                                    k => k.name == headerDeserdado.properties[i].name && k.className == headerDeserdado.properties[i].className);



                                if (indexPropriedadeDeserdada != -1)
                                    classe.properties.RemoveAt(indexPropriedadeDeserdada);


                            }

                        // remove metodos deserdados.
                        if ((classe.methods != null) && (headerDeserdado.methods != null))
                            for (int i = 0; i < headerDeserdado.methods.Count; i++)
                            {


                                int indexMetodoDeserdado = classe.methods.FindIndex(
                                     k => k.name == headerDeserdado.methods[i].name && k.className == headerDeserdado.methods[i].className);




                                if (indexMetodoDeserdado != -1)
                                    classe.methods.RemoveAt(indexMetodoDeserdado);
                            }

                        // remove operadores deserdados.
                        if ((classe.operators != null) && (headerDeserdado.operators != null))
                            for (int i = 0; i < headerDeserdado.operators.Count; i++)
                            {


                                int indexOperadorDeserdado = classe.operators.FindIndex(
                                    k => k.className == headerDeserdado.operators[i].className && k.name == headerDeserdado.operators[i].name);




                                if (indexOperadorDeserdado != -1)
                                    classe.operators.RemoveAt(indexOperadorDeserdado);


                            }
                    }
                }
            }
        }


        public override string ToString()
        {
            string str = "class: " + this.nomeClasse;


            if ((methods != null) && (methods.Count > 0))
                str += "   methods: " + methods.Count;


            if ((properties != null) && (properties.Count > 0))
                str += "   properties: " + properties.Count;

            if ((operators != null) && (operators.Count > 0))
                str += "  operarators: " + operators.Count;

            return str;
        }


        /// <summary>
        /// verifica se a expressao nao eh um ID. um ID não contem caracteres como parenteses, colchetes, virgula, ponto-e-virgula, nomes de operadores.
        /// </summary>
        public static bool IsID(string exprssID)
        {



            List<string> tokensExpressao = new List<string>();
            tokensExpressao.Add("(");
            tokensExpressao.Add(")");
            tokensExpressao.Add("=");
            tokensExpressao.Add("[");
            tokensExpressao.Add("]");
            tokensExpressao.Add(".");
            tokensExpressao.Add(",");
            tokensExpressao.Add(";");
            tokensExpressao.Add("{");
            tokensExpressao.Add("}");
            tokensExpressao.Add(":");
            tokensExpressao.Add("&");
            tokensExpressao.Add("$");
            tokensExpressao.Add("#");
            for (int x = 0; x < tokensExpressao.Count; x++)
            {
                if (exprssID.Contains(tokensExpressao[x]))
                {
                    return false;
                }
            }



            return true;

        }

    }
    public class ObjectHeader
    {


        /// <summary>
        /// tokens do objeto header.
        /// </summary>
        public List<string> tokens = new List<string>();


     

        /// <summary>
        /// se true, o object é multi-argumento, podendo representar 1 ou mais objeto de mesmo tipo, num array de objetos quando na
        /// definicao de um metodo.
        /// </summary>
        public bool isMultArgument = false;

     

        public string name;
        public string className;
        public ObjectHeader()
        {
            tokens = new List<string>();
            className = "";
            name = "";
        }

        public override string ToString()
        {
            string str = "";
            if (tokens != null)
            {
                for (int x = 0; x < tokens.Count; x++)
                {
                    str += tokens[x].ToString() + " ";
                }
            }
            return str;
        }


    }

    public class MethodProperty : HeaderProperty
    {
        public string tipoRetorno;
        public List<HeaderProperty> parameters = new List<HeaderProperty>();

        private static MethodProperty metodoCurrent;
        public bool isMetodoParameter = false;
        
        /// <summary>
        /// retorna o metodo-parameter currente.
        /// </summary>
        /// <returns></returns>
        public static MethodProperty ExtractMethoParameter()
        {
            metodoCurrent.isMethodParameter = true;
            return metodoCurrent;
        }


        /// <summary>
        /// faz a validacao de um metodo parametro.
        /// </summary>
        /// <param name="tokens">tokens da classe.</param>
        /// <param name="index">indice currente da malha de processamento de tokens.</param>
        /// <returns>[1 metodo parametro] se validou, [null] se não validou.</returns>
        public static bool ValidateMethodParameter(List<string> tokens, int index)
        {
            if ((tokens == null) || (tokens.Count == 0))
            {
                return false;
            }
            if ((index + 1 < tokens.Count) && (HeaderClass.IsID(tokens[index])) && (HeaderClass.IsID(tokens[index + 1])))
            {
                string tipoRetorno = tokens[index];
                string name = tokens[index + 1];
                if ((index + 2 < tokens.Count) && (tokens[index + 2] == "("))
                {

                    int indexStartParams = index + 2;
                    List<string> tokensParams = UtilTokens.GetCodigoEntreOperadores(indexStartParams, "(", ")", tokens);
                    if ((tokensParams != null) && (tokensParams.Count >= 2))
                    {
                        
                        HeaderMethod funcaoExtrairParametros = new HeaderMethod();
                        funcaoExtrairParametros.BuildParmeters(tokensParams, 0);
                        if ((funcaoExtrairParametros != null) && (funcaoExtrairParametros.parameters != null)) 
                        {

                            metodoCurrent = new MethodProperty();
                            metodoCurrent.parameters = funcaoExtrairParametros.parameters;
                            metodoCurrent.name = name;
                            metodoCurrent.tipoRetorno = tipoRetorno;
                            metodoCurrent.acessor = "private";
                            if (funcaoExtrairParametros.parameters.Count > 0)
                            {
                                // +1 do tipo retorno, +1 do nome do metodo, E +2 dos parenteses da interface de metodo  + a quantida de tokens de parametros, da interface parametros.
                                metodoCurrent.countTokens = 1 + 1 + tokensParams.Count;
                            }
                            else
                            if (funcaoExtrairParametros.parameters.Count == 0)
                            {
                                // +1 do tipo retorno, +1 do nome do metodo, E +2 dos parenteses da interface de metodo.
                                metodoCurrent.countTokens = 1 + 2;

                            }
                            metodoCurrent.tokens = tokens.GetRange(index, metodoCurrent.countTokens - index);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                        
                    }

                }


            }

            return false;
        }
    }


    /// <summary>
    /// classe de definicao de propriedades.
    /// </summary>
    public class HeaderProperty : ObjectHeader
    {

        public string acessor;
        public bool isStatic = false;
        public bool isMethodParameter = false;
        public List<string> tokensParameterWrapperObject = new List<string>();
        public List<string> tokensAtribuicao = new List<string>();
        public int countTokens = 0;

        /// <summary>
        /// campo para wrapper data objects.
        /// </summary>
        public string tipoElemento = "";



        public HeaderProperty()
        {

        }


        public HeaderProperty(string acessor, string propertyName, string className, bool isStatic, List<string> tokensParameterWrapperObject, string tipoElemento)
        {
            this.acessor = acessor;
            this.name = propertyName;
            this.className = className;
            this.isStatic = isStatic;
            this.tokensParameterWrapperObject = tokensParameterWrapperObject;
            this.tipoElemento = tipoElemento;
            this.countTokens = 0;
        }


        public override string ToString()
        {
            string str = className + "." + name;

            return str;
        }







    }
    public class HeaderOperator : ObjectHeader
    {
        // sintaxe:
        // operador ID ID ( ID , ID) "prioridade ID;
        // operador ID ID ( ID ) "prioridade ID;  


        public List<string> operands;
        public string tipoRetorno;
        public int prioridade;
        public List<string> bodyOperator=new List<string>();
        public enum typeOperator { unary_pre, unary_pos, binary }

        public typeOperator tipoDoOperador;
        public HeaderOperator(string className)
        {


            this.className = className;
            this.operands = new List<string>();
         

        }

        public override string ToString()
        {
            string str = name + "(";
            if (operands.Count >= 2)
                str += operands[0] + "," + operands[1] + ")";
            else
                str += operands[0] + ")";

            return str;
        }

        /// <summary>
        /// valida tokens para um novo operador, unario ou binario.
        /// </summary>
        /// <param name="nomeClasse">nome da classe do operador.</param>
        /// <param name="tokens">tokens da instrucao operador.</param>
        /// <param name="index">indice da malha de tokens.</param>
        /// <returns>retorna um [HeaderOperator] se validar, ou null se nao.</returns>
        public static HeaderOperator ValidaOperador(string nomeClasse, List<string> tokens, int index)
        {

            if (tokens[index] == "operador")
            {

                // operador ID ID ( ID ) prioridade ID;  
                if ((tokens.Count >= 8) &&
                    (HeaderClass.IsID(tokens[index + 1])) &&
                    (HeaderClass.IsID(tokens[index + 2])) &&
                    (tokens[index + 3] == "(") &&
                    (HeaderClass.IsID(tokens[index + 4])) &&
                    (tokens[index + 5] == ")") &&
                    (tokens[index + 6] == "prioridade") &&
                    (HeaderClass.IsID(tokens[index + 7]))) 
                {
                    HeaderOperator operadorUnario = new HeaderOperator(nomeClasse);
                    operadorUnario.name = tokens[index + 2];
                    operadorUnario.operands.Add(tokens[index + 4]);
                    operadorUnario.prioridade = int.Parse(tokens[index + 7]);
                    operadorUnario.tipoRetorno = tokens[index + 1];
                    operadorUnario.tokens = tokens.GetRange(index, 8);
                    operadorUnario.tipoDoOperador = typeOperator.unary_pos;
                    operadorUnario.bodyOperator = BuildBodyOfMehtodOperador(tokens, index);

                    return operadorUnario;
                }
                else
                // operador ID ID ( ID , ID) prioridade ID;
                if ((tokens.Count >= 10) && 
                    (HeaderClass.IsID(tokens[index + 1])) &&
                    (HeaderClass.IsID(tokens[index + 2])) &&
                    (tokens[index + 3] == "(") &&
                    (HeaderClass.IsID(tokens[index + 4])) &&
                    (tokens[index + 5] == ",") &&
                    (HeaderClass.IsID(tokens[index + 6])) &&
                    (tokens[index + 7] == ")") &&
                    (tokens[index + 8] == "prioridade") &&
                    (HeaderClass.IsID(tokens[index + 9])))
                {
                    HeaderOperator operadorBinario = new HeaderOperator(nomeClasse);
                    operadorBinario.name = tokens[index + 2];
                    operadorBinario.tipoRetorno = tokens[index + 1];
                    operadorBinario.operands.Add(tokens[index + 4]);
                    operadorBinario.operands.Add(tokens[index + 6]);
                    operadorBinario.prioridade = int.Parse(tokens[index + 9]);
                    operadorBinario.tokens = tokens.GetRange(index, 10);
                    operadorBinario.tipoDoOperador = typeOperator.binary;
                    operadorBinario.bodyOperator = BuildBodyOfMehtodOperador(tokens, index);
                    return operadorBinario;
                }
                else
                {
                    return null;
                }


            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// constroi o corpo do metodo, com instrucoes.
        /// </summary>
        /// <param name="tokens">tokens da classe.</param>
        /// <param name="indexStart">index de comeco do corpo do metodo.</param>
        /// <returns></returns>
        private static List<string> BuildBodyOfMehtodOperador(List<string> tokens, int index)
        {
            int indexStart = tokens.IndexOf("{", index);
            List<string> bodyMethod = UtilTokens.GetCodigoEntreOperadores(indexStart, "{", "}", tokens);
            if ((bodyMethod == null) || (bodyMethod.Count < 2)) 
            {
                return new List<string>();
            }
            else
            {
                bodyMethod.RemoveAt(0);
                bodyMethod.RemoveAt(bodyMethod.Count - 1);
                return bodyMethod;
            }
        }

    }

    /// <summary>
    /// classe de extração e registro de tokens de um metodo.
    /// </summary>
    public class HeaderMethod : HeaderProperty
    {
        public enum tiposParametros { normal, multiArgumento, parametroMetodo, wrapperObject }

        /// <summary>
        /// lista de objetos que formam a interface de parametros do metodo.
        /// </summary>
        public List<HeaderProperty> parameters = new List<HeaderProperty>();


        public new string acessor;
        public string typeReturn;
        public tiposParametros typeHeaderParameter;


        public HeaderMethod()
        {
            parameters = new List<HeaderProperty>();
            this.tokens = new List<string>();

        }

        public HeaderMethod(string classNameMethod, string nameMethod)
        {
            parameters = new List<HeaderProperty>();

            this.name = nameMethod;
            this.className = classNameMethod;
            this.tokens = new List<string>();


            

        }


        /// <summary>
        /// constroi os tokens do corpo do metodo.
        /// </summary>
        /// <param name="tokensCorpoDoMetodo">tokens raw do corpo do metodo.</param>
        public void BuildBodyMethod(List<string> tokensCorpoDoMetodo)
        {
            if ((tokensCorpoDoMetodo == null) || (tokensCorpoDoMetodo.Count < 2))
            {
                return;
            }
            if (tokensCorpoDoMetodo[0] == "{")
            {
                tokensCorpoDoMetodo.RemoveAt(0);
                tokensCorpoDoMetodo.RemoveAt(tokensCorpoDoMetodo.Count - 1);
            }
            this.tokens = tokensCorpoDoMetodo.ToList<string>();

        }


        /// <summary>
        /// constroi os parametros do metodo.
        /// </summary>
        /// <param name="tokensRaw">tokens da classe.</param>
        /// <param name="offset">indice de deslocamento na malha de tokens da classe.</param>
        /// 
        public void BuildParmeters(List<string> tokensRaw, int offset)
        {


            List<string> tokensParameters = UtilTokens.GetCodigoEntreOperadores(offset, "(", ")", tokensRaw);
            if ((tokensParameters == null) || (tokensParameters.Count < 2))
            {
                return;
            }
            tokensParameters.RemoveAt(0);
            tokensParameters.RemoveAt(tokensParameters.Count - 1);
            int i = 0;
            int pilhaParenteses = 0;
            int pilhaBracas = 0;
            int pilhaColchetes = 0;
            List<string> lstTokensParametroCurr = new List<string>();
            bool isMultArgument = false;
            HeaderProperty parametroWrapperObject = null;
            int lastIndexCurr = 0;
            while (i < tokensParameters.Count)
            {
                lastIndexCurr = i;
                string tokenCurr = tokensParameters[i];
                lstTokensParametroCurr.Add(tokenCurr);


                if (tokenCurr == "!")
                {
                    isMultArgument = true;
                    lstTokensParametroCurr.Clear();
                    i += 1;
                    continue;
                }
                else
                if (tokenCurr == "(")
                {
                    pilhaParenteses++;
                }
                else
                if (tokenCurr == ")")
                {
                    pilhaParenteses--;
                }
                else
                if (tokenCurr == "[")
                {
                    pilhaColchetes++;
                }
                else
                if (tokenCurr == "]")
                {
                    pilhaColchetes--;
                }
                else
                if (tokenCurr == "[")
                {
                    pilhaBracas++;
                }
                else
                if (tokenCurr == "]")
                {
                    pilhaBracas--;
                }
                else
                // PROCESSAMENTO DE PARAMETROS METODO-PARAMETRO,
                if (MethodProperty.ValidateMethodParameter(tokensParameters, i))
                {
                    MethodProperty metodoParamero = MethodProperty.ExtractMethoParameter();
                    if (metodoParamero != null)
                    {
                        i += metodoParamero.countTokens;
                        if ((i < tokensParameters.Count) && (tokensParameters[i] == ","))
                        {
                            i += 1;
                        }
                        metodoParamero.isMetodoParameter = true;
                        this.parameters.Add(metodoParamero);
                        lstTokensParametroCurr.Clear();
                        this.typeHeaderParameter = tiposParametros.parametroMetodo;
                        isMultArgument = false;
                        continue;
                    }
                }
                else
                // PROCESSAMENTO DE PARAMETROS WRAPPER OBJECT.
                if (HeaderClass.BuildParametrosWrapperObject(ref i, tokensParameters, ref parametroWrapperObject))
                {
                    parametroWrapperObject.isMultArgument = isMultArgument;
                    parametroWrapperObject.acessor = "private";
                    parametroWrapperObject.countTokens = i - lastIndexCurr;
                    this.parameters.Add(parametroWrapperObject);
                    lstTokensParametroCurr.Clear();
                    isMultArgument = false;


                    continue;
                }
                else
                // PROCESSAMENTO DE UM PARAMETRO NORMAL.
                if ((lstTokensParametroCurr.Count >= 2) && (tokenCurr == ",") &&
                    (pilhaParenteses == 0) &&
                    (pilhaColchetes == 0) &&
                    (pilhaColchetes == 0))
                {
                    ExtractParameterNormal(ref lstTokensParametroCurr, ref isMultArgument, ref i, ref parameters);
                    
                }

                i++;
            }

            // VERIFICA SE O ULTIMO PARAMETRO É UM PARAMETRO NORMAL.
            if ((lstTokensParametroCurr != null) && (lstTokensParametroCurr.Count >= 2))
            {
                ExtractParameterNormal(ref lstTokensParametroCurr, ref isMultArgument, ref i, ref this.parameters);
            }
            else
            // VERIFICA SE O ULTIMO PARAMETRO É UM WRAPPER OBJECT.
            if (HeaderClass.BuildParametrosWrapperObject(ref i, tokensParameters, ref parametroWrapperObject))
            {
                parametroWrapperObject.isMultArgument = isMultArgument;
                parametroWrapperObject.acessor = "private";
                this.parameters.Add(parametroWrapperObject);
                lstTokensParametroCurr.Clear();
                isMultArgument = false;


            }
            else
            // VERIFICACAO SE O ULTIMO PARAMETRO É UM METODO-´PARAMETRO.
            if (MethodProperty.ValidateMethodParameter(tokensParameters, i))
            {
                MethodProperty metodoParamero = MethodProperty.ExtractMethoParameter();
                if (metodoParamero != null)
                {
                    metodoParamero.isMethodParameter = true;
                    this.parameters.Add(metodoParamero);
                    this.typeHeaderParameter = tiposParametros.parametroMetodo;
                    i +=  metodoParamero.countTokens;
                    if ((i < tokensParameters.Count) && (tokensParameters[i] == ",")) 
                    {
                        i += 1;
                    }
                    lstTokensParametroCurr.Clear();
                    isMultArgument = false;

                }
            }

        }


        /// <summary>
        /// extrai um parametro normal, com [id] [id]: [tipo][name];
        /// </summary>
        /// <param name="lstTokensParametroCurr">lista de tokens ids.</param>
        /// <param name="isMultArgument">[true] se é multi-argumento.</param>
        /// <param name="i">indice da malha de tokens.</param>
        /// <param name="parameters">lista de parametros formados.</param>
        private static void ExtractParameterNormal(ref List<string> lstTokensParametroCurr, ref bool isMultArgument, ref int i, ref List<HeaderProperty> parameters)
        {
            
            if ((lstTokensParametroCurr != null) && (lstTokensParametroCurr.Count > 1))
            {


                HeaderProperty property = new HeaderProperty("private", lstTokensParametroCurr[1], lstTokensParametroCurr[0], false, lstTokensParametroCurr, null);
                property.acessor = "private";
                property.isMultArgument = isMultArgument;
                property.countTokens = 2;
               
                parameters.Add(property);
                




                
                isMultArgument = false;
            }
            lstTokensParametroCurr.Clear();
        }



        public override string ToString()
        {
            string str = typeReturn + " " + className + "." + name;
            if (parameters != null)
            {
                str += "(";
                for (int x = 0; x < parameters.Count; x++)
                    str += parameters[x].ToString() + " ";
                str += ")";
            }


            return str;
        }


    }


}
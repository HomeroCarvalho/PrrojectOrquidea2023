using MathNet.Numerics.Optimization.TrustRegion;
using Microsoft.SqlServer.Server;
using ModuloTESTES;
using parser.ProgramacaoOrentadaAObjetos;
using parser.textoFormatado;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

using Util;
using Wrappers.DataStructures;

namespace parser
{
    public class ExpressaoGrupos
    {
        /// <summary>
        /// grupos de tokens, entre parenteses, de um codigo de expressao.
        /// </summary>
        private List<GruposEntreParenteses> grupos = new List<GruposEntreParenteses>();



        /// <summary>
        /// utilizado em metodos, funcoes, estaticos.
        /// </summary>
        private static int contadorObjetosEstaticos = 0;
        /// <summary>
        /// aleatorizador para nome de objetos estaticos.
        /// </summary>
        private Random rnd = new Random();



        /// <summary>
        /// gerenciador de wrappers data object, para expressoes set/get.
        /// </summary>
        GerenciadorWrapper gerenciadorWrapper = new GerenciadorWrapper();


        /// <summary>
        /// lista de tokens sem processamento.
        /// </summary>
        private List<string> tokensNAOResumidos = new List<string>();
        /// <summary>
        /// tokens sem grupos de parametros entre parenteses.
        /// </summary>
        private List<string> tokensResumidos = new List<string>();
        /// <summary>
        /// lista de expressoes processados.
        /// </summary>
        private List<Expressao> exprssRetorno = new List<Expressao>();


        // listas de operadores.
        private List<string> opBinary = new List<string>();
        private List<string> opUnary = new List<string>();
        private List<string> opBinaryAndToUnary = new List<string>();

        // lista com dados de operadores: nome, indice de insercao na expressao de retorno.
        List<DataOperators> dadosDeOperadores = new List<DataOperators>();

        /// <summary>
        /// escopo do metodo, contendo funcoes do metodo que podem ser chamado em expressoes, sem o operador dot e sem objeto caller.
        /// </summary>
        Escopo escopoMetodo;

        /// <summary>
        /// extrai expressoes, utilizando a tecnica original de resumir os tokens, retirando tokens entre parenteses,
        /// tornando melhor o processamento de expressoes, ja que o codigo resumido não varia muito.
        /// </summary>
        /// <param name="codigo">codigo contendo a expressao.</param>
        /// <param name="escopo">contexto em que  a expressao esta.</param>
        /// <returns>retorna uma lista de expressoes formadas a partir do codigo.</returns>
        public List<Expressao> ExtraiExpressoes(string codigo, Escopo escopo)
        {
            this.escopoMetodo= escopo;

            tokensNAOResumidos = new Tokens(codigo).GetTokens();
            this.tokensResumidos = GruposEntreParenteses.RemoveAndRegistryGroups(tokensNAOResumidos, ref grupos, escopo);
            return ExtraiExpressoes(tokensResumidos, escopo);
        }


        /// <summary>
        /// extrai expressoes, utilizando a tecnica original de resumir os tokens, retirando tokens entre parenteses,
        /// tornando melhor o processamento de expressoes, ja que o codigo resumido não varia muito.
        /// </summary>
        /// <param name="tokensResumidos">tokens de formato reduzido, sem tokens entre parenteses.</param>
        /// <param name="escopo">contexto em que a expressao esta.</param>
        /// 
        /// <returns>retorna uma lista de expressoes formadas a partir do codigo.</returns>
        public List<Expressao> ExtraiExpressoes(List<string> tokensResumidos, Escopo escopo)
        {
           

            List<Metodo> fncParametrosEDeClasse= new List<Metodo>();    
            this.escopoMetodo = escopo;
            if ((this.escopoMetodo.tabela.GetFuncoes()!=null) && (this.escopoMetodo.tabela.GetFuncoes().Count() > 0))
            {
                foreach(Metodo funcoesDoEscopo in this.escopoMetodo.tabela.GetFuncoes())
                {
                    fncParametrosEDeClasse.Add(funcoesDoEscopo);    
                }
            }



            this.tokensResumidos = tokensResumidos.ToList<string>();

            if (Expressao.headers == null)
            {
                Expressao.InitHeaders("");
            }

            if ((tokensResumidos == null) || (tokensResumidos.Count == 0))
            {
                UtilTokens.WriteAErrorMensage("empty expression, without tokens to extract exppressions", tokensResumidos, escopo);
                return null;
            }


            GetNomesOperadoresBinariosEUnarios(ref opBinary, ref opUnary, ref opBinaryAndToUnary);


            int i = 0;



            // indice do grupo currente, de tokens de grupo.
            int indexGrupoCurrent = 0;
            // offset para procura de parametros.
            int offsetSearchParameters = 0;

            int indexTokenObjeto = 0;

            bool isStatic = false;
            Objeto objetoCurrent = null;
            List<HeaderMethod> FNC_WHITOUT_OBJ_CALLER = new List<HeaderMethod>();
            Objeto objActual = null;
            while (i < tokensResumidos.Count)
            {

            
                // possibilidade que o token pode ser:
                string tokenCurrent = tokensResumidos[i];
                string literalToken = tokensResumidos[i];
                string nomeOperador = tokensResumidos[i];
                string dotOperador = tokensResumidos[i];
                string tokenParentesesAbre = tokensResumidos[i];
                string nomeClasseEstatica = null;
                string nomeFuncao = tokensResumidos[i];
                string nomeFuncaoEscopoDoMetodo = tokensResumidos[i];
                string nomeObjetoActual = tokensResumidos[i];
                HeaderClass headerClasseEstatica = null;
                if ((i + 1 < tokensResumidos.Count) && (tokensResumidos[i + 1] == ".")) 
                {
                    headerClasseEstatica = Expressao.headers.cabecalhoDeClasses.Find(k => k.nomeClasse == tokensResumidos[i]);
                }
                else
                {
                    headerClasseEstatica = null;
                }


                // obtem as funcoes da classe currente, tendo o [tokenCurrent] como nome de função.
                HeaderClass headerFuncaoClasseCurrente = Expressao.headers.cabecalhoDeClasses.Find(k => k.nomeClasse == Escopo.nomeClasseCurrente);
                if (headerFuncaoClasseCurrente != null) 
                {
                    if ((headerFuncaoClasseCurrente.methods != null) && (headerFuncaoClasseCurrente.methods.Count > 0))
                    {
                        FNC_WHITOUT_OBJ_CALLER = headerFuncaoClasseCurrente.methods.FindAll(k => k.name == nomeFuncao);
                    }
                    else
                    {
                        FNC_WHITOUT_OBJ_CALLER = null;
                    }
                }
                else
                {
                    FNC_WHITOUT_OBJ_CALLER = null;
                }




                // token current possivel nome de objeto nao estatico.
                string nomeObjetoNAOEstatico = tokenCurrent;
                // token possivel nome de objeto estatico.
                string nomeObjetoESTATICO = tokenCurrent;

                // PROCESSAMENTO de objeto NAO estatico.
                Objeto objNAOestatico = escopo.tabela.GetObjeto(nomeObjetoNAOEstatico, escopo);


                // PROCESSAMENTO do objeto [actual].
                if (nomeObjetoActual == "actual")
                {
                    objActual = new Objeto("public", Escopo.nomeClasseCurrente, "actual", null);
                    objetoCurrent = objActual.Clone();
                    isStatic = false;
                    indexTokenObjeto = i;
                }

                /// FAZ o processamento de OBJETOS ESTATICOS, OBJETOS NAO ESTATICOS, OBJETO DE CLASSE ACTUAL. 
                /// se não há operador dot, ja registra o objeto como expressao objeto, na lista de expressoes de retorno.
                if (headerClasseEstatica != null || objNAOestatico != null || objActual!=null)
                {

                    // PROCESSAMENTO DE UM OBJETO ESTATICO.
                    if (headerClasseEstatica != null)
                    {
                        nomeClasseEstatica = headerClasseEstatica.nomeClasse;

                        Objeto objSimulaEstatica = new Objeto("public", nomeClasseEstatica, "objEstaticoTMP_" + (contadorObjetosEstaticos++).ToString(), null);
                        objetoCurrent = objSimulaEstatica.Clone();

                        isStatic = true;
                        indexTokenObjeto = i;
                    }

                    else
                    // PROCESSAMENTO DE UM OBJETO NAO ESTATICO.
                    if (objNAOestatico != null)
                    {
                        objetoCurrent = objNAOestatico.Clone();
                        indexTokenObjeto = i;

                        isStatic = false;
                        // PROCESSAMENTO DE UM OBJETO NAO ESTATICO WRAPPER OBJECT.
                        if ((tokensResumidos.Count > 1) && (objetoCurrent.isWrapperObject) && (ProcessingWrapperObjects(objetoCurrent, escopo, ref i, ref tokensResumidos))) 
                        {

                            continue;
                        }
                    }

                    /// REGISTRO de objeto, SEM operador dot, seguinte: ex.: a=1;
                    if (objetoCurrent != null)
                    {
                        if ((tokensResumidos.Count == 1) ||
                            ((i + 1 < tokensResumidos.Count) && (tokensResumidos[i + 1] != ".")) ||
                            (i == tokensResumidos.Count - 1))  
                        {
                            
                            ExpressaoObjeto exprssObj = new ExpressaoObjeto(objetoCurrent);
                            exprssRetorno.Add(exprssObj);

                            objActual = null;
                            
                            i += 1;
                            continue;
                        }
                        else
                        {
                            i += 1;
                            objActual = null;
                            continue;
                        }
                    }
                    


                }
                else
                // PROCESSAMENTO DE PROPRIEDADES ANINHADAS, CHAMADAS DE METODOS.
                if ((dotOperador == ".") && (objetoCurrent != null))
                {

                    indexTokenObjeto = i;


                    string nomeMetodo = tokensResumidos[i + 1];
                    string nomeProprieadade = tokensResumidos[i + 1];
                    string nomeClasse = objetoCurrent.GetTipo();

                   


                    // PROCESSAMENTO DE UMA CHAMADA DE METODO.
                    if (VerificaMetodo(nomeClasse, nomeMetodo))
                    {
                        bool isToIncludeObjectCallerAsParameter = false;
                        if (((nomeClasse == "double") || (nomeClasse == "string")) && (!isStatic))
                        {
                            isToIncludeObjectCallerAsParameter = true; 
                        }
                        // invoca o metodo de obter uma expressao chamada de metodo.
                        ExpressaoChamadaDeMetodo exprssChamadaMetodo1 = BuildChamadaDeMetodo(
                                tokensNAOResumidos, ref indexGrupoCurrent, objetoCurrent.Clone(), nomeClasse, nomeMetodo, ref indexGrupoCurrent, escopo, isStatic, isToIncludeObjectCallerAsParameter);

                     

                        if (exprssChamadaMetodo1 != null)
                        {

                            if ((exprssChamadaMetodo1.funcao.nomeClasse == "double") || (exprssChamadaMetodo1.funcao.nomeClasse == "string"))
                            {
                                // obtem o valor de um object de classe [double] ou [string].
                                exprssChamadaMetodo1.objectCaller.valor = objetoCurrent.valor;

                                // clona o metodo, e passa valores de processamento.
                                exprssChamadaMetodo1.funcao = exprssChamadaMetodo1.funcao.Clone();
                                exprssChamadaMetodo1.funcao.isStatic = isStatic;
                                if (!isStatic)
                                {   // se for um objeto NAO ESTATICO, seta o 1o. parametro como o objeto caller.
                                    exprssChamadaMetodo1.funcao.isToIncludeCallerIntoParameters = true;
                                      
                                }
                                else
                                {   // se for um objeto ESTATICO (chamada de metodo estática), nao inclui o 1o. parametro o objeto caller.
                                    exprssChamadaMetodo1.funcao.isToIncludeCallerIntoParameters = false;
                                }
                            }
                            else
                            {   // opcao default: o valor é o proprio objeto que contem o valor.
                                exprssChamadaMetodo1.objectCaller.valor = exprssChamadaMetodo1.objectCaller;
                            }

                            exprssRetorno.Add(exprssChamadaMetodo1);
                            i += 2;
                            isStatic = false;
                            objetoCurrent = null;
                            objActual = null;

                            continue;
                        }


                    }


                    // PROCESSAMENTO DE UMA PROPRIEDADE ANINHADA.
                    if (objetoCurrent.GetField(nomeProprieadade) != null)
                    {

                        // INVESTIGACAO SE É PROPRIEDADES ANINHADAS, OU PROPRIEDADES ANINHADAS COM CHAMADA DE METOO ANINHADO.
                        exprssRetorno = ExtractProperties(objetoCurrent.Clone(), tokensResumidos, escopo, ref i, ref indexGrupoCurrent, ref offsetSearchParameters, exprssRetorno);

                        objActual = null;
                        objetoCurrent = null;
                        isStatic = false;
                        continue;
                    }

                    objActual = null;
                    objetoCurrent = null;

                }


                // PROCESAMENTO de OPERADOR IGUAL
                if (tokenCurrent == "=")
                {
                    ProcessingSignalEqualsOperator(ref i, escopo);
                    i += 1;
                    continue;
                }

                // PROCESSAMENTO de EXPRESSAO_ENTRE_PARENTESES
                if (tokenParentesesAbre == "(")
                {

                    List<string> tokensExpressaoEntreParenteses = UtilTokens.GetCodigoEntreOperadores(i, "(", ")", tokensResumidos);
                    if ((tokensExpressaoEntreParenteses != null) && (tokensExpressaoEntreParenteses.Count > 0))
                    {
                        tokensExpressaoEntreParenteses.RemoveAt(0);
                        tokensExpressaoEntreParenteses.RemoveAt(tokensExpressaoEntreParenteses.Count - 1);

                        if (tokensExpressaoEntreParenteses.Count > 0)
                        {

                            // obtem uma lista de expressao entre parenteses.
                            Expressao expressoesEntreParenteses = new Expressao(tokensExpressaoEntreParenteses.ToArray(), escopo);
                            if (expressoesEntreParenteses != null)
                            {
                                // instancia uma expressao entre parenteses, que é um container de expressoes, afetando a avaliacao quando em pos-ordem.
                                ExpressaoEntreParenteses exprssEntreParentesesContainer = new ExpressaoEntreParenteses(expressoesEntreParenteses, escopo);
                                exprssRetorno.Add(exprssEntreParentesesContainer);
                            }
                        }

                        i += tokensExpressaoEntreParenteses.Count + 2; //+2 dos parenteses.
                        continue;
                    }
                }





                // PROCESSAMENTO DE OPERADORES, e retirada de dados de operadores: nome, indice de insercao, para posterior processamento.
                if (ProcessingOperadores(ref i, escopo, tokenCurrent))
                {
                    continue;
                }

                // PROCESSAMENTO DE FUNCOES SEM OBJETO CALLER.
                if ((FNC_WHITOUT_OBJ_CALLER != null) && (FNC_WHITOUT_OBJ_CALLER.Count > 0) && (indexGrupoCurrent < grupos.Count)) 
                {
                    List<string> tokensGrupo1 = grupos[indexGrupoCurrent++].tokens;
                    tokensGrupo1.Insert(0, "(");
                    tokensGrupo1.Insert(tokensGrupo1.Count, ")");

                    int indexBeginParams = tokensGrupo1.IndexOf("(");
                    List<string> tokensParametros = UtilTokens.GetCodigoEntreOperadores(indexBeginParams, "(", ")", tokensGrupo1);
                    Classe classe1 = RepositorioDeClassesOO.Instance().GetClasse(Escopo.nomeClasseCurrente);

                    if (classe1 == null)
                    {
                        UtilTokens.WriteAErrorMensage("none class to processing it, function: " + nomeFuncao + " not found!", tokensNAOResumidos, escopo);
                        return null;
                    }
                    string nomeFuncao2 = "";

                    // fazer o processamento de chamada de funcao, da funcao compilada depois.
                    tokensParametros.RemoveAt(0);
                    tokensParametros.RemoveAt(tokensParametros.Count - 1);

                    // PROCESSAMENTO DE CHAMADAS DE FUNCOES VINDAS DA CLASSE CURRENTE DO ESCOPO.
                    bool isFoundFunctionCompatible = false;
                    for (int m = 0; m < FNC_WHITOUT_OBJ_CALLER.Count; m++)
                    {

                        // compoe os parametros da chamada de funcao.
                        List<Expressao> parametros = Expressao.ExtraiExpressoes(tokensParametros, escopo);

                        // constroi o objeto que chamada a funcao, tornando-se chamada de metodo.
                        nomeClasseEstatica = classe1.GetNome();
                        Objeto objSimulaEstatica = new Objeto("public", nomeClasseEstatica, "objEstaticoTMP_" + (contadorObjetosEstaticos++).ToString(), null);


                        nomeFuncao2 = FNC_WHITOUT_OBJ_CALLER[m].name;
                        string nomeClasseFuncao = FNC_WHITOUT_OBJ_CALLER[m].className;
                        Metodo metodoFuncao = UtilTokens.FindMethodCompatible(objSimulaEstatica, nomeClasseEstatica, nomeFuncao2, nomeClasseFuncao, parametros, escopo, false, false);

                        if (metodoFuncao != null)
                        {
                            isFoundFunctionCompatible = true;
                            ExpressaoChamadaDeMetodo exprssChamadaFuncao = new ExpressaoChamadaDeMetodo(objSimulaEstatica, metodoFuncao, parametros);
                            exprssRetorno.Add(exprssChamadaFuncao);
                            i += 1 + tokensParametros.Count + 2; // +1 do nome da funcao, +2 dos parenteses da interface de parametros, + countTokensParametros.
                            break;
                        }
                        else
                        {

                            continue;
                        }

                    }
                    if (!isFoundFunctionCompatible)
                    {
                        UtilTokens.WriteAErrorMensage("bad sintaxe for function: " + nomeFuncao2 + ".", tokensNAOResumidos, escopo);
                        return null;
                    }






                    continue;
                }
                else
                // PROCESSAMENTO DE FUNCOES-PARAMETRO.
                if ((fncParametrosEDeClasse != null) && (fncParametrosEDeClasse.Count > 0))
                {
                    int indexFuncao = fncParametrosEDeClasse.FindIndex(k => k.nome == nomeFuncaoEscopoDoMetodo);
                    if (indexFuncao != -1)
                    {
                        bool isFail = false;
                        this.ProcessamentoFuncosSemObjetoChamador(this.escopoMetodo, tokensNAOResumidos, exprssRetorno, nomeFuncao,
                            ref i);
                        if (isFail)
                        {
                            UtilTokens.WriteAErrorMensage("function not found: " + nomeFuncao + " in escopo current. check the syntax", tokensNAOResumidos, escopo);
                            return null;
                        }
                    }
                }
                // extracao de EXPRESSAO NUMERO.
                if (ExpressaoNumero.isNumero(tokenCurrent))
                {
                    string numero = tokenCurrent;
                    ExpressaoNumero exprssNumero = new ExpressaoNumero(numero);
                    exprssNumero.tipoDaExpressao = ExpressaoNumero.GetTypeNumber(numero);

                    exprssRetorno.Add(exprssNumero);
                    i += 1;
                    continue;
                }
                // extracao de EXPRESSAO LITERAL
                if (tokenCurrent.Contains(ExpressaoLiteralText.aspas))
                {
                    string literal = tokenCurrent;
                    ExpressaoLiteralText exprssLiteral = new ExpressaoLiteralText(literal);
                    exprssRetorno.Add(exprssLiteral);

                    i += 1;
                    continue;


                }

                // incrementto dos tokes. se nao encontrar uma classificacao para o token, previne de nao entrar em loop infinito.
                i += 1;
            }


            // faz a inserção e validação de operadores, com seus operandos e tipos de operador.
            ProcessingInsertingOperators(escopo);

            return exprssRetorno;


        }



        /// <summary>
        /// funcao de verificar se há algum metodo com o nome do parametro, na classe parametro.
        /// nao verifica se o metodo é o que se está procurando, então sem validação de parametros do metodo.
        /// </summary>
        /// <param name="nomeClasse">nome da classe do metodo.</param>
        /// <param name="nomeMetodo">nome do metodo a verificar.</param>
        /// <returns>true se o metodo pertence a classe.</returns>
        private bool VerificaMetodo(string nomeClasse, string nomeMetodo)
        {

            // econtrou um header com o nome da classe do objeto caller.
            HeaderClass headerClasseChamadaDeMetodo = Expressao.headers.cabecalhoDeClasses.Find(k => k.nomeClasse == nomeClasse);
            if (headerClasseChamadaDeMetodo != null)
            {
                // encontrou o nome do metodo, nos metodos do header classe.
                if (headerClasseChamadaDeMetodo.methods.FindIndex(k => k.name == nomeMetodo) != -1)
                {
                    return true;
                }
            }
            else
            {
                return false;
            }

            return false;
        }



        /// <summary>
        /// EXPRESSAO COM OPERADOR = EXPRESSAO ATRIBUICAO.
        /// </summary>
        /// <param name="escopo">contexto onde o operador está.</param>
        /// <returns>true se o operador igual foi processado corretamente.</returns>
        private bool ProcessingSignalEqualsOperator(ref int indexTokenCurrent, Escopo escopo)
        {
            int indexExpressionReturn = exprssRetorno.Count;


            dadosDeOperadores.Add(new DataOperators("=", indexExpressionReturn, DataOperators.tipoComOperandos.binary));
            int op = dadosDeOperadores.Count - 1;


            if (op >= 0)
            {
                int indexOperator = dadosDeOperadores[op].indexInListOfExpressions;

                if ((indexOperator <= exprssRetorno.Count) && (indexOperator - 1) >= 0)
                {
                    // TIPO DO OBJETO A RECEBER É UM [OBJETO].
                    if (exprssRetorno[indexOperator - 1].GetType() == typeof(ExpressaoObjeto))
                    {
                        Objeto objRecebeAtribuicao = ((ExpressaoObjeto)exprssRetorno[indexOperator - 1]).objectCaller;
                        BuildExpressaoAtribuicao(objRecebeAtribuicao, escopo, ref indexTokenCurrent, ref exprssRetorno);


                    }
                    else
                    // TIPO DO OBJETO A RECEBER É UMA [PROPRIEDADE_ANINHADA].
                    if (exprssRetorno[indexOperator - 1].GetType() == typeof(ExpressaoPropriedadesAninhadas))
                    {
                        ExpressaoPropriedadesAninhadas exprssProps = ((ExpressaoPropriedadesAninhadas)exprssRetorno[indexOperator - 1]);
                        List<Objeto> objetosAninhados = exprssProps.aninhamento;
                        if ((objetosAninhados == null) || (objetosAninhados.Count == 0))
                        {
                            UtilTokens.WriteAErrorMensage("properties:bad format", tokensResumidos, escopo);
                            return false;
                        }

                        Objeto objReceberAtribuicao = objetosAninhados[objetosAninhados.Count - 1];
                        BuildExpressaoAtribuicao(objReceberAtribuicao, escopo, ref indexTokenCurrent, ref exprssRetorno);




                    }


                }
            }


            return false;
        }


        /// <summary>
        /// faz o processamento de funcoes sem objeto caller, como funcoes-parametro, e chamadas de funcoes-dentro de sua classe.
        /// </summary>
        /// <param name="escopo">contexgto onde a expressao esta.</param>
        /// <param name="tokens">tokens nao resukmidos.</param>
        /// <param name="exprssRetorno">lista de expressoes resultantes do proccessamento</param>
        /// <param name="nomeFuncao">nome da funcao sem objeto caller.</param>
        /// <param name="i">indice currente da malha de tokens.</param>
        /// <returns></returns>
        private bool ProcessamentoFuncosSemObjetoChamador(Escopo escopo, List<string> tokens, List<Expressao> exprssRetorno, string nomeFuncao, ref int i)
        {


            List<Metodo> funcoesDoEscopo = escopo.tabela.GetFuncoes();
            if ((funcoesDoEscopo != null) && (funcoesDoEscopo.Count > 0)) 
            {
                int indexTokenParenteses = tokens.IndexOf("(", i);
                if (indexTokenParenteses != -1)
                {

                    List<string> tokensGrupo = UtilTokens.GetCodigoEntreOperadores(indexTokenParenteses, "(", ")", tokens);
                    if ((tokensGrupo != null) && (tokensGrupo.Count >= 2))
                    {
                        // extrai o grupo entre parenteses pertinente a funcao sem objeto caller.
                        List<GruposEntreParenteses> grupoParametroFuncao = new List<GruposEntreParenteses>();
                        List<string> tokensGrupo1 = GruposEntreParenteses.RemoveAndRegistryGroups(tokens, ref grupoParametroFuncao, escopo);
                        tokensGrupo1.Insert(0, "(");
                        tokensGrupo1.Insert(tokensGrupo1.Count, ")");

                        int indexBeginParams = tokensGrupo1.IndexOf("(");
                        List<string> tokensParametros = UtilTokens.GetCodigoEntreOperadores(indexBeginParams, "(", ")", tokensGrupo1);
                        HeaderClass classHeaderDaFuncao = Expressao.headers.cabecalhoDeClasses.Find(k => k.nomeClasse == funcoesDoEscopo[0].nome);


                        if (classHeaderDaFuncao == null)
                        {
                            UtilTokens.WriteAErrorMensage("none class to processing it, function: " + nomeFuncao + " not found!", tokensNAOResumidos, escopo);
                            return false;
                        }
                        string nomeFuncao2 = "";

                        // fazer o processamento de chamada de funcao, da funcao compilada depois.
                        tokensParametros.RemoveAt(0);
                        tokensParametros.RemoveAt(tokensParametros.Count - 1);

                        // PROCESSAMENTO DE CHAMADAS DE FUNCOES VINDAS DA CLASSE CURRENTE DO ESCOPO.
                        bool isFoundFunctionCompatible = false;
                        for (int m = 0; m < funcoesDoEscopo.Count; m++)
                        {

                            // compoe os parametros da chamada de funcao.
                            List<Expressao> parametros = Expressao.ExtraiExpressoes(tokensParametros, escopo);

                            // constroi o objeto que chamada a funcao, tornando-se chamada de metodo.
                            string nomeClasseEstatica = classHeaderDaFuncao.nomeClasse;
                            Objeto objSimulaEstatica = new Objeto("public", nomeClasseEstatica, "objEstaticoTMP_" + (contadorObjetosEstaticos++).ToString(), null);


                            nomeFuncao2 = funcoesDoEscopo[m].nome;
                            string nomeClasseFuncao = funcoesDoEscopo[m].nomeClasse;
                            Metodo metodoFuncao = UtilTokens.FindMethodCompatible(objSimulaEstatica, nomeClasseEstatica, nomeFuncao2, nomeClasseFuncao, parametros, escopo, false, false);

                            if (metodoFuncao != null)
                            {
                                isFoundFunctionCompatible = true;
                                ExpressaoChamadaDeMetodo exprssChamadaFuncao = new ExpressaoChamadaDeMetodo(objSimulaEstatica, metodoFuncao, parametros);
                                exprssRetorno.Add(exprssChamadaFuncao);
                                i += 1 + tokensParametros.Count + 2; // +1 do nome da funcao, +2 dos parenteses da interface de parametros, + countTokensParametros.
                                return true;

                            }
                            else
                            {

                                continue;
                            }

                        }


                        if (!isFoundFunctionCompatible)
                        {
                            UtilTokens.WriteAErrorMensage("bad sintaxe for function: " + nomeFuncao2 + ".", tokensNAOResumidos, escopo);
                            return false;
                        }
                    }
                    else
                    {
                        UtilTokens.WriteAErrorMensage("error in function: " + nomeFuncao + " without parameters interface valid", tokens, escopo);
                        return false;
                    }
                }
                else
                {
                    {
                        UtilTokens.WriteAErrorMensage("error in function: " + nomeFuncao + " without parameters interface", tokens, escopo);
                        return false;
                    }

                }

            }
            return false;
        }

        /// <summary>
        /// constroi uma expressao de atribuicao de retorno.
        /// </summary>
        /// <param name="objRecebeAtribuicao">objeto a receber a atribuição.</param>
        /// <param name="escopo">contexto onde o objeto e a expressao atribuir esta.</param>
        /// <param name="indexTokenCurrent">indice da malha de processo de tokens</param>
        /// <param name="exprssRetorno">lista de expressoes processados.</param>
        /// <returns></returns>
        private bool BuildExpressaoAtribuicao(Objeto objRecebeAtribuicao, Escopo escopo, ref int indexTokenCurrent, ref List<Expressao> exprssRetorno)
        {
            if (objRecebeAtribuicao == null)
            {
                UtilTokens.WriteAErrorMensage("object caller to set the atribution is null", tokensResumidos, escopo);
                return false;
            }
            int indexExpressaoAtribuicao = tokensNAOResumidos.IndexOf(objRecebeAtribuicao.GetNome()) + 2;
            List<string> tokensTotalExpressaoAtribuicao = tokensNAOResumidos.GetRange(indexExpressaoAtribuicao, tokensNAOResumidos.Count - indexExpressaoAtribuicao);




            Expressao exprssATRIBUICAO = new Expressao(tokensTotalExpressaoAtribuicao.ToArray(), escopo);
            if (exprssATRIBUICAO != null)
            {
                ExpressaoObjeto exprssObjeto = new ExpressaoObjeto(objRecebeAtribuicao);
                ExpressaoAtribuicao exprssAtribui = new ExpressaoAtribuicao(exprssObjeto, exprssATRIBUICAO, escopo);

                // a expressao de retorno inteiro está na expressao de atribuicao.
                exprssRetorno.Clear();
                // retorna a expressao atribuição.
                exprssRetorno.Add(exprssAtribui);

                DataOperators.UpdateIndex(dadosDeOperadores);

                indexTokenCurrent += tokensTotalExpressaoAtribuicao.Count;

                return true;

            }
            if ((exprssATRIBUICAO != null) && (exprssATRIBUICAO.tipoDaExpressao != objRecebeAtribuicao.GetTipo()))
            {
                indexTokenCurrent += 1;
                UtilTokens.WriteAErrorMensage("types of object of atribution and expression atribution dont match!", tokensResumidos, escopo);
                return false;
            }
            else
            if (exprssATRIBUICAO == null)
            {
                indexTokenCurrent += 1;
                UtilTokens.WriteAErrorMensage("invalid expression in atributation", tokensTotalExpressaoAtribuicao, escopo);
                return false;
            }

            return false;
        }

        /// <summary>
        /// faz a insercao dos operadores.
        /// </summary>
        /// <param name="escopo">contexto onde a expressao está,</param>
        /// <returns></returns>
        private bool ProcessingInsertingOperators(Escopo escopo)
        {

            if ((dadosDeOperadores != null) && (dadosDeOperadores.Count > 0))
            {
                // em expressoes de mais de um operador, certo, a expressao operador anterior se comporta como operando da proxima
                // expressao operador.
                for (int op = 0; op < dadosDeOperadores.Count; op++)
                {
                    string nameOperator = dadosDeOperadores[op].nameOperator;
                    // operador igual já foi extraido.
                    if (nameOperator == "=")
                    {
                        continue;
                    }

                    // OPERADOR EXCLUSIVAMENTE BINARIO. processamento de OPERANDOS.
                    if (dadosDeOperadores[op].tipo == DataOperators.tipoComOperandos.binary)
                    {
                        // fazer processamento aqui dos operandos do operador binario.
                        int indexPrimeiroOperando = dadosDeOperadores[op].indexInListOfExpressions - 1;
                        int indexSegundoOperando = dadosDeOperadores[op].indexInListOfExpressions;

                        if ((indexPrimeiroOperando >= exprssRetorno.Count) || (indexSegundoOperando >= exprssRetorno.Count))
                        {
                            return false;
                        }
                        if (indexPrimeiroOperando < 0)
                        {
                            UtilTokens.WriteAErrorMensage("binary operator: " + nameOperator + " without first parameter", tokensResumidos, escopo);
                            return false;
                        }

                        if (indexSegundoOperando < 0)
                        {
                            UtilTokens.WriteAErrorMensage("unary operator: " + nameOperator + " without second parameter", tokensResumidos, escopo);
                            return false;
                        }

                        Expressao exprssOperando1 = exprssRetorno[indexPrimeiroOperando];
                        Expressao exprssOperando2 = exprssRetorno[indexSegundoOperando];
                        bool isBinaryAdnUnary = false;
                        // processamento de encontrar operador compativel com os tipos do operando.
                        Operador operatorCompatible = UtilTokens.FindOperatorCompatible(dadosDeOperadores[op].nameOperator, exprssOperando1.tipoDaExpressao, exprssOperando2.tipoDaExpressao, ref isBinaryAdnUnary);
                        if (operatorCompatible != null)
                        {
                            ExpressaoOperador exprssOperador = new ExpressaoOperador(operatorCompatible);
                            exprssOperador.tipoDaExpressao = operatorCompatible.tipoRetorno;


                            // insere o operador na lista de expressoes.
                            exprssRetorno.Insert(dadosDeOperadores[op].indexInListOfExpressions, exprssOperador);
                            // atualiza a lista de operadores.
                            DataOperators.UpdateIndex(dadosDeOperadores);

                            continue;
                        }
                        if (operatorCompatible == null)
                        {
                            UtilTokens.WriteAErrorMensage("operator: " + nameOperator + " not match with parameters list for it", tokensResumidos, escopo);
                            return false;
                        }


                    }
                    else
                    // OPERADOR EXCLUSIVAMENTE UNARIO. processamento de OPERANDOS.
                    if (dadosDeOperadores[op].tipo == DataOperators.tipoComOperandos.unary)
                    {
                        int indexPrimeiroOperando = dadosDeOperadores[op].indexInListOfExpressions - 1;
                        int indexSegundoOperando = dadosDeOperadores[op].indexInListOfExpressions;

                        string nomeOperador = dadosDeOperadores[op].nameOperator;
                        bool isBinaryAndUnary = false;



                        if ((indexPrimeiroOperando >= 0) && (UtilTokens.IsUnaryOperator(nomeOperador, exprssRetorno[indexPrimeiroOperando])))
                        {
                            // operador UNARIO POS.
                            Expressao exprssOperando = exprssRetorno[indexPrimeiroOperando];
                            Operador operadorCompatible = UtilTokens.FindOperatorCompatible(nomeOperador, exprssOperando.tipoDaExpressao, null, ref isBinaryAndUnary);

                            if (operadorCompatible != null)
                            {
                                ExpressaoOperador exprssOperatorUnary = new ExpressaoOperador(operadorCompatible);
                                exprssOperatorUnary.tipoDaExpressao = operadorCompatible.tipoRetorno;


                                // insere o operador na lista de expressoes.
                                exprssRetorno.Insert(dadosDeOperadores[op].indexInListOfExpressions, exprssOperatorUnary);

                                // atualiza a insercao de operadores.
                                DataOperators.UpdateIndex(dadosDeOperadores);


                                continue;
                            }

                            if (operadorCompatible == null)
                            {
                                UtilTokens.WriteAErrorMensage("operator: " + nomeOperador + " not match with parameters for it", tokensResumidos, escopo);
                                return false;
                            }

                        }
                        else
                        if ((indexSegundoOperando < exprssRetorno.Count) && (UtilTokens.IsUnaryOperator(nomeOperador, exprssRetorno[indexSegundoOperando])))
                        {
                            //operador UNARIO PRE
                            Expressao exprssOperando2 = exprssRetorno[indexSegundoOperando];
                            Operador operadorCompatible = UtilTokens.FindOperatorCompatible(nomeOperador, null, exprssOperando2.tipoDaExpressao, ref isBinaryAndUnary);
                            if (operadorCompatible == null)
                            {
                                UtilTokens.WriteAErrorMensage("operator: " + nomeOperador + " not match with parameters list for it", tokensResumidos, escopo);
                                return false;
                            }
                            if (operadorCompatible != null)
                            {
                                ExpressaoOperador exprssOperator = new ExpressaoOperador(operadorCompatible);
                                exprssOperator.tipoDaExpressao = operadorCompatible.tipoRetorno;


                                // insere o operador na lista de expressoes.
                                exprssRetorno.Insert(dadosDeOperadores[op].indexInListOfExpressions, exprssOperator);
                                // atualiza a insercao de operadores.
                                DataOperators.UpdateIndex(dadosDeOperadores);

                                continue;
                            }

                        }
                        else
                        {   // nao validou o operador unario.
                            UtilTokens.WriteAErrorMensage("operator:" + nomeOperador + " is invalid name, or dont match with parameters for it", tokensResumidos, escopo);
                            return false;
                        }


                    }
                    else
                    // operador UNARIO E BINARIO. processamento de OPERANDOS.
                    if (dadosDeOperadores[op].tipo == DataOperators.tipoComOperandos.binaryAndUnary)
                    {


                        string nomeOperador = dadosDeOperadores[op].nameOperator;
                        int indexOperador = dadosDeOperadores[op].indexInListOfExpressions;
                        int indexPrimeiroOperando = dadosDeOperadores[op].indexInListOfExpressions - 1;
                        int indexSegundoOperando = dadosDeOperadores[op].indexInListOfExpressions;

                        bool isValidFirstOperand = false;
                        bool isValidSecondOperand = false;
                        // operador UNARIO E BINARIO funcionando como BINARIO.
                        if (IsValidBinaryOperands(exprssRetorno, indexOperador, ref isValidFirstOperand, ref isValidSecondOperand))
                        {


                            Expressao operando1 = exprssRetorno[indexPrimeiroOperando];
                            Expressao operando2 = exprssRetorno[indexSegundoOperando];

                            // fazer processamento aqui dos operandos do operador unario e binario.
                            Operador operadorCompatibleBinary = UtilTokens.FindOperatorBinarioEUnarioMasComoBINARIO(
                                 operando1.tipoDaExpressao, nomeOperador, operando1.tipoDaExpressao, operando2.tipoDaExpressao);

                            if (operadorCompatibleBinary != null)
                            {
                                ExpressaoOperador exprssOperatorBinario = new ExpressaoOperador(operadorCompatibleBinary);
                                exprssOperatorBinario.tipoDaExpressao = operadorCompatibleBinary.tipoRetorno;

                                // insere o operador na lista de expressoes.
                                exprssRetorno.Insert(dadosDeOperadores[op].indexInListOfExpressions, exprssOperatorBinario);
                                // atualiza a insercao de operadores.
                                DataOperators.UpdateIndex(dadosDeOperadores);

                                continue;

                            }
                            if (operadorCompatibleBinary == null)
                            {
                                UtilTokens.WriteAErrorMensage("binary operator: " + nomeOperador + " not found, or dont match with parameters list for it.", tokensResumidos, escopo);
                                return false;
                            }



                        }
                        else
                        // operador UNARIO E BINARIO, funcionando como UNARIO PRE.
                        if ((!isValidFirstOperand) && (isValidSecondOperand))
                        {
                            Expressao operando = exprssRetorno[indexSegundoOperando];
                            Operador operadorCompatibleUnary = UtilTokens.FindOperatorBinarioUnarioMasComoUNARIO(
                                nomeOperador, null, operando.tipoDaExpressao);


                            if (operadorCompatibleUnary != null)
                            {
                                ExpressaoOperador exprssOperadorUnarioPRE = new ExpressaoOperador(operadorCompatibleUnary);
                                exprssOperadorUnarioPRE.tipoDaExpressao = operadorCompatibleUnary.tipoRetorno;

                                // insere o operador na lista de expressoes.
                                exprssRetorno.Insert(dadosDeOperadores[op].indexInListOfExpressions, exprssOperadorUnarioPRE);
                                // atualiza a insercao de operadores.
                                DataOperators.UpdateIndex(dadosDeOperadores);

                                continue;
                            }
                            if (operadorCompatibleUnary == null)
                            {
                                UtilTokens.WriteAErrorMensage("binary and unary operator, not match for your type as unary, or dont match with parameters list for it", tokensResumidos, escopo);
                                return false;
                            }



                        }
                        else
                        // operador UNARIO E BINARIO, funcionando como UNARIO POS.
                        if ((isValidFirstOperand) && (!isValidSecondOperand))
                        {
                            Expressao operando = exprssRetorno[indexPrimeiroOperando];
                            Operador operadorCompatibleUnary = UtilTokens.FindOperatorBinarioUnarioMasComoUNARIO(
                                                                     nomeOperador, operando.tipoDaExpressao, null);

                            if (operadorCompatibleUnary != null)
                            {
                                ExpressaoOperador exprssOperadorUnarioPOS = new ExpressaoOperador(operadorCompatibleUnary);
                                exprssOperadorUnarioPOS.tipoDaExpressao = operadorCompatibleUnary.tipoRetorno;


                                // insere o operador na lista de expressoes.
                                exprssRetorno.Insert(dadosDeOperadores[op].indexInListOfExpressions, exprssOperadorUnarioPOS);
                                // atualiza a insercao de operadores.
                                DataOperators.UpdateIndex(dadosDeOperadores);

                                continue;
                            }
                            if (operadorCompatibleUnary == null)
                            {
                                UtilTokens.WriteAErrorMensage("binary and unary operator, not match for your type as unary, or dont match with parameters list for it", tokensResumidos, escopo);
                                return false;
                            }


                        }



                    }
                }
            }

            return true;
        }


        /// <summary>
        /// extrai expressoes, que nao tem nada a ver um com outro.
        /// </summary>
        /// <param name="codigo">codigo com as expressoes.</param>
        /// <param name="escopo">contexto onde o codigo está,</param>
        /// <returns>retorna uma lista de expressoes ou null se resultar em erros.</returns>
        public List<Expressao> ExtraiMultipasExpressoesIndependentes(string codigo, Escopo escopo)
        {
            List<string> tokensNAOResumidos = new Tokens(codigo).GetTokens();


            int offsetTokensVirgula = 0;
            List<List<string>> tokensEXPRESSOES_INDEPENDENTES = new List<List<string>>();
            int indexBEGINTokensINDPENDENTES = 0;
            int indexENDtokensINDEPENDENTES = -1;
            int pilhaParenteses = 0;
            List<Expressao> exprssRetorno = new List<Expressao>();
            int x = 0;
            bool isTerminadoExtracao = false;
            int indexBeginTokensExpressao = 0;
            while ((x < tokensNAOResumidos.Count) && (!isTerminadoExtracao))
            {

                offsetTokensVirgula = +1;


                // ENCONTROU UM OPERADOR [;], PASSA PARA A PROXIMA EXPRESSAO.
                if (tokensNAOResumidos[x] == ";")
                {
                    ExtraiUMAExpressao(tokensNAOResumidos, tokensEXPRESSOES_INDEPENDENTES, ref indexBeginTokensExpressao, ref indexBEGINTokensINDPENDENTES, ref indexENDtokensINDEPENDENTES, ref x, ref offsetTokensVirgula);
                    continue;
                }


                if ((tokensNAOResumidos[x] == "(") &&
                    (x - 1 >= 0) &&
                    (x < tokensNAOResumidos.Count) &&
                    (GruposEntreParenteses.IsID(tokensNAOResumidos[x - 1])))
                {

                    // atingiu uma area de parametros de uma chamada de metodo;
                    List<string> tokensParametrosFuncao = UtilTokens.GetCodigoEntreOperadores(x, "(", ")", tokensNAOResumidos);
                    if ((tokensParametrosFuncao != null) && (tokensParametrosFuncao.Count > 0))
                    {
                        x += tokensParametrosFuncao.Count;  // faz o incremento do indice de malha, abrangendo os tokens parametros de uma expressao (chamada de metodo).
                        continue;
                    }
                    else
                    {
                        x++;
                    }
                    continue;
                }

                // verifica se nao está dentro de uma expressao entre parenteses.
                if (tokensNAOResumidos[x] == "(")
                {
                    pilhaParenteses++;
                }
                if (tokensNAOResumidos[x] == ")")
                {
                    pilhaParenteses--;
                }



                // adiciona os tokens para uma nova lista de tokens de uma expressao independente.
                if ((tokensNAOResumidos[x] == ",") && (pilhaParenteses == 0))
                {
                    ExtraiUMAExpressao(tokensNAOResumidos, tokensEXPRESSOES_INDEPENDENTES, ref indexBeginTokensExpressao,
                        ref indexBEGINTokensINDPENDENTES,
                        ref indexENDtokensINDEPENDENTES, ref x, ref offsetTokensVirgula);

                    continue;
                }

                x++;



            }
            // PROCESSAMENTO DA ULTIMA EXPRESSAO, PENDENTE DENTRO DA LISTA DE TOKENS currente;
            if ((indexBEGINTokensINDPENDENTES != -1) && (indexENDtokensINDEPENDENTES < tokensNAOResumidos.Count))
            {
                List<string> tokensIND = tokensNAOResumidos.GetRange(indexBEGINTokensINDPENDENTES, tokensNAOResumidos.Count - indexBEGINTokensINDPENDENTES);
                tokensEXPRESSOES_INDEPENDENTES.Add(new List<string>());
                tokensEXPRESSOES_INDEPENDENTES[tokensEXPRESSOES_INDEPENDENTES.Count - 1].AddRange(tokensIND);
            }

            if (tokensEXPRESSOES_INDEPENDENTES.Count > 0)
            {


                for (int n = 0; n < tokensEXPRESSOES_INDEPENDENTES.Count; n++)
                {
                   Expressao umaExpressao = new Expressao(tokensEXPRESSOES_INDEPENDENTES[n].ToArray(), escopo);
                    if (umaExpressao == null)
                    {
                        UtilTokens.WriteAErrorMensage("bad formation in group of independents expressions", tokensNAOResumidos, escopo);
                        return null;
                    }
                    else
                    {

                        exprssRetorno.Add(umaExpressao);
                    }

                }
                return exprssRetorno;
            }



            return exprssRetorno;
        }


        /// <summary>
        /// faz o processamento de tokens de uma expressao independente.
        /// </summary>
        /// <param name="tokensEXPRESSOES_INDEPENDENTES">lista de todas expressoes encontradas, seus tokens.</param>
        /// <param name="indexBeginTokensExpressao">indice de comeco da expressao.</param>
        /// <param name="indexBEGINTokensINDPENDENTES">indice da lista de expressao independentes;</param>
        /// <param name="indexENDtokensINDEPENDENTES">indice da lista de expressao independentes;</param>
        /// <param name="x">variavel de malha de tokens.</param>
        /// <param name="offsetTokensVirgula">offset para contabilizar o operador [;].</param>
        private void ExtraiUMAExpressao(List<string> tokensNAOResumidos, List<List<string>> tokensEXPRESSOES_INDEPENDENTES, ref int indexBeginTokensExpressao, ref int indexBEGINTokensINDPENDENTES, ref int indexENDtokensINDEPENDENTES, ref int x, ref int offsetTokensVirgula)
        {
            List<string> tokensIND = tokensNAOResumidos.GetRange(indexBeginTokensExpressao, x + 1 - indexBeginTokensExpressao);
            if ((tokensIND != null) && (tokensIND.Count == 1) && (tokensIND[0] == ","))
            {
                x += 1;
                return;
            }

            tokensEXPRESSOES_INDEPENDENTES.Add(new List<string>());
            tokensEXPRESSOES_INDEPENDENTES[tokensEXPRESSOES_INDEPENDENTES.Count - 1].AddRange(tokensIND);

            indexBeginTokensExpressao = x + 1;


            indexBEGINTokensINDPENDENTES = x + 1;
            offsetTokensVirgula = indexBEGINTokensINDPENDENTES;






            x = indexBEGINTokensINDPENDENTES;
        }




        /// <summary>
        /// extrai propriedades, e chamadas de metodo aninhados a propriedades aninhadas.
        /// </summary>
        /// <param name="tokenCurrent">token currente.</param>
        /// <param name="tokensResumidos">tokens sem grupos de tokens enttre parenteses.</param>
        /// <param name="escopo">contexto onde os tokens resumidos está,</param>
        /// <param name="i">indice da malha de tokens.</param>
        /// <param name="indexGrupoCurrent">indice currente do grupo entre parenteses.</param>
        /// <param name="offsetSearchParameters">offseet para procura de parametros, entre os tokens nao resumidos.</param>
        /// <param name="exprssRetorno">lista de expressoes resultado.</param>
        /// <returns></returns>
        private List<Expressao> ExtractProperties(Objeto objCurrente, List<string> tokensResumidos, Escopo escopo, ref int i, ref int indexGrupoCurrent, ref int offsetSearchParameters, List<Expressao> exprssRetorno)
        {



            string nomeMetodo = tokensResumidos[i + 1];
            string nomePropriedadeAninhada = tokensResumidos[i + 1];
            string nomeClasse = objCurrente.GetTipo();
            int indexClass = Expressao.headers.cabecalhoDeClasses.FindIndex(k => k.nomeClasse == nomeClasse);

            List<string> proprieddadsOuMetodoAninhados = new List<string>();
            List<Objeto> aninhamento = new List<Objeto>();
            if (exprssRetorno == null)
            {
                exprssRetorno = new List<Expressao>();
            }
            // INVESTIGACOES: PROPRIEDADES ANINHADAS e CHAMADA DE METODO ANINHADO.
            if (indexClass != -1)
            {




                ExpressaoPropriedadesAninhadas exprssPropriedades = new ExpressaoPropriedadesAninhadas();
                if (i + 1 >= tokensResumidos.Count)
                {
                    i++;

                    return exprssRetorno;
                }


                // atribui o objeto caller para as propriedades aninhadas.
                exprssPropriedades.objectCaller = objCurrente.Clone();



                // VERIFICA SE HÁ MAIS PROPRIEDADES, OU CHAMADAS DE METODOS ANINHADOS.
                // se encontrar um token[i+2]!="." e naoID(token[i]), para a malha.
                while (((i + 1) < tokensResumidos.Count) && (GruposEntreParenteses.IsID(tokensResumidos[i + 1]) && (tokensResumidos[i] == ".")))
                {

                    proprieddadsOuMetodoAninhados.Add(tokensResumidos[i + 1]);

                    i += 2;
                }

                // encontrou um token que não é um id: operadores p.ex.
                if ((i + 1 < tokensResumidos.Count) && (!GruposEntreParenteses.IsID(tokensResumidos[i + 1])))
                {
                    BuildAninhamento(objCurrente, exprssPropriedades, proprieddadsOuMetodoAninhados, aninhamento, ref offsetSearchParameters, escopo);
                    if (aninhamento != null)
                    {
                        exprssPropriedades.aninhamento.AddRange(aninhamento);
                    }

                    SetTypeExpression(aninhamento, exprssPropriedades);

                    exprssRetorno.Add(exprssPropriedades);
                    return exprssRetorno;
                }

                // faz o procedimento de registrar propriedades e metodos aninhados.
                if (proprieddadsOuMetodoAninhados.Count > 0)
                {
                    BuildAninhamento(objCurrente, exprssPropriedades, proprieddadsOuMetodoAninhados, aninhamento, ref offsetSearchParameters, escopo);

                    if (aninhamento != null)
                    {
                        exprssPropriedades.aninhamento.AddRange(aninhamento);
                    }
                    SetTypeExpression(aninhamento, exprssPropriedades);
                }
                if (proprieddadsOuMetodoAninhados.Count == 0)
                {
                    UtilTokens.WriteAErrorMensage("property or calling method not found", tokensResumidos, escopo);
                    return null;
                }
                if (exprssPropriedades != null)
                {
                    exprssRetorno.Add(exprssPropriedades);
                    return exprssRetorno;
                }

            }


            return null;
        }

        private static void SetTypeExpression(List<Objeto> aninhamento, ExpressaoPropriedadesAninhadas exprssPropriedades)
        {
            // seta o tipo da expressao.
            if ((aninhamento != null) && (aninhamento.Count > 0))
            {   // se tiver aninhamento, conjunto de propriedades aninhadas, o tipo da expressao é o ultimo objeto aninhado.
                exprssPropriedades.tipoDaExpressao = aninhamento[aninhamento.Count - 1].GetTipo();
            }
            else
            if ((exprssPropriedades.Elementos != null) && (exprssPropriedades.Elementos.Count > 0))
            {
                // se tiver chamadas de metodos, o tipo da expressao é o ultimo chamada de metodo.
                exprssPropriedades.tipoDaExpressao = exprssPropriedades.Elementos[exprssPropriedades.Elementos.Count - 1].tipoDaExpressao;
            }
        }


        /// <summary>
        /// constroi o aninhamento de propriedades aninhadas, e constroi as chamadas de metodo aninhadas.
        /// </summary>
        /// <param name="objCurrente">objeto que chamou a propriedade aninhadas.</param>
        /// <param name="exprssPropriedades">expressao de retorno.</param>
        /// <param name="proprieddadsOuMetodoAninhados">lista de nomes de propriedades aninhadas</param>
        /// <param name="aninhamento">lista de objetos que constitui o aninhamento.</param>
        /// <param name="offsetSearchParameters">indice de procura por parametros.</param>
        /// <param name="escopo">contexto onde a expressao de retorno esta.</param>
        /// <returns></returns>
        private bool BuildAninhamento(Objeto objCurrente, ExpressaoPropriedadesAninhadas exprssPropriedades, List<string> proprieddadsOuMetodoAninhados, List<Objeto> aninhamento, ref int offsetSearchParameters, Escopo escopo)
        {
            Objeto objPropriedadeOuMetodo = new Objeto(objCurrente);
            int p = 0;
            while (p < proprieddadsOuMetodoAninhados.Count)
            {
                // PROCESSAMENTO DE PROPRIEDADES ANINHADAS JÁ INSTANCIADAS.
                Objeto propCurrent = objPropriedadeOuMetodo.GetField(proprieddadsOuMetodoAninhados[p]);
                if (propCurrent != null)
                {
                    aninhamento.Add(propCurrent);
                    objPropriedadeOuMetodo = propCurrent.Clone();
                    p++;
                    continue;
                }
                else
                // PROCESAMENTO DE PROPRIEDADES ANINHADAS AINDA NAO INSTANCIADAS.
                if (Expressao.headers.cabecalhoDeClasses.Find(k => k.nomeClasse == objPropriedadeOuMetodo.GetTipo()) != null)
                {
                    HeaderClass header = Expressao.headers.cabecalhoDeClasses.Find(k => k.nomeClasse == objPropriedadeOuMetodo.GetTipo());

                    HeaderProperty umaPropriedadeAninhada = header.properties.Find(k => k.name == proprieddadsOuMetodoAninhados[p]);
                    if (umaPropriedadeAninhada != null)
                    {
                        // instancia uma propriedade aninhada.
                        Objeto propAninhadaInstanciacao = new Objeto(umaPropriedadeAninhada.acessor, umaPropriedadeAninhada.className, umaPropriedadeAninhada.name, null);
                        aninhamento.Add(propAninhadaInstanciacao);
                        objPropriedadeOuMetodo = propAninhadaInstanciacao.Clone();
                        p++;
                        continue;

                    }
                    else
                    {
                        // VERIFICA SE HÁ CHAMADAS DE METODO ANINHADOS.
                        string nomeClasseMetodo2 = objPropriedadeOuMetodo.GetTipo();
                        string nomeMetodo2 = proprieddadsOuMetodoAninhados[p];

                        // SE TRUE, há um metodo aninhado, as propriedades aninhadas.
                        if (VerificaMetodo(nomeClasseMetodo2, nomeMetodo2))
                        {

                            List<string> tokensParametros = new List<string>();


                            int indexParametro = tokensNAOResumidos.IndexOf("(", offsetSearchParameters);
                            offsetSearchParameters = indexParametro + 1;
                            if (indexParametro == -1)
                            {
                                tokensParametros = tokensNAOResumidos;
                            }
                            else
                            {
                                tokensParametros = UtilTokens.GetCodigoEntreOperadores(indexParametro, "(", ")", tokensNAOResumidos);
                                tokensParametros.RemoveAt(0);
                                tokensParametros.RemoveAt(tokensParametros.Count - 1);

                            }
                            if ((tokensParametros != null) && (tokensParametros.Count >= 0))
                            {

                                List<Expressao> exprssParametros = Expressao.ExtraiExpressoes(tokensParametros, escopo);

                                Metodo methodCompatible = UtilTokens.FindMethodCompatible(objCurrente, nomeClasseMetodo2, nomeMetodo2, nomeClasseMetodo2, exprssParametros, escopo, false, false);
                                if (methodCompatible != null)
                                {
                                    ExpressaoChamadaDeMetodo exprssChamadaAninhado = new ExpressaoChamadaDeMetodo(objPropriedadeOuMetodo, methodCompatible, exprssParametros);
                                    exprssPropriedades.Elementos.Add(exprssChamadaAninhado);
                                }
                                else
                                {

                                    return false;
                                }

                            }
                            if ((tokensParametros == null) || (tokensParametros.Count < 2))
                            {

                                return false;
                            }


                            p++;
                            continue;

                        }

                        else
                        {
                            p++;
                            continue;
                        }
                    }


                }

                p++;
                continue;
            }
            return true;
        }

        /// <summary>
        /// constroi uma expressao chamada de metodo nao wrapper data object.
        /// </summary>
        /// <param name="objCaller">objeto que chama o metodo.</param>
        /// <param name="nomeClasse">nome da classe do metodo.</param>
        /// <param name="nomeMetodo">nome do metodo.</param>
        /// <param name="indexGrupoCurrent">indice de grupo, vindo de tokens resumidos.</param>
        /// <param name="escopo">contexto onde a expressao esta.</param>
        /// <returns></returns>
        private ExpressaoChamadaDeMetodo BuildChamadaDeMetodo(List<string> tokensNAOResumidos, ref int offsetSearchParmeters, Objeto objCaller, string nomeClasse, string nomeMetodo, ref int indexGrupoCurrent, Escopo escopo, bool isStaticCalling, bool isToIncludeObjectCallerToFirstParameter)
        {


            Metodo methodCompatible = null;
            List<Expressao> exprssParametros = null;
            List<string> tokensParametros = new List<string>();
            if (indexGrupoCurrent < grupos.Count)
            {
                tokensParametros = grupos[indexGrupoCurrent].tokens;
            }
            if (indexGrupoCurrent == 0 && grupos.Count > 0)
            {
                int indexParentesesParametros = tokensNAOResumidos.IndexOf("(", offsetSearchParmeters);
                if (indexParentesesParametros != -1)
                {
                    tokensParametros = UtilTokens.GetCodigoEntreOperadores(indexParentesesParametros, "(", ")", tokensNAOResumidos);
                }
                else
                {
                    tokensParametros = tokensNAOResumidos;
                }

                if ((tokensParametros != null) && (tokensParametros.Count >= 2) && (tokensParametros[0] == "("))
                {
                    tokensParametros.RemoveAt(0);
                    tokensParametros.RemoveAt(tokensParametros.Count - 1);
                }

           
                // constroi os parametros da chamada de metodo.
                exprssParametros = Expressao.ExtraiExpressoes(tokensParametros, escopo);
                List<Expressao> exprssParametrosElementos = new List<Expressao>();
                if ((exprssParametros != null) && (exprssParametros.Count > 0))
                {
                    for (int k = 0; k < exprssParametros.Count; k++)
                    {
                        if ((exprssParametros[k].Elementos != null) && (exprssParametros[k].Elementos.Count > 0))
                        {
                            exprssParametrosElementos.Add(exprssParametros[k].Elementos[0]);
                        }
                    }

                }

                // encontra um metodo compativel com as expressoes parametros.
                methodCompatible = UtilTokens.FindMethodCompatible(objCaller, nomeClasse, nomeMetodo, nomeClasse, exprssParametrosElementos, escopo, isStaticCalling, isToIncludeObjectCallerToFirstParameter);


                // se nao é metodo desta vez, volta o indice de grupo, para uma chamada de metodo que seja presente no codigo.
                if (methodCompatible != null)
                {
                    // atualiza o offset de procura de parametros em tokens nao resumidos.
                    offsetSearchParmeters = indexParentesesParametros + 1;

                    // faz o processamento da CHAMADA DE METODO NAO ESTATICA, COM PARAMETROS VAZIO.
                    ExpressaoChamadaDeMetodo exprssChamada = new ExpressaoChamadaDeMetodo(objCaller, methodCompatible, exprssParametros);
                    return exprssChamada;


                }
                else
                {
                    // atualiza o indice de grupos de tokens entre parenteses.
                    if (indexGrupoCurrent < grupos.Count)
                    {
                        indexGrupoCurrent++;
                    }
                    UtilTokens.WriteAErrorMensage("not found method for calling method: " + UtilString.UneLinhasLista(tokensNAOResumidos), tokensNAOResumidos, escopo);
                    return null;
                }

            }
            return null;
        }


        /// <summary>
        /// faz o processo de extrair dados de operadores que surge nos tokens resumidos.
        /// </summary>
        /// <param name="i">indice da malha de tokens</param>
        /// <param name="escopo">contexto onde os tokens está.</param>
        /// <param name="tokenCurrent">token currente.</param>
        private bool ProcessingOperadores(ref int i, Escopo escopo, string tokenCurrent)
        {
            string tokenParentesesAbre = tokenCurrent;
            string nomeOperador = tokenCurrent;

            // extracao de EXPRESSAO_ENTRE_PARENTESES
            if (tokenParentesesAbre == "(")
            {

                List<string> tokensExpressaoEntreParenteses = UtilTokens.GetCodigoEntreOperadores(i, "(", ")", tokensResumidos);
                if ((tokensExpressaoEntreParenteses != null) && (tokensExpressaoEntreParenteses.Count > 0))
                {
                    tokensExpressaoEntreParenteses.RemoveAt(0);
                    tokensExpressaoEntreParenteses.RemoveAt(tokensExpressaoEntreParenteses.Count - 1);

                    if (tokensExpressaoEntreParenteses.Count > 0)
                    {
                        string codigoExpressao = UtilString.UneLinhasLista(tokensExpressaoEntreParenteses);
                        // obtem uma lista de expressao entre parenteses.
                        List<Expressao> expressoesEntreParenteses = ExtraiExpressoes(codigoExpressao, escopo);
                        if ((expressoesEntreParenteses != null) && (expressoesEntreParenteses.Count > 0))
                        {
                            // instancia uma expressao entre parenteses, que é um container de expressoes, afetando a avaliacao quando em pos-ordem.
                            ExpressaoEntreParenteses exprssEntreParentesesContainer = new ExpressaoEntreParenteses(expressoesEntreParenteses[0], escopo);
                            exprssEntreParentesesContainer.Elementos = new List<Expressao>();
                            exprssEntreParentesesContainer.Elementos.AddRange(expressoesEntreParenteses);
                            exprssEntreParentesesContainer.tipoDaExpressao = expressoesEntreParenteses[0].tipoDaExpressao;
                            exprssRetorno.Add(exprssEntreParentesesContainer);
                        }
                    }

                    i += tokensExpressaoEntreParenteses.Count + 2;
                    return true;
                }

            }


            // OPERADOR DE ATRIBUICAO ==.
            if (nomeOperador == "=")
            {

                // extracao de operador "=" (atribuicao).
                if ((i - 1 < 0) || ((i + 1) >= tokensResumidos.Count))
                {
                    UtilTokens.WriteAErrorMensage("error in expression, operator [=] dont have valid parameters.", tokensResumidos, escopo);
                    return false;
                }

                dadosDeOperadores.Add(new DataOperators("=", exprssRetorno.Count, DataOperators.tipoComOperandos.binary));
                i += 1;
                return true;
            }
            else
            if (opBinary.FindIndex(k => k.Equals(nomeOperador)) != -1)
            {

                // OPERADOR EXCLUSIVAMENTE BINARIO.
                dadosDeOperadores.Add(new DataOperators(nomeOperador, exprssRetorno.Count, DataOperators.tipoComOperandos.binary));
                i += 1;
                return true;
            }
            else
            if (opUnary.FindIndex(k => k.Equals(nomeOperador)) != -1)
            {
                // OPERADOR EXCLUSIVAMENTE UNARIO.
                dadosDeOperadores.Add(new DataOperators(nomeOperador, exprssRetorno.Count, DataOperators.tipoComOperandos.unary));
                i += 1;
                return true;

            }
            else
            if (opBinaryAndToUnary.FindIndex(k => k.Equals(nomeOperador)) != -1)
            {
                // OPERADOR BINARIO E UNARIO.
                dadosDeOperadores.Add(new DataOperators(nomeOperador, exprssRetorno.Count, DataOperators.tipoComOperandos.binaryAndUnary));
                i += 1;
                return true;
            }


            return false;
        }


        /// <summary>
        /// processamento de objetos wrapper.
        /// </summary>
        /// <param name="objCurrente">objeto wrapper.</param>
        /// <param name="escopo">contexto onde os tokens resumidos está.</param>
        /// <param name="i">indice da malha de tokens processados.</param>
        /// <returns></returns>
        private bool ProcessingWrapperObjects(Objeto objCurrente, Escopo escopo, ref int i, ref List<string> tokensResumidos)
        {
            // extracao de OBJETO WRAPPER DATA.
            if ((objCurrente.isWrapperObject) && 
                ((tokensResumidos!=null && tokensResumidos.Count==1)  ||
                (i + 2 < this.tokensResumidos.Count) && (this.tokensResumidos[i + 1] != ".")))
            {

                string nomeObjetoWrapper = objCurrente.GetNome();
                bool isFoundChamadaDeMetodoWrapper = false;


                List<string> tokensProcessed1 = new List<string>();
                // VERFICA SE É UMA EXPRESSAO GET OU SET WRAPPER OBJECT.
                List<string> tokensChamadaDeMetodo = gerenciadorWrapper.ConverteParaTokensChamadaDeMetodo(tokensNAOResumidos, escopo, ref isFoundChamadaDeMetodoWrapper, ref tokensProcessed1);



                if ((tokensChamadaDeMetodo != null) && (tokensChamadaDeMetodo.Count > 0))
                {
                    // encontrou uma expressao GET OU SET ELEMENT, de um objeto wrapper data:  a.fnc(), a.fnc(1,1).
                    string nomeClasse = objCurrente.GetTipo();
                    int indexNomeObjeto = tokensChamadaDeMetodo.IndexOf(nomeObjetoWrapper);
                    string nomeMetodo = tokensChamadaDeMetodo[indexNomeObjeto + 2];

                    int indexParentesesAbre = tokensChamadaDeMetodo.IndexOf("(");
                    if (indexParentesesAbre == -1)
                    {
                        return false;
                    }
                    // extrai os tokens dos parametros.
                    List<string> tokensParametros = UtilTokens.GetCodigoEntreOperadoresComRetiradaDeTokensiniFini(indexParentesesAbre, "(", ")", tokensChamadaDeMetodo);

                    // compoe as expressoes parametros.
                    ExpressaoGrupos expressao = new ExpressaoGrupos();
                    List<Expressao> exprssPARAMETROS = expressao.ExtraiMultipasExpressoesIndependentes(UtilString.UneLinhasLista(tokensParametros), escopo);


                    // encontra um metodo compativel: nome de classe, nome de metodo,lista de expressao parametros.
                    Metodo methodCompatible = UtilTokens.FindMethodCompatible(objCurrente, nomeClasse, nomeMetodo, nomeClasse, exprssPARAMETROS, escopo, false, false);
                    if (methodCompatible != null)
                    {
                        ExpressaoChamadaDeMetodo exprssGET_SET = new ExpressaoChamadaDeMetodo(objCurrente, methodCompatible, exprssPARAMETROS);
                        exprssGET_SET.tipoDaExpressao = objCurrente.GetTipoElement();

                        this.exprssRetorno.Add(exprssGET_SET);

                        int indexNomeObjetoWrapper = this.tokensNAOResumidos.IndexOf(objCurrente.GetNome());
                        int indexOperadaoIgual = this.tokensNAOResumidos.IndexOf("=");
                        if ((this.tokensNAOResumidos.Contains("=")) && (indexNomeObjetoWrapper < indexOperadaoIgual))
                        {
                            indexNomeObjetoWrapper++;
                        }
                        int countTokens = tokensProcessed1.Count;
                        int indexBeginTokens = indexNomeObjetoWrapper;
                        for (int k = indexBeginTokens; k < indexBeginTokens + countTokens; k++)
                        {
                            if (k < tokensNAOResumidos.Count)
                            {
                                tokensNAOResumidos[k] = "";
                            }
                        }



                        // atualiza a malha de tokens, para pegar outros objetos wrapper.
                        i = tokensProcessed1.Count + indexNomeObjetoWrapper;


                        return true;

                    }
                    else
                    {
                        return false;
                    }

                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }


        }


        /// <summary>
        /// constroi uma expressao chamada de metodo, a partir de tokens da chamada de metodo.
        /// </summary>
        /// <param name="objCurrente">objeto que invoca o metodo.</param>
        /// <param name="tokensChamadaDeMetodo">tokens da chamada de metodo.</param>
        /// <param name="tokensResumidos">tokens resumidos, sem grupos de tokens entre parenteses.</param>
        /// <param name="nomeClasse">nome da classe.</param>
        /// <param name="nomeMetodo">nome do metodo.</param>
        /// <param name="escopo">contexto onde os tokens da chamada de metodo está.</param>
        /// <returns></returns>
        public ExpressaoChamadaDeMetodo BuildCallingMethod(Objeto objCurrente, List<string> tokensChamadaDeMetodo, List<string> tokensResumidos, string nomeClasse, string nomeMetodo, Escopo escopo, bool isStaticCalling)
        {
            if ((tokensChamadaDeMetodo != null) && (tokensChamadaDeMetodo.Count > 0))
            {

                int indexParenteses = tokensChamadaDeMetodo.IndexOf("(");
                List<string> tokensParametros = UtilTokens.GetCodigoEntreOperadores(indexParenteses, "(", ")", tokensChamadaDeMetodo);
                if ((tokensParametros == null) || (tokensParametros.Count < 2)) 
                {
                    UtilTokens.WriteAErrorMensage("error in extract parameter of a wrapper object data", tokensResumidos, escopo);
                    return null;
                }

                tokensParametros.RemoveAt(0);
                tokensParametros.RemoveAt(tokensParametros.Count - 1);

                ExpressaoGrupos expressao = new ExpressaoGrupos();

                List<Expressao> exprssParametros = expressao.ExtraiMultipasExpressoesIndependentes(UtilString.UneLinhasLista(tokensParametros), escopo);


                // encontra um metodo compativel com as expressoes parametros.
                Metodo methodCompatible = UtilTokens.FindMethodCompatible(objCurrente, nomeClasse, nomeMetodo, nomeClasse, exprssParametros, escopo, isStaticCalling, false);

                if (methodCompatible != null)
                {
                    objCurrente.valor = objCurrente;
                    // É UMA EXPRESSAO SET/GET ELEMENT, fazer aqui o processamento da expressao chamada de metodo.
                    ExpressaoChamadaDeMetodo exprssMethodCalling = new ExpressaoChamadaDeMetodo(objCurrente, methodCompatible, exprssParametros);
                    exprssMethodCalling.tipoDaExpressao = objCurrente.GetTipoElement();
                    return exprssMethodCalling;
                }
                else
                if (methodCompatible == null)
                {
                    UtilTokens.WriteAErrorMensage("error method to wrapper data object not found", tokensResumidos, escopo);
                    return null;
                }
                
                
            }
            else
            {
                UtilTokens.WriteAErrorMensage("error in processing a wrapper object data", tokensResumidos, escopo);
                return null;
            }

            return null;
        }


        /// <summary>
        /// extrai dos headers, todos nome de operadores das classes presente no codigo.
        /// </summary>
        /// <param name="opBinary">lista de operadores binarios.</param>
        /// <param name="opUnary">lista de operadores unarios.</param>
        /// <param name="opBinaryAndToUnary">lista de operadores que sao unarios e binarios.</param>
        public static void GetNomesOperadoresBinariosEUnarios( ref List<string> opBinary, ref List<string> opUnary, ref List<string> opBinaryAndToUnary)
        {
            if ((opBinary != null) && (opBinary.Count > 0))
            {
                return;
            }
            else
            {
                opBinary = new List<string>();
                opUnary = new List<string>();

           
                // encontra todos nomes de operadores, registrados nas classes base e classe do codigo.
                List<HeaderClass> headers = Expressao.headers.cabecalhoDeClasses;




                for (int x = 0; x < headers.Count; x++)
                {
                    List<HeaderOperator> todosOperadoresDeUmaClasse = headers[x].operators;
                    if (todosOperadoresDeUmaClasse != null)
                    {
                        for (int i = 0; i < todosOperadoresDeUmaClasse.Count; i++)
                        {

                            // operadores EXCLUSIVAMENTE BINARIOS.
                            if (todosOperadoresDeUmaClasse[i].tipoDoOperador == HeaderOperator.typeOperator.binary)
                            {
                                if (opBinary.IndexOf(todosOperadoresDeUmaClasse[i].name) == -1)
                                {
                                    opBinary.Add(todosOperadoresDeUmaClasse[i].name);
                                }

                            }
                            // operadores EXCLUSIVAMENTE UNARIOS.
                            if (((todosOperadoresDeUmaClasse[i].tipoDoOperador == HeaderOperator.typeOperator.unary_pos) ||
                                (todosOperadoresDeUmaClasse[i].tipoDoOperador == HeaderOperator.typeOperator.unary_pre))) 
                            {
                                if (opUnary.IndexOf(todosOperadoresDeUmaClasse[i].name) == -1)
                                {
                                    opUnary.Add(todosOperadoresDeUmaClasse[i].name);
                                }
                                
                            }
                        }

                       
                    }
                }

                // OPERADORES BINARIOS E UNARIOS AO MESMO TEMPO
                if ((opBinary != null) && (opUnary != null))
                {
                    for (int i = 0; i < opBinary.Count; i++)
                    {
                        string operador = opBinary[i];
                        int indexOperadorUnario = opUnary.FindIndex(k => k == operador);
                        if ((opUnary.FindIndex(k => k == operador) != -1) && (opBinaryAndToUnary.FindIndex(k => k.Equals(operador)) == -1))
                        {
                            opBinaryAndToUnary.Add(opBinary[i]);
                            for (int op = 0; op < opBinary.Count; op++)
                            {
                                if (opBinary[op] == operador)
                                {
                                    opBinary.RemoveAt(op);
                                    op--;
                                }
                            }
                            for (int op = 0; op < opUnary.Count; op++)
                            {
                                if (opUnary[op] == operador)
                                {
                                    opUnary.RemoveAt(op);
                                    op--;
                                }

                            }
                            i--;
                        }
                    }
                }

                // remove o operador "=", da lista de operadores, pois é um operador de instanciação,
                // não operador matemático, condicional ou booleano.
                opBinary.Remove("=");
                opUnary.Remove("=");

            }
        }

        /// <summary>
        /// verifica se os operandos para um operador binario sao operandos validos para operador binario.
        /// </summary>
        /// <param name="elementos">lista de expressoes contendo operandos, operadores,...</param>
        /// <param name="indexOperator">indice da expressao operador, dentro da lista de expressoes.</param>
        /// <param name="isValidFirst">true se o 1o. operando eh valido.</param>
        /// <param name="isValidSecond">true se o 2o. operando eh valido.</param>
        /// <returns>retorna true se os dois operandos do operador sao operandos validos.</returns>
        private bool IsValidBinaryOperands(List<Expressao> elementos,int indexOperator, ref bool isValidFirst, ref bool isValidSecond)
        {
            isValidSecond = false;
            isValidFirst = false;

            if (elementos == null)
            {
                return false;   
            }
            List<Type> tiposValidosOperand = new List<Type>();
            tiposValidosOperand.Add(typeof(ExpressaoObjeto));
            tiposValidosOperand.Add(typeof(ExpressaoChamadaDeMetodo));
            tiposValidosOperand.Add(typeof(ExpressaoPropriedadesAninhadas));
            tiposValidosOperand.Add(typeof(ExpressaoNumero));
            tiposValidosOperand.Add(typeof(ExpressaoLiteralText));
            tiposValidosOperand.Add(typeof(ExpressaoEntreParenteses));
            tiposValidosOperand.Add(typeof(Expressao));
            if (indexOperator - 1 < 0)
            {
                

                if (indexOperator<elementos.Count)
                {
                    isValidSecond = tiposValidosOperand.FindIndex(k => k == elementos[indexOperator].GetType()) != -1;
                }
                return false;
            }
            if (indexOperator>=elementos.Count)
            {
                if (indexOperator - 1 > 0)
                {
                    isValidFirst = tiposValidosOperand.FindIndex(k => k == elementos[indexOperator - 1].GetType()) != -1;
                }
                return false;
            }

     
            
            isValidFirst = tiposValidosOperand.FindIndex(k => k == elementos[indexOperator - 1].GetType()) != -1;
            isValidSecond = tiposValidosOperand.FindIndex(k => k == elementos[indexOperator].GetType()) != -1;

            return (isValidFirst && isValidSecond);
        }


        public class Testes : SuiteClasseTestes
        {
            public Testes() : base("testes de expressao grupos")
            {
              
            }
            public void TesteExpressaoCondicional2(AssercaoSuiteClasse assercao)
            {
                string code = "int x=2; int y=5;";
                ProcessadorDeID compilador = new ProcessadorDeID(code);
                compilador.Compilar();

                Expressao exprssCondicional = new Expressao("x>y", compilador.escopo);

                try
                {
                    assercao.IsTrue(exprssCondicional.Elementos.Count == 3 && exprssCondicional.Elementos[1].GetType() == typeof(ExpressaoOperador));
                }
                catch (Exception e)
                {
                    string msgError = e.Message;
                    assercao.IsTrue(false, "TESTE FALHOU");
                }


            }

            public void TestesChamadasDeMetodosEstaticas(AssercaoSuiteClasse assercao)
            {
                char aspas = ExpressaoLiteralText.aspas;
                string code_createText = "string text=" + aspas + "Amazonia" + aspas + ";";
                string code_createVector = "Vector separadores_1;";

                ProcessadorDeID compilador = new ProcessadorDeID(code_createVector + code_createText);
                compilador.Compilar();
               
                //compilador.escopo.tabela.GetObjeto("separadores_1", compilador.escopo).isWrapperObject = true;

                string codigo_0_0 = "string.CuttWords(text, separadores_1);";
                Escopo escopo = new Escopo(codigo_0_0);
                Expressao expss_0_0 = new Expressao(codigo_0_0, compilador.escopo);


            }
            public void TesteExtracaoExpressoes(AssercaoSuiteClasse assercao)
            {
                //Expressao.headers = null;

                string codigo = "t=x+y;";

                Objeto obj1 = new Objeto("private", "int", "x", 1);
                Objeto obj2 = new Objeto("private", "int", "y", 5);
                Objeto obj3 = new Objeto("private", "int", "t", 3); 


                Escopo escopo = new Escopo(codigo);
                escopo.tabela.RegistraObjeto(obj1);
                escopo.tabela.RegistraObjeto(obj2);

                Expressao exprssResult = new Expressao(codigo, escopo);

                try
                {
                    assercao.IsTrue(
                        exprssResult.Elementos[0].GetType() == typeof(ExpressaoObjeto) &&
                        exprssResult.Elementos[1].GetType() == typeof(ExpressaoOperador));
                }
                catch (Exception ex)
                {
                    string codeError = ex.Message;
                    assercao.IsTrue(false, "Falha no teste: " + codigo);
                }
            }

            public void TesteExpressaoPropriedadesAninhadas(AssercaoSuiteClasse assercao)
            {
                //Expressao.headers = null;

                //preparacao para testes unitarios em massa.
                string codigoClasseA = "public class classeA { public int propriedadeA;  public classeA() { };  public int metodoA(int a, int b ){ int x =1; x=x+1;}; }";
                string codigoClasseB = "public class classeB {public classeA propriedadeB;  public classeB(){ }; }";




                List<string> tokensClasseA = new Tokens(codigoClasseA).GetTokens();
                List<string> tokensClasseB = new Tokens(codigoClasseB).GetTokens();


                // testes unitarios em massa.
                string codigoObjectsCreate = "classeB objetoB= create(); classeB objetoB2= create(); classeA objetoA= create();";
                string codigo0_1 = " objetoB.propriedadeB;";
                string codigo0_2 = " objetoB2.propriedadeB.propriedadeA;";
                string codigo0_3 = "objetoA.propriedadeA=1;";
                string codigo0_5 = "objetoA.propriedadeA + objetoA.propriedadeA;";
                string codigo0_6 = "objetoB.propriedadeB.metodoA( 1, 1);";

                string codigoTotal = codigoClasseA + codigoClasseB + codigoObjectsCreate;

                Escopo escopo = new Escopo(codigoTotal);
                ProcessadorDeID compilador = new ProcessadorDeID(codigoTotal);
                compilador.Compilar();


                Expressao expressoes_3 = new Expressao(codigo0_3, compilador.escopo);
                Expressao expressoes_1 = new Expressao(codigo0_1, compilador.escopo);
                Expressao expressoes_6 = new Expressao(codigo0_6, compilador.escopo);
                Expressao expressoes_5 = new Expressao(codigo0_5, compilador.escopo);
                Expressao expressoes_2 = new Expressao(codigo0_2, compilador.escopo);


                try
                {
                    assercao.IsTrue(expressoes_1.Elementos[0].GetType() == typeof(ExpressaoPropriedadesAninhadas), codigo0_1);
                    assercao.IsTrue(expressoes_2.Elementos[0].GetType() == typeof(ExpressaoPropriedadesAninhadas), codigo0_2);
                    assercao.IsTrue(expressoes_3.Elementos[0].GetType() == typeof(ExpressaoAtribuicao), codigo0_3);
                    assercao.IsTrue(expressoes_5.Elementos[0].GetType() == typeof(ExpressaoPropriedadesAninhadas), codigo0_5);
                    assercao.IsTrue(expressoes_6.Elementos[0].Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo), codigo0_6);

                    assercao.IsTrue(RepositorioDeClassesOO.Instance().GetClasse("classeA").GetPropriedade("propriedadeA") != null &&
                                    RepositorioDeClassesOO.Instance().GetClasse("classeB").GetPropriedade("propriedadeB") != null, "instanciacao de classes codigo feito.");
                }
                catch (Exception e)
                {
                    assercao.IsTrue(false, "FATAL ERROR em validar os resultados." + e.Message);
                }

            }


            public void TesteExpressaoChamadasDeMetodo(AssercaoSuiteClasse assercao)
            {

                //Expressao.headers = null;

                //preparacao para testes unitarios em massa.
                string codigoClasseA = "public class classeA { public int propriedadeA;  public classeA() { int b=2; };  public int metodoA(int a, int b ){ int x =1; x=x+1;}; };";
                string codigoClasseB = "public class classeB {public int propriedadeB;  public classeB(){ int c=2;; }; };";





                // testes unitarios em massa.
                string codigoCreate = "classeB objetoB= create(); classeB objetoB2= create(); classeA objetoA= create(); int x=5; int y=1;";
                string codigo0_1 = "objetoA.metodoA(x,1);";
                string codigo0_2 = "objetoA.metodoA(1,5);";
                string codigo0_3 = "objetoA.metodoA((x+1),y+1);";


                string codigoTotal = codigoClasseA + codigoClasseB + codigoCreate;

                Escopo escopo = new Escopo(codigoTotal);
                ProcessadorDeID compilador = new ProcessadorDeID(codigoTotal);
                compilador.Compilar();

                compilador.escopo.tabela.RegistraObjeto(new Objeto("private", "int", "x", "1"));
                compilador.escopo.tabela.RegistraObjeto(new Objeto("private", "int", "y", "5"));





                Expressao expressoes_2 = new Expressao(codigo0_2, compilador.escopo);
                Expressao expressoes_3 = new Expressao(codigo0_3, compilador.escopo);
                Expressao expressoes_1 = new Expressao(codigo0_1, compilador.escopo);




                assercao.IsTrue(AssertFuncoes(expressoes_1), codigo0_1);
                assercao.IsTrue(AssertFuncoes(expressoes_2), codigo0_2);
                assercao.IsTrue(AssertFuncoes(expressoes_3), codigo0_3);

            }

            private bool AssertFuncoes(Expressao expressoes_1)
            {
                string codigoErro = "";
                try
                {
                    return expressoes_1.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo);
                }
                catch (Exception e)
                {
                    codigoErro = e.Message;
                    return false;
                }

            }

            

            public void TesteExpressaoAtribuicao(AssercaoSuiteClasse assercao)
            {
                //Expressao.headers = null;

                string codigo0_1 = "y=x+1";
                string codigo0_2 = "y=-1";
                string codigo0_3 = "y=x+1*y";
                string codigo0_4 = "y=1";
                string codigo0_5 = "y=x*1";
                string codigo0_6 = "y=++x";
                string codigo0_7 = "y=x++";
                Objeto obj1 = new Objeto("private", "int", "x", 1);
                Objeto obj2 = new Objeto("private", "int", "y", 5);
                Escopo escopo = new Escopo(codigo0_5);
                escopo.tabela.RegistraObjeto(obj1);
                escopo.tabela.RegistraObjeto(obj2);

                ExpressaoGrupos exprssao = new ExpressaoGrupos();

                try
                {
                    List<Expressao> exprssResultCodigo7 = exprssao.ExtraiExpressoes(codigo0_7, escopo);
                    List<Expressao> exprssResultCodigo6 = exprssao.ExtraiExpressoes(codigo0_6, escopo);


                    List<Expressao> exprssResultCodigo1 = exprssao.ExtraiExpressoes(codigo0_1, escopo);
                    List<Expressao> exprssResultCodigo2 = exprssao.ExtraiExpressoes(codigo0_2, escopo);
                    List<Expressao> exprssResultCodigo3 = exprssao.ExtraiExpressoes(codigo0_3, escopo);
                    List<Expressao> exprssResultCodigo4 = exprssao.ExtraiExpressoes(codigo0_4, escopo);
                    List<Expressao> exprssResultCodigo5 = exprssao.ExtraiExpressoes(codigo0_5, escopo);


                    assercao.IsTrue(true, "teste feito sem erros fatais.");
                    assercao.IsTrue(AssertAtribuicao(exprssResultCodigo1, 3), codigo0_1);
                    assercao.IsTrue(AssertAtribuicao(exprssResultCodigo2, 2), codigo0_2);
                    assercao.IsTrue(AssertAtribuicao(exprssResultCodigo3, 5), codigo0_3);
                    assercao.IsTrue(AssertAtribuicao(exprssResultCodigo4, 1), codigo0_4);
                    assercao.IsTrue(AssertAtribuicao(exprssResultCodigo5, 3), codigo0_5);
                    assercao.IsTrue(AssertAtribuicao(exprssResultCodigo6, 2), codigo0_6);
                    assercao.IsTrue(AssertAtribuicao(exprssResultCodigo7, 2), codigo0_7);
                }
                catch (Exception e)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + e.Message);
                }




            }

            private bool AssertAtribuicao(List<Expressao> exprssResult, int qtdElementosExpressaoAtribuicao)
            {
                return exprssResult != null &&
                    exprssResult.Count > 0 &&
                    exprssResult[0].GetType() == typeof(ExpressaoAtribuicao) &&
                     ((ExpressaoAtribuicao)exprssResult[0]).exprssAtribuicao != null;

            }

            public void TesteUmaChamadaDeMetodo(AssercaoSuiteClasse assercao)
            {

                //Expressao.headers = null;

                //preparacao para testes unitarios em massa.
                string codigoClasseA = "public class classeA { public int propriedadeA;  public classeA() { };  public int metodoA(int a, int b ){ int x =1; x=x+1;}; };";





                // testes unitarios em massa.
                string codigoCreate = "classeA objetoA= create(); int x; int y;";
                string code_chamadaDeMetodo = "objetoA.metodoA(1,5);";
                string code_propriedadeAninhadas = "objetoA.propriedadeA";
                string code_chamadaDeMetodoESTATICO = "double.abs(x)";

                string codigoTotal = codigoClasseA + codigoCreate;

                Escopo escopo = new Escopo(codigoTotal);
                ProcessadorDeID compilador = new ProcessadorDeID(codigoTotal);
                compilador.Compilar();



                Expressao expressao_propriedades = new Expressao(code_propriedadeAninhadas, compilador.escopo);
                Expressao expressao_chamadaDeMetodoEstatico = new Expressao(code_chamadaDeMetodoESTATICO, compilador.escopo);
                Expressao expressao_chamadaDeMetodo = new Expressao(code_chamadaDeMetodo, compilador.escopo);

                try
                {
                    assercao.IsTrue(expressao_chamadaDeMetodoEstatico.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo));
                    assercao.IsTrue(expressao_propriedades.Elementos[0].GetType() == typeof(ExpressaoPropriedadesAninhadas), code_propriedadeAninhadas);
                    assercao.IsTrue(AssertFuncoes(expressao_chamadaDeMetodo), code_chamadaDeMetodo);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }

            }


     

            public void TesteAvaliacaoAtribuicaoPropriedadesAninhadas(AssercaoSuiteClasse assercao)
            {
                //Expressao.headers = null;

                string code_classe_0_1 = "public class classeA { public int propriedade1; public classeA(){int y=3;}};";
                string code_create_obj = "classeA obj1= create();";
                string code_expression_0_1 = "obj1.propriedade1= 5;";
                string code_expression_0_2 = "obj1.propriedade1= -1;";
                string code_expression_0_3 = "obj1.propriedade1= obj1.propriedade1+1;";

                ProcessadorDeID compilador = new ProcessadorDeID(code_classe_0_1 + code_create_obj);
                compilador.Compilar();

                Expressao exprssProp_03 = new Expressao(code_expression_0_3, compilador.escopo);
                Expressao exprssProp_01 = new Expressao(code_expression_0_1, compilador.escopo);
                Expressao exprssProp_02 = new Expressao(code_expression_0_2, compilador.escopo);

                try
                {
                    assercao.IsTrue(exprssProp_01.Elementos[0].GetType() == typeof(ExpressaoAtribuicao));
                    assercao.IsTrue(exprssProp_02.Elementos[0].GetType() == typeof(ExpressaoAtribuicao));
                    assercao.IsTrue(exprssProp_03.Elementos[0].GetType() == typeof(ExpressaoAtribuicao));

                }
                catch (Exception ex)
                {
                    string errorCode = ex.Message;
                    assercao.IsTrue(false, "TESTE FALHOU");
                }

            }

 
            public void TesteExpressaoCondicional(AssercaoSuiteClasse assercao)
            {
                //Expressao.headers = null;


                Escopo escopo = new Escopo("int x=0");
                escopo.tabela.RegistraObjeto(new Objeto("private", "int", "x", 1));
                escopo.tabela.RegistraObjeto(new Objeto("private", "int", "n", 5));
                
                string exprss_0_0 = "x<n";

                Expressao exprssCondicional = new Expressao(exprss_0_0, escopo);

            }

            public void TestesExpressoesOperadores(AssercaoSuiteClasse assercao)
            {
                //Expressao.headers = null;
                
                
                string code_0_0 = "-2+x*y";


                Escopo escopo = new Escopo(code_0_0);
                escopo.tabela.RegistraObjeto(new Objeto("private", "int", "x", 1));
                escopo.tabela.RegistraObjeto(new Objeto("private", "int", "y", 5));
                Expressao exprssOperador = new Expressao(code_0_0, escopo);
                try
                {
                    assercao.IsTrue(exprssOperador.Elementos.Count > 0);
                }
                catch (Exception e)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + e.Message);
                }
            }


 
    
            public void TesteMetodosEstaticos(AssercaoSuiteClasse assercao)
            {
                //Expressao.headers = null;
                List<Classe> classes = LinguagemOrquidea.Instance().GetClasses();
                
                string codigo_0_0 = "double.abs(1)";
                string codgio_0_1 = "double.power(2,2)";
                Escopo escopo = new Escopo(codigo_0_0);

                Expressao exprss_0_0 = new Expressao(codigo_0_0, escopo);
                Expressao exprrs_0_1 = new Expressao(codgio_0_1, escopo);
                try
                {
                    assercao.IsTrue(exprss_0_0.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo), codigo_0_0);
                    assercao.IsTrue(exprrs_0_1.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo), codgio_0_1);
                }
                catch
                {
                    assercao.IsTrue(false, "falha no teste, expressao: " + codigo_0_0);
                }
               
            }

   


            public void TesteExpressaoEntreParenteses(AssercaoSuiteClasse assercao)
            {
                //Expressao.headers = null;

                string codigo_0_0 = "y=x+1";
                string codigo_0_1 = "y= x+(1+y)*3;";
                string codigo_0_2 = "(x+1)*5;";
                string codigo_0_3 = "x=(x+2)-5;";
                Objeto obj1 = new Objeto("private", "int", "x", 1);
                Objeto obj2 = new Objeto("private", "int", "y", 5);
                Escopo escopo = new Escopo(codigo_0_1 + codigo_0_2 + codigo_0_3);
                escopo.tabela.RegistraObjeto(obj1);
                escopo.tabela.RegistraObjeto(obj2);

                Expressao expressoes_0_1 = new Expressao(codigo_0_1, escopo);
                Expressao expressoes_0_2 = new Expressao(codigo_0_2, escopo);
                Expressao expressoes_0_3 = new Expressao(codigo_0_3, escopo);
                Expressao expressao_0_0 = new Expressao(codigo_0_0, escopo);

                try
                {
                    assercao.IsTrue(expressoes_0_1.Elementos[0].GetType() == typeof(ExpressaoAtribuicao), codigo_0_1);
                    assercao.IsTrue(expressao_0_0.Elementos[0].GetType() == typeof(ExpressaoAtribuicao), codigo_0_0);
                    assercao.IsTrue(expressoes_0_2.Elementos.Count == 3, codigo_0_2);
                    assercao.IsTrue(expressoes_0_3.Elementos[0].GetType() == typeof(ExpressaoAtribuicao), codigo_0_3);

                }
                catch (Exception e)
                {
                    assercao.IsTrue(false, "FATAL ERROR em validacao dos resultados" + e.Message);
                }

                


            }

          

            public void TesteListaExpressoes(AssercaoSuiteClasse assercao)
            {
                //Expressao.headers = null;

                // codigo de 3 sub-expressoes.
                string codigo_0_1 = "y= x+(1+y)*3, x+1";
                string codigo0_2 = "(x+3)*1, x*y,1+y";

                Objeto obj1 = new Objeto("private", "int", "x", 1);
                Objeto obj2 = new Objeto("private", "int", "y", 5);
                Escopo escopo = new Escopo(codigo_0_1);
                escopo.tabela.RegistraObjeto(obj1);
                escopo.tabela.RegistraObjeto(obj2);

                ExpressaoGrupos expressao = new ExpressaoGrupos();
                List<Expressao> exprss_0_2 = expressao.ExtraiMultipasExpressoesIndependentes(codigo0_2, escopo);
                List<Expressao> exprss_0_1 = expressao.ExtraiMultipasExpressoesIndependentes(codigo_0_1, escopo);

                assercao.IsTrue(true, "extracao de expressoes feito sem erros fatais.");
                assercao.IsTrue(exprss_0_1 != null && exprss_0_1.Count == 2, codigo_0_1);
                assercao.IsTrue(exprss_0_2 != null && exprss_0_2.Count == 3, codigo0_2);

            }


 
 
            public void TesteExtracaoGrupos(AssercaoSuiteClasse assercao)
            {
                //Expressao.headers = null;

                string codigo_0_1 = "a.fx(1,5)+b.fnc(x,y+1);";
                string codigo_0_2 = "a.fx(1)+(2+x)*3;";
                string codigo_0_3 = "(x+5)*3+fx(1,5,x+1)";

                Escopo escopo = new Escopo(codigo_0_1);
                List<string> tokensOriginais_0_1 = new Tokens(codigo_0_1).GetTokens();
                List<string> tokensOriginais_0_2 = new Tokens(codigo_0_2).GetTokens();
                List<string> tokensOriginais_0_3 = new Tokens(codigo_0_3).GetTokens();


                List<GruposEntreParenteses> grupos = null;
                ExpressaoGrupos exprss = new ExpressaoGrupos();

                List<string> tokensResumidos_0_3 = GruposEntreParenteses.RemoveAndRegistryGroups(codigo_0_3, ref grupos, escopo);
                PrintResults(tokensOriginais_0_3, grupos, tokensResumidos_0_3);
                
                
                grupos = null;
                List<string> tokensResumidos_0_2 = GruposEntreParenteses.RemoveAndRegistryGroups(codigo_0_2, ref grupos, escopo);
                PrintResults(tokensOriginais_0_2, grupos, tokensResumidos_0_2);

                grupos = null;
                List<string> tokensResumidos_0_1 = GruposEntreParenteses.RemoveAndRegistryGroups(codigo_0_1, ref grupos, escopo);
                PrintResults(tokensOriginais_0_1, grupos, tokensResumidos_0_1);







                System.Console.ReadLine();

            }

            private static void PrintResults(List<string> tokensOriginais, List<GruposEntreParenteses> grupos, List<string> tokensResumidos_0_1)
            {
                UtilTokens.PrintTokensWithoutSpaces(tokensOriginais, "tokens originais:");
                UtilTokens.PrintTokensWithoutSpaces(tokensResumidos_0_1, "tokens resumidos:");
                if ((grupos != null) && (grupos.Count > 0))
                {
                    for (int i = 0; i < grupos.Count; i++)
                    {
                        UtilTokens.PrintTokensWithoutSpaces(grupos[i].tokens, "grupo: " + i);
                    }
                }
            }

  
   
           
        }

        /// <summary>
        /// agrupa tokens entre parenteses, para um objeto grupo.
        /// </summary>
        public class GruposEntreParenteses
        {
            /// <summary>
            /// tokens do grupo.
            /// </summary>
            public List<string> tokens = new List<string>();
            /// <summary>
            /// indice dentro dos tokens da expressao.
            /// </summary>
            public int index;
            /// <summary>
            /// texto contendo todos tokens, formatados em modo texto.
            /// </summary>
            public string text;


            private static List<string> nameOperators = null;
            private static List<string> opBinaries;
            private static List<string> opUnaries;
            private static List<string> opBinariesAndUnaries;


            /// <summary>
            /// token identificador de literais (constantes string).
            /// </summary>
            private static char aspas = '\u0022';

            /// <summary>
            /// construtor.
            /// </summary>
            /// <param name="tokens">tokens entre parenteses extraido.</param>
            /// <param name="index">indice dos tokens entre a expressao dos tokens.</param>
            public GruposEntreParenteses(List<string> tokens, int index)
            {
                this.tokens = tokens;
                this.index = index;

                this.text = "";
                if (nameOperators == null)
                {
                    AllOperators();
                }
                

                // remove os parenteses externos da lista de tokens do grupo.
                if ((tokens[0].Equals("(")) && (tokens.Count > 0))
                {
                    tokens.RemoveAt(0);
                }

                if ((tokens.Count > 0) && (tokens[tokens.Count - 1].Equals(")")))
                {
                    tokens.RemoveAt(tokens.Count - 1);
                }

                // forma o texto do grupo com todos tokens.
                for (int i = 0; i < tokens.Count; i++)
                {
                    text += tokens[i] + " ";
                }
                // format o texto do grupo.
                this.text = UtilTokens.FormataEntrada(text);

                

            }
            /// <summary>
            /// retira grupos de tokens entre parenteses, tornnando "facil" fazer o processamento de tokens de expressoes.
            /// </summary>
            /// <param name="codigo">codgo da expressao.</param>
            /// <param name="grupos">grupos extraidos.</param>
            /// <returns>retorna um lista de tokens sem os grupos.</returns>
            public static List<string> RemoveAndRegistryGroups(string codigo, ref List<GruposEntreParenteses> grupos, Escopo escopo)
            {
                grupos = new List<GruposEntreParenteses>();
                List<string> tokensExpressao = new Tokens(codigo).GetTokens();

                if ((tokensExpressao == null) || (tokensExpressao.Count == 0))
                {

                    return tokensExpressao;
                }

                int offsetIndice = 0;
                while ((offsetIndice < tokensExpressao.Count) && (tokensExpressao.IndexOf("(", offsetIndice) != -1)) 
                {
                    
                    int indiceParenteses = tokensExpressao.IndexOf("(", offsetIndice);
                    if (indiceParenteses == 0)
                    {
                        List<string> tokensExpressaoEntreParenteses = UtilTokens.GetCodigoEntreOperadores(indiceParenteses, "(", ")", tokensExpressao);
                        if ((tokensExpressaoEntreParenteses != null) && (tokensExpressaoEntreParenteses.Count > 0))
                        {
                            offsetIndice = indiceParenteses + 1;
                            continue;
                        }
                    }
                    else
                    // se o token currente não é um operador, mas um id, faz o processamento de grupos.
                    if (((indiceParenteses - 1) >= 0) && (isOperator(tokensExpressao[indiceParenteses-1])))
                    {
                        offsetIndice=indiceParenteses+1;
                        continue;
                    }
                    else
                    {
                        List<string> umGrupo = UtilTokens.GetCodigoEntreOperadores(indiceParenteses, "(", ")", tokensExpressao);
                        if ((umGrupo == null) || (umGrupo.Count == 0))
                        {
                            UtilTokens.WriteAErrorMensage("bad format for tokens with parentesis", tokensExpressao, escopo);
                            return null;
                        }
                        tokensExpressao.RemoveRange(indiceParenteses, umGrupo.Count);

                        GruposEntreParenteses grupo = new GruposEntreParenteses(umGrupo, indiceParenteses);
                        grupos.Add(grupo);

                        offsetIndice = indiceParenteses + 1;
                    }
                }
                return tokensExpressao;

            }

            /// <summary>
            /// extrai tokens entre parenteses, formando uma lista de tokens resumidos.
            /// </summary>
            /// <param name="tokensExpressao">tokens com grupos de parenteses.</param>
            /// <param name="grupos">grupo de tokens entre parenteses.</param>
            /// <returns></returns>
            public static List<string> RemoveAndRegistryGroups(List<string> tokensExpressao, ref List<GruposEntreParenteses> grupos, Escopo escopo)
            {
                string codigoExpressao = Util.UtilString.UneLinhasLista(tokensExpressao);

                return RemoveAndRegistryGroups(codigoExpressao, ref grupos, escopo);

            }
            
            /// <summary>
            /// verifica se um token é nome de um operador.
            /// </summary>
            /// <param name="token">token possivelmente operador.</param>
            /// <param name="nameOperators">lista de todos operadores.</param>
            /// <returns>true se o token é operador, false se nao.</returns>
            private static bool isOperator(string token)
            {
                if (IsID(token))
                {
                    return false;
                }
                else
                 if (IsNumber(token))
                {
                    return false;
                }
                else
                 if (IsLiteral(token))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }


            private static bool IsLiteral(string exprssID)
            {
                if (exprssID.Contains(aspas.ToString()))
                    return true;
                else
                    return false;
            }



            /// <summary>
            /// verifica se a expressao nao eh um ID. um ID não contem caracteres como parenteses, colchetes, virgula, ponto-e-virgula, nomes de operadores.
            /// </summary>
            public static bool IsID(string exprssID)
            {

                if (nameOperators == null)
                {
                    AllOperators();
                }
                 

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

                for (int x = 0; x < tokensExpressao.Count; x++)
                {
                    if (exprssID.Contains(tokensExpressao[x]))
                    { 
                        return false;
                    }
                }

                if (nameOperators.Find(k => k.Equals(exprssID)) != null)
                {
                    return false;
                }
                    

                return true;

            }

            private static bool IsNumber(string token)
            {
                string numeros = "0 1 2 3 4 5 6 7 8 9";
                List<string> tokensNumeros = numeros.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();

                for (int c = 0; c < token.Length; c++)
                {
                    if (tokensNumeros.IndexOf(token[c].ToString()) == -1)
                        return false;
                }
                return true;
            }



            private static void AllOperators()
            {
                if (nameOperators == null)
                {
                    opBinaries = new List<string>();
                    opUnaries = new List<string>();
                    opBinariesAndUnaries = new List<string>();
                    nameOperators = new List<string>();

                    ExpressaoGrupos.GetNomesOperadoresBinariosEUnarios(ref opBinaries, ref opUnaries, ref opBinariesAndUnaries);

                    GruposEntreParenteses.nameOperators.AddRange(opBinaries);
                    GruposEntreParenteses.nameOperators.AddRange(opUnaries);
                    GruposEntreParenteses.nameOperators.AddRange(opBinariesAndUnaries);
   
                }
            }

            public override string ToString()
            {
                if (text != null)
                {
                    return text;
                }
                else
                {
                    return "indefined";
                }
            }
        }

        public class DataOperators
        {
            public string nameOperator;
            public int indexInListOfExpressions;
       
            public enum tipoComOperandos{ binary, unary, binaryAndUnary};
            public tipoComOperandos tipo;

            /// <summary>
            /// construtor.
            /// </summary>
            /// <param name="nameOperator">nome do operador.</param>
            /// <param name="indexInListOfExpressions">indice do token, com relacao a lista de expressoes extraidas.</param>
            /// <param name="typeOperator">tipo do operador: binario, unario binarioEUnario.</param>
            public DataOperators(string nameOperator, int indexInListOfExpressions, tipoComOperandos typeOperator)
            {
                this.nameOperator = nameOperator;
                this.indexInListOfExpressions = indexInListOfExpressions;
                this.tipo = typeOperator;

            }
            

            /// <summary>
            /// atualiza a lista de operadores, ante a insercao de um operador na lista de expressoes.
            /// </summary>
            /// <param name="datas">lista de dados contendo operadores.</param>
            public static void UpdateIndex(List<DataOperators> datas)
            {
                if ((datas==null) || (datas.Count==0))
                {
                    return;
                }
                for (int i = 0; i < datas.Count; i++) 
                {
                    datas[i].indexInListOfExpressions++;
                }
            }

            public override string ToString()
            {
                string type = "";
                switch (this.tipo)
                {
                    case tipoComOperandos.binary:
                        type = "BINARIO";
                        break;
                    case tipoComOperandos.unary:
                        type = "UNARIO";
                        break;
                    case tipoComOperandos.binaryAndUnary:
                        type = "BINARIO_E_UNARIO";
                        break;
                }


                return nameOperator + ": " + type;
            }
        }
    }
}

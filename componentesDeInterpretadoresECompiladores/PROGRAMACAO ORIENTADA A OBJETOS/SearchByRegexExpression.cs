using parser.textoFormatado;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Text.RegularExpressions;
using System.Linq;
using System.ComponentModel;
using System.Runtime.ExceptionServices;

using System.Runtime.CompilerServices;
using System.Security.Policy;

namespace parser
{


    public class SearchByRegexExpression
    {

        /// <summary>
        /// lista de searchs com pattern code que diferencia tipos de expressões.
        /// </summary>
        public static List<DataSearch> searchers
        {
            get;
            set;
        }


        /// <summary>
        /// pattern calculado automaticamente.
        /// </summary>
        public string patternResumed
        {
            get;
            set;
        }

        /// <summary>
        /// lista de patterns code com um só token na definição.
        /// </summary>
        private static List<string> patternsWithOneToken
        {
            get;
            set;
        }

        /// <summary>
        /// objeto regex, para localizar e extrair ids, exprss, operadores, numeros, literais, e expressoes.
        /// </summary>
        private Regex expressaoRegex
        {
            get;
            set;
        }




        /// <summary>
        /// tipo de expressão associada ao search regex. economiza um bocado de código
        /// no processamento de expressoes, através de searchers.
        /// </summary>
        public Type TipoExpressao
        {
            get;
            set;
        }






        // nome de operadores da linguagem orquidea.
        private static List<string> nomesOperadoresBinarios = null;

        // operadores unarios.
        private static List<string> nomesOperadoresUnarios = null;

        /// <summary>
        /// se uma expressão contém mais sub-expressões, como expressao operador, ou chamadas de metodo aninhada,
        /// ou propriedades aninhadas, é registrado nesta lista.
        /// </summary>
        public List<SearchByRegexExpression> subExpressoesSearch
        {
            get;
            set;
        }



        
        
        
        
        
        
        /// <summary>
        /// nome de objetos, numeros, literais texto, NILL, etc..., que tem um único token representativo.
        /// </summary>
        public List<SearchByRegexExpression> ids
        {
            get;
            set;
        }

        /// <summary>
        /// parte burocrática de registro de ocorrências em texto regex, do pattern resumed: "exprss".
        /// </summary>
        public List<TextExpression.Ocorrencia> exprss = new List<TextExpression.Ocorrencia>();

        /// <summary>
        /// parte burocrática mas importante para registro de operadores, de expressões ExpressaoOperador.
        /// </summary>
        public List<TextExpression.Ocorrencia> operators = new List<TextExpression.Ocorrencia>();





        /// <summary>
        /// parte importante para a solução das expressões: parâmetros são a diferenciação de um texto regex completo,
        /// é preciso isolar o problema de parâmetros, criando uma lista para ele, e resolver a parte.
        /// </summary>
        public List<SearchByRegexExpression> parametros = new List<SearchByRegexExpression>();

        /// <summary>
        /// grupo de parâmetros. Um grupo é um conjunto de parãmetros de uma chamada de método,
        /// que forma a única expressão que utiliza parâmetros.
        /// </summary>
        public List<GroupDataParameters> gruposDeParametros = new List<GroupDataParameters>();
        public List<TextExpression.Ocorrencia> todasOcorrencias = new List<TextExpression.Ocorrencia>();

       

        public int indexParameter
        {
            get;
            set;
        }

        /// <summary>
        /// texto de entrada, que sofre processamento de extração de parâmetros.
        /// </summary>
        public string input;

        /// <summary>
        /// texto de entrada, sem processamento de extração de parâmetros.
        /// </summary>
        public string inputRaw;


        public List<string> tokens
        {
            get;
            set;
        }

        List<string> tokensPatternAutomate
        {
            get;
            set;

        }

        public SearchByRegexExpression(string inputIn)
        {

            if ((searchers == null) || (searchers.Count == 0))
            {

                // inicializa a lista de dados de procura.
                searchers = new List<DataSearch>();

                // inicializa os searchers de pattern para regex expressions.
                searchers = GetTextsRegex(inputIn);

               

            }

            // inicializa lista de sub-expressoes.
            this.subExpressoesSearch = new List<SearchByRegexExpression>();


            // OBTEM OS TOKENS da expressão associada ao search a validar.
            this.tokens = new Tokens(inputIn).GetTokens();




            // obtem um input sem processamento de extração de parâmetros.
            this.inputRaw = (string)inputIn.Clone();

            // seta o imput sem processamento de parâmetros.
            this.input = (string)inputIn.Clone();





            // forma os grupos de parametros, e modifica a entrada input, após o processamento de extração de parâmetros.
            this.gruposDeParametros = GetParameters(ref this.input);



            // calcula um pattern automatizado resumido com a entrada [input], apos processamento de extração de parâmetros.
            this.patternResumed = SetPatternResumed(this.input);
            this.tokensPatternAutomate = new Tokens(patternResumed).GetTokens(); // guarda os tokens do pattern automatizado.

            TextExpression textExpression = new TextExpression();

            // forma a expressao regex, a partir pattern resumido, e instancia regex.
            this.expressaoRegex = new Regex(textExpression.FormaExpressaoRegularGenerica(this.patternResumed));




        }



        /// <summary>
        /// formata um texto de entrada, colocando espaçamento de 1 unidade, nos tokens do texto.
        /// </summary>
        /// <param name="input">texto a ser formatado.</param>
        public static string FormataEntrada(string input)
        {
            string inputOut = "";
            List<string> tokensInput = new Tokens(input).GetTokens();
            if (tokensInput != null)
                for (int x = 0; x < tokensInput.Count; x++)
                    inputOut += tokensInput[x] + " ";
            return inputOut.Trim(' ');
        }







        /// <summary>
        /// constroi o patter resumed automatizado, com o tipo de expressao
        /// </summary>
        private string SetPatternResumed(string input)
        {
            
            if (SearchByRegexExpression.nomesOperadoresBinarios == null)
            {
               GetNomesOperadoresBinarios(ref nomesOperadoresBinarios, ref nomesOperadoresUnarios);
            }


            List<string> tokensInput = new Tokens(input).GetTokens();

            bool isExpressaoEntreParenteses = false;
            int indexParentesesAbre = tokensInput.IndexOf("(");


            if (((indexParentesesAbre - 1 >= 0) && (!IsID(tokensInput[indexParentesesAbre - 1]))) ||
                (indexParentesesAbre == 0)) 
            {
                // case de expressao entre parênteses.
                isExpressaoEntreParenteses = true;
            }


            bool isOperator = false;
            if ((tokensInput.IndexOf("=") == -1) && (!isExpressaoEntreParenteses)) 
                for (int x = 0; x < tokensInput.Count; x++)
                {
                    if ((nomesOperadoresBinarios.IndexOf(tokensInput[x]) != -1) && (tokensInput.IndexOf("=") == -1)) 
                    {
                        isOperator = true;
                        break;
                    }
                    if ((nomesOperadoresUnarios.IndexOf(tokensInput[x]) != -1) && (tokensInput.IndexOf("=") == -1))
                    {
                        isOperator = true;
                        break;
                    }
                }







            TextExpression textExpression = new TextExpression();

            

            // trata do case em que é uma expressão entre parênteses,
            string padraoAutomatizado = "";
            if (isExpressaoEntreParenteses)
            {
                padraoAutomatizado = "(exprss)";
                return padraoAutomatizado;
            }




            if (isOperator)
                padraoAutomatizado = textExpression.FormaPatterResumedForOperators(input);
            else
                padraoAutomatizado = textExpression.FormaPatternResumed(input);




            return padraoAutomatizado;
        }


        /// <summary>
        /// procura um match entre pattern automatizado e pattern codigo.
        /// o match precisa ser exato, pois há condições do pattern codigo ser reconhecido inteiramente,
        /// em relação ao pattern automatizado.
        /// </summary>
        private bool MatchPatterns(List<string> tokensPatternAutomatizado, List<string> tokensPatternCodigo)
        {
            if ((tokensPatternAutomatizado == null) && (tokensPatternCodigo == null))
                return true;
            if ((tokensPatternAutomatizado != null) && (tokensPatternCodigo == null))
                return false;
            if ((tokensPatternAutomatizado == null) && (tokensPatternCodigo != null))
                return false;

            // se o pattern automatizado conter operadores, faz a conversão de tokens do pattern codigo, a fim de contabilizar
            // "exprss", que é o descrito nos patterns automatizado.
            if (HasOperator(tokensPatternAutomatizado))
            {
                for (int x = 0; x < tokensPatternCodigo.Count; x++)
                {
                    if (patternsWithOneToken.IndexOf(tokensPatternCodigo[x]) != -1)
                        tokensPatternCodigo[x] = "exprss";
                }
            }

            


            // prepara a verificação por tokens, determinando a quantidade de tokens a ser verificados.
            if (tokensPatternAutomatizado.Count < tokensPatternCodigo.Count)
                return false;
   


            // faz a verificação por tokens, utilizando casting para ids e express, pois as diferenças são minimas.
            // ids e numeros são convertidos para "exprss", pois são "exprss" também. Para compatibilizar com 
            // patterns automatizado para expressoes com operadores.
            for (int x = 0; x < tokensPatternCodigo.Count; x++)
            {
                if (!ValidaTokens(tokensPatternCodigo[x], tokensPatternAutomatizado[x]))
                    return false;
            }

            return true;

        }

        private bool ValidaTokens(string idCodigo, string idAutomate)
        {
            if (idCodigo == idAutomate)
                return true;

            if ((idCodigo == "number") && (idAutomate == "exprss"))
                return true;
            if ((idCodigo == "exprss") && (idAutomate == "number"))
                return true;

            if ((idCodigo == "id") && (idAutomate == "exprss"))
                return true;
            if ((idCodigo == "exprss") && (idAutomate == "id"))
                return true;
           
            
            if ((idCodigo == "id") && (idAutomate == "number"))
                return true;

            if ((idCodigo == "number") && (idAutomate == "number"))
                return true;

            if ((idCodigo == "id") && (idAutomate == "literal"))
                return true;

            if ((idCodigo == "literal") && (idAutomate == "literal"))
                return true;

            if ((ExpressaoNumero.isNumero(idCodigo)) && (ExpressaoNumero.isNumero(idAutomate)))
                return true;
            
            return false;
        }

        /// contem textos de procura por expressoes regex, para todas especificações de tipos de expressao.
        private List<DataSearch> GetTextsRegex(string input)
        {
            List<DataSearch> dataSeachers = new List<DataSearch>();

            // patterns code de expressões de instanciação.
            string codigoInstanciacao = "id id = exprss";
            

            string codigoAtribuicao0 = "id = exprss";
            string codigoAtribuicao1 = "id = id";
            string codigoAtribuicao2 = "id = - exprss";
            string codigoAtribuicao3 = "id = + exprss";
           

            
            // pattern code para expressão entre parênteses, não chamada de método.
            string codigoExpressaoEntreParenteses = "( exprss )";

            // patterns code para chamadas de metodo.
            string codigoChamadaDeMetodo = "id . id ( ) ";
            string codigoChamadaDeMetodo2 = "id . id ( id ";
            string codigoChamadaDeMetodo3 = "id . id ( exprss";

            string codigoChamadaDeMetodoSemObjetoCaller = "id ( )";
            string codigoChamadaDeMetodoSemObjetoCallerComParametros = "id ( exprss";
            string codigoChamadaDeMetodoSemObjetoCallerComParametros2 = "id ( id";

            // pattern code para propriedades aninhadas.
            string codigoPropriedadesAninhadas = "id . id"; 


            // patterns code para um token.
            string codigoNill = "NILL";
            string codigoLiteral = "literal";
            string codigosNumeros1 = "number"; 
            string codigosNumeros2 = "- number";
            string codigosNumeros3 = "+ number";
            string codigoObjeto = "id";


            string codigoTipoObjeto = "id";

            // lista todos patterns code com um só token:
            SearchByRegexExpression.patternsWithOneToken = new List<string>();
            SearchByRegexExpression.patternsWithOneToken.Add(codigoLiteral);
            SearchByRegexExpression.patternsWithOneToken.Add(codigosNumeros1);
            SearchByRegexExpression.patternsWithOneToken.Add(codigosNumeros2);
            SearchByRegexExpression.patternsWithOneToken.Add(codigosNumeros3);
            SearchByRegexExpression.patternsWithOneToken.Add(codigoNill);
            SearchByRegexExpression.patternsWithOneToken.Add(codigoObjeto);
            SearchByRegexExpression.patternsWithOneToken.Add(codigoTipoObjeto);

            if ((nomesOperadoresBinarios == null) || (nomesOperadoresBinarios.Count == 0))
            {
                GetNomesOperadoresBinarios(ref nomesOperadoresBinarios, ref nomesOperadoresUnarios);

            }

            if ((nomesOperadoresBinarios != null) && (nomesOperadoresUnarios != null))
            {


                for (int i = 0; i < nomesOperadoresBinarios.Count; i++)
                {
                    if (nomesOperadoresUnarios.FindIndex(k => k.Equals(nomesOperadoresBinarios[i])) != -1)
                    {
                        string codigoOperadorBinarioEUnario = nomesOperadoresBinarios[i] + "exprss";
                        dataSeachers.Add(new DataSearch(codigoOperadorBinarioEUnario, typeof(ExpressaoOperador)));
                        // o operador é unario e binario ao mesmo tempo.
                    }
                }
            }

            string codigoOperador = "";
            if (nomesOperadoresBinarios != null)
            {

                // forma expressoes para processamento de expressoes com operadores binarios;
                for (int x = 0; x < nomesOperadoresBinarios.Count; x++)
                {


                    // constroi padroes regex para cada operador.
                    codigoOperador = "exprss " + nomesOperadoresBinarios[x] + " exprss";
                    dataSeachers.Add(new DataSearch(codigoOperador, typeof(ExpressaoOperador)));
                }
            }

            if (nomesOperadoresUnarios != null)
            {
                // forma expressoes para processamento de expressoes com operadores unarios.
                for (int x = 0; x < nomesOperadoresUnarios.Count; x++)
                {


                    // constroi padroes regex para operador pos-ordem.
                    codigoOperador = "exprss " + nomesOperadoresUnarios[x];
                    dataSeachers.Add(new DataSearch(codigoOperador, typeof(ExpressaoOperador)));


                    // constroi padroes regex para operador pre-ordem
                    string codigoOperador2 = nomesOperadoresUnarios[x] + "exprss";
                    dataSeachers.Add(new DataSearch(codigoOperador2, typeof(ExpressaoOperador)));

                }


            }


            // atribuicao de objetos.
            dataSeachers.Add(new DataSearch(codigoAtribuicao0, typeof(ExpressaoAtribuicao))); 
            dataSeachers.Add(new DataSearch(codigoAtribuicao1, typeof(ExpressaoAtribuicao)));
            dataSeachers.Add(new DataSearch(codigoAtribuicao2, typeof(ExpressaoAtribuicao)));
            dataSeachers.Add(new DataSearch(codigoAtribuicao3, typeof(ExpressaoAtribuicao)));

            // instanciacao de objetos.
            dataSeachers.Add(new DataSearch(codigoInstanciacao, typeof(ExpressaoInstanciacao))); /// expressao de instanciacao com expressao de inicializacao.


            // expressao entre parenteses.
            dataSeachers.Add(new DataSearch(codigoExpressaoEntreParenteses, typeof(ExpressaoEntreParenteses))); // expressao entre parenteses.


            // funcoes.
            dataSeachers.Add(new DataSearch(codigoChamadaDeMetodoSemObjetoCaller, typeof(ExpressaoChamadaDeMetodo)));
            dataSeachers.Add(new DataSearch(codigoChamadaDeMetodoSemObjetoCallerComParametros, typeof(ExpressaoChamadaDeMetodo)));
            dataSeachers.Add(new DataSearch(codigoChamadaDeMetodoSemObjetoCallerComParametros2, typeof(ExpressaoChamadaDeMetodo)));

            // metodos.
            dataSeachers.Add(new DataSearch(codigoChamadaDeMetodo, typeof(ExpressaoChamadaDeMetodo))); // expressao chamada de metodo.
            dataSeachers.Add(new DataSearch(codigoChamadaDeMetodo2, typeof(ExpressaoChamadaDeMetodo))); // expressao chamada de metodo.
            dataSeachers.Add(new DataSearch(codigoChamadaDeMetodo3, typeof(ExpressaoChamadaDeMetodo))); // expressao chamada de metodo.





            dataSeachers.Add(new DataSearch(codigoPropriedadesAninhadas, typeof(ExpressaoPropriedadesAninhadas))); // expressao propriedades aninhadas.
            dataSeachers.Add(new DataSearch(codigosNumeros1, typeof(ExpressaoNumero)));
            dataSeachers.Add(new DataSearch(codigosNumeros2, typeof(ExpressaoNumero)));
            dataSeachers.Add(new DataSearch(codigosNumeros3, typeof(ExpressaoNumero)));
            dataSeachers.Add(new DataSearch(codigoNill, typeof(ExpressaoNILL))); // expressao para texto NILL.
            dataSeachers.Add(new DataSearch(codigoLiteral, typeof(ExpressaoLiteralText))); // expressao para expressao literal texto constante.
            dataSeachers.Add(new DataSearch(codigoObjeto, typeof(ExpressaoObjeto)));



            // ordena decrescentemente pela quantidade de tokens os pattern codes do regex.
            ComparerDataSearch compare = new ComparerDataSearch();
            searchers.Sort(compare);


            return dataSeachers;
        }

        private static void GetNomesOperadoresBinarios(ref List<string> nomesOperadoresBinarios, ref List<string>nomesOperadoresUnarios)
        {
            if ((nomesOperadoresBinarios != null) && (nomesOperadoresBinarios.Count > 0))
                return;
            else
            {
                nomesOperadoresBinarios = new List<string>();
                nomesOperadoresUnarios = new List<string>();

                if ((Expressao.headers == null) || (Expressao.headers.cabecalhoDeClasses == null) || (Expressao.headers.cabecalhoDeClasses.Count == 0))
                    Expressao.InitHeaders("");

                // encontra todos nomes de operadores, registrados nas classes base e classe do codigo.
                List<HeaderClass> headers = Expressao.headers.cabecalhoDeClasses;




                for (int x = 0; x < headers.Count; x++)
                {
                    List<HeaderOperator> todosOperadoresDeUmaClasse = headers[x].operators;



                    if (todosOperadoresDeUmaClasse != null)
                    {
                        for (int i = 0; i < todosOperadoresDeUmaClasse.Count; i++)
                        {
                            if ((nomesOperadoresBinarios.IndexOf(todosOperadoresDeUmaClasse[i].name) == -1) &&
                                (todosOperadoresDeUmaClasse[i].tipoDoOperador == HeaderOperator.typeOperator.binary))
                                nomesOperadoresBinarios.Add(todosOperadoresDeUmaClasse[i].name);

                            
                            if ((nomesOperadoresUnarios.IndexOf(todosOperadoresDeUmaClasse[i].name) == -1) &&
                                ((todosOperadoresDeUmaClasse[i].tipoDoOperador == HeaderOperator.typeOperator.unary_pos) || 
                                (todosOperadoresDeUmaClasse[i].tipoDoOperador == HeaderOperator.typeOperator.unary_pre)))
                                nomesOperadoresUnarios.Add(todosOperadoresDeUmaClasse[i].name);

                        }
                    }
                }

                // registra operadores que sao binarios e unarios ao mesmo tempo, não há um metodo para implementar este operador unarios.
                nomesOperadoresUnarios.Add("+");
                nomesOperadoresUnarios.Add("-");
                // remove o operador "=", da lista de operadores, pois é um operador de instanciação,
                // não operador matemático, condicional ou booleano.
                nomesOperadoresBinarios.Remove("=");
                nomesOperadoresUnarios.Remove("=");

            }
        }


        public void ProcessingPattern()
        {
            // obtem todos searchers registrado.
            if ((searchers == null) || (searchers.Count == 0))
            {
                searchers = GetTextsRegex(input);
            }
                

            if ((input == null) || (input.Length == 0))
            {
                return;
            }
                


            Match result = this.expressaoRegex.Match(input);
            if (result.Success)
            {


                // forma uma regex com o pattern automatizado, construindo um pattern regex a partir do pattern reduzido automatizado.
                Regex regex = this.expressaoRegex;

                // obtem o valor da avaliação de regex.
                string valorRegex = regex.Match(input).Value;
                // se nao encontrou um match, passa para o proximo searchers.
                if (valorRegex == null)
                    return;


                GetTipoExpressao(this, this.patternResumed, this.input, searchers);




                // obtem ocorrencias de ids.
                List<TextExpression.Ocorrencia> id_ocurrences = TextExpression.GetOcurrencesGroup(this.patternResumed, this.input, "id", "number", "literal");

                // obtem ocorrencias de exprss.
                this.exprss = TextExpression.GetOcurrencesGroup(this.patternResumed, this.inputRaw, "exprss");

                // obtem ocorrencias de operadores.
                this.operators = TextExpression.GetOcurrencesOperators(this.patternResumed, this.inputRaw);


                // compoe lista contendo todos ids, exprss, e operators.
                this.todasOcorrencias.AddRange(id_ocurrences); // ocorrencias de ids.

                if ((this.exprss != null) && (this.exprss.Count > 0))
                    this.todasOcorrencias.AddRange(this.exprss);

                if ((this.operators != null) && (this.operators.Count > 0))
                    this.todasOcorrencias.AddRange(this.operators);



                // ordena a lista de acordo com o indice do token no input.
                TextExpression.ComparerOcorrencias comparer = new TextExpression.ComparerOcorrencias();
                this.todasOcorrencias.Sort(comparer);


                // forma as searchs id, sem recursão, pois são expressoes que não tem outras sub-expressoes, ou parâmetros.
                if ((id_ocurrences != null) && (id_ocurrences.Count > 0))
                {
                    this.ids = new List<SearchByRegexExpression>();
                    // obtem a expressao regex para o id ocurrente.
                    SearchByRegexExpression expsSearch = new SearchByRegexExpression(id_ocurrences[0].text);
                    // obtem o tipo do id currente.
                    GetTipoExpressao(expsSearch, expsSearch.patternResumed, expsSearch.input, searchers);
                    
                    // retorna se encontrou um tipo de expressao nao nulo.
                    if (TipoExpressao != null)
                    {
                        for (int k = 0; k < id_ocurrences.Count; k++)
                        {
                            this.ids.Add(new SearchByRegexExpression(id_ocurrences[k].text));
                        }

                        return;
                    }
                }

                // forma sub-expressoes de exprss econtrado. busca recursiva para mais expressoes.
                if (exprss != null)
                {

                    for (int i = 0; i < exprss.Count; i++)
                    {



                        // obtem um input com espaçamento.
                        string input_sub_expression = exprss[i].text;


                        // busca recursiva para identificar o tipo da sub-expressao vindo de "exprss[i]".
                        SearchByRegexExpression sub_expressao_search = new SearchByRegexExpression(input_sub_expression);
                        sub_expressao_search.ProcessingPattern();



                        // adiciona a sub-expressao search, para fins de obter sub-expressoes da classe "Expressao".
                        this.subExpressoesSearch.Add(sub_expressao_search);

                    }

                }

            }

            // procede o processamento de propriedades aninhadas, ou chamadas de metodo aninhadas,
            // de mais de uma chamada de metodo, ou mais de uma propriedade aninhada.
            this.ProcessamentoDeAninhamento(this, input);






            /// faz o processamento de parametros. acrescenta um grupo de parametros, para a sub-expressao currente.
            if (CountTokens(input) > 1)
            {
                int offsetParenteses2 = 0;
                ProcessingParameters(this, ref offsetParenteses2, this.gruposDeParametros);
            }



        }


        /// <summary>
        /// é preciso inicializar antes o search de retorno.
        /// </summary>
        /// <param name="searchRetorno">seacher retorno</param>
        /// <param name="patternResumed">pattern reduzido para buscas regex.</param>
        /// <param name="input">expressao a ser parseada.</param>
        /// <param name="searchers">lista de seacher para tipos de Expressao</param>
        private void GetTipoExpressao(SearchByRegexExpression searchRetorno, string patternResumed, string input, List<DataSearch> searchers)
        {
          

            // procura um pattern codigo compativel com o pattern automatizado, a fim de obter o tipo da expressao encontrada.
            for (int x = 0; x < searchers.Count; x++)
            {
                // para obter o tipo de expressao, procura combinar o pattern automatizado com o pattern codigo (que possui o tipo de expressao):
                if (MatchPatterns(this.tokensPatternAutomate, searchers[x].tokensPatternCode))
                {

                    // obtem o tipo de expressão que match com o pattern resumed do searchers[x].
                    searchRetorno.TipoExpressao = searchers[x].tipoDaExpressao;

                    return;

                }
            }
        }

        private bool ProcessamentoDeAninhamento(SearchByRegexExpression searchCurrent, string __input)
        {
            string input = FormataEntrada((string)__input.Clone());


            int countTokensDot = CountOneToken(input, ".");
            int countGroupsFound = 0;

            if (countTokensDot > 1)
            {

                List<string> tokens = new Tokens(input).GetTokens();

                


                int indexPreviousDotOperator = 0;
                int idParentesesInput = 0;
                for (int x = 1; x < countTokensDot; x++)
                {

                    indexPreviousDotOperator = GetNTokenFromListTokens(x, tokens, "."); /// indice do "dot" anterior.
                    int indexCurrentDotOperator = GetNTokenFromListTokens(x + 1, tokens, "."); // indice do "dot" currente para processamento de aninhamento.



                    // utilizado para match de situações de aninhamento de chamadas de metodo e aninhamento de propriedades.
                    int idParentesesTokens = IndexWithMinimaListTokens(indexCurrentDotOperator, "(", tokens);



                    // indice para match de grupos de parâmetros..
                    idParentesesInput = IndexWithMinimalCountText(countGroupsFound, "(", input);





                    
                    if ((indexCurrentDotOperator + 2 == idParentesesTokens) &&  // identifica "a.propriedadeA.metodoA()", e "a.metodoA().metodoB()".
                       ((indexCurrentDotOperator - indexPreviousDotOperator) <= 4)) // tem que diferenciar de "a.metodoA() + b.metodoB()"
                    {


                        string nomeMetodo = tokens[idParentesesTokens - 1];


                        // formata o texto da chamada de metodo, sem parâmetros.
                        string chamadaDeMetodoAninhada = FormataEntrada(nomeMetodo + "( " + ")");




                        SearchByRegexExpression searchChamadaAninhada = new SearchByRegexExpression(chamadaDeMetodoAninhada);
                        searchChamadaAninhada.ProcessingPattern();


                        // chamadas de metodo tem parâmetros, é necessário obter parametros da chamada de metodo.
                        if ((searchCurrent.gruposDeParametros != null) && (searchCurrent.gruposDeParametros.Count > 0))
                        {
                            for (int indexGroup = 0; indexGroup < searchCurrent.gruposDeParametros.Count; indexGroup++)
                            {
                                if (searchCurrent.gruposDeParametros[indexGroup].indexBeginGroup == idParentesesInput)
                                {
                                    // encontrou um grupo de parâmetros, incrementa o contador para um proximo match de grupos ocorrer, no indice certo..
                                    countGroupsFound++;

                                    /// faz o processamento dos parâmetros da chamada de metodo aninhada.
                                    for (int parametro = 0; parametro < searchCurrent.gruposDeParametros[indexGroup].parametrosDoGrupo.Count; parametro++)
                                    {

                                        // constroi o search do parâmetro, com o texto do parâmetro.
                                        SearchByRegexExpression searchParameter = new SearchByRegexExpression(
                                            searchCurrent.gruposDeParametros[indexGroup].parametrosDoGrupo[parametro].inputParameter);

                                        // faz o processamento do parâmetro, que pode ser chamadas de metodos, objetos, constantes, numeros,...
                                        searchParameter.ProcessingPattern();



                                        // adiciona um parametro, para a lista de parametros da chamada de metodo aninhada.
                                        if (searchParameter != null)
                                        {
                                            searchChamadaAninhada.parametros.Add(searchParameter);

                                        }
                                    }
                                }
                            }





                        } // if searchCurrent


                        // adiciona a search da chamada de metodo aninhada, para a search currente.
                        searchCurrent.subExpressoesSearch.Add(searchChamadaAninhada);

                    } // if indexParentesesEmTokens.
                    else
                     // é uma propriedade aninhada. ex.: proprieadade1.propriedade2.propriedade3;
                     if (indexPreviousDotOperator + 2 == indexCurrentDotOperator)
                    {


                        int indexDotAninhadoProximo = this.GetNTokenFromListTokens(x, tokens, ".");

                        if (indexDotAninhadoProximo + 4 < tokens.Count) 
                        {
                            
                         
                            // formata a entrada da propriedade aninhada.
                            string chamadaPropriedadeAninhada = FormataEntrada(tokens[indexDotAninhadoProximo + 1] + "." + tokens[indexDotAninhadoProximo+3]);


                            // propriedades aninhadas não tem parâmetros, uma vez localizada, instancia uma search.
                            SearchByRegexExpression searchPropriedadeAninhada = new SearchByRegexExpression(chamadaPropriedadeAninhada);
                            searchPropriedadeAninhada.ProcessingPattern();


                            searchCurrent.subExpressoesSearch.Add(searchPropriedadeAninhada);
                        }

                    }

                } // for x

                // houve processamento de aninhamento.
                return true;
            }

            // não houve processamento de aninhamento.
            return false;
        }


  

        private int IndexWithMinimaListTokens(int indexMinimum, string token, List<string> tokens)
        {
            int index = 0;
            while ((index != -1) && (index < indexMinimum))
                index = tokens.IndexOf(token, index + 1);

            return index;
        }


        private int IndexWithMinimalCountText(int count, string token, string input)
        {
            int contador = 0;
            int index = input.IndexOf(token, 0);
            while ((index != -1) && (contador < count))
            {
                index = input.IndexOf(token, index + 1);
                contador++;
            }
                
            return index;
        }


        private int GetNTokenFromListTokens(int countNTokens, List<string> tokens, string token)
        {
            int indexBegin = 0;
            int indexOffset = 0;
            for (int i = 0; i < countNTokens; i++)
            {
                indexBegin = tokens.IndexOf(token, indexOffset);
                indexOffset = indexBegin + 1;
            }

            return indexBegin;
        }

        private void ProcessingParameters(SearchByRegexExpression sub_expressao_currente, ref int offsetIndiceAbreParenteses, List<GroupDataParameters> grupos)
        {

            // busca recursiva para encontrar parametros da search currente.
            if (grupos != null)
            {

                // obtem o indice de parenteses abre, no string "input".
                int indexParentesesAbre = input.IndexOf("(", offsetIndiceAbreParenteses);

                // atualiza o indice offset de procura de parenteses abre...
                offsetIndiceAbreParenteses = indexParentesesAbre + 1;



                // associa o grupo com indice do parenteses abre igual à entrada de parentes currente do input.
                for (int indexGrupo = 0; indexGrupo < grupos.Count; indexGrupo++)
                {

                    if (grupos[indexGrupo].indexBeginGroup == indexParentesesAbre)
                    {

                        if (grupos[indexGrupo].parametrosDoGrupo != null)
                            for (int indexParametro = 0; indexParametro < grupos[indexGrupo].parametrosDoGrupo.Count; indexParametro++)
                            {
                                SearchByRegexExpression serachParametro = new SearchByRegexExpression(grupos[indexGrupo].parametrosDoGrupo[indexParametro].inputParameter);
                                serachParametro.ProcessingPattern();

                                if (serachParametro != null)
                                {
                                    sub_expressao_currente.parametros.Add(serachParametro);
                                }
                            }
                        // faz o processamento de um grupo só, para a entrada no input. o grupo foi encontrado como a entrada mais próxima de inserção, baseada
                        // no indice do parenteses abre, mais proximo.
                        return;
                    }
                }

            }
        }




        private bool HasOperator(List<string> tokensInput)
        {
            if ((nomesOperadoresBinarios == null) || (nomesOperadoresBinarios.Count == 0))
            {
                GetNomesOperadoresBinarios(ref nomesOperadoresBinarios,ref nomesOperadoresUnarios);
            }

            if ((tokensInput == null) || (tokensInput.Count == 0))
                return false;

            int pilhaParenteses = 0;

            for (int x = 0; x < tokensInput.Count; x++)
            {
                // contagem de parenteses, para verificar se os operadores estão num parametro
                // de chamada de metodo, por ex., o que não é desejado por procura de operadores.
                if (tokensInput[x] == "(")
                    pilhaParenteses += 1;

                if (tokensInput[x] == ")")
                    pilhaParenteses -= 1;

                // se um operador foi encontrado, e os parênteses estão certos (sem entrada em uma chamada de metodo, por exemplo), retorna [true] há operador.
                if ((nomesOperadoresBinarios.IndexOf(tokensInput[x]) != -1) && (pilhaParenteses == 0))
                    return true;
            }
            return false;

        }


        private int CountOneToken(string input, string token)
        {
            int count = 0;
            int offsetCount = 0;
            int index = input.IndexOf(token, 0);

            while (index != -1)
            {
                count++;
                offsetCount = index + 1;
                index = input.IndexOf(token, offsetCount);

            }

            return count;
        }
        private int CountTokens(string input)
        {
            if ((input == null) || (input.Length == 0))
                return 0;
            else
            {
                List<string> tokensInput = new Tokens(input).GetTokens();
                if ((tokensInput == null) || (tokensInput.Count == 0))
                {
                    return 0;
                }
                else
                {
                    return tokensInput.Count;
                }

            }

        }



        /// <summary>
        /// verifica se a expressao nao eh um ID. um ID não contem caracteres como parenteses, colchetes, virgula, ponto-e-virgula, nomes de operadores.
        /// </summary>
        private static bool IsID(string exprssID)
        {
            switch (exprssID)
            {
                case "(":
                case ")":
                case "=":
                case "[":
                case "]":
                case ".":
                case ",":
                case ";":
                    return false;
            }

            if (nomesOperadoresBinarios.Find(k => k.Equals(exprssID)) != null)
                return false;

            return true;

        }


        public override string ToString()
        {
            string str_tokens = "";
            if ((tokens != null) && (tokens.Count > 0))
                str_tokens = Utils.OneLineTokens(tokens);
            if (this.TipoExpressao != null)
                return this.TipoExpressao.Name + " : " + str_tokens;
            else
                return str_tokens;
        }


        public class DataSearch
        {
            public string patternCode
            {
                get;
                set;
            }


            public Type tipoDaExpressao
            {
                get;
                set;
            }

            public List<string> tokensPatternCode
            {
                get;
                set;
            }
            public DataSearch(string patternCode, Type tipoDaExpressao)
            {
                this.patternCode = patternCode;
                this.tipoDaExpressao = tipoDaExpressao;
                this.tokensPatternCode = new Tokens(patternCode).GetTokens();

            }

            public override string ToString()
            {
                return this.tipoDaExpressao.Name;
            }
        }

        public class GroupDataParameters
        {

            public List<string> idGrup
            {
                get;
                set;
            }
            public List<DataParameter> parametrosDoGrupo
            {
                get;
                set;
            }

            public int indexBeginGroup
            {
                get;
                set;
            }

            public GroupDataParameters(List<DataParameter> parameters, int indexBeginGroup)
            {
                this.parametrosDoGrupo = parameters;
                this.indexBeginGroup = indexBeginGroup;
                this.idGrup = new List<string>();
            }

            public GroupDataParameters()
            {
                this.parametrosDoGrupo = new List<DataParameter>();
                this.indexBeginGroup = -1;
                this.idGrup = new List<string>();
            }

            public GroupDataParameters(List<string> id)
            {
                this.idGrup = id.ToList<string>();
            }

            public void Add(DataParameter parameter)
            {

                if (this.parametrosDoGrupo == null)
                    this.parametrosDoGrupo = new List<DataParameter>();

                this.parametrosDoGrupo.Add(parameter);

            }

            public override string ToString()
            {
                string str = "";
                if (this.parametrosDoGrupo != null)
                {
                    for (int x = 0; x < parametrosDoGrupo.Count - 1; x++)
                    {
                        str += parametrosDoGrupo[x].inputParameter + ", ";
                    }
                    str += parametrosDoGrupo[parametrosDoGrupo.Count - 1].inputParameter;
                }

                return str;
            }
        }

        public class DataParameter
        {


            public int index
            {
                get;
                set;
            }

            public string inputParameter
            {
                get;
                set;
            }

            public List<string> tokensPatternCode
            {
                get;
                set;
            }

            public DataParameter(int indexPrimeiroParenteses, string textParameter)
            {
                this.index = indexPrimeiroParenteses;
                this.inputParameter = textParameter.Trim(' ');
                this.tokensPatternCode = new Tokens(textParameter).GetTokens();

            }

            public override string ToString()
            {
                if (this.inputParameter != null)
                    return "posicao: " + index + "  text: " + inputParameter;
                else
                    return "empty";
            }

        }
        private List<GroupDataParameters> GetParameters(ref string input)
        {
            /// obtem grupos de parametros.
            /// cada grupo de parametros está delimitado por parenteses abre e fecha, E pilha de parenteses = 0.
            ///         --- cada parametro dentro do grupo, é delimitado por virgula, e pára quando o token currente é virgula, E pilha de parenteses==0.
            ///             quando atinge o token final da lista de tokens delimitado por parenteses, que é calculado para capturar tokens entre parenteses abre e fecha, e com pilha de parenteses=0,
            ///             É recalculado a lista de tokens, removendo a antiga lista de tokens delimitado por parenteses, da lista de tokens total...


            if ((input == null) || (input.Length == 0) || (input.IndexOf("(") == -1))
            {
                return new List<GroupDataParameters>();
            }
            else
            {

                //  exemplo: "a.metodoB(a+b+c+metodoB(x), a+b, a.metodoB(y)) + metodoC(x)";
                List<string> tokensRaw = new Tokens(input).GetTokens();
                int pilhaParenteses = 0;

                int indexParentesesAbreEmTokens = 0;

                List<GroupDataParameters> gruposParametros = new List<GroupDataParameters>();


                while (tokensRaw.Count > 0)
                {
                    int indexParentesesAbre = tokensRaw.IndexOf("(");
                    // se não haver mais tokens de parenteses, nada ha mais para processar, retorna a lista de grupos de parametros.
                    if (indexParentesesAbre == -1)
                        return gruposParametros;

                    //****************************************************************************************************************************
                    bool isExpressaoEntreParenteses = false;
                    // o case é de uma expressao entre parenteses, pois o token anterior não é um "id", se tiver "id", é uma expressao chamada de metodo.
                    indexParentesesAbreEmTokens= tokensRaw.IndexOf("(", indexParentesesAbreEmTokens);
                    if (((indexParentesesAbreEmTokens - 1 >= 0) && (!IsID(tokensRaw[indexParentesesAbreEmTokens - 1])))
                        || (indexParentesesAbreEmTokens <= 0))
                    {

                        isExpressaoEntreParenteses = true;
                    }
                    else
                        isExpressaoEntreParenteses = false;


                    // remove os tokens da expressao entre parenteses,pois não há parametros.
                    if (isExpressaoEntreParenteses)
                    {
                        List<string> tokensExpressaoParenteses = UtilTokens.GetCodigoEntreOperadores(indexParentesesAbreEmTokens, "(", ")", tokensRaw);
                        if ((tokensExpressaoParenteses != null) && (tokensExpressaoParenteses.Count > 2))
                        {
                            // remove os tokens da expressao entre parenteses, porque esta expressão não tem parâmetros.
                            tokensRaw.RemoveRange(indexParentesesAbreEmTokens, tokensExpressaoParenteses.Count - indexParentesesAbre);
                            continue;
                        }
                    }
                   
                    //*****************************************************************************************************************************
                    // o case é de extrair parameetros dos tokens.
                    List<string> tokensDeUmGrupo = UtilTokens.GetCodigoEntreOperadores(indexParentesesAbre, "(", ")", tokensRaw);

                    // se nao houver mais grupo de parametros, retorna a lista de grupos de parametros currente.
                    if ((tokensDeUmGrupo == null) || (tokensDeUmGrupo.Count == 0))
                        return gruposParametros;


                    GroupDataParameters umGrupo = new GroupDataParameters(tokensDeUmGrupo);





                    // passa para o proximo grupo de parametros, ex: se "a.metodoB(x)+ c.metodoC(x+1), o proximo grupo de parametros será: "x+1".
                    tokensRaw.RemoveRange(0, indexParentesesAbre + 1); // +1 porque é contador de dimensão (size(list)), nao indice, que começa a partir de 0.
                    tokensRaw.RemoveRange(0, tokensDeUmGrupo.Count - 1); // remove os tokens entre os parenteses, 

                    // remove os parenteses da lista de tokens do grupo de parametros currente.
                    if (tokensDeUmGrupo.Count > 0)
                        tokensDeUmGrupo.RemoveAt(0);


                    if (tokensDeUmGrupo.Count > 0)
                        tokensDeUmGrupo.RemoveAt(tokensDeUmGrupo.Count - 1);




                    if ((tokensDeUmGrupo == null) || (tokensDeUmGrupo.Count == 0))
                        return new List<GroupDataParameters>();

                    // "a.metodoA(b.metodoC(x,y,z)+1) + b.metodoB(n,m)";
                    int indexToken = 0;
                    indexParentesesAbre = 0;

                    while (indexToken < tokensDeUmGrupo.Count)
                    {
                        switch (tokensDeUmGrupo[indexToken])
                        {
                            case "(":
                                pilhaParenteses++;
                                break;
                            case ")":
                                pilhaParenteses--;
                                if (pilhaParenteses < 0)
                                {
                                    // erro no codigo.
                                    return new List<GroupDataParameters>();
                                }
                                break;

                            case ",":
                                if (pilhaParenteses == 0)
                                {
                                    // -1 para nao conter o token virgula, +1 por que contador de dimensão (tamanho da lista), não indice (que começa a partir de 0).
                                    List<string> umParametro = tokensDeUmGrupo.GetRange(indexParentesesAbre, indexToken - 1 - indexParentesesAbre + 1);
                                    string str_parametro = Utils.OneLineTokens(umParametro);


                                    // adiciona um parametro no grupo de parametros currente.
                                    umGrupo.Add(new DataParameter(indexToken, str_parametro.Trim(' ')));



                                    // atualiza o indice de começo de um parametro do grupo.
                                    indexParentesesAbre = indexToken + 1;




                                }
                                break;

                        }
                        indexToken++;

                    }

                    if (indexParentesesAbre != -1)
                    {
                        List<string> ultimoParametro = tokensDeUmGrupo.GetRange(indexParentesesAbre, tokensDeUmGrupo.Count - indexParentesesAbre);
                        string str_ultimoParametro = Utils.OneLineTokens(ultimoParametro);



                        umGrupo.Add(new DataParameter(indexParentesesAbre, str_ultimoParametro.Trim(' ')));


                    }


                    gruposParametros.Add(umGrupo);

                } // while tokensRaw


                int offsetParentesesAbre = 0;
                input = FormataEntrada(input);
                for (int indexGroup = 0; indexGroup < gruposParametros.Count; indexGroup++)
                {

                    // forma 
                    string inputID_do_grupo = Utils.OneLineTokens(gruposParametros[indexGroup].idGrup);

                    if ((inputID_do_grupo == null) || (inputID_do_grupo.Length == 0))
                        throw new Exception("Error in formating parameters, in input= " + input);


                    // formata o input do grupo.
                    inputID_do_grupo = FormataEntrada(inputID_do_grupo);


                    // encontra o indice do grupo, como indice de entrada na string input.
                    int index_um_grupo = input.IndexOf(inputID_do_grupo, offsetParentesesAbre);
                    if (index_um_grupo == -1)
                        throw new Exception("Error in formating parameters, in input= " + input);

                    offsetParentesesAbre = index_um_grupo + 1;

                    // acerta o indice de inserção do grupo.
                    gruposParametros[indexGroup].indexBeginGroup = index_um_grupo;

                    // remove o input do grupo, na string input parametro de entrada.
                    input = input.Remove(index_um_grupo, inputID_do_grupo.Length);

                    // adiciona os parenteses do input do grupo.
                    input = input.Insert(index_um_grupo, "( )");


                    input = FormataEntrada(input);





                    // obtem o indice de começo do grupo, como uma substring de input.
                    gruposParametros[indexGroup].indexBeginGroup = index_um_grupo;

                }




                return gruposParametros;

            }


        }

        public string ResumeInput(SearchByRegexExpression search, string __input)
        {
            string input = (string)__input.Clone();

        
            if (search.TipoExpressao== typeof(ExpressaoChamadaDeMetodo))
            {
                input+= input.Replace(search.inputRaw, "ID ");
            }
            else
            if (search.TipoExpressao == typeof(ExpressaoPropriedadesAninhadas))
            {
                input = input.Replace(search.inputRaw, "ID ");
            }
            else
            if (search.TipoExpressao == typeof(ExpressaoObjeto))
            {
                input += "ID  ";
            }
            else
            if (search.TipoExpressao == typeof(ExpressaoNumero))
            {
                input += "ID ";
            }
            else
            if (search.TipoExpressao == typeof(ExpressaoLiteralText))
            {
                input += "ID ";
            }
            

            return input;
        }

        private class ComparerDataSearch : IComparer<DataSearch>
        {


            // compara textos de procura, se o search contem pattern reduzido com "exprss", que vem em primeiro,
            // se nao, compara decrescentemento de acordo com o tamanho da lista de tokens. 
            public int Compare(DataSearch x, DataSearch y)
            {
                int countTokensX = x.tokensPatternCode.Count;
                int countTokensY = y.tokensPatternCode.Count;

                if (countTokensX>countTokensY)
                    return -1;
                if (countTokensX < countTokensY)
                    return +1;
                else return 0;

            }
        }


        public class Testes : SuiteClasseTestes
        {
            public Testes() : base("testes para classe extratora de sub-expressoes atraves de expressoes regex.")
            {
            }

            public void TesteOperadorUnario(AssercaoSuiteClasse assercao)
            {
                string input = "a++";

                SearchByRegexExpression search = new SearchByRegexExpression(input);
                search.ProcessingPattern();


                assercao.IsTrue(
                    search != null &&
                    search.TipoExpressao == typeof(ExpressaoOperador));
                   
            }


            public void Teste2PropriedadesAninhadasCom1ChamadaDeMetodo(AssercaoSuiteClasse assercao)
            {
                string input = "obj.propriedadeA.propriedadeB.metodoB(x,y,z)";

                SearchByRegexExpression search = new SearchByRegexExpression(input);
                search.ProcessingPattern();


                assercao.IsTrue(
                    search != null &&
                    search.TipoExpressao == typeof(ExpressaoPropriedadesAninhadas) &&
                    search.subExpressoesSearch != null &&
                    search.subExpressoesSearch.Count == 2 &&
                    search.subExpressoesSearch[1].parametros != null &&
                    search.subExpressoesSearch[1].parametros.Count == 3);
            }



            public void TesteFormacaoDeExpressaoInstanciacao(AssercaoSuiteClasse assercao)
            {
                string input = "int a = 1";
                SearchByRegexExpression search = new SearchByRegexExpression(input);
                search.ProcessingPattern();


                // teste  automatizado.
                assercao.IsTrue(search != null &&
                                search.TipoExpressao == typeof(ExpressaoInstanciacao) &&
                                search.ids != null &&
                                search.ids.Count == 3);
            }

            public void TesteExpressaoEntreParenteses(AssercaoSuiteClasse assercao)
            {
                string input = "(a+b+c)";
                SearchByRegexExpression search = new SearchByRegexExpression(input);
                search.ProcessingPattern();

                // teste nao automatizado.
                assercao.IsTrue(
                    search != null &&
                    search.TipoExpressao == typeof(ExpressaoEntreParenteses) &&
                    search.exprss.Count == 1 &&
                    search.subExpressoesSearch != null &&
                    search.subExpressoesSearch.Count > 0 &&
                    search.subExpressoesSearch[0].TipoExpressao == typeof(ExpressaoOperador));


            }


            public void TesteExpressaoLiteral(AssercaoSuiteClasse assercao)
            {
                string aspas = '\u0022'.ToString();
                string input = aspas + "nome" + aspas;

                Expressao exprssMain = new Expressao();
                exprssMain.tokens = new List<string>();

                SearchByRegexExpression search = new SearchByRegexExpression(input);
                search.ProcessingPattern();

                // teste automatizado.
                assercao.IsTrue(search.TipoExpressao == typeof(ExpressaoLiteralText));
            }

            public void TesteComposicaoDeChamadaDeMetodoComOperadorMaisSeguidoDeObjeto(AssercaoSuiteClasse assercao)
            {
                string input = "b.metodoC(x,y,z)+c";

                SearchByRegexExpression serach = new SearchByRegexExpression(input);
                serach.ProcessingPattern();


                // teste automatizado.
                assercao.IsTrue(
                    serach != null &&
                    serach.subExpressoesSearch != null &&
                    serach.subExpressoesSearch.Count == 2 &&
                    serach.subExpressoesSearch[0].TipoExpressao == typeof(ExpressaoChamadaDeMetodo) &&
                    serach.subExpressoesSearch[1].TipoExpressao == typeof(ExpressaoObjeto));
            }



  
            public void TesteFormacaoDeExpressaoInstanciacaoComChamadaDeMetodo(AssercaoSuiteClasse assercao)
            {
                string input = "int a = metodoA()";
                SearchByRegexExpression search = new SearchByRegexExpression(input);
                search.ProcessingPattern();

                // teste automatizado.
                assercao.IsTrue(
                    search != null &&
                    search.TipoExpressao == typeof(ExpressaoInstanciacao));

            }

  


      
            public void TesteDuasChamadasDeMetodoAninhadasSemParametros(AssercaoSuiteClasse assercao)
            {
                /// codigo incompleto, não está ainda extraindo os parâmetros de cada chamada de metodo.
                string input = "obj.metodoA().metodoB()";

                SearchByRegexExpression search = new SearchByRegexExpression(input);
                search.ProcessingPattern();

                // teste automatizado.
                assercao.IsTrue(
                    search != null &&
                    search.subExpressoesSearch != null &&
                    search.subExpressoesSearch.Count == 1 &&
                    search.subExpressoesSearch[0].TipoExpressao == typeof(ExpressaoChamadaDeMetodo));
            }




            public void Teste2PropriedadesAninhadas(AssercaoSuiteClasse assercao)
            {
                string input = "obj.propriedadeB.propriedadeC";

                SearchByRegexExpression search = new SearchByRegexExpression(input);
                search.ProcessingPattern();

                assercao.IsTrue(search != null &&
                                search.TipoExpressao == typeof(ExpressaoPropriedadesAninhadas));
            }


            public void TesteComposicaoVariosParametros2ChamadasDeMetodo(AssercaoSuiteClasse assercao)
            {
                // entrada com duas chamadas de metodo e varios parametros.
                string input = "a.metodoA(x,y,z) + b.metodoB(a,b)";

                SearchByRegexExpression search = new SearchByRegexExpression(input);
                search.ProcessingPattern();


                // teste automatizado.
                assercao.IsTrue(
                    search != null &&
                    search.parametros != null &&
                    search.subExpressoesSearch != null &&
                    search.subExpressoesSearch.Count == 2 &&
                    search.subExpressoesSearch[0].TipoExpressao == typeof(ExpressaoChamadaDeMetodo) &&
                    search.subExpressoesSearch[0].parametros != null &&
                    search.subExpressoesSearch[0].parametros.Count == 3);


            }


            public void Teste1PropriedadesAninhadas1ComChamadaDeMetodo(AssercaoSuiteClasse assercao)
            {
                string input = "obj.propriedadeA.metodoB(x,y)";

                SearchByRegexExpression search = new SearchByRegexExpression(input);
                search.ProcessingPattern();


                assercao.IsTrue(
                    search != null &&
                    search.subExpressoesSearch != null &&
                    search.subExpressoesSearch.Count == 1 &&
                    search.subExpressoesSearch[0].TipoExpressao == typeof(ExpressaoChamadaDeMetodo) &&
                    search.subExpressoesSearch[0].parametros != null &&
                    search.subExpressoesSearch[0].parametros.Count == 2 &&
                    search.subExpressoesSearch[0].parametros[0].input == "x");


            }






  
            public void TesteChamadeDeMetodoComParametros(AssercaoSuiteClasse assercao)
            {
                string input = "a.metodoA(x,y)";





                SearchByRegexExpression search = new SearchByRegexExpression(input);
                search.ProcessingPattern();



                // teste automatizado.
                assercao.IsTrue(search != null &&
                    search.TipoExpressao != null &&
                    search.TipoExpressao == typeof(ExpressaoChamadaDeMetodo) &&
                    search.parametros != null &&
                    search.parametros.Count == 2);



            }


            public void TesteComposicaoDeChamadaDeMetodosComParametrosChamadaDeMetodos(AssercaoSuiteClasse assercao)
            {
                string input = "a.metodoA(b.metodoC(x,y,z)+c) + b.metodoB(n,m)";

                SearchByRegexExpression search = new SearchByRegexExpression(input);
                search.ProcessingPattern();


                // teste automatizado.
                assercao.IsTrue(
                    search != null &&
                    search.subExpressoesSearch != null &&
                    search.subExpressoesSearch[0].parametros != null &&
                    search.subExpressoesSearch[0].parametros.Count == 1 &&
                    search.subExpressoesSearch[1].parametros != null &&
                    search.subExpressoesSearch[1].parametros.Count == 2);
            }








            public void TesteExpressaoNumeros(AssercaoSuiteClasse assercao)
            {
               string input = "1+ a";

                Expressao exprssMain = new Expressao();
                exprssMain.tokens = new List<string>();

                SearchByRegexExpression search = new SearchByRegexExpression(input);
                search.ProcessingPattern();

                // teste automatizado.
                assercao.IsTrue(
                    search != null &&
                    search.exprss != null &&
                    search.exprss.Count == 2 &&
                    search.subExpressoesSearch != null &&
                    search.subExpressoesSearch.Count == 2 &&
                    search.subExpressoesSearch[0].TipoExpressao == typeof(ExpressaoNumero));

            }

            public void TesteExpressaoOperador(AssercaoSuiteClasse assercao)
            {
               
                // codigo de uma expressao "ExpressaoOperador".
                string input = "a+b";

                // instancia uma expressao sem tokens, para expressao principal.
                Expressao exprssMain = new Expressao();
                if (exprssMain.tokens == null)
                    exprssMain.tokens = new List<string>();



                // faz uma busca atraves de expressoes regulares, regex.
                SearchByRegexExpression search = new SearchByRegexExpression(input);
                search.ProcessingPattern();

                Type tiposExpressoes = search.TipoExpressao;

                // teste não automatizado.
                assercao.IsTrue(
                    search.todasOcorrencias != null &&
                    search.todasOcorrencias.Count == 3 &&
                    search.operators != null &&
                    search.operators.Count == 1 &&
                    search.operators[0].text == "+");

            }











        }

    }



}



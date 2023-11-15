using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace parser.textoFormatado
{
    public class TextExpression
    {
     
        public static string ID;
        public static string EXPRESS;
        public static string number;
        public static string literal;
     
        private string espacoEmBranco0_ou_mais;

        private string quaisquerCaracteres;


        private string parentesesAbre;
        private string parentesesFecha;





        private static List<string> nomesOperadores = new List<string>();



        private char aspas = '\u0022';

        public TextExpression()
        {

            // lista de todos operadores currentes no codigo.
            // (operadores das classes base, e operadores do codigo, se começou o processamento do codigo).
            List<string> nomesOperadores = ObtemNomesOperadores();
            string operadores = "";
            if ((nomesOperadores != null) && (nomesOperadores.Count > 0))
            {
                for (int x = 0; x < nomesOperadores.Count; x++)
                {
                    operadores += @"\" + nomesOperadores[x];
                }

            }

          

            ID = @"(?<id>\w+)";
            EXPRESS = @"(?<exprss>.+)";
            number = @"(?<number>[0-9]+)";
            literal = @"(?<literal>" + aspas.ToString() + "+\\w+" + aspas.ToString() + ")";

            
            parentesesAbre = @"[(]";
            parentesesFecha = @"[)]";
         
            espacoEmBranco0_ou_mais = @"\s*";

            quaisquerCaracteres = ".*";
        }

        /// <summary>
        /// encontra no [textoParaMatch], o padrão [textoResumido], com texto opcional, que pode aparecer ou não.
        /// </summary>
        /// <param name="patternResumed">texto formatado, como: "id id ;", "id id ( )".</param>
        /// <param name="input">texto a reconher o match.</param>
        /// <param name="opcional">texto com tokens de uma opção, que pode aparecer ou não.</param>
        /// <returns></returns>
        public string Match(string patternResumed, string input, string opcional)
        {
            List<string> tokensDoTextoResumido = patternResumed.Split(new string[] { " "}, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
            string textoRegex = FormaExpressaoRegularGenerica(patternResumed, opcional);


            // faz o processamento do regex, procurando validar o padrão de texto, para o texto a reconhecer.
            Regex expressaoRegex = new Regex(textoRegex);


            if (expressaoRegex.Match(input).Success)
                return expressaoRegex.Match(input).Value;
            else
                return "";

        }

        public string FormaPatternResumed(string input1)
        {
            /*
             * 
             * 	---->objetivo:  codificar um metodo que calcula o "pattern resumido", a partir de um texto "input", não só nos casos com operadores.
							-----> o algoritmo:
										1- carregar todos tokens que possam ser delimitadores: "(" , ")" , "." , "[" , "]".
										2- dividir o "input" com esses tokens delimitadores.
										3- calcular os ids, e seus indices de aparecimento no "input".
										4- calcular os tokens delimitadores presentes, e seus indices de aparecimento no "input".
										5- juntar as listas de tokens, e as listas de indices.
										6- fazer o processamento, inserindo "ids", ou "exprss", ou "number", nos tokens ids.
										7- completar o processamento, inserindo os tokens delimitadores presentes.
             * 
             */



            int offset = 0;
            List<string> todosTermosChaveDaLinguagem = GetTodosTermosChavesIniciais();

            List<string> tokens = new Tokens(input1).GetTokens();
            List<string> ids = new List<string>();
            List<string> operadores = ObtemNomesOperadores();
            List<int> indicesOperadoresPresentes = new List<int>();

            List<string> operadoresPresentes = new List<string>();
            for (int x = 0; x < tokens.Count; x++)
            {
                if (operadores.IndexOf(tokens[x]) != -1)
                {
                    int index = tokens.IndexOf(tokens[x]);
                    indicesOperadoresPresentes.Add(index);


                    operadoresPresentes.Add(tokens[x]);
                }
            }

            List<int> indicesIds = new List<int>();

            offset = 0;
            for (int x = 0; x < tokens.Count; x++)
            {
                if (IsID(tokens[x]))
                {
                    ids.Add(tokens[x]);
                    int indice = tokens.IndexOf(tokens[x], offset);
                    indicesIds.Add(indice);

                    offset = indice + 1;
                }

            }


            // processamento de tokens delimitadores, que não são ids, numbers, ou exprss.
            string delimitadores = "( ) [ ] , ; . {  }  : ";
            List<string> tokensDelimitadores = delimitadores.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();


            List<string> tokensDelimitadoresPresentes = new List<string>();
            List<int> indicesDelimitadoresPresentes = new List<int>();


            offset = 0;
            for (int x = 0; x < tokens.Count; x++)
                if (tokensDelimitadores.IndexOf(tokens[x]) != -1)
                {
                    tokensDelimitadoresPresentes.Add(tokens[x].Trim(' '));

                    int indice = tokens.IndexOf(tokens[x].Trim(' '), offset);
                    indicesDelimitadoresPresentes.Add(indice);

                    offset = indice + 1;

                }


            offset = 0;
            int offset_id = 0;
            // obtem a lista de indices dos tokens delimitadores.
            for (int x = 0; x < tokens.Count; x++)
            {

                if ((operadores.IndexOf(tokens[x]) != -1) && (tokens[x] != "="))
                {
                    if ((x - 1 >= 0) && (IsID(tokens[x - 1])))
                    {
                        int indexTokenAnterior = tokens.IndexOf(tokens[x - 1], offset);
                        int indexId = ids.IndexOf(tokens[x - 1], offset_id);



                        if (indexTokenAnterior != -1)
                            ids[indexId] = "exprss";

                        offset = indexTokenAnterior + 1;
                        offset_id = indexId + 1;
                    }


                    if (((x + 1) < tokens.Count) && (IsID(tokens[x + 1])))
                    {
                        int indexTokenPosterior = tokens.IndexOf(tokens[x + 1], offset);
                        int indexId = ids.IndexOf(tokens[x + 1], offset_id);


                        if (indexTokenPosterior != -1)
                            ids[indexId] = "exprss";

                        offset = indexTokenPosterior + 1;
                        offset_id = indexId + 1;
                    }


                }

            }

            // lista de todos tokens, e todos indices.
            List<string> todosTokens = new List<string>();
            List<int> todosIndices = new List<int>();

            todosTokens.AddRange(ids);
            todosTokens.AddRange(tokensDelimitadoresPresentes);
            todosTokens.AddRange(operadoresPresentes);

            todosIndices.AddRange(indicesIds);
            todosIndices.AddRange(indicesDelimitadoresPresentes);
            todosIndices.AddRange(indicesOperadoresPresentes);


            List<tokenResumed> todosTokensResumed = new List<tokenResumed>();
            for (int x = 0; x < todosTokens.Count; x++)
            {
                todosTokensResumed.Add(new tokenResumed(todosTokens[x], todosIndices[x]));
            }
                

            ComparerTokensResumed comparer = new ComparerTokensResumed();
            todosTokensResumed.Sort(comparer);





            string patternResumedRAW = "";


            offset = 0;
            // processamento das listas de todos tokens e indices.
            for (int x = 0; x < todosTokensResumed.Count; x++)
            {
                if (todosTokensResumed[x].token == "=")
                {
                    patternResumedRAW += "=" + " ";
                }
                else
                if (operadores.IndexOf(todosTokensResumed[x].token, offset) != -1) // o token é nome de um operador.
                {
                    patternResumedRAW += todosTokensResumed[x].token + " ";
                }
                else

                if (tokensDelimitadoresPresentes.IndexOf(todosTokensResumed[x].token, offset) != -1) // o token é um delimitador.
                {
                    patternResumedRAW += todosTokensResumed[x].token + " ";
                }
                else



                if (this.IsNumber(todosTokensResumed[x].token)) // o token é um numero
                {
                    patternResumedRAW += "number" + " ";
                }
                else

                if (todosTokensResumed[x].token == "exprss") // o token pode ser resumido como "exprss".
                {
                    patternResumedRAW += "exprss" + " ";
                }
                else
                if (this.IsLiteral(todosTokensResumed[x].token))
                {
                    patternResumedRAW += "literal" + " ";
                }
                else
                if (todosTermosChaveDaLinguagem.IndexOf(todosTokensResumed[x].token) != -1)
                {
                    patternResumedRAW += todosTokensResumed[x].token + " ";
                }
                else
                if (IsID(todosTokensResumed[x].token)) // o token pode ser resumido como "id"
                {
                    patternResumedRAW += "id" + " ";
                }
            }
            return patternResumedRAW.Trim(' ');

        }

        public static List<string> GetTodosTermosChavesIniciais()
        {
            List<string> todosTermosChaveDaLinguagem = LinguagemOrquidea.Instance().GetTodosTermosChave();
            todosTermosChaveDaLinguagem.Remove("(");
            todosTermosChaveDaLinguagem.Remove(")");
            todosTermosChaveDaLinguagem.Remove("=");
            todosTermosChaveDaLinguagem.Remove(",");
            todosTermosChaveDaLinguagem.Remove(":");
            todosTermosChaveDaLinguagem.Remove("{");
            todosTermosChaveDaLinguagem.Remove("}");
            todosTermosChaveDaLinguagem.Remove(".");
            todosTermosChaveDaLinguagem.Remove(":-");
            todosTermosChaveDaLinguagem.Remove("[");
            todosTermosChaveDaLinguagem.Remove("]");
            todosTermosChaveDaLinguagem.Remove(";");
          
            List<string> operadoresBasicos = LinguagemOrquidea.Instance().GetTodosOperadores();
            for (int x = 0; x < todosTermosChaveDaLinguagem.Count; x++)
            {
                if (operadoresBasicos.IndexOf(todosTermosChaveDaLinguagem[x]) != -1)
                {
                    todosTermosChaveDaLinguagem.RemoveAt(x);
                    x--;
                }
            }

            return todosTermosChaveDaLinguagem;
        }

        private static void RemoveEspacosVazios(List<string> tokensId)
        {
            for (int x = 0; x < tokensId.Count; x++)
            {
                if (tokensId[x] == " ")
                {
                    tokensId.RemoveAt(x);
                    x--;
                }
            }
        }

        public class tokenResumed
        {
            public string token
            {
                get;
                set;
            }
            public int index
            {
                get;
                set;
            }
            public tokenResumed(string token, int index)
            {
                this.token = token;
                this.index = index;
            }

            public override string ToString()
            {
                return token;
            }


        }

        private class ComparerTokensResumed : IComparer<tokenResumed>
        {
            public int Compare(tokenResumed x, tokenResumed y)
            {
                if (x.index < y.index)
                    return -1;
                else
                if (x.index > y.index)
                    return +1;
                else
                    return 0;
            }
        }

        /// <summary>
        /// forma um pattern resumed com todos operadores presentes.
        ///  "a.metodoA(x)+ b.metodoB(a,b)+ c.metodoC(1,5) ===> a.metodoA()+ b.metodoeB()+ c.metodoC() ==> exprss + exprss + exprss";
        /// </summary>
        /// <param name="input">o texto a ser parseado por expressoes regex.</param>
        public string FormaPatterResumedForOperators(string input)
        {
            // cria uma copia para modificações, sem passar para a string parametro.
            string input_out = (string)input.Clone();

            List<string> nomesOperadores = ObtemNomesOperadores();


          
            //  "a.metodoa()+ b.metodoeB()+ c.metodoC()";
            // divide a entrada atraves dos nomes de operadores, e caracter vazio.
            List<string> tokensNaoOperadores = input_out.Split(nomesOperadores.ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList<string>();
            List<int> indicesTokensNaoOperadores = new List<int>();



            int offsetIndices = 0;





            for (int x = 0; x < tokensNaoOperadores.Count; x++)
            {
                int indexTokenNaoOperador = input_out.IndexOf(tokensNaoOperadores[x], offsetIndices);
                indicesTokensNaoOperadores.Add(indexTokenNaoOperador);

                offsetIndices = indexTokenNaoOperador + 1;
            }





            // obtem os tokens de operadores.
            List<string> tokensOperadores = input_out.Split(tokensNaoOperadores.ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList<string>();
            List<int> indicesTokensOperadores = new List<int>();
            int offsetIndicesOperadores = 0;


            for (int x = 0; x < tokensOperadores.Count; x++)
            {

                int indexOperador = input_out.IndexOf(tokensOperadores[x], offsetIndicesOperadores);
                indicesTokensOperadores.Add(indexOperador);

                offsetIndicesOperadores = indexOperador + 1;
            }



            List<string> tokensOperadoresExcluidos = new List<string>();
            List<int> indicesTokensExcluidos = new List<int>();

           

            // RECALCULA AS LISTAS DE TOKENS NAO OPERADORES,  E LISTA DE INDICES DE TOKENS NAO OPERADORES.
            offsetIndices = 0;
            // recalcula a lista de tokens nao operadores.
            tokensNaoOperadores = input_out.Split(tokensOperadores.ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList<string>();



            indicesTokensNaoOperadores.Clear();
            // recalcula a lista de indices de tokens nao operadores.
            for (int x = 0; x < tokensNaoOperadores.Count; x++)
            {
                int indexTokenNaoOperador = input_out.IndexOf(tokensNaoOperadores[x], offsetIndices);
                indicesTokensNaoOperadores.Add(indexTokenNaoOperador);

                offsetIndices = indexTokenNaoOperador;
            }






            /*
             * algoritmo complexo, é necessário:
					1 - calcular os indices dos operadores presentes.
				    2-  eliminar os indices de operadore presentes fora do fluxo principal.
					3-  calcular os tokens nao operadores, transforma-los em "id","express","number".
					4-  calcular os indices de tokens nao operadores.
                        4.1 - recalcula a lista de tokens nao operadores, transformando operadores excluidos em outro caracter de marcacao.
				    5-  juntar a lista de tokens operadores, e nao operadores.
					6-  juntar a lista de indices de tokens operadores, e nao operadores.
					7-  montar o pattern, inserindo tokens com seus indices.

             */





            List<string> todosTokens = new List<string>();
            List<int> todosIndices = new List<int>();

            todosTokens.AddRange(tokensNaoOperadores);
            todosTokens.AddRange(tokensOperadores);

            todosIndices.AddRange(indicesTokensNaoOperadores);
            todosIndices.AddRange(indicesTokensOperadores);




            // cria uma lista de todas ocorrencias [token, index], dos tokens.
            List<Ocorrencia> todasOcorrencias = new List<Ocorrencia>();
            for (int x = 0; x < todosTokens.Count; x++)
            {
                todasOcorrencias.Add(new Ocorrencia(todosTokens[x], todosIndices[x]));
            }

            // ordena a lista de ocorrencias segundo o index da ocorrenncias.
            ComparerOcorrencias comparer = new ComparerOcorrencias();
            todasOcorrencias.Sort(comparer);




            //  "a.metodoa()+ b.metodoeB()+ c.metodoC()";

            // forma um pattern modificado, de comprimento dos tokens nao operadores + comprimento dos tokens operadores.
            string patterModificadoRaw = "";



            for (int x = 0; x < todasOcorrencias.Count; x++)
            {
                // se o token currente for nome de operador, adiciona como o nome do operador.
                if (nomesOperadores.IndexOf(todasOcorrencias[x].text.Trim(' ')) != -1)
                    patterModificadoRaw += todasOcorrencias[x].text + " ";
                else
                {
                    //classifica ids, numbers ou express como "exprss". A função ID é chamada explicitamente para é ou não id, para fins de legibilidade do codigo.
                    if ((!IsID(todasOcorrencias[x].text.Trim(' '))) ||
                        (IsNumber(todasOcorrencias[x].text.Trim(' '))) ||
                        (IsID(todasOcorrencias[x].text.Trim(' '))))
                        patterModificadoRaw += "exprss" + " ";

                }
            }



            // forma o pattern final, a partir dos tokens do pattern raw, evitando espacamentos adicionais.
            List<string> tokensPatternRaw = patterModificadoRaw.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();



            string patterModificadoProcessado = "";
            for (int x = 0; x < tokensPatternRaw.Count; x++)
                patterModificadoProcessado += tokensPatternRaw[x] + " ";




            patterModificadoProcessado = patterModificadoProcessado.Trim();
            return patterModificadoProcessado;
        }


 


        private bool IsLiteral(string exprssID)
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

            if (nomesOperadores.Count == 0)
                nomesOperadores = ObtemNomesOperadores();

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

            if (exprssID.Contains("\""))
            {
                return true;
            }

            for (int x = 0; x < tokensExpressao.Count; x++)
                if (exprssID.Contains(tokensExpressao[x]))
                    return false;

            if (nomesOperadores.Find(k => k.Equals(exprssID)) != null)
                return false;

            return true;

        }

        private bool IsNumber(string token)
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






        private static List<string> ObtemNomesOperadores()
        {
            if ((nomesOperadores != null) && (nomesOperadores.Count > 0))
            {
                return nomesOperadores;
            }
            else
            {
                nomesOperadores = new List<string>() { "+", "-", "*", "/", "!", "<", "<=", ">", ">=", "++", "--", "!=", "=" };
                return nomesOperadores;
            }
            
        }

        public static List<Ocorrencia> GetOcurrencesOperators(string pathResume, string input)
        {
            if (nomesOperadores == null)
                nomesOperadores = ObtemNomesOperadores();   
            
            
            string[] tudoMenooperadores = pathResume.Split(nomesOperadores.ToArray(), StringSplitOptions.RemoveEmptyEntries);
            string[] operadores = pathResume.Split(tudoMenooperadores, StringSplitOptions.RemoveEmptyEntries);





            if ((operadores == null) || (operadores.Length == 0))
                return new List<Ocorrencia>();
            else
            {
                List<Ocorrencia> ocorrencias = new List<Ocorrencia>();
                int offset = 0;
                for (int x = 0; x < operadores.Length; x++)
                {
                    int index = input.IndexOf(operadores[x], offset);

                    Ocorrencia ocorrenciaOperador = new Ocorrencia(operadores[x].Trim(' '), index);
                    ocorrencias.Add(ocorrenciaOperador);


                    offset = index + 1;
                }
                return ocorrencias;
            }

        }
        /// <summary>
        /// obtem todas ocorrencias de um grupo presente num texto pesquisado por uma regex.
        /// </summary>
        /// <param name="pathResume">codigo resumido, como: "id = id + id".</param>
        /// <param name="input">texto a ser compilado.</param>
        /// <param name="nameGroup">nome do grupo.</param>
        /// <returns></returns>
        public static  List<Ocorrencia> GetOcurrencesGroup(string pathResume, string input, params string[] nameGroup)
        {


            string pattern = new TextExpression().FormaExpressaoRegularGenerica(pathResume);
            Regex regex = new Regex(pattern);





            GroupCollection groups = regex.Match(input).Groups;

            List<Ocorrencia> ocorrencias = new List<Ocorrencia>();
            string[] nomesGrupo = regex.GetGroupNames();

            for (int x = 0; x < nameGroup.Length; x++)
            {
                
                int indexGroup = nomesGrupo.ToList<string>().FindIndex(k => k.Equals(nameGroup[x]));

                foreach (Capture umaOcorrencia in groups[indexGroup].Captures)
                {
                    // guarda o texto encontrado no match, e o indice dentro do texto input.
                    ocorrencias.Add(new Ocorrencia(umaOcorrencia.Value, umaOcorrencia.Index));
                }


            }
            return ocorrencias;
        }

        /// <summary>
        /// tenta encontrar textos que match com o texto padrão.
        /// </summary>
        /// <param name="textPattern">pattern text.</param>
        /// <param name="input">text to search.</param>
        public static void GetGroupsSearch(string patternResume, string input)
        {

            string textPattern = new TextExpression().FormaExpressaoRegularGenerica(patternResume);

           
            Regex regex = new Regex(textPattern);
            CaptureCollection umaCaptura = Regex.Match(input, textPattern).Groups[0].Captures;
         



            if (umaCaptura.Count>0)
            {
                GroupCollection groups = Regex.Match(input, textPattern).Groups;
                for (int x=0; x< groups.Count; x++)
                {
                    System.Console.WriteLine("group: {0}: ", regex.GetGroupNames()[x]);

                    string[] nomesGrupo = regex.GetGroupNames();
                    foreach (Capture cap in groups[x].Captures)
                    {

                        System.Console.WriteLine("value: {0}", cap);
                    }

                    System.Console.WriteLine();
                    System.Console.WriteLine();
                }
                

               
            }

       

        }

        /// <summary>
        /// encontra indices do tokens formatado, que corresponda aos tokens do opcional.
        /// </summary>
        /// <param name="tokensDoOpcional">texto formatado do opcional. ex: ", id id".</param>
        /// <param name="tokensFormatado">tokens já formatado.</param>
        private List<int> GetIndexTokensOption(List<string> tokensDoOpcional, List<string> tokensFormatado)
        {
            List<int> indicesTokensOpcional = new List<int>();

            for (int x = 0; x < tokensFormatado.Count; x++)
            {
                bool isFound = true;
                for (int i = 0; i < tokensDoOpcional.Count; i++) 
                    if (tokensFormatado[x+i]!=tokensDoOpcional[i])
                    {
                        isFound = false;
                        break;
                    }
                if (isFound)
                    indicesTokensOpcional.Add(x);

            }

            return indicesTokensOpcional;
        }

        /// <summary>
        /// forma textos de procura por expressao regex, a partir de um parttern resumido, contendo tokens como "id","exprss","number", parenteses...
        /// </summary>
        /// <param name="patternResume">texto contendo nomes como id, expss, (, ), etc...eex: "id.id()"</param>
        /// <param name="opcional">opcional do texto formatado, podendo aparecer ou não.</param>
        /// <returns>retorna um texto de procura por expressoes regex.</returns>
        public string FormaExpressaoRegularGenerica(string patternResume, params string[] opcional)
        {


            List<string> tokensPatternResumed = new Tokens(patternResume).GetTokens();


            // obtem indices de aparecimento de tokens do opcional.
            List<List<int>> indicesOpcionais = new List<List<int>>();
            List<List<string>> tokensDosOpcionais = new List<List<string>>();


            if ((opcional != null) && (opcional.Length > 0) && (opcional[0] != ""))
                for (int x = 0; x < opcional.Length; x++)
                {
                    tokensDosOpcionais.Add(opcional[x].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList<string>());
                    indicesOpcionais.Add(GetIndexTokensOption(tokensDosOpcionais[x], tokensPatternResumed));
                }



            //  caracteres da sintaxe regex:  \ , ^ , $ , . , | , ? , * , + , { , }

            List<string> nomesOperadoresPresentesEmRegexExpression = new List<string>();
            nomesOperadoresPresentesEmRegexExpression.Add("+");
            nomesOperadoresPresentesEmRegexExpression.Add("*");
            nomesOperadoresPresentesEmRegexExpression.Add(@"\");
            nomesOperadoresPresentesEmRegexExpression.Add("$");
            nomesOperadoresPresentesEmRegexExpression.Add("{");
            nomesOperadoresPresentesEmRegexExpression.Add("}");
            nomesOperadoresPresentesEmRegexExpression.Add(".");
            nomesOperadoresPresentesEmRegexExpression.Add("^");
            nomesOperadoresPresentesEmRegexExpression.Add("?");
            nomesOperadoresPresentesEmRegexExpression.Add("|");
            nomesOperadoresPresentesEmRegexExpression.Add(",");
            nomesOperadoresPresentesEmRegexExpression.Add("[");
            nomesOperadoresPresentesEmRegexExpression.Add("]");

        
            string expressaoRegex = "";
            for (int x = 0; x < tokensPatternResumed.Count; x++)
            {
              

                if (tokensPatternResumed[x] == "++")
                {
                    expressaoRegex += "\\+\\+" + espacoEmBranco0_ou_mais;
                }
                else
                if (tokensPatternResumed[x] == "--")
                {
                    expressaoRegex += "\\-\\-" + espacoEmBranco0_ou_mais;
                }
                else
                if (nomesOperadoresPresentesEmRegexExpression.IndexOf(tokensPatternResumed[x]) != -1)
                {
                    expressaoRegex += "\\" + tokensPatternResumed[x] + espacoEmBranco0_ou_mais;
                }
                else
                if ((nomesOperadores.IndexOf(tokensPatternResumed[x]) != -1) && (nomesOperadoresPresentesEmRegexExpression.IndexOf(tokensPatternResumed[x]) == -1))
                {
                    expressaoRegex += tokensPatternResumed[x] + espacoEmBranco0_ou_mais;
                }
                else
               
                {
                    switch (tokensPatternResumed[x])
                    {
                        case "id":
                            expressaoRegex += ID + espacoEmBranco0_ou_mais;
                            break;

                        case "number":
                            expressaoRegex += number + espacoEmBranco0_ou_mais;
                            break;

                        case "literal":
                            expressaoRegex += literal + espacoEmBranco0_ou_mais;
                            break;

                        case "exprss":
                            expressaoRegex += EXPRESS + espacoEmBranco0_ou_mais;
                            break;

                        case "bloco":
                            expressaoRegex += quaisquerCaracteres;
                            break;

                        case "(":
                            // caso exceção porque o token é um operador de formatacao dentro de expressoes regex
                            expressaoRegex += parentesesAbre + espacoEmBranco0_ou_mais;
                            break;

                        case ")":
                            // caso exceção porque o token é um operador de formatacao dentro de expressoes regex
                            expressaoRegex += parentesesFecha + espacoEmBranco0_ou_mais;
                            break;

                 

                        default:
                            expressaoRegex += tokensPatternResumed[x] + espacoEmBranco0_ou_mais;
                            break;




                    }




                    if ((indicesOpcionais != null) && (indicesOpcionais.Count > 0))
                    {
                        for (int indexOpcional = 0; indexOpcional < indicesOpcionais.Count; indexOpcional++)
                            for (int indexOption = 0; indexOption < indicesOpcionais[indexOpcional].Count; indexOption++)
                                if (x == indicesOpcionais[indexOpcional][indexOption])
                                    expressaoRegex += FormaExpressaoRegularGenerica(tokensPatternResumed[indexOpcional], "");

                    }

                }
            } // for x


            return expressaoRegex;
        }


        public class Ocorrencia
        {
            /// <summary>
            /// texto encontrado no match com expressoes regex.
            /// </summary>
            public string text
            {
                get;
                set;
            }

            /// <summary>
            /// indice do texto encontra, perante o texto input para expressoes regex.
            /// </summary>
            public int index
            {
                get;
                set;
            }

            public Ocorrencia(string textOcurrence, int index)
            {
                this.text = textOcurrence;
                this.index = index;
            }



            
            public override string ToString()
            {
                if (text != null)
                    return text;
                else
                    return "";
            }
        }

        public class ComparerOcorrencias : IComparer<Ocorrencia>
        {
            public int Compare(Ocorrencia x, Ocorrencia y)
            {
                if (x.index < y.index)
                    return -1;
                else
                if (x.index > y.index)
                    return +1;
                else
                    return 0;

            }
        }

        public class Testes : SuiteClasseTestes
        {
            public Testes() : base("testes para epressoes regulares utilizadas para match de casos de uso")
            {
            }
            // "int x= create();";

            public void TestePatternChamadaDeMetodoExtracaoDeIds(AssercaoSuiteClasse assercao)
            {

                string input = "a.metodoB()";

                TextExpression textExpression = new TextExpression();
                string patternResumed = textExpression.FormaPatternResumed(input);
                string patternRegex = textExpression.FormaExpressaoRegularGenerica(patternResumed);


                Regex regex = new Regex(patternRegex);
                if (regex.IsMatch(input))
                {
                    System.Console.WriteLine("texto encontrado!");
                }
                else
                {
                    System.Console.WriteLine("texto nao encontrado!");
                }
                System.Console.WriteLine("pattern resumido: {0}", patternResumed);


                List<Ocorrencia> ocorrenciasExprss = TextExpression.GetOcurrencesGroup(patternResumed, input, "id");
                if (ocorrenciasExprss != null)
                    for (int x = 0; x < ocorrenciasExprss.Count; x++)
                        System.Console.WriteLine(ocorrenciasExprss[x].text);


                System.Console.WriteLine(patternRegex);
                System.Console.ReadLine();
            }

            public void TesteComandoLinguagem(AssercaoSuiteClasse assercao)
            {
                string input= "int x= create();";
                TextExpression textExpression = new TextExpression();

                string patternResumed = textExpression.FormaPatternResumed(input);
                string patternRegex = textExpression.FormaExpressaoRegularGenerica(patternResumed);

                Regex regex = new Regex(patternRegex);
                System.Console.WriteLine("input: " + input + ", " + "   pattern resumed: " + patternResumed);
                if (regex.IsMatch(input))
                {
                    System.Console.WriteLine("texto reconhecido.");
                }
                else
                {
                    System.Console.WriteLine("texto nao reconhecido.");
                }



                List<Ocorrencia> ocorrenciasExprss = TextExpression.GetOcurrencesGroup(patternResumed, input, "id");
                if (ocorrenciasExprss != null)
                    for (int x = 0; x < ocorrenciasExprss.Count; x++)
                        System.Console.WriteLine(ocorrenciasExprss[x].text);



                ocorrenciasExprss = TextExpression.GetOcurrencesGroup(patternResumed, input, "exprss");
                if (ocorrenciasExprss != null)
                    for (int x = 0; x < ocorrenciasExprss.Count; x++)
                        System.Console.WriteLine(ocorrenciasExprss[x].text);



                System.Console.ReadLine();



            }
            public void TesteOperadoresPresentesEmInput(AssercaoSuiteClasse assercao)
            {

                TextExpression textExpression = new TextExpression();

                string input = "while (x<1)";
                string patternResumed = "while (exprss)";
                string patternRegex = textExpression.FormaExpressaoRegularGenerica(patternResumed);

                Regex regex2 = new Regex(patternRegex);
                System.Console.WriteLine("input: " + input + ", " + "   pattern resumed: " + patternResumed);
                if (regex2.IsMatch(input))
                {
                    System.Console.WriteLine("texto reconhecido.");
                }
                else
                {
                    System.Console.WriteLine("texto nao reconhecido.");
                }



                List<Ocorrencia> ocorrenciasExprss = TextExpression.GetOcurrencesGroup(patternResumed, input, "id");
                if (ocorrenciasExprss != null)
                    for (int x = 0; x < ocorrenciasExprss.Count; x++)
                        System.Console.WriteLine(ocorrenciasExprss[x].text);



                ocorrenciasExprss = TextExpression.GetOcurrencesGroup(patternResumed, input, "exprss");
                if (ocorrenciasExprss != null)
                    for (int x = 0; x < ocorrenciasExprss.Count; x++)
                        System.Console.WriteLine(ocorrenciasExprss[x].text);



                System.Console.ReadLine();

            }

            public void TesteOperadoresPresentesEmExpressaoRegexSintaxe(AssercaoSuiteClasse assercao)
            {

                TextExpression textExpression = new TextExpression();

                string input2 = "var b = { : }";

            
                string patternRegex2 = textExpression.FormaExpressaoRegularGenerica("var id = { : }");

                Regex regex2 = new Regex(patternRegex2);
                System.Console.WriteLine("input: " + input2 + ", " + "   pattern resumed: " + "var id = { : }");
                if (regex2.IsMatch(input2))
                {
                    System.Console.WriteLine("texto reconhecido.");
                }
                else
                {
                    System.Console.WriteLine("texto nao reconhecido.");
                }


                string input = "{ a }";


                string patternResumed = textExpression.FormaPatternResumed(input);
                string patternRegex = textExpression.FormaExpressaoRegularGenerica(patternResumed);

                Regex regex = new Regex(patternRegex);

                System.Console.WriteLine("input: " + input + ", " + "   pattern resumed: " + patternResumed);
                if (regex.IsMatch(input))
                {
                    System.Console.WriteLine("texto reconhecido.");
                }
                else
                {
                    System.Console.WriteLine("texto nao reconhecido.");
                }


                List<Ocorrencia> ocorrenciasExprss = TextExpression.GetOcurrencesGroup(patternResumed, input, "id");
                if (ocorrenciasExprss != null)
                    for (int x = 0; x < ocorrenciasExprss.Count; x++)
                        System.Console.WriteLine(ocorrenciasExprss[x].text);




                System.Console.ReadLine();
        
            }

            public void TesteTokensQueFazemParteDeRegexExpressions(AssercaoSuiteClasse assercao)
            {
                TextExpression textExpression = new TextExpression();

                string input5 = "a*b+c++";
                string patternResumed5 = textExpression.FormaPatternResumed(input5);
                string patternRegex5 = textExpression.FormaExpressaoRegularGenerica(patternResumed5);

                Regex regex = new Regex(patternRegex5);
                regex.Match(input5);

                // regex inicializado sem erros fatais.
                assercao.IsTrue(true);

                string input1 = "<x,y,z>";

                string patternResumed1 = textExpression.FormaPatternResumed(input1);
                string patternRegex1 = textExpression.FormaExpressaoRegularGenerica(patternResumed1);

                Regex regex5 = new Regex(patternRegex1);
                regex5.Match(input1);

                // inicializacao de regex feito sem erros fatais.
                assercao.IsTrue(true);

                string input2 = "{a,b}";
                string patternResumed2 = textExpression.FormaPatternResumed(input2);
                string patternRegex2 = textExpression.FormaExpressaoRegularGenerica(patternResumed2);


                Regex regex2 = new Regex(patternRegex2);
                regex2.Match(input2);   

                // inicializacao de regex feito sem erros fatais.
                assercao.IsTrue(true);

                string input3 = "a..b";
                string patternResumed3 = textExpression.FormaPatternResumed(input3);
                string patternRegex3 = textExpression.FormaExpressaoRegularGenerica(patternResumed3);

                Regex regex3 = new Regex(patternRegex3);
                regex3.Match(input3);

                // inicializacao de regex feito sem erros fatais.
                assercao.IsTrue(true);

            
            }


            public void TestePatternWrapperData(AssercaoSuiteClasse assercao)
            {
                string patternResumed = "var id = [ exprss ]";
                string input = "var M= [1]";

                TextExpression textExpression = new TextExpression();
              
                string patterRegex = textExpression.FormaExpressaoRegularGenerica(patternResumed);

                Regex regex = new Regex(patterRegex);
                if (regex.IsMatch(input))
                {
                    System.Console.WriteLine("texto encontrado!");
                }
                else
                {
                    System.Console.WriteLine("texto nao encontrado!");
                }
                
                List<Ocorrencia> ocorrenciasExprss = TextExpression.GetOcurrencesGroup(patternResumed, input, "id");
                if (ocorrenciasExprss != null)
                    for (int x = 0; x < ocorrenciasExprss.Count; x++)
                        System.Console.WriteLine(ocorrenciasExprss[x].text);

                List<Ocorrencia> ocorrenciasExprss2 = TextExpression.GetOcurrencesGroup(patternResumed, input, "exprss");
                if (ocorrenciasExprss2 != null)
                    for (int x = 0; x < ocorrenciasExprss2.Count; x++)
                        System.Console.WriteLine(ocorrenciasExprss2[x].text);


                System.Console.ReadLine();

            }

            public void TestePatternInstrucaoFor(AssercaoSuiteClasse assercao)
            {
                TextExpression textExpression = new TextExpression();
                string input = "for (int a = 1; a<20; a++)";


                string patternResumed = "for ( id id = exprss ; exprss ; exprss )";
                string patternRegex = textExpression.FormaExpressaoRegularGenerica(patternResumed);

                Regex regex = new Regex(patternRegex);
                if (regex.IsMatch(input))
                {
                    System.Console.WriteLine("texto encontrado!");
                }
                else
                {
                    System.Console.WriteLine("texto nao encontrado!");
                }


                List<Ocorrencia> ocorrenciasExprss = TextExpression.GetOcurrencesGroup(patternResumed, input, "id");
                if (ocorrenciasExprss != null)
                    for (int x = 0; x < ocorrenciasExprss.Count; x++)
                        System.Console.WriteLine(ocorrenciasExprss[x].text);

                List<Ocorrencia> ocorrenciasExprss2 = TextExpression.GetOcurrencesGroup(patternResumed, input, "exprss");
                if (ocorrenciasExprss2 != null)
                    for (int x = 0; x < ocorrenciasExprss2.Count; x++)
                        System.Console.WriteLine(ocorrenciasExprss2[x].text);


                System.Console.ReadLine();

            }

            public void TestePatternOperadorUnario(AssercaoSuiteClasse assercao  )
            {
                string input = "b--";
                TextExpression textExpression = new TextExpression();
                string patternResumed= textExpression.FormaPatternResumed(input);

                string patternRegex = textExpression.FormaExpressaoRegularGenerica(patternResumed);

                Regex regex = new Regex(patternRegex);

                if (regex.IsMatch(input))
                {
                    System.Console.WriteLine("texto encontrado!");
                }
                else
                {
                    System.Console.WriteLine("texto nao encontrado!");
                }
                System.Console.WriteLine("pattern resumido: {0}", patternResumed);


                List<Ocorrencia> ocorrenciasExprss = TextExpression.GetOcurrencesGroup(patternResumed, input, "id");
                if (ocorrenciasExprss != null)
                    for (int x = 0; x < ocorrenciasExprss.Count; x++)
                        System.Console.WriteLine(ocorrenciasExprss[x].text);

                List<Ocorrencia> ocorrenciasExprssNumeros = TextExpression.GetOcurrencesGroup(patternResumed, input, "number");
                if (ocorrenciasExprss != null)
                    for (int x = 0; x < ocorrenciasExprssNumeros.Count; x++)
                        System.Console.WriteLine(ocorrenciasExprssNumeros[x].text);

                System.Console.WriteLine(patternRegex);
                System.Console.ReadLine();

            }


            public void TestePatternInstanciacao(AssercaoSuiteClasse assercao)
            {
                string input = "int a=1;";

                TextExpression textExpression = new TextExpression();
                string patternResumed = textExpression.FormaPatternResumed(input);
                string patternRegex = textExpression.FormaExpressaoRegularGenerica(patternResumed);


                Regex regex = new Regex(patternRegex);
                if (regex.IsMatch(input))
                {
                    System.Console.WriteLine("texto encontrado!");
                }
                else
                {
                    System.Console.WriteLine("texto nao encontrado!");
                }
                System.Console.WriteLine("pattern resumido: {0}", patternResumed);


                List<Ocorrencia> ocorrenciasExprss = TextExpression.GetOcurrencesGroup(patternResumed, input, "id");
                if (ocorrenciasExprss != null)
                    for (int x = 0; x < ocorrenciasExprss.Count; x++)
                        System.Console.WriteLine(ocorrenciasExprss[x].text);

                List<Ocorrencia> ocorrenciasExprssNumeros = TextExpression.GetOcurrencesGroup(patternResumed, input, "number");
                if (ocorrenciasExprss != null)
                    for (int x = 0; x < ocorrenciasExprssNumeros.Count; x++)
                        System.Console.WriteLine(ocorrenciasExprssNumeros[x].text);

                System.Console.WriteLine(patternRegex);
                System.Console.ReadLine();
            }

 

            public void TestePatternProperties(AssercaoSuiteClasse assercao)
            {
                string input = "a.b.c";
                TextExpression textExpression = new TextExpression();
                string patternResumed = textExpression.FormaPatternResumed(input);


                string patternRegex = textExpression.FormaExpressaoRegularGenerica(patternResumed);
                Regex regex = new Regex(patternRegex);

                if (regex.IsMatch(input))
                {
                    MatchCollection collection = regex.Matches(input);
                    if ((collection != null) && (collection.Count > 0))
                    {
                        assercao.IsTrue(true);

                        for (int x = 0; x < collection.Count; x++)
                        {
                            System.Console.WriteLine(collection[x].Value);
                        }
                    }
                }

                System.Console.ReadLine();


            }

            public void TesteOutraDefinicaoDeExprss(AssercaoSuiteClasse assercao)
            {
                //  @"(?<exprss>.+)";
                string input = "(text1+text2)";
                string pattern = "(?<exprss>[\\w\\[\\]])+";

                Regex regex = new Regex(pattern);

                if (regex.IsMatch(input))
                {
                    MatchCollection collection = regex.Matches(input);
                    if ((collection != null) && (collection.Count > 0))
                    {
                        assercao.IsTrue(true);

                        for (int x = 0; x < collection.Count; x++)
                        {
                            System.Console.WriteLine(collection[x].Value);
                        }
                    }
                }

                System.Console.ReadLine();
            }
     
            public void TesteExprssModificada(AssercaoSuiteClasse assercao)
            {
                TextExpression textExpression = new TextExpression();


                string input = "a.metodo(c + d)";

                string pattenResumed = textExpression.FormaPatternResumed(input);
                System.Console.WriteLine("pattern resumido: {0}: " + pattenResumed);

                string patternRegex = textExpression.FormaExpressaoRegularGenerica(pattenResumed);

                Regex regexExprss = new Regex(patternRegex);
                if (regexExprss.IsMatch(input))
                {
                    System.Console.WriteLine("input reconhecido!");
                
                    List<Ocorrencia> exprssFound = TextExpression.GetOcurrencesGroup(pattenResumed, input, "exprss");
                    List<Ocorrencia> idsFound = TextExpression.GetOcurrencesGroup(pattenResumed, input, "id");

                    if (exprssFound != null)
                    {
                        for (int x = 0; x < exprssFound.Count; x++)
                            System.Console.WriteLine("exprss: {0}", exprssFound[x].text);

                        System.Console.WriteLine();
                    }
                    if (idsFound != null)
                    {
                        for (int x = 0; x < idsFound.Count; x++)
                            System.Console.WriteLine("id: {0}", idsFound[x].text);
                    }

                }
                else
                {
                    System.Console.WriteLine("input nao reconhecido...");
                }

                System.Console.ReadLine();
            }

            public void TesteFormacaoExpressaoRegularDefinicaoDeMetodo(AssercaoSuiteClasse assercao)
            {
                string input = "int metodoB( )";
                string patternResumido = new TextExpression().FormaPatternResumed(input);

                TextExpression expressoesRegulares = new TextExpression();

                string textoReconhecido = expressoesRegulares.Match(patternResumido, input, "");

                if ((textoReconhecido == null) || (textoReconhecido == ""))
                    System.Console.WriteLine("texto não match!");
                else
                {
                    System.Console.WriteLine("texto match: " + textoReconhecido);
                    System.Console.WriteLine("pattern resumido: {0}", patternResumido);
                    TextExpression.GetGroupsSearch(patternResumido, input);
                }



                System.Console.ReadLine();


            }


            public void TestePatternDuaChamadasDeCodigo(AssercaoSuiteClasse assercao)
            {
                string input = "a.metodoB() + b.metodoA()";

                TextExpression textExpression = new TextExpression();
                string patternResumed = textExpression.FormaExpressaoRegularGenerica(textExpression.FormaPatterResumedForOperators(input));

                Regex regex = new Regex(patternResumed);
                if (regex.IsMatch(input))
                {
                    System.Console.WriteLine("texto encontrado!");
                }
                else
                {
                    System.Console.WriteLine("texto nao encontrado!");
                }


                System.Console.WriteLine("pattern resumido: {0}",patternResumed);
                List<Ocorrencia> ocorrenciasExprss = TextExpression.GetOcurrencesGroup(patternResumed, input, "exprss");
                if (ocorrenciasExprss != null)
                    for (int x = 0; x < ocorrenciasExprss.Count; x++)
                        System.Console.WriteLine(ocorrenciasExprss[x].text);


                System.Console.WriteLine(patternResumed);
                System.Console.ReadLine();
            }


          

            public void TesteCorrecaoPatternResumed(AssercaoSuiteClasse assercao)
            {
                string input = "a.metodoa()+ b.metodoeB()+ c.metodoC()";
           
                TextExpression textExpression = new TextExpression();
                string patternModificado = textExpression.FormaPatterResumedForOperators(input);


                System.Console.WriteLine("pattern resumido: {0}", patternModificado);
                List<Ocorrencia> ocorrenciasExprss = TextExpression.GetOcurrencesGroup(patternModificado, input, "exprss");
                if (ocorrenciasExprss != null)
                    for (int x = 0; x < ocorrenciasExprss.Count; x++)
                        System.Console.WriteLine(ocorrenciasExprss[x].text);


                System.Console.ReadLine();


            }



            public void TesteExpressao(AssercaoSuiteClasse assercao)
            {
                string input = "a + b";
                string patternResumed = "exprss + exprss";
                TextExpression textExpression = new TextExpression();

                List<TextExpression.Ocorrencia> exprssOcorrencias = TextExpression.GetOcurrencesGroup(patternResumed, input, "exprss");

                System.Console.WriteLine("input: {0}", input);
                if (exprssOcorrencias != null)
                    for (int x = 0; x < exprssOcorrencias.Count; x++)
                        System.Console.WriteLine("exprss: {0}", exprssOcorrencias[x].text);

                System.Console.ReadLine();


            }


            public void TesteOperacional(AssercaoSuiteClasse assercao)
            {
                string input = "metodoB(x, y)";
                string patternResumed = "id ( id , id )";

                TextExpression textExpression = new TextExpression();
                List<TextExpression.Ocorrencia> ids = TextExpression.GetOcurrencesGroup(patternResumed, input, "id");

                System.Console.WriteLine("input: {0}", input);
                if (ids != null)
                    for (int x = 0; x < ids.Count; x++)
                        System.Console.WriteLine("id: {0}", ids[x].text);

                System.Console.ReadLine();
            }



            public void TesteDeMesa(AssercaoSuiteClasse assercao)
            {
                string input = "metodoB(x, y)";
                string patternResumed = "id ( id , id )";




                TextExpression expressoesRegulares = new TextExpression();
                TextExpression.GetGroupsSearch(patternResumed, input);


                System.Console.ReadLine();


            }



     

            public void TesteFormacaoExpressaoRegularChamadaDeMetodo(AssercaoSuiteClasse assercao)
            {
                string caseOfUseTest = "metodoB ( x , y )";




                TextExpression expressoesRegulares = new TextExpression();
                string textMatch = expressoesRegulares.Match("id ( id )", caseOfUseTest, ", id");



                System.Console.WriteLine(caseOfUseTest);
                System.Console.WriteLine(textMatch);

                System.Console.ReadLine();


            }


      
        }
    }


}
    

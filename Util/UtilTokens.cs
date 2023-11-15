using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.CompilerServices;

using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Integration;
using Microsoft.SqlServer.Server;
using parser.ProgramacaoOrentadaAObjetos;
namespace parser
{
    public class UtilTokens
    {
        private static LinguagemOrquidea linguagem = LinguagemOrquidea.Instance();

        /// <summary>
        /// escreve uma mensagem de erro na lista de mensagens no objeto escopo, com localização do código.
        /// </summary>
        public static void WriteAErrorMensage(string mensagemDeErro, List<string> tokensDOProcessamento, Escopo escopo)
        {
            if ((tokensDOProcessamento != null) && (tokensDOProcessamento.Count > 0))
            {
                PosicaoECodigo posicao = new PosicaoECodigo(tokensDOProcessamento);
                escopo.GetMsgErros().Add(mensagemDeErro + " , linha: " + posicao.linha + " , coluna: " + posicao.coluna + ".");

            }
            else
                escopo.GetMsgErros().Add(mensagemDeErro);

        }

        /// <summary>
        /// obtem uma lista de tokens a partir de uma lista de tokens inicial, começando a extração pelo indice [indiceInicio].
        /// o procesamento prossegue até a pilha dos operdores abre/fecha for = 0.
        /// </summary>
        /// <param name="indiceInicio">indice inicial da procura, deve ter como token neste indice, o operador abre.</param>
        /// <param name="operadorAbre">operador de inicio, da extração.</param>
        /// <param name="operadorFecha">operador de término da extração.</param>
        /// <param name="tokensEntreOperadores">lista de tokens inicial, contendo todos tokens, inclusive a lista de tokens resultante entre os operadores</param>
        /// <returns>retorna uma lista de tokens entre os operadores abre e fecha, onde um criterio é que a soma entre operadores abre e fecha seja = 0,
        /// ou null se não resultar em tokens, ou se o numero de tokens < 2.</returns>
        public static List<string> GetCodigoEntreOperadoresComRetiradaDeTokensiniFini(int indiceInicio, string operadorAbre, string operadorFecha, List<string> tokensEntreOperadores)
        {
            if (indiceInicio == -1)
                return null;


            List<string> tokens = new List<string>();
            int pilhaInteiros = 0;


            int indexToken = indiceInicio;

            while (indexToken < tokensEntreOperadores.Count)
            {
                if (tokensEntreOperadores[indexToken] == operadorAbre)
                {
                    tokens.Add(operadorAbre);
                    pilhaInteiros++;
                }
                else
                if (tokensEntreOperadores[indexToken] == operadorFecha)
                {
                    tokens.Add(operadorFecha);
                    pilhaInteiros--;
                    if (pilhaInteiros == 0)
                    {
                        tokens.RemoveAt(0);
                        tokens.RemoveAt(tokens.Count - 1);
                        return tokens;
                    }
                      

                } // if
                else
                    tokens.Add(tokensEntreOperadores[indexToken]);
                indexToken++;
            } // While

            if ((tokens == null) || (tokens.Count == 0) || (tokens.Count < 2))
            {
                return new List<string>();
             
            }
            else
            {
                tokens.RemoveAt(0);
                tokens.RemoveAt(tokens.Count - 1);
                return tokens;
            }
            
        } 


        /// <summary>
        ///  obtem uma lista de tokens, entre operador abre e operador fecha, com criterio que a quantidade da diferenca dos operadores abre/fecha seja = 0.
        /// </summary>
        /// <param name="indiceInicio">indice inicial, cujo token deste indice é um operador abre.</param>
        /// <param name="operadorAbre">operador inicial.</param>
        /// <param name="operadorFecha">operador final.</param>
        /// <param name="tokensEntreOperadores">tokens contendo inclusive a lista de tokens resultante.</param>
        /// <returns>retorna uma lista de tokens entre os parenteses, ou null se não resultar em tokens, ou se o numero de tokens < 2</returns>
        public static List<string> GetCodigoEntreOperadores(int indiceInicio, string operadorAbre, string operadorFecha, List<string> tokensEntreOperadores)
        {
            if (indiceInicio == -1)
                return null;


            List<string> tokens = new List<string>();
            int pilhaInteiros = 0;
            
            
            int indexToken = indiceInicio;
            
            while (indexToken < tokensEntreOperadores.Count)
            {
                if (tokensEntreOperadores[indexToken] == operadorAbre)
                {
                    tokens.Add(operadorAbre);
                    pilhaInteiros++;
                }
                else
                if (tokensEntreOperadores[indexToken] == operadorFecha)
                {
                    tokens.Add(operadorFecha);
                    pilhaInteiros--;
                    if (pilhaInteiros == 0)
                        return tokens;

                } // if
                else
                    tokens.Add(tokensEntreOperadores[indexToken]);
                indexToken++;
            } // While

            return tokens;
        } // GetCodigoEntreOperadores()


        public static void PrintTokens(List<string> tokens, string caption)
        {
            if ((tokens == null) || (tokens.Count == 0))
            {
                System.Console.WriteLine("without tokens to print!");
                return;
            }
            else
            {
                System.Console.Write(caption + "     ");
                for (int x = 0; x < tokens.Count; x++)
                {
                    System.Console.Write(tokens[x] + " ");
                }
                System.Console.WriteLine();
            }

        }
        public static void PrintTokensWithoutSpaces(List<string> tokens, string caption)
        {
            if ((tokens == null) || (tokens.Count == 0))
            {
                System.Console.WriteLine("without tokens to print!");
                return;
            }
            else
            {
                System.Console.Write(caption + "     ");
                for (int x = 0; x < tokens.Count; x++)
                {
                    System.Console.Write(tokens[x]);
                }
                System.Console.WriteLine();
            }
        }


        /// <summary>
        /// método especializado para retirar listas de tokens, como na instrução "casesOfUse".
        /// 
        /// se houver [ini] e [fini1], retira a lista de tokens entre [ini] e [fini1].
        /// se não houver mais [ini1], retorna as listas de tokens.
        /// se não houver mais [fini1], e houver [ini], retira a lista de tokens entre [ini] até o final da lista de tokens.
        /// </summary>
        public static List<List<List<string>>> GetCodigoEntreOperadoresCases(List<string> tokens)
        {
            // sintaxe: "casesOfUse y:  (case < x): { y = 2; }; ";

            List<List<List<string>>> tokensRetorno = new List<List<List<string>>>();
            int x = tokens.IndexOf("(");

            int firstOperadorAbre = tokens.IndexOf("{");
            int lastOperadorFecha = tokens.LastIndexOf("}");

            if ((firstOperadorAbre == -1) && (lastOperadorFecha == -1))
            {
                return null;
            }
            tokens.RemoveAt(firstOperadorAbre);
            lastOperadorFecha = tokens.LastIndexOf("}");
            tokens.RemoveAt(lastOperadorFecha);


            int offsetMarcadores = 0;
            int offsetProcessamento = tokens.IndexOf("{");
            while ((x >= 0) && (x < tokens.Count))
            {
                // faz o processamento do cabecalho do case.
                int indexCabecalhoCase = tokens.IndexOf("(", offsetMarcadores);
                if (indexCabecalhoCase == -1)
                {
                    return tokensRetorno;
                }
                List<string> cabecalhoUmCase = GetCodigoEntreOperadores(indexCabecalhoCase, "(", ")", tokens);

            
                if ((cabecalhoUmCase == null) || (cabecalhoUmCase.Count== 0))
                {
                    return tokensRetorno;
                }


                // faz o processamento do corpo do case.
                // obtem o primeiro token abre, apos o token abre de delimitação dos cases.
                int indexProcessamentoCase = tokens.IndexOf("{", offsetProcessamento);
                List<string> tokensCorpoDeUnCase = UtilTokens.GetCodigoEntreOperadores(indexProcessamentoCase, "{", "}", tokens);
                if ((tokensCorpoDeUnCase!=null) && (tokensCorpoDeUnCase.Count > 0))
                {
                    // se encontrou tokens validos, adiciona o case completo para a lista de cases de retorno.    
                    List<List<string>> umCaseCompleto = new List<List<string>>() { cabecalhoUmCase, tokensCorpoDeUnCase };

                    tokensRetorno.Add(umCaseCompleto);
                        
                       
                }
                else
                {
                    return null;
                }

                // atualiza os offsets de procura de tokens.
                offsetMarcadores += cabecalhoUmCase.Count;
                offsetProcessamento += tokensCorpoDeUnCase.Count;
                x = offsetProcessamento + offsetMarcadores;
                    
            }

            return tokensRetorno;

        }  





        /// <summary>
        /// faz a conversao de tipos basicos de classes importado, para o sistema de tipos da linguagem orquidea.
        /// </summary>
        /// <param name="tipo">tipo importado.</param>
        /// <returns>retorna o tipo correpondente, ou o tipo parametro se nao houver correspondente.</returns>
        public static string Casting(string tipo)
        {
            if ((tipo.Contains("Single")) || (tipo.Contains("single")))
            {
                return "float";
            }
            else
            if ((tipo.Contains("Int32")) || (tipo.Contains("Int16")))
            {
                return "int";
            }
            else
            if ((tipo.Contains( "Float")) || (tipo.Contains("float")))
            {
                return "float";
            }
            else
            if ((tipo.Contains("Double")) || (tipo.Contains("double")))
            {
                return "double";
            }
            else
            if (tipo.Contains ("Boolean"))
            {
                return "bool";
            }
            else
            if ((tipo.Contains("string")) || (tipo.Contains("String")))
            {
                return "string";
            }
            else
            if ((tipo.Contains("Char")) || (tipo.Contains("char")))
            {
                return "char";
            }
            else
            if (tipo.Contains("Wrappers"))
            {
                tipo = tipo.Replace("Wrappers.DataStructures.", "");
            }
            return tipo;
        }

        /// <summary>
        /// encontra um operador que é binario e unario ao mesmo tempo, mas funcionando com binario.
        /// </summary>
        /// <param name="nameOperator">nome do operador</param>
        /// <param name="tipoOperando1">tipo do operando 1.</param>
        /// <param name="tipoOperando2">tipo do operando 2.</param>
        /// <returns>retorna a funcao que implementa o operador.</returns>
        public static Operador FindOperatorBinarioEUnarioMasComoBINARIO(string classOperator, string nameOperator, string tipoOperando1, string tipoOperando2)
        {
            bool isFoundOperator = false;

            Operador operadorComoBinario = null;
            List<HeaderClass> classesOperadores = Expressao.headers.cabecalhoDeClasses;
            HeaderClass headerClassOperator = Expressao.headers.cabecalhoDeClasses.Find(k => k.nomeClasse == classOperator);

            if (headerClassOperator != null)
            {
                string nomeClasse= headerClassOperator.nomeClasse;
                List<HeaderOperator> operators = headerClassOperator.operators;
                HeaderOperator headerOperador = operators.Find(k => k.name == nameOperator && k.operands.Count==2 && k.operands[0] == tipoOperando1 && k.operands[1] == tipoOperando2 && k.tipoDoOperador == HeaderOperator.typeOperator.binary);
                if (headerOperador != null)
                {
                    return RepositorioDeClassesOO.Instance().GetClasse(nomeClasse).GetOperadores().Find(k => k.nome == nameOperator && k.tipo == "BINARIO");
                }

            }
            if (classesOperadores != null)
            {
                for (int x = 0; x < classesOperadores.Count; x++)
                {
                    List<HeaderOperator> operators = classesOperadores[x].operators;
                    if ((operators != null) && (operators.Count > 0))
                    {
                        // verifica se o operador é binario, com operandos compativeis com os parametros de entrada.
                        for (int op = 0; op < operators.Count; op++)
                        {
                            if (operators[op].tipoDoOperador == HeaderOperator.typeOperator.binary)
                            {
                                if ((operators[op].name == nameOperator) && (operators[op].operands[0] == tipoOperando1) && (operators[op].operands[1] == tipoOperando2))
                                {
                                    return RepositorioDeClassesOO.Instance().GetClasse(classesOperadores[x].nomeClasse).GetOperadores().Find(k => k.nome == nameOperator && k.tipo.Contains("BINARIO"));
                                    
                                  
                                }
                            }
                        }


                    }

                }
            }
            // nao encontrou nenhum operador.
            if (!isFoundOperator)
            {
                return null;
            }
            else
            {
                return operadorComoBinario;
            }
           
        }



        /// <summary>
        /// encontra um operador unario e binario ao mesmo tempo, mas funcionando como unario.
        /// </summary>
        /// <param name="nameOperator">nome do operador.</param>
        /// <param name="tipoOperando1">tipo do operando 1.</param>
        /// <param name="tipoOperando2">tipo do operando 2,dentro da expressao (operador unario tem apenas um operando).</param>
        /// <returns>retorna a funcao que implementa o operador.</returns>
        public static Operador FindOperatorBinarioUnarioMasComoUNARIO(string nameOperator, string tipoOperando1, string tipoOperando2)
        {
            bool isFoundOperator = false;

            Operador operatorCompatible = null;

            List<HeaderClass> classesOperadores = Expressao.headers.cabecalhoDeClasses;
            int indexClass = -1;
            if (tipoOperando1 != null)
            {
                indexClass = Expressao.headers.cabecalhoDeClasses.FindIndex(k => k.nomeClasse == tipoOperando1);
            }
            else
            if (tipoOperando2 != null)
            {
                indexClass = Expressao.headers.cabecalhoDeClasses.FindIndex(k => k.nomeClasse == tipoOperando2);
            }

            if (indexClass == -1)
            {
                return null;
            }

            string nameClassOperator = classesOperadores[indexClass].nomeClasse;

            List<HeaderOperator> operators = classesOperadores[indexClass].operators;
            if ((operators != null) && (operators.Count > 0))
            {

                // verifica se o operador é unario.
                for (int op = 0; op < operators.Count; op++)
                {
                    if ((tipoOperando1 != null) && (operators[op].name == nameOperator) && (operators[op].tipoDoOperador == HeaderOperator.typeOperator.unary_pre) &&
                        (operators[op].operands[0] == tipoOperando1))
                    {
                        operatorCompatible = RepositorioDeClassesOO.Instance().GetClasse(nameClassOperator).GetOperadores().Find(k => k.nome == nameOperator && k.tipo.Contains("UNARIO"));
                        operatorCompatible.tipo = "UNARIO POS";

                        isFoundOperator = true;

                    }
                    else
                    if ((tipoOperando2 != null) && (operators[op].name == nameOperator) && (operators[op].tipoDoOperador == HeaderOperator.typeOperator.unary_pos) &&
                        (operators[op].operands[0] == tipoOperando2))
                    {
                        operatorCompatible = RepositorioDeClassesOO.Instance().GetClasse(nameClassOperator).GetOperadores().Find(k => k.nome == nameOperator && k.tipo.Contains("UNARIO"));
                        operatorCompatible.tipo = "UNARIO PRE";
                        isFoundOperator = true;

                    }

                }
            }
            // nao encontrou nenhum operador.
            if (!isFoundOperator)
            {
                return null;
            }
            return operatorCompatible;

        }
        /// <summary>
        /// verficia se um  nome é um token de operador unario.
        /// </summary>
        /// <param name="nameOperator">nome do tokens dito operador unario a investigar.</param>
        /// <param name="operand">expressao que contem o tipo de operando do operador unario.</param>
        /// <returns></returns>
        public static bool IsUnaryOperator(string nameOperator, Expressao operand)
        {
            string nameClassOperator = operand.tipoDaExpressao;
            List<HeaderOperator> operators = null;

            HeaderClass classHeader = Expressao.headers.cabecalhoDeClasses.Find(k => k.nomeClasse == operand.tipoDaExpressao);
            if (classHeader == null)
            {
                return false;
            }
            operators = classHeader.operators;

            if ((operators == null) || (operators.Count == 0))
            {
                return false;
            }

            int indexOP = operators.FindIndex(k => k.name == nameOperator);


            if (indexOP != -1)
            {
                if ((operators[indexOP].tipoDoOperador == HeaderOperator.typeOperator.unary_pos) &&
                    (operators[indexOP].operands.Count > 0) && (operators[indexOP].operands[0] == operand.tipoDaExpressao))
                {
                    return true;
                }

                if ((operators[indexOP].tipoDoOperador == HeaderOperator.typeOperator.unary_pos) &&
                    (operators[indexOP].operands.Count > 1) && (operators[indexOP].operands[1] == operand.tipoDaExpressao))
                {
                    return true;
                }



            }

            return false;
        }

        /// <summary>
        /// encontra um operador, binario, unario pos, ou unario pre, de nome de entrada, e com tipos de operandos de entrada.
        /// </summary>
        /// <param name="nameOperator">nome do operador.</param>
        /// <param name="tipoOperando1">tipo do operando 1.</param>
        /// <param name="tipoOperando2">tipo do operando 2.</param>
        /// <returns></returns>
        public static Operador FindOperatorCompatible(string nameOperator, string tipoOperando1, string tipoOperando2, ref bool isBinaryAndUnary)
        {
            
            isBinaryAndUnary = false;
            bool isBinary = false;
            bool isUnary = false;
            bool isFoundOperator = false;

            Operador operatorCompatible = null;

            List<HeaderClass> classesOperadores = Expressao.headers.cabecalhoDeClasses;
            if (classesOperadores == null)
            {
                return null;
            }
            classesOperadores = Expressao.headers.cabecalhoDeClasses;
            int indexClass = -1;
            if (tipoOperando1 != null)
            {
                indexClass = Expressao.headers.cabecalhoDeClasses.FindIndex(k => k.nomeClasse == tipoOperando1);
            }
            else
            if (tipoOperando2 != null)
            {
                indexClass = Expressao.headers.cabecalhoDeClasses.FindIndex(k => k.nomeClasse == tipoOperando2);
            }

            if (indexClass == -1)
            {
                return null;
            }

            string nameClassOperator = classesOperadores[indexClass].nomeClasse;
            List<HeaderOperator> operators = classesOperadores[indexClass].operators;

            // verifica se o operador é binario, com operandos compativeis com os parametros de entrada.

            // verifica se o operador é unario.
            for (int op = 0; op < operators.Count; op++)
            {
                if (operators[op].tipoDoOperador == HeaderOperator.typeOperator.binary)
                {
                    if ((operators[op].name == nameOperator) && (operators[op].operands[0] == tipoOperando1) && (operators[op].operands[1] == tipoOperando2))
                    {
                        operatorCompatible = RepositorioDeClassesOO.Instance().GetClasse(nameClassOperator).GetOperador(operators[op].name);
                        operatorCompatible.tipo = "BINARIO";
                        isBinary = true;
                        isFoundOperator = true;
                    }
                }

                if ((operators[op].name == nameOperator) && (operators[op].tipoDoOperador == HeaderOperator.typeOperator.unary_pos) &&
                    (tipoOperando2 != null) && (operators[op].operands[0] == tipoOperando2))
                {
                    operatorCompatible = RepositorioDeClassesOO.Instance().GetClasse(nameClassOperator).GetOperador(operators[op].name);
                    operatorCompatible.tipo = "UNARIO POS";

                    isUnary = true;
                    isFoundOperator = true;

                }
                else
                if ((operators[op].name == nameOperator) && (operators[op].tipoDoOperador == HeaderOperator.typeOperator.unary_pos) &&
                    (tipoOperando1 != null) && (operators[op].operands[0] == tipoOperando1))
                {
                    operatorCompatible = RepositorioDeClassesOO.Instance().GetClasse(nameClassOperator).GetOperador(operators[op].name);
                    operatorCompatible.tipo = "UNARIO PRE";
                    isUnary = true;
                    isFoundOperator = true;

                }


            }
            // nao encontrou nenhum operador.
            if (!isFoundOperator)
            {
                return null;
            }
            // encontrou operador, verifica se é binario E unario.
            isBinaryAndUnary = isBinary && isUnary;
            return operatorCompatible;

        }

        /// <summary>
        /// encontra um metodo que seja compativel com a lista de parâmetros de uma chamada de método.
        /// se for para incluir o objeto caller na lista de parâmetros da chamada, faz e valida com a lista de parametros do metodo.
        /// </summary>
        /// <param name="objCaller">objeto que chama a funcao.</param>
        /// <param name="classObjCaller">classe do objeto da chamada de metodo.</param>
        /// <param name="nameMethod">nome do metodo</param>
        /// <param name="nameClass">classe do metodo</param>
        /// <param name="parameters">lista de parâmetros da chamada de metodo, que o metodo tenha para validar.</param>
        /// <param name="escopo">contexto onde a chamada de método está.</param>
        /// <param name="isStatic">chamada estatica ou nao.</param>
        /// <param name="isToIncludeFirstParameterObjectCaller">se [true], inclui o objeto caller como primeiro parametro.</param>
        /// <returns>retorna um metodo, com nome, e lista de parâmetros compativel aos parâmetros da chamada.</returns>
        public static Metodo FindMethodCompatible(Objeto objCaller, string classObjCaller, string nameMethod, string nameClass, List<Expressao> parameters, Escopo escopo, bool isStatic, bool isToIncludeFirstParameterObjectCaller)
        {
            
            Classe classeDoMetodo = RepositorioDeClassesOO.Instance().GetClasse(nameClass);
           
            if (classeDoMetodo == null)
            {
                throw new Exception("An error to find class name=" + nameClass + " of method: " + nameMethod + ".");
            }

            // obtem os metodos polimorticos, com o mesmo nome e classe, dos parametros de entrada.
            List<Metodo> lst_metodos = classeDoMetodo.GetMetodos();


            // obtem metodos de classe que herdaram a classe parametro.
            List<Classe> todasClasses = RepositorioDeClassesOO.Instance().GetClasses();
            if ((todasClasses != null) && (todasClasses.Count > 0)) 
            {
                for (int i = 0; i < todasClasses.Count; i++) 
                {
                    if ((todasClasses[i].classesHerdadas != null) && (todasClasses[i].classesHerdadas.FindIndex(k => k.nome == nameClass)) != -1)
                    {
                        int index = todasClasses[i].classesHerdadas.FindIndex(k => k.nome == nameClass);
                        List<Metodo> metodosQueHerdaramAClasseParam = todasClasses[i].classesHerdadas[index].GetMetodos().FindAll(k=>k.nome == nameClass);
                        if ((metodosQueHerdaramAClasseParam != null) && (metodosQueHerdaramAClasseParam.Count > 0))
                        {
                            lst_metodos.AddRange(metodosQueHerdaramAClasseParam);
                        }
                    }
                }
            }


            

            // obtem uma lista de metodos da classe do objeto caller, cujo nome é o mesmo do nome da chamada de metodo parametro.
            List<Metodo> metodos = lst_metodos.FindAll(k => k.nome == nameMethod).ToList<Metodo>();

            // se o metodo for importado, ou com acessor public, é mantido na lista de metodos.
            for (int i = 0; i < metodos.Count; i++)
            {
                // obtem metodos se forem importados, publicos, ou dentro do escopo da classe, ou privado dentro do escopo da classe currente..
                if ((metodos[i].isMethodImported) || (metodos[i].acessor == "public") || ((metodos[i].acessor == "private") && (metodos[i].nomeClasse == Escopo.nomeClasseCurrente)))
                {
                    continue;

                }
                // metodos fora de qualquer classe, são metodos globais, sendo possíveis acessá-los em qualquer classe.
                if (escopo.ID == Escopo.tipoEscopo.escopoGlobal)
                {
                    continue;
                }
                
            }



            // obtem uma lista de funções do escopo, cujo nome é o mesmo do nome de metodo da chamada de metodo parametro.
            List<Metodo> funcoes = escopo.tabela.GetFuncoes();


            // adiciona as funcoes, na lista de metodos a investigar a compatibilidade com a chamada de metodo que invocou este metodo.
            if ((funcoes != null) && (funcoes.Count > 0))
            {
                // se a lista de metodos resultou em uma lista vazia, pode ser que o metodo compativel esteja na lista de funcoes,
                // entao inicializa a lista de metodos, afim de investigar as funcoes.
                if (metodos == null)
                {
                    metodos = new List<Metodo>();
                }

                // adiciona as funcoes que tem o mesmo nome do metodo parametro.
                List<Metodo> funcoesComNomeDoMetodo = funcoes.FindAll(k => k.nome == nameMethod).ToList<Metodo>();
                if ((funcoesComNomeDoMetodo != null) && (funcoesComNomeDoMetodo.Count > 0))
                {
                    metodos.AddRange(funcoesComNomeDoMetodo);
                }

            }


            if ((metodos == null) || (metodos.Count == 0))
            {
                return null;
            }


            for (int x = 0; x < metodos.Count; x++)
            {
                bool isFound = true;
                // casos de metodos sem parametros.
                if ((metodos[x].parametrosDaFuncao == null) && (parameters == null))
                {
                    return metodos[x].Clone();
                }
                else
                if ((metodos[x].parametrosDaFuncao.Length == 0) && (parameters != null) && (parameters.Count == 0))
                {
                    return metodos[x].Clone();
                }
                else


                    if ((metodos[x].parametrosDaFuncao == null) && (parameters != null) && (parameters.Count > 0))
                {
                    return null;
                }
                else
                    if ((metodos[x].parametrosDaFuncao == null) && (parameters != null))
                {
                    return null;
                }
                else
                    if ((metodos[x].parametrosDaFuncao != null) && (parameters == null))
                {
                    return null;
                }
                else
                {

                    int indexParamsFunction = 0;
                    int indexParamsCHAMADADeMetodo = 0;
                    if (isToIncludeFirstParameterObjectCaller)
                    {
                        parameters.Insert(0, new ExpressaoObjeto(objCaller));

                    }

                    for (indexParamsFunction = 0, indexParamsCHAMADADeMetodo = 0; indexParamsFunction < metodos[x].parametrosDaFuncao.Length; indexParamsFunction++, indexParamsCHAMADADeMetodo++)
                    {
                        

                        // obtem o tipo do parametro currente do metodo investigado.
                        string tipoParamsFnc = metodos[x].parametrosDaFuncao[indexParamsFunction].tipo;
                        // CASTING DE PARAMETROS NUMEROS.
                        CastingExpressao(x, indexParamsCHAMADADeMetodo, indexParamsFunction, parameters, metodos, tipoParamsFnc);


                        // VERIFICACAO SE O OBJETO-CALLER DEVE SER INCLUIDO NOS PARAMETROS.
                        if ((LinguagemOrquidea.Instance().isClassToIncludeObjectCallerAsParameter(classeDoMetodo.GetNome())) && (!isStatic)) 
                        {
                            if ((classObjCaller == metodos[x].parametrosDaFuncao[indexParamsFunction].tipo) &&
                                (indexParamsCHAMADADeMetodo < parameters.Count) &&
                                (indexParamsFunction < metodos[x].parametrosDaFuncao.Length)) 
                            {
                                if (parameters[indexParamsCHAMADADeMetodo].tipoDaExpressao == metodos[x].parametrosDaFuncao[indexParamsFunction].tipo)
                                {
                                    continue;
                                }
                                else
                                {
                                    break;
                                }
                            }
                           
                        }


                        // VERIFICACAO DE O PARAMETRO DO METODO É DO TIPO [OBJECT].
                        if (metodos[x].parametrosDaFuncao[indexParamsFunction].tipo == "Object")
                        {
                            continue;
                        }
                        else
                        /// PROCESSAMENTO DE  PARAMETRO-METODO.
                        if ((metodos[x].parametrosDaFuncao[indexParamsFunction].isFunctionParameter) && (parameters[indexParamsCHAMADADeMetodo].GetType() == typeof(ExpressaoChamadaDeMetodo)))
                        {
                            ExpressaoChamadaDeMetodo expressaoChamada = (ExpressaoChamadaDeMetodo)parameters[indexParamsCHAMADADeMetodo];

                            // obtem o metodo parametro na expressao chamada de metodo.
                            Metodo metodoChamada = expressaoChamada.funcao;

                            HeaderClass headerParametroFuncao = Expressao.headers.cabecalhoDeClasses.Find(k => k.nomeClasse == metodos[x].nomeClasse);
                            if (headerParametroFuncao != null)
                            {


                                HeaderMethod headerMethod = headerParametroFuncao.methods.Find(k => k.name == metodos[x].nome);
                                if (headerMethod != null)
                                {
                                    List<HeaderProperty> paramtersOfFuncionParameter = headerMethod.parameters;
                                    if ((paramtersOfFuncionParameter.Count == 0) && (metodos[x].parametrosDaFuncao.Length == 0))
                                    {
                                        continue;
                                    }
                                    if ((paramtersOfFuncionParameter.Count != 0) && (metodos[x].parametrosDaFuncao.Length == 0))
                                    {
                                        break;
                                    }
                                    if ((paramtersOfFuncionParameter.Count == 0) && (metodos[x].parametrosDaFuncao.Length != 0))
                                    {
                                        break;
                                    }
                                    // validacao de tipos de retorno.
                                    if (metodos[x].tipoReturn != headerMethod.typeReturn)
                                    {
                                        break;
                                    }
                                    // validacao dos tipos de parametros.
                                    for (int p = 0; p < paramtersOfFuncionParameter.Count; p++)
                                    {
                                        if (metodos[x].parametrosDaFuncao[p].tipo != ((ObjectHeader)paramtersOfFuncionParameter[p]).className)
                                        {
                                            break;
                                        }
                                    }

                                }
                                else
                                {
                                    break;
                                }
                            }
                
                        }
                        else
                        // O PARAMETRO É UM PARAMETRO NORMAL.
                        if ((indexParamsCHAMADADeMetodo < parameters.Count) && (!metodos[x].parametrosDaFuncao[indexParamsFunction].isFunctionParameter) &&(!metodos[x].parametrosDaFuncao[indexParamsFunction].isMultArgument) && (!metodos[x].isToIncludeCallerIntoParameters)) 
                        {
                           
                            if ((!metodos[x].parametrosDaFuncao[indexParamsFunction].isFunctionParameter) && 
                                (parameters[indexParamsCHAMADADeMetodo].tipoDaExpressao == metodos[x].parametrosDaFuncao[indexParamsFunction].tipo))
                            {
                                continue;
                            }
                            else
                            {
                                isFound = false;
                                break;
                            }
                        }


                        // PARAMETRO É MULTI-ARGUMENTO.
                        // o parametro é um multi-argumento (lista variavel de parametros), e é do tipo [Vector], que armazenará os valores em quantidade variavel.
                        if (metodos[x].parametrosDaFuncao[indexParamsFunction].isMultArgument)
                        {
                            if (metodos[x].parametrosDaFuncao[indexParamsFunction].GetTipo() == "Vector")
                            {
                                // faz a validacao de parametro multi-argumento.
                                if (!ValidatingParametersMultiArguments(metodos[x]))
                                {
                                    int i_multi = indexParamsCHAMADADeMetodo;
                                    int contadorArgs = 0;
                                    string tipoDoMultiArgumento = metodos[x].parametrosDaFuncao[indexParamsFunction].tipoElemento;

                                    // obtem a faixa de objetos dentro de multi-argumento.
                                    while ((i_multi < parameters.Count) && (parameters[i_multi].GetTipoExpressao() == tipoDoMultiArgumento))
                                    {
                                        i_multi++;
                                        contadorArgs++;
                                    }

                                    // verifica se atingiu tokens de parametro multi-agumento;
                                    if (i_multi <= parameters.Count)
                                    {

                                        // acerta o indice de malha de expressoes parametro.
                                        indexParamsCHAMADADeMetodo += contadorArgs;
                                        continue;
                                    }
                                    else
                                    {
                                        isFound = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    isFound = false;
                                    break;
                                }

                            }
                            else
                            {
                                isFound = false;
                                break;
                            }
                        }

                    }

                    if (isFound)
                    {
                        return metodos[x].Clone();
                    }

                    else
                        continue;

                }




            }

            return null;
        }

        /// <summary>
        /// faz o casting entre numeros: double,float,int, convertendo o numero da expressao para o numero do parametro da funcao.
        /// </summary>
        /// <param name="x">indice do metodo currente.</param>
        /// <param name="indexParamsCHAMADADeMetodo">indice do parametro da chamada de metodo.</param>
        /// <param name="indexParamsFunction">indice do parametro da lista de parametros do metodo compativel.</param>
        /// <param name="parameters">lista de expressoes parametros da chamada de metodo.</param>
        /// <param name="metodos">lista de metodos compativeis ou nao.</param>
        /// <param name="tipoParamsFnc">tipo do parametro da função, como base para a conversão.</param>
        private static void CastingExpressao(int x, int indexParamsCHAMADADeMetodo,int indexParamsFunction, List<Expressao> parameters, List<Metodo>metodos, string tipoParamsFnc)
        {

            // VEFIRICA SE O PARAMETRO É UMA CONSTANTE NUMERO, E FAZ CASTING ENTRE OS NUMEROS DA FUNCAO E DA CHAMADA, SE NECESSÁRIO.
            if ((indexParamsCHAMADADeMetodo < parameters.Count) && ((parameters[indexParamsCHAMADADeMetodo].GetType() == typeof(ExpressaoNumero)) || (parameters[indexParamsCHAMADADeMetodo].GetType() == typeof(ExpressaoObjeto))) && ((tipoParamsFnc == "double") || (tipoParamsFnc == "float") || (tipoParamsFnc == "int")))
            {
            
                string tipoNumberRequired = metodos[x].parametrosDaFuncao[indexParamsFunction].tipo;
                string tipoNumberChamada = parameters[indexParamsCHAMADADeMetodo].tipoDaExpressao;


                // conversao de int/float para double.
                if (tipoNumberRequired == "double") 
                {
                    parameters[indexParamsCHAMADADeMetodo].tipoDaExpressao = "double";
                    if (parameters[indexParamsCHAMADADeMetodo].GetType() == typeof(ExpressaoObjeto))
                    {
                        ((ExpressaoObjeto)parameters[indexParamsCHAMADADeMetodo]).objectCaller.tipo = "double";
                    }
                    
                    return;
                }


                // conversao de float/double para int.
                if (tipoNumberRequired == "int")
                {
                    if (tipoNumberChamada == "double")
                    {
                        parameters[indexParamsCHAMADADeMetodo].tipoDaExpressao = "int";
                        if (parameters[indexParamsCHAMADADeMetodo].GetType() == typeof(ExpressaoObjeto))
                        {
                            ((ExpressaoObjeto)parameters[indexParamsCHAMADADeMetodo]).objectCaller.tipo = "int";
                        }

                        return;
                    }
                    else
                    if (tipoNumberChamada == "float")
                    {
                        parameters[indexParamsCHAMADADeMetodo].tipoDaExpressao = "int";

                        if (parameters[indexParamsCHAMADADeMetodo].GetType() == typeof(ExpressaoObjeto))
                        {
                            ((ExpressaoObjeto)parameters[indexParamsCHAMADADeMetodo]).objectCaller.tipo = "int";
                        }

                        return;
                    }

                }


            }


        }


        /// <summary>
        /// extrai os dados de metodo parametro.
        /// </summary>
        /// <param name="parametroDoMetodoPrincipal">parmaetros do metodo que contem o metodo parametro.</param>
        /// <returns>retorna uma lista de metodos com a mesma classe e nome do metodo-parametro.</returns>
        private static List<Metodo> ExtractDataMethod(Objeto parametroDoMetodoPrincipal)
        {
            Objeto metodoObjeto = parametroDoMetodoPrincipal;
            string nomeClassMetodo = metodoObjeto.GetTipo();
            string nomeMetodo = metodoObjeto.GetNome();

            Classe classeMetodo = RepositorioDeClassesOO.Instance().GetClasse(nomeClassMetodo);
            if (classeMetodo != null)
            {
                List<Metodo> metodosFound = classeMetodo.GetMetodos().FindAll(k => k.nome == nomeMetodo);
                return metodosFound;
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// verifica se há metodos equivalentes, de um metodo vindo de uma expressao de chamada de metodo,
        /// e uma lista de metodos da classe do currente.
        /// </summary>
        /// <param name="metodos">lista de metodos da classe do metodo parametro.</param>
        /// <param name="metodoChamada">metodo da expressao chamada de metodo.</param>
        /// <returns></returns>
        private static bool IsMethodsEquivalents(Metodo metodoParametro, Metodo metodoChamada)
        {
            if ((metodoParametro.tipoReturn != null) && (metodoChamada.tipoReturn != null))
            {
                if (metodoParametro.tipoReturn == metodoChamada.tipoReturn)
                {
                    if ((metodoParametro.parametrosDaFuncao != null) && (metodoChamada.parametrosDaFuncao != null) &&
                    (metodoChamada.parametrosDaFuncao.Length == metodoParametro.parametrosDaFuncao.Length))
                    {
                        return ValidaParametrosEntre2Metodos(metodoParametro, metodoChamada);
                    }
                    else
                    if ((metodoParametro.parametrosDaFuncao == null) && (metodoChamada.parametrosDaFuncao == null))
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }

            }
         

            return false;
        }
        private static bool ValidaParametrosEntre2Metodos(Metodo metodoParametro, Metodo metodoChamada)
        {
            if (metodoParametro.parametrosDaFuncao.Length == metodoChamada.parametrosDaFuncao.Length)
            {
                for (int i = 0 , j=0; i < metodoParametro.parametrosDaFuncao.Length; i++, j++)
                {
                    // se o parametro foi um metodo, avanca para o proximo parametro.
                    if (metodoChamada.parametrosDaFuncao[i] is Metodo)
                    {
                        j--; // acerta o indice de malha do metodo chamada.
                        continue;
                    }
                    // se os tipos dos parametros forem iguais, avança para o proximo parametro.
                    if (metodoChamada.parametrosDaFuncao[i].tipo == metodoParametro.parametrosDaFuncao[j].tipo)
                    {
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// retorna true se há parametros multi-argummentos invalidados.
        /// 2 parametros multi-argumentos em sequencia não podem ter o mesmom tipo,
        /// pois torna impossivel saber se um parametro pertence a qual parametro-argumento.
        /// </summary>
        /// <param name="method">metodo com os parmaetros multi-argumentos.</param>
        /// <returns>[true] se é invalido o metodo com parametros multi-argumentos, [false] se houve irregularidade.</returns>
        public static bool ValidatingParametersMultiArguments(Metodo method)
        {
            if ((method.parametrosDaFuncao != null) && (method.parametrosDaFuncao.Length >= 2))
            {
                for (int i = 1; i < method.parametrosDaFuncao.Length; i++)
                {
                    if ((method.parametrosDaFuncao[i].isMultArgument) &&
                       (method.parametrosDaFuncao[i - 1].isMultArgument))
                    {
                        if (method.parametrosDaFuncao[i].tipo == method.parametrosDaFuncao[i - 1].tipo)
                        {
                            return true;
                        }
                        else
                        if ((WrapperData.isWrapperData(method.parametrosDaFuncao[i].tipo) != null) &&
                            (method.parametrosDaFuncao[i].tipoElemento == method.parametrosDaFuncao[i - 1].tipoElemento)) 
                        {
                            return true;
                        }
                    }


                }
            }
            return false;

        }

   
        public static void LinkEscopoPaiEscopoFilhos(Escopo escopoPai, Escopo escopoFilho)
        {
            if ((escopoPai != null) && (escopoFilho != null))
            {
                escopoFilho.escopoPai = escopoPai;
                escopoPai.escopoFolhas.Add(escopoFilho);
            }
            
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



        public class Testes: SuiteClasseTestes
        {
            public Testes():base("testes de classe util tokens")
            {

            }

            public void TesteMetodosParametrosComChamadaDeFuncao(AssercaoSuiteClasse assercao)
            {
                string codigo_classeA_0_0 = "public class classeA { public int propriedadeA; public int metodoB(int funcaco(int x), int a) { funcao(1);} public classeA(){int x=1; };  public int metodoA(int y){int x=2;}; };";
                string codigo_create = "classeA obj1= create();";
                string codigo_expressaoChamada = "obj1.metodoB(obj1.metodoA(1),1)";


                ProcessadorDeID compilador = new ProcessadorDeID(codigo_classeA_0_0 + codigo_create);
                compilador.Compilar();

                Expressao exprssChamada = new Expressao(codigo_expressaoChamada, compilador.escopo);

                try
                {
                    assercao.IsTrue(exprssChamada.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo), codigo_expressaoChamada);
                    assercao.IsTrue(((ExpressaoChamadaDeMetodo)exprssChamada.Elementos[0]).parametros[0].Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo), codigo_expressaoChamada);
                   
                }
                catch (Exception e)
                {
                    string codeError = e.Message;
                    assercao.IsTrue(false, "FALHA NO TESTE:");
                }

            }
            public void TesteParametrosMetodos(AssercaoSuiteClasse assercao)
            {
                string codigo_classeA_0_0 = "public class classeA { public int propriedadeA; public int metodoA(int y){int x=2;};  public classeA(){int x=1; }; public int metodoB(int funcaco(int x), a) {int m=1;}};";
                string codigo_create = "classeA obj1= create();";
                string codigo_expressaoChamada = "obj1.metodoB(obj1.metodoA(1),1)";

                
                ProcessadorDeID compilador = new ProcessadorDeID(codigo_classeA_0_0 + codigo_create);
                compilador.Compilar();

                Expressao exprssChamada = new Expressao(codigo_expressaoChamada, compilador.escopo);

                try
                {
                    assercao.IsTrue(exprssChamada.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo), codigo_expressaoChamada);
                    assercao.IsTrue(((ExpressaoChamadaDeMetodo)exprssChamada.Elementos[0]).parametros[0].Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo), codigo_expressaoChamada);
                }
                catch (Exception e)
                {
                    string codeError = e.Message;
                    assercao.IsTrue(false, "FALHA NO TESTE:");
                }

            }

  


        }
    }


}

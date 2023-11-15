using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using parser.ProgramacaoOrentadaAObjetos;
using Wrappers.DataStructures;

namespace parser
{


    public class EvalExpression
    {

  
        public object EvalPosOrdem(Expressao expss_, Escopo escopo)
        {
            // converte a expressao para pos-ordem.
            if (!expss_.isPosOrdem)
            {
                if ((expss_.Elementos != null) &&
                    (expss_.Elementos.Count > 0) &&
                    (expss_.Elementos[0] != null) &&
                    (expss_.Elementos[0].GetType() == typeof(ExpressaoOperador))) 
                {
                    expss_.Elementos[0].PosOrdemExpressao();
                }
                else
                {
                    expss_.PosOrdemExpressao();
                }

                expss_.isPosOrdem = true;
            }


            if (expss_.GetType() == typeof(ExpressaoNumero))
            {
                return Expressao.Instance.ConverteParaNumero(expss_.ToString(), escopo);
            }
                


            if (expss_.isModify == false)
            {
                return expss_.oldValue;
            }
            else
            {
                // recalcula a expressao.
                expss_.isModify = true;
                object valorExpressao = null;
                if ((expss_.Elementos != null) && (expss_.Elementos.Count > 0) && (expss_.Elementos[0] != null) && (expss_.Elementos[0].GetType() == typeof(ExpressaoOperador))) 
                {
                    if (!isExpressionOperatorPresent(expss_))
                    {
                        valorExpressao = this.Eval(expss_.Elementos[0], escopo);
                    }
                    else
                    {
                        expss_.PosOrdemExpressao();
                        valorExpressao = this.Eval(expss_, escopo);
                    }
                    
                }
                else
                {
                    valorExpressao = this.Eval(expss_, escopo);

                }

                expss_.oldValue = valorExpressao;

                return valorExpressao;
            }
        
        } // EvalPosOrdem()

        protected object Eval(Expressao expss, Escopo escopo)
        {

            if (Expressao.Instance.IsNumero(expss.ToString()))
                return Expressao.Instance.ConverteParaNumero(expss.ToString(), escopo);


            object result1 = 0;
            Pilha<object> pilhaOperandos = new Pilha<object>("pilhaOperandos");



            if (expss.Elementos.Count == 0)
            {

                if ((expss.GetType() == typeof(Expressao)) && (expss.tokens != null) && (expss.tokens.Count > 0)) 
                {
                    if (escopo.tabela.GetObjeto(expss.tokens[0].ToString(), escopo) != null)
                    {
                        return escopo.tabela.GetObjeto(expss.tokens[0].ToString(), escopo);
                    }
                    else
                    {
                        return expss.tokens[0];
                    }
                    
                }
                
                // constante string formando um unico token.
                if (expss.GetType() == typeof(ExpressaoLiteralText))
                {
                    return ((ExpressaoLiteralText)expss).literalText;
                }
                else
                // constante numero formando um unico token.
                if (ExpressaoNumero.isNumero(expss.ToString()))
                {
                    bool isFoundANumber = false;
                    object numero = null;
                    string str_numero = expss.ToString();
                    GetNumber(ref numero, ref isFoundANumber, str_numero, escopo);
                    if (isFoundANumber)
                    {
                        pilhaOperandos.Push(numero);                        
                    }

                }
                else 
                // um objeto somente, sem sub-expressoes.
                if (expss.GetType() == typeof(ExpressaoObjeto))
                {
                    return ((ExpressaoObjeto)expss).objectCaller.GetValor();
                }
                else
                {
                    return null;
                }
                    
            }


            // obtem o tipo da expressao.
            string tipoDaExpressao = Expressao.Instance.GetTipoExpressao(expss, escopo);
           
            for (int x = 0; x < expss.Elementos.Count; x++)
            {
                if (expss.Elementos[x].GetType() == typeof(ExpressaoLiteralText))
                {
                    ExpressaoLiteralText exprssLiteral = (ExpressaoLiteralText)expss.Elementos[x];
                    pilhaOperandos.Push(exprssLiteral.literalText);                
                }

                // EXPRESSAO NILL.
                if (expss.Elementos[x].GetType() == typeof(ExpressaoNILL))
                {
                    object elementoAVerificar = pilhaOperandos.Pop();
                    if (elementoAVerificar == null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
                else
                // EXPRESSAO ATRIBUICAO.
                if (expss.Elementos[x].GetType() == typeof(ExpressaoAtribuicao))
                {
                    ExpressaoAtribuicao exprssAtribui = ((ExpressaoAtribuicao)expss.Elementos[x]);
                  
                    object valorAtribuir = EvalPosOrdem(exprssAtribui.exprssAtribuicao, escopo);
                    Objeto objetoAtribuicao = null;
                    
                    // O OBJETO DE RETORNO É DO TIPO PROPRIEDADES ANINHADAS.
                    if (exprssAtribui.exprssObjetoAAtribuir.GetType() == typeof(ExpressaoPropriedadesAninhadas))
                    {
                        ExpressaoPropriedadesAninhadas exprsAninhadas = ((ExpressaoPropriedadesAninhadas)exprssAtribui.exprssObjetoAAtribuir);
                        objetoAtribuicao = exprsAninhadas.aninhamento[exprsAninhadas.aninhamento.Count - 1];
                    }
                    else
                    // O OBJETO DE RETORNO É DO TIPO OBJETO.
                    if (exprssAtribui.exprssObjetoAAtribuir.GetType() == typeof(ExpressaoObjeto))
                    {
                        objetoAtribuicao = ((ExpressaoObjeto)exprssAtribui.exprssObjetoAAtribuir).objectCaller;

                    }

                    // se o objeto da atribuicao for da classe Objeto, seta o valor do objeto da atribuicao.
                    if ((objetoAtribuicao != null) && (objetoAtribuicao is Objeto)) 
                    {
                        objetoAtribuicao.valor = valorAtribuir;
                        pilhaOperandos.Push(objetoAtribuicao);
                    }
                    
                    // SE o objeto for de uma expressao objeto, ou expressao propriedades aninhadas, seta o valor do objeto atribuicao, e retorna com o objeto da atribuição.
                    if (objetoAtribuicao != null)
                    {
                        WrapperData.CastingObjeto(valorAtribuir, objetoAtribuicao);
                        pilhaOperandos.Push(objetoAtribuicao);
                    }
                    // SENAO, retorna o valor sem atribuir a algum objeto.
                    else
                    {
                        return valorAtribuir;
                    }
                }
                // EXPRESSAO PROPRIEDADES ANINHADAS.
                if (expss.Elementos[x].GetType() == typeof(ExpressaoPropriedadesAninhadas))
                {
                    // faz o processamento basico de uma propriedade aninhada, ex.: "a.propriedadeB";
                    ExpressaoPropriedadesAninhadas exprss_aninhadas = (ExpressaoPropriedadesAninhadas)expss.Elementos[x];
                    Objeto obj_callerCurrente = exprss_aninhadas.objectCaller;
                    Objeto obj_propriedade = exprss_aninhadas.aninhamento[exprss_aninhadas.aninhamento.Count - 1];

                    if ((obj_propriedade != null) && (obj_propriedade.valor != null))
                    {
                        if ((exprss_aninhadas.aninhamento != null) &&
                               (exprss_aninhadas.aninhamento.Count > 0) &&
                               ((exprss_aninhadas.Elementos == null) || (exprss_aninhadas.Elementos.Count == 0)))
                        {
                            // o valor a ser guardado é da ultima propriedade do aninhamento.
                            Objeto objetoPropriedade = exprss_aninhadas.aninhamento[exprss_aninhadas.aninhamento.Count - 1];
                            pilhaOperandos.Push(objetoPropriedade.GetValor());

                        }
                        else
                           if ((exprss_aninhadas.Elementos != null) &&
                               (exprss_aninhadas.Elementos.Count > 0) &&
                               (exprss_aninhadas.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo)))
                        {
                            ExpressaoChamadaDeMetodo exprsessaoChamadaAninhada = (ExpressaoChamadaDeMetodo)exprss_aninhadas.Elementos[0];
                            Metodo fnc = exprsessaoChamadaAninhada.funcao;
                            List<Expressao> parametros = exprsessaoChamadaAninhada.parametros;
                            if ((parametros == null) || (parametros.Count == 0))
                            {
                                parametros = new List<Expressao>();
                            }
                            object result = fnc.ExecuteAMethod(parametros, escopo, exprsessaoChamadaAninhada.objectCaller);
                            pilhaOperandos.Push(result);


                        }
                        else
                        {
                            UtilTokens.WriteAErrorMensage("property of object: " + obj_callerCurrente.GetNome() + " is null but enter in calculus", exprss_aninhadas.tokens, escopo);
                            throw new Exception("property of object: " + obj_callerCurrente.GetNome() + " is null but enter in calculus");
                        }




                    }
                    else
                    {
                        throw new Exception("property of object: " + obj_callerCurrente.GetNome() + " it is null, but it was enter in calculus!");
                    }


                }
                else
                // EXPRESSAO CHAMADA DE METODO.
                if (expss.Elementos[x].GetType() == typeof(ExpressaoChamadaDeMetodo))
                {


                    // forma a chamada de metodo inicial.
                    ExpressaoChamadaDeMetodo umaChamadaDeMetodo = (ExpressaoChamadaDeMetodo)expss.Elementos[x];

                    List<Expressao> exprssParametros = umaChamadaDeMetodo.parametros;
                    if (exprssParametros == null)
                    {
                        exprssParametros = new List<Expressao>();
                    }
                    Metodo fnc = umaChamadaDeMetodo.funcao;
                    object result = fnc.ExecuteAMethod(exprssParametros, escopo, umaChamadaDeMetodo.objectCaller);
                    pilhaOperandos.Push(result);

                    // faz o processamento de chamadas de metodos adicionais,. ex: "a.metodoA(x,y).metodoB(a)", ".metodoB" é uma chamada de metodo adicional. 
                    if ((umaChamadaDeMetodo.Elementos != null) && (umaChamadaDeMetodo.Elementos.Count > 0))
                    {
                        int i = 0;
                        // faz o processamento de chamadas de metodos adicionais.
                        while (i < umaChamadaDeMetodo.Elementos.Count)
                        {
                            if (umaChamadaDeMetodo.Elementos[i].GetType() == typeof(ExpressaoChamadaDeMetodo))
                            {
                                ExpressaoChamadaDeMetodo chamadaAdicional = (ExpressaoChamadaDeMetodo)umaChamadaDeMetodo.Elementos[i];

                                List<Expressao> exprssParametros2 = chamadaAdicional.parametros;
                                object result2 = chamadaAdicional.funcao.ExecuteAMethod(exprssParametros2, escopo, chamadaAdicional.objectCaller);

                                pilhaOperandos.Pop();
                                pilhaOperandos.Push(result2);


                            }
                            i++;


                        }

                    }


                }
                else
                if (expss.Elementos[x].GetType() == typeof(Expressao))
                {
                    // o elemento da expressão é outra expressão.
                    EvalExpression evalExpressaoElemento = new EvalExpression();
                    object result = evalExpressaoElemento.EvalPosOrdem(expss.Elementos[x], escopo); // avalia a expressão elemento.
                    pilhaOperandos.Push(result);
                }
                else
                // EXPRESSAO NUMERO.
                if (expss.Elementos[x].GetType() == typeof(ExpressaoNumero))
                {
                    object numero = null;
                    string str_numero = ((ExpressaoNumero)expss.Elementos[x]).numero;
                    bool isFoundANumber = false;

                    GetNumber(ref numero, ref isFoundANumber, str_numero, escopo);
                    if (isFoundANumber)
                    {
                        pilhaOperandos.Push(numero);
                    }

                }
                else
                // EXPRESSAO OBJETO.
                if (expss.Elementos[x].GetType() == typeof(ExpressaoObjeto))
                {
                    Objeto v = escopo.tabela.GetObjeto(((ExpressaoObjeto)expss.Elementos[x]).nomeObjeto, escopo);
                    if (v != null)
                    {
                        pilhaOperandos.Push(v.valor);
                    }
                    else
                    {
                        pilhaOperandos.Push(null);

                    }
                }
                else
                // EXPRESSAO ELEMENTO.
              if (expss.Elementos[x].GetType() == typeof(ExpressaoElemento))
                {

                    // nova funcionalidade: elementos que podem ser o nome de variáveis.
                    Objeto v = escopo.tabela.GetObjeto(((ExpressaoElemento)expss.Elementos[x]).GetElemento().ToString(), escopo);
                    if (v != null)
                    {
                        pilhaOperandos.Push(v.GetValor());
                    }

                }
                else
                // EXPRESSAO OPERADOR.
                if (expss.Elementos[x].GetType() == typeof(ExpressaoOperador))
                {

                    Operador operador = ((ExpressaoOperador)expss.Elementos[x]).operador;
                    if (operador.tipo == "BINARIO")
                    {

                        if (operador != null)
                        {
                            if (operador.nome == "=")
                            {
                                object novoValor = pilhaOperandos.Pop();

                                // RETORNO com atribuicao OBJETO.
                                if (expss.Elementos[0].GetType() == typeof(ExpressaoObjeto))
                                {
                                    escopo.tabela.GetObjeto(((ExpressaoObjeto)expss.Elementos[0]).nomeObjeto, escopo).SetValor(novoValor);
                                }
                                else
                                // RETORNO com atribuicao PROPRIEDADES ANINHADAS.
                                if (expss.Elementos[0].GetType() == typeof(ExpressaoPropriedadesAninhadas))
                                {
                                    ExpressaoPropriedadesAninhadas epxrssRetornoPropriedades = (ExpressaoPropriedadesAninhadas)expss.Elementos[0];
                                    epxrssRetornoPropriedades.aninhamento[epxrssRetornoPropriedades.aninhamento.Count - 1].SetValor(novoValor);

                                }

                            }
                            else
                            {
                                object oprnd2 = pilhaOperandos.Pop();
                                object oprnd1 = pilhaOperandos.Pop();

                                // o metodo "Parser_Number" NÃO SE APLICA SOMENTE A NUMEROS, MAS SE 
                                // NÃO FOR NUMERO, RETORNA O VALOR DO TIPO DE OBJETO...
                                oprnd1 = this.Parser_OPERANDOS(oprnd1);
                                oprnd2 = this.Parser_OPERANDOS(oprnd2);

                                // execução de operador nativo.
                                result1 = operador.ExecuteOperador(operador.nome, escopo, oprnd1, oprnd2);
                                pilhaOperandos.Push(result1);
                            } // else
                        } // if
                    }
                    else
                    if (operador.tipo.Contains("UNARIO"))
                    {



                        object oprnd2 = pilhaOperandos.Pop();

                        if (oprnd2 == null)
                        {
                            continue;
                        }
                        oprnd2 = this.Parser_OPERANDOS(oprnd2);


                        object valor = oprnd2;

                        if (operador.tipo.Contains("POS"))
                        {
                            // primeiro guarda o valor, depois faz a operacao do operador.
                            pilhaOperandos.Push(valor);
                            // ex.: c++. (unario pos)
                            object valorPOS_UNARIO = operador.ExecuteOperador(operador.nome, escopo, oprnd2);

                            // atualiza o valor do operando, cumprindo o script de operador unario.
                            if (oprnd2.GetType() == typeof(Objeto))
                            {
                                Objeto objOperando = (Objeto)oprnd2;
                                objOperando.SetValor(valorPOS_UNARIO);
                            }



                        }
                        else
                        if (operador.tipo.Contains("PRE"))
                        {
                            // primeiro faz a operacao do operador, depois guarda o valor.
                            // ex.: ++c. (unario pre)
                            object valorPRE_UNARIO = operador.ExecuteOperador(operador.nome, escopo, oprnd2);
                            pilhaOperandos.Push(valorPRE_UNARIO);

                            /// atualiza o valor do operando.
                            if (oprnd2.GetType() == typeof(Objeto))
                            {
                                Objeto objOperando = (Objeto)oprnd2;
                                objOperando.SetValor(valorPRE_UNARIO);
                            }
                        }




                    }
                } 

            } // for x
            if (pilhaOperandos.lenghtPilha > 0)
            {
                result1 = pilhaOperandos.Pop();
            }
            return result1;
        } // Eval()

        /// <summary>
        /// obtem um numero a partir de um token [str_numero].
        /// </summary>
        /// <param name="numero">numero da conversao.</param>
        /// <param name="isFoundANumber">[true] se converteu o token para um numero: int, float, double.</param>
        /// <param name="str_numero">token a ser convertido em numero.</param>
        /// <param name="escopo">contexto onde o numero está.</param>
        public static void GetNumber(ref object numero, ref bool isFoundANumber, string str_numero, Escopo escopo)
        {
            // o objeto tem valor como numero.
            if (ExpressaoNumero.isNumero(str_numero))
            {
                isFoundANumber = true;
                ExpressaoNumero exprss = new ExpressaoNumero();
                numero= exprss.ConverteParaNumero(str_numero, escopo);
            }
            else
            {
                isFoundANumber = false;
            }
        }


        /// <summary>
        /// verifica se a expressao tem sub-expressoes expressao operador.
        /// </summary>
        /// <param name="exprss">expressao a verificar.</param>
        /// <returns>[true] se contem sub-expressoes [ExpressaoOperador].</returns>
        private static bool isExpressionOperatorPresent(Expressao exprss)
        {
            if ((exprss == null) || (exprss.Elementos == null) || (exprss.Elementos.Count == 0))
            {
                return false;
            }
            for (int x=0;x< exprss.Elementos.Count;x++)
            {
                if (exprss.Elementos[x].GetType() == typeof(ExpressaoOperador)) 
                {
                    return true;
                }
            }
            return false;
        }




        /// <summary>
        ///  se for um numero, transforma para object.
        ///  se não for um número, e se for Objeto, retorna o valor do Objeto,
        ///  senão retorna o objeto de entrada.
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        private object Parser_OPERANDOS(object number)
        {
            if (number == null)
            {
                return null;
            }
            else
            if (Expressao.Instance.IsTipoInteiro(number.ToString()))
            {
                return int.Parse(number.ToString());
            }
            else
            if (Expressao.Instance.IsTipoFloat(number.ToString()))
            {
                return float.Parse(number.ToString());
            }
            else
            if (Expressao.Instance.IsTipoDouble(number.ToString()))
            {
                return double.Parse(number.ToString());
            }
            else
            if (number.GetType() == typeof(Objeto))
            {
                Objeto obj = (Objeto)number;
                return obj.GetValor();
            }
            else
            {
                return number;
            }
                
        }


        public class Testes : SuiteClasseTestes
        {
            public Testes() : base("testes para availiacao de expressoes.")
            {
            }
           
            char aspas = ExpressaoLiteralText.aspas;
            public static char singleQuote = '\u0020';

            private EvalExpression eval = new EvalExpression();





            public void TestesFuncoesClasseString(AssercaoSuiteClasse assercao)
            {


                string codigoCreate_0_0 = "string s;";
                string codigoCreate_0_1 = "string texto= " + aspas + "este e um texto literal" + aspas + ";";
                string codigoCreate_0_2 = "bool b= true;";
                string codigoCreate_0_3 = "string textoToLower=" + aspas + "MARTE" + aspas + ";";
                string codigoCreate_0_5 = "string textoToReplace=" + aspas + "Terra Azul" + aspas + ";";
                string codigoCreate_0_6 = "int x=0;";

                string codigo_0_0 = "string.textFromInt(5)";
                string codigo_0_1 = "string.Contains(" + aspas + "este e um texto literal" + aspas + "," + aspas + "literal" + aspas + " );";
                string codigo_0_2 = "string.Start(" + aspas + "comeco eh tudo" + aspas + "," + aspas + "comeco" + aspas + ")";
                string codigo_0_3 = "string.EqualsText(" + aspas + "Terra" + aspas + "," + aspas + "Marte" + aspas + ")";
                string codigo_0_5 = "string.Index(" + aspas + "numero" + aspas + "," + aspas + "num" + aspas + ")";
                string codigo_0_6 = "string.ReplaceFor(" + aspas + "Terra Azul" + aspas + "," + aspas + "Azul" + aspas + "," + aspas + "Vermelha" + aspas + ");";
                string codigo_0_7 = "string._Upper(" + aspas + "terra" + aspas + ");";
                string codigo_0_8 = "textoToLower._Lower()";
                string codigo_0_9 = "textoToReplace.ReplaceFor(" + aspas + "Azul" + aspas + "," + aspas + "Verde" + aspas + ");";
                string codigo_0_9_1 = "x= string.Index(" + aspas + "numero" + aspas + ", " + aspas + "num" + aspas + ")";



                EvalExpression eval = new EvalExpression();

                ProcessadorDeID compilador = new ProcessadorDeID(codigoCreate_0_0 + codigoCreate_0_1 + codigoCreate_0_2 + codigoCreate_0_3 + codigoCreate_0_5 + codigoCreate_0_6);
                compilador.Compilar();

                Expressao exprss_0_9 = new Expressao(codigo_0_9, compilador.escopo);
                object result_0_9 = eval.EvalPosOrdem(exprss_0_9, compilador.escopo);



                Expressao exprss_0_9_1 = new Expressao(codigo_0_9_1, compilador.escopo);
                Expressao exprss_0_8 = new Expressao(codigo_0_8, compilador.escopo);
                Expressao express_0_7 = new Expressao(codigo_0_7, compilador.escopo);
                Expressao express_0_6 = new Expressao(codigo_0_6, compilador.escopo);
                Expressao express_0_5 = new Expressao(codigo_0_5, compilador.escopo);
                Expressao express_0_3 = new Expressao(codigo_0_3, compilador.escopo);
                Expressao express_0_2 = new Expressao(codigo_0_2, compilador.escopo);
                Expressao express_0_0 = new Expressao(codigo_0_0, compilador.escopo);
                Expressao express_0_1 = new Expressao(codigo_0_1, compilador.escopo);


                object result_0_0 = eval.EvalPosOrdem(express_0_0, compilador.escopo);

                object result_0_9_1 = eval.EvalPosOrdem(exprss_0_9_1, compilador.escopo);
                object result_0_8 = eval.EvalPosOrdem(exprss_0_8, compilador.escopo);
                object result_0_7 = eval.EvalPosOrdem(express_0_7, compilador.escopo);
                object result_0_6 = eval.EvalPosOrdem(express_0_6, compilador.escopo);
                object result_0_5 = eval.EvalPosOrdem(express_0_5, compilador.escopo);
                object result_0_3 = eval.EvalPosOrdem(express_0_3, compilador.escopo);
                object result_0_2 = eval.EvalPosOrdem(express_0_2, compilador.escopo);
                object result_0_1 = eval.EvalPosOrdem(express_0_1, compilador.escopo);


                try
                {
                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("x", compilador.escopo).valor.ToString() == "0", codigo_0_9_1);
                    assercao.IsTrue(result_0_9.ToString().Contains("Verde"));
                    assercao.IsTrue(result_0_8.ToString() == "marte", codigo_0_8);
                    assercao.IsTrue(result_0_7.ToString() == "TERRA", codigo_0_7);
                    assercao.IsTrue(result_0_6.ToString().Contains("Vermelha"), codigo_0_6);
                    assercao.IsTrue(result_0_5.ToString() == "0", codigo_0_5);
                    assercao.IsTrue(result_0_3.ToString() == "False", codigo_0_3);
                    assercao.IsTrue(result_0_2.ToString() == "True", codigo_0_2);
                    assercao.IsTrue(result_0_1.ToString() == "True", codigo_0_1);
                    assercao.IsTrue(result_0_0.ToString() == "5", codigo_0_0);

                }
                catch (Exception e)
                {
                    string codigoError = e.Message;
                    assercao.IsTrue(false, "FALHA NO TESTE");
                }


            }

            public void TestsVectorAtribution(AssercaoSuiteClasse assercao)
            {
                EvalExpression eval = new EvalExpression();

                string code_create = "int[] v1[15]; int[] v2[15];";
                string code_init_0_0 = "v1[0]=5";
                string code_init_0_1 = "v2[0]=1";
                string code_0_0 = "v1=v2";

                ProcessadorDeID compilador = new ProcessadorDeID(code_create);
                compilador.Compilar();

                Expressao exprss_init_0_0 = new Expressao(code_init_0_0, compilador.escopo);
                Expressao exprss_init_0_1 = new Expressao(code_init_0_1, compilador.escopo);

                object result_0_0 = eval.Eval(exprss_init_0_0, compilador.escopo);
                object result_0_1 = eval.Eval(exprss_init_0_1, compilador.escopo);


                Expressao exprss_0_0 = new Expressao(code_0_0, compilador.escopo);

                try
                {
                    object result = eval.EvalPosOrdem(exprss_0_0, compilador.escopo);
                    Vector vtResult = (Vector)((Objeto)result).valor;
                    assercao.IsTrue(vtResult.GetElement(0).ToString() == "1", code_0_0);
                    
                }
                catch (Exception e)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + e.Message);
                }

            }


            public void TesteAvaliacaoNumeros(AssercaoSuiteClasse assercao)
            {

                string codigoCreate = "int a=1; int b=5; int c=3;";
                string codigo_0_0 = "c= a + b;";
                string codigo_0_1 = "a=2*b;";
                string codigo_0_2 = "b=b-c*a";


                ProcessadorDeID compilador = new ProcessadorDeID(codigoCreate);
                compilador.Compilar();

                Expressao exprss_0_1 = new Expressao(codigo_0_1, compilador.escopo);
                Expressao exprss_0_2 = new Expressao(codigo_0_2, compilador.escopo);
                Expressao exprss_0_0 = new Expressao(codigo_0_0, compilador.escopo);


             


                try
                {


                    EvalExpression eval = new EvalExpression();
                    object valor_0_0 = eval.EvalPosOrdem(exprss_0_0, compilador.escopo);
                    SetResult(valor_0_0, "c", compilador.escopo);       
                    object valor_0_1 = eval.EvalPosOrdem(exprss_0_1, compilador.escopo);
                    SetResult(valor_0_1, "a", compilador.escopo);
                    object valor_0_2 = eval.EvalPosOrdem(exprss_0_2, compilador.escopo);
                    SetResult(valor_0_2, "b", compilador.escopo);

                    assercao.IsTrue(((Objeto)valor_0_0).valor.ToString() == "6");
                    assercao.IsTrue(((Objeto)valor_0_1).valor.ToString() == "10");
                    assercao.IsTrue((((Objeto)valor_0_2).valor.ToString() == "-55"));


                }
                catch (Exception ex)
                {
                    string codeError = ex.Message;
                    assercao.IsTrue(false, "TESTE FALHOU");
                }

            }


            private void SetResult(object result, string nomeObjeto, Escopo escopo)
            {
                escopo.tabela.GetObjeto(nomeObjeto,escopo).valor=((Objeto)result).valor;
            }


            public void TesteAvaliacaoChamadaDeMetodo(AssercaoSuiteClasse assercao)
            {

                string codigoMetodo_0_abs = "double.abs(x);";
                string codigoMetodo_1_abs = "x.abs();";
                string codigoMetodo_2_abs = "double.abs(-1.0);";

                string codigoCreate = "double x=1.0;";


                string codigo_minus_1 = "x=-1.0";
                string codigo_sqrt_0 = "double.root2(9.0)";
                string codigo_sqrt_1 = "double.root2(2.0)";

                EvalExpression eval = new EvalExpression();

                ProcessadorDeID compilador = new ProcessadorDeID(codigoCreate);
                compilador.Compilar();

                Expressao exprss_1_abs = new Expressao(codigoMetodo_1_abs, compilador.escopo);
                object result_0_1_abs = eval.EvalPosOrdem(exprss_1_abs, compilador.escopo);


                Expressao exprss_0_abs = new Expressao(codigoMetodo_0_abs, compilador.escopo);
                Expressao exprss_0_1_sqrt_0 = new Expressao(codigo_sqrt_0, compilador.escopo);
                Expressao exprss_0_2_sqrt_1 = new Expressao(codigo_sqrt_1, compilador.escopo);
                Expressao exprss_2_abs = new Expressao(codigoMetodo_2_abs, compilador.escopo);
                Expressao exprss_0_minus_1 = new Expressao(codigo_minus_1, compilador.escopo);










                object result_0_0_abs = eval.EvalPosOrdem(exprss_0_abs, compilador.escopo);
                object result_0_sqrt_1 = eval.EvalPosOrdem(exprss_0_2_sqrt_1, compilador.escopo);
                object result_0_sqrt = eval.EvalPosOrdem(exprss_0_1_sqrt_0, compilador.escopo);
                object result_0_2_abs = eval.EvalPosOrdem(exprss_2_abs, compilador.escopo);
                

                object result_0_minus1 = eval.EvalPosOrdem(exprss_0_minus_1, compilador.escopo);



                try
                {
                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("x", compilador.escopo).valor.ToString() == "-1", codigo_minus_1);

                    assercao.IsTrue(result_0_sqrt.ToString() == "3", codigo_sqrt_0);
                    assercao.IsTrue(result_0_sqrt_1.ToString().Contains("1.41"), codigo_sqrt_1);



                    assercao.IsTrue(result_0_2_abs.ToString() == "1", codigoMetodo_2_abs);
                    assercao.IsTrue(result_0_1_abs.ToString() == "1", codigoMetodo_1_abs);
                    assercao.IsTrue(result_0_0_abs.ToString() == "1", codigoMetodo_0_abs);
                }
                catch (Exception e)
                {
                    assercao.IsTrue(false, "falha no teste: " + e.Message);


                }
            }




            public void TestsSplitFunction(AssercaoSuiteClasse assercao)
            {
                
                string codigoCreate = "string[] separadores_1[5];";
                string codeText = "string text=" + aspas + "A Terra eh verde da Amazonia" + aspas + ";";
                string codigoCreateElement = "separadores_1[0]=" + aspas + " " + aspas;
                string codigo_0_0 = "string.CuttWords(text, separadores_1);";


                EvalExpression eval = new EvalExpression();

                ProcessadorDeID compilador = new ProcessadorDeID(codeText + codigoCreate);
                compilador.Compilar();

                try
                {
                    Expressao exprssVectorSetElement = new Expressao(codigoCreateElement, compilador.escopo);
                    object result_setElement = eval.EvalPosOrdem(exprssVectorSetElement, compilador.escopo);



                    Expressao exprss_0_0 = new Expressao(codigo_0_0, compilador.escopo);
                    object result__1 = eval.EvalPosOrdem(exprss_0_0, compilador.escopo);


                    // vector criado no compilador;
                    Vector vtResult = (Vector)result__1;

                    assercao.IsTrue(vtResult.GetElement(0).ToString() == "A");

                }
                catch (Exception e)
                {
                    assercao.IsTrue(false, "Teste Falhou: " + e.Message);

                }


            }

            public void TesteParametrosMultiArgumentos(AssercaoSuiteClasse assercao)
            {
                

                string codigoClasse = "public class classeA { public int propriedadeA = 1;  public classeA(){ int x=1; } ;public int metodoB(double x, ! int[] y){ return 5;} ;};";
                string codigoCreate = "classeA objA= create(); double x=1;";
                string codigoChamadaDeMetodo_pass = "objA.metodoB(x,1,1,1);";
                ProcessadorDeID compilador = new ProcessadorDeID(codigoClasse + codigoCreate);
                compilador.Compilar();


                Expressao exprssChamadaDeMetodo_pass = new Expressao(codigoChamadaDeMetodo_pass, compilador.escopo);



                // avalia as expressoes chamada de metodo.
                EvalExpression eval = new EvalExpression();
                object result_pass = eval.Eval(exprssChamadaDeMetodo_pass, compilador.escopo);

                try
                {
                    assercao.IsTrue(result_pass.ToString() == "5", codigoChamadaDeMetodo_pass);
                }
                catch (Exception e)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + e.Message + ":    " + codigoChamadaDeMetodo_pass);
                }

            }



  
            public void TesteAvaliacaoAtribuicaoPropriedadesAninhadas(AssercaoSuiteClasse assercao)
            {
        
                string code_classe_0_1 = "public class classeA { public int propriedade1; public classeA(){int y=3;}};";
                string code_create_obj = "classeA obj1= create(); obj1.propriedade1= create();";
                string code_expression_0_1 = "obj1.propriedade1= 5;";
                string code_expression_0_2 = "obj1.propriedade1= -1;";
                string code_expression_0_3 = "obj1.propriedade1= obj1.propriedade1+1;";

                ProcessadorDeID compilador = new ProcessadorDeID(code_classe_0_1 + code_create_obj);
                compilador.Compilar();

                Expressao exprssProp_01 = new Expressao(code_expression_0_1, compilador.escopo);
                Expressao exprssProp_02 = new Expressao(code_expression_0_2, compilador.escopo);
                Expressao exprssProp_03 = new Expressao(code_expression_0_3, compilador.escopo);

                EvalExpression eval = new EvalExpression();


                object result_0_1 = eval.EvalPosOrdem(exprssProp_01, compilador.escopo);
                try
                {
                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("obj1", compilador.escopo).GetField("propriedade1").valor.ToString() == "5");
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message + " " + code_expression_0_1);
                }






                object result_0_3 = eval.EvalPosOrdem(exprssProp_03, compilador.escopo);
                try
                {
                    Objeto objReturn = (Objeto)result_0_3;
                    assercao.IsTrue(objReturn.valor.ToString() == "6");
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message + "  " + code_expression_0_3);
                }



                object result_0_2 = eval.EvalPosOrdem(exprssProp_02, compilador.escopo);
                try
                {
                    assercao.IsTrue(compilador.escopo.tabela.GetObjeto("obj1", compilador.escopo).GetField("propriedade1").valor.ToString() == "-1", code_expression_0_2);
                }
                catch (Exception e)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + e.Message + ":   " + code_expression_0_2);
                }





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

  
  


      
 
 
  
        }


    } //class EvalExpression

} // namespace

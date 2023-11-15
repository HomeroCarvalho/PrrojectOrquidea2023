using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace parser
{
    public partial class Expressao
    {



        public Expressao ExtraiUmaExpressaoSemValidar(List<string> tokens, Escopo escopo)
        {
            /// Expressao faz a compilação de expressoes sem validar, como regra, não excessão.
            ExpressaoSearch exprss_search = new ExpressaoSearch();
            Expressao exprss_retorno = exprss_search.BuildExpressions(tokens.ToArray(), escopo);

            return exprss_retorno;


        }

    


        /// converte uma expressão para uma lista de tokens.
        public List<string> Convert()
        {
            List<string> tokens = ParserUniversal.GetTokens(this.ToString());
            return tokens;
        } // Convert()

        public bool IsEqualsExpressions(Expressao expressao1, Expressao expressao2)
        {

            if ((expressao1 == null) && (expressao2 == null))
                return true;
            if ((expressao1 != null) && (expressao2 == null))
                return false;
            if ((expressao1 == null) && (expressao2 != null))
                return false;
            if ((expressao1.Elementos == null) && (expressao2.Elementos == null))
                return true;
            if ((expressao1.Elementos.Count == 0) && (expressao2.Elementos.Count == 0))
                return true;
            if (expressao1.ToString() == expressao2.ToString())
                return true;

            if (expressao1.Elementos.Count != expressao2.Elementos.Count)
                return false;

            for (int x = 0; x < expressao1.Elementos.Count; x++)
                if (expressao1.Elementos[x].ToString() != expressao2.Elementos[x].ToString())
                    return false;
            return true;

        }

        public object ConverteParaNumero(string str_numero, Escopo escopo)
        {
            object obj_result = null;
            if (IsTipoInteiro(str_numero))
                obj_result = int.Parse(str_numero);
            else
            if (IsTipoFloat(str_numero))
                obj_result = float.Parse(str_numero);
            else
            if (IsTipoDouble(str_numero))
                obj_result = double.Parse(str_numero);

            return obj_result;
        }

        public bool IsTipoInteiro(string str_numero)
        {
            int numeroInt = 0;
            return int.TryParse(str_numero.Trim(' '), out numeroInt);
        }

        public bool IsTipoFloat(string str_numero)
        {
            float numeroFloat = 0.0f;
            return float.TryParse(str_numero.Trim(' '), out numeroFloat);
        }
        public bool IsTipoDouble(string str_numero)
        {
            double numeroDouble = 0.0;
            return double.TryParse(str_numero.Trim(' '), out numeroDouble);
        }

        public bool IsNumero(string str_numero)
        {
            return IsTipoInteiro(str_numero) || IsTipoFloat(str_numero) || (IsTipoDouble(str_numero));
        }

        /// valida a colocação lógica sequencial de variáveis e operadores, e funções
        public bool ValidaExpressoesGeral(List<string> tokenDaExpressao)
        {
            int sinalExpressao = 1;
            for (int x = 0; x < tokenDaExpressao.Count; x++)
            {
                // o token é um ID? mas está na sequencia certa?
                if ((tokenDaExpressao[x] == "ID") && (sinalExpressao == 1))
                {
                    sinalExpressao = 2;
                    continue;
                } // if
                else
                // o token é um ID? mas está na sequencia errada?
                if ((tokenDaExpressao[x] == "ID") && (sinalExpressao == 1))
                {
                    return false;
                } // else
                else
                // o token é operador BINARIO? mas a sequencia está certa?
                if ((tokenDaExpressao[x] == "OPERADOR_BINARIO") && (sinalExpressao == 2))
                {
                    sinalExpressao = 1;
                    continue;
                } // if
                else
                // o token é operador BINARIO? mas a sequencia está errada?
                if ((tokenDaExpressao[x] == "OPERADOR_BINARIO") && (sinalExpressao == 1))
                {
                    return false;
                } // else
                else
                // o token é operador UNARIO?
                if (tokenDaExpressao[x] == "OPERADOR_UNARIO")
                {


                    // sequencia: "OPERADOR_UNARIO" "ID" "OPERADOR_BINARIO". exemplo: "++ c +"
                    if ((x >= 0) && ((x + 2) < tokenDaExpressao.Count) &&
                    (tokenDaExpressao[x + 1] == "ID") &&
                    (tokenDaExpressao[x + 2] == "OPERADOR_BINARIO"))
                    {
                        sinalExpressao = 1;
                        continue;
                    } // if
                    else

                // sequencia: "OPERADOR_UNARIO" "OPERADOR_BINARIO". exemplo:  "c ++ +"
                if ((x - 1) >= 0 &&
                    (tokenDaExpressao[x - 1] == "ID") &&
                    (tokenDaExpressao[x + 1] == "OPERADOR_BINARIO"))
                    {
                        sinalExpressao = 2;
                        continue;
                    } // if
                    else
                        return false; // exemplo: "c ++ b"

                } // if "OPERADOR_UNARIO"

            } // for x
            return true;
        } // ValidaExpressoesGeral()

        internal List<string> ObtemExpressaoCondicionalResumida(Expressao exprss, Escopo escopo)
        {
            LinguagemOrquidea linguagem = LinguagemOrquidea.Instance();
            List<string> resumidoSubExpressao = new List<string>();
            List<string> resumidoExressaoPrincipal = new List<string>();
            List<string> tokensDaExpressao = exprss.Convert();


            int pilhaInteiroParenteses = 0;


            for (int x = 0; x < exprss.Elementos.Count; x++)
            {

                // é nome de função? (chamada de função).
                if (exprss.Elementos[x].GetType() == typeof(ExpressaoChamadaDeMetodo))
                {
                    List<string> tokensDaChamada = ObtemExpressaoCondicionalResumida(exprss.Elementos[x], escopo);
                    if ((tokensDaChamada == null) || (tokensDaChamada.Count == 0))
                        return null;
                    x += tokensDaChamada.Count;

                } // if

              


                if (exprss.Elementos[x].GetType() == typeof(ExpressaoObjeto))
                {
                    string nomeObjeto = ((Objeto)exprss.Elementos[x].GetElemento()).GetNome();

                    //é operador não condicional, mas um operador?
                    if ((linguagem.VerificaSeEhOperador(nomeObjeto) && (!linguagem.IsOperadorCondicional(nomeObjeto))))
                    {
                        if (resumidoSubExpressao.Find(k => k.Equals("ID")) == null) // verifica se a sub-expressão já tem um operando resumido ID.
                            resumidoSubExpressao.Add("ID"); // inicia a sub-expressão com um ID, resumindo variáveis, e operadores aritmeticos.
                    } // if

                    else
                    // é um ID não nome de função? 
                    if ((linguagem.IsID(nomeObjeto)) && (escopo.tabela.IsFunction(nomeObjeto, escopo) == null))
                    {
                        if (resumidoSubExpressao.Find(k => k.Equals("ID")) == null) // verifica se a sub-expressão já tem um operando resumido ID.
                            resumidoSubExpressao.Add("ID"); // inicia a sub-expressão com um ID, resumindo variáveis, e operadores aritmeticos.
                    }
                    else
                    // é operador condicional?
                    if (linguagem.IsOperadorCondicional(nomeObjeto))
                    {
                        resumidoSubExpressao.Add("CONDICIONAL"); // acrescenta um resumo condicional, e começa uma nova sub-expressão.
                        resumidoExressaoPrincipal.AddRange(resumidoSubExpressao);
                        resumidoSubExpressao.Clear();
                    } // if 
                    else
                    //é  parenteses abre?
                    if (nomeObjeto.Equals("("))
                    {
                        if (resumidoSubExpressao.Find(k => k.Equals("ID")) == null) // verifica se a sub-expressão já tem um operando resumido ID.
                            resumidoSubExpressao.Add("ID"); // inicia a sub-expressão com um ID, resumindo variáveis, e operadores aritmeticos.

                        pilhaInteiroParenteses++; // incrementa a pilha de parenteses, para verificar se a expressão tem um termo final (parenteses fecha).
                    } // if
                    else
                    // é parenteses fecha?
                    if (nomeObjeto.Equals(")"))
                    {
                        pilhaInteiroParenteses--;
                        if (pilhaInteiroParenteses == 0) // se a pilha de parenteses zerar, retorna a expressão resumida principal.
                        {
                            resumidoExressaoPrincipal.AddRange(resumidoSubExpressao);  // descarrega a lista da sub-expressão, que não foi acrescentada ainda.
                            return resumidoExressaoPrincipal;
                        }
                    } // if
                } // for x
            } //if Getype()==ItemExpressao
            resumidoExressaoPrincipal.AddRange(resumidoSubExpressao);
            return resumidoExressaoPrincipal;
        } // ObtemExpressaoCondicionalResumida()


        public bool ValidaExpressaoCondicional(Expressao expressao, Escopo escopo)
        {
            int contadorOperadoresCondicional = 0;
            if ((expressao != null) && (expressao.Elementos != null) && (expressao.Elementos.Count > 0)) 
            {
                for (int x = 0; x < expressao.Elementos.Count; x++)
                {
                    if (expressao.Elementos[x].GetType()==typeof(ExpressaoOperador))
                    {
                        ExpressaoOperador exprssOperador = ((ExpressaoOperador)expressao.Elementos[x]);
                        if (exprssOperador.operador != null)
                        {
                            string nomeOperador = exprssOperador.operador.nome;
                            switch (nomeOperador)
                            {
                                case (">"):
                                case ("<"):
                                case (">="):
                                case ("<="):
                                case ("=="):
                                case ("!"):
                                    contadorOperadoresCondicional++;
                                    break;

                            }
                        }
                    }
                    
                    
                }
            }
            return contadorOperadoresCondicional == 1;
 
        } 


        /// <summary>
        /// obtém uma lista resumida, com operadores, nomes de variaveis, nomes de funções.
        /// </summary>
        internal List<string> ObtemExpressaoGeralResumida(Expressao exprss, Escopo escopo)
        {

            List<string> resumida = new List<string>();
            LinguagemOrquidea linguagem = LinguagemOrquidea.Instance();
            for (int x = 0; x < exprss.Elementos.Count; x++)
            {
                //a expessao é uma funcao?
                if (exprss.Elementos[x].GetType() == typeof(ExpressaoChamadaDeMetodo))
                {
                    List<string> chamadaFuncao = ObtemExpressaoGeralResumida(exprss.Elementos[x], escopo);
                    if ((chamadaFuncao == null) || (chamadaFuncao.Count == 0))
                    {
                        escopo.GetMsgErros().Add("expressao condicional; " + exprss + " invalida!");
                    }


                }  // if
                else
                // a expressão é uma variável singular?
                if (exprss.Elementos[x].GetType() == typeof(ExpressaoObjeto))
                {
                    string nomeObjeto = ((ExpressaoObjeto)exprss.Elementos[x]).nomeObjeto;
                    if (linguagem.IsNumero(nomeObjeto))
                        resumida.Add("ID");
                    else
                    {

                        Objeto v = escopo.tabela.GetObjeto(nomeObjeto, escopo);
                        string tipoItem = v.GetTipo();
                        if (tipoItem != null)
                        {
                            resumida.Add("ID");
                        } // if

                        if (exprss.Elementos[x].GetType() == typeof(ExpressaoOperador))
                        {
                            string operadorItem = ((ExpressaoOperador)exprss.Elementos[x]).nomeOperador;

                            int indexOperadorBinario = linguagem.GetOperadoresBinarios().FindIndex(k => k.nome == operadorItem);
                            if (indexOperadorBinario != -1)
                                resumida.Add("OPERADOR_BINARIO");
                            else
                            {
                                int indexOperadorUnario = linguagem.GetOperadoresUnarios().FindIndex(k => k.nome == operadorItem);
                                if (indexOperadorUnario != -1)
                                    resumida.Add("OPERADOR_UNARIO");
                            } // else

                        } //else
                    } // else
                } // if exprss.subExpressaoes[x]
            } // for x
            return resumida;
        } // ObtemExpressaoGeralResumida()


        public bool IsExpressaoAritimeticoUnario(Expressao exprss, Escopo escopo)
        {
            int hasIntVariable = 0;
            int hasOperatorUnary = 0;
            for (int x = 0; x < exprss.Elementos.Count; x++)
            {
                if (exprss.Elementos[x] is ExpressaoObjeto)
                {
                    ExpressaoObjeto exprssaoOperando = (ExpressaoObjeto)exprss.Elementos[x];
                    if (exprssaoOperando.tipoDaExpressao == "int")
                    {
                        hasIntVariable++;

                        if (hasIntVariable > 1)
                            return false;

                    }

                }


                if (exprss.Elementos[x].GetType() == typeof(ExpressaoOperador))
                {
                    ExpressaoOperador exprssOperador = ((ExpressaoOperador)exprss.Elementos[x]);
                    Operador op = exprssOperador.operador;

                    if ((op.tipoRetorno == "int") && (op.tipo == "UNARIO")) 
                    {
                        hasOperatorUnary++;
                        if (hasOperatorUnary > 1)
                            return false;
                    }
                }
            }
            return (hasIntVariable == 1 && hasOperatorUnary == 1);
        }



        public bool IsExpressionAtibuicao(Expressao exprss)
        {

            List<string> expressaoComOperadorAtribuicao = exprss.tokens.FindAll(k => k.Equals("="));
            return expressaoComOperadorAtribuicao.Count == 1;
        }


    }
}

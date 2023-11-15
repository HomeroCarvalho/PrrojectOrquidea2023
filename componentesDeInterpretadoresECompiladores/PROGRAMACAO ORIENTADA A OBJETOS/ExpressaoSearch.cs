using MathNet.Numerics.Optimization;
using parser.ProgramacaoOrentadaAObjetos;
using parser.textoFormatado;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;

using System.Text;
using System.Threading.Tasks;
using Util;

namespace parser
{
    public class ExpressaoSearch
    {


      

        public Expressao BuildExpressions(string[] tokenDexpressao, Escopo escopo)
        {
            if ((tokenDexpressao == null) || (tokenDexpressao.Length == 0))
                return null;
            else
            {
                string str_exprss = "";
                foreach (string umToken in tokenDexpressao)
                    str_exprss += umToken + " ";

                return BuildExpressions(str_exprss.Trim(' '), escopo);
            }
            
        }


        public Expressao BuildExpressions(string str_expressao, Escopo escopo)
        {


            // constroi os headers, se não foi construido ainda.
            if (Expressao.headers == null)
            {
                Expressao.InitHeaders("");
            }
                

            // constroi o search de regex, e input o codigo da expressao.
            SearchByRegexExpression search = new SearchByRegexExpression(str_expressao);
            // faz o processamento do input.
           search.ProcessingPattern();

        

            // erro se não conseguir identicar o tipo de expressao.
            if (search==null)
            {
                UtilTokens.WriteAErrorMensage("expression type not found, view the code expression: " + str_expressao + ".", search.tokens, escopo);
                return null;
            }

            Expressao exprssRetorno= FactoryExpressao(search, escopo);
            if ((exprssRetorno != null) && (exprssRetorno.tokens != null) && (exprssRetorno.tokens.Count != search.tokens.Count)) 
            {
                // fazer aqui o processamento de mais expressoes que vieram como grupo de expressoes.
            }

            return exprssRetorno;
           
        }



        public Expressao FactoryExpressao(SearchByRegexExpression search, Escopo escopo)
        {

            
          
            Type tipoDaExpressao = search.TipoExpressao;
            if (tipoDaExpressao == typeof(ExpressaoAtribuicao))
            {
                ExpressaoAtribuicao exprss1 = this.Atribuicao(search, escopo);
                if (exprss1 != null)
                {
                    exprss1.tokens = search.tokens.ToList<string>();
                }
                return exprss1;
            }
            else
            if (tipoDaExpressao == typeof(ExpressaoInstanciacao))
            {
                ExpressaoInstanciacao exprss1 = this.Instanciacao(search, escopo);
                if (exprss1 != null)
                {
                    exprss1.tokens = search.tokens.ToList<string>();
                }

                return exprss1;
            }
            else
            if (tipoDaExpressao == typeof(ExpressaoChamadaDeMetodo))
            {
                ExpressaoChamadaDeMetodo exprss1 = this.ChamadaDeMetodo(search, escopo);
                if (exprss1 != null)
                {
                    exprss1.tokens = search.tokens.ToList<string>();
                }
                return exprss1;
            }
            else
            if (tipoDaExpressao == typeof(ExpressaoPropriedadesAninhadas))
            {
                ExpressaoPropriedadesAninhadas exprss1 = this.PropriedadesAninhadas(search, escopo);
                if (exprss1 != null)
                {
                    exprss1.tokens = search.tokens.ToList<string>();
                }
                return exprss1;
            }

            else
            if (tipoDaExpressao == typeof(ExpressaoObjeto))
            {
                ExpressaoObjeto exprss1 = this.ExpressaoObjeto(search, escopo);
                if (exprss1 != null)
                {
                    return exprss1;
                }
                else
                {
                    ExpressaoNumero exprss2 = this.ExpressaoNumero(search, escopo);
                    if (exprss2 != null)
                    {
                        return exprss2;
                    }
                    else
                        if (Expressao.headers.cabecalhoDeClasses.FindIndex(k => k.nomeClasse == search.input) != -1)
                    {
                        ExpressaoElemento exprss3 = new ExpressaoElemento(search.input);
                        return exprss3;
                    }
                }
                
            }


            else
            if (tipoDaExpressao == typeof(ExpressaoEntreParenteses))
            {
                ExpressaoEntreParenteses exprss1 = this.EntreParenteses(search, escopo);
                if (exprss1 != null)
                {
                    exprss1.tokens = search.tokens.ToList<string>();
                }
                return exprss1;
            }
            else
            if (tipoDaExpressao == typeof(ExpressaoLiteralText))
            {
                ExpressaoLiteralText exprss1 = this.Literal(search, escopo);
                if (exprss1 != null)
                {
                    exprss1.tokens = search.tokens.ToList<string>();
                }
                return exprss1;
            }
            else
            if (tipoDaExpressao == typeof(ExpressaoNumero))
            {
                ExpressaoNumero exprss1 = this.ExpressaoNumero(search, escopo);
                if (exprss1 != null)
                {
                    exprss1.tokens = search.tokens.ToList<string>();
                }
                return exprss1;
            }

            else
            if (tipoDaExpressao == typeof(ExpressaoNILL))
            {
                ExpressaoNILL exprss1 = this.ExpressaoNill(search, escopo);
                if (exprss1 != null)
                {
                    exprss1.tokens = search.tokens.ToList<string>();
                }
                return exprss1;
            }

            else
            if (tipoDaExpressao == typeof(ExpressaoOperador))
            {
                ExpressaoOperador exprss1 = this.Operadores(search, escopo);
                if (exprss1 != null)
                {
                    exprss1.tokens = search.tokens.ToList<string>();
                }
                return exprss1;
            }
            else
            {
                ExpressaoLiteralText exprss5 = new ExpressaoLiteralText(search.input);
                return exprss5;
            }

            return null;
        }

        public ExpressaoOperador Operadores(SearchByRegexExpression search, Escopo escopo)
        {
            if ((search==null) || (search.tokens.Count==0))
            {
                UtilTokens.WriteAErrorMensage("erro de sintaxe para expressao operador", search.tokens, escopo);
                return null;
            }


            ExpressaoOperador exprssOperadores = new ExpressaoOperador(null);




            // sem operadores ou operandos.
            if ((search.subExpressoesSearch == null) || (search.subExpressoesSearch.Count == 0))
            {
                UtilTokens.WriteAErrorMensage("cannot processing operator expression, lack of operands", search.tokens, escopo);
                return null;

            }


            int contadorOperandosEOperadores = 0;

            // obtem o tipo do 1o. operando, e VALIDA.
            Expressao exprssOperando1 = FactoryExpressao(search.subExpressoesSearch[contadorOperandosEOperadores++], escopo);
            if (exprssOperando1 == null)
            {
                UtilTokens.WriteAErrorMensage("bad format in expression operators, 1o. operand is null", search.tokens, escopo);
                return null;
            }

            // instancia o 2o.operando.
            Expressao exprssOperando2 = null;


            
            


          

          
          
            int contador_operadores = 0;


       
            // enquanto há operadores não "contabilizado", obtem 2 operandos e um operador.
            while (contador_operadores < search.operators.Count)
            {
                if (contadorOperandosEOperadores > search.subExpressoesSearch.Count) 
                {
                    return exprssOperadores;
                }
                
                // faz O PROCESSO de encontrar o 2o. operando.
                bool isNOTHasSecondOperand = false;
                if (contadorOperandosEOperadores>=search.subExpressoesSearch.Count)
                {
                    isNOTHasSecondOperand=true;
                    exprssOperando2 = exprssOperando1;
                }

                if (!isNOTHasSecondOperand)
                {
                    /// obtem a expressão do 2o. operando.
                    exprssOperando2 = FactoryExpressao(search.subExpressoesSearch[contadorOperandosEOperadores++], escopo);

                    // VALIDA o 2o. operando.
                    if (exprssOperando2 == null)
                    {
                        UtilTokens.WriteAErrorMensage("cannot to found a 2a. operand in operador: ", search.tokens, escopo);
                        return null;
                    }
                }


                

                bool isOperatorBinary_and_Unary = false;
                string nomeOperadorCurrente = search.operators[contador_operadores].text;
      
                Operador operadorCompativel = UtilTokens.FindOperatorCompatible(nomeOperadorCurrente, exprssOperando1.tipoDaExpressao, exprssOperando2.tipoDaExpressao, ref isOperatorBinary_and_Unary);



                // OPERADOR É BINARIO E UNARIO AO MESMO TEMPO, SOBRECARGA.
                if ((operadorCompativel!=null) && (isOperatorBinary_and_Unary))
                {
                    if ((exprssOperando1 == null) || ((exprssOperando1!=null) && (exprssOperando1.GetType()==typeof(ExpressaoEntreParenteses))))
                    {
                        // operador funcionano como unario
                        operadorCompativel = UtilTokens.FindOperatorBinarioUnarioMasComoUNARIO(nomeOperadorCurrente, exprssOperando1.tipoDaExpressao, exprssOperando2.tipoDaExpressao);


                        // encontrou um operador binario, guarda o tipo de dados do operador, o metodo do operador, e os 2 operandos.
                        ExpressaoOperador exprssOp = new ExpressaoOperador(operadorCompativel);
                        exprssOperadores.tipoDaExpressao = operadorCompativel.tipoRetorno;
                        exprssOperadores.operador = operadorCompativel;


                        if (contador_operadores == 0)
                        {
                            exprssOperadores.Elementos.Add(exprssOperando1);
                            exprssOperadores.Elementos.Add(exprssOp);
                            exprssOperadores.Elementos.Add(exprssOperando2);

                            exprssOperadores.operando1 = exprssOperando1;
                            exprssOperadores.operando2 = exprssOperando2;

                        }
                        else
                        {
                            exprssOperadores.operando1 = exprssOperando2;
                            exprssOperadores.Elementos.Add(exprssOp);
                            exprssOperadores.Elementos.Add(exprssOperando2);
                        }

                        exprssOperadores.tokens = search.tokens;


                    }
                    else
                    {
                        // operador funcionando como binario.
                        operadorCompativel = UtilTokens.FindOperatorBinarioEUnarioMasComoBINARIO(exprssOperando1.tipoDaExpressao, nomeOperadorCurrente, exprssOperando1.tipoDaExpressao, exprssOperando2.tipoDaExpressao);

                        ExpressaoOperador exprssOp = new ExpressaoOperador(operadorCompativel);
                        exprssOperadores.tipoDaExpressao = operadorCompativel.tipoRetorno;
                        exprssOperadores.operador = operadorCompativel;


                        if (contador_operadores == 0)
                        {
                            exprssOperadores.Elementos.Add(exprssOperando1);
                            exprssOperadores.Elementos.Add(exprssOp);
                            exprssOperadores.Elementos.Add(exprssOperando2);

                            exprssOperadores.operando1 = exprssOperando1;
                            exprssOperadores.operando2 = exprssOperando2;

                        }
                        else
                        {
                            exprssOperadores.operando1 = exprssOperando2;
                            exprssOperadores.Elementos.Add(exprssOp);
                            exprssOperadores.Elementos.Add(exprssOperando2);
                        }

                        exprssOperadores.tokens = search.tokens;
                        exprssOperadores.tipoDaExpressao = operadorCompativel.tipoRetorno;


                    }
                }

                // OPERADOR EXCLUSIVAMENTE BINARIO.
                if ((operadorCompativel != null) && (operadorCompativel.tipo == "BINARIO") && (!isOperatorBinary_and_Unary))
                {

                    // encontrou um operador binario, guarda o tipo de dados do operador, o metodo do operador, e os 2 operandos.
                    ExpressaoOperador exprssOp = new ExpressaoOperador(operadorCompativel);
                    exprssOperadores.tipoDaExpressao = operadorCompativel.tipoRetorno;
                    exprssOperadores.operador = operadorCompativel;


                    if (contador_operadores == 0)
                    {
                        exprssOperadores.Elementos.Add(exprssOperando1);
                        exprssOperadores.Elementos.Add(exprssOp);
                        exprssOperadores.Elementos.Add(exprssOperando2);

                        exprssOperadores.operando1 = exprssOperando1;
                        exprssOperadores.operando2 = exprssOperando2;

                    }
                    else
                    {
                        exprssOperadores.operando1 = exprssOperando2;
                        exprssOperadores.Elementos.Add(exprssOp);
                        exprssOperadores.Elementos.Add(exprssOperando2);
                    }

                    exprssOperadores.tokens = search.tokens;


                }
                else
                // OPERADOR EXCLUSIVO UNARIO.
                if ((operadorCompativel != null) && (operadorCompativel.tipo.Contains("UNARIO"))&& (!isOperatorBinary_and_Unary))
                {
                    // encontrou um operado unaio, guarda o tipo de dados, o operador e 1 operando.
                    ExpressaoOperador exprssOpUnario = new ExpressaoOperador(operadorCompativel);
                    exprssOpUnario.operador = operadorCompativel;
                    exprssOpUnario.operando1 = exprssOperando1;
                    exprssOpUnario.tokens = search.tokens;

                    // adiciona o operando e o operador unario.
                    exprssOperadores.Elementos.Add(exprssOperando1);
                    exprssOperadores.Elementos.Add(exprssOpUnario);

                    List<string> tokensSearch = search.tokens;

                    string str_operando = SearchByRegexExpression.FormataEntrada(Utils.OneLineTokens(exprssOperando1.tokens));
                    string str_expressaoInteiro = SearchByRegexExpression.FormataEntrada(Utils.OneLineTokens(search.tokens));

                    int indexOperador = 0;
                    indexOperador = str_expressaoInteiro.IndexOf(exprssOpUnario.nomeOperador, contadorOperandosEOperadores - 1);
                    int indexOperando = str_expressaoInteiro.IndexOf(str_operando, indexOperador + 1);

                    if (indexOperador < indexOperando)
                    {
                        operadorCompativel.tipo = "UNARIO PRE";
                    }
                    else
                    if (indexOperador > indexOperando)
                    {
                        operadorCompativel.tipo = "UNARIO POS";
                    }
                    else
                    {
                        operadorCompativel.tipo = "UNARIO_POS";
                    }


                    exprssOperadores.tipoDaExpressao = operadorCompativel.tipoRetorno;

                }
                else
                if (operadorCompativel == null)
                {
                    UtilTokens.WriteAErrorMensage("compatible operator: " + search.operators[contador_operadores].text + " not found ", search.tokens, escopo);
                    return null;
                }



                // passa para o proximo operador.
                contador_operadores++;

            } // while contador_operadores

            return exprssOperadores;

        }


        public ExpressaoAtribuicao Atribuicao(SearchByRegexExpression search, Escopo escopo)
        {
            if ((search.tokens==null) || (search.tokens.Count<3))
            {
                UtilTokens.WriteAErrorMensage("sintaxe error in instantiate expression", search.tokens, escopo);
                return null;
            }
            else
            {

                string nomeObjeto = search.tokens[0];
       
                Objeto objInstanciado = escopo.tabela.GetObjeto(nomeObjeto, escopo);
                if (objInstanciado == null)
                {
                    return null;
                }
                // expressao do valor a atribuir, em tempo de execução.
                Expressao exprssInstantiate = null;



                // constroi a expressao de calculo do valor do objeto, em tempo de execução.
                int indexBeginTokensExprssInstanticao = search.tokens.IndexOf("=");
                if (indexBeginTokensExprssInstanticao != -1)
                {
                    List<string> tokensExpressaoInstanciacao = search.tokens.GetRange(indexBeginTokensExprssInstanticao + 1, search.tokens.Count - (indexBeginTokensExprssInstanticao + 1));
                    exprssInstantiate = new Expressao(tokensExpressaoInstanciacao.ToArray(), escopo);
                }


                // constroi a expressao de atibuicao de retorno.
                ExpressaoAtribuicao exprssAtribuir = new ExpressaoAtribuicao(new ExpressaoObjeto(objInstanciado), exprssInstantiate, escopo);

                return exprssAtribuir;
            }

        }




        public ExpressaoInstanciacao Instanciacao(SearchByRegexExpression search, Escopo escopo)
        {
            if (((search == null) || (search.ids == null) || (search.ids.Count == 0)) && ((search.exprss == null) || (search.exprss.Count == 0)))
            {
                UtilTokens.WriteAErrorMensage("sintaxe error in instantiate expression", search.tokens, escopo);
                return null;
            }
            else
            {
                string nomeObjeto = search.ids[1].input;
                string nomeClasse = search.ids[0].input;
                object valorObjeto = null;
                if (search.ids.Count > 2)
                {
                    valorObjeto = search.ids[2].input;
                }

                Objeto objetoAInstanciar = null;
                if (escopo.tabela.GetObjeto(nomeObjeto, escopo)==null)
                {
                    // instancia o objeto da expressao.
                    objetoAInstanciar = new Objeto("private", nomeClasse, nomeObjeto, valorObjeto);
                    escopo.tabela.GetObjetos().Add(objetoAInstanciar);

                }
                else
                {
                    objetoAInstanciar = escopo.tabela.GetObjeto(nomeObjeto, escopo);
                }



                ExpressaoInstanciacao exprssRetorno = null;
                Expressao exprssCalculoValor = null;

                int indexBeginTokensExprssInstanticao = search.tokens.IndexOf("=");
                if (indexBeginTokensExprssInstanticao != -1)
                {
                    List<string> tokensExpressaoInstanciacao = search.tokens.GetRange(indexBeginTokensExprssInstanticao + 1, search.tokens.Count - (indexBeginTokensExprssInstanticao + 1));
                    exprssCalculoValor = new Expressao(tokensExpressaoInstanciacao.ToArray(), escopo);
                }

                exprssRetorno = new ExpressaoInstanciacao(nomeObjeto, nomeClasse, exprssCalculoValor, escopo);
                return exprssRetorno;
            }

        }


        

        public ExpressaoEntreParenteses EntreParenteses(SearchByRegexExpression search, Escopo escopo)
        {
            if ((search == null) || (search.exprss==null) || (search.exprss.Count==0))
            {
                UtilTokens.WriteAErrorMensage("erro de sintaxe de expressao entre parenteses", search.tokens, escopo);
                return null;
            }
            else
            {
                string str_exprss = search.exprss[0].text;
                Expressao exprssExpressaoEntreOsParenteses = new Expressao(str_exprss, escopo);
                if ((exprssExpressaoEntreOsParenteses == null) || (exprssExpressaoEntreOsParenteses.Elementos.Count > 0))
                {
                    ExpressaoEntreParenteses exprssParenteses = new ExpressaoEntreParenteses(exprssExpressaoEntreOsParenteses.Elementos[0], escopo);
                    return exprssParenteses;

                }
                else
                {
                    UtilTokens.WriteAErrorMensage("bad format to ExpressaoEntreParenteses, in ExpressaoSearch.EntreParenteses.", search.tokens, escopo);
                    return null;
                }

            }
        }



        public ExpressaoLiteralText Literal(SearchByRegexExpression search, Escopo escopo)
        {
            // verifica o processamento do search, para captação da string constante.
            if ((search.ids == null) || (search.ids.Count == 0))
            {
                UtilTokens.WriteAErrorMensage("expressao de literal com erro de sintaxe", search.tokens, escopo);
                return null;
            }
            else
            {
                // obtem a string constante (literal), do search.
                string literal = search.ids[0].input;

                // constroi a expressao literal.
                ExpressaoLiteralText exprssRetorno = new ExpressaoLiteralText(literal);
   
                return exprssRetorno;

            }
        }

        public ExpressaoPropriedadesAninhadas PropriedadesAninhadas(SearchByRegexExpression search, Escopo escopo)
        {
            ExpressaoPropriedadesAninhadas exprssRetorno = new ExpressaoPropriedadesAninhadas();

            if (search.TipoExpressao == typeof(ExpressaoPropriedadesAninhadas))
            {
                string str_nameObject = search.ids[0].input;


                Objeto obj_caller = escopo.tabela.GetObjeto(str_nameObject, escopo);
                // valida o objeto que chamou a propriedade aninhada.
                if (obj_caller == null)
                {
                    UtilTokens.WriteAErrorMensage("object not found, in current scopo", search.tokens, escopo);
                    return null;
                }


               



                if (exprssRetorno.aninhamento == null)
                    exprssRetorno.aninhamento = new List<Objeto>();
                
                // obtem os tokens da expressao.
                exprssRetorno.tokens = search.tokens.ToList<string>();



                int x = 0;
                if (obj_caller != null)
                {
                    // obtem o nome da classe da propriedade aninhada.
                    string str_classObject = obj_caller.GetTipo();
                    if (obj_caller.GetField(search.ids[1].input) != null)
                    {
                        exprssRetorno.aninhamento.Add(obj_caller);
                        exprssRetorno.nomeObjeto = obj_caller.GetNome();
                        exprssRetorno.classeObjeto = obj_caller.GetTipo();
                        exprssRetorno.propriedade = search.ids[1].input;

                        // passa para a proxima entrada de expressoes propriedades aninhadas,
                        // a 1a. propriedade foi registrada, passa então para as próximas propriedades
                        // e chamadas de metodo, se tiver...

                      
                        while (x < search.subExpressoesSearch.Count)
                        {
                            // valida se a sub-search currente contém uma "ExpressaoProppriedadesAninhadas".
                            if (search.subExpressoesSearch[x].TipoExpressao == typeof(ExpressaoPropriedadesAninhadas))
                            {
                                

                                if (search.subExpressoesSearch[x].ids.Count >= 2)
                                {
                                    // pega a propriedade currente, para validação, e formação da ExpressaoPropriedadeAninhada.
                                    string str_proprerty = search.subExpressoesSearch[x].ids[0].input;

                                    // obtem um objeto que representa a próxima propriedade aninhada.
                                    Objeto obj_property = obj_caller.GetField(str_proprerty);
                           

                                    if (obj_property != null)
                                    {

                                        List<Objeto> aninhamento= new List<Objeto>();
                                        List<string> propriedadesAninhadas = new List<string>();

                                        aninhamento.Add(obj_property);
                                        propriedadesAninhadas.Add(str_proprerty);
                                        
                                        // seta o objeto caller para o objeto da propriedade, a fim de continuar o processamento de outras
                                        // propriedades aninhadas ou chamadas de metodo aninhadas.
                                        obj_caller = obj_property;

                                        // forma a expressao aninhada.
                                        ExpressaoPropriedadesAninhadas eprss = new ExpressaoPropriedadesAninhadas(aninhamento, propriedadesAninhadas);
                                        exprssRetorno.Elementos.Add(eprss);


                                    }
                                }
                            }
                            
                            x++;
                        } // while x

                        return exprssRetorno;
                    } // if obj_caller.GetField()
                    
                } // if obj_caller<>null.
                else
                {
                    UtilTokens.WriteAErrorMensage("objetct to prorerty aninhada not found", search.tokens, escopo);
                    return null;
                }
            }
            else
            {
                UtilTokens.WriteAErrorMensage("sintxa error in declared property", search.tokens, escopo);
                return null;
            }

            return exprssRetorno;

        }


        private void ProcessamentoDemaisChamadasDeMetodoAninhadas(int indexSubSearch, SearchByRegexExpression search,Objeto obj_caller, ExpressaoChamadaDeMetodo exprssRetorno, Escopo escopo)
        {
            while (indexSubSearch < search.subExpressoesSearch.Count)
            {
                // valida se a sub-search currente contém uma "ExpressaoProppriedadesAninhadas".
                if (search.subExpressoesSearch[indexSubSearch].TipoExpressao == typeof(ExpressaoPropriedadesAninhadas))
                {


                    if (search.subExpressoesSearch[indexSubSearch].ids.Count >= 2)
                    {
                        // pega a propriedade currente, para validação, e formação da ExpressaoPropriedadeAninhada.
                        string str_proprerty = search.subExpressoesSearch[indexSubSearch].ids[0].input;

                        // obtem um objeto que representa a próxima propriedade aninhada.
                        Objeto obj_property = obj_caller.GetField(str_proprerty);


                        ExpressaoPropriedadesAninhadas eprss = null;
                        List<Objeto> aninhamento = new List<Objeto>();
                        List<string> propriedadesAninhadas = new List<string>();
                        propriedadesAninhadas.Add(str_proprerty);
                        if (obj_property != null)
                        {

                          
                            
                       
                            aninhamento.Add(obj_caller);
                            aninhamento.Add(obj_property);

                            



                            // seta o objeto caller para o objeto da propriedade, a fim de continuar o processamento de outras
                            // propriedades aninhadas ou chamadas de metodo aninhadas.
                            obj_caller = obj_property;

                        }

                        eprss = new ExpressaoPropriedadesAninhadas(aninhamento, propriedadesAninhadas);
                    }
                }
                
                indexSubSearch++;
            } // while indexSubSearch


        }
        public ExpressaoNILL ExpressaoNill(SearchByRegexExpression search, Escopo escopo)
        {
            if ((search.ids == null) || (search.ids.Count == 0))
            {
                UtilTokens.WriteAErrorMensage("erro de sintaxe de expressao", search.tokens, escopo);
                return null;
            }

            if (search.ids[0].input== "NILL")
            {
                ExpressaoNILL exprssRetorno = new ExpressaoNILL();

                exprssRetorno.nill = "NILL"; /// seta o texto da expressão para a constante "NILL".
                exprssRetorno.tipoDaExpressao = "NILL"; //seta o tipo da expressão.
                exprssRetorno.tokens = search.tokens.ToList<string>(); // obtem os tokens da expressao.
                return exprssRetorno;
            }
            else
            {
                UtilTokens.WriteAErrorMensage("esperado token NILL", search.tokens, escopo);
                return null;
            }

        }

        public ExpressaoNumero ExpressaoNumero(SearchByRegexExpression search, Escopo escopo)
        {
            if ((search.tokens != null) && (search.tokens.Count > 0))
            {

                if (search.tokens.Contains("="))
                {
                    int indexNumero = search.tokens.IndexOf("=") + 1;
                    if (indexNumero >= search.tokens.Count)
                    {
                        return null;
                    }
                    string nomeNumero= search.tokens[indexNumero];
                    Expressao exprss1= new Expressao();
                    if (exprss1.IsNumero(nomeNumero))
                    {
                        ExpressaoNumero exprssRetorno = new ExpressaoNumero();
                        exprssRetorno.numero = nomeNumero;
                        // obtem os tokens da expressao.
                        exprssRetorno.tokens = search.tokens.GetRange(indexNumero,1);
                        // obtem o tipo da expressao: int, double, float,...                                                       
                        exprssRetorno.tipoDaExpressao = exprssRetorno.GetTipoExpressao();



                        return exprssRetorno;
                    }
                    else
                    if (search.tokens.Count > 3)
                    {
                        // caso de operador unario e binario ao mesmo tempo.
                        string operadorNumero = search.tokens[indexNumero];

                        if ((operadorNumero == "-") || (operadorNumero == "+"))
                        {
                            nomeNumero = operadorNumero + search.tokens[indexNumero + 1];
                            if (!exprss1.IsNumero(nomeNumero))
                            {
                                return null;
                            }
                            ExpressaoNumero exprssRetorno = new ExpressaoNumero();
                            exprssRetorno.numero = nomeNumero;
                            // obtem os tokens da expressao.
                            exprssRetorno.tokens = search.tokens.GetRange(indexNumero, 2);
                            // obtem o tipo da expressao: int, double, float,...                                                       
                            exprssRetorno.tipoDaExpressao = exprssRetorno.GetTipoExpressao();



                            return exprssRetorno;
                        }
                        
                    }
                }





                Expressao exprss = new Expressao();
                if (exprss.IsNumero(search.tokens[0]))
                {
                    ExpressaoNumero exprssRetorno = new ExpressaoNumero();
                    exprssRetorno.numero = search.tokens[0]; // obtem o valor do numero, no search regex.
                    // obtem os tokens da expressao.
                    exprssRetorno.tokens = search.tokens.GetRange(0, 1);
                    // obtem o tipo da expressao: int, double, float,...                                                       
                    exprssRetorno.tipoDaExpressao = exprssRetorno.GetTipoExpressao();



                    return exprssRetorno;
                }
                else
                if ((search.tokens.Count >= 2) && exprss.IsNumero(search.tokens[1])) 
                {
                    string nomeNumero = "";
                 
                    if ((search.tokens[0] == "+") || (search.tokens[0] == "-"))
                    {
                        nomeNumero = search.tokens[0] + search.tokens[1];
                        if (!exprss.IsNumero(nomeNumero))
                        {
                            return null;
                        }
                       
                    }
                    else
                    {
                        nomeNumero = search.tokens[1];
                       
                    }
                    

                    ExpressaoNumero exprssRetorno = new ExpressaoNumero();
                    exprssRetorno.numero = nomeNumero; 
                    // obtem os tokens da expressao.
                    exprssRetorno.tokens = search.tokens.GetRange(0,2); 

                    // obtem o tipo da expressao: int, double, float,...
                    exprssRetorno.tipoDaExpressao = exprssRetorno.GetTipoExpressao();



                    return exprssRetorno;
                }
                {
                    UtilTokens.WriteAErrorMensage("esperado numero", search.tokens, escopo);
                    return null;
                }

            }
            else
            {
                return null;
            }

        }

        public ExpressaoObjeto ExpressaoObjeto(SearchByRegexExpression search, Escopo escopo)
        {
            if ((search == null) || (search.tokens == null) || (search.tokens.Count==0))
            {
                UtilTokens.WriteAErrorMensage("internal error in ExpressaoSearch class, method ExpressaoObjeto.", search.tokens, escopo);
                return null;
            }
            string nomeObjeto = search.tokens[0];
            int index = Expressao.headers.cabecalhoDeClasses.FindIndex(k => k.nomeClasse == nomeObjeto);
            if (index != -1)
            {
                Objeto objCaller = new Objeto("public", nomeObjeto, nomeObjeto, null);
                // extensao devido a processamento de wrapper data, que tem parametros como um tipo de objeto.
                ExpressaoObjeto exprssRetornoObjetoComoTipo = new ExpressaoObjeto(objCaller);
                return exprssRetornoObjetoComoTipo;
            }
            // o token do objeto nao eh um tipo de objeto, entao a  verificacao se o objeto esta presente no contexto de escopo.
            if (escopo.tabela.GetObjeto(nomeObjeto, escopo)==null)
            {
                UtilTokens.WriteAErrorMensage("indefined objet in present scope, object: " + nomeObjeto, search.tokens, escopo);
                return null;
            }
            else
            {
                Objeto objMain= escopo.tabela.GetObjeto(nomeObjeto, escopo);
                ExpressaoObjeto exprssRetorno = new ExpressaoObjeto(objMain);
                return exprssRetorno;
            }
        }



        public ExpressaoChamadaDeMetodo ChamadaDeMetodo(SearchByRegexExpression search, Escopo escopo)
        {

            
            if (search == null)
            {
                return null;
            }
                


            if (search.TipoExpressao != typeof(ExpressaoChamadaDeMetodo))
            {
                return null;
            }
                

            // se nao houver tokens na chamada, ou nao ha os parenteses da interface de parametros, retorna.
            if ((search.tokens == null) || (!search.tokens.Contains("(")) || (!search.tokens.Contains(")")))
            {
                return null;
            }

            string str_caller = search.tokens[0];
            string str_method = "";
     
            Objeto obj_caller = escopo.tabela.GetObjeto(str_caller, escopo);

            //***************************************************************************************************
            /// case  m1.fx(1,5);

            if (search.tokens.Count >= 3)
            {
                str_method = search.tokens[2];
               
                // casos: com objeto caller, OU metodo estatico (com objeto caller um nome de classe).
                if (search.tokens[3] == "(")
                {

                    // obtem metodos com mesmo nome dos tokens da chamada.
                    List<HeaderMethod> metodos = GetMethodsFromHeaders(str_method);


                    // verificacao se obteve metodos com o mesmo nome da chamada de metodo.
                    if ((metodos != null) && (metodos.Count > 0))
                    {

                        // obtem a interface de parametros da funcao.
                        List<string> tokensParametrosChamada = search.tokens.ToList<string>();
                        // retira os tokens da classe, o operador dot, e nome do metodo estatico.
                        tokensParametrosChamada.RemoveRange(0, 3);
                        // retira o operador fim de expressao, se houver.
                        if (search.tokens.Contains(";"))
                        {
                            tokensParametrosChamada.Remove(";");
                        }
                        // retira os operadores parenteses.
                        tokensParametrosChamada.RemoveAt(0);
                        tokensParametrosChamada.RemoveAt(tokensParametrosChamada.Count - 1);



                        // obtem as expressoes, dos parametros.
                        List<Expressao> exprss_parametrosFuncao = Expressao.ExtraiExpressoes(tokensParametrosChamada, escopo);



                        //  o metodo eh METODO ESTATICO. qualquer dos metodos encontrados são metodos estaticos.
                        if (str_method == metodos[0].className)
                        {
                            string str_nameClass = (string)str_method.Clone();

                            if ((search.ids == null) || (search.ids.Count < 2))
                            {
                                return null;
                            }
                            // passa para o proximo token, pois o primeiro eh o nome da classe: um metodo estatico.
                            str_method = search.tokens[2]; // token[0]: nome da classe. token[1]: operador dot token[2]: nome da funcao estática.


                            Metodo umMetodo = UtilTokens.FindMethodCompatible(obj_caller, str_nameClass, str_method, str_nameClass, exprss_parametrosFuncao, escopo, false, false);
                            // encontrou o metodo estático.
                            if (umMetodo != null)
                            {


                                // constroi a expressao chamada de metodo.
                                ExpressaoChamadaDeMetodo chamada = new ExpressaoChamadaDeMetodo(obj_caller, umMetodo, exprss_parametrosFuncao);
                                return chamada;
                            }
                            else
                            {
                                return null;
                            }
                        }


                        if (obj_caller != null)
                        {
                            // o metodo nao eh NAO ESTATICO.
                            for (int i = 0; i < metodos.Count; i++)
                            {
                                str_caller = obj_caller.GetNome();
                                string str_classNameOfMethod = obj_caller.GetTipo();

                                Metodo umMetodo = UtilTokens.FindMethodCompatible(obj_caller, str_classNameOfMethod, str_method, str_classNameOfMethod, exprss_parametrosFuncao, escopo, false, false);
                                if (umMetodo != null)
                                {


                                    // constroi a expressao chamada de metodo.
                                    ExpressaoChamadaDeMetodo chamada = new ExpressaoChamadaDeMetodo(obj_caller, umMetodo, exprss_parametrosFuncao);
                                    return chamada;
                                }


                            }

                        }



                        UtilTokens.WriteAErrorMensage("method definition not found", search.tokens, escopo);
                        return null;
                    }
                }
                

            }


            // caso: não há objeto, é uma funcao,nao metodo.
            List<Metodo> funcoes = escopo.tabela.GetFuncao(search.tokens[0]);
            if (funcoes == null)
            {
                return null;
            }
            List<string> tokensParametros = search.tokens.ToList<string>();
            if (tokensParametros.Contains(";"))
            {
                tokensParametros.Remove(";");
            }
            tokensParametros.RemoveRange(0,3); // remove o nome da classe, o nome de operador dot, o nome da funcao.. 


            tokensParametros.RemoveAt(0);      // remove os tokens de abertura/fechamento da interface de parametros. 
            tokensParametros.RemoveAt(tokensParametros.Count - 1);



            List<string> tokensParametrosCopy= tokensParametros.ToList();
            List<Expressao> exprssParametros = Expressao.ExtraiExpressoes(tokensParametrosCopy, escopo);


            Metodo funcaoCompativel = FindFunctionCompatibliteWithParameters(funcoes, tokensParametros, escopo);

                //se encontrou um metodo compativel, constroi a expressao chamada de metodo e retorna com esta expressao.
                if (funcaoCompativel == null)
                {
                    UtilTokens.WriteAErrorMensage("internal error, in ExpressaoSearch.ChamadaDeMetodo, method definition not found or sintaxe error in name and class of method.", search.tokens, escopo);
                    return null;
                }
                else
                {

                ExpressaoChamadaDeMetodo exprssRetorno = new ExpressaoChamadaDeMetodo(obj_caller, funcaoCompativel, exprssParametros);


                List<string> tokensConsumidos = new List<string>();
                tokensConsumidos.Add(funcaoCompativel.nome);
                tokensConsumidos.Add("(");
                tokensConsumidos.AddRange(tokensParametros);
                tokensConsumidos.Add(")");
                if (search.tokens.Contains(";"))
                {
                    tokensConsumidos.Add(";");
                }

                // faz o processamento de mais de uma expressao chamada de metodo, ou mais propriedades aninhadas.
                this.ProcessamentoDemaisChamadasDeMetodoAninhadas(0, search, obj_caller, exprssRetorno, escopo);

                exprssRetorno.tokens = tokensConsumidos;
                    return exprssRetorno;
                }

        }

        private static List<HeaderMethod> GetMethodsFromHeaders(string str_method)
        {
            List<HeaderClass> headers = Expressao.headers.cabecalhoDeClasses;
            List<HeaderMethod> metodos = new List<HeaderMethod>();
            for (int x = 0; x < headers.Count; x++)
            {


                if (headers[x].methods != null)
                {
                    bool isfoundMethod= false;
                    for (int m = 0; m < headers[x].methods.Count; m++)
                    {
                        if ((headers[x].methods[m].name == str_method) || (str_method == headers[x].nomeClasse))
                        {
                            isfoundMethod = true;
                            metodos.Add(headers[x].methods[m]);
                          
                        }
                        
                    }
                    if (isfoundMethod)
                    {
                        return metodos;
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// encontra uma funcao dentre de uma lista de funcoes com mesmo nome, qual funcao que tem os memos tipos parametro.
        /// </summary>
        /// <param name="funcoes">lista de funcoes com mesmo nome.</param>
        /// <param name="tokensParametros">tokens da interface de parametros.</param>
        /// <param name="escopo">contexto onde estao as funcoes. Busca recursiva pode chegar ao escopo global.</param>
        /// <returns>retorna uma funcao compativel com os tipos de parametros da chamada de funcao.</returns>
        private Metodo FindFunctionCompatibliteWithParameters(List<Metodo> funcoes, List<string> tokensParametros, Escopo escopo)
        {
          
            List<Expressao> exprss_parametros = Expressao.ExtraiExpressoes(tokensParametros, escopo);

            for (int x = 0; x < funcoes.Count; x++)
            {
                if ((funcoes[x].parametrosDaFuncao == null) && (exprss_parametros == null))
                {
                    return funcoes[x];
                }
                if ((funcoes[x].parametrosDaFuncao.Length == 0) && (exprss_parametros.Count == 0))
                {
                    return funcoes[x];
                }
            }

            for (int x = 0; x < funcoes.Count; x++)
            {
                bool isFoundCompatible = true;
                for (int p = 0; p < exprss_parametros.Count; p++)
                {
                    if (exprss_parametros[p].tipoDaExpressao != funcoes[x].parametrosDaFuncao[p].tipo)
                    {
                        isFoundCompatible = false;
                        break;
                    }
                }

                if (isFoundCompatible)
                {
                    return funcoes[x];
                }
            }

            return null;
        }


        public List<Expressao> ExtraiExpressoes(List<SearchByRegexExpression> data, Escopo escopo)
        {
            if ((data == null) || (data.Count == 0))
            {
                return new List<Expressao>();
            }
            else
            {
                List<Expressao> expressoes = new List<Expressao>();
                for (int x = 0; x < data.Count; x++)
                {

                    // previsao para tipos de dados como parametros de uma expressao
                    if ((data[x].tokens!=null) && (data[x].tokens.Count>0) && (Expressao.headers.cabecalhoDeClasses.FindIndex(k => k.nomeClasse == data[x].ids[0].ToString())!=-1))
                    {
                        ExpressaoElemento exprssElement = new ExpressaoElemento(data[x].tokens[0]);
                        expressoes.Add(exprssElement);  
                    }
                    else
                    // previsao para expressoes de instanciacao.
                    if (data[x].TipoExpressao == typeof(ExpressaoInstanciacao))
                    {
                        ExpressaoInstanciacao exprssIntantiate = this.Instanciacao(data[x], escopo);
                        if (exprssIntantiate != null)
                        {
                            expressoes.Add(exprssIntantiate);
                        }
                        else
                        {
                            escopo.GetMsgErros().Add("expressao: " + data[x].inputRaw + " erro de sintaxe.");
                        }
                    }
                    else
                    // previsao para expressoes propriedades aninhadas.
                    if (data[x].TipoExpressao == typeof(ExpressaoPropriedadesAninhadas))
                    {
                        ExpressaoPropriedadesAninhadas exprssProprieadades = this.PropriedadesAninhadas(data[x], escopo);
                        if (exprssProprieadades != null)
                        {
                            expressoes.Add(exprssProprieadades);
                        }
                        else
                        {
                            escopo.GetMsgErros().Add("expressao: " + data[x].inputRaw + " erro de sintaxe.");
                        }
                    }
                    else
                    // previsao para expressoes chamada de metodo.
                    if (data[x].TipoExpressao == typeof(ExpressaoChamadaDeMetodo))
                    {
                        ExpressaoChamadaDeMetodo exprssChamada = this.ChamadaDeMetodo(data[x], escopo);
                        if (exprssChamada != null)
                        {
                            expressoes.Add(exprssChamada);
                        }
                        else
                        {
                            escopo.GetMsgErros().Add("expressao: " + data[x].inputRaw + " erro de sintaxe.");
                        }
                    }
                    else
                    // previsao para expressoes objeto.
                    if (data[x].TipoExpressao == typeof(ExpressaoObjeto))
                    {
                        ExpressaoObjeto exprssObjeto = this.ExpressaoObjeto(data[x], escopo);
                        if (exprssObjeto != null)
                        {
                            expressoes.Add(exprssObjeto);
                           
                        }
                        else
                        {
                            UtilTokens.WriteAErrorMensage("erro de sintaxe de objeto", data[x].tokens, escopo);

                        }
                    }
                    else
                    // previsao para expressoes de numeros.
                    if (data[x].TipoExpressao == typeof(ExpressaoNumero))
                    {
                        ExpressaoNumero exprssNumero = this.ExpressaoNumero(data[x], escopo);
                        if (exprssNumero != null)
                        {
                            expressoes.Add(exprssNumero);
                        }
                        else
                        {
                            UtilTokens.WriteAErrorMensage("erro de sintaxe, esperado numero", data[x].tokens, escopo);
                        }
                    }
                    else
                    // previsao para expressoes entre parenteses.
                    if (data[x].TipoExpressao == typeof(ExpressaoEntreParenteses))
                    {
                        Expressao exprssEntreParenteses = this.EntreParenteses(data[x], escopo);
                        if (exprssEntreParenteses != null)
                        {
                            expressoes.Add(exprssEntreParenteses);
                        }
                        else
                        {
                            escopo.GetMsgErros().Add("expressao: " + data[x].inputRaw + " erro de sintaxe.");
                        }
                    }
                    else
                    // expressoes operador.
                    if (data[x].TipoExpressao == typeof(ExpressaoOperador))
                    {
                        Expressao exprssOperador = this.Operadores(data[x], escopo);
                        if (exprssOperador != null)
                        {
                            expressoes.Add(exprssOperador);
                        }
                        else
                        {
                            escopo.GetMsgErros().Add("expressao: " + data[x].inputRaw + " erro de sintaxe.");
                        }
                    }
                    else
                    // expressoes literais.
                    if (data[x].TipoExpressao == typeof(ExpressaoLiteralText))
                    {
                        ExpressaoLiteralText exprssLiteral = this.Literal(data[x], escopo);
                        if (exprssLiteral != null)
                        {
                            expressoes.Add(exprssLiteral);
                        }
                        else
                        {
                            escopo.GetMsgErros().Add("expressao: " + data[x].inputRaw + " erro de sintaxe.");
                        }
                    }
                    else
                    // expressao null.
                    if (data[x].TipoExpressao == typeof(ExpressaoNILL))
                    {
                        ExpressaoNILL exprssNill = this.ExpressaoNill(data[x], escopo);
                        if (exprssNill != null)
                        {
                            expressoes.Add(exprssNill);
                        }
                        else
                        {
                            escopo.GetMsgErros().Add("expressao: " + data[x].inputRaw + " erro de sintaxe.");
                        }


                    }
         
                }

                // retorna as expressoes extraidas.
                return expressoes;

            }


               

        }

        public class Testes : SuiteClasseTestes
        {
            public Testes() : base("testes para construção de expressões com searchers expression")
            {
            }

            public void TesteExpressaoAtribuicao(AssercaoSuiteClasse assercao)
            {
                string str_expressao = "a=1;";
                string str_codigo = "int a= 0;";

                ProcessadorDeID compilador= new ProcessadorDeID(str_codigo);
                compilador.Compilar();

                ExpressaoSearch exprsSearch = new ExpressaoSearch();
                Expressao exprssresult =exprsSearch.BuildExpressions(str_expressao, compilador.escopo);

                assercao.IsTrue(
                    exprssresult != null &&
                    exprssresult.GetType() == typeof(ExpressaoAtribuicao));

            }

            public void TesteExpressaoInstanciacao(AssercaoSuiteClasse assercao)
            {
                string str_expressao = "int a= c +d ";
                string str_codigo = "int a=1; int c=1; int d=1;";

                Expressao.InitHeaders("");

                ProcessadorDeID compilador = new ProcessadorDeID(str_codigo);
                compilador.Compilar();

                ExpressaoSearch expressaoSearch = new ExpressaoSearch();
                Expressao exprssRetorno = expressaoSearch.BuildExpressions(str_expressao, compilador.escopo);

                assercao.IsTrue(
                    exprssRetorno != null &&
                    exprssRetorno.GetType() == typeof(ExpressaoInstanciacao));
            }


            public void TesteExpressaoChamadaDeMetodoSimples(AssercaoSuiteClasse assercao)
            {
                string str_classe = "public class classeB { public classeB(){ int x=0; } public int metodoB(int x, int y){x=x+y;}}; classeB b= create();";
                string str_expressao = " b.metodoB(1,1)";

                // inicializa os headers, das classes base, e da classe do codigo.
                Expressao.InitHeaders(str_classe);



                ProcessadorDeID compilador = new ProcessadorDeID(str_classe);
                compilador.Compilar();




                ExpressaoSearch expressaoSrc = new ExpressaoSearch();
                Expressao exprssRetorno = expressaoSrc.BuildExpressions(str_expressao, compilador.escopo);


                assercao.IsTrue(
                    (exprssRetorno != null) &&
                    (exprssRetorno.GetType() == typeof(ExpressaoChamadaDeMetodo)) &&
                    (((ExpressaoChamadaDeMetodo)exprssRetorno).nomeMetodo != null) &&
                    (((ExpressaoChamadaDeMetodo)exprssRetorno).nomeMetodo == "metodoB"));
            }


            public void TestePropriedadesAninhadas(AssercaoSuiteClasse assercao)
            {
                string codigo_X = "public class classeB { public classeB(){ int x=0; }; public int metodoB(int x, int y){x=x+y;}}; classeB b= create();";
                string codigo_Y = "public class classeC { public classeB propriedade1; public classeC(){ int x=0; }; public int metodoC(int x, int y){x=x+y;}}; classeB b= create();";
                string codigo_Z = "classeC objC = create(); objC.propriedade1= create();";
                string str_expressao = "objC.propriedade1";


                // inicializa os headers das classes basicas, e das classes do codigo.
                Expressao.InitHeaders(codigo_X + " " + codigo_Y + codigo_Z);



                ProcessadorDeID compilador = new ProcessadorDeID(codigo_X + " " + codigo_Y + " " + codigo_Z);
                compilador.Compilar();


                ExpressaoSearch search = new ExpressaoSearch();
                Expressao exprssRetorno = search.BuildExpressions(str_expressao, compilador.escopo);


                // teste automatizado.
                assercao.IsTrue(
                    (exprssRetorno != null) &&
                    (exprssRetorno.GetType() == typeof(ExpressaoPropriedadesAninhadas)) &&
                    (((ExpressaoPropriedadesAninhadas)exprssRetorno).aninhamento != null) &&
                    (((ExpressaoPropriedadesAninhadas)exprssRetorno).aninhamento.Count == 1) &&
                    (((ExpressaoPropriedadesAninhadas)exprssRetorno).aninhamento[0].GetNome() == "objC") &&
                    (((ExpressaoPropriedadesAninhadas)exprssRetorno).propriedade == "propriedade1") &&
                    (((ExpressaoPropriedadesAninhadas)exprssRetorno).nomeObjeto == "objC"));


            }


            public void TestExpressaoSimples(AssercaoSuiteClasse assercao)
            {
                if (Expressao.headers == null)
                    Expressao.InitHeaders("");

                string codigo = "int e=1; int f=1";
                string str_expressao = "e+f";
                ProcessadorDeID compilador = new ProcessadorDeID(codigo);
                compilador.Compilar();




                ExpressaoSearch expressaoSrc = new ExpressaoSearch();
                Expressao exprssRetorno = expressaoSrc.BuildExpressions(str_expressao, compilador.escopo);


                assercao.IsTrue(
                    (exprssRetorno != null) &&
                    (exprssRetorno.GetType() == typeof(ExpressaoOperador)));

            }


            public void TesteExpressaoQuatroOperadores(AssercaoSuiteClasse assercao)
            {
                string codigo = "int a=1; int b=2; int c=3; int d=4;";
                string str_expressao = "a+b*c/d";



                ProcessadorDeID compilador = new ProcessadorDeID(codigo);
                compilador.Compilar();


                ExpressaoSearch search = new ExpressaoSearch();
                Expressao exprssRetorno = search.BuildExpressions(str_expressao, compilador.escopo);


                assercao.IsTrue(
                    (exprssRetorno != null) &&
                    exprssRetorno.GetType() == typeof(ExpressaoOperador) &&
                    (exprssRetorno.Elementos != null) &&
                    (exprssRetorno.Elementos.Count == 7) &&
                    (exprssRetorno.Elementos[1].GetType() == typeof(ExpressaoOperador)) &&
                    (((ExpressaoOperador)exprssRetorno.Elementos[1]).operador != null) &&
                    (((ExpressaoOperador)exprssRetorno.Elementos[1]).operador.nome == "+"));

            }




            public void Teste1OperadorE2ChamadasDeMetodo(AssercaoSuiteClasse assercao)
            {

                string str_classeA = "public class classeA{ public classeA() {int x=0;} public int metodoA(int z){int y=2; } public int metodoC(int x, int y, int z){ int x=1;} public int metodoB(int n, int m){ int x=1;}} ";
                string str_objetos = "classeA a= create(); classeA b= create(); int x=1; int y=1; int z=1; int c=2; int n=2; int m=2;";

                string str_total = str_classeA;

                string str_expressao = "a.metodoA(b.metodoC(x,y,z)+c) + b.metodoB(n,m)";



                Expressao.InitHeaders(str_classeA);

                ProcessadorDeID compilador = new ProcessadorDeID(str_total + str_objetos);
                compilador.Compilar();


                ExpressaoSearch exprsSearch = new ExpressaoSearch();
                Expressao exprssRetorno = exprsSearch.BuildExpressions(str_expressao, compilador.escopo);


                // teste automatizado.
                assercao.IsTrue(
                    (exprssRetorno != null) &&
                    (exprssRetorno.Elementos != null) &&
                    (exprssRetorno.Elementos.Count == 3) &&
                    (exprssRetorno.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo)) &&
                    (((ExpressaoChamadaDeMetodo)exprssRetorno.Elementos[0]).parametros != null) &&
                    (((ExpressaoChamadaDeMetodo)exprssRetorno.Elementos[0]).parametros.Count == 1) &&
                    (((Expressao)((ExpressaoChamadaDeMetodo)exprssRetorno.Elementos[0]).parametros[0]).Elementos != null) &&
                    (((Expressao)((ExpressaoChamadaDeMetodo)exprssRetorno.Elementos[0]).parametros[0]).Elementos.Count == 3));



            }


  

 
        }
    }

     
        

}

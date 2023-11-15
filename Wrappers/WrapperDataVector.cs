using parser.textoFormatado;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text.RegularExpressions;
using Wrappers.DataStructures;

namespace parser
{
    // implementa o processamento de vetores atraves de expressoes de chamada de metodo, possibilitando utilização de estruturas de dados como se fossem nativas,
    // e sem processamento adicional: as estruturas de dados fazem suas operações por expressões chamada de metodo.
    public class WrapperDataVector : WrapperData
    {

        public Vector vectorElements;                                   // objeto wrapper data.



        // codigo resumido para instanciacao var, formato: var m=[1,2,3,x+1,x*y,metodoB(a,b)], todos elementos podem ser expressoes; 



        private string codigoInstanciacaoRigor = "Vector id [ exprss ]";  // instanciacao tipada.
        private string coddigoInstanciacaoClassica = "id [ ] id [ exprss ]";  // instanciacao classica.

        protected static Regex regexInstanciacaoResumidaRigor;          // regex para instanciacao rigor.
        protected static Regex regexInstanciacaoResumidaClassica;       // regex para instanciacao classica. 



        

        public WrapperDataVector()
        {

            // constroi a string da expressao regex.
            TextExpression textRegex = new TextExpression();
            string textRigor = textRegex.FormaExpressaoRegularGenerica(this.codigoInstanciacaoRigor);
            string textClassico = textRegex.FormaExpressaoRegularGenerica(this.coddigoInstanciacaoClassica);



            // instancia a expressao regex, para tipos de  instanciação resumida.
            regexInstanciacaoResumidaRigor = new Regex(textRigor);
            regexInstanciacaoResumidaClassica = new Regex(textClassico);


            this.tipo = "Vector";


        }

        /// <summary>
        /// obtem o nome de um wrrapper data object.
        /// </summary>
        /// <param name="tokens">tokens contendo a definicao do wrapper data.</param>
        /// <returns></returns>
        public override string GetNameWrapperObject(List<string> tokens, int index)
        {
            /*
             *  codigoInstanciacaoRigor = "Vector id [ exprss ]";  // instanciacao tipada.
                coddigoInstanciacaoClassica = "id [ ] id [ exprss ]";  // instanciacao classica.
             */

            if ((tokens[index] == "Vector") && (tokens.Count >= 1))
            {
                // instanciacao tipada.
                // codigoInstanciacaoRigor = "Vector id [ exprss ]" ---> Vector id (como parametro).
                return tokens[index + 1];
            }
            else
            if ((index + 2 < tokens.Count) && (tokens[index + 1] == "[") && (tokens[index + 2] == "]") && (TextExpression.IsID(tokens[index])) && (TextExpression.IsID(tokens[index + 3]))) 
            {
                // instanciacao classica.
                // coddigoInstanciacaoClassica = "id [ ] id [ exprss ]";--> id [ ] id (como parametro).  
                return tokens[tokens.IndexOf("]") + 1];
            }
            else
            {
                return null;
            }
           
        }


        /// <summary>
        /// obtem o tipo elemento do vetor.
        /// </summary>
        /// <param name="tokens">tokens contendo a definicao do wrapper object.</param>
        /// <param name="countTokens">contador de tokens utilizado na definicao do wrapper vector.</param>
        /// <returns></returns>
        public override string GetTipoElemento(List<string> tokens, ref int countTokens)
        {
            // "Vector id [ exprss ]" ---> Vector id (como parametro;  
            if (tokens.IndexOf("Vector") != -1)
            {
                countTokens = 2;
                return "Object";
            }
            else
            // "id [ ] id [ exprss ]   ---> id [ ]  id  (como parametro).
            {
                int indexTipoElemento = tokens.IndexOf("[");
                if (indexTipoElemento - 1 >= 0) 
                {
                    countTokens = 4;
                    return tokens[indexTipoElemento - 1];
                }
            }

            return null;

            

        }

      
        /// <summary>
        /// verifica se ha um wrapper data vector.
        /// </summary>
        /// <param name="tokens">tokens onde está contido o wrapper data.</param>
        /// <returns></returns>
        public override List<string> isThisTypeWrapperParameter(List<string> tokens, int index)
        {

            // codigoInstanciacaoRigor = "Vector id [ exprss ]";  ----> "Vector id" (como parametro) ;
            // coddigoInstanciacaoClassica = "id [ ] id [ exprss ]";  ----> id[] id (como parametro); 

            if ((index + 2 <= tokens.Count) && (tokens[index] == "Vector"))
            {

                if (TextExpression.IsID(tokens[index + 1])) 
                {
                    List<string> tokensWrapper = tokens.GetRange(index, 2);
                    return tokensWrapper;
                }
                else
                {
                    return null;
                }

            }
            else
            if ((index + 3 < tokens.Count) && (tokens[index + 1] == "[") && (tokens[index + 2] == "]") &&
                (TextExpression.IsID(tokens[index])) && (TextExpression.IsID(tokens[index + 3]))) 
            {
                List<string> tokensWrapper = tokens.GetRange(index, 4);
                return tokensWrapper;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// retorna uma chamada de metodo estatica que faz a construcao do objeto vetor em tempo de execução do codigo.
        /// </summary>
        /// <param name="exprssInstanciacaoEmNotacaoWrapper">expressao da instanciacao, em objetos wrapper.</param>
        /// <param name="escopo">contexto onde está a expressao.</param>
        /// <returns>retorna a lista de tokens da chamada de metodo estatica que instanciara o objeto wrapper vector.</returns>
        public override List<string> Create(ref string exprssInstanciacaoEmNotacaoWrapper, Escopo escopo)
        {

            // obtem a lista de tokens da expressao.
            List<string> tokensOriginais = new Tokens(exprssInstanciacaoEmNotacaoWrapper).GetTokens();
            if ((tokensOriginais == null) || (tokensOriginais.Count == 0))
                return null;


            string nomeObjetoVetor = null;
            string tipoObjetoElemento = null;

            string pattern = new TextExpression().FormaPatternResumed(Utils.OneLineTokens(tokensOriginais));
            pattern = pattern.Replace("number", "exprss");
            List<string> tokensPattern = new Tokens(pattern).GetTokens();
            if ((tokensPattern == null) || (tokensPattern.Count == 0))
            {
                return null;
            }

            if (tokensPattern[0] == "id")
            {
                tokensPattern[0] = "Vector";
            }


            // INSTANCIACAO CLASSICA:   // id[] id[exprs]   
            string pattern1 = new TextExpression().FormaPatternResumed(Utils.OneLineTokens(tokensOriginais));
            pattern1 = pattern1.Replace("number", "exprss");

            List<string> tokensPatternClassic = new Tokens(pattern1).GetTokens();
            tipoObjetoElemento = tokensOriginais[0];
            nomeObjetoVetor = tokensOriginais[3];


            // CREATE NOTACAO RIGOR: "Vector id [ exprss ]".
            // codigoInstanciacaoRigor = "Vector id [ exprss ]";  ----> "Vector id" (como parametro) ;
            if (MatchElement(tokensPatternClassic.ToArray(), WrapperDataVector.regexInstanciacaoResumidaRigor))
            {
                string nomeVector = "";
                int indexTokenVector = tokensOriginais.IndexOf("Vector");
                if (indexTokenVector != -1)
                {
                    if (indexTokenVector + 1 < tokensOriginais.Count)
                    {
                        nomeVector = tokensOriginais[indexTokenVector + 1];
                        int indexSizeExpress = tokensOriginais.IndexOf("[", indexTokenVector);
                        if (indexSizeExpress != -1)
                        {
                            List<string> tokensSize = UtilTokens.GetCodigoEntreOperadores(indexSizeExpress, "[", "]", tokensOriginais);
                            if ((tokensSize != null) && (tokensSize.Count > 2))
                            {
                                tokensSize.RemoveAt(0);
                                tokensSize.RemoveAt(tokensSize.Count - 1);
                                List<Expressao> exprssSIZE = Expressao.ExtraiExpressoes(tokensSize, escopo);
                                if ((exprssSIZE != null) && (exprssSIZE.Count > 0) && (exprssSIZE[0].tipoDaExpressao == "int"))
                                {
                                    List<string> tokensRetorno = new List<string>();
                                    tokensRetorno.Add(nomeObjetoVetor);
                                    tokensRetorno.Add(".");
                                    tokensRetorno.Add("Create");
                                    tokensRetorno.Add("(");
                                    tokensRetorno.AddRange(exprssSIZE[0].tokens);
                                    tokensRetorno.Add(")");
                                    if (tokensOriginais.IndexOf(";") > -1)
                                    {
                                        tokensRetorno.Add(";");
                                    }

                                    // instancia um vetor, com o tipo do elemento vindo da instanciacao.
                                    Vector vecObj = new Vector("Object");
                                    vecObj.SetNome(nomeObjetoVetor);
                                    vecObj.SetTipoElement("Object");
                                    vecObj.isWrapperObject = true;
                                    // registra o vetor criado.
                                    escopo.tabela.GetObjetos().Add(vecObj);




                                    // extrai os codigos consumidos na anotação wrapper data.
                                    exprssInstanciacaoEmNotacaoWrapper = WrapperData.ExtraiTokensConsumido(tokensOriginais, tokensOriginais, tokensRetorno);

                                    return tokensRetorno;

                                }


                            }
                            else
                            {
                                // caso de um parametro, sem definicao de tamanho!.
                                Vector vectorCreated = new Vector("Object");
                                vectorCreated.SetNome(nomeVector);
                                vectorCreated.SetTipoElement("Object");
                                vectorCreated.isWrapperObject = true;
                                escopo.tabela.RegistraObjeto(vectorCreated);


                                List<string> tokensRetorno = new List<string>();
                                tokensRetorno.Add(nomeObjetoVetor);
                                tokensRetorno.Add(".");
                                tokensRetorno.Add("Create");
                                tokensRetorno.Add("(");
                                tokensRetorno.Add(")");
                                if (tokensOriginais.IndexOf(";") > -1)
                                {
                                    tokensRetorno.Add(";");
                                }

                                // extrai os codigos consumidos na anotação wrapper data.
                                exprssInstanciacaoEmNotacaoWrapper = WrapperData.ExtraiTokensConsumido(tokensOriginais, tokensOriginais, tokensRetorno);

                                return tokensRetorno;

                            }
                        }
                    }
                }
            }
            // CREATE NOTACAO CLASSICA: "id[] id[exprs]".   
            if (MatchElement(tokensPatternClassic.ToArray(), WrapperDataVector.regexInstanciacaoResumidaClassica))
            {
                if ((tokensOriginais.Count >= 4) && (TextExpression.IsID(tokensOriginais[0])) && (tokensOriginais[1] == "[") && (tokensOriginais[2] == "]") && (TextExpression.IsID(tokensOriginais[3])))
                {

                    int indexOperatorBacket = tokensOriginais.IndexOf("[");
                    indexOperatorBacket = tokensOriginais.IndexOf("[", indexOperatorBacket + 1);


                    List<string> tokensParametrosConstrutor = UtilTokens.GetCodigoEntreOperadores(indexOperatorBacket, "[", "]", tokensOriginais);
                    if ((tokensParametrosConstrutor == null) || (tokensParametrosConstrutor.Count == 0))
                    {
                        return null;
                    }
                    tokensParametrosConstrutor.RemoveAt(0);
                    tokensParametrosConstrutor.RemoveAt(tokensParametrosConstrutor.Count - 1);


                    try
                    {

                        // constroi o parametro de tamanho do vetor.
                        List<Expressao> parametrosConstrutorResumido2 = Expressao.ExtraiExpressoes(tokensParametrosConstrutor, escopo);
                        if ((parametrosConstrutorResumido2[0].Elementos[0].tipoDaExpressao == "int"))
                        {
                            // constroi a chamada de metodo [Create], contendos os tokens processado na instanciacao.


                            List<string> tokensRetorno = new List<string>();
                            tokensRetorno.Add(nomeObjetoVetor);
                            tokensRetorno.Add(".");
                            tokensRetorno.Add("Create");
                            tokensRetorno.Add("(");
                            tokensRetorno.AddRange(parametrosConstrutorResumido2[0].tokens);
                            tokensRetorno.Add(")");
                            if (tokensOriginais.IndexOf(";") > -1)
                            {
                                tokensRetorno.Add(";");
                            }

                            // instancia um vetor, com o tipo do elemento vindo da instanciacao.
                            Vector vecObj = new Vector(tipoObjetoElemento);
                            vecObj.SetNome(nomeObjetoVetor);
                            vecObj.SetTipoElement(tipoObjetoElemento);
                            vecObj.isWrapperObject = true;
                            // registra o vetor criado.
                            escopo.tabela.GetObjetos().Add(vecObj);




                            // extrai os codigos consumidos na anotação wrapper data.
                            exprssInstanciacaoEmNotacaoWrapper = WrapperData.ExtraiTokensConsumido(tokensOriginais, tokensOriginais, tokensRetorno);

                            return tokensRetorno;


                        }
                        else
                        {
                            UtilTokens.WriteAErrorMensage("error in index vector object", tokensOriginais, escopo);
                            return null;
                        }
                    }
                    catch
                    {
                        UtilTokens.WriteAErrorMensage("error in create a vector object", tokensOriginais, escopo);
                        return null;
                    }

                }

            }



            return null;

        }

        /// <summary>
        /// verifica se os tokens da anotação é de uma chamada de metodo set element.
        /// </summary>
        /// <param name="tokensNotacaoWrapper">tokens da anotação wrapper, a investigar.</param>
        /// <returns>[true] se a anotação é de set element.</returns>
        public override bool IsSetElement(string nomeObjeto, List<string> tokensNotacaoWrapper)
        {
            int indexNomeObjeto = tokensNotacaoWrapper.IndexOf(nomeObjeto);
            int indexSinalIgual = tokensNotacaoWrapper.IndexOf("=");
            if (indexSinalIgual == -1)
            {
                return false;
            }
            if (indexNomeObjeto < indexSinalIgual)
            {
                return true;
            }

            return false;
        }







        /// <summary>
        /// em tempo de compilação, é convertido uma expressão em anotação do wrapper, ex.: M[1,5]= valor,
        /// para uma expressão de instanciacao: M.SetElement(1,5, valor).
        /// extrai também dos tokens da expressão de entrada, os tokens consumidos pela anotação wrapper.
        /// </summary>
        /// <param name="exprssEmNotacaoWrapper">expressao em anotação wrapper.</param>
        /// <param name="escopo">contexto onde a expressão wrapper está.</param>
        /// <param name="tokensProcessed">tokens consumidos da anotacao wrapper.</param>
        /// <returns>retorna uma expressão chamada de metodo, contendo dados para setar um elemento, no indice calculado.</returns>
        public override List<string> SETChamadaDeMetodo(ref List<string> tokensOriginais, Escopo escopo, List<string> tokensProcessed)
        {


            if ((tokensOriginais != null) && (tokensOriginais.Count > 0))
            {
                Objeto obj_caller = escopo.tabela.GetObjeto(tokensOriginais[0], escopo);
                if (!obj_caller.isWrapperObject)
                {
                    return null;
                }

                if ((obj_caller != null) || (obj_caller.GetTipo().Equals("Vector")))
                {




                    int indexEquals = tokensOriginais.IndexOf("=");
                    int indexOperadorAbre = tokensOriginais.IndexOf("[");

                    if ((indexOperadorAbre == -1) || (indexEquals == -1))
                        return null;


                    // obtem os tokens que formam os parãmetros da chamada de metodo.
                    List<string> tokens_parametros = UtilTokens.GetCodigoEntreOperadores(indexOperadorAbre, "[", "]", tokensOriginais);

                    if ((tokens_parametros == null) || (tokens_parametros.Count == 0))
                    {
                        return null;
                    }
                    // obtem os tokens do indice. m[indice].
                    tokens_parametros.RemoveAt(0);
                    tokens_parametros.RemoveAt(tokens_parametros.Count - 1);

                    // EXTRAI AS EXPRESSOES PARAMETROS.
                    List<Expressao> exprssParametros = Expressao.ExtraiExpressoes(tokens_parametros, escopo);
                    try
                    {

                        if (exprssParametros[0].Elementos[0].tipoDaExpressao != "int")
                        {
                            UtilTokens.WriteAErrorMensage("error in extract index of a vector object ", tokensOriginais, escopo);
                            return null;
                        }
                    }
                    catch (Exception e)
                    {
                        UtilTokens.WriteAErrorMensage("error in extract index of a vector object " + e.Message, tokensOriginais, escopo);
                        return null;
                    }




                    int indexBeginValor = tokensOriginais.IndexOf("=");
                    if (indexBeginValor == -1)
                    {
                        return null;
                    }

                    List<string> tokensValor = tokensOriginais.GetRange(indexBeginValor, tokensOriginais.Count - indexBeginValor);
                    tokensValor.Remove("=");
                    Expressao exprssValor = new Expressao(tokensValor.ToArray(), escopo);

                    if ((exprssValor == null) || (exprssValor.Elementos == null || (exprssValor.Elementos.Count < 1)))
                    {
                        return null;
                    }

                    tokensProcessed.Add(tokensOriginais[0]);
                    tokensProcessed.Add("[");
                    tokensProcessed.AddRange(exprssParametros[0].tokens);
                    tokensProcessed.Add("]");
                    tokensProcessed.Add("=");
                    tokensProcessed.AddRange(exprssValor.tokens);

                    if (tokensOriginais.IndexOf(";") != -1)
                    {
                        tokensProcessed.Add(";");
                    }


                    // m[1]=5 --- > m.setElement(1,5);
                    List<string> tokens_retorno = new List<string>();
                    tokens_retorno.Add(obj_caller.GetNome());
                    tokens_retorno.Add(".");
                    tokens_retorno.Add("SetElement");
                    tokens_retorno.Add("(");
                    tokens_retorno.AddRange(exprssParametros[0].tokens);
                    tokens_retorno.Add(",");
                    tokens_retorno.AddRange(exprssValor.tokens);
                    tokens_retorno.Add(")");
                    tokens_retorno.Add(";");


                    return tokens_retorno;


                }
            }
            return null;
        }




        /// <summary>
        /// converte uma expressao em anotação Wrapper, para uma chamada de método.
        /// </summary>
        /// <param name="exprssNotacaoWrapper">expressao Wrapper, p. ex: M[1,1] é expressão para WrapperData matriz,
        /// b[1] é uma anotação wrapper para vector get element.</param>
        /// <param name="escopo">contexto da expressao Wrapper</param>
        /// <returns>retorna uma chamada de método, que subsitui a expressão em anotação wrapper.</returns>
        public override List<string> GETChamadaDeMetodo(ref List<string> tokens_expressao, Escopo escopo, List<string> tokensProcessed)
        {

            if ((tokens_expressao != null) && (tokens_expressao.Count > 0))
            {

                List<string> tokensOriginais = tokens_expressao.ToList();

                string nomeObjeto = this.GetNameOfFirstObjectWrapper(escopo, Utils.OneLineTokens(tokens_expressao));
                if (nomeObjeto == null)
                {
                    return null;
                }


                Objeto obj_caller = escopo.tabela.GetObjeto(nomeObjeto, escopo);

                if ((obj_caller != null) && (obj_caller.GetTipo().Equals("Vector")))
                {




                    int indexBeginParametros = tokens_expressao.IndexOf("[");

                    // obtem os tokens que formam os parãmetros da chamada de metodo.
                    // permite que ademais tokens não anotação wrapper, sejam processados fora do wrapper
                    List<string> tokens_parametros = UtilTokens.GetCodigoEntreOperadores(indexBeginParametros, "[", "]", tokens_expressao);

                    if ((tokens_parametros == null) || (tokens_parametros.Count < 0))
                    {
                        return null;
                    }

                    tokens_parametros.RemoveAt(0);
                    tokens_parametros.RemoveAt(tokens_parametros.Count - 1);

                    List<Expressao> expressaoIndice = Expressao.ExtraiExpressoes(tokens_parametros, escopo);
                    try
                    {
                        if (expressaoIndice[0].Elementos[0].tipoDaExpressao != "int")
                        {
                            UtilTokens.WriteAErrorMensage("error in extract index of a vector object ", tokensOriginais, escopo);
                            return null;
                        }
                    }
                    catch (Exception e)
                    {
                        UtilTokens.WriteAErrorMensage("error in extract index of a vector object:  " + e.Message, tokensOriginais, escopo);
                        return null;
                    }
                    int indexNomeObjeto = tokens_expressao.IndexOf(obj_caller.GetNome());

                    // forma os tokens processado.
                    tokensProcessed.Add(tokens_expressao[indexNomeObjeto]);
                    tokensProcessed.Add("[");
                    tokensProcessed.AddRange(expressaoIndice[0].tokens);
                    tokensProcessed.Add("]");


                    // compoe os tokens da expressao de retorno (chamada de metodo GetElement).
                    List<string> tokens_retorno = new List<string>();
                    tokens_retorno.Add(obj_caller.GetNome());
                    tokens_retorno.Add(".");
                    tokens_retorno.Add("GetElement");
                    tokens_retorno.Add("(");
                    tokens_retorno.AddRange(expressaoIndice[0].tokens);
                    tokens_retorno.Add(")");

                    if (tokensOriginais.IndexOf(";") != -1)
                    {
                        tokensProcessed.Add(";");
                        tokens_retorno.Add(";");
                    }


                    return tokens_retorno;

                }
            }

            return null;


        }







        // tenta reconhecer o acesso a um elemento do wrapper data.
        private static bool MatchElement(string[] tokens_expressao, Regex umaRegex)
        {
            if ((tokens_expressao != null) && (tokens_expressao.Length > 0))
            {
                string textExpression = UtilTokens.FormataEntrada(Utils.OneLineTokens(tokens_expressao.ToList<string>()));


                if (umaRegex.IsMatch(textExpression))
                {
                    return true;
                }
                else
                    return false;
            }


            else
                return false;
        }

        public override bool IsInstantiateWrapperData(List<string> str_exprss)
        {
            if (MatchElement(str_exprss.ToArray(), regexInstanciacaoResumidaRigor))
            {
                return true;
            }
            else
            if (MatchElement(str_exprss.ToArray(), regexInstanciacaoResumidaClassica))
            {
                return true;
            }

            return false;
        }

        public override bool isWrapper(string tipoObjeto)
        {
            return tipoObjeto.Equals("Vector");
        }



        /// <summary>
        /// casting entre um object e um Vector.
        /// </summary>
        /// <param name="objtFromCasting">object contendo o conteudo do casting.</param>
        /// <param name="objToReceiveCast">Objeto a receber o casting.</param>
        public override bool Casting(object objtFromCasting, Objeto objToReceiveCast)
        {
            if ((objtFromCasting == null) || (objToReceiveCast == null))
            {
                return false;
            }


            if ((objtFromCasting.GetType() == typeof(Vector)) && (objToReceiveCast.valor.GetType() == typeof(Vector))) 
            {
                Vector vtFrom = (Vector)objToReceiveCast.valor;
                vtFrom.Casting(objtFromCasting);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// obtem nomes de identificam uma instanciacao wrapper object.
        /// </summary>
        /// <returns></returns>
        public override List<string> getNamesIDWrapperData()
        {
            return new List<string>() { "Vector", "[ ]" };
        }


        public new class Testes : SuiteClasseTestes
        {
            public Testes() : base("testes wrapper vector")
            {
            }


            public void TesteCasting(AssercaoSuiteClasse assercao)
            {

                Vector v1= new Vector();
                v1.nome = "v1";

                Vector v2 = new Vector();
                v2.nome = "v2";
                v2.SetElement(0, "1");

                object objSim = (object)v2;

                WrapperDataVector wrapperData = new WrapperDataVector();
                wrapperData.Casting(objSim, v1);

                assercao.IsTrue(v1.GetElement(0).ToString() == "1");
            }

            public void TesteCreateChamadaDeMetodo(AssercaoSuiteClasse assercao)
            {
                // id[] id[exprs]
                string exprssInstanciacao = "int[] vetor1[20]";
                Escopo escopo = new Escopo(exprssInstanciacao);
                WrapperDataVector wrapper = new WrapperDataVector();
                List<string> tokensChamadaCreate = wrapper.Create(ref exprssInstanciacao, escopo);

                assercao.IsTrue(tokensChamadaCreate != null && tokensChamadaCreate.Contains("Create"));



                string exprssInstanciacao2 = "Vector m[20]";
                Escopo escopo2 = new Escopo(exprssInstanciacao2);
                WrapperDataVector wrapper2 = new WrapperDataVector();
                List<string> tokensChamadaCreate2 = wrapper2.Create(ref exprssInstanciacao2, escopo);

                assercao.IsTrue(tokensChamadaCreate2 != null && tokensChamadaCreate2.Contains("Create"));

            }






        }

    }



}

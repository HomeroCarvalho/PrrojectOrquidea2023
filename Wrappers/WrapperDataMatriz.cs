using parser;
using parser.textoFormatado;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wrappers.DataStructures;

namespace Wrappers
{
    public class WrapperDataMatriz : WrapperData
    {

 
        private string patternRigor = "Matriz id [exprss, exprss]";
      
        private Regex regexRigor;
    

        // anotações wrappers data;
        // var m=[exprss, exprss] (instanciacao)
        // m[exprss, exprss] (getElement)
        // m[exprss, exprss]= valor (setElement).


        public WrapperDataMatriz()
        {

            // constroi a string da expressao regex.
            TextExpression textRegex = new TextExpression();

            // pattern com tipagem.
            string textPatternRigor = textRegex.FormaExpressaoRegularGenerica(patternRigor);                    
         

            // instancia a regex para instanciacao tipada.
            regexRigor = new Regex(textPatternRigor);
          
            this.tipo = "Matriz";
        }


        /// <summary>
        /// retorna o nome do objeto wrapper, contido numa definicao.
        /// </summary>
        /// <param name="tokens">tokens que contem a definicao de um wrapper object.</param>
        /// <returns></returns>
        public override string GetNameWrapperObject(List<string> tokens, int index)
        {

            // "rigor: Matriz id [exprss, exprss]" ---> Matriz id (como parametro).
            int indexTypeData = tokens.IndexOf("Matriz", index);
            if ((indexTypeData >= 0) && (indexTypeData + 1 < tokens.Count)) 
            {
                return tokens[indexTypeData + 1];
            }
            else
            {
                return null;
            }


        }




        /// <summary>
        /// retorna true se há definicao de um wrapper data matriz.
        /// </summary>
        /// <param name="tokens">tokens que contem a definicao.</param>
        /// <returns></returns>
        public override List<string> isThisTypeWrapperParameter(List<string> tokens, int index)
        {
            // rigor = "Matriz id [exprss, exprss]"  ---> como parametro: Matriz id.
            if ((index + 1 < tokens.Count) && (tokens[index] == "Matriz"))
            {
                List<string> tokensWrapper = new List<string>();
                tokensWrapper = tokens.GetRange(index, 2);
                
                return tokensWrapper;
            }
            else
            {
                return null;
            }
        }



        /// <summary>
        /// obtem o tipo de elemento de uma matriz, na sua definicao de criação.
        /// </summary>
        /// <param name="tokens">tokens em que está a definicao de criação da matriz.</param>
        /// <param name="countTokensWrapper">contador de tokens utilizados na definicao.</param>
        /// <returns>retorna o tipo de elemento, e o contador dos tokens utilizados na definição.</returns>
        public override string GetTipoElemento(List<string> tokens, ref int countTokensWrapper)
        {
            countTokensWrapper = 2;
            return "double";
            
        }




        /// <summary>
        /// /cria uma chamada de metodo estatica, que instanciara um objeto matriz.
        /// </summary>
        /// <param name="exprssInstanciacaoEmNotacaoWrapper">expressao wrapper de instanciacao</param>
        /// <param name="escopo">contexto onde esta a expressao.</param>
        /// <returns>retorna os tokens da chamada de metodo.</returns>
        public override List<string> Create(ref string exprssInstanciacaoEmNotacaoWrapper, Escopo escopo)
        {
            string[] exprss = new Tokens(exprssInstanciacaoEmNotacaoWrapper).GetTokens().ToArray();

            List<string> tokensOriginais = new Tokens(exprssInstanciacaoEmNotacaoWrapper).GetTokens();
            if ((tokensOriginais == null) || (tokensOriginais.Count == 0))
            {
                return null;
            }
            for (int i = 0; i < tokensOriginais.Count; i++)
            {
                tokensOriginais[i] = tokensOriginais[i].Replace("number", "exprss");
            }
            if (exprss[0] == "Matriz")
            {
                tokensOriginais[0] = "Matriz";
            }

            if (!MatchElement(tokensOriginais.ToArray(), this.regexRigor)) 
            {
                return null;
            }



            string nomeObjeto;


            if (!exprss[0].Equals("Matriz"))
            {
                //  template:  "typeItem [ , ] id = [ exprss , exprss ]";
                if ((exprss.Length >= 10) && (TextExpression.IsID(exprss[0]) && (exprss[1] == "[") && (exprss[2] == ",") &&
                    (exprss[3] == "]")) && (TextExpression.IsID(exprss[4]) && (exprss[5] == "=") && (exprss[6] == "[")))
                {
                    string tipoElemento = exprss[0];                // tipo do elemento da matriz.
                    nomeObjeto = exprss[4];                         // nome do objeto instanciado.

                    List<string> tokens = exprss.ToList<string>();
                    int indexOperadorAbre = tokens.IndexOf("[", 2); // captura o indice do segundo operador abre.
                    List<string> parametrosDims = exprss.ToList<string>().GetRange(indexOperadorAbre, tokens.Count - indexOperadorAbre);

                    if ((parametrosDims == null) || (parametrosDims.Count < 2))
                    {
                        return null;
                    }
                    else
                    {
                        parametrosDims.RemoveAt(0);
                        parametrosDims.RemoveAt(parametrosDims.Count - 1);

                        List<Expressao> expssDims = Expressao.ExtraiExpressoes(parametrosDims, escopo);

                        try
                        {
                            if ((expssDims[1].Elementos[0].tipoDaExpressao != "int") ||
                                (expssDims[0].Elementos[0].tipoDaExpressao != "int"))
                            {
                                UtilTokens.WriteAErrorMensage("error in extract indexes for matriz object ", tokensOriginais, escopo);
                                return null;
                            }
                        }
                        catch (Exception e)
                        {
                            UtilTokens.WriteAErrorMensage("erro in matriz create, index invalid: " + e.Message, tokensOriginais, escopo);
                            return null;
                        }




                        List<string> tokensRetorno = new List<string>();
                        tokensRetorno.Add(nomeObjeto);
                        tokensRetorno.Add(".");
                        tokensRetorno.Add("Create");
                        tokensRetorno.Add("(");
                        tokensRetorno.AddRange(expssDims[0].tokens);
                        tokensRetorno.Add(",");
                        tokensRetorno.AddRange(expssDims[0].tokens);
                        tokensRetorno.Add(")");
                        if (exprssInstanciacaoEmNotacaoWrapper.IndexOf(";") > -1)
                        {
                            tokensRetorno.Add(";");
                        }


                        // instancia um objeto Matriz.
                        Matriz mtObj = new Matriz();
                        mtObj.SetNome(nomeObjeto);
                        mtObj.isWrapperObject = true;
                        mtObj.tipoElemento = "double";


                        // registra o objeto instanciado, em tempo de compilacao, para na compilacao de expressoes, ser identificado.
                        escopo.tabela.GetObjetos().Add(mtObj);


                        // extrai os codigos consumidos na anotação wrapper data.
                        exprssInstanciacaoEmNotacaoWrapper = WrapperData.ExtraiTokensConsumido(
                            exprss.ToList<string>(), exprss.ToList<string>(), tokensRetorno);


                        return tokensRetorno;


                    }


                }
            }

            if (exprss[0] == "Matriz")
            {


                if (exprss.Length < 5)
                {
                    return null;
                }


                // cria e instancia o objeto matriz, no escopo onde está a anotação.
                nomeObjeto = exprss[1];




                List<string> tokensInstiate = exprss.ToList<string>();

                tokensInstiate.RemoveAt(0); // remove o identificador  de tipo "Matriz".
                tokensInstiate.RemoveAt(0); // remove o nome do objeto.



                int indexBeginFirstParameters = tokensInstiate.IndexOf("[");
                if (indexBeginFirstParameters == -1)
                    return null;



                List<string> tokensParams = UtilTokens.GetCodigoEntreOperadores(indexBeginFirstParameters, "[", "]", tokensInstiate);
                if ((tokensParams == null) || (tokensParams.Count == 0))
                    return null;

                tokensParams.RemoveAt(0);
                tokensParams.RemoveAt(tokensParams.Count - 1);

                // extrai as expressoes de indices.
                List<Expressao> exprssLinesCols = Expressao.ExtraiExpressoes(tokensParams, escopo);
                try
                {
                    if ((exprssLinesCols[0].Elementos[0].tipoDaExpressao != "int") ||
                        (exprssLinesCols[1].Elementos[0].tipoDaExpressao != "int"))
                    {
                        UtilTokens.WriteAErrorMensage("error in extract indexes to matriz object", tokensOriginais, escopo);
                        return null;
                    }
                }
                catch
                {
                    UtilTokens.WriteAErrorMensage("error in extract indexes to matriz object", tokensOriginais, escopo);
                    return null;

                }



                if ((exprssLinesCols == null) || (exprssLinesCols.Count < 2) || (exprssLinesCols[0].Elementos == null)
                    || (exprssLinesCols[0].Elementos.Count == 0))
                {
                    return null;
                }

                List<string> tokensRetorno = new List<string>() { nomeObjeto, ".", "Create", "(" };
                tokensRetorno.AddRange(exprssLinesCols[0].tokens);
                tokensRetorno.Add(",");
                tokensRetorno.AddRange(exprssLinesCols[0].tokens);
                tokensRetorno.Add(")");
                if (exprssInstanciacaoEmNotacaoWrapper.IndexOf(";") > -1)
                {
                    tokensRetorno.Add(";");
                }

                // instancia um objeto Matriz.
                Matriz mtObj = new Matriz();
                mtObj.SetNome(nomeObjeto);
                mtObj.isWrapperObject = true;


                // registra o objeto no escopo.
                escopo.tabela.GetObjetos().Add(mtObj);


                // extrai os codigos consumidos na anotação wrapper data.
                exprssInstanciacaoEmNotacaoWrapper = WrapperData.ExtraiTokensConsumido(
                    exprss.ToList<string>(), exprss.ToList<string>(), tokensRetorno);


                return tokensRetorno;

            }

            return null;
        }


        /// <summary>
        /// converte uma anotação wrapper para uma expressao chamada de metodo matriz.GetElement(lin,col).
        /// ex.: m[1,1] (anotação wrapper), para m.GetElement(1,1).
        /// </summary>
        /// <param name="exprssEmNotacaoWrapper">anotação wrapper contendo os dados de setar elemento da matriz.</param>
        /// <param name="escopo">contexto onde a anotação está.</param>
        /// <returns></returns>
        public override List<string> GETChamadaDeMetodo(ref List<string> tokensAnotacaoWrapper, Escopo escopo, List<string> tokensProcessed)
        {
        
            List<string> tokensOriginal= tokensAnotacaoWrapper.ToList();

            // tenta encontrar o nome do primeiro objeto wrapper da notacao parametro.
            string nomeObjeto = this.GetNameOfFirstObjectWrapper(escopo, Utils.OneLineTokens(tokensAnotacaoWrapper));
            if (nomeObjeto==null)
            {
                return null;
            }
         
            Objeto obj_caller = escopo.tabela.GetObjeto(nomeObjeto, escopo);
            if (obj_caller == null)
            {
                return null;
            }


            int indexObjeto = tokensAnotacaoWrapper.IndexOf(obj_caller.GetNome());
            int indexSignalEquals = tokensAnotacaoWrapper.IndexOf("=");


            // verifica se é mesmo um set/get element: m=b[1,1], ou x=b[1,5].
            if (indexObjeto < indexSignalEquals)
            {
                return null;
            }

        

            int indexBeginParameters = tokensAnotacaoWrapper.IndexOf("[");
            if (indexBeginParameters == -1)
            {
                return null;
            }

            int indexEndParameters = tokensAnotacaoWrapper.IndexOf("]");
            if (indexEndParameters == -1)
            {
                return null;
            }


            List<string> tokensIndex = UtilTokens.GetCodigoEntreOperadores(indexBeginParameters, "[", "]", tokensAnotacaoWrapper);
            if ((tokensIndex == null) || (tokensIndex.Count < 2))
            {
                return null;
            }
            // remove os operadores de matriz: [,].
            tokensIndex.RemoveAt(0);
            tokensIndex.RemoveAt(tokensIndex.Count-1);

            try
            {


                List<Expressao> exprssIndices = Expressao.ExtraiExpressoes(tokensIndex, escopo);
                try
                {
                    if ((exprssIndices == null) || (exprssIndices.Count < 2) ||
                        (exprssIndices[0].Elementos[0].tipoDaExpressao != "int") ||
                        (exprssIndices[1].Elementos[0].tipoDaExpressao != "int"))
                    {
                        UtilTokens.WriteAErrorMensage("error in get element matriz, indexes invalid: ", tokensOriginal, escopo);
                        return null;
                    }
                }
                catch (Exception e)
                {
                    UtilTokens.WriteAErrorMensage("error in get element matriz, indexes invalid: " + e.Message, tokensOriginal, escopo);
                    return null;
                }

                // ex.: m[1,1].
                tokensProcessed.Add(nomeObjeto); // adiciona o token nome do objeto, para a lista de tokens processado.
                tokensProcessed.Add("[");
                tokensProcessed.AddRange(exprssIndices[0].tokens);
                tokensProcessed.Add(",");
                tokensProcessed.AddRange(exprssIndices[0].tokens);
                tokensProcessed.Add("]");
        
       
                /// ex.: m.GetElement(1, 1). 
                List<string> tokens_retorno = new List<string>();
                tokens_retorno.Add(nomeObjeto);
                tokens_retorno.Add(".");
                tokens_retorno.Add("GetElement");
                tokens_retorno.Add("(");
                tokens_retorno.AddRange(exprssIndices[0].tokens);
                tokens_retorno.Add(",");
                tokens_retorno.AddRange(exprssIndices[1].tokens);
                tokens_retorno.Add(")");


                return tokens_retorno;
            }
            catch
            {
                escopo.GetMsgErros().Add("erro no processamento de indices/valor de um objeto matriz.");
            }
            return null;
        }



        /// <summary>
        /// converte uma anotação wrapper, para uma chamada de metodo para o metodo matriz.SetElement(linha,coluna).
        /// ex.; m[1,1]=5 , para m.SetElement(1,1,5).
        /// </summary>
        /// <param name="exprssEmNotacaoWrapper">anotação wrapper contendo os dados da chamada de metodo.</param>
        /// <param name="escopo">contexto onde a anotação wrapper está.</param>
        /// <returns></returns>
        public override List<string> SETChamadaDeMetodo(ref List<string> tokens, Escopo escopo, List<string> tokensProcessed)
        {
       

            // anotação wrapper; m[exprss,exprss]
            List<string> tokensOriginal = tokens.ToList();


            // obtem o nome do objeto.
            string nomeObjeto = tokens[0];

            // valida se o objeto matriz está contido no escopo.
            Objeto obj_caller = escopo.tabela.GetObjeto(nomeObjeto, escopo);
            if ((obj_caller == null) || (!obj_caller.isWrapperObject))
            {
                return null;
            }


            // valida se é uma expressao wrapper de setar elemento.
            int indexBeginParameters = tokens.IndexOf("[");
            if (indexBeginParameters == -1)
            {
                return null;
            }

            int indiceNomeObjeto = tokens.IndexOf(nomeObjeto);
            int indiceOperadorIgual = tokens.IndexOf("=");
            if ((indiceOperadorIgual == -1) || (indiceNomeObjeto> indiceOperadorIgual))
            {
                return null;
            }


            // obtem os tokens contendo expressoes de indices.
            List<string> tokensIndicesExprss = UtilTokens.GetCodigoEntreOperadores(indexBeginParameters, "[", "]", tokens);
            if ((tokensIndicesExprss == null) || (tokensIndicesExprss.Count == 0))
            {
                return null;
            }

            // retira os tokens dos operadores "[" , "]".
            tokensIndicesExprss.RemoveAt(0);
            tokensIndicesExprss.RemoveAt(tokensIndicesExprss.Count - 1);

            // extrai os indices do elemento [lin,col].
            List<Expressao> expressoesIndices = Expressao.ExtraiExpressoes(tokensIndicesExprss, escopo);
            try
            {
                if ((expressoesIndices[0].Elementos[0].tipoDaExpressao != "int") ||
                    (expressoesIndices[1].Elementos[0].tipoDaExpressao != "int"))
                {
                    UtilTokens.WriteAErrorMensage("error in indexes to set element matriz", tokensOriginal, escopo);
                    return null;
                }
            }
            catch(Exception e)
            {
                UtilTokens.WriteAErrorMensage("error in indexes to set element matriz: " + e.Message, tokensOriginal, escopo);
                return null;
            }



            // constroi a expressao que contem o valor a ser atribuido ao SetElement.
            List<string> tokensValor = tokens.GetRange(indiceOperadorIgual + 1, tokens.Count - (indiceOperadorIgual + 1));
            Expressao exprssValor = new Expressao(tokensValor.ToArray(), escopo);


            /// ex.: M[1,4]=5 ---> M.SetElement(1,4,5)
            List<string> tokens_retorno = new List<string>();
            tokens_retorno.Add(nomeObjeto);
            tokens_retorno.Add(".");
            tokens_retorno.Add("SetElement");
            tokens_retorno.Add("(");
            tokens_retorno.AddRange(expressoesIndices[0].tokens);
            tokens_retorno.Add(",");
            tokens_retorno.AddRange(expressoesIndices[1].tokens);
            tokens_retorno.Add(",");
            tokens_retorno.AddRange(exprssValor.tokens);
            tokens_retorno.Add(")");

            if (tokens.IndexOf(";") != -1)
            {
                tokens_retorno.Add(";");
            }

            // constroi a lista de tokens processado:  ex.: M[1,4]
            tokensProcessed.Add(nomeObjeto);
            tokensProcessed.Add("[");
            tokensProcessed.AddRange(expressoesIndices[0].tokens);
            tokensProcessed.Add(",");
            tokensProcessed.AddRange(expressoesIndices[0].tokens);
            tokensProcessed.Add("]");
            tokensProcessed.Add("=");
            tokensProcessed.AddRange(exprssValor.tokens); // adiciona os tokens da expressao de valor a se atribuido, para a lista de tokens processado.

            if (tokens.IndexOf(";") != -1)
            {
                tokensProcessed.Add(";");
            }


            return tokens_retorno;

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
            return MatchElement(str_exprss.ToArray(), regexRigor);
           
        }


        public override bool isWrapper(string tipoObjeto)
        {
            return tipoObjeto.Equals("Matriz");
        }


        /// <summary>
        /// casting entre um object e um Matriz.
        /// </summary>
        /// <param name="objtFromCasting">object contendo o valor do casting.</param>
        /// <param name="ObjToReceiveCast">objeto a receber o valor.</param>
        public override bool Casting(object objtFromCasting, Objeto ObjToReceiveCast)
        {
            if ((objtFromCasting == null) || (ObjToReceiveCast == null)) 
            {
                return false;
            }
            if ((objtFromCasting.GetType() == typeof(Matriz)) && (ObjToReceiveCast.valor.GetType() == typeof(Matriz))) 
            {
                Matriz mtObj = (Matriz)ObjToReceiveCast.valor;
                mtObj.Casting(objtFromCasting);

                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// obtem nomes que identificam uma instanciacao de matriz.
        /// </summary>
        /// <returns></returns>
        public override List<string> getNamesIDWrapperData()
        {
            return new List<string>() { "Matriz" };
        }

        public new class Testes : SuiteClasseTestes
        {
            public Testes() : base("testes para classe WrapperDataMatriz")
            {

            }

            public void TestesChamadaDeMetodoCreate(AssercaoSuiteClasse assercao)
            {
                // "typeItem [ , ] id = [ exprss , exprss ]"
                string expressaoCreate1 = "int [,] mt_1= [20,20]";
                Escopo escopo1 = new Escopo(expressaoCreate1);

                WrapperDataMatriz dataMatriz= new WrapperDataMatriz();
                List<string> tokensCreate = dataMatriz.Create(ref expressaoCreate1, escopo1);
                assercao.IsTrue(tokensCreate != null && tokensCreate.Count > 0 && tokensCreate.Contains("Create"));



                // "Matriz id [ exprss, exprss ]"

                string expressaoCreate2 = "Matriz mt_1 [ 5, 4 ]";
                Escopo escopo2= new Escopo(expressaoCreate2);

                WrapperDataMatriz dataMatriz2= new WrapperDataMatriz();
                List<string> tokensCreate2= dataMatriz2.Create(ref expressaoCreate2, escopo2);
                assercao.IsTrue(tokensCreate2 != null && tokensCreate2.Count > 0 && tokensCreate2.Contains("Create"));


            }


         

            
            public void ObtemChamadasDeMetodo(AssercaoSuiteClasse assercao)
            {
                string codigo_Get = "m[1,1]";
                string codigo_Set = "m[1,1]=5;";

                List<string> tokensGet = new Tokens(codigo_Get).GetTokens();
                List<string> tokensSet = new Tokens(codigo_Set).GetTokens();

                Matriz objMatriz = new Matriz(3, 3);
                objMatriz.SetNome("m");

                Escopo escopo= new Escopo(codigo_Get);
                escopo.tabela.RegistraObjeto(objMatriz);

                WrapperDataMatriz wrapper = new WrapperDataMatriz();

                List<string> tokensProcessed = new List<string>();
                
                List<string> exprssGET = wrapper.GETChamadaDeMetodo(ref tokensGet, escopo, tokensProcessed);
                List<string> exprssSET = wrapper.SETChamadaDeMetodo(ref tokensSet, escopo, tokensProcessed);

                UtilTokens.PrintTokens(exprssGET, "tokens chamada de metodo get element: ");
                UtilTokens.PrintTokens(exprssSET, "tokens chamada de metodo set element: ");
                UtilTokens.PrintTokens(tokensProcessed, "tokens anotação wrapper retirado.");



                assercao.IsTrue(exprssGET != null, "validacao de chamada de metodo Get.");
                assercao.IsTrue(exprssSET != null, "validacao de chamada de metodo Set.");


              

            }

        }
    }
}

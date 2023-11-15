using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using parser;
using parser.textoFormatado;
using Wrappers.DataStructures;
namespace Wrappers
{
    public class WrapperDataDictionaryText : WrapperData
    {
       
        /// <summary>
        /// pattern resumed tipado para instanciacao wrapper.
        /// </summary>
        private string patternRigor = "DictionaryText id = { id }";
        /// <summary>
        /// regex para pattern rigor.
        /// </summary>
        private Regex regexRigor;                                       

        //string str_getElement = "id {exprss}"; // pattern resumed para obter um elemento, em anotação wrapper.
        //string str_setElement = "id {exprss, exprss}"; // pattern resumed para setar um elemento, em anotação wrapper.

        // anotação wrapper instanciacao: var m= {:}
        // anotação wrapper getElement {key}.
        // anotação wrapper setElement {key, value}.


        public WrapperDataDictionaryText()
        {
            // constroi a string da expressao regex.
            TextExpression textRegex = new TextExpression();
            string textPatternRigor = textRegex.FormaExpressaoRegularGenerica(patternRigor);
            regexRigor = new Regex(textPatternRigor);

            this.tipo = "DictionaryText";
        }

        /// <summary>
        /// obtem o tipo de elemento do dictionary text, e o contador de tokens utilizados na definição.
        /// </summary>
        /// <param name="tokens">tokens contendo a definição do wrapper object.</param>
        /// <param name="countTokensWrapper">contador de tokens utilizados na instanciacao.</param>
        /// <returns></returns>
        public override string GetTipoElemento(List<string> tokens, ref int countTokensWrapper)
        {
            ////DictionaryText id  { id } ---> DictionaryText id  { id } (como parametro).
            int indexTypeData = tokens.IndexOf("DictionaryText");
            if (indexTypeData != -1) 
            {
                countTokensWrapper = 5;
                return tokens[indexTypeData + 3];
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// obtem o nome do wrapper data object, dentro de tokens de definição de objeto.
        /// </summary>
        /// <param name="tokens">tokens contendo a definicao do wrapper data object.</param>
        /// <returns></returns>
        public override string GetNameWrapperObject(List<string> tokens, int index)
        {
            // DictionaryText id = { id }   -----> como parametro: DictionaryText id { id } 
            int indexTypeData = tokens.IndexOf("DictionaryText", index);
            if ((indexTypeData >= 0) && (indexTypeData + 4 < tokens.Count)) 
            {
                return tokens[indexTypeData + 4];
            }
            else
            {
                return null;
            }
           
        }



        /// <summary>
        /// retorna true, se há definição de um dictionary text wrapper object.
        /// </summary>
        /// <param name="tokens">tokens contendo a definicao do wrapper objetc.</param>
        /// <returns></returns>
        public override List<string> isThisTypeWrapperParameter(List<string> tokens, int index)
        {
            // DictionaryText id = { id } ---> DictionaryText id { id };
            if ((index>=0) && (index<tokens.Count))
            {
   
                if ((index + 5 < tokens.Count) && (tokens[index] == "DictionaryText")) 
                {
                    List<string> tokensWrapper = new List<string>();
                    tokensWrapper.AddRange(tokens.GetRange(index, 5));
                    return tokensWrapper;
                }
                

            }

            return null;

        }




        /// <summary>
        /// cria uma chamada de metodo estatica, para instanciar um objeto wrapper [DictionaryText].
        /// </summary>
        /// <param name="exprssInstanciacaoEmNotacaoWrapper">expressao wrapper da instanciacao.</param>
        /// <param name="escopo">contexto onde a expressao esta.</param>
        /// <returns>retorna uma lista de tokens da chamada de metodo estatica de instanciacao.</returns>
        public override List<string> Create(ref string exprssInstanciacaoEmNotacaoWrapper, Escopo escopo)
        {
            string[] str_expressao = new Tokens(exprssInstanciacaoEmNotacaoWrapper).GetTokens().ToArray();

            string pattern = new TextExpression().FormaPatternResumed(Utils.OneLineTokens(str_expressao.ToList<string>()));

            List<string> tokensPattern = new Tokens(pattern).GetTokens();
            if ((tokensPattern == null) || (tokensPattern.Count == 0))
            {
                return null;
            }


            if (str_expressao[0] == "DictionaryText")
            {
                tokensPattern[0] = "DictionaryText";
            }

            if (MatchElement(tokensPattern.ToArray(), regexRigor))
            {
                //DictionaryText id = { id }
                // obtem o nome do objeto a ser instanciado.
                string nomeObjeto = str_expressao[1];
                string nomeTipoElemento = str_expressao[4];

                List<string> exprssRetorno = new List<string>();
                exprssRetorno.Add(nomeObjeto);
                exprssRetorno.Add(".");
                exprssRetorno.Add("Create");
                exprssRetorno.Add("(");
                exprssRetorno.Add(")");
                if (exprssInstanciacaoEmNotacaoWrapper.IndexOf(";") > -1)
                {
                    exprssRetorno.Add(";");
                }

                // instancia um objeto [DictionaryText].
                DictionaryText dictObj = new DictionaryText();
                dictObj.SetNome(nomeObjeto);
                dictObj.SetTipoElement(nomeTipoElemento);

                // registra o objeto em tempo de compilacao, para que possa fazer validacoes de expressoes, e ser idenficado neste processamento.
                escopo.tabela.GetObjetos().Add(dictObj);

                return exprssRetorno;
            }
            else
            {
                return null;
            }

        }


        /// <summary>
        /// constroi uma expressao chamada de metodo, para o metodo DictionaryText.GetElement(key).
        /// anotação wrapper: id {exprss}.
        /// </summary>
        /// <param name="exprssEmNotacaoWrapper">codigo em anotação wrapper.</param>
        /// <param name="escopo">contexto onde está a anotação wrapper.</param>
        /// <param name="tokensProcessed">tokens consumido na anotacao wrapper.</param>
        /// <returns></returns>
        public override List<string> GETChamadaDeMetodo(ref List<string> tokensGet, Escopo escopo, List<string> tokensProcessed)
        {
       
            List<string> tokensOriginal= tokensGet.ToList();

            if ((tokensGet == null) || (tokensGet.Count == 0))
            {
                return null;
            }
                

            if (tokensGet.Count < 4)
            {
                return null;
            }
                


            string nomeObjeto = this.GetNameOfFirstObjectWrapper(escopo, Utils.OneLineTokens(tokensGet));
            if (nomeObjeto == null)
            {
                return null;
            }


            Objeto objWrapper = escopo.tabela.GetObjeto(nomeObjeto, escopo);

        

            if (!objWrapper.GetTipo().Equals("DictionaryText"))
            {
                return null;
            }
                




            int indexBeginParametros = tokensGet.IndexOf("{");
            if (indexBeginParametros == -1)
            {
                return null;
            }
                

            List<string> tokensParametros = UtilTokens.GetCodigoEntreOperadores(indexBeginParametros, "{", "}", tokensGet);
            if ((tokensParametros == null) || (tokensParametros.Count == 0))
            {
                return null;
            }
                

            tokensParametros.RemoveAt(0);
            tokensParametros.RemoveAt(tokensParametros.Count - 1);

            if (tokensParametros.Count != 1)
            {
                return null;
            }

         
            tokensProcessed.Add(nomeObjeto); // adiciona o token do nome do objeto, para a lista de tokens processado.
            tokensProcessed.Add("{");
            tokensProcessed.AddRange(tokensParametros); // adiciona os tokens dos parametros, para a lista de tokens processado.
            tokensProcessed.Add("}");


            // constroi a lista de tokens da chamada de metodo a ser feita.
            List<string> tokens_retorno = new List<string>();
            tokens_retorno.Add(nomeObjeto);
            tokens_retorno.Add(".");
            tokens_retorno.Add("GetElement");
            tokens_retorno.Add("(");
            tokens_retorno.AddRange(tokensParametros);
            tokens_retorno.Add(")");


            return tokens_retorno;


        }

        /// <summary>
        /// constroi uma expressao chamada de metodo, para o metodo DictionaryText.SetElement(key,value).
        /// anotaçao wrapper: id{key, value}
        /// </summary>
        /// <param name="exprssEmNotacaoWrapper">codigo em anotação wrapper, ex.: m{"animal","urso"}</param>
        /// <param name="escopo">contexto onde a anotação está.</param>
        /// <param name="tokensProcessed">tokens consumidos na anotacao wrapper.</param>
        /// <returns></returns>
        public override List<string> SETChamadaDeMetodo(ref List<string> tokens, Escopo escopo, List<string> tokensProcessed)
        {
            List<string> tokensOriginal= tokens.ToList();   


            // obtem o nome do objeto wrapper.
            string nomeObjeto = tokens[0];
            Objeto objWrapper = escopo.tabela.GetObjeto(nomeObjeto, escopo);
            if ((objWrapper == null) || (!objWrapper.GetTipo().Equals("DictionaryText"))) 
                return null;
           




            int indexSeparador = tokens.IndexOf(",");
            if (indexSeparador == -1)
                return null;

            int indexOperadorBracas= tokens.IndexOf("{");
            if (indexOperadorBracas == -1)
                return null;


            List<string> key;
            List<string> value;

            try
            {
                key = tokens.GetRange(indexOperadorBracas + 1, indexSeparador - indexOperadorBracas - 1);
                value = tokens.GetRange(indexSeparador + 1, tokens.Count - (indexSeparador + 1) - 1);

            }
            catch (Exception e)
            {
                UtilTokens.WriteAErrorMensage("error in set element to a wrapper object dictionary text. " + e.Message, tokensOriginal, escopo);
                return null;
            }

            // ex.: dict{key: value}
            tokensProcessed.Add(nomeObjeto);
            tokensProcessed.Add("{");
            tokensProcessed.AddRange(key);
            tokensProcessed.Add(",");
            tokensProcessed.AddRange(value);
            tokensProcessed.Add("}");


            // ex.: dict{key:value} ----> dict.SetElement(key, value)
            List<string> tokens_retorno = new List<string>();
            tokens_retorno.Add(nomeObjeto);
            tokens_retorno.Add(".");
            tokens_retorno.Add("SetElement");
            tokens_retorno.Add("(");
            tokens_retorno.AddRange(key);
            tokens_retorno.Add(",");
            tokens_retorno.AddRange(value);
            tokens_retorno.Add(")");

            if (tokens.IndexOf(";") != -1)
            {
                tokens_retorno.Add(";");
                tokensProcessed.Add(";");
            }


            return tokens_retorno;
                
        }




        /// <summary>
        /// tenta reconhecer o acesso a um elemento do wrapper data.
        /// </summary>
        /// <param name="tokens_expressao">tokens da expressão.</param>
        /// <param name="umaRegex">expressão regex.</param>
        /// <returns></returns>
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
            if (MatchElement(str_exprss.ToArray(), regexRigor))
            {
                return true;
            }
            return false;
        }


        public override bool isWrapper(string tipoObjeto)
        {
            return tipoObjeto.Equals("DictionaryText");
        }

      

        /// <summary>
        /// retorna true se a chamada de metodo for set element.
        /// </summary>
        /// <param name="nomeObjeto"></param>
        /// <param name="tokensNotacaoWrapper"></param>
        /// <returns></returns>
        public override bool IsSetElement(string nomeObjeto, List<string> tokensNotacaoWrapper)
        {
            return tokensNotacaoWrapper.IndexOf(",") != -1;
        }


        /// <summary>
        /// faz a conversao entre um object e um DictionaryText.
        /// </summary>
        /// <param name="objtFromCasting">object contendo o valor do casting.</param>
        /// <param name="ObjToReceiveCast">objeto a receber o casting.</param>
        public override bool Casting(object objtFromCasting, Objeto ObjToReceiveCast)
        {
            if ((objtFromCasting == null) || (ObjToReceiveCast == null))
            {
                return false;
            }

            if (ObjToReceiveCast.valor.GetType() == typeof(DictionaryText)) 
            {
                DictionaryText dict1 = (DictionaryText)ObjToReceiveCast.valor;
                dict1.Casting(objtFromCasting);

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// retorna nomes que identificam a instanciacao de um [DictionaryText]
        /// </summary>
        /// <returns></returns>
        public override List<string> getNamesIDWrapperData()
        {
            return new List<string>() { "DictionaryText" };
        }

        public new class Testes : SuiteClasseTestes
        {
            public Testes() : base("testes para wrapper de dictionary text.")
            {
            }

            
            public void TesteChamadaDeMetodoCreate(AssercaoSuiteClasse assercao)
            {
                string exprssInstanciacao = "DictionaryText dict1 = { string }";
                Escopo escopo = new Escopo(exprssInstanciacao);

                WrapperDataDictionaryText wrapperData= new WrapperDataDictionaryText();
                List<string> tokensCreate = wrapperData.Create(ref exprssInstanciacao, escopo);

                assercao.IsTrue(tokensCreate != null && tokensCreate.Count > 0 && tokensCreate.Contains("Create"));
            }

        
    
         
        }
    }
}

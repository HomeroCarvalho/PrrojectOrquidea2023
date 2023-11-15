using ParserLinguagemOrquidea.Wrappers;
using System.Collections.Generic;
using System.Linq;

using Wrappers;

namespace parser
{
    public class GerenciadorWrapper
    {
        List<WrapperData> wrappers;
        

        public GerenciadorWrapper()
        {
            this.wrappers = new List<WrapperData>();

            this.wrappers.Add(new WrapperDataDictionaryText());
            this.wrappers.Add(new WrapperDataVector());
            this.wrappers.Add(new WrapperDataMatriz());
            this.wrappers.Add(new WrapperDataJaggedArray());
            
        }


        /// <summary>
        /// instancia objetos wrapper estrutures, se forem objetos wrappers.
        /// </summary>
        /// <param name="tokensExpresssao">tokens contendo a instanciacao wrapper.</param>
        /// <param name="escopo">contexto onde os tokens estão.</param>
        /// <returns>[true] se os tokens são de uma instanciacao wrapper, [false] se  não.</returns>
        public List<string> IsToWrapperInstantiate(string[] tokensExpresssao, Escopo escopo)
        {
            for (int i = 0; i < wrappers.Count; i++)
            {
                // verifica se a expressão é uma instanciação de wrapper data objeto.
                if (wrappers[i].IsInstantiateWrapperData(tokensExpresssao.ToList<string>())) 
                {
                    string exprssCreate = Util.UtilString.UneLinhasLista(tokensExpresssao.ToList<string>());
                    // chamada a instanciação do wrapper data objeto, identificado.
                    List<string> tokensInstanciacao= wrappers[i].Create(ref exprssCreate, escopo);
                    return tokensInstanciacao;
                }
                
            }
            return null;
        }



        /// <summary>
        /// faz o processamento de anotação wrapper data, extraindo tokens GetElement, SetElement.
        /// </summary>
        /// <param name="tokensExpressao">tokens expressao wrapper resumido.</param>
        /// <param name="escopo">contexto onde a expressao wraper esta.</param>
        /// <param name="isFoundWrapperDataObject">retorna [true] se houve processamento wrapper.</param>
        /// <param name="tokensProcessed">tokens consumidos na chamada de metodo.</param>
        /// <returns>retorna tokens de uma chamada de metodo GetElement, ou SeElement.</returns>
        public List<string> ConverteParaTokensChamadaDeMetodo(List<string> tokensExpressao, Escopo escopo, ref bool isFoundWrapperDataObject, ref List<string> tokensProcessed)
        {
            isFoundWrapperDataObject = false;

            if ((tokensExpressao == null) || (tokensExpressao.Count == 0))
                return null;

            List<string> exprssTotalReturn = new List<string>();
            int i = 0;


            // procura por objetos de uma das classes WrapperData.
            while (i < tokensExpressao.Count)
            {
                string nomeObjeto = tokensExpressao[i];
                Objeto umObjeto = escopo.tabela.GetObjeto(nomeObjeto, escopo);

                bool isFoundWrapper = false;
                // o objeto é um wrapper data object, faz o processamento de chamadas GetElement, SetElement.
                if ((umObjeto != null) && (umObjeto.isWrapperObject))
                {
                    string tipoObjeto = umObjeto.GetTipo();

                    isFoundWrapper = false;


                    // verifica se os tokens é de uma chamada de metodo SET ELEMENT.
                    for (int x = 0; x < this.wrappers.Count; x++) 
                    {
                        if ((this.wrappers[x].isWrapper(tipoObjeto)) && (wrappers[x].IsSetElement(umObjeto.GetNome(), tokensExpressao)))
                        {
                            tokensProcessed = new List<string>();
                            string exprss = Util.UtilString.UneLinhasLista(tokensExpressao);
                            List<string> tokensSetElement = this.wrappers[x].SETChamadaDeMetodo(ref tokensExpressao, escopo, tokensProcessed);

                            if ((tokensSetElement != null) && (tokensSetElement.Count > 0))
                            {
                                // adiciona os tokens da chamada de metodo SET_Element.
                                exprssTotalReturn.AddRange(tokensSetElement);

                                return exprssTotalReturn;
                            }
                            else
                            {
                                continue;
                            }
                   
                        }
                    }


                    // verifica se os tokens é de uma chamada de metodo GET ELEMENT.
                    for (int x = 0; x < this.wrappers.Count; x++) 
                    {
                        if ((wrappers[x].isWrapper(tipoObjeto)) && (!wrappers[x].IsSetElement(umObjeto.GetNome(), tokensExpressao))) 
                        {

                            tokensProcessed = new List<string>();
                            // faz o processamento de tokens [GetElement].
                            List<string> tokensGetElement = this.wrappers[x].GETChamadaDeMetodo(ref tokensExpressao, escopo, tokensProcessed);

                            // se o tokens wrapper nao for do tipo wrapper currente, passa para o proximo wrapper data classe.
                            if ((tokensGetElement != null) && (tokensGetElement.Count > 0))
                            {
                                exprssTotalReturn.AddRange(tokensGetElement);
                                return exprssTotalReturn;
                            }
                            else
                            {
                                continue;

                            }
                           




                        }


                    }
                }
                // se nao for nome de objeto wrapper,adiciona a lista de tokens processados.
                if (!isFoundWrapper)
                {
                    exprssTotalReturn.Add(tokensExpressao[i]);
                }



                // avança a variavel de malha de tokens.
                i += 1; 
            } // while i



            // se houve anotação wrapper data feita, retorna este processamento.
            if ((exprssTotalReturn != null) && (exprssTotalReturn.Count > 0))
            {
                tokensExpressao = exprssTotalReturn.ToList();
                return exprssTotalReturn;
            }
            else
            {
                // se nao houve processamento, retorna os tokens da expressão parâmetro.
                return tokensExpressao;
            }
            
        }

    }


 


} 

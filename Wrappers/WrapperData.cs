using ParserLinguagemOrquidea.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wrappers;
using Wrappers.DataStructures;

namespace parser
{


    /// <summary>
    /// classe base para tipos de WrappersData. Provê métodos que deverão ser implementados
    /// para que uma estrutura de dados Wrapper seja funcional.
    /// </summary>
    public abstract class WrapperData:Objeto
    {


     

        /// <summary>
        /// retorna true se é um dos tipos de wrapper data object.
        /// </summary>
        /// <param name="tipo">nome da classe a investigar.</param>
        /// <returns></returns>
        public static string isWrapperData(string tipo)
        {
            if ((tipo == "Vector") || (tipo == "Matriz") || (tipo == "DictionaryText") || (tipo == "JaggedArray"))
            {
                return tipo;
            }
            else
            {
                return null;
            }
            
        }

        /// <summary>
        /// retornam tokens que identificam a definicao de um wrapper data. p.ex. Vector vetor1, o id=Vector.
        /// </summary>
        /// <returns>retorna uma lista de tokens idenficadores.</returns>
        public abstract List<string> getNamesIDWrapperData();
        


        /// <summary>
        /// verifica se um determinado objeto é um tipo wrapper data.
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns>retorna a lista de tokens do wrapper data, se for wrapper, ou null se nao for wrapper data.</returns>
        public abstract List<string> isThisTypeWrapperParameter(List<string> tokens, int index);

        /// <summary>
        /// obtem o nome do objeto wrapper contido nos tokens.
        /// </summary>
        /// <param name="tokens">tokens da definicao do wrapper data object.</param>
        /// <returns></returns>
        public abstract string GetNameWrapperObject(List<string> tokens, int index);

       
        /// <summary>
        /// obtem o tipo de elemento constituinte do objeto wrapper.
        /// </summary>
        /// <param name="tokens">tokens contendo a definicao do objeto wrapper.</param>
        /// <param name="countTokensWrapper">contador de tokens utilizado na definicao do wrapper object.</param>
        /// 
        /// <returns>retorna o tipo do elemento, se houver, ou null se não for um objeto wrapper ou nao tiver tipo elemento especifico.</returns>
        public abstract string GetTipoElemento(List<string> tokens, ref int countTokensWrapper);


        /// <summary>
        /// verifica se os tokens contem uma instanciacao de objeto wrapper.
        /// </summary>
        /// <param name="str_exprss"></param>
        /// <returns></returns>
        public abstract bool IsInstantiateWrapperData(List<string> str_exprss);

        /// <summary>
        /// verifica se é uma estrutura de dados wrapper.
        /// </summary>
        /// <param name="tipoObjeto">tipo do objeto, wrapper ou nao.</param>
        /// <returns></returns>
        public abstract bool isWrapper(string tipoObjeto);


        /// <summary>
        /// verifica se os tokens da anotação é de uma chamada de metodo set element.
        /// </summary>
        /// <param name="tokensNotacaoWrapper">tokens da anotação wrapper, a investigar.</param>
        /// <returns>[true] se a anotação é de set element.</returns>
        public abstract bool IsSetElement(string nomeObjeto, List<string> tokensNotacaoWrapper);
        




        /// <summary>
        /// converte uma anotação wrapper em uma lista de tokens para uma expressao chamada de metodo, em metodo getElement..
        /// </summary>
        /// <param name="exprssEmNotacaoWrapper">expressao contendo os dados do elemento, ex.: M[1],M[1,4,8], M[x,y,z+1].</param>
        /// <param name="escopo">contexto onde a expressão está.</param>
        /// <param name="tokensProcessed">tokens consumidos da chamada de objeto.</param>
        /// <returns>retorna uma expressao chamada de metodo contendo os dados da expressao wrapper de getElement.</returns>
        public abstract List<string> GETChamadaDeMetodo(ref List<string> tokens, Escopo escopo, List<string> tokensProcessed);



        /// <summary>
        /// converte uma anotação wrapper em uma lista de tokens para uma expressao chamada de metodo.
        /// </summary>
        /// <param name="exprssEmNotacaoWrapper">expressao em anotação wrapper. ex>: M[1,4,8]=5.</param>
        /// <param name="escopo">contexto onde a expressão wrapper está.</param>
        /// <param name="tokensProcessed">tokens consumidos na chamada de metodo.</param>
        /// <returns>retorna uma expressao chamada de metodo contendo os dados da expressao wraper de setElement.</returns>
        public abstract List<string> SETChamadaDeMetodo(ref List<string> tokens, Escopo escopo, List<string> tokensProcessed);



        /// <summary>
        /// converte uma instanciação wrapper, em um construtor via chamada de metodo [Create],
        /// </summary>
        /// <param name="exprssInstanciacaoEmNotacaoWrapper">anotação da instanciação do objeto wrapper data.</param>
        /// <param name="escopo">contexto onde está o objeto a ser instanciado.</param>
        /// <returns></returns>
        public abstract List<string> Create(ref string exprssInstanciacaoEmNotacaoWrapper, Escopo escopo);




        /// <summary>
        /// faz o casting com um object contendo o valor do casting.
        /// </summary>
        /// <param name="objToCasting">object com o valor do casting.</param>
        public static void CastingObjeto(object objToCasting, Objeto objReceive)
        {
            if (objToCasting != null)
            {

                if (objToCasting.GetType() == typeof(Vector))
                {
                    objReceive.valor = (Vector)objToCasting;
                }
                else
                if (objToCasting.GetType() == typeof(Matriz))
                {
                    objReceive.valor = (Matriz)objToCasting;
                }
                else
                if (objToCasting.GetType() == typeof(DictionaryText))
                {
                    objReceive.valor = (DictionaryText)objToCasting;
                }
                else
                if (objToCasting.GetType() == typeof(JaggedArray))
                {
                    objReceive.valor = (JaggedArray)objToCasting;
                }
                else
                if (objToCasting.GetType() == typeof(Objeto))
                {
                    objReceive.valor = ((Objeto)objToCasting).valor;
                    return;
                }



            }
        }


        /// <summary>
        /// retorna o indice do primeiro objeto wrapper, dentro do escopo.
        /// </summary>
        /// <param name="escopo">contexto onde os objetos wrapper está,</param>
        /// <param name="exprssEmNotacaoWWrapper">texto contendo a expressao wrapper.</param>
        /// <returns></returns>
        public string GetNameOfFirstObjectWrapper(Escopo escopo, string exprssEmNotacaoWWrapper)
        {
            List<Objeto> objWrapper = escopo.tabela.GetObjetos().FindAll(k => k.isWrapperObject == true);
            if ((objWrapper == null) || (objWrapper.Count == 0))
            {
                return null;
            }

            List<string> tokensNotacaoWrapper = new Tokens(exprssEmNotacaoWWrapper).GetTokens();
            if ((tokensNotacaoWrapper == null) || (tokensNotacaoWrapper.Count == 0))
            {
                return null;
            }

            for (int i = 0; i < objWrapper.Count; i++)
            {
                int indexObjWrapper = tokensNotacaoWrapper.FindIndex(k => k.Equals(objWrapper[i].GetNome()));
                if (indexObjWrapper != -1)
                {
                    return objWrapper[i].GetNome();
                }
            }

            return null;
        }

        /// <summary>
        /// faz a conversao de um object [objToCasting] de um wrapper object, para um Objeto.
        /// </summary>
        /// <param name="objtFromCasting">objeto contendo o conteudo do casting.</param>
        /// <param name="ObjToReceiveCast">objeto a receber o casting.</param>
        public abstract bool Casting(object objtFromCasting, Objeto ObjToReceiveCast);
     


        /// <summary>
        /// retorna uma expressao, sem os tokens processado em uma anotação wrapper.
        /// </summary>
        /// <param name="tokens_input">tokens da expressão original.</param>
        /// <param name="tokensConsumidosWrapper">tokens consumidos na anotação wrapper.</param>
        /// <param name="tokensRetorno">tokens de retorno da anotação: tokens de GetElement,SetElement.</param>
        /// <returns></returns>
        public static string ExtraiTokensConsumido(List<string> tokens_input, List<string> tokensConsumidosWrapper, List<string> tokensRetorno)
        {

            tokensConsumidosWrapper.Remove(";");
            
            string exprssEmNotacaoWrapper = "";
            // extrai do texto da expressão de entrada, os tokens consumidos na expressao wrapper.
            int indexBeginProcessed = 0;
            int indexCurrentInput = 0;
            int indexCurrentConsumido = 0;

            while ((indexCurrentInput < tokensConsumidosWrapper.Count) &&
                  ((indexBeginProcessed + indexCurrentConsumido) < tokensConsumidosWrapper.Count) &&
                  (indexCurrentInput < tokens_input.Count))
            {
                if (tokens_input[indexCurrentInput] == tokensConsumidosWrapper[indexBeginProcessed + indexCurrentConsumido])
                {
                    indexCurrentInput++;
                    indexCurrentConsumido++;
                }
                else
                {
                    indexCurrentInput = 0;
                    indexCurrentConsumido = 0;

                    indexBeginProcessed++;

                    if (indexBeginProcessed >= tokensConsumidosWrapper.Count)
                    {
                        break;
                    }
                }

            }
            if ((tokens_input != null) && (tokensConsumidosWrapper != null) && (tokens_input.Count == tokensConsumidosWrapper.Count))
            {
                // todos tokens da entrada foram consumidos, retorna uma string vazia.
                return "";
            }

            if ((indexBeginProcessed < tokensConsumidosWrapper.Count) && (tokens_input.Count <= tokensConsumidosWrapper.Count))
            {
                // remove os tokens consumidos na anotação wrapper.
                tokens_input.RemoveRange(indexBeginProcessed, tokensConsumidosWrapper.Count - indexBeginProcessed);
                // insere os tokens de retorno: GetElement(), SetElement();
                tokens_input.InsertRange(indexBeginProcessed, tokensRetorno);
            }

            exprssEmNotacaoWrapper = "";
            for (int i = 0; i < tokens_input.Count; i++)
            {
                exprssEmNotacaoWrapper += tokens_input[i].ToString() + " ";
            }

            return exprssEmNotacaoWrapper;
        }




     


        public new class Testes : SuiteClasseTestes
        {
            public Testes() : base("testes de wrappers data funcoes")
            {
            }

            public void TesteGetNomeWrapperDataObject(AssercaoSuiteClasse assercao)
            {
                string code_0_1_Vector = "int[] vetor1[20]";
                string code_0_2_Matriz = "int [,] mt_1= [20,20]";
                string code_0_3_JaggedArray = "JaggedArray j1 = [ 20 ] [ ]";
                string code_0_4_DictionaryText = "DictionaryText dict1 = { string }";

                List<string> tokens_0_1_vector = new Tokens(code_0_1_Vector).GetTokens();
                List<string> tokens_0_1_matriz = new Tokens(code_0_2_Matriz).GetTokens();
                List<string> tokens_0_1_jagged = new Tokens(code_0_3_JaggedArray).GetTokens();
                List<string> tokens_0_1_dictionary = new Tokens(code_0_4_DictionaryText).GetTokens();

                List<WrapperData> wrappersDefinition = new List<WrapperData>() {
                    new WrapperDataVector(),
                    new WrapperDataMatriz(),
                    new WrapperDataDictionaryText(),
                    new WrapperDataJaggedArray()};

                int countAssercoes = 0;
                int countTokens = 0;
                string tipoElemento = null;
                for (int i = 0; i < wrappersDefinition.Count; i++)
                {
                    if (wrappersDefinition[i].isThisTypeWrapperParameter(tokens_0_1_vector, 0) != null) 
                    {

                       
                        string name = wrappersDefinition[i].GetNameWrapperObject(tokens_0_1_vector, 0);
                        tipoElemento = wrappersDefinition[i].GetTipoElemento(tokens_0_1_vector, ref countTokens);
                        if ((name == "vetor1") && (tipoElemento=="int"))
                        {
                            countAssercoes++;
                        }
                        assercao.IsTrue(name == "vetor1" && tipoElemento=="int", code_0_1_Vector);
                        
                    }

                    if (wrappersDefinition[i].isThisTypeWrapperParameter(tokens_0_1_matriz,0) != null)
                    {
                        string name = wrappersDefinition[i].GetNameWrapperObject(tokens_0_1_matriz, 0);
                        tipoElemento = wrappersDefinition[i].GetTipoElemento(tokens_0_1_matriz, ref countTokens);

                        if (name == "mt_1" && tipoElemento=="int")
                        {
                            countAssercoes++;
                        }
                        assercao.IsTrue(name == "mt_1" && tipoElemento == "int", code_0_2_Matriz);

                    }

                    if (wrappersDefinition[i].isThisTypeWrapperParameter(tokens_0_1_jagged, 0) != null)
                    {
                        string name = wrappersDefinition[i].GetNameWrapperObject(tokens_0_1_jagged, 0);
                        if (name == "j1")
                        {
                            countAssercoes++;
                        }
                        assercao.IsTrue(name == "j1", code_0_3_JaggedArray);
                        
                    }

                    if (wrappersDefinition[i].isThisTypeWrapperParameter(tokens_0_1_dictionary, 0) != null)
                    {
                        string name = wrappersDefinition[i].GetNameWrapperObject(tokens_0_1_dictionary, 0);
                        tipoElemento= wrappersDefinition[i].GetTipoElemento (tokens_0_1_dictionary, ref countTokens);
                        if (name == "dict1" && tipoElemento == "string")
                        {
                            countAssercoes++;
                        }
                        assercao.IsTrue(name == "dict1", code_0_4_DictionaryText);
                        
                    }
                }
            
                assercao.IsTrue(countAssercoes == 4, "contador de assercoes true feito.");
            }

          
        }

    }

}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using System.Threading;
using System.Web;
using Wrappers.DataStructures;

namespace parser
{
    public class MetodosString : ImportaMetodosClassesBasicas
    {
        public static char aspas = '\u0022';

        public MetodosString()
        {

            /// importa todos metodos desta classe, para dentro da classe "string" orquidea.
            this.LoadMethods("string", typeof(MetodosString));
        }

        /// <summary>
        /// remove aspas de constantes string.
        /// </summary>
        /// <param name="text">string a remover aspas.</param>
        /// <returns></returns>
        private string RemoveAspas(string text)
        {
            return text.Replace(aspas.ToString(), "");
        }

        /// <summary>
        /// retorna um valor hash, calculado pelas letras da string.
        /// </summary>
        /// <param name="text">string</param>
        /// <returns>retorna um objeto int, contendo o hash.</returns>
        public int Hash(string texto)
        {
            texto= RemoveAspas(texto);

            string chars = "ABCDEFGHIJKLMNOPQRSTUVXYWZabcdefghijklmnopqrstuvxywz_0123456789_";
            int hashNumber = 0;
            int baseHash = 1;

            // garante que o numero hash começa com os caracteres primeiros sejam os valores mais altos do polinomio hash.
            for (int x=0;x< texto.Length; x++)
            {
                baseHash *= (chars.Length);
            }



            for (int x = 0; x < texto.Length; x++)
            {
                hashNumber += chars.IndexOf(texto[x]) * baseHash;


                // atualiza a base do polinomio hash.
                baseHash /= chars.Length;
            }

            return hashNumber;

        }

        /// <summary>
        /// quebra o texto em palavras, delimitadas por caracteres separadores.
        /// </summary>
        /// <param name="text">texto com todas as palavras.</param>
        /// <param name="vtSeparadores">carcteres que separam e quebram em palavras.</param>
        /// <returns>retorna um vector contendo as palavras separadas.</returns>
        /// <exception cref="ArgumentException"></exception>
        public Vector SplitChar(string text, Vector vtSeparadores)
        {
            if (vtSeparadores == null)
            {
                return null;
            }
            text = RemoveAspas(text);

            List<string> separadores = new List<string>();
            for (int i = 0; i < vtSeparadores.size(); i++)
            {
                if (vtSeparadores.GetElement(i) != null)
                {
                    separadores.Add(RemoveAspas((string)vtSeparadores.GetElement(i)));
                }
            }

            if ((text != null) && (text.Length > 0))
            {
                Vector results = new Vector("string");

                string[] palavras = text.Split(separadores.ToArray(), StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < palavras.Length; i++)
                {
                    results.SetElement(i, palavras[i]);
                }
                return results;
            }
            return null;
        }


        /// <summary>
        /// separa um texto a partir de um delimitador. ex.: SliceText("111#222#333", "#") retorna {"111","222","333"}
        /// </summary>
        /// <param name="text"></param>
        /// <param name="separador"></param>
        /// <returns></returns>
        public  string[] Splice(string text, string separador)
        {
            text = RemoveAspas(text);
            separador= RemoveAspas(separador);

            return text.Split(new string[] { separador }, StringSplitOptions.RemoveEmptyEntries);
        }


        /// <summary>
        /// retorna o tamanho do texto, em número de caracteres.
        /// </summary>
        /// <param name="text">texto para o tamanho retornado.</param>
        /// <returns></returns>
        public  int Size(string text)
        {
            text = RemoveAspas(text);
            return ((string)text).Length;        
        }


        /// <summary>
        /// obtem um sub-texto. ex: SubText("cat",0,1) retorna "ca".
        /// </summary>
        /// <param name="text">texto principal.</param>
        /// <param name="begin">indice de inicio do sub-texto.</param>
        /// <param name="end">indice do fim do sub-texto.</param>
        /// <returns></returns>
        public  string SubText(string text, int begin, int end)
        {
            text = RemoveAspas(text);
            return text.Substring(begin, end - begin + 1);
        }

        /// <summary>
        /// obtem um texto que é o reverso em caractere do texto de entrada.
        /// </summary>
        /// <param name="text">texto a reverter.</param>
        /// <returns></returns>
        public string Reverse(string str)
        {
            str = RemoveAspas(str);
            string str_reverse = "";

            for (int x=0;x< str.Length; x++)
            {
                str_reverse += str[str.Length - 1 - x];
            }

            return str_reverse;

        }

        /// <summary>
        /// remove um sub-texto dentro do texto principal.ex: Remove("tiger",2,2) retorna "tir", retirado "ge".
        /// </summary>
        /// <param name="text">texto principal.</param>
        /// <param name="index">indice do começo do text a remover.</param>
        /// <param name="size">tamanho do texto a resolver.</param>
        /// <returns>retorna o texto sem o sub-texto.</returns>
        public  string Remove(string text, int index, int size)
        {
            text = RemoveAspas(text);

            string textBegin = ((string)text).Substring(0, index - 1);
            string textEnd = ((string)text).Substring(index, size);


            return textBegin + textEnd;
        }


        /// <summary>
        /// une um texto  para o final de um texto entrada.
        /// </summary>
        /// <param name="text">texto principal.</param>
        /// <param name="textAppend">texto para unir.</param>
        /// <returns></returns>
        public  string Append(string text, string textAppend)
        {
            text = RemoveAspas(text);
            return text + textAppend;
        }

        /// <summary>
        /// retorna o caracter do texto, num determinado indice.
        /// </summary>
        /// <param name="text">texto principal.</param>
        /// <param name="index">indice do caracter a retornar.</param>
        /// <returns></returns>
        public char GetChar(string text, int index)
        {
            text = RemoveAspas(text);
            return text[index];
        }

        /// <summary>
        /// retorna se o texto está sem caracteres, ou nulo.
        /// </summary>
        /// <param name="text">texto principal.</param>
        /// <returns></returns>
        public  bool IsEmpty(string text)
        {
            if ((text != null) && (text.Length > 0))
            {
                text = RemoveAspas(text);
            }
            return text == null || text.Length == 0;
        }


        /// <summary>
        /// insere um sub-texto, começando por determinado indice, para dentro de um texto principal.
        /// </summary>
        /// <param name="text">texto principal.</param>
        /// <param name="subText">sub-texto a inserir</param>
        /// <param name="indexBegin">indice de inserção do sub-texto.</param>
        /// <returns></returns>
        public  string Insert(string text, string subText, int indexBegin)
        {
            text = RemoveAspas(text);

            string str_begin = text.Substring(0, indexBegin);
            return str_begin + subText;
       
        }

        /// <summary>
        /// retorna o indice de um sub-texto, encontrado dentro de um texto principal.
        /// </summary>
        /// <param name="text">texto principal.</param>
        /// <param name="subText">texto a encontrar.</param>
        /// <returns></returns>
        public  int Find(string text, string subText)
        {
            text = RemoveAspas(text);
            return text.IndexOf(subText);
        }

        /// <summary>
        /// retorna o texto principal em caracteres maiúsculos.
        /// </summary>
        /// <param name="text">texto principal.</param>
        /// <returns></returns>
        public  string _Lower(string text)
        {
            text = RemoveAspas(text);
            return text.ToLower();
        }

        /// <summary>
        /// retorna o texto principal em caracteres minúsculos.
        /// </summary>
        /// <param name="text">texto principal.</param>
        /// <returns></returns>
        public  string _Upper(string text)
        {
            text = RemoveAspas(text);
            return text.ToUpper();
        }

        /// <summary>
        /// retorna o texto principal, substitui um texto a remover, por um texto a inserir.
        /// </summary>
        /// <param name="text">texto principal.</param>
        /// <param name="textToRemove">texto a remover.</param>
        /// <param name="textToReplace">texto a substituir o texto removido.</param>
        /// <returns></returns>
        public  string ReplaceFor(string text, string textToRemove, string textToReplace)
        {
            text = RemoveAspas(text);
            textToRemove = RemoveAspas(textToRemove);
            textToReplace = RemoveAspas(textToReplace);
            return text.Replace(textToRemove, textToReplace);
        }


        /// <summary>
        /// retorna o primeiro indice de um sub-texto, dentro de um texto principal.
        /// </summary>
        /// <param name="text">texto principal.</param>
        /// <param name="textToLocate">texto a localizar o indice.</param>
        /// <returns></returns>
        public  int Index(string text, string textToLocate)
        {
            text = RemoveAspas(text);
            textToLocate = RemoveAspas(textToLocate);
            int index = text.IndexOf(textToLocate);
            return index;
        }

        /// <summary>
        /// compara se um texto é igual a outro.
        /// </summary>
        /// <param name="text1">texto para comparação.</param>
        /// <param name="text2">texto para comparação.</param>
        /// <returns></returns>
        public  bool EqualsText(string text1, string text2)
        {
            text1 = RemoveAspas(text1);
            text2 = RemoveAspas(text2);
            return text1 ==  text2;
        }

    

        /// <summary>
        /// retorna [true] se o texto principal começa com um sub-texto de entrada.
        /// </summary>
        /// <param name="text">texto principal.</param>
        /// <param name="textToStart">sub-texto no inicio do texto principal, a verificar.</param>
        /// <returns></returns>
        public bool Start(string text, string textToStart)
        {
            text = RemoveAspas(text);
            textToStart = RemoveAspas(textToStart);
            return text.IndexOf(textToStart) == 0;
        }

        /// <summary>
        /// retorna [true] se o texto principal termina com  sub-texto entrada.
        /// </summary>
        /// <param name="text">texto principal.</param>
        /// <param name="textToEnd">sub-texto no final do texto principal, a verificar.</param>
        /// <returns></returns>
        public bool End(string text, string textToEnd)
        {
            text = RemoveAspas(text);
            textToEnd = RemoveAspas(textToEnd);

            for (int x = 0; x < ((string)textToEnd).Length; x++)
            {
                if (text[text.Length - 1 + x] != textToEnd[textToEnd.Length - 1 + x])
                    return false;

            }
            return true;
        }

        /// <summary>
        /// retorna [true] se o sub-texto foi encontrado no texto principal.
        /// </summary>
        /// <param name="text">texto principal.</param>
        /// <param name="subText">sub-texto a encontrar dentro do texto principal.</param>
        /// <returns></returns>
        public bool Contains(string text, string subText)
        {
            text = RemoveAspas(text);
            subText = RemoveAspas(subText);

            return text.IndexOf(subText.Replace(aspas.ToString(), "")) != -1;
        }

        /// <summary>
        /// retorna um texto descrevendo o numero de entrada.
        /// </summary>
        /// <param name="n">numero a retornar um texto.</param>
        /// <returns></returns>
        public string textFromInt(int n)
        {
            return n.ToString();
        }

        /// <summary>
        /// retorna um texto descrevendo o numero ponto-flutuante de entrada.
        /// </summary>
        /// <param name="n">numero a retornar um texto.</param>
        /// <returns></returns>
        public string textFromFloat(float n)
        {
            return n.ToString();
        }

        /// <summary>
        /// retorna um texto representando o caracter de entrada.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public string textFromString(string c)
        {
            c = RemoveAspas(c);
            return (string)c.Clone();
        }

        /// <summary>
        /// retorna um texto "true" ou "false" representando o valor booleano de entrada.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public string textFromBool(bool n)
        {
            if (n)
                return "true";
            else
                return "false";
        }

        /// <summary>
        /// retorna um texto com o sub-texto como começo do texto principal.
        /// </summary>
        /// <param name="text">texto principal.</param>
        /// <param name="textToPush">sub-texto do começo do texto principal.</param>
        /// <returns></returns>
        public  string PushBegin(string text, string textToPush)
        {
            text = RemoveAspas(text);
            textToPush = RemoveAspas(textToPush);
            return textToPush + text;
        }

        /// <summary>
        /// retorna um texto com o sub-texto como fim do texto principal.
        /// </summary>
        /// <param name="text">texto principal.</param>
        /// <param name="textToPush">sub-texto do fim do texto principal.</param>
        /// <returns></returns>
        public  string PushEnd(string text, string textToPush)
        {
            text = RemoveAspas(text);
            textToPush = RemoveAspas(textToPush);
            return text + textToPush;
        }


        /// <summary>
        /// remove os caracteres de um texto principal, do indice [begin] até o indice [end].
        /// </summary>
        /// <param name="text">texto principal.</param>
        /// <param name="begin">indice de inicio dos caracteres a serem removidos.</param>
        /// <param name="end">indice do fim de caracteres a serem removidos.</param>
        /// <returns></returns>
        public string Erase(string text, int begin, int end)
        {
            text = RemoveAspas(text);
           
            string textBegin = text.Substring(0, begin);
            string textEnd = text.Substring(begin, end - begin + 1);

            return textBegin + textEnd;
        }

        /// <summary>
        /// retorna um vetor com os indices do caracter presente no texto parametro.
        /// </summary>
        /// <param name="text">texto da busca.</param>
        /// <param name="caracter">caracter a encontrar.</param>
        /// <returns>um vetor com os indices do caracter, dentro do texto de busca.</returns>
        public Vector OcurrenciesChar(string text, int offset, char caracter)
        {
            Vector result = new Vector("int");
            result.Clear();
            for (int i = 0; i < text.Length; i++)
            {
                if (offset + i >= text.Length)
                {
                    return result;
                }
                if (text[offset + i] == caracter)
                {
                    result.Append(i);
                }


            }

            return result;
        }

        /// <summary>
        /// encontra a primeira ocorrencia de [search] em [text]
        /// </summary>
        /// <param name="text">texto da busca.</param>
        /// <param name="search">texto a procurar.</param>
        /// <returns>retorna o indice da primeira ocorrencia, ou -1 se nao houve ocorrencias.</returns>
        public int FirstOcurr(string text, string search)
        {
            Vector result= this.Ocurrencies(text, search);
            if ((result != null) && (result.Size > 0))
            {
                return (int)result.GetElement(0);
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// encontra a ultima ocorrencia de [search] em [text].
        /// </summary>
        /// <param name="text">texto da busca.</param>
        /// <param name="search">texto de ocorrencias.</param>
        /// <returns>retorno o indice da ultima ocorrencia, ou -1 se nao houve ocorrencias.</returns>
        public int LastOcurr(string text, string search)
        {
            Vector result = this.Ocurrencies(text, search);
            if ((result != null) && (result.Size > 0))
            {
                return (int)result.GetElement(result.Size - 1);
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// encontra a i-esima ocorrencia de [search] em [text].
        /// </summary>
        /// <param name="text">texto da busca.</param>
        /// <param name="search">texto de ocorrencias.</param>
        /// <param name="count">contador da i-esima ocorrencia a encontrar.</param>
        /// <returns></returns>
        public int CountOcurr(string text, string search, int count)
        {
            Vector result = this.Ocurrencies(text, search);
            if ((result != null) && (result.Size >= count - 1)) 
            {
                return (int)result.GetElement(count - 1);
            }
            else
            {
                return -1;
            }
        }



        /// <summary>
        /// retorna um vetor de indices de ocorrencias de um texto de busca em um texto principal.
        /// </summary>
        /// <param name="text">texto principal.</param>
        /// <param name="search">texto da busca.</param>
        /// <returns></returns>
        public Vector Ocurrencies(string text, string search)
        {
            Vector result = new Vector("int");
            result.Clear();
   
            int offfset = 0;
            int countLetterSearch = 0;
            while (offfset < text.Length) 
            {
         
                if (offfset >= text.Length)
                {
                    break;
                }
                
                if ((countLetterSearch<search.Length) && 
                   (offfset < text.Length) && 
                   (text[offfset] == search[countLetterSearch]))
                {
                    countLetterSearch++;
                    if (countLetterSearch== search.Length)
                    {
                        result.Append(offfset - search.Length + 1);
                        countLetterSearch = 0;
                    }
                    offfset++;
                    continue;
                }
                else
                {
                    countLetterSearch = 0;
                    offfset++;
                   
                }

                

            }

            return result;
        }

        /// <summary>
        /// remove do texto [text] a primeira ocorrencia de [search].
        /// </summary>
        /// <param name="text">texto contendo a ocorrencia.</param>
        /// <param name="seach">texto da busca.</param>
        /// <returns>retorna um texto sem a primeria ocorrencia de [search],
        /// ou o texto inteiro se não encontrar ocorrencias.</returns>
        public string RemoveFirstOcurr(string text, string search)
        {
            Vector ocorrencias = this.Ocurrencies(text, search);
            if ((ocorrencias != null) && (ocorrencias.size() > 0)) 
            {
                int indexFirstOcurr = (int)ocorrencias.Get(0);
                if (indexFirstOcurr + 1 < text.Length)
                {
                    text = text.Remove(indexFirstOcurr, search.Length);
                    return text;
                }
                else
                {
                    return text;
                }
            }
            else
            {
                return text;
            }
        }


        /// <summary>
        /// remove a ultima ocorrencia de [search], em [text].
        /// </summary>
        /// <param name="text">texto da procura.</param>
        /// <param name="search">texto a encontrar.</param>
        /// <returns>retorna o texto sem a ultima ocorrencia de [search].</returns>
        public string RemoveLastOcurr(string text, string search)
        {
            Vector ocorrencias = this.Ocurrencies(text, search);
            if ((ocorrencias != null) && (ocorrencias.size() > 0))
            {
                int indexFirstOcurr = (int)ocorrencias.Get(ocorrencias.size() - 1);
                text = text.Remove(indexFirstOcurr, search.Length);
                return text;
            }
            else
            {
                return text;
            }
        }

        /// <summary>
        /// remove a i-esima ocorrencia de [search] em [text].
        /// </summary>
        /// <param name="text">texto da procura.</param>
        /// <param name="search">texto a encontrar.</param>
        /// <param name="countOcurr">indice da ocorrencia, comeca em 1.</param>
        /// <returns>retorna o texto sem a i-esima ocorrencia de [search] em [text].</returns>
        public string RemoveCountOcurr(string text, string search, int countOcurr)
        {
            Vector ocorrencias = this.Ocurrencies(text, search);
            if ((ocorrencias != null) && (ocorrencias.size() >= countOcurr - 1)) 
            {
                int indexFirstOcurr = (int)ocorrencias.Get(countOcurr - 1);
                text = text.Remove(indexFirstOcurr, search.Length);
                return text;
            }
            else
            {
                return text;
            }

        }



        /// <summary>
        /// recorta o texto parametro, com divisórias de textos de [separtors] vetor.
        /// </summary>
        /// <param name="text">texto a fatiar.</param>
        /// <param name="separators">textos de separadores.</param>
        /// <returns>retorna um vetor com as palavras recortadas, ou o texto se nao houve recortes.</returns>
        public Vector CuttWords(string text, Vector separators)
        {
            text = RemoveAspas(text);
            Vector result = new Vector("string");
            result.Clear();
            if (separators == null)
            {
                result.Append(text);
                return result;
            }
            List<string> textCutters = new List<string>();
            for (int i = 0; i < separators.Size; i++) 
            {
                if (separators.Get(i) != null)
                {
                    textCutters.Add(RemoveAspas((string)separators.Get(i)));
                }
            }
            List<Vector> ocorrenciasSeparadores= new List<Vector>();
            for (int i=0;i<textCutters.Count;i++)
            {
                // conta as ocorrencias de cada separador.
                Vector vtOcurrSep = Ocurrencies(text, textCutters[i]);
                if (vtOcurrSep != null)
                {
                    ocorrenciasSeparadores.Add(vtOcurrSep);
                }
            }

            // se nao ha ocorrencias, retorna o texto inteiro, nao houve recortes.
            if (ocorrenciasSeparadores.Count == 0)
            {
                result.Append(text);
                return result;

            }

            int indexSeparator = 1;

            // tabela hash para marcar indices de ocorrencias de separadores.
            int[] tabelaHash = new int[text.Length];
            for (int i=0;i< ocorrenciasSeparadores.Count;i++)
            {
                for (int ocurr=0; ocurr < ocorrenciasSeparadores[i].Size;ocurr++)
                {
                    if (ocorrenciasSeparadores[i].Get(ocurr) != null)
                    {
                        int indexHash = (int)ocorrenciasSeparadores[i].Get(ocurr);
                        tabelaHash[indexHash] = indexSeparator;
                    }

                    
                }
                indexSeparator++;
            }

            // vetor de retorno.
            Vector palavras = new Vector("string");
            palavras.Clear();

            // palavra currente.
            string palavraCurr = "";

            string separadorCurr = "";
            // busca por delimitações entre separadores.
            for (int indexChar = 0; indexChar < tabelaHash.Length; indexChar++)
            {
                if (tabelaHash[indexChar] >= 1)
                {// encontrou um limite de delimitação, adiciona a palavra currente no vetor de retorno.

                    // calculo do separador atingido.
                    separadorCurr = textCutters[tabelaHash[indexChar] - 1];
   
                    
                    palavras.Append(palavraCurr);

                    // calculo da proxima palavra, tem que pular o separador!
                    indexChar += separadorCurr.Length;
                    if (indexChar>=tabelaHash.Length)
                    {
                        break;
                    }

                    palavraCurr = "";
                }

               
   
                // se nao houve indice de delimitacao, adiciona o caracter currente para a palavra currente.
                palavraCurr += text[indexChar];

            }

            // adiciona a ultima palavra.
            if (palavraCurr != "")
            {
                palavras.Append(palavraCurr);
            }

            

            return palavras; 
        }

      
        

        public class Testes:SuiteClasseTestes
        {
            public Testes():base("testes biblioteca string")
            {

            }
            public void TesteCutterWordsSeparadoresComoPalavras(AssercaoSuiteClasse assercao)
            {
                string texto = "Brasil verde da Amazonia verde";
                
                string umSeparador_0_0 = "Brasil";
                string umSeparador_0_1 = "Amazonia";
                Vector separators = new Vector("string");
                separators.Clear();
                separators.Append(umSeparador_0_0);
                separators.Append(umSeparador_0_1);


                try
                {
                    MetodosString funcoes = new MetodosString();
                    Vector palavras = funcoes.CuttWords(texto, separators);

                    assercao.IsTrue(palavras.Size == 3);
                }
                catch (Exception e)
                {
                    assercao.IsTrue(false, "TESTE FALHOU" + e.Message);
                }



            }


            public void TesteRemoveOcurrencies(AssercaoSuiteClasse assercao)
            {
                string texto = "Brasil verde da Amazonia verde";
                string search = "verde";

                MetodosString funcoes = new MetodosString();
                string result_count = funcoes.RemoveCountOcurr(texto, search, 2);
                string result_first = funcoes.RemoveFirstOcurr(texto, search);
                string result_last = funcoes.RemoveLastOcurr(texto, search);

                try
                {
                    assercao.IsTrue(result_count.Trim(' ') == "Brasil verde da Amazonia");
                    assercao.IsTrue(result_last.Trim(' ') == "Brasil verde da Amazonia");
                    assercao.IsTrue(result_first.Trim(' ') == "Brasil  da Amazonia verde");

                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }

                
                
            }

   
            public void TesteCutterWords(AssercaoSuiteClasse assercao)
            {
                string texto_0_0 = "Brasil#Verde";
                string texto_0_1 = "Brasil#Verde#Da#Amazonia#Vasta";
                string texto_0_2 = "Brasil Verde#Da Amazonia Vasta";

                string separador1 = "#";
                Vector separadores_0_0 = new Vector("string");
                separadores_0_0.Clear();
                separadores_0_0.Append(separador1);

                string separador2 = " ";
                Vector separadores_0_1 = new Vector("string");
                separadores_0_1.Clear();
                separadores_0_1.Append(separador1);
                separadores_0_1.Append(separador2);



                MetodosString funcoes = new MetodosString();
                Vector palavras_0_0 = funcoes.CuttWords(texto_0_0, separadores_0_0);
                Vector palavras_0_1 = funcoes.CuttWords(texto_0_1, separadores_0_0);
                Vector palavras_0_2 = funcoes.CuttWords(texto_0_2, separadores_0_1);

                try
                {
                    assercao.IsTrue(palavras_0_0.Size == 2, texto_0_0);
                    assercao.IsTrue(palavras_0_1.Size == 5, texto_0_1);
                    assercao.IsTrue(palavras_0_2.Size == 5, texto_0_2);
                }
                catch (Exception e)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + e.Message);
                }



            }
            public void TesteOcorrenciasText(AssercaoSuiteClasse assercao)
            {
                string texto = "o verde do verde Amazonia";
                string search = "verde";

                try
                {
                    MetodosString funcoes = new MetodosString();
                    Vector result_all = funcoes.Ocurrencies(texto, search);
                    int result_first = funcoes.FirstOcurr(texto, search);
                    int result_last = funcoes.LastOcurr(texto, search);
                    int result_ocurr = funcoes.CountOcurr(texto, search, 1);

                    assercao.IsTrue(result_all.Size == 2, texto + ",search:" + search);
                    assercao.IsTrue(result_first == 2, texto + ",search:" + search);
                    assercao.IsTrue(result_last == 11, texto + ",search:" + search);
                    assercao.IsTrue(result_ocurr == 2, texto + ",search:" + search);

                }
                catch(Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }
               
            }

            public void TesteOcurrenciasChar(AssercaoSuiteClasse assercao)
            {
                string texto = "A terra eh verde de amazonia";
                char cBusca = 'r';
                MetodosString funcoesString = new MetodosString();
                Vector result = funcoesString.OcurrenciesChar(texto, 0, cBusca);

                assercao.IsTrue(result.Size == 3);
            }
        }
    }
}

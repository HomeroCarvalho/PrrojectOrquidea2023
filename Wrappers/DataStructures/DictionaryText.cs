using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using parser;
namespace Wrappers.DataStructures
{
    public class DictionaryText: Objeto
    {

        public Dictionary<string, object> _dict;


        /// <summary>
        /// obtem o dicionario a partir de um object.
        /// </summary>
        public new object valor
        {
            get
            {
                return (object)this;
            }
            set
            {
                DictionaryText dictValor= (DictionaryText)value;
                this._dict = dictValor._dict;


            }
        }

        public override string ToString()
        {
            string str = "";
            if (_dict != null)
            {

                foreach (KeyValuePair<string, object> umItem in _dict)
                {
                    if ((umItem.Key != null) && (umItem.Value != null)) 
                    {
                        str += "[" + umItem.Key + ":" + umItem.Value + "],";
                    }
                    
                }
                str = str.Remove(str.Length - 1);
                
            }
            return str;
        }


        /// <summary>
        /// faz o casting de um object para o dicionaryText;
        /// </summary>
        /// <param name="vt">dictionary text parametro.</param>
        public void Casting(object vt)
        {
            DictionaryText dictToCopy=(DictionaryText)vt;
            if (dictToCopy._dict != null)
            {
                if (this._dict == null)
                {
                    this._dict = new Dictionary<string, object>();
                }
                

                foreach(KeyValuePair<string,object> umItem in dictToCopy._dict)
                {
                    this._dict.Add(umItem.Key, umItem.Value);
                }
            }
            else
            {
                this._dict=null;
            }
        }


        public DictionaryText()
        {
            _dict = new Dictionary<string, object>();
            this.tipo = "DictionaryText";
            this.tipoElemento = "string";
        }

        public void SetElement(string chave, object valor)
        {
            _dict[chave] = valor;
           
        }

        public object GetElement(string chave)
        {
            return _dict[chave];
        }

        public void RemoveElement(string chave)
        {
            _dict.Remove(chave);
        }

        public int Size()
        {
            return _dict.Count;
        }

        /// <summary>
        /// retorna um vetor com todos valores de chave do dicionario.
        /// </summary>
        /// <returns></returns>
        public Vector GetKeys()
        {
            int contKeys = 0;
            Vector vt_retorno = new Vector(Size());
            Dictionary<string, object>.Enumerator iterator = _dict.GetEnumerator();
            while (iterator.MoveNext())
            {
                vt_retorno.SetElement(iterator.Current.Key, contKeys);
                contKeys++;
            }
            return vt_retorno;
        }

        /// <summary>
        /// retorna um vetor com todos value do dicionario.
        /// </summary>
        /// <returns></returns>
        public Vector GetValues()
        {
            int contKeys = 0;
            Vector vt_retorno = new Vector(Size());
            Dictionary<string, object>.Enumerator iterator = _dict.GetEnumerator();
            while (iterator.MoveNext())
            {
                vt_retorno.SetElement(iterator.Current.Value, contKeys);
                contKeys++;
            }
            return vt_retorno;
        }


        /// <summary>
        /// instancia um dictionary text, a partir de uma chamada de metodo. 
        /// </summary>
        /// <param name="obj">objeto instanciado e registrado, em tempo de compilação.</param>
        public void Create()
        {
            this._dict = new Dictionary<string, object>();
            this.tipo= "DictionaryText";
            this.isWrapperObject = true;
          
 
        }


        /// <summary>
        /// retorna o tipo dos elementos constituinte.
        /// </summary>
        /// <returns></returns>
        public new string GetTipoElement()
        {
            return "Object";
        }

        public new class Testes : SuiteClasseTestes
        {
            public Testes() : base("teste classe wrapper dictionary text")
            {
            }

            public void TesteInstanciacao(AssercaoSuiteClasse assercao)
            {
                DictionaryText dict = new DictionaryText();
                dict.SetNome("b");

                assercao.IsTrue(dict != null, "validacao para instanciacao de objeto dictionary text.");
                assercao.IsTrue(dict.GetNome() == "b", "validacao para nome de objeto dictionary text.");
            }


            public void TesteSETElementGETElement(AssercaoSuiteClasse assercao)
            {
                DictionaryText dict = new DictionaryText();
                dict.SetNome("b");

                dict._dict["fruta"] = "uva";

                assercao.IsTrue(dict != null, "validacao para instanciacao de objeto dictionary text.");
                assercao.IsTrue(dict._dict != null && dict._dict["fruta"] != null);


            }
        }
    }
}

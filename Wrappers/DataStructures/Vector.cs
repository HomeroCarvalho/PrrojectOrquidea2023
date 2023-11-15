using parser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;


namespace Wrappers.DataStructures
{


    public class Vector : Objeto
    {

        /// <summary>
        /// array contendo os elementos do vetor.
        /// </summary>
        public object[] VectorObjects;

        public int _size = 10;


        /// <summary>
        /// obtem o vetor a partir de um object.
        /// </summary>
        public new object valor
        {
            get
            {
                return (object)this;
            }
            set
            {
                Vector vectorValue = (Vector)value;
                this.Casting(vectorValue);

            }

        }
        

        public int Size
        {
            get
            {
                return _size;
            }
            set
            {

                List<object> lstElements = VectorObjects.ToList<object>();
                int offset = 0;
                if (value > _size)
                {
                    offset = value - _size;
                    for (int x = 0; x < offset; x++)
                        lstElements.Add(new object());
                }
                else
                {
                    offset = _size - value;
                    for (int x = 0; x < offset; x++)
                        lstElements.RemoveAt(lstElements.Count - 1);
                }

                this.VectorObjects = lstElements.ToArray();

                this._size = value;
            }
        }


        public Vector(int size)
        {
            
            this.VectorObjects = new object[size];
            this.SetTipoElement("Object");
            this.tipo = "Vector";
            this.isWrapperObject = true;
            this._size = size;
            this.valor = this;
        }

        public Vector()
        {
            this.VectorObjects = new object[10];
            this.SetTipoElement("Object");
            this.Size = 10;
            this.tipo = "Vector";
            this.isWrapperObject = true;
            this.valor = this;
        }

        public Vector(string tipoElemento)
        {
            this.VectorObjects= new object[10];
            this.Size= 10;
            this.SetTipoElement(tipoElemento);
            this.tipo = "Vector";
            this.isWrapperObject = true;
            this.valor= this;
        }

        public int size()
        {
            return this.Size;
        }

        public void reSize(int newSize)
        {
            this.Size = newSize;
        }

        private object GetVectorAsObjeto(object valor)
        {
            if (valor.GetType() == typeof(Objeto))
            {
                return ((Objeto)valor).GetValor();
            }
            else
            {
                return valor;
            }    
        }

        public void Validacao(object valor)
        {
            if (valor != null)
            {
                string tipoDoValor = valor.GetType().Name.ToLower();
                if ((tipoDoValor == "int32") || (tipoDoValor == "int64"))
                {
                    tipoDoValor = "int";
                }
                if ((tipoDoValor != tipoElemento) && (tipoElemento != "Object"))
                {
                    throw new Exception("Type of Elements of vector: " + this.GetNome() + " must be: " + tipoElemento + "!");
                }
            }
            
        }

        public void pushFront(object valor)
        {
            Validacao(valor);
            valor = GetVectorAsObjeto(valor);

            List<object> lstVetor = this.VectorObjects.ToList<object>();
            lstVetor.Insert(0, valor);
            this.VectorObjects = lstVetor.ToArray();

        }

        public object popFront()
        {

            object valor = this.VectorObjects[0];

            // remove o primeiro elemento do vetor.
            List<object> lstVetor = this.VectorObjects.ToList<object>();
            lstVetor.RemoveAt(0);

            // recalcula o vetor, com o primeiro elemento retirado.
            this.VectorObjects = lstVetor.ToArray();

            // retorna o 1o. valor.
            return valor;
        }

        public void pushBack(object valor)
        {
            Validacao(valor);
            valor= GetVectorAsObjeto(valor);

            List<object> lstVetor = this.VectorObjects.ToList<object>();
            lstVetor.Add(valor);
            this.VectorObjects = lstVetor.ToArray();
        }


        /// <summary>
        /// retorna o ultimo elemento setado no indice mais alto. é responsabilidade do
        /// desenvolvedor se houver index of bound exception, como todo código delega, por motivos
        /// de desenpenho se o codigo estiver correto.
        /// </summary>
        public object popBack()
        {
            object valor = this.VectorObjects[this.VectorObjects.Length - 1];
            
            
            List<object> lstVetor= this.VectorObjects.ToList<object>();
            lstVetor.RemoveAt(lstVetor.Count - 1);
  
            
            this.VectorObjects = lstVetor.ToArray();

            return valor;
        }

        public void insert(int index, object valor)
        {

            Validacao(valor);
            valor= GetVectorAsObjeto(valor);
            this.VectorObjects[index] = valor;  
         }

        public void remove(int index)
        {
            List<object> lstVetor = this.VectorObjects.ToList<object>();
            lstVetor.RemoveAt(index);

            this.VectorObjects = lstVetor.ToArray();
        }

        public void Set(int index, object valor)
        {
            Validacao(valor);
            valor = GetVectorAsObjeto(valor);

            this.VectorObjects[index] = valor;
        }

        /// <summary>
        /// esvazia o conteudo do vetor.
        /// </summary>
        public void Clear()
        {
            this._size = 0;
            List<object> lstVazio = new List<object>();
            this.VectorObjects=lstVazio.ToArray();

        }

        /// <summary>
        /// copia os dados de um vetor.
        /// </summary>
        /// <param name="vt">vetor a ser copiado.</param>
        public void Casting(object vt)
        {
            Vector vtToCopy = (Vector)vt;
            this.VectorObjects = vtToCopy.VectorObjects;
            if (vtToCopy.VectorObjects != null)
            {
                for (int x = 0; x < vtToCopy.VectorObjects.Length; x++)
                {
                    this.VectorObjects[x] = vtToCopy.VectorObjects[x];
                }
            }

            this._size = ((Vector)vt)._size;
        }




        /// <summary>
        /// retorna o tipo dos elementos constituinte.
        /// </summary>
        /// <returns></returns>
        public new string GetTipoElement()
        {

            return this.tipoElemento;
        }

        public new void SetTipoElement(string newType)
        {
            this.tipoElemento = newType;
        }



        /// <summary>
        /// faz o processamento de instanciacao do vector, contido numa expressao chamada de metodo.
        /// </summary>
        /// <param name="exprss">expresssao chamada de metodo da instanciacao.</param>
        /// <param name="escopo">contexto onde a expressao esta.</param>
        public void Create(int size)
        {
    
            this.VectorObjects = new object[size];
            this._size= size;
            this.isWrapperObject = true;

        }


        /// <summary>
        /// obtem o i-esimo elemento.
        /// </summary>
        /// <param name="index">indice do elemento.</param>
        /// <returns></returns>
        public object Get(int index)
        {
            if (index<this.size())
            {
                return this.VectorObjects[index];
            }
            else
            {
                return null;
            }
        }



        /// <summary>
        /// obtem o elemento no i-esimo [indice].
        /// </summary>
        /// <param name="indice">indice do elemento.</param>
        /// <returns></returns>
        public object GetElement(int indice)
        {
            if (indice < this.size()) 
            {
                return this.VectorObjects.GetValue((int)indice);
            }
            else
            {
                return null;
            }
           

        }

        /// <summary>
        /// adiciona um elemento no final do vetor.
        /// </summary>
        /// <param name="element">objeto a adicionar.</param>
        public void Append(object element)
        {
            List<object> lstElement= VectorObjects.ToList<object>();
            lstElement.Add(element);
            VectorObjects = lstElement.ToArray();


            if (lstElement.Count > _size)
            {
                this._size++;
            }
        }


        public override string ToString()
        {
            string str = "";
            if ((this.VectorObjects != null)  && (this.VectorObjects.Length<15))
            {
                for (int x = 0; x < size(); x++)
                {
                    if (VectorObjects[x] != null)
                    {
                        str += VectorObjects[x].ToString()+",";
                    }


                }
                str += "...";
                str=str.Remove(str.Length-1);
            }
            return str;
        }



        public void Print(string message)
        {
            System.Console.Write(message + ": [");
            if (this.VectorObjects != null)
                for (int x = 0; x < VectorObjects.Length; x++)
                {
                    if (VectorObjects[x] != null)
                    {
                        System.Console.Write(VectorObjects[x].ToString() + " ");
                    }
                        

                }

            System.Console.WriteLine("]");
        }


        /// <summary>
        /// seta um elemento, que vem de uma expressao, no indice da expressao parametro.
        /// </summary>
        /// <param name="valor">valor do elemento.</param>
        /// <param name="index">indice dentro do vetor.</param>
        /// <exception cref="Exception"></exception>
        public void SetElement(object index, object valor)
        {
            Validacao(valor);
            this.VectorObjects[(int)index] = valor;
        }

     
     
        public new class Testes : SuiteClasseTestes
        {
            public Testes() : base("testes estrutura de dados Vector")
            {
            }

            public void TesteInstanciacaoVector(AssercaoSuiteClasse assercao)
            {
                Vector umVetor = new Vector(8);

                umVetor.Set(0, 1);
                object valor = umVetor.Get(0);
                assercao.IsTrue(valor.ToString() == "1", "valor de set-get validacao.");


                umVetor.Print("vetor antes das operacoes");


                umVetor.pushFront(6);
                umVetor.Print("adicao na frente valor 6: elements: "+umVetor.size());
                assercao.IsTrue(umVetor.size() == 11,"operacao push front");




                umVetor.pushBack(4);
                umVetor.Print("adicao na tras valor 4 elements: "+umVetor.size());
                assercao.IsTrue(umVetor.size() == 11, "operacao push back");




                umVetor.insert(2, 3);
                umVetor.Print("inserindo um elemento na posicao 2, valor 3 elements:"+umVetor.size());
                assercao.IsTrue(umVetor.size() == 11, "operacao insert");


                object valor2= umVetor.popBack();
                umVetor.Print("valor retirado operacao pop back.elements: "+umVetor.size());
                assercao.IsTrue(umVetor.size() == 11);


                object valor3=umVetor.popFront();
                umVetor.Print("valor retirado operacao pop front elements: "+umVetor.size());
                assercao.IsTrue(umVetor.size() == 11);



                // teste automatizado.
                assercao.IsTrue(umVetor.size() == 11, "validacao de tamanho do vetor apos operacoes.");

            }
        }
    }
   
}

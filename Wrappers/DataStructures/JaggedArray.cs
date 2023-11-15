using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using parser;

namespace Wrappers.DataStructures
{
    public class JaggedArray: Objeto
    {

        /// <summary>
        /// faz o casting de um objeto para o jagged array objeto.
        /// </summary>
        public new object valor
        {
            get
            {
                return (object)this;
            }
            set
            {
                JaggedArray jValor = (JaggedArray)value;
                this.array = jValor.array;
            }

        }


        // array do JaggedArray.        
        private List<List<object>> array;


        /// <summary>
        /// faz o casting entre o objeto parametro, para um jagged array.
        /// </summary>
        /// <param name="objToCopy">object para conversao.</param>
        public void Casting(object objToCopy)
        {
            if (objToCopy != null)
            {
                JaggedArray j1 = (JaggedArray)objToCopy;
                if (this.array == null)
                {
                    this.array = new List<List<object>>();
                }
                else
                {
                    this.array.Clear();
                }

                for (int x=0; x< j1.array.Count; x++)
                {

                    this.array.Add(new List<Object>());

                    for (int y = 0; y < j1.array[x].Count; y++)
                    {
                        this.array[x].Add(j1.array[x][y]); 
                    }
                }
            }
            else
            {
                this.array = null;
            }
        }

        /// metodos: 
        /// insertRow
        /// insertColumn.
        /// resize
        /// getSize
        /// insertElement
        /// setElement
        /// getElement

        public override string ToString()
        {
            string str = "";
            if ((array != null) && (array.Count > 0))
            {
                for (int i = 0; i < array.Count; i++)
                {
                    if (array[i] != null)
                    {
                        for (int j=0;j< array[i].Count; j++)
                        {
                            str += array[i][j].ToString()+",";
                        }
                    }
                }
                str = str.Remove(str.Length - 1);
            }
            return str;
        }
        public JaggedArray(int sizeInRows)
        {
            array = new List<List<object>>();

            // inicia o jagged array com [sizeInRows] linhas.
            for (int i = 0; i < sizeInRows; i++)
                array.Add(new List<object>());

            this.tipo = "JaggedArray";
        }

        public JaggedArray()
        {
            array=new List<List<object>>();
            for (int i = 0; i < 10; i++)
            {
                array.Add(new List<object>());
            }
            this.tipo = "JaggedArray";
        }


        public void InsertRow(JaggedArray arrayToInsert, int rowToInsert)
        {
            array[array.Count - 1].Add(arrayToInsert.array[rowToInsert]);
        }
        public void InsertColumn(JaggedArray arrayToInsert, int colFrom, int colTo)
        {
            array.Insert(colTo, arrayToInsert.array[colFrom]);
        }

        public void ReSize(int indexRow, int sizeNew)
        {
            int sizeOld = array[indexRow].Count;

            int sizeToChange = sizeNew - sizeOld;
            for (int x = 0; x < sizeToChange; x++)
                array[indexRow].Add(0);

        }

        public int GetSize(int indexRow)
        {
            if (indexRow < array.Count)
                return array[indexRow].Count;
            else
                return 0;
        }

        /// <summary>
        /// retorna o tipo dos elementos constituinte.
        /// </summary>
        /// <returns></returns>
        public new string GetTipoElement()
        {
            return "Object";
        }

        public void AddElement(int indexRow, object valor)
        {
            array[indexRow].Add(valor);                
        }


        public void SetElement(int index1, int index2, object valor)
        {
            array[index1][index2] = valor;
        }

        public object GetElement(int index1, int index2)
        {
            return array[index1][index2];
        }



        /// <summary>
        ///  cria um objeto jagged array, a partir de uma chamada de metodo.
        /// </summary>
        /// <param name="obj">objeto currente.</param>
        /// <param name="size">dimensao inicial do objeto.</param>
        public void Create(int size)
        {
            this.array = new List<List<object>>();
            for (int x = 0; x < size; x++)
            {
                this.array.Add(new List<object>());
            }
            this.isWrapperObject = true;
        }



        public void Print(string message)
        {
            System.Console.WriteLine(message);  

            for (int row=0;row<array.Count;row++)
            {
                if (array[row] != null)
                {
                    for (int col = 0; col < array[row].Count; col++)
                    {
                        if (array[row][col] != null)
                        {
                            System.Console.Write(array[row][col].ToString()+" ");
                        }
                    }
                        
                }
                    
            }
        }
        public new class Testes : SuiteClasseTestes
        {
            public Testes() : base("testes para classe wrapper structure JaggedArray")
            {
            }

            public void TestesOperacoesJaggedArray(AssercaoSuiteClasse assercao)
            {
                JaggedArray jaggedArray = new JaggedArray(1);

                jaggedArray.Print("tamanho 1a linha: " + jaggedArray.GetSize(0));
                assercao.IsTrue(jaggedArray.GetSize(0) == 0);

                Random rand = new Random();

                for (int x = 0;  x < 15; x++) 
                jaggedArray.AddElement(0, rand.Next(15));
                jaggedArray.Print("");
                assercao.IsTrue(jaggedArray.GetSize(0) == 15);


                jaggedArray.ReSize(0, 20);
                jaggedArray.Print("row 0: " + jaggedArray.GetSize(0));
                assercao.IsTrue(jaggedArray.GetSize(0) == 20);
                


            }
        }
    }
}

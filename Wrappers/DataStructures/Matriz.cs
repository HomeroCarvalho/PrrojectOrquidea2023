using parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wrappers;

using MathNet.Numerics.LinearAlgebra;
namespace Wrappers.DataStructures
{
    public class Matriz : Objeto
    {
        /// <summary>
        /// objeto importado de bblioteca especializada em tb matrizes.
        /// </summary>
        private Matrix<double> _mtData;

        private int lines;
        private int colummns;

        /// <summary>
        /// faz o casting convertendo para um dicionario um object.
        /// </summary>
        public new object valor
        {
            get
            {
                return (object)this;
            }
            set
            {
                Matriz mtValor = (Matriz)value;
                this._mtData = mtValor._mtData;
                this.lines = mtValor.lines;
                this.colummns = mtValor.colummns;
            }

        }
        
        public Matriz()
        {
            this.lines = 5;
            this.colummns = 6;
            this._mtData = Matrix<double>.Build.Dense(lines, colummns);
            this.tipo = "Matriz";
            this.tipoElemento = "double";
            this.SetTipoElement("double");
        }


        public Matriz(int lines, int colummns)
        {
            this.lines = lines;
            this.colummns = colummns;
            _mtData = Matrix<double>.Build.Dense(lines, colummns);
            this.tipo = "Matriz";
            this.tipoElemento= "double";
            this.SetTipoElement("double");
        }



        /// <summary>
        /// copia os dados de uma matriz.
        /// </summary>
        /// <param name="vt">matriz a ser copiado.</param>
        public void Casting(object vt)
        {
            Matriz vtToCopy = (Matriz)vt;
            this._mtData = vtToCopy._mtData;
            this.lines = vtToCopy.lines;
            this.colummns=vtToCopy.colummns;

            if (vtToCopy._mtData != null)
            {
                for (int lin = 0; lin < vtToCopy.lines; lin++)
                {
                    for (int col = 0; col < vtToCopy.colummns; col++)
                    {
                        this._mtData[lin,col]= vtToCopy._mtData[lin,col];
                    }
                }
                
            }

        }

        public override string ToString()
        {
            string str = "[";
            if (_mtData != null)
            {
                for (int lin=0;lin<lines;lin++)
                {
                    for (int col=0;col<colummns;col++)
                    {
                        str += "[" + _mtData[lin, col].ToString("N0") + "]";
                    }
                }
            }
            str+= "]";
            return str;
        }
        public object GetElement(int lin, int col)
        {
            return _mtData[lin, col];
        }

        public void SetElement(int lin, int col, object valor)
        {
            _mtData[lin, col] = double.Parse(valor.ToString());
        }



        /// <summary>
        /// instancia um objeto wrapper matriz, a partir de uma chamda de metodo. 
        /// </summary>
        /// <param name="mt">objeto matriz instanciado e registrado.</param>
        /// <param name="lines">numero de linhas da matriz.</param>
        /// <param name="columns">numero de colunas da matriz.</param>
        public void Create(int lines, int columns)
        {
            this._mtData = Matrix<double>.Build.Dense(lines, columns);
            this.tipo = "Matriz";
            this.tipoElemento= "double";
            this.isWrapperObject = true;

        }

        public void Inverse()
        {
            _mtData = _mtData.Inverse();
        }

        public void Print()
        {
            System.Console.WriteLine(_mtData.ToString());
        }

        public new class Testes : SuiteClasseTestes
        {
            public Testes() : base("testes para wrapper estructure matriz")
            {
            }

            public void testesGetSetElements(AssercaoSuiteClasse assercao)
            {
                Matriz matriz1 = new Matriz(3, 3);


                matriz1.SetElement(0, 0, 1);
                matriz1.SetElement(1, 1, 2);
                matriz1.Print();


                matriz1.SetElement(0, 0, matriz1.GetElement(1, 1));
                matriz1.Print();


                assercao.IsTrue((double)matriz1.GetElement(0, 0) == (double)2);
            }
          
        }
    }
}

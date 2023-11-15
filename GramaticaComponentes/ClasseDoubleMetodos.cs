using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace parser
{
    public class MetodosDouble: ImportaMetodosClassesBasicas
    {
        public static int contadorNomes = 0;


        public MetodosDouble()
        {
            this.LoadMethods("double", typeof(MetodosDouble));

            


        }
        
        public double root2(double x)
        {
            double y = Math.Sqrt(x);
            return y;
        }

        public double power(double x, double expoent)
        {
        
            double y = Math.Pow(x, expoent);
            return y;
            
        }

        public double log(double x, double base_)
        {
            double y = Math.Log(x, base_);
            return y;
            
        }

        public double round(double x)
        {
            double y = Math.Round(x);
            return y;
        }

        public string toText(double x)
        {
            string str = x.ToString();
            return str;
        }

        public double abs(double x)
        {
            double y = Math.Abs(x);
            return y;
        }

        public double clamp(double x, double begin, double end)
        {

            if (x < begin)
                return begin;
            else
            if (x > end)
                return end;
            else
                return x;
        }

        public double atan(double x, double dx, double dy)
        {
            double y = Math.Atan2(dy, dx);
            return y;
        }

        public double asin(double x)
        {
            double y = Math.Asin(x);
            return y;
        }

        public double acos(double x)
        {
            double y = Math.Acos(x);
            return y; ;
        }

        public double sin(double x)
        {
            double y = Math.Sin(x);
            return y;
        }

        public double cos(double x)
        {
            double y = Math.Cos(x);
            return y;
        }

        public double tan(double x)
        {
            double y = Math.Tan(x);
            return y;
        }

        public double sinh(double x)
        {
            double y = Math.Sinh(x);
            return y;
        }

        public double cosh(double x)
        {
                double y = Math.Cosh(x);
                return y;
        }

        public double tanh(double x)
        {
            double y = Math.Tanh(x);
            return y;
        }

        // inclusão de um double x, para compatibilizar chamadas de metodo como x.E(), na anotação OO.
        public double E(double x)
        {
            double y = Math.E;
            return y;
        }


        // inclusão de um double x, para compatibilizar chamadas de metodo como x.E(), na anotação OO.
        public double PI(double x)
        {
            double y = Math.PI;
            return y;
        }


        public class Testes : SuiteClasseTestes
        {
            public Testes() : base("testes para metodos da classe double")
            {
            }

            public void TesteLoadMetodosDouble(AssercaoSuiteClasse assercao)
            {
                LinguagemOrquidea linguagem = LinguagemOrquidea.Instance();

                Classe classeDouble = linguagem.GetClasses().Find(k => k.nome == "double");
                List<Metodo> metodoList = new List<Metodo>();

                foreach (Metodo umMetodo in classeDouble.GetMetodos())
                {
                    System.Console.WriteLine("metodo: {0}", umMetodo.nome);
                }

                System.Console.ReadLine();
            }
        }
    }
}

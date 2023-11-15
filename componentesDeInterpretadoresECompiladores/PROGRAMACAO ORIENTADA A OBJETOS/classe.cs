using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;


using System.Reflection;
using System;
using parser.ProgramacaoOrentadaAObjetos;
namespace parser
{
    /// <summary>
    /// bloco básico para linguagens orientada a objeto, como o orquidea.
    /// </summary>
    public class Classe
    {

        /// <summary>
        /// nome da classe.
        /// </summary>
        public string nome { get; set; }
        public string nomeLongo { get; set; }

        /// <summary>
        /// acessor da classe: public, protected, ou private.
        /// </summary>
        public string acessor { get; set; }
  


        /// <summary>
        /// escopo do corpo da classe.
        /// </summary>
        public Escopo escopoDaClasse { get; set; }

        /// <summary>
        /// tokens da classe.
        /// </summary>
        public List<string> tokensDaClasse { get; set; }




        public delegate object UmMetodoDaClasse(params object[] parametros);
        public enum tipoBluePrint { EH_CLASSE, EH_INTERFACE };



        /// <summary>
        /// classes herdadas da classe.
        /// </summary>
        public List<Classe> classesHerdadas { get; set; }
        
        /// <summary>
        /// interfaces da classe.
        /// </summary>
        public List<Classe> interfacesHerdadas { get; set; }




        /// <summary>
        /// instrucoes do corpo da classe.
        /// </summary>
        public List<Instrucao> instrucoesDoCorpoDaClasse { get; set; }
     
        /// <summary>
        /// construtores possiveis da classe.
        /// </summary>
        public List<Metodo> construtores { get; set; }


        /// <summary>
        /// lista de proprieades estaticas da classe.
        /// </summary>
        public List <Objeto> propriedadesEstaticas { get; set; }





        public string GetNome()
        {
            return nome;
        }
        public List<Metodo> GetMetodos()
        {
            return metodos;
        }

        public List<Metodo> GetMetodo(string nomeMetodo)
        {
            return metodos.FindAll(k => k.nome.Equals(nomeMetodo));
        }
        public List<Operador> GetOperadores()
        {
            return operadores;
        }
        public List<Objeto> GetPropriedades()
        {
            return propriedades;
        }

        public Objeto GetPropriedade(string nomeProp)
        {
            return propriedades.Find(k => k.GetNome() == nomeProp);
        }

        public Operador GetOperador(string nomeOperador)
        {
            return operadores.Find(k => k.nome.Equals(nomeOperador));
        }


        public Objeto actual { get; set; }


        public List<Metodo> metodos = new List<Metodo>();
        public List<Objeto> propriedades = new List<Objeto>();
        public List<Operador> operadores = new List<Operador>();

        /// <summary>
        /// se true terminou a compilação.
        /// </summary>
        public bool isBuildMethods = true;

        private static LinguagemOrquidea linguagem;

        MethodInfo[] metodoReflexao { get; set; }
        ConstructorInfo[] construtoresReflexao { get; set; }
        
        public Classe()
        {
            if (linguagem == null)
                linguagem = LinguagemOrquidea.Instance();
            this.metodos = new List<Metodo>();
            this.operadores = new List<Operador>();
            this.propriedades = new List<Objeto>();
            this.tokensDaClasse = new List<string>();
            this.construtores = new List<Metodo>();
            this.propriedadesEstaticas = new List<Objeto>();
            this.classesHerdadas = new List<Classe>();
            this.interfacesHerdadas = new List<Classe>();
            this.GetInfoReflexao();
        }
        public Classe(string acessor, string name, List<Metodo> methods, List<Operador> operadores, List<Objeto> propriedades):base()
        {
          
            this.acessor = acessor;
            this.nome = name;
            this.tokensDaClasse = new List<string>();
            this.metodos = new List<Metodo>();
            this.propriedades = new List<Objeto>();
            this.propriedadesEstaticas = new List<Objeto>();
            this.operadores = new List<Operador>();
            this.construtores = new List<Metodo>();
            this.classesHerdadas = new List<Classe>();
            this.interfacesHerdadas = new List<Classe>();
            // adiciona os métodos da classe.
            if (methods != null)
                for (int i = 0; i < methods.Count; i++)
                    this.metodos.Add(methods[i]);

            // adiciona as propriedades da classe.
            if (propriedades != null)
                for (int i = 0; i < propriedades.Count; i++)
                    this.propriedades.Add(propriedades[i]);

            // adiciona os operadores da classe.
            if (operadores != null)
                for (int i = 0; i < operadores.Count; i++)
                {
                    operadores[i].tipoRetorno = this.nome; // seta a classe a qual o operador pertence.
                    this.operadores.Add(operadores[i]);
                }
            this.GetInfoReflexao();
            RepositorioDeClassesOO.Instance().RegistraUmaClasse(this);

       } // constructor


        private void GetInfoReflexao()
        {
            string pathAssembly = Path.GetFullPath("PrrojectOrquidea2023.dll");
            Assembly assemblyParserOrquidea = Assembly.LoadFrom(pathAssembly);

            List<Type> types = assemblyParserOrquidea.GetTypes().ToList<Type>();
            Type tipoDaClasse = types.Find(k => k.Name.Equals(this.nome));
            
            
            if (tipoDaClasse != null)
            {
                this.metodoReflexao = tipoDaClasse.GetMethods();
                this.construtoresReflexao = tipoDaClasse.GetConstructors();
            }
        }

     
        /// <summary>
        /// faz uma chamada do método constructor de objeto importados, que tem os parâmetros especificados na entrada.
        /// </summary>
        public object CallConstructor(Type tipoClasse, object[] parametrosConstructor)
        {
            ConstructorInfo[] construtores = tipoClasse.GetConstructors();
            if (construtores.Length > 0)
                return construtores[0].Invoke(parametrosConstructor);
            else
                return null;
        } // GetConstructor()

        /// faz uma chamada de um método via reflexao.
        public object CallAMethod(object objectCaller, string nameMethod, object[] parametersCall)
        {
            Metodo metodoInvocado = this.GetMetodos().Find(k => k.nome.Equals(nameMethod));
            return metodoInvocado.InfoMethod.Invoke(objectCaller, parametersCall);
        }

        /// Obtem para um arquivo texto, a descrição da classe, com propriedades, metodos, e operadores.
        public void Save(Classe classeAGuardar)
        {
            Stream stream = new FileStream("classe_" + this.nome + ".txt", FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(stream);
          
            SaveClassePropriedadesMetodosOperadores(classeAGuardar, writer);  // escreve recursivamente todas propriedades, metodos, ou operadores, herdados ou nao.

            writer.Close();
            stream.Close();

            writer.Dispose();
            stream.Dispose();
        }  // Save()

        private void SaveClassePropriedadesMetodosOperadores(Classe classeAGuardar, StreamWriter writer)
        {
            string name = "";
            if (RepositorioDeClassesOO.Instance().GetClasse(classeAGuardar.nome) != null)
                name = "classe";
            else
            if (RepositorioDeClassesOO.Instance().ObtemUmaInterface(classeAGuardar.nome) != null)
                name = "interface";
            writer.WriteLine(name + ": " + classeAGuardar.nome + TextoDescritivo(classeAGuardar) + "\n");
            writer.WriteLine();
            if ((classeAGuardar.classesHerdadas != null) && (classeAGuardar.classesHerdadas.Count > 0))
                for (int x = 0; x < classeAGuardar.classesHerdadas.Count; x++)
                {
                    writer.WriteLine("classe Herdada: \n");
                    SaveClassePropriedadesMetodosOperadores(classeAGuardar.classesHerdadas[x], writer); // escrita recursiva, para capturar todos metodos, propriedades, e operadores de todas classes herdadas.
                    writer.WriteLine();
                }
            if ((classeAGuardar.interfacesHerdadas != null) && (classeAGuardar.interfacesHerdadas.Count > 0))
                for (int x = 0; x < classeAGuardar.interfacesHerdadas.Count; x++)
                {
                    writer.WriteLine("interface Herdada: \n");
                    SaveClassePropriedadesMetodosOperadores(classeAGuardar.interfacesHerdadas[x], writer);
                    writer.WriteLine();
                }
        

        }

        /// <summary>
        /// carrega uma classe em um arquivo.
        /// </summary>
        /// <param name="nomeClasse">nome da classe a carregar.</param>
        /// <returns></returns>
        public static Classe Load(string nomeClasse)
        {
            LinguagemOrquidea linguage = LinguagemOrquidea.Instance();
            Stream stream = new FileStream("classe_" + nomeClasse + ".txt", FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(stream);

            // le todos tokens da classe, em uma linha só.
            string todosTokensDaClasse = reader.ReadLine();
            todosTokensDaClasse = todosTokensDaClasse.Trim(' ');

            // cria um extrator de classes, métodos e propriedades.
            ExtratoresOO extratores = new ExtratoresOO(new Escopo(new List<string>() { todosTokensDaClasse }), new List<string>() { todosTokensDaClasse });
            Classe umaClasse = extratores.ExtaiUmaClasse(Classe.tipoBluePrint.EH_CLASSE);
            if (umaClasse == null)
                return null;
            reader.Close();
            stream.Close();

            reader.Dispose();
            stream.Dispose();
            return umaClasse;
        } // Load()


        public override string ToString()
        {
            return TextoDescritivo(this);
        } // ToString()


        public static string TextoDescritivo(Classe classe)
        {
            string str = "nome: " + classe.GetNome() + "  \n";

            if ((classe.propriedadesEstaticas != null) && (classe.propriedadesEstaticas.Count > 0))
            {
                str += "propriedades estaticas: \n";
                for (int x = 0; x < classe.propriedadesEstaticas.Count; x++)
                    str += classe.propriedadesEstaticas[x].ToString() + "  \n";
            }
            str += "\n";
            str += "\n";
            if ((classe.propriedades != null) && (classe.propriedades.Count > 0))
            {
                str += "propriedades: \n";
                for (int x = 0; x < classe.propriedades.Count; x++)
                    str += classe.propriedades[x].ToString() + "  \n";
            } // if

            str += "\n";
            str += "\n";
            if ((classe.metodos != null) && (classe.metodos.Count > 0))
            {
                str += "metodos: \n";
                for (int x = 0; x < classe.metodos.Count; x++)
                    str += classe.metodos[x].ToString() + " \n";
            } // if

            str += "\n";
            str += "\n";
            if ((classe.operadores != null) && (classe.operadores.Count > 0))
            {
                str += "operadores:  \n";
                for (int x = 0; x < classe.operadores.Count; x++)
                    str += classe.operadores[x].ToString();
            }
            return str;

        }
    } // class Classe

} // namespace parser

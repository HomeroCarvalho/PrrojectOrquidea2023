using parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Modulos
{
    public class Library
    {

        public Library()
        {
            nameFilesLibrariesRegistred["Prompt"] = typeof(Prompt);
        }

        /// <summary>
        /// classes que implementam uma biblioteca.
        /// </summary>
        private List<Type> bibliotecas = new List<Type>();


        /// <summary>
        /// lista de todas bibliotecas padrão da linguagem.
        /// </summary>
        private static Dictionary<string, Type> nameFilesLibrariesRegistred = new Dictionary<string, Type>();

        /// <summary>
        /// informações de metodos importados.
        /// </summary>
        public List<MethodInfo> metodosBibliotecas = new List<MethodInfo>();



        public object RunMethod(string nameLibrary, string nameMethod, params object[] parmametersMethod)
        {
            // escopo vazio, não interfere na execução de uma função da classe basr.
            Escopo escopo = new Escopo(new List<string>());
            
            // tenta encontrar a classe da biblioteca, na lista de nomes registrados da biblioteca.
            Type typeLibrary = null;
            nameFilesLibrariesRegistred.TryGetValue(nameLibrary, out typeLibrary);
            

            // tenta encontrar os dados do metodo importado.
            
            MethodInfo infoMetodo = metodosBibliotecas.Find(k => k.Name.Equals(nameMethod));
            if (infoMetodo == null)
            {
                Import(nameLibrary);
                infoMetodo = metodosBibliotecas.Find(k => k.Name.Equals(nameMethod));
            }




            Objeto[] parametersEmpty = new Objeto[1];
            // instancia uma função, com dados do metodo, e parametros.
            Metodo fncMethod = new Metodo(typeLibrary.Name, "public", nameMethod, infoMetodo, null, parametersEmpty);

            // executa a função;
            return fncMethod.ExecuteAFunction(parmametersMethod.ToList<object>(), escopo);

          

        }

        /// <summary>
        /// importa todas classes da linguagem base, de um arquivo .exe, ou .dll.
        /// </summary>
        /// <param name="path">path de arquivo.</param>
        public static void ImportLibrayFromAssembly(string path)
        {
            ImportadorDeClasses importador = new ImportadorDeClasses(path);
        }


        /// <summary>
        /// importa classes da linguagem base, de um arquivo .exe ou .dll.
        /// Libraries devem estar contida numa classe, ou conjunto de classe definidas.
        /// </summary>
        /// <param name="path">path do arquivo Assembly, .exe ou .dll, que contem a library.</param>
        /// <param name="namesLibrary">nomes de classes da library.</param>
        public static void ImportLibrayFromAssembly(string path, string[] namesLibrary)
        {
            ImportadorDeClasses importador = new ImportadorDeClasses(path);
            foreach(string umaBiblioteca in namesLibrary)
            {
                importador.ImportAClassFromAssembly(umaBiblioteca);
            }     
        }



        /// <summary>
        /// importa libraries já dentro do escopo do codigo do projeto.
        /// se tiver num arquivo externo, utiliza-se [ImportLibrayFromAssembly()].
        /// </summary>
        /// <param name="nameLibrary">nome da bibioteca, seu [Type].</param>
        /// <exception cref="Exception"></exception>
        public void Import(string nameLibrary)
        {
           try
            {

                Type classeDaBiblioteca = null;
                nameFilesLibrariesRegistred.TryGetValue(nameLibrary, out classeDaBiblioteca);

                if (classeDaBiblioteca == null)
                    throw new Exception("library name: " + nameLibrary + " is invalid!");


                MethodInfo[] metodosInfo = classeDaBiblioteca.GetMethods();
                if (metodosInfo != null)
                    this.metodosBibliotecas.AddRange(metodosInfo);
                
            }
            catch
            {
                throw new Exception("error in file of library, not found!");
            }
        }

    }
}
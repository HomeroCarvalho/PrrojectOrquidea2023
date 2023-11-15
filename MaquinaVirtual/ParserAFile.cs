using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using parser.ProgramacaoOrentadaAObjetos;
using System.Drawing;
using Modulos;
using System.Diagnostics.Eventing.Reader;

namespace parser
{
    
    /// <summary>
    /// converte uma arquivo de texto, em uma lista de tokens da linguagem orquidea.
    /// </summary>
    public class ParserAFile
    {
        private List<string> tokens;
        private List<string> code;
       

        private List<char> caracteresRemover = new List<char>() { '\t', '\n' };

        public ParserAFile(string pathFile)
        {
            this.Parser(pathFile);
        }
        public List<string> GetCode()
        {
            return this.code;
        }

        public List<string> GetTokens()
        {
            return this.tokens;
        }

        /// <summary>
        /// lista de erros no carregamento do arquivo de codigo.
        /// </summary>
        public List<string> msgErros= new List<string>();

      

        public static void InitSystem()
        {
            TablelaDeValores.expressoes = new List<Expressao>();
            LinguagemOrquidea.Instance().Aspectos = new List<Aspecto>();
            
        }

        /// <summary>
        /// le, obtem tokens, compila e executa um programa orqquidea.
        /// </summary>
        /// <param name="fileName">nome do arquivo contendo o programa.</param>
        /// <exception cref="Exception"></exception>
        public static void ExecuteAProgram(string fileName)
        {
            ParserAFile parser = new ParserAFile(fileName);
            if ((parser != null) && (parser.GetTokens() != null) && (parser.GetTokens().Count > 0))
            {
                ProcessadorDeID compilador = new ProcessadorDeID(parser.GetTokens());
                compilador.Compilar();
                if ((compilador.GetInstrucoes() != null) && (compilador.GetInstrucoes().Count > 0))
                {
                    ProgramaEmVM programaVM = new ProgramaEmVM(compilador.GetInstrucoes());
                    programaVM.Run(compilador.escopo);
                }
                else
                {
                    throw new Exception("error in compile program: " + fileName + " not valid instructions was found.");
                }

            }
            else
            {
                throw new Exception("erro in parse, not found tokens valid.");
            }
        }

        /// Le o arquivo na entrada, e converte em tokens e codigos na saída.
        private void Parser(string fileName)
        {
            InitSystem();
            
            PosicaoECodigo.InitCalculoPosicoes();   // essencial para se contar a posicao de tokens, é preciso inicializar algumas propriedades estáticas.

            this.code = new List<string>();
            this.tokens = new List<string>();

            FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read); // abre um dos arquivos texto para  leitura de código de linguagem.
            StreamReader reader = new StreamReader(stream);

            int line = 0;
            while (!reader.EndOfStream) // lê o arquivo currente e constroi a lista de código e tokens.
            {
                line++;
                string lineOfCode = reader.ReadLine();

                // procura e remove comentarios de linhas, deixando apenas o codigo para o processo de compilação.
                lineOfCode = ProcessamentoLinhasComComentarios(lineOfCode);


                // PROCESSAMENTO DE INSTRUCAO MODULE.
                if (lineOfCode.Contains("Module"))
                {


                    string pathhFileModule = lineOfCode.Replace("Module", "").Trim(' ');
                    pathhFileModule = pathhFileModule.Replace('\"', ' ').Trim(' ');
                    pathhFileModule = pathhFileModule.Replace(";", "");
         
                    ParserAFile parserModule = new ParserAFile(pathhFileModule);
                    if (parserModule.msgErros.Count > 0)
                    {
                        this.msgErros.AddRange(parserModule.msgErros);
                        return;
                    }
                   
                    this.code.AddRange(parserModule.GetCode());

                }

                // PROCESSAMENTO DA INSTRUCAO LIBRARY.
                if (lineOfCode.Contains("Library"))
                {
                    string pathLibraryFile= lineOfCode.Replace("Library", "").Trim(' ');
                    pathLibraryFile = pathLibraryFile.Replace(";", "");
                    pathLibraryFile = pathLibraryFile.Replace('\"', ' ').Trim(' ');

                    int indexBeginNames = pathLibraryFile.IndexOf('{');
                    if (indexBeginNames == -1)
                    {
                        msgErros.Add("bad formation in import library, not found operators {} of import class, line: " + line.ToString());
                        return;
                    }

                    string nameLibrary = pathLibraryFile.Substring(indexBeginNames + 1);
                    nameLibrary = nameLibrary.Replace("{", "");
                    nameLibrary = nameLibrary.Replace("}", "");


                    pathLibraryFile = pathLibraryFile.Substring(0, indexBeginNames + 1 - 1);
                    pathLibraryFile = pathLibraryFile.Trim(' ');

                    

                    List<string> namesPATHfolderLibrary = ProcessamentoNomesDeLibraries(pathLibraryFile);
                    if ((namesPATHfolderLibrary == null) || (namesPATHfolderLibrary.Count == 0))
                    {
                        msgErros.Add("bad formation in import library, not found any class names, line: " + line.ToString());
                        return;
                    }

                    ImportadorDeClasses importador = new ImportadorDeClasses();
                    // importa a library, chamando a API de reflexão.
                    for (int i=0;i<namesPATHfolderLibrary.Count;i++)
                    {
                        importador.ImportLibrary(nameLibrary, namesPATHfolderLibrary[i]);
                    }
                    

                    // remove classes de executável .EXE Assembly, criando uma biblioteca Orquidea, que aceita framorks .NET 4.8, a 
                    // versao deste projeto.
                    this.TransformAssembleEXEToDllLibrary();

                }

                // remoção de caracteres de linha de texto.
                lineOfCode = lineOfCode.Replace("\t", "");
                lineOfCode = lineOfCode.Replace("\n", "");
                PosicaoECodigo.AddLineOfCode(lineOfCode);
                // lê uma linha de código, sem tokens, apenas código.
                if ((lineOfCode != null) && (lineOfCode.Length > 0))
                {
                    code.Add(lineOfCode.Trim(' '));
                }

                

            } // while

            LinguagemOrquidea linguagem = LinguagemOrquidea.Instance();
            if ((code != null) && (code.Count > 0))
                this.tokens = new Tokens(code).GetTokens(); // converte o código para uma lista de tokens.

        } // Parser()


        /// <summary>
        /// elimina textos de comentários (operador: \\).
        /// </summary>
        /// <param name="lineOfCode">linha onde possa estar um comentario.</param>
        /// <returns>retorna a linha de codigo apenas com o codigo, sem comentarios.</returns>
        private static string ProcessamentoLinhasComComentarios(string lineOfCode)
        {
            int indiceComentario = lineOfCode.IndexOf(@"//");
            if (indiceComentario != -1)
            {
                lineOfCode = lineOfCode.Remove(indiceComentario, lineOfCode.Length - indiceComentario);
            }

            return lineOfCode;
        }


        /// <summary>
        /// extrai nomes de bibliotecas, instrucao library.
        /// </summary>
        /// <param name="lineOfCode">linha de codigo contendo a instrução Library.</param>
        /// <returns></returns>
        private List<string> ProcessamentoNomesDeLibraries(string line)
        {
            string lineOfCode = line.Replace("{", "");
            lineOfCode = lineOfCode.Replace("}", "");


            if ((lineOfCode == null) || (lineOfCode.Length == 0))
            {
                return null;
            }

            string[] nomeClasses = lineOfCode.Split(new string[] { ",", " "}, StringSplitOptions.RemoveEmptyEntries);
            if ((nomeClasses != null) || (nomeClasses.Length > 0))
            {
                
                for (int i = 0; i < nomeClasses.Length; i++)
                {
                    nomeClasses[i] = nomeClasses[i].Trim(' ');
                    nomeClasses[i] += ".dll";
                    DirectoryInfo dirLibs= new DirectoryInfo(@"libs\");

                    // verifica se o diretorio de libs existe, para dar continuidade a obtenção do arquivo da library currente.
                    if (dirLibs.Exists)
                    {
                        FileInfo[] files = dirLibs.GetFiles(); 
                        if (files!=null && files.Length > 0)
                        {
                            List<FileInfo> lstFilesInfo= files.ToList();
                            FileInfo nameLibInDirectory = lstFilesInfo.Find(k => k.Name.Equals(nomeClasses[i].Trim()));
                            if (nameLibInDirectory != null)
                            {
                                nomeClasses[i]= nameLibInDirectory.FullName;
                                continue;
                            }
                            else
                            {
                                msgErros.Add("library" + nomeClasses[i] + " not found, verify path, must be in project folder, or in libs folder. libraries are in .dll file format");
                                return null; 
                            }
                        }
                    }
                }
                return nomeClasses.ToList();
            }
            else
            {
                this.msgErros.Add("bad formatation of declaring Library: not found none names to import");
                return null;
            }

        }


        /// <summary>
        /// converte uma Assembly .EXE para um .NET bibliotecaa, removendo partes de executável.
        /// Um dado é que na .NET biblioteca .EXE é que não pode haver execuções de código em Program.
        /// </summary>
        private void TransformAssembleEXEToDllLibrary()
        {
            string typeToRemove = "Program";
            RepositorioDeClassesOO.Instance().RemoveClasse(typeToRemove);


        }
        public class Testes : SuiteClasseTestes
        {
            public Testes() : base("testes de parser file")
            {
            }

            public void TesteCompilacaoExecucaoPromptSsWrite(AssercaoSuiteClasse assercao)
            {
                string nameFile = @"programasTestes\programaLeNome.txt";
                ParserAFile.ExecuteAProgram(nameFile);

                System.Console.WriteLine("Pressione ÈNTER para terminar o teste");
                System.Console.ReadLine();
            }
            public void TesteModulesAndLibraries(AssercaoSuiteClasse assercao)
            {
                ParserAFile parser = new ParserAFile(@"programasTestes\programContagensModules.txt");
                List<string> linesOfCode = parser.GetCode();
                for (int i = 0; i < linesOfCode.Count; i++)
                {
                    System.Console.WriteLine(linesOfCode[i]);
                }


                System.Console.WriteLine("Pressione ÈNTER para terminar o teste");
                System.Console.ReadLine();

            }

            

            

            public void TesteParserCompilacaoExecucao(AssercaoSuiteClasse assercao)
            {
                
                string nameFile = @"programasTestes\HelloWorld.txt";
                ParserAFile.ExecuteAProgram(nameFile);

                System.Console.WriteLine("Pressione ÈNTER para terminar o teste");
                System.Console.ReadLine();
            }
            public void TesteLinhasDeCometarios(AssercaoSuiteClasse assercao)
            {
                ParserAFile parser = new ParserAFile(@"programasTestes\programaFatorialComComentarios.txt");
                List<string> linesOfCode = parser.GetCode();
                for  (int i = 0; i < linesOfCode.Count; i++)
                {
                    System.Console.WriteLine(linesOfCode[i]);
                }

                System.Console.WriteLine("Pressione ÈNTER para terminar o teste");
                System.Console.ReadLine();

            }

        }
    }
}

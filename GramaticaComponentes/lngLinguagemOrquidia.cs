﻿using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using parser.ProgramacaoOrentadaAObjetos;
using Modulos;
using System;
using Wrappers.DataStructures;

namespace parser
{
    /// <summary>
    /// implementa uma linguage chamada orquidea
    /// </summary>
    public class LinguagemOrquidea : UmaGramaticaComputacional
    {
   
       
        private LinguagemOrquidea() : base()
        {
            // lista de aspectos.
            Aspectos = new List<Aspecto>();
            
            
            // inicializa operadores.
            this.InicializaClassesBasicas(false);


            // obtem termos-chave da linguagem.
            this.GetTodosTermosChave().Add("aspecto");


        }

      
        private static LinguagemOrquidea linguagemOrquideaSingleton;

     
        public static LinguagemOrquidea Instance()
        {
            if (linguagemOrquideaSingleton == null)
            {
                linguagemOrquideaSingleton = new LinguagemOrquidea();
                
                if (RepositorioDeClassesOO.Instance().GetClasse(typeof(Objeto).Name) == null)
                {
                    // importa as a estruturas de dados da linguagem:  Objeto.
                    ImportadorDeClasses importer = new ImportadorDeClasses();
                    importer.ImportAClassFromApplication(typeof(Objeto));

                    /*
                    List<Funcao> operadoresObjeto = new OperadoresImplementacao().GetImplentacao("parser.Objeto");
                    List<Funcao> operadoresVetor = new OperadoresImplementacao().GetImplentacao("parse.Vetor");
                    */
                }
            }
            return linguagemOrquideaSingleton;
        }

        public static List<Operador> operadoresCondicionais { get; set; }
        public static List<Operador> operadoresBinarios { get; set; }
        public static List<Operador> operadoresUnarios { get; set; }

       
        public bool isClassToIncludeObjectCallerAsParameter(string nomeClasse)
        {
            return (nomeClasse == "string") || (nomeClasse == "double");
        }

        public void InicializaClassesBasicas(bool inicializaClassesImportadas)
        {


            if (LinguagemOrquidea.Classes != null)
            {
                if ((RepositorioDeClassesOO.Instance().GetClasses() == null) || (RepositorioDeClassesOO.Instance().GetClasses().Count == 0))
                    InicializaClassesBasicas(false);
                return;
            }



            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            // obtem os operadores codificados na classe base.
            OperadoresImplementacao implementadao = new OperadoresImplementacao();
            List<Operador> op_int = OperadoresImplementacao.operadoresInt;
            List<Operador> op_float = OperadoresImplementacao.operadoresFloat;
            List<Operador> op_double = OperadoresImplementacao.operadoresDouble;
            List<Operador> op_string = OperadoresImplementacao.operadoresString;
            List<Operador> op_bool = OperadoresImplementacao.operadoresBoolean;
            List<Operador> op_char = OperadoresImplementacao.operadoresChar;
            List<Operador> op_matriz = OperadoresImplementacao.operadoresMatriz; 

            LinguagemOrquidea.Classes = new List<Classe>();

            adicionaClasse(new Classe("public", "int", null, op_int, null));
            adicionaClasse(new Classe("public", "float", null, op_float, null));
            adicionaClasse(new Classe("public", "double", null, op_double, null));
            adicionaClasse(new Classe("public", "string", null, op_string, null));
            adicionaClasse(new Classe("public", "bool", null, op_bool, null));
            adicionaClasse(new Classe("public", "char", null, op_char, null));


            // importa classes base de funcionamento da linguagem.
            // incluindo as estruturas de dados nativa, os wrapper data estructures.
            ImportadorDeClasses importadorClassesNativas = new ImportadorDeClasses();
            ImportarClasseParaRepositorio(importadorClassesNativas, typeof(Prompt));
            ImportarClasseParaRepositorio(importadorClassesNativas, typeof(Vector));
            ImportarClasseParaRepositorio(importadorClassesNativas, typeof(JaggedArray));
            ImportarClasseParaRepositorio(importadorClassesNativas, typeof(Matriz));
            ImportarClasseParaRepositorio(importadorClassesNativas, typeof(DictionaryText));
            ImportarClasseParaRepositorio(importadorClassesNativas, typeof(Metodo));

            // metodo diferente para carregar a classe novamente, pois esta classe tem que modificar um metodo.
            Prompt.Init();
            

            // importa metodos da classe double.
            MetodosDouble importaMetodosDouble = new MetodosDouble();
            // importa metodos da classe string.
            MetodosString importaMetodosString = new MetodosString();

            GetOperadores().AddRange(op_int);
            GetOperadores().AddRange(op_float);
            GetOperadores().AddRange(op_double);
            GetOperadores().AddRange(op_string);
            GetOperadores().AddRange(op_bool);
            GetOperadores().AddRange(op_char);
            GetOperadores().AddRange(op_matriz);


            operadoresCondicionais = this.GetOperadoresCondicionais();
            operadoresBinarios = this.GetOperadoresBinarios();
            operadoresUnarios = this.GetOperadoresUnarios();

            if (inicializaClassesImportadas)
            {
                try
                {
                    ImportadorDeClasses importador = new ImportadorDeClasses("PrrojectOrquidea2023.dll");
                    importador.ImportAllClassesFromAssembly();
                }
                catch
                {
                    Log.addMessage("Erro no carregamento das classes do programa. Verifique se não mudou o nome do arquivo final compilado, tem que ser [ParserLinguagemOrquidea.exe].");
                }
            }
        } // inicializaOperadores()

        private void ImportarClasseParaRepositorio(ImportadorDeClasses importador1, Type tipoClasse)
        {
            importador1.ImportAClassFromApplication(tipoClasse);
            Classe classeImportada = RepositorioDeClassesOO.Instance().GetClasse(tipoClasse.Name);
            if (classeImportada != null)
                adicionaClasse(classeImportada);
        }

        private static void CriaOperadores(List<Operador> operadores, List<Metodo> funcoesImpl)
        {
            for (int x = 0; x < funcoesImpl.Count; x++)
            {
                string tipoDeOperador = "";
                tipoDeOperador = funcoesImpl[x].tipo;
                operadores.Add(new Operador(funcoesImpl[x].nomeClasse, funcoesImpl[x].nome, funcoesImpl[x].prioridade, funcoesImpl[x].parametrosDaFuncao, tipoDeOperador, funcoesImpl[x].InfoMethod, null));
            }
        }

        /// adiciona o operador para os operadores binários da linguagem.
        public bool AddOperator(Operador operador)
        {
            List<Operador> todosOperadoresDaLinguagem = this.GetOperadores();
            for (int op = 0; op < todosOperadoresDaLinguagem.Count; op++)
            {
                if ((todosOperadoresDaLinguagem[op].nome == operador.nome) &&
                    (todosOperadoresDaLinguagem[op].parametrosDaFuncao[0].GetTipo() == operador.parametrosDaFuncao[0].GetTipo()) &&
                    (todosOperadoresDaLinguagem[op].parametrosDaFuncao.Length > 1) &&
                    (todosOperadoresDaLinguagem[op].parametrosDaFuncao[1].GetTipo() == operador.parametrosDaFuncao[1].GetTipo())) 
                    return false;
            } // for op
            this.GetOperadores().Add(operador);
            return true;
        } // AddOperatorInLanguem()

        public  List<Aspecto> Aspectos { get; set; }

        public static List<Classe> Classes { get; set; }
        
        public List<Classe> GetClasses()
        {
            return Classes;
        } // GetClasses()


        private void adicionaClasse(Classe c)
        {
            Classes.Add(c);
            RepositorioDeClassesOO.Instance().RegistraUmaClasse(c);
        } // adicionaClasse()

        /// <summary>
        /// inicializa as produções a partir de um arquivo XML.
        /// </summary>
        public override void inicializaProducoesDaLinguagem()
        {
            if (UmaGramaticaComputacional.producoes.Count == 0)
            {
                xmlREADER_LINGUAGEM xmlreader = new xmlREADER_LINGUAGEM();
                xmlreader.LE_ARQUIVO_LINGUAGEM(producoes, "ORQUIDIA");
            } // if

            
        } // inicializaProducoesParaLinguagem()


        public  Operador GetOperador(string nome, string nomeClasse)
        {
            Classe classe = RepositorioDeClassesOO.Instance().GetClasse(nomeClasse);
            if (classe != null)
            {
                List<Operador> operadores = classe.GetOperadores().FindAll(k => k.nome == nome);
                if ((operadores == null) || (operadores.Count == 0))
                    return null;

                return operadores[0];
            }
            return null;
        }


        public override List<string> GetTodosOperadores()
        {

            List<string> nomeOperadores = base.GetTodosOperadores();

            List<Classe> classesNativas = RepositorioDeClassesOO.Instance().GetClasses();
            if (classesNativas != null)
                for (int x = 0; x < classesNativas.Count; x++)
                {
                    for (int y = 0; y < classesNativas[x].GetOperadores().Count; y++)
                    {
                        string nome = classesNativas[x].GetOperadores()[y].nome;
                        if (nome != "")
                        {
                            int indexOperador = nomeOperadores.FindIndex(k => k.Equals(nome));
                            if (indexOperador == -1)
                                nomeOperadores.Add(nome);
                        }
                    }
                } // for x
            return nomeOperadores;
        }

        public bool isOperador(string tokenOperador)
        {
            return GetOperadores().Find(k => k.nome.Equals(tokenOperador)) != null;
        }


        public override List<Operador> GetOperadores()
        {
            List<Operador> operadores = base.GetOperadores().ToList<Operador>();

            List<Classe> classes = RepositorioDeClassesOO.Instance().GetClasses();
            if ((classes != null) && (classes.Count > 0))
                for (int x = 0; x < classes.Count; x++)
                {
                    List<Operador> operadoresDaClasse = classes[x].GetOperadores();
                    if ((operadoresDaClasse != null) && (operadoresDaClasse.Count > 0))
                        operadores.AddRange(operadoresDaClasse);
                } // for x

            return operadores;
        }

        public List<Operador> GetOperadoresCondicionais() 
        {
            List<Operador> condicionais = this.GetOperadores().FindAll(k => k.GetTipoFuncao().Contains("CONDICIONAL"));
            return condicionais;
        } // GetOperadoresCondicionais() 

        public List<Operador> GetOperadoresBinarios()
        {
            List<Operador> binarios=this.GetOperadores().FindAll(k => k.GetTipo().Contains("BINARIO"));
            return binarios;

        } // GetOperadoresBinarios()

        public List<Operador> GetOperadoresUnarios()
        {
            List<Operador> unarios = this.GetOperadores().FindAll(k => k.GetTipo().Contains("UNARIO"));
            return unarios;
        } // GetOperadoresUnarios()

        public bool IsOperadorBinario(string nomeOperador)
        {
            return operadoresBinarios.FindIndex(k => k.nome == nomeOperador) != -1;
        } // IsOperadorBinario()

        public bool IsOperadorUnario(string nomeOperador)
        {
            return operadoresUnarios.FindIndex(k => k.nome == nomeOperador) != -1;
        } // IsOperadorUnario()


        public bool IsOperadorCondicional(string nomeOperador)
        {
            return operadoresCondicionais.FindIndex(k => k.nome == nomeOperador && k.GetTipoFuncao() == "CONDICIONAL") != -1;
        } // IsOperadorCondicional()


        public bool IsOperadorAritmetico(string nomeOperador)
        {
            int indexBinariosAritmeticos = operadoresBinarios.FindIndex(k => k.nome == nomeOperador && (k.GetTipoFuncao().Contains("ARITMETICO")));
            if (indexBinariosAritmeticos != -1)
                return true;
            int indexUnariosAritmeticos = this.GetOperadoresUnarios().FindIndex(k => k.nome == nomeOperador && (k.GetTipoFuncao().Contains("ARITMETICO")));
            if (indexUnariosAritmeticos != -1)
                return true;
            return false;

        } // IsOperadorAritmetico()
    } // class LinguagemOrquidea


    // classe para conversores de tipos.
    public class ConversoresDeTipos
    {

        public List<ConverteParaOutroTipo> ConversoresDinamico = new List<ConverteParaOutroTipo>();

        public delegate object ConverteParaOutroTipo(object typeAtConvert);

        public static int ConverFloatToInt(float x)
        {
            return (int)x;
        }
        public static float ConvertIntToFloat(int x)
        {
            return (float)x;
        }

        public static int ConvertByteToInt(byte b)
        {
            return (int)b;
        }

        public static byte ConvertIntToByte(int x)
        {
            return (byte)x;
        }
    }// class

} // namespace parser

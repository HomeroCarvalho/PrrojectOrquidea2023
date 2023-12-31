﻿using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using System.Reflection;
using MATRIZES;

using System.IO;

namespace parser
{
    public class OperadoresImplementacao
    {
        public string nome;
        public List<Metodo> funcaoImplOperadores = new List<Metodo>();
        public List<MethodInfo> metodosImpl = new List<MethodInfo>();

        // alguns operadores pre-definidos. Pode ser extendido, com metodo AdicionaOperadorNativo().
        List<string> operadoresPrioridade1 = new List<string>() { "+", "-" };
        List<string> operadoresPrioridade2 = new List<string>() { "*", "/" };
        List<string> operadoresPrioridade3 = new List<string>() { "^" };
        List<string> operadoresPrioridade4 = new List<string>() { "<", ">,", ">=", "<=", "==", "!=" };
        List<string> operadoresPrioridade5 = new List<string>() { "=" };
        List<string> operadoresPrioridade6 = new List<string>() { "++", "--" };


        private string nameAssembly;
        private string classeDeOperadores;
        private Assembly assemblyToImporter = null;
        private string nameClassOperators=null;
        private Escopo escopoImpl = null;     
    
        /// <summary>
        /// obtem o assembly que contem os tipos com metodos de operadores.
        /// </summary>
        /// <param name="nameAssembly"></param>
        /// <param name="escopo">contexto onde o operador estara.</param>
        public OperadoresImplementacao(string nameAssembly, string nameClassWithOperators)
        {
            this.nameAssembly = nameAssembly;
            this.classeDeOperadores = nameClassWithOperators;
            this.assemblyToImporter= Assembly.LoadFile(Path.GetFullPath(nameAssembly));
            this.nameClassOperators = nameClassWithOperators;
        }
        public OperadoresImplementacao()
        {
            operadoresInt = new List<Operador>();
            operadoresDouble = new List<Operador>();
            operadoresFloat= new List<Operador>();
            operadoresString= new List<Operador>();
            operadoresChar= new List<Operador>();   
            operadoresBoolean= new List<Operador>();
            operadoresMatriz= new List<Operador>(); 

            this.GetOperators();
        }

        public static List<Operador> operadoresInt;
        public static List<Operador> operadoresDouble;
        public static List<Operador> operadoresFloat;
        public static List<Operador> operadoresString;
        public static List<Operador> operadoresChar;
        public static List<Operador> operadoresBoolean;
        public static List<Operador> operadoresMatriz;


        /// <summary>
        /// constroi um operador a partir da metodos importados da classe base.
        /// </summary>
        /// <param name="nameClass">nome da classe importada.</param>
        /// <param name="nameMethodImpl"><nome do metodo importado./param>
        /// <param name="nameOperatorInOrquidea">nome do operador, ante a linguagem orquidea.</param>
        /// <param name="prioridade">prioridade do operador.</param>
        /// <returns></returns>
        public Operador GetOperatorComMetodoImportado(string nameMethodImpl, string nameOperatorInOrquidea, int prioridade)
        {
            if (assemblyToImporter == null)
            {
                this.assemblyToImporter = Assembly.LoadFile(Path.GetFullPath(nameAssembly));
                this.nameClassOperators = classeDeOperadores;
            }

            List<Type> classesDoAsseembly = this.assemblyToImporter.GetTypes().ToList<Type>();
            int indexClass = -1;
            for (int x = 0; x < classesDoAsseembly.Count; x++)
            {
                if (classesDoAsseembly[x].FullName == nameClassOperators)
                {
                    indexClass = x;
                    break;
                }
            }
            

            if (indexClass != -1)
            {
                int indexMethodo= classesDoAsseembly[indexClass].GetMethods().ToList<MethodInfo>().FindIndex(k => k.Name == nameMethodImpl);
                if (indexMethodo != -1) 
                {
                    MethodInfo metodoImpl = classesDoAsseembly[indexClass].GetMethods()[indexMethodo];


                    // obtem as definicoes dos parametros: seus tipos, e qtd.
                    ParameterInfo[] parametros = metodoImpl.GetParameters();
                    List<Objeto> objParametros= new List<Objeto>();
                    if (((parametros != null) && (parametros.Length > 0)) && (parametros.Length <= 2) && (parametros.Length>=1)) 
                    {
                        for (int i = 0; i < parametros.Length; i++)
                        {
                            string classeDoParametro = UtilTokens.Casting(parametros[i].ParameterType.Name);
                            objParametros.Add(new Objeto("private", classeDoParametro, "x" + i, new object()));
                        } 
                    }
                    else
                    {
                        escopoImpl.GetMsgErros().Add("metodo:" + nameMethodImpl + " dont is compatible with operators functions!");
                        return null;
                    }
                    

                    // classifica o operador.
                    string tipo = "";
                    if (parametros.Length==2)
                    {
                        tipo = "BINARIO";
                    }
                    else
                    if (parametros.Length == 1)
                    {
                        tipo = "UNARIO";
                    }


                    // constroi o operador, vindo de um metodo importado da classe base.
                    Operador op = new Operador(nameClassOperators, nameOperatorInOrquidea, prioridade, objParametros.ToArray(), tipo, metodoImpl, escopoImpl);
                    op.tipoReturn = metodoImpl.ReturnType.Name;
                    return op;
                }

            }
            return null;
        }


        /// <summary>
        /// metodo principal: obtem os metodos implementadores, e instancia os operadores orquidea.
        /// </summary>
        public void GetOperators()
        {
            OperadoresImplementacao implementacaoInt = new OperadoresImplementacao(@"PrrojectOrquidea2023.dll", "parser.OperadoresInt");
            operadoresInt.Add(implementacaoInt.GetOperatorComMetodoImportado("Resto", "%", 5));
            operadoresInt.Add(implementacaoInt.GetOperatorComMetodoImportado("NumeroNegativo", "-", 6));
            operadoresInt.Add(implementacaoInt.GetOperatorComMetodoImportado("NumeroPositivo", "+", 6));
            operadoresInt.Add(implementacaoInt.GetOperatorComMetodoImportado("Soma", "+", 2));
            operadoresInt.Add(implementacaoInt.GetOperatorComMetodoImportado("Sub", "-", 2));
            operadoresInt.Add(implementacaoInt.GetOperatorComMetodoImportado("Mult", "*", 3));
            operadoresInt.Add(implementacaoInt.GetOperatorComMetodoImportado("Div", "/", 3));
            operadoresInt.Add(implementacaoInt.GetOperatorComMetodoImportado("Igual", "=", 1));
            operadoresInt.Add(implementacaoInt.GetOperatorComMetodoImportado("ComparacaoIgual", "==", 1));
            operadoresInt.Add(implementacaoInt.GetOperatorComMetodoImportado("Desigual", "!=", 1));
            operadoresInt.Add(implementacaoInt.GetOperatorComMetodoImportado("Maior", ">", 1));
            operadoresInt.Add(implementacaoInt.GetOperatorComMetodoImportado("MaiorOuIgual", ">=", 1));
            operadoresInt.Add(implementacaoInt.GetOperatorComMetodoImportado("Menor", "<", 1));
            operadoresInt.Add(implementacaoInt.GetOperatorComMetodoImportado("MenorOuIgual", "<=", 1));
            operadoresInt.Add(implementacaoInt.GetOperatorComMetodoImportado("IncrementoUnario", "++", 6));
            operadoresInt.Add(implementacaoInt.GetOperatorComMetodoImportado("DecrementoUnario", "--", 6));
            operadoresInt.Add(implementacaoInt.GetOperatorComMetodoImportado("Potenciacao", "^", 6));

            OperadoresImplementacao implementacaoDouble= new OperadoresImplementacao(@"PrrojectOrquidea2023.dll", "parser.OperadoresDouble");

            operadoresDouble.Add(implementacaoDouble.GetOperatorComMetodoImportado("NumeroNegativo", "-", 6));
            operadoresDouble.Add(implementacaoDouble.GetOperatorComMetodoImportado("NumeroPositivo", "+", 6));
            operadoresDouble.Add(implementacaoDouble.GetOperatorComMetodoImportado("Soma", "+", 2));
            operadoresDouble.Add(implementacaoDouble.GetOperatorComMetodoImportado("Sub", "-", 2));
            operadoresDouble.Add(implementacaoDouble.GetOperatorComMetodoImportado("Mult", "*", 3));
            operadoresDouble.Add(implementacaoDouble.GetOperatorComMetodoImportado("Div", "/", 3));
            operadoresDouble.Add(implementacaoDouble.GetOperatorComMetodoImportado("Igual", "=", 1));
            operadoresDouble.Add(implementacaoDouble.GetOperatorComMetodoImportado("ComparacaoIgual", "==", 1));
            operadoresDouble.Add(implementacaoDouble.GetOperatorComMetodoImportado("Desigual", "!=", 1));
            operadoresDouble.Add(implementacaoDouble.GetOperatorComMetodoImportado("Maior", ">", 1));
            operadoresDouble.Add(implementacaoDouble.GetOperatorComMetodoImportado("MaiorOuIgual", ">=", 1));
            operadoresDouble.Add(implementacaoDouble.GetOperatorComMetodoImportado("Menor", "<", 1));
            operadoresDouble.Add(implementacaoDouble.GetOperatorComMetodoImportado("MenorOuIgual", "<=", 1));
            operadoresDouble.Add(implementacaoDouble.GetOperatorComMetodoImportado("IncrementoUnario", "++", 6));
            operadoresDouble.Add(implementacaoDouble.GetOperatorComMetodoImportado("DecrementoUnario", "--", 6));
            operadoresDouble.Add(implementacaoDouble.GetOperatorComMetodoImportado("Potenciacao", "^", 6));



            OperadoresImplementacao implementacaoFloat = new OperadoresImplementacao(@"PrrojectOrquidea2023.dll", "parser.OperadoresFloat");
            operadoresFloat.Add(implementacaoFloat.GetOperatorComMetodoImportado("NumeroNegativo", "-", 6));
            operadoresFloat.Add(implementacaoFloat.GetOperatorComMetodoImportado("NumeroPositivo", "+", 6));
            operadoresFloat.Add(implementacaoFloat.GetOperatorComMetodoImportado("Soma", "+", 2));
            operadoresFloat.Add(implementacaoFloat.GetOperatorComMetodoImportado("Sub", "-", 2));
            operadoresFloat.Add(implementacaoFloat.GetOperatorComMetodoImportado("Mult", "*", 3));
            operadoresFloat.Add(implementacaoFloat.GetOperatorComMetodoImportado("Div", "/", 3));
            operadoresFloat.Add(implementacaoFloat.GetOperatorComMetodoImportado("Igual", "=", 1));
            operadoresFloat.Add(implementacaoFloat.GetOperatorComMetodoImportado("ComparacaoIgual", "==", 1));
            operadoresFloat.Add(implementacaoFloat.GetOperatorComMetodoImportado("Desigual", "!=", 1));
            operadoresFloat.Add(implementacaoFloat.GetOperatorComMetodoImportado("Maior", ">", 1));
            operadoresFloat.Add(implementacaoFloat.GetOperatorComMetodoImportado("MaiorOuIgual", ">=", 1));
            operadoresFloat.Add(implementacaoFloat.GetOperatorComMetodoImportado("Menor", "<", 1));
            operadoresFloat.Add(implementacaoFloat.GetOperatorComMetodoImportado("MenorOuIgual", "<=", 1));
            operadoresFloat.Add(implementacaoFloat.GetOperatorComMetodoImportado("IncrementoUnario", "++", 6));
            operadoresFloat.Add(implementacaoFloat.GetOperatorComMetodoImportado("DecrementoUnario", "--", 6));
            operadoresFloat.Add(implementacaoFloat.GetOperatorComMetodoImportado("Potenciacao", "^", 6));



            OperadoresImplementacao implmentacaoString = new OperadoresImplementacao(@"PrrojectOrquidea2023.dll", "parser.OperadoresString");
            operadoresString.Add(implmentacaoString.GetOperatorComMetodoImportado("Soma", "+", 2));
            operadoresString.Add(implmentacaoString.GetOperatorComMetodoImportado("Sub", "-", 2));
            operadoresString.Add(implmentacaoString.GetOperatorComMetodoImportado("Igual", "=", 1));
            operadoresString.Add(implmentacaoString.GetOperatorComMetodoImportado("ComparacaoIgual", "==", 3));
            operadoresString.Add(implmentacaoString.GetOperatorComMetodoImportado("Desigual", "!=", 3));
            operadoresString.Add(implmentacaoString.GetOperatorComMetodoImportado("Maior", ">", 3));
            operadoresString.Add(implmentacaoString.GetOperatorComMetodoImportado("MaiorOuIgual", ">=", 3));
            operadoresString.Add(implmentacaoString.GetOperatorComMetodoImportado("Menor", "<", 3));
            operadoresString.Add(implmentacaoString.GetOperatorComMetodoImportado("MenorOuIgual", "<=", 3));

            OperadoresImplementacao implmentacaoChar = new OperadoresImplementacao(@"PrrojectOrquidea2023.dll", "parser.OperadoresString");
            operadoresChar.Add(implmentacaoChar.GetOperatorComMetodoImportado("Igual", "=", 1));
            operadoresChar.Add(implmentacaoChar.GetOperatorComMetodoImportado("ComparacaoIgual", "==", 2));
            operadoresChar.Add(implmentacaoChar.GetOperatorComMetodoImportado("Desigual", "!=", 2));
            operadoresChar.Add(implmentacaoChar.GetOperatorComMetodoImportado("Maior", ">", 2));
            operadoresChar.Add(implmentacaoChar.GetOperatorComMetodoImportado("MaiorOuIgual", ">=", 2));
            operadoresChar.Add(implmentacaoChar.GetOperatorComMetodoImportado("Menor", ">", 2));
            operadoresChar.Add(implmentacaoChar.GetOperatorComMetodoImportado("MenorOuIgual", ">=", 2));

            
            OperadoresImplementacao implmentacaoBoolean = new OperadoresImplementacao(@"PrrojectOrquidea2023.dll", "parser.OperadoresBoolean");
            operadoresBoolean.Add(implmentacaoBoolean.GetOperatorComMetodoImportado("Igual", "=", 1));
            operadoresBoolean.Add(implmentacaoBoolean.GetOperatorComMetodoImportado("ComparacaoIgual", "==", 1));
            operadoresBoolean.Add(implmentacaoBoolean.GetOperatorComMetodoImportado("Desigual", "!=", 1));
            operadoresBoolean.Add(implmentacaoBoolean.GetOperatorComMetodoImportado("Not", "!", 1));


            OperadoresImplementacao implmentacaoMatriz = new OperadoresImplementacao(@"PrrojectOrquidea2023.dll", "parser.OperadoresMatriz");
            operadoresMatriz.Add(implmentacaoMatriz.GetOperatorComMetodoImportado("Soma", "+", 2));
            operadoresMatriz.Add(implmentacaoMatriz.GetOperatorComMetodoImportado("Sub", "-", 2));
            operadoresMatriz.Add(implmentacaoMatriz.GetOperatorComMetodoImportado("Mult", "*", 3));
            operadoresMatriz.Add(implmentacaoMatriz.GetOperatorComMetodoImportado("Div", "/", 3));
            operadoresMatriz.Add(implmentacaoMatriz.GetOperatorComMetodoImportado("Igual", "=", 1));
            operadoresMatriz.Add(implmentacaoMatriz.GetOperatorComMetodoImportado("Maior", ">", 1));
            operadoresMatriz.Add(implmentacaoMatriz.GetOperatorComMetodoImportado("MaiorOuIgual", ">=", 1));
            operadoresMatriz.Add(implmentacaoMatriz.GetOperatorComMetodoImportado("Menor", "<", 1));
            operadoresMatriz.Add(implmentacaoMatriz.GetOperatorComMetodoImportado("Menor", "<=", 1));
            operadoresMatriz.Add(implmentacaoMatriz.GetOperatorComMetodoImportado("ComparacaoIgual", "==", 1));

        }

        public class TestesOperadoresBase : SuiteClasseTestes
        {
         

            public TestesOperadoresBase() : base("testes de definicao de operadores base")
            {
            }
            public void TesteObterOperadores(AssercaoSuiteClasse assercao)
            {
                OperadoresImplementacao implementacao = new OperadoresImplementacao(@"PrrojectOrquidea2023.dll", "parser.OperadoresInt");
                Operador op_0 = implementacao.GetOperatorComMetodoImportado("Soma", "+", 1);
                Operador op_1 = implementacao.GetOperatorComMetodoImportado("NumeroNegativo", "-", 3);


                try
                {
                    assercao.IsTrue(op_0.prioridade == 1,"Soma");
                    assercao.IsTrue(op_1.prioridade == 3,"NumeroNegativo");
                }
                catch(Exception ex)
                {
                    string codeError= ex.Message;
                    assercao.IsTrue(false, "falha nos testes unitarios");
                }
            }
        }


        /// <summary>
        /// obtem a implementação do operador, de função importada da classe base.
        /// </summary>
        /// <param name="classeOperador">classe-tipo do operador.</param>
        /// <returns></returns>
        public List<Metodo> GetImplementacao(string classeOperador)
        {
            // obtem dados de metodos da classe herdada, para retirar os metodos da classe herdada, da lista de metodos implementadores de operadores nativos.
            List<MethodInfo> metodosClasseBase = Type.GetType(classeOperador).BaseType.GetMethods().ToList<MethodInfo>();

            // obtem dados de metodos de uma classe.
            MethodInfo[] metodosmplementadores = Type.GetType(classeOperador).GetMethods();
            this.metodosImpl = metodosmplementadores.ToList<MethodInfo>();
            
            
            int prioridadeOperador = 0;
            foreach (MethodInfo umMetodoImpl in metodosmplementadores)
            {
                if (metodosClasseBase.Find(k => k.Name.Equals(umMetodoImpl.Name)) != null)
                    continue;
                string nomeOperador = "";
                string tipoOperador = null;
                if (umMetodoImpl.GetBaseDefinition().Name.Contains("_Operador"))
                    continue;

                GetNomeOperador(umMetodoImpl, ref nomeOperador, ref tipoOperador);
                List<Objeto> parametrosDoOperador = GetTipoParametros(umMetodoImpl);

                if (tipoOperador != null)
                {
                    prioridadeOperador = GetPrioridadeOperador(nomeOperador);
                    Metodo fncImplementador1 = new Metodo(umMetodoImpl.DeclaringType.Name, "public", nomeOperador, umMetodoImpl, UtilTokens.Casting(umMetodoImpl.ReturnType.Name), parametrosDoOperador.ToArray());


                    fncImplementador1.tipoReturn = umMetodoImpl.ReturnType.Name;
                    fncImplementador1.prioridade = prioridadeOperador;
                    fncImplementador1.tipo = tipoOperador;

                    this.funcaoImplOperadores.Add(fncImplementador1);
                }
                
            }
            return this.funcaoImplOperadores;
        }

      

        /// <summary>
        /// obtem o tipo de operandos, de um operador importado.
        /// </summary>
        /// <param name="info">metodo reflexivo que implementa o operador.</param>
        /// <returns>retorna uma lista de operandos da execucao do operador.</returns>
        private List<Objeto> GetTipoParametros(MethodInfo info)
        {
            List<Objeto> objetosParametros = new List<Objeto>();
            for (int x = 0; x < info.GetParameters().Length; x++)
            {
                ParameterInfo parametroDados = info.GetParameters()[x];
                Type umTipoParametro = parametroDados.ParameterType;
                Objeto obj_param = new Objeto("private", UtilTokens.Casting(umTipoParametro.Name), "a", null);
                objetosParametros.Add(obj_param);
            }
            return objetosParametros;

        }
        /// <summary>
        /// permite a extensão de operadores, com importação da função que executa o operador, atraves de reflexão.
        /// </summary>
        /// <param name="nomeOperador">nome do operador.</param>
        /// <param name="umMetodoImpl">metodo info reflexao que contem os dados da funcao que executa o operador.</param>
        /// <param name="prioridade">prioridade do operador, para calculos em expressoes.</param>
        /// <param name="classeDoOperador">classe do operador.</param>
        public void AdicionaOperadorNativo(string nomeOperador, MethodInfo umMetodoImpl, int prioridade, string classeDoOperador)
        {
            string tipoOperador = null;
            Metodo fncImplementador = null;
            GetNomeOperador(umMetodoImpl, ref nomeOperador, ref tipoOperador);

            
            List<Objeto> parametrosObjetos = null;
            if ((umMetodoImpl.GetParameters() != null) && (umMetodoImpl.GetParameters().Length > 0))
                parametrosObjetos = GetTipoParametros(umMetodoImpl);
   
            
            if (parametrosObjetos == null)
                fncImplementador = new Metodo(classeDoOperador, "public", nomeOperador, umMetodoImpl, UtilTokens.Casting(umMetodoImpl.ReturnType.Name));
            else
                fncImplementador = new Metodo(classeDoOperador, "public", nomeOperador, umMetodoImpl, UtilTokens.Casting(umMetodoImpl.ReturnType.Name), parametrosObjetos.ToArray());

            fncImplementador.prioridade = prioridade;

            this.funcaoImplOperadores.Add(fncImplementador);
        }

        /// <summary>
        /// obtem a prioridade, quando confronto com expressoes com varios operadores.
        /// </summary>
        /// <param name="nomeOperador">nome do operador</param>
        /// <returns>retorna a prioridade, na forma de numero.</returns>
        private int GetPrioridadeOperador(string nomeOperador)
        {
            int prioridadeOperador = 7;

           
            if (operadoresPrioridade1.Find(k => k.Equals(nomeOperador)) != null)
                prioridadeOperador = 2;
            if (operadoresPrioridade2.Find(k => k.Equals(nomeOperador)) != null)
                prioridadeOperador = 3;
            if (operadoresPrioridade3.Find(k => k.Equals(nomeOperador)) != null)
                prioridadeOperador = 4;
            if (operadoresPrioridade4.Find(k => k.Equals(nomeOperador)) != null)
                prioridadeOperador = 5;
            
            if (operadoresPrioridade5.Find(k => k.Equals(nomeOperador)) != null)
                prioridadeOperador = 0;

            if (operadoresPrioridade6.Find(k => k.Equals(nomeOperador)) != null)
                prioridadeOperador = 6;

            return prioridadeOperador;
        }

        /// <summary>
        /// obtem o nome do operador implementado via importacao na linguagem base.
        /// </summary>
        /// <param name="umMetodoImpl">metodo reflexao de implementacao do operador.</param>
        /// <param name="nomeOperador">nome do operador a retornar.</param>
        /// <param name="tipoOperador">tipo do operador: BINARIO, UNARIO.</param>
        private static void GetNomeOperador(MethodInfo umMetodoImpl, ref string nomeOperador, ref string tipoOperador)
        {
            tipoOperador = "";
            switch (umMetodoImpl.Name)
            {
                case "Resto":
                    tipoOperador= "BINARIO";
                    nomeOperador = "%";
                    break;
                case "Igual":
                    tipoOperador = "BINARIO";
                    nomeOperador = "=";
                    break;
                case "NumeroNegativo":
                    tipoOperador = "UNARIO";
                    nomeOperador = "-";
                    break;
                case "NumeroPositivo":
                    tipoOperador = "UNARIO";
                    nomeOperador = "+";
                    break;
                case "Soma":
                    tipoOperador = "BINARIO";
                    nomeOperador = "+";
                    break;
                case "Sub":
                    tipoOperador = "BINARIO";
                    nomeOperador = "-";
                    break;
                case "Mult":
                    tipoOperador = "BINARIO";
                    nomeOperador = "*";
                    break;
                case "Div":
                    tipoOperador = "BINARIO";
                    nomeOperador = "/";
                    break;
                case "ComparacaoIgual":
                    tipoOperador = "BINARIO";
                    nomeOperador = "==";
                    break;
                case "Desigual":
                    tipoOperador = "BINARIO";
                    nomeOperador = "!=";
                    break;
                case "Maior":
                    tipoOperador = "BINARIO";
                    nomeOperador = ">";
                    break;
                case "MaiorOuIgual":
                    tipoOperador = "BINARIO";
                    nomeOperador = ">=";
                    break;
                case "Menor":
                    tipoOperador = "BINARIO";
                    nomeOperador = "<";
                    break;
                case "MenorOuIgual":
                    tipoOperador = "BINARIO";
                    nomeOperador = "<=";
                    break;
                case "IncrementoUnario":
                    tipoOperador = "UNARIO";
                    nomeOperador = "++";
                    break;
                case "DecrementoUnario":
                    tipoOperador = "UNARIO";
                    nomeOperador = "--";
                    break;
                case "Atribuicao":
                    tipoOperador = "BINARIO";
                    nomeOperador = "=";
                    break;
                case "Potenciacao":
                    tipoOperador = "BINARIO";
                    nomeOperador = "^";
                    break;
                default:
                    tipoOperador = "BINARIO";
                    nomeOperador = umMetodoImpl.Name;
                    break;
            }

        }

    }

  
    public class OperadoresInt: OperadoresImplementacao
    {
        public delegate int operadorBinario(int x, int y);
        public delegate int operadorUnario(int x);

        /// NumeroNegativo
        // NumeroPositivo
        //  Soma
        // Sub
        //  Mult
        // Div
        //Igual
        // ComparacaoIgual
        // Desigual
        // Maior
        //MaiorOuIgual
        // Menor
        // MenorOuIgual
        // IncrementoUnario
        // DecrementoUnario
        // Potenciacao
        // Rest

        public int Resto(int x, int y)
        {
            return x % y;
        }
        public int NumeroNegativo(int x)
        {
            return -x;
        }

        public int NumeroPositivo(int x)
        {
            return +x;
        }

        public int Soma(int x, int y)
        {
            return x + y;
        }
        public int Sub(int x, int y)
        {
            return x - y;
        }

        public int Mult(int x, int y)
        {
            return x * y;
        }

        public int Div(int x, int y)
        {
            if (y == 0)
                throw new Exception("divisao por zero!");
            return x / y;
        }
     
        public int Igual(int x, int y)
        {
            return y;
        }

        public bool ComparacaoIgual(int x, int y)
        {
            return x == y;
        }

        public bool Desigual(int x, int y)
        {
            return x != y;
        }

        public bool Maior(int x, int y)
        {
            return x > y;
        }

        public bool MaiorOuIgual(int x, int y)
        {
            return x >= y;
        }

        public bool Menor(int x, int y)
        {
            return x < y;
        }

        public bool MenorOuIgual(int x, int y)
        {
            return x <= y;
        }

     

        public int IncrementoUnario(int x)
        {
            return ++x;
        }

        public int DecrementoUnario(int x)
        {
            return --x;
        }

        public int Potenciacao(int x, int y)
        {
            return (int)Math.Pow(x, y);
        }
    }



    public class OperadoresDouble : OperadoresImplementacao
    {
        public delegate double operadorBinario(double x, double y);
        public delegate double operadorUnario(double x);

        public double NumeroNegativo(double x)
        {
            return -1;
        }

        public double NumeroPositivo(double x)
        {
            return +1;
        }

        public double Soma(double x, double y)
        {
            return x + y;
        }
        public double Sub(double x, double y)
        {
            return x - y;
        }

        public double Mult(double x, double y)
        {
            return x * y;
        }

        public double Div(double x, double y)
        {
            if (y == 0)
                throw new Exception("divisao por zero!");
            return x / y;
        }

        public double Igual(double x, double y)
        {
            return y;
        }

        public bool ComparacaoIgual(double x, double y)
        {
            return x == y;
        }

        public bool Desigual(double x, double y)
        {
            return x != y;
        }

        public bool Maior(double x, double y)
        {
            return x > y;
        }

        public bool MaiorOuIgual(double x, double y)
        {
            return x >= y;
        }

        public bool Menor(double x, double y)
        {
            return x < y;
        }

        public bool MenorOuIgual(double x, double y)
        {
            return x <= y;
        }

       

        public double IncrementoUnario(double x)
        {
            return ++x;
        }

        public double DecrementoUnario(double x)
        {
            return --x;
        }

        public double Potenciacao(double x, double y)
        {
            return (double)Math.Pow(x, y);
        }
    }


    public class OperadoresFloat : OperadoresImplementacao
    {
        public delegate float operadorBinario(float x, float y);
        public delegate float operadorUnario(float x);

        public float  NumeroNegativo(float x)
        {
            return -x;
        }

        public float NumeroPositivo(float x)
        {
            return +x;
        }

        public float Soma(float x, float y)
        {
            return x + y;
        }
        public float Sub(float x, float y)
        {
            return x - y;
        }

        public float Mult(float x, float y)
        {
            return x * y;
        }

        public float Div(float x, float y)
        {
            if (y == 0)
                throw new Exception("divisao por zero!");
            return x / y;
        }

        public float Igual(float x, float y)
        {
            return y;
        }

        public bool ComparacaoIgual(float x, float y)
        {
            return x == y;
        }

        public bool Desigual(float x, float y)
        {
            return x != y;
        }

        public bool Maior(float x, float y)
        {
            return x > y;
        }

        public bool MaiorOuIgual(float x, float y)
        {
            return x >= y;
        }

        public bool Menor(float x, float y)
        {
            return x < y;
        }

        public bool MenorOuIgual(float x, float y)
        {
            return x <= y;
        }

   
        public float IncrementoUnario(float x)
        {
            return ++x;
        }

        public float DecrementoUnario(float x)
        {
            return --x;
        }

        public float Potenciacao(float x, float y)
        {
            return (float)Math.Pow(x, y);
        }
    }


    public class OperadoresString : OperadoresImplementacao
    {
        public delegate string operadorBinario(string x, string y);
        public delegate string operadorUnario(string x);

        
        public string Soma(string x, string y)
        {
            return x + y;
        }
        public string Sub(string x, string y)
        {
            return x.Replace(y, "");
        }

        public string Igual(string x, string y)
        {
            return (string)y.Clone();
        }
        public bool ComparacaoIgual(string x, string y)
        {
            return x == y;
        }

        public bool Desigual(string x, string y)
        {
            return x != y;
        }

        public bool Maior(string x, string y)
        {
            return x.Length > y.Length;
        }

        public bool MaiorOuIgual(string x, string y)
        {
            return x.Length >= y.Length;
        }

        public bool Menor(string x, string y)
        {
            return x.Length < y.Length;
        }

        public bool MenorOuIgual(string x, string y)
        {
            return x.Length <= y.Length;
        }

  
    }

    public class OperadoresChar : OperadoresImplementacao
    {
        public delegate Char operadorBinario(Char x, Char y);
        public delegate Char operadorUnario(Char x);


       
        
        public Char Igual(Char x, Char y)
        {
            return y;
        }

        public bool ComparacaoIgual(Char x, Char y)
        {
            return x == y;
        }

        public bool Desigual(Char x, Char y)
        {
            return x != y;
        }

        public bool Maior(Char x, Char y)
        {
            return x > y;
        }

        public bool MaiorOuIgual(Char x, Char y)
        {
            return x>= y;
        }

        public bool Menor(Char x, Char y)
        {
            return x < y;
        }

        public bool MenorOuIgual(Char x, Char y)
        {
            return x <= y;
        }

     
    }


    public class OperadoresBoolean : OperadoresImplementacao
    {
        public delegate Boolean operadorBinario(Boolean x, Boolean y);
        public delegate Boolean operadorUnario(Boolean x);

        
        public Boolean Igual(Boolean x, Boolean y)
        {
            return y;
        }

        public bool ComparacaoIgual(Boolean x, Boolean y)
        {
            return x == y;
        }

        public bool Desigual(Boolean x, Boolean y)
        {
            return x != y;
        }

        public bool Not(Boolean x)
        {
            return !x;
        }

    }
    public class OperadoresMatriz:OperadoresImplementacao
    {


        private static Matriz aux1;
        private static Matriz aux2;

        public Matriz mtMain;

        public OperadoresMatriz() { }


        public OperadoresMatriz(int linhas, int colunas)
        {
            if ((aux1 == null) || (aux2 == null))
            {
                aux1 = new Matriz(1, 5);
                aux2 = new Matriz(5, 1);

                aux1.PreencheMatriz(1.0);
                aux2.PreencheMatriz(1.5);
            } // if

            this.nome = "Matriz";
            this.mtMain = new Matriz(linhas, colunas);
        }

        public static Matriz GetMatriz(Matriz M)
        {
            Matriz mt = new Matriz(M.qtLin, M.qtCol);
            for (int lin = 0; lin < M.qtLin; lin++)
                for (int col = 0; col < M.qtCol; col++)
                    mt.SetElement(lin, col, M.GetElement(lin, col));
            return mt;
        }
        public float GetElement(int lin, int col)
        {
            return (float)this.mtMain.GetElement(lin, col);
        }

        public void SetElement(int lin, int col, object valor)
        {
            this.mtMain.SetElement(lin, col, (float)valor);
        }
        public object Soma(Matriz m1, Matriz m2)
        {
            Matriz mResult = m1+ m2;
            return mResult;
        } 

       

        public Matriz Sub(Matriz m1, Matriz m2)
        {
            Matriz mResult = m1 - m2;
            return mResult;
        } 

        public Matriz Mult(Matriz m1, Matriz m2)
        {
            Matriz mResult = m1 * m2;
            return mResult;
        } 

        public Matriz Div(Matriz m1, Matriz m2)
        {
            Matriz mResult = Mult(m1, Matriz.MatrizInversaNaoQuadratica(m2));
            return mResult;
        } 

        public Matriz Igual(Matriz m1, Matriz m2)
        {
            m1 = m2.Clone();
            return m1;
        }
        
        public object Maior(Matriz m1, Matriz m2)
        {
            double gp1 = 0.0;
            double gp2 = 0.0;
            if (!CalcGrauPrecisao(m1, m2, ref gp1, ref gp2))
            {
                return null;
            }
            else
            {
                return (gp1 > gp2);
            }
            
        }

        public object MaiorOuIgual(Matriz m1, Matriz m2)
        {
            
            double gp1 = 0.0;
            double gp2 = 0.0;
            if (!CalcGrauPrecisao(m1, m2, ref gp1, ref gp2))
            {
                return null;
            }
            else
            {
                return (gp1 >= gp2);
            }
            
        }

        public object Menor(Matriz m1, Matriz m2)
        {
            double gp1 = 0.0;
            double gp2 = 0.0;
            if (!CalcGrauPrecisao(m1, m2, ref gp1, ref gp2))
            {
                return null;
            }
            else
            {
                return (gp1 < gp2);
            }
            
        }

        public object MenorOuIgual(Matriz m1, Matriz m2)
        {
            double gp1 = 0.0;
            double gp2 = 0.0;
            if (!CalcGrauPrecisao(m1, m2, ref gp1, ref gp2))
            {
                return null;
            }
            else
            {
                return (gp1 <= gp2);
            }
            
        }

        public object ComparacaoIgual(Matriz m1, Matriz m2)
        {
            double gp1 = 0.0;
            double gp2 = 0.0;
            if (!CalcGrauPrecisao(m1, m2, ref gp1, ref gp2))
            {
                return null;
            }
            else
            {
                return (gp1 == gp2);
            }
            
        }




        //__________________________________________________________________________________________________________________________________________________________________
        // CÁLCULO DO GRAU DE PRECISÃO, UM REDUTOR DE DIMENSÕES DE MATRIZ, UTILIZADA PARA COMPARAR MATRIZES.
        /// <summary>
        /// calcula o redutor de matrizes, grau de precisão.
        /// 1- se as colunas das matrizes auxiliares tiverem colunas diferentes da matriz m1, recalcula as matrizes auxiliares, com o mesmo numero de colunas da matriz m1.
        /// 2- se as matrizes m1 e m2 tiverem dimensões diferentes, retorna false, pois não há como comparar graus de precisão com matrizes de dimensões diferentes entre si.
        private static bool CalcGrauPrecisao(Matriz m1, Matriz m2, ref double grauPrecisao1, ref double grauPrecisao2)
        {

            if ((m1.qtCol != m2.qtCol) || (m1.qtCol != m2.qtCol))
                return false;
            if (m1.qtCol != aux1.qtCol)
            {
                aux1 = new Matriz(1, m1.qtCol);
                aux2 = new Matriz(m1.qtCol, 1);
                aux1.PreencheMatriz(1.5);
                aux2.PreencheMatriz(1.5);
            }

            grauPrecisao1 = GetMatriz(aux1* m1 * aux2).GetElement(0, 0);
            grauPrecisao2 = GetMatriz(aux1 * m2 * aux2).GetElement(0, 0);

            return true;
        }


        private void LoadParameters(object[] operandos, out Matriz m1, out Matriz m2, out Matriz mResult)
        {
            m1 = (Matriz)operandos[0];
            m2 = (Matriz)operandos[1];
            mResult = new Matriz(m1.qtLin, m1.qtCol);
        }


    }
 
    
} // namespace

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using ModuloTESTES;
using System.Security.Cryptography;


namespace parser
{

    public class SuiteTestesII
    {
        /// <summary>
        /// guarda o resultado de um teste.
        /// </summary>
        public static object result;
        /// <summary>
        /// prototipo de funcao que verifica se o esperado é ou não como o resultado.
        /// </summary>
        /// <returns></returns>
        public delegate bool Assertion(params object[] args);

        
        /// <summary>
        /// verifica se as condicoes de validacao do teste resultou em teste passou/nao passou.
        /// </summary>
        /// <param name="conditions">lista de condicoes.</param>
        /// <returns></returns>
        public static bool Validation(params bool[] conditions)
        {
            try
            {
                for (int i = 0; i < conditions.Length; i++)
                {
                    if (!conditions[i])
                    {
                        LoggerTests.AddMessage("teste nao passou");
                        return false;
                    }
                }

               return true;
            }
            catch
            {
                LoggerTests.AddMessage("teste terminado com erro fatal. Teste nao passou");
                return false;
            }
        }





        // funcionalidades:
        // 1- objeto estatico guardando o resultado do teste.
        // 3- um delegate para verificação dos resultados.
        // 3- uma funcao estatica que faz o teste (com entrada nome de metodo, ou um construtor (mesmo nome da classe).
        // 4- uma funcao estatica que faz a validacao.


        /// <summary>
        /// metodo que faz a comparacao do esperado com o resultado, e grava no arquivo logg o resultado.
        /// </summary>
        /// <param name="funcaoAssercao">funcao que faz a comparacao.</param>
        /// <param name="caption">texto info a mais para colocar no arquivo logg.</param>
        /// <returns>return true se o teste passou, false se o teste nao passou, ou falhou na comparacao lançando uma exceção.</returns>
        public static bool AssertionIsTrue(Assertion funcaoAssercao)
        {
            try
            {
                if (funcaoAssercao())
                {
                    LoggerTests.AddMessage("[TEST PASS]:  ");
                    return true;
                }
                else
                {
                    LoggerTests.AddMessage("[TESTE FAIL]:  ");
                    return false;
                }
            }
            catch(Exception e)
            {
                LoggerTests.AddMessage("[TEST FAIL], FATAL ERROR: " + e.Message);
                return false;
            }
        }



        /// <summary>
        /// funcao de teste, mas sem objeto caller. Precisa de um construtor vazio, e que não faça nada.
        /// </summary>
        /// <param name="classeEmTeste">classe do metodo e do objeto caller.</param>
        /// <param name="metodoEmTeste">nome do metodo do teste.</param>
        /// <param name="parametrosDoMetodoTeste">parametros do metodo do teste.</param>
        /// 
        public static void ConstructorTest(string classeEmTeste, string metodoEmTeste, Assertion functionValidation, params object[] parametrosDoMetodoTeste)
        {

            Type tipo = Type.GetType(classeEmTeste);
            ConstructorInfo[] construtors = tipo.GetConstructors();
            if ((construtors == null) || (construtors.Length == 0))
            {
                LoggerTests.AddMessage("not found any constructors compatible. Test fail.");
                return;
            }

            foreach(ConstructorInfo constructor in construtors)
            {
                
                if (IsConstructorCompatible(constructor,parametrosDoMetodoTeste))
                {
                    result=constructor.Invoke(parametrosDoMetodoTeste);
                    AssertionIsTrue(functionValidation);
                    return;
    
                }
            }

        }

 
        /// <summary>
        /// executa uma um metodo teste, e retorna o resultado em [SuiteTestesII.result].
        /// </summary>
        /// <param name="caller">objeto que chamou o metodo teste.</param>
        /// <param name="nomeMetodoTeste">nome do metodo a testar.</param>
        /// <param name="parametrosDoMetodoTeste">lista de parametros variavel do metodo teste.</param>
        /// 
        public static void MethodTest(object caller, string nomeMetodoTeste, Assertion functionValidation, params object[] parametrosDoMetodoTeste)
        {
            // obtem a classe teste do objeto;
            string nomeClasseTesteFULL = caller.GetType().FullName;
            string nomeClasseTesteSHORT = caller.GetType().Name;

            if (nomeMetodoTeste == nomeClasseTesteSHORT)
            {
                LoggerTests.AddMessage("test fail because the method is a constructor. execute other method from test class");
                throw new Exception("method is a constructor, execute test with ohter method from test class");
            }


            bool isFoundMethoCompatible = false;
            MethodInfo[] metodos = caller.GetType().GetMethods();
            foreach (MethodInfo mi in metodos)
            {


                // encontra o metodo compativel ao nome, e parametros da chamada do tess
                if ((mi.Name == nomeMetodoTeste) && (IsMethodCompatible(mi, parametrosDoMetodoTeste)))
                {
                    // fazer a execucao dos metodo do cenario de teste.
                    SuiteTestesII.result = mi.Invoke(caller, parametrosDoMetodoTeste);
                    AssertionIsTrue(functionValidation);
                    isFoundMethoCompatible = true;
                    return;
                }

            }
            if (!isFoundMethoCompatible)
            {
                LoggerTests.AddMessage("not found method compatible with test parameters!.Teste fail.");
                return;
            }
        }


        /// <summary>
        /// metodo teste sem objeto caller, instanciado dentro do metodo.
        /// </summary>
        /// <param name="nomeClasseTeste">nome COMPLETO da classe do metodo do teste.</param>
        /// <param name="nomeMetodoTeste">nome do metodo do teste.</param>
        /// <param name="functionValidation">funcao de validacao.</param>
        /// <param name="parametrosDoMetodoTeste">parametros do metodo do teste.</param>
        /// <exception cref="Exception"></exception>
        public static void MethodTest(string nomeClasseTeste, string nomeMetodoTeste, Assertion functionValidation, params object[] parametrosDoMetodoTeste)
        {
            string nomeClasseSHORT= Type.GetType(nomeClasseTeste).Name;

            // obtem a classe teste do objeto;
            if (nomeMetodoTeste == nomeClasseSHORT)
            {
                LoggerTests.AddMessage("test fail because the method is a constructor. execute other method from test class");
                throw new Exception("method is a constructor, execute test with ohter method from test class");
            }


            Type tipo = Type.GetType(nomeClasseTeste);
            object caller = null;
            ConstructorInfo[] construtors = tipo.GetConstructors();
            if ((construtors == null) || (construtors.Length == 0))
            {
                LoggerTests.AddMessage("not found any constructors compatible. Test fail.");
                return;
            }


            foreach (ConstructorInfo constructor in construtors)
            {

                // obtem o construtor sem parametros.
                if (IsConstructorCompatible(constructor, new object[0]))
                {
                    caller = constructor.Invoke(parametrosDoMetodoTeste);
                    break;
                }
            }
            if (caller == null)
            {
                LoggerTests.AddMessage("constructor with empty parameters not found, test fail");
                return;
            }

            bool isFoundMethoCompatible = false;
            MethodInfo[] metodos = caller.GetType().GetMethods();
            foreach (MethodInfo mi in metodos)
            {


                // encontra o metodo compativel ao nome, e parametros da chamada do tess
                if ((mi.Name == nomeMetodoTeste) && (IsMethodCompatible(mi, parametrosDoMetodoTeste)))
                {
                    // fazer a execucao dos metodo do cenario de teste.
                    SuiteTestesII.result = mi.Invoke(caller, parametrosDoMetodoTeste);
                    AssertionIsTrue(functionValidation);
                    isFoundMethoCompatible = true;
                    return;
                }

            }
            if (!isFoundMethoCompatible)
            {
                LoggerTests.AddMessage("not found method compatible with test parameters!.Teste fail.");
                return;
            }
        }


        /// <summary>
        /// verifica se o metodo tem os parametros que os parametros da chamada do metodo.
        /// </summary>
        /// <param name="method">metodo Reflexao a verificar.</param>
        /// <param name="parameters">lista de parametros da chamada do metodo.</param>
        /// <returns></returns>
        private static bool IsMethodCompatible(MethodInfo method, object[] parameters)
        {
            if (((method.GetParameters()==null) || (method.GetParameters().Length==0)) && ((parameters==null) || (parameters.Length==0)))
            {
                return true;
            }
            for (int x = 0; x < method.GetParameters().Length; x++)
            {
                if (method.GetParameters()[x].GetType() == (parameters[x].GetType())) 
                {
                    continue;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// verifica se um construtor é compativel com os parametros de chamada do teste.
        /// </summary>
        /// <param name="construtor">construtor a verificar.</param>
        /// <param name="parameters">lista de parametros do teste.ç</param>
        /// <returns>retorna true se o construtor é compativel com os parametros.</returns>
        private static bool IsConstructorCompatible(ConstructorInfo construtor, object[] parameters)
        {
            if (((construtor.GetParameters() == null) || (construtor.GetParameters().Length == 0)) && ((parameters == null) || (parameters.Length == 0)))
            {
                return true;
            }
            if (construtor.GetParameters().Length!=parameters.Length)
            {
                return false;
            }

            for (int x = 0; x < construtor.GetParameters().Length; x++)
            {
                if (construtor.GetParameters()[x].ParameterType == (parameters[x].GetType()))
                {
                    continue;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }


    }
    public class SuiteClasseTestes
    {
        private delegate void MetodoTeste(AssercaoSuiteClasse assercao);

        private List<MethodInfo> metodosTeste { get; set; }
        private MethodInfo metodoAntes { get; set; }
        private MethodInfo metodoDepois { get; set; }

        private string infoTextoCabacalho { get; set; }

        private List<AssercaoSuiteClasse> assercoesCenarioTeste = new List<AssercaoSuiteClasse>();



        public SuiteClasseTestes(string infoTextoNomeClasse)
        {
            this.infoTextoCabacalho = infoTextoNomeClasse;
            this.metodosTeste = new List<MethodInfo>();

            List<MethodInfo> metodos = this.GetType().GetMethods().ToList<MethodInfo>();

            foreach (MethodInfo umMetodo in metodos)
            {
                List<ParameterInfo> parametrosDoMetodo = umMetodo.GetParameters().ToList<ParameterInfo>();

                if ((parametrosDoMetodo != null) && (parametrosDoMetodo.Count > 0) && (parametrosDoMetodo[0].ParameterType == typeof(AssercaoSuiteClasse)))
                    this.metodosTeste.Add(umMetodo);

                if (umMetodo.Name.Equals("Antes"))
                    this.metodoAntes = umMetodo;

                if (umMetodo.Name.Equals("Depois"))
                    this.metodoDepois = umMetodo;

                AssercaoSuiteClasse umaAssercao = new AssercaoSuiteClasse(umMetodo.Name);
                this.assercoesCenarioTeste.Add(umaAssercao);


            }


        }
        public void ExecutaTestes()
        {


            if (metodosTeste == null)
            {
                LoggerTests.AddMessage("Nao ha testes a serem executados nesta classe para testes.");
                return;
            }


            LoggerTests.WriteEmptyLines();
            LoggerTests.AddMessage(this.infoTextoCabacalho);



            TemporizadorParaDesempenho tempoTotalDeTodosTestes = new TemporizadorParaDesempenho();

            tempoTotalDeTodosTestes.Begin();

            int contadorAssercao = 0;
            int x = 0;

            foreach (MethodInfo metodo in metodosTeste)
            {
                try
                {

                    assercoesCenarioTeste[contadorAssercao].temporizadorUmCenarioDeTeste.Begin();








                    // executa o metodo preparador para o teste.
                    if (metodoAntes != null)
                        metodoAntes.Invoke(this, null);


                    // executa o teste. 
                    if ((metodo.Name != "Antes") && (metodo.Name != "Depois"))
                        metodo.Invoke(this, new object[] { assercoesCenarioTeste[contadorAssercao] });

                    // executa o metodo finalizador para o teste.
                    if (metodoDepois != null)
                        metodoDepois.Invoke(this, null);





                    this.assercoesCenarioTeste[contadorAssercao].temporizadorUmCenarioDeTeste.End();


                    contadorAssercao += 1;



                    string nomeMetodoCenarioTeste = assercoesCenarioTeste[x].nameMethodTest;
                  


                    for (int i = 0; i < assercoesCenarioTeste[x].validacoesFeitas.Count; i++)
                    {
                        string umaValidacao = assercoesCenarioTeste[x].validacoesFeitas[i];
                        string messagemValidacao = assercoesCenarioTeste[x].messageInfo[i];


                        if ((messagemValidacao == "") || (messagemValidacao == null)) 
                        {
                            LoggerTests.AddMessage("teste: [" + nomeMetodoCenarioTeste + "]:   " + umaValidacao);
                        }
                        else
                        {
                            LoggerTests.AddMessage("  teste: [" + nomeMetodoCenarioTeste + "]:   " + umaValidacao + "info: " + messagemValidacao );
                        }
                        

                    }

                    x++;
                }
                catch (Exception exc)
                {
                    LoggerTests.AddMessage("teste: " + metodo.Name + ", na classe: " + this.GetType().Name + " gerou excecao que interrompeu o seu processamento." + " falha porque: " + exc.Message + ", Stack: " + exc.StackTrace);
                    LoggerTests.WriteEmptyLines();
                    continue;
                }

            }
            // foreach.

            tempoTotalDeTodosTestes.End();


          
            // dá um espaçamento em linhas no arquivo log.
            LoggerTests.WriteEmptyLines();

            // texto do tempo de processamento de todos cenarios de testes.
            string tempoDeProcessamentoTodosCenariosTeste = "todos cenarios de testes executados em: " + tempoTotalDeTodosTestes.GetTime() + "  mls.   ";
            LoggerTests.AddMessage(tempoDeProcessamentoTodosCenariosTeste);


            LoggerTests.WriteEmptyLines();


        } //  class SuiteClasseTestes

        public class AssercaoSuiteClasse
        {
            public List<string> validacoesFeitas
            {
                get;
                private set;
            }

            public string nameMethodTest
            {
                get;
                set;
            }

            public TemporizadorParaDesempenho temporizadorUmCenarioDeTeste
            {
                get;
                set;
            }

            public List<string> messageInfo
            {
                get;
                set;
            }


            public AssercaoSuiteClasse(string nomeMetodoSobTeste)
            {
                this.temporizadorUmCenarioDeTeste = new TemporizadorParaDesempenho();
                this.nameMethodTest = nomeMetodoSobTeste;
                this.validacoesFeitas = new List<string>();
                this.messageInfo = new List<string>();
            }

            public bool IsTrue(bool condicaoValidacao)
            {
                this.messageInfo.Add("");
                if (condicaoValidacao)
                {
                    validacoesFeitas.Add("[TESTE PASSOU]");
                    return true;
                }
                if (!condicaoValidacao)
                {
                    validacoesFeitas.Add("[TESTE NAO PASSOU]");
                    return false;
                }

                return false;
            }


            public bool IsTrue(bool condicaoValidacao, string messageInfo)
            {
                
                this.messageInfo.Add(messageInfo);

                if (condicaoValidacao)
                {
                    validacoesFeitas.Add("[TESTE PASSOU.]");
                    return true;
                }
                if (!condicaoValidacao)
                {
                    validacoesFeitas.Add("[TESTE NAO PASSOU.]");
                    return false;
                }

                return false;
            }

        }
    }
    public class TemporizadorParaDesempenho
    {
       
        private long temporizadorBegin
        {
            get;
            set;
        }

        private long temporizadorEnd
        {
            get;
            set;
        }

        public TemporizadorParaDesempenho()
        {


        }


        public void Begin()
        {

            temporizadorBegin = TimeActual(DateTime.Now);
        }

        public void End()
        {
            temporizadorEnd = TimeActual(DateTime.Now);
        }

        public long GetTime()
        {
            return temporizadorEnd - temporizadorBegin;
        }

        public long TimeActual(DateTime time)
        {
            return time.Millisecond + time.Second * 1000 + time.Minute * 1000 * 60 + time.Hour * 1000 * 60 * 60;
        }

    }



} // namespace

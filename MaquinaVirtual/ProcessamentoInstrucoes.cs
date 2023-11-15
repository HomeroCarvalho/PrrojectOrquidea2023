using System.Collections.Generic;
using System.Linq;
using parser.ProgramacaoOrentadaAObjetos;
using System.Reflection;
using System;
using System.Security.Cryptography;

namespace parser
{
    public class ProgramaEmVM
    {


        /// <summary>
        /// lista todas instruções do programa, rorando dentro da VM.
        /// </summary>
        private List<Instrucao> instrucoes = new List<Instrucao>();

        /// <summary>
        /// linguagem de computação utilizada.
        /// </summary>
        private static LinguagemOrquidea linguagem = null;



        internal static Dictionary<int, HandlerInstrucao> dicHandlers = null;
        /// <summary>
        /// lista de tags das instruções a ser executadas.
        /// </summary>
        public static List<int> codesInstructions;


        public static int codeWhile = 0;
        public static int codeIfElse = 7;
        public static int codeFor = 3;
        public static int codeAtribution = 4;
        public static int codeCallerFunction = 5;
        public static int codeReturn = 2;
        public static int codeBlock = 6;
        public static int codeBreak = 9;
        public static int codeContinue = 10;
        public static int codeDefinitionFunction = 11; // a definição de função não é uma instrução, mas resultado da compilação.
        public static int codeGetObjeto = 14;
        public static int codeSetObjeto = 16;
        public static int codeOperadorBinario = 17;
        public static int codeOperadorUnario = 18;
        public static int codeCasesOfUse = 19;
        public static int codeCreateObject = 20;
        public static int codeImporter = 22;
        public static int codeCallerMethod = 25;
        public static int codeExpressionValid = 26;
        public static int codeConstructorUp = 27;
        public static int codeAspectos = 28;
        public static int codeRiseError = 29;

        public delegate object HandlerInstrucao(Instrucao umaInstrucao, Escopo escopo);

        private int IP_contador = 0; // guarda o id das sequencias.
        private bool isQuit = false;

        public object lastReturn;

        // inicia o programa na VM.
        public void Run(Escopo escopo)
        {

            IP_contador = 0; // inicia a primeira instrução do software.

            while (IP_contador < instrucoes.Count)
            {
                ExecutaUmaInstrucao(instrucoes[IP_contador], escopo);
                IP_contador++;

                if (isQuit)
                    break;
            } // while
        } // Run()


        /// <summary>
        /// inicializa uma instancia do programa virtual.
        /// </summary>
        /// <param name="instrucoesPrograma"></param>
        public ProgramaEmVM(List<Instrucao> instrucoesPrograma)
        {
            if (ProgramaEmVM.linguagem == null)
                ProgramaEmVM.linguagem = LinguagemOrquidea.Instance();

            if (ProgramaEmVM.codesInstructions == null)
                ProgramaEmVM.codesInstructions = new List<int>();

            instrucoes = instrucoesPrograma.ToList<Instrucao>();

            if (dicHandlers == null)
            {
                dicHandlers = new Dictionary<int, HandlerInstrucao>();

                // nem todas instruções são acessíveis pelo dicionario: break, e continue ficam dentro das instruções de repetição. codeBlock não precisa ter handler.
                dicHandlers[codeAtribution] = this.InstrucaoAtribuicao;
                dicHandlers[codeCallerFunction] = this.InstrucaoChamadaDeFuncao;
                dicHandlers[codeCallerMethod] = this.InstrucaoChamadaDeMetodo;
                dicHandlers[codeDefinitionFunction] = this.InstrucaoDefinicaoDeFuncao;
                dicHandlers[codeIfElse] = this.InstrucaoIfElse;
                dicHandlers[codeFor] = InstrucaoFor;
                dicHandlers[codeWhile] = InstrucaoWhile;
                dicHandlers[codeGetObjeto] = InstrucaoGetObjeto;
                dicHandlers[codeSetObjeto] = InstrucaoSetObjeto;
                dicHandlers[codeOperadorUnario] = InstrucaoOperadorUnario;
                dicHandlers[codeOperadorBinario] = InstrucaoOperadorBinario;
                dicHandlers[codeCasesOfUse] = InstrucaoCasesOfUse;
                dicHandlers[codeCreateObject] = InstrucaoCreateObject;
                dicHandlers[codeImporter] = InstrucaoImporter;
                dicHandlers[codeReturn] = InstrucaoReturn;
                dicHandlers[codeExpressionValid] = InstrucaoExpressaoValida;
                dicHandlers[codeConstructorUp] = InstrucaoConstrutorUP;
                dicHandlers[codeAspectos] = InstrucaoAspecto;
            }
        } // InstrucoesVM()


        /// executa uma instrução dentro do programa contido na VM.
        private object ExecutaUmaInstrucao(Instrucao umaInstrucao, Escopo escopo)
        {
            this.lastReturn = dicHandlers[umaInstrucao.code](umaInstrucao, escopo);
            return this.lastReturn;
        } // ExecutaUmaInstrucao()





        private object InstrucaoImporter(Instrucao instrucao, Escopo escopo)
        {
            // as classes já foram importadas no compilador..
            return null;

        } // InstrucaoCreateObject()


        private object InstrucaoChamadaDeMetodo(Instrucao instrucao, Escopo escopo)
        {

            object resultChamada = null;
            Expressao ExpressaoObjetoChamadaDeMetodo = instrucao.expressoes[0];
            ExpressaoObjetoChamadaDeMetodo.isModify = true;

            EvalExpression eval = new EvalExpression();
            resultChamada = eval.EvalPosOrdem(ExpressaoObjetoChamadaDeMetodo, escopo);


            // retorna apenas o resultado da útltima chamada de método, ex.: "a.metodoB().metdoA(x)",
            // retorna o resultado de "metodoA(x)", o que segue a lógica de chamadas de métodos aninhadas...
            return resultChamada;

        }
    



        private object InstrucaoExpressaoValida(Instrucao instrucao, Escopo escopo)
        {
            Expressao expressao = instrucao.expressoes[0];
            EvalExpression eval = new EvalExpression();
            
            
            object result = eval.EvalPosOrdem(expressao, escopo);
            instrucao.expressoes[0].isModify = true;


            return result;

        } 

        private object InstrucaoConstrutorUP(Instrucao instrucao, Escopo escopo)
        {
            ///   Cabecalho de listas de expressoes:
            ///   0- nomeDaClasseHerdeira
            ///   1- nomeDaClasseHerdada.
            ///   2- indice do construtor da classe herdada.
            ///   3- expressao cujos elementos são os parametros do construtor.


            /// template: nomeDaClasseHerdeira.construtorUP(nomeClasseHerdada, List<Expressao> parametrosDoConstrutor).
            ///           pode ser o objeto "actual";
            ///           



            string nomeDaClasseHerdeira = instrucao.expressoes[0].ToString();
            string nomeClasseHerdada = instrucao.expressoes[1].ToString();


            Objeto ObjetoAtual = escopo.tabela.GetObjeto("atual", nomeClasseHerdada, escopo); // obtem o objeto referenciado pelo construtor principal.
            if (ObjetoAtual == null)
                return null;



            Classe classeHerdada = RepositorioDeClassesOO.Instance().GetClasse(nomeClasseHerdada);  // obtem a classe do objeto a ser instanciado.

            int indexConstrutorClasseHerdada = int.Parse(instrucao.expressoes[2].ToString());
            Metodo construtor = classeHerdada.construtores[indexConstrutorClasseHerdada]; // obtem o construtor para instanciar o objeto a ser instanciado.



            List<Expressao> parametrosParaOConstrutor = instrucao.expressoes[3].Elementos; //obtm os parâmetros a serem passados para o construtor da classe herdada.



            Escopo escopoConstrutorUP = escopo.Clone();


            // executa o construtor, com o escopo que detém os valores dos objetos herdados.
            construtor.ExecuteAConstructor(ObjetoAtual, parametrosParaOConstrutor, nomeClasseHerdada, escopoConstrutorUP, indexConstrutorClasseHerdada);

            escopo = escopoConstrutorUP.Clone();

            return new object();

        }

        private static void AdicionaObjetosAtuais(Escopo escopoConstrutorUP, List<Classe> classesHerdadas)
        {
            if ((classesHerdadas != null) && (classesHerdadas.Count > 0)) // constroi objetos atual, util para construtores de classes herdadas.
            {


                for (int c = 0; c < classesHerdadas.Count; c++)
                {

                    // constroi objetos "atual", para cada classe herdada. é útil para invocar construtores de classes herdadas, que é preciso ser chamado quando se instancia objetos da classe herdeira.
                    Objeto umObjetoAtual = new Objeto("private", classesHerdadas[c].GetNome(), "atual", null);

                    // registra os objetos atual no escopo de invocação de construtores herdaddos.
                    escopoConstrutorUP.tabela.GetObjetos().Add(umObjetoAtual);
                } // for c



            }
        }

        private static void RemoveObjetosAtuais(Escopo escopo, List<Classe> classesHerdadas)
        {
            if (classesHerdadas != null)
                for (int c = 0; c < classesHerdadas.Count; c++)    // remove o objeto utilizado para chamada de construtores herdados. 
                                                                   // Este objeto é util para chamadas de construtores de classes herdadas.
                {

                    Objeto umDosObjetosAtual = escopo.tabela.GetObjeto("atual", classesHerdadas[c].GetNome(), escopo);
                    if (umDosObjetosAtual != null)
                        escopo.tabela.GetObjetos().Remove(umDosObjetosAtual);
                }
        }

        /// <summary>
        /// cria uma instancia de um objeto, se já não foi criado (em tempo de compilação).
        /// </summary>
        /// <param name="instrucao">dados da instrução.</param>
        /// <param name="escopo">contexto onde as expressoes da instrucao está.</param>
        /// <returns></returns>
        private object InstrucaoCreateObject(Instrucao instrucao, Escopo escopo)
        {


            /*
             *  ESTRUTURA DE DADOS CONTIDA NA LISTA DE EXPRESSOES.
             * 
               ELemento 0: token "create";
               ELemento 1: tipo do objeto instanciado.
               ELemento 2: nome do objeto instanciado.
               ELemento 3: token "Objeto" ou "Vetor" (deprecado).
               ELemento 4: tipo de um elemento do vetor.
               ELemento 5: lista de parametros, para o create, se for Objeto, ou parametros que compoe os indices matriciais se for Vetor.
               ELemento 6: indice do construtor compativel.
               Elemento 7: nome do objeto caller.
             * 
             */


            if (instrucao.expressoes[0].Elementos[0].ToString() != "create")
            {
                return null;
            }
                

            string tipoDoObjeto = instrucao.expressoes[0].Elementos[1].ToString();
            string nomeDoObjeto = instrucao.expressoes[0].Elementos[2].ToString();
            int indexConstructor = int.Parse(instrucao.expressoes[0].Elementos[6].ToString());


            Expressao expressoesParametros;
            if (instrucao.expressoes[0].Elementos[5] != null) 
            {
                expressoesParametros = instrucao.expressoes[0].Elementos[5];
            }
            else
            {
                expressoesParametros = new Expressao();
            }
                





            /// passos do algoritmo
            /// 1- criar o escopo no qual as modificações do construtor serão feitas.
            /// 2- criar objetos "actual", para acesso a construtores de classes herdadas.
            /// 3- adicionar no escopo as propriedades da classe do objeto, pois o construtor atua sobre as propriedades do objeto a ser construido.
            /// 4- executar o construtor.
            /// 5- passar os valores das propriedades do objeto construido, vindos do escopo do construtor.
            /// 6- remover os objetos "atual" do escopo do construtor.
            /// 7- remover as propriedades do objeto construido, do escopo do construtor.
           
            /// passo a mais: verificar se as listas de propriedades não são nulas, e se a lista de classes herdadas não são nulas.


            Objeto objJaInstanciado = escopo.tabela.GetObjeto(nomeDoObjeto, escopo);

            Metodo construtor = RepositorioDeClassesOO.Instance().GetClasse(tipoDoObjeto).construtores[indexConstructor];
            Classe classeDoObjetoInstanciado = RepositorioDeClassesOO.Instance().GetClasse(tipoDoObjeto);

            // constroi e registra o objeto atual.
            Objeto actual = objJaInstanciado.Clone();
            actual.SetNome("actual");
            escopo.tabela.RegistraObjeto(actual);

            Escopo escopoCreate = escopo;

            List<Classe> classesHerdadas = classeDoObjetoInstanciado.classesHerdadas;


            // PROCESSAMENTO DE PROPRIEDADES DE CLASSES HERDADAS.
            if ((classesHerdadas != null) && (classesHerdadas.Count > 0)) 
            {
                // constroi objetos de nome: "atual", para cada classe herdada.
                AdicionaObjetosAtuais(escopoCreate, classesHerdadas);
                // seta as propriedades modificadas no construtor, e que podem ter sido modificados por construtores herdados.
                AdicionaPropriedadesHerdadasAoEscopo(objJaInstanciado, classesHerdadas, escopoCreate); 
            }
            // PROCESSAMENTO DE PROPRIEDADES DA CLASSE DO OBJETO CRIADO
            AdicionaPropriedadesNaoHerdadas(objJaInstanciado, classeDoObjetoInstanciado, escopoCreate);



            ///__________________________________________________________________________________________________________________________________________________
            // CONSTROI UMA NOVA INSTANCIA DO OBJETO, o objeto construido está no [escopoCreate].
            object objetoResult = construtor.ExecuteAConstructor(objJaInstanciado, new List<Expressao>() { expressoesParametros }, tipoDoObjeto, escopoCreate, indexConstructor);


            // obtem valor de propriedade pelo objeto [actual] ([this] em outras linguagens).
            if ((objetoResult!=null) && (objetoResult.GetType() == typeof(Objeto)) && ((Objeto)objetoResult).GetNome() == "actual")
            {
                Objeto objCaller = escopo.tabela.GetObjeto(instrucao.expressoes[0].Elementos[7].ToString(), escopo);
                Objeto actual1= (Objeto)objetoResult;
                object valorField = actual1.GetField(nomeDoObjeto);
                if (objCaller.GetField(nomeDoObjeto) == null)
                {
                    objCaller.SetField(new Objeto("public", tipoDoObjeto, nomeDoObjeto, valorField));
                }
                else
                {
                    objCaller.SetValorField(nomeDoObjeto, valorField);
                }
                escopo.tabela.RemoveObjeto("actual");  
                return objCaller;
            }
            if (escopo.tabela.GetObjeto("actual", escopo) != null)
            {
                escopo.tabela.RemoveObjeto("actual");
            }
            //____________________________________________________________________________________________________________________________________________________

            objJaInstanciado.SetValor(escopoCreate.tabela.GetObjeto(objJaInstanciado.GetNome(), escopo).GetValor());

            AtualizaTabelaExpressoes(objJaInstanciado); // atualiza a lista de expressoes, pois o objeto foi modificado.

            SetaValoresModificadosDePropriedades(objJaInstanciado, escopoCreate); // repassa os valores modificados na construção, ao objeto instanciado.

            if (classesHerdadas != null)
            {
                RemoveObjetosAtuais(escopoCreate, classesHerdadas); // remove todos objetos atuais construidos, desde da classe do objeto construido, até os de classes herdadas.
                RemovePropriedadesPropriedadesHerdadaAoEscopo(objJaInstanciado, classesHerdadas, escopoCreate);
            }
            RemovePropriedadesNaoHerdadosDoEscopo(objJaInstanciado, classeDoObjetoInstanciado, escopoCreate); // remove os campos do objeto instanciado, presentes no escopo create.

            

            return objJaInstanciado;
        } 



        private static void AtualizaTabelaExpressoes(Objeto objJaInstanciado)
        {
            ExpressaoObjeto expressaoObjetoInstanciado = new ExpressaoObjeto(objJaInstanciado);
            if (TablelaDeValores.expressoes != null)
                for (int x = 0; x < TablelaDeValores.expressoes.Count; x++)
                {
                    if ((TablelaDeValores.expressoes[x].Elementos != null) && (TablelaDeValores.expressoes[x].Elementos.Count > 0))
                    {
                        for (int subExprss = 0; subExprss < TablelaDeValores.expressoes[x].Elementos.Count; subExprss++)
                            if (Expressao.Instance.IsEqualsExpressions(TablelaDeValores.expressoes[x].Elementos[subExprss], expressaoObjetoInstanciado))
                                TablelaDeValores.expressoes[x].isModify = true;

                    }
                }
        }

        private static void SetaValoresModificadosDePropriedades(Objeto objJaInstanciado, Escopo escopoCreate)
        {
            foreach (Objeto propriedadeModificada in escopoCreate.tabela.GetObjetos())
            {
                if (objJaInstanciado.GetFields().Find(k => k.GetNome() == propriedadeModificada.GetNome()) != null)
                {
                    objJaInstanciado.GetField(propriedadeModificada.GetNome()).SetValor(propriedadeModificada.GetValor());
                }
                    
            }
                
        }

       


        private static void AdicionaPropriedadesHerdadasAoEscopo(Objeto objJaInstanciado, List<Classe> classesHerdadas, Escopo escopo)
        {
            foreach (Classe classesHeranca in classesHerdadas)
            {

                foreach (Objeto propriedadeHerdada in classesHeranca.GetPropriedades())
                {
                    if ((propriedadeHerdada.GetAcessor() == "public") || (propriedadeHerdada.GetAcessor() == "protected"))
                    {
                        escopo.tabela.GetObjetos().Add(propriedadeHerdada);
                    }
                        
                }
                    
            }
        }



        private static void AdicionaPropriedadesNaoHerdadas(Objeto objJaInstanciado, Classe classeDoObjetoInstanciado, Escopo escopoCreate)
        {
               
            if (classeDoObjetoInstanciado.GetPropriedades() != null)
            {
                foreach (Objeto propriedadeDoObjeto in classeDoObjetoInstanciado.GetPropriedades())
                {
                    if ((propriedadeDoObjeto.GetNome() != objJaInstanciado.GetNome()) && (escopoCreate.tabela.GetObjeto(propriedadeDoObjeto.GetNome(), escopoCreate) == null))
                    {
                        escopoCreate.tabela.GetObjetos().Add(propriedadeDoObjeto); // faz um registro das propriedades do objeto, que poderão ser modificadas pelo construtor!
                    }
                    else
                    if (escopoCreate.tabela.GetObjeto(propriedadeDoObjeto.GetNome(), escopoCreate) != null) 
                    {
                        object valor = propriedadeDoObjeto.valor;
                        escopoCreate.tabela.GetObjeto(propriedadeDoObjeto.GetNome(), escopoCreate).valor = valor;
                    }
                        

                }


            }
            
        }


        private static void RemovePropriedadesPropriedadesHerdadaAoEscopo(Objeto objJaInstanciado, List<Classe> classesHerdadas, Escopo escopoCreate)
        {
            if (objJaInstanciado != null)
            {
                foreach (Classe umaClasseHerdada in classesHerdadas)
                {
                    foreach (Objeto umaPropriedadeHerdada in umaClasseHerdada.GetPropriedades())
                    {
                        if ((umaPropriedadeHerdada.GetAcessor() == "public") || (umaPropriedadeHerdada.GetAcessor() == "private"))
                        {
                            escopoCreate.tabela.GetObjetos().Remove(umaPropriedadeHerdada);
                        }
                            
                    }
                        
                }
                    
            }
                
            
        }


        private static void RemovePropriedadesNaoHerdadosDoEscopo(Objeto objJaInstanciado, Classe classeDoObjetoInstanciado, Escopo escopoCreate)
        {
            if (objJaInstanciado != null)
            {
                if (classeDoObjetoInstanciado.GetPropriedades() != null)
                {
                    foreach (Objeto propriedadeDoObjeto in classeDoObjetoInstanciado.GetPropriedades())
                    {
                        if (propriedadeDoObjeto.GetNome() != objJaInstanciado.GetNome())
                        {
                            // faz um registro das propriedades do objeto, que poderão ser modificadas pelo construtor!
                            escopoCreate.tabela.GetObjetos().Remove(propriedadeDoObjeto); 
                        }
                            
                    }
                        
                }
                    
            }
        }


        //  instrução de construção do casesOfUses. como não é uma instrução feita massivamente, a construção de um objeto ProcessadorDeID não afeta o desempenho.
        private object InstrucaoCasesOfUse(Instrucao instrucao, Escopo escopo)
        {

            // formato:
            /// exprss[umCaseIndex]:a expressao condicional do case.
            /// instrucao[umCaseIndex]: instrucoes.


            List<Expressao> expressoes = instrucao.expressoes;
            

            List<Expressao> exprssCodicionaisDoCase = instrucao.expressoes;

            EvalExpression eval = new EvalExpression(); // inicializa o avaliador de expressões.

            // percorre os cases da instrução, se a expressão condicional do case for true, roda a lista de instruções do case.
            for (int x = 0; x < exprssCodicionaisDoCase.Count; x++)
            {
                exprssCodicionaisDoCase[x].isModify = true;



                bool resultCondicionalDoCase = (bool)eval.EvalPosOrdem(exprssCodicionaisDoCase[x], escopo);
                if (resultCondicionalDoCase)
                {
                    // obtém o bloco de instruções do case avaliado.
                    List<Instrucao> instrucoesDoCase = instrucao.blocos[x]; 
                    for (int i = 0; i < instrucoesDoCase.Count; i++)
                        this.ExecutaUmaInstrucao(instrucoesDoCase[i], escopo);
                } // if
            } // for x
            return null;
        } 


        private object InstrucaoOperadorBinario(Instrucao instrucao, Escopo escopo)
        {
            // obtém alguns dados do novo operador binario.
            string tipoRetornoDoOperador = ((ExpressaoElemento)instrucao.expressoes[0]).GetElemento().ToString();
            string nomeOperador = ((ExpressaoElemento)instrucao.expressoes[1]).GetElemento().ToString();
            string tipoOperando1 = ((ExpressaoElemento)instrucao.expressoes[2]).GetElemento().ToString();
            string nomeOpérando1 = ((ExpressaoElemento)instrucao.expressoes[3]).GetElemento().ToString();
            string tipoOperando2 = ((ExpressaoElemento)instrucao.expressoes[4]).GetElemento().ToString();
            string nomeOperando2 = ((ExpressaoElemento)instrucao.expressoes[5]).GetElemento().ToString();
            int prioridade = int.Parse(((ExpressaoElemento)instrucao.expressoes[6]).GetElemento().ToString());
            Metodo metodoDeImplantacaoDoOperador = ((ExpressaoChamadaDeMetodo)instrucao.expressoes[7]).funcao;


            // encontra a classe em que se acrescentará o novo operador binnario.
            Classe classeDoOperador = escopo.tabela.GetClasses().Find(k => k.GetNome() == tipoRetornoDoOperador);
            if (classeDoOperador == null)
            {
                return null;
            }
                
            // consroi o novo operador binario, a partir dos dados coletados.
            Operador novoOperadorBinario = new Operador(classeDoOperador.GetNome(), nomeOperador, prioridade, new string[] { tipoOperando1, tipoOperando2 }, tipoRetornoDoOperador, metodoDeImplantacaoDoOperador, escopo);
            novoOperadorBinario.funcaoImplementadoraDoOperador = metodoDeImplantacaoDoOperador; // seta a função de cálculo do operador.

            if (novoOperadorBinario == null)
            {
                return null;
            }
                

            // adiciona o novo operador binario para a classe do tipo de retorno do operador.
            classeDoOperador.GetOperadores().Add(novoOperadorBinario);

            LinguagemOrquidea.Instance().AddOperator(novoOperadorBinario);

            // atualiza a classe no repositório.
            Classe classeRepositorio = RepositorioDeClassesOO.Instance().GetClasse(tipoRetornoDoOperador);
            if (classeRepositorio != null)
            {
                RepositorioDeClassesOO.Instance().GetClasse(tipoRetornoDoOperador).GetOperadores().Add(novoOperadorBinario);
            }
                
            return null;
        }

        // instrução de construção de operador binario. como não é uma instrução feita massivamente, a construção de um objeto ProcessadorDeID não afeta o desempenho.
        private object InstrucaoOperadorUnario(Instrucao instrucao, Escopo escopo)
        {
            // obtém alguns dados do novo operador unario.
            string tipoRetornoDoOperador = ((ExpressaoElemento)instrucao.expressoes[0]).GetElemento().ToString();
            string nomeOperador = ((ExpressaoElemento)instrucao.expressoes[1]).GetElemento().ToString();
            string tipoOperando1 = ((ExpressaoElemento)instrucao.expressoes[2]).GetElemento().ToString();
            string nomeOpérando1 = ((ExpressaoElemento)instrucao.expressoes[3]).GetElemento().ToString();
            int prioridade = int.Parse(((ExpressaoElemento)instrucao.expressoes[4]).GetElemento().ToString());
            Metodo funcaoOperador = ((ExpressaoChamadaDeMetodo)instrucao.expressoes[5]).funcao;


            // encontra a classe em que se acrescentará o novo operador unario.
            Classe classeDoOperador = escopo.tabela.GetClasses().Find(k => k.GetNome() == tipoRetornoDoOperador);
            if (classeDoOperador == null)
            {
                return null;
            }
                
            // consroi o novo operador unario, a partir dos dados coletados.
            Operador novoOperadorUnario = new Operador(classeDoOperador.GetNome(), nomeOperador, prioridade, new string[] { tipoOperando1 }, tipoRetornoDoOperador, ((ExpressaoChamadaDeMetodo)instrucao.expressoes[5]).funcao, escopo);

            novoOperadorUnario.funcaoImplementadoraDoOperador = funcaoOperador;
            if (novoOperadorUnario == null)
            {

            }
             

            // atualiza a classe no escopo.
            classeDoOperador.GetOperadores().Add(novoOperadorUnario); // adiciona o novo operador unario para a classe do tipo de retorno do operador.

            LinguagemOrquidea.Instance().AddOperator(novoOperadorUnario);

            // atualiza a classe no repositorio.
            Classe classeRepositorioOperador = RepositorioDeClassesOO.Instance().GetClasse(tipoRetornoDoOperador);
            if (classeRepositorioOperador != null)
            {
                RepositorioDeClassesOO.Instance().GetClasse(tipoRetornoDoOperador).GetOperadores().Add(novoOperadorUnario);
            }
                
            return null;
        }


        /// obtém o valor de uma variável.
        /// a variavel está na expressão[0].
        private object InstrucaoGetObjeto(Instrucao instruvcao, Escopo escopo)
        {
            object valor = ((ExpressaoObjeto)instruvcao.expressoes[0]).objectCaller.GetValor();
            return valor;
        }

        // seta o valor de uma variável.
        // o valor está na expressao[1].
        private object InstrucaoSetObjeto(Instrucao instrucao, Escopo escopo)
        {
            Objeto v = ((ExpressaoObjeto)instrucao.expressoes[0]).objectCaller;
            v.SetValor(((ExpressaoElemento)instrucao.expressoes[1]).elemento);
            AtualizaTabelaExpressoes(v);
            return null;
        }

        private object InstrucaoReturn(Instrucao instrucao, Escopo escopo)
        {
            if ((instrucao.expressoes == null) || (instrucao.expressoes.Count == 0))
                return null;

            instrucao.expressoes[0].isModify = true;
            EvalExpression eval = new EvalExpression();
            object objRetorno = eval.EvalPosOrdem(instrucao.expressoes[0], escopo);

            return objRetorno;
          

        }

        private object InstrucaoWhile(Instrucao instrucao, Escopo escopo)
        {

            Expressao exprssControle = instrucao.expressoes[0];
            
            EvalExpression eval = new EvalExpression();
            exprssControle.isModify = true;
            while ((bool)eval.EvalPosOrdem(exprssControle, escopo))
            {
                Executa_bloco(instrucao, escopo, 0);
                exprssControle.isModify = true;
            }
            return null;
        } // WhileInstrucao()

        private object InstrucaoIfElse(Instrucao instrucao, Escopo escopo)
        {

            // expresssao controle: expressao[0]
            // blocos: instrucoes.Blocos. (2 para else).

            Expressao exprssControle = instrucao.expressoes[0];
            exprssControle.isModify = true;
            
            
            EvalExpression eval = new EvalExpression();
            if ((bool)eval.EvalPosOrdem(exprssControle, escopo))
            {
                return Executa_bloco(instrucao, escopo, 0);
            }
                
            else
            {
                if (instrucao.blocos.Count > 1)  //procesamento da instrução else. O segundo bloco é para instrução else.
                {
                    return Executa_bloco(instrucao, escopo, 1);
                }
                    
            }
            exprssControle.isModify = true;
            return null;

        } // IfElseInstrucao()

        private object Executa_bloco(Instrucao instrucao, Escopo escopo, int bloco)
        {
            object result = null;
            for (int umaInstrucao = 0; umaInstrucao < instrucao.blocos[bloco].Count; umaInstrucao++)
            {
                if (instrucao.blocos[bloco][umaInstrucao].code == codeBreak)
                {
                    break;
                }
                    
                if (instrucao.blocos[bloco][umaInstrucao].code == codeContinue)
                {
                    continue;
                }

                if (instrucao.blocos[bloco][umaInstrucao].code != codeReturn) 
                {
                    result = ExecutaUmaInstrucao(instrucao.blocos[bloco][umaInstrucao], escopo);
                }
                else
                {
                    object resultReturn = new EvalExpression().EvalPosOrdem(instrucao.blocos[bloco][umaInstrucao].expressoes[0], escopo);
                    return resultReturn;
                }

                
                    
                

            } // for bloco
            return result;
        }

        private object InstrucaoFor(Instrucao instrucao, Escopo escopo)
        {

            /// template: for (int x=0;x.controleLimite; x++)
            /// template instruções:
            ///    instrucao.expressoes[0]: expressão de atribuição da variável controle. 
            ///    instrucao.expressoes[1]; expressão de controle para a instrução.
            ///    instrucao.expressoes[2]: expressão de incremento da instrucao.
            ///    

            Expressao exprssAtribuicao = instrucao.expressoes[0];
            Expressao exprsCondicional = instrucao.expressoes[1];
            Expressao exprsIncremento = instrucao.expressoes[2];

            EvalExpression eval = new EvalExpression();

            object valorAtribuicao = (((Objeto)eval.EvalPosOrdem(exprssAtribuicao, escopo)).valor);

            int varAtribuicao = (int)valorAtribuicao; // faz a primeira atribuição do controle da malha.

            // calcula o limite do contador para o "for".
            Expressao exprssControle = exprsCondicional.Clone();
            exprssControle.isModify = true;


            
      
            while ((bool)eval.EvalPosOrdem(exprsCondicional, escopo))  // avalia a expressão de controle.
            {
                exprsCondicional.isModify = true;
                if ((instrucao.blocos[0] != null) && (instrucao.blocos[0].Count > 0))
                {
                    

                    // executa as instrucoes do operador bloco.
                    Executa_bloco(instrucao, escopo, 0);



                    exprsIncremento.isModify = true;

                    // calcula o proximo valor da variavel de malha.
                    varAtribuicao = (int)eval.EvalPosOrdem(exprsIncremento, escopo);
                    // atualiza a expressão de incremento.
                    escopo.tabela.GetObjeto(((ExpressaoObjeto)exprsIncremento.Elementos[0]).nomeObjeto, escopo).SetValor(varAtribuicao);


                }
                else
                    break;
            } // for

            return null;
        } // ForInstrucao()

        private object InstrucaoAtribuicao(Instrucao instrucao, Escopo escopo)
        {

            /// estrutura de dados para atribuicao:
            /// 0- Elemento[0]: tipo do objeto.
            /// 1- Elemento[1]: nome do objeto.
            /// 2- Elemento[2]: se tiver propriedades/metodos aninhados: expressao de aninhamento. Se não tiver, ExpressaoElemento("") ".
            /// 3- Elemento[3]: expressao da atribuicao ao objeto. (se nao tiver: ExpressaoELemento("")




            string tipoObjetoAAtribuir = ((ExpressaoElemento)instrucao.expressoes[0].Elementos[0]).elemento;
            string nomeObjetoAAtribuir = ((ExpressaoElemento)instrucao.expressoes[0].Elementos[1]).elemento;


            EvalExpression eval = new EvalExpression();
            Expressao atribuicao = instrucao.expressoes[0].Elementos[3];
            
            object novoValorObjeto = null;
            if (atribuicao != null)
            {   

                novoValorObjeto = eval.EvalPosOrdem(atribuicao, escopo);
                if ((novoValorObjeto != null) && (novoValorObjeto.GetType() == typeof(Objeto)))
                {
                    Objeto objValor= (Objeto)novoValorObjeto;
                    novoValorObjeto = objValor.valor;
                }
                
            }


            // PROCESSAMENTO DE OBJETOS PROPRIEDADES ESTATICAS.
            if ((instrucao.expressoes[0].Elementos.Count >= 5) && (((ExpressaoElemento)instrucao.expressoes[0].Elementos[4]).elemento == "estatica"))
            {

                string tipoDaPropriedadeEstatica = tipoObjetoAAtribuir;
                string nomeDaPropriedadeEstatica = nomeObjetoAAtribuir;

                Classe classe = escopo.tabela.GetClasse(tipoDaPropriedadeEstatica, escopo);
                if (classe != null)
                {
                    object novoValorB = new EvalExpression().EvalPosOrdem(instrucao.expressoes[0].Elementos[3], escopo);

                    Objeto objPropEstatica = classe.propriedadesEstaticas.Find(k => k.GetNome().Equals(nomeDaPropriedadeEstatica));
                    if (objPropEstatica != null)
                    {
                        objPropEstatica.valor = novoValorB;
                    }


                    AtualizaTabelaExpressoes(objPropEstatica);
                    return classe.propriedadesEstaticas.Find(k => k.GetNome().Equals(nomeDaPropriedadeEstatica));

                }


            }
            // PROCESSAMENTO DE OBJETOS EM EXPRESSAO OBJETO.
            if (instrucao.expressoes[0].Elementos[1].GetType() == typeof(ExpressaoObjeto))
            {


                Objeto objAtribuir = escopo.tabela.GetObjeto(nomeObjetoAAtribuir, escopo);

                if (objAtribuir != null)
                {
                    objAtribuir.valor = novoValorObjeto;
                    escopo.tabela.RemoveObjeto(nomeObjetoAAtribuir);
                    escopo.tabela.RegistraObjeto(objAtribuir);
                    return objAtribuir;
                }
                else
                {
                    Objeto objCreated = new Objeto("private", tipoObjetoAAtribuir, nomeObjetoAAtribuir, novoValorObjeto);
                    escopo.tabela.RegistraObjeto(objCreated);

                    return objCreated;
                }

            }
            else
            // PROCESSAMENTO DE PROPRIEDADES ANINHADAS NAO ESTATICAS.
            if (instrucao.expressoes[0].Elementos[1].GetType() == typeof(ExpressaoPropriedadesAninhadas))
            {
                ExpressaoPropriedadesAninhadas exprssAninhamento = (ExpressaoPropriedadesAninhadas)instrucao.expressoes[0].Elementos[1];

                Objeto objCaller = exprssAninhamento.objectCaller;
                objCaller.SetValorField(nomeObjetoAAtribuir, novoValorObjeto);

            }
            // PROCESSAMENTO DE VARIAVEIS, QUE NAO SAO OBJETOS! SAO TIPOS DE BASE: STRING, INT,... QUE NAO SAO DO TIPO OBJETO!
            else
            {
                Objeto objAtribui = escopo.tabela.GetObjeto(nomeObjetoAAtribuir, escopo);
                if ((objAtribui != null) && (novoValorObjeto != null) && (nomeObjetoAAtribuir!="actual"))
                {
                    if (novoValorObjeto.GetType() != typeof(Objeto))
                    {
                        if (escopo.tabela.GetObjeto(nomeObjetoAAtribuir, escopo) != null)
                        {
                            escopo.tabela.GetObjeto(nomeObjetoAAtribuir, escopo).valor = novoValorObjeto;
                        }

                    }
                    else
                    if (novoValorObjeto.GetType() == typeof(Objeto))
                    {
                        Objeto objValor = (Objeto)novoValorObjeto;
                        objAtribui.valor = objValor.valor;
                    }



                    return objAtribui;
                }
                else
                if (objAtribui.nome=="actual")
                {
                    // FIXAR O OBJETO CALLER, pois a propriedade aninhada é deste objeto.
                    Objeto objAtribuiInstanciado = new Objeto("public", tipoObjetoAAtribuir, nomeObjetoAAtribuir, novoValorObjeto);
                    objAtribuiInstanciado.valor = novoValorObjeto;
                    escopo.tabela.RegistraObjeto(objAtribuiInstanciado);
                    return objAtribuiInstanciado;

                }
                else
                if (escopo.tabela.GetObjeto("actual", escopo) != null)
                {
                    Objeto actual = escopo.tabela.GetObjeto("actual", escopo);
                    actual.SetValorField(nomeObjetoAAtribuir, novoValorObjeto);

                    return actual;
                }
            
    }
            return null;
        } // InstrucaoAtribuicao()

        private static string ObtemTipoRecursivamente(Escopo escopo, string nomePropriedadeProcurada, Objeto objetoAtribuicaoCampo)
        {
            if (objetoAtribuicaoCampo == null)
            {
                return null;
            }
                

            if (objetoAtribuicaoCampo.GetNome() == nomePropriedadeProcurada)
            {
                return objetoAtribuicaoCampo.GetTipo();
            }
                
            else
            {
                string tipoPropriedadeProcurada = null;
                for (int i = 0; i < objetoAtribuicaoCampo.GetFields().Count; i++)
                {

                    tipoPropriedadeProcurada = ObtemTipoRecursivamente(escopo, nomePropriedadeProcurada, objetoAtribuicaoCampo.GetFields()[i]);
                    if (tipoPropriedadeProcurada != null)
                    {
                        return tipoPropriedadeProcurada;
                    }
                        
                }
            }
            return null;
        }

        private object InstrucaoChamadaDeFuncao(Instrucao instrucao, Escopo escopo)
        {

            if (instrucao.expressoes[1].GetType() == typeof(ExpressaoChamadaDeMetodo))
            {
                ExpressaoChamadaDeMetodo funcaoExpressao = (ExpressaoChamadaDeMetodo)instrucao.expressoes[1];
                Metodo funcaoDaChamada = funcaoExpressao.funcao;
                List<Expressao> expressoesParametros = funcaoExpressao.Elementos;
                return funcaoDaChamada.ExecuteAFunction(expressoesParametros, funcaoDaChamada.caller, escopo);
            } // if
            return null;
        }




        private object InstrucaoDefinicaoDeFuncao(Instrucao instrucao, Escopo escopo)
        {
            // a instrucao de definicao nao eh executada no programa VM
            return null;
        }


        private object InstrucaoRiseError(Instrucao instrucao, Escopo escopo)
        {
            if (instrucao.blocos.Count > 0)
            {
                try
                {
                    if ((instrucao.blocos[0] != null) && (instrucao.blocos[0] != null) && (instrucao.blocos[0].Count > 0)) 
                    {
                        List<Instrucao> umBlocoTry = instrucao.blocos[0];
                        for (int x = 0; x < umBlocoTry.Count; x++)
                            ExecutaUmaInstrucao(umBlocoTry[x], escopo);

                        return null;
                    }
                }
                catch
                {
                    if ((instrucao.blocos.Count > 1) && (instrucao.blocos[1] != null) && (instrucao.blocos[1].Count > 0))
                    {
                        List<Instrucao> umBlocoRiseError = instrucao.blocos[0];
                        for (int x = 0; x < umBlocoRiseError.Count; x++)
                            ExecutaUmaInstrucao(umBlocoRiseError[x], escopo);

                        return null;
                    }
                }
            }

            return null;
        }

        private object InstrucaoAspecto(Instrucao instrucao, Escopo escopo)
        {

            if (instrucao.expressoes[0].GetType() == typeof(ExpressaoChamadaDeMetodo))
            {
                // obtem a funcao do corte aspecto.
                ExpressaoChamadaDeMetodo umaExpressaoObjetoChamadaDeMetodo = (ExpressaoChamadaDeMetodo)instrucao.expressoes[0];
                Metodo funcaoDaChamada = umaExpressaoObjetoChamadaDeMetodo.funcao;

                // obtem o nome do objeto parametro da funcao corte.
                string nomeObjetoMonitorado = ((ExpressaoElemento)instrucao.expressoes[1]).ToString();
                
                
                if (escopo.tabela.GetObjeto(nomeObjetoMonitorado, escopo) != null)
                {
                    /// int b; 
                    /// int funcao(int x);
                    /// x.valor= b.valor;
                    /// b.valor= funcao(x);
                    
                    Objeto objetoSobAspecto = escopo.tabela.GetObjeto(nomeObjetoMonitorado, escopo);// obtem o objeto sob aspecto, no escopo.
                    funcaoDaChamada.parametrosDaFuncao[0].SetValor(objetoSobAspecto.GetValor());

                    // obtem o valor da funcao corte, tendo parametro com valor do objeto monitorado.
                    object novoValor = funcaoDaChamada.ExecuteAFunction(
                        new List<Expressao>() { new ExpressaoObjeto(funcaoDaChamada.parametrosDaFuncao[0]) },
                        funcaoDaChamada.caller, escopo);

                    // seta o valor do objeto monitorado, com o valor de retorno da função.
                    escopo.tabela.GetObjeto(nomeObjetoMonitorado, escopo).SetValor(novoValor);

                    // atualiza a situação das expressoes que possuem o objeto monitorado.
                    AtualizaTabelaExpressoes(objetoSobAspecto);

                    if (escopo.tabela.GetObjeto(funcaoDaChamada.parametrosDaFuncao[0].GetNome(), escopo) != null)
                    {
                        escopo.tabela.GetObjeto(funcaoDaChamada.parametrosDaFuncao[0].GetNome(), escopo).SetValor(novoValor);
                        // atualiza a situação das expressoes que possuem o objeto parametro da funcao de corte.
                        AtualizaTabelaExpressoes(escopo.tabela.GetObjeto(funcaoDaChamada.parametrosDaFuncao[0].GetNome(), escopo));
                    }

                    return novoValor;
                }
                
            } // if
            return null;
        }



    } // class ProcessamentoInstrucoes()


    // uma instrução da linguagem orquidea  tem 4 objetos: 1- um id do tipo int, para controle de chamadas de métodos/funções, 2- o codigo da instrução, 3- a lista de expressões utilizadas, 4- a lista de blocos de sequencias que comporarão bloos associados à instrução.
    public class Instrucao
    {
        public int code; // tipo da instrução.
        //public int IP_Instrucao = 0; // ponteiro de obter instruções a serem avaliadas.

        public List<Expressao> expressoes { get; set; } // expressões da instrução.
        public List<List<Instrucao>> blocos { get; set; } // blocos de instrução associada à instrução.
        public List<int> flags { get; set; }

        public delegate void BuildInstruction(int code, List<Expressao> expressoesDaInstrucao, List<List<Instrucao>> blocos, UmaSequenciaID sequencia);


        public const int EH_OBJETO = 1; //: a atribuica é feita sobre um objeto.
        public const int EH_VETOR = 7; // a atribuicao é feita sobre uma variavel vetor.
        public const int EH_PRPOPRIEDADE_ESTATICA = 4; //a atribuição é feita sobre uma propriedade estatica.
     
        public const int EH_DEFINICAO = 5; //é definição (criação)
        public const int EH_MODIFICACAO = 6; //sem definicao, apenas modificacao do valor.

        private static Dictionary<int, string> dicNamesOfInstructions;
        private static System.Random random = new System.Random();

        public void InitNames()
        {
            
            dicNamesOfInstructions = new Dictionary<int, string>();
            dicNamesOfInstructions = new Dictionary<int, string>();
            dicNamesOfInstructions[ProgramaEmVM.codeAtribution] = "Atribution";
            dicNamesOfInstructions[ProgramaEmVM.codeCallerFunction] = "Caller of a Function";
            dicNamesOfInstructions[ProgramaEmVM.codeDefinitionFunction] = "Definition of a Function";
            dicNamesOfInstructions[ProgramaEmVM.codeIfElse] = "if/else";
            dicNamesOfInstructions[ProgramaEmVM.codeFor] = "for";
            dicNamesOfInstructions[ProgramaEmVM.codeWhile] = "while";
            dicNamesOfInstructions[ProgramaEmVM.codeReturn] = "return";
            dicNamesOfInstructions[ProgramaEmVM.codeContinue] = "continue flux";
            dicNamesOfInstructions[ProgramaEmVM.codeBreak] = "break flux";
            dicNamesOfInstructions[ProgramaEmVM.codeGetObjeto] = "GetObjeto";
            dicNamesOfInstructions[ProgramaEmVM.codeSetObjeto] = "SetVar";
            dicNamesOfInstructions[ProgramaEmVM.codeOperadorBinario] = "operador binario";
            dicNamesOfInstructions[ProgramaEmVM.codeOperadorUnario] = "operador unario";
            dicNamesOfInstructions[ProgramaEmVM.codeCasesOfUse] = "casesOfUse";
            dicNamesOfInstructions[ProgramaEmVM.codeCreateObject] = "Create a Object";
            dicNamesOfInstructions[ProgramaEmVM.codeCallerMethod] = "Call a method";
            dicNamesOfInstructions[ProgramaEmVM.codeExpressionValid] = "Valid Express";
            dicNamesOfInstructions[ProgramaEmVM.codeConstructorUp] = "Constructor base";
            dicNamesOfInstructions[ProgramaEmVM.codeAspectos] = "aspect insertion";

    
        }


        public delegate Instrucao handlerCompilador(UmaSequenciaID sequencia, Escopo escopo);
        
        /// POSSIBILITA A EXTENSÃO DE INSTRUÇÕES DA LINGUAGEM, REUNINDO EM UM SÓ LUGAR TODOS OBJETOS NECESSÁRIOS PARA IMPLEMENTAR UMA NOVA INSTRUÇÃO.

        
        /// adiciona um novo tipo de instrução, com id identificador, texto contendo a sintaxe da instrução, e um método para processamento da instrução, e
        /// um metodo para comiplar a instrucao, e também uma sequencia id para reconhecer a instrucao, no compilador.
        public void AddNewTypeOfInstruction(int code, string templateInstruction,ProgramaEmVM.HandlerInstrucao instruction, string sequenciaID_mapeada, handlerCompilador buildCompilador)
        {
            dicNamesOfInstructions[code] = templateInstruction;
            ProgramaEmVM.dicHandlers[code] = instruction;
        }

   
        public Instrucao()
        {
            // do nothing, for instructions processed, but not needs execution in run-time.
        }

        // construtor. contém os elementos de uma instrução VM: codigo ID, expressoes associadas, e blocos de instruções.
        public Instrucao(int code, List<Expressao> expressoesDaInstrucao, List<List<Instrucao>> blocos)
        {
            this.flags = new List<int>();
            if (dicNamesOfInstructions == null)
                this.InitNames();
         
            this.code = code;
            if ((expressoesDaInstrucao != null) && (blocos != null))
            {
                this.expressoes = expressoesDaInstrucao.ToList<Expressao>();
                this.blocos = blocos.ToList<List<Instrucao>>();
            }  // if
            else
            {
                this.expressoes = new List<Expressao>();
                this.blocos = new List<List<Instrucao>>();
            }

        } // Instrucao()


        // construtor. contém os elementos de uma instrução VM: codigo ID, expressoes associadas, e blocos de instruções.
        public Instrucao(int code, List<Expressao> expressoesDaInstrucao, List<List<Instrucao>> blocos, Escopo escopo)
        {
            this.flags = new List<int>();
            if (dicNamesOfInstructions == null)
                this.InitNames();
            this.code = code;
            if ((expressoesDaInstrucao != null) && (blocos != null))
            {
                this.expressoes = expressoesDaInstrucao;
                this.blocos = blocos.ToList<List<Instrucao>>();
            }  // if

        } // Instrucao()

        /// padrão de projetos [Command], permitindo que a lista de instruções da VM possa ser extendida.
        public void AddTipoInstrucao(ProgramaEmVM.HandlerInstrucao novoTipoInstrucao, int code)
        {
            // para inserir um novo comando, construa a string de definicao, o metodo Build, o indice de codigo, e o metodo de execução do comando.
            ProgramaEmVM.dicHandlers[code] = novoTipoInstrucao;
        }

        
        public override string ToString()
        {
            return dicNamesOfInstructions[this.code].ToString();
            
        }






        public class Testes : SuiteClasseTestes
        {
            public Testes() : base("teste de execução de instruçoes orquidea")
            {
            }


            public void TestesPequenosProgramasTextos(AssercaoSuiteClasse assercao)
            {
                string pathProgramaLeNome = "programasTestes\\programaLeNome.txt";

                List<string> lstPrograms = new List<string>() { pathProgramaLeNome };

                for (int i = 0; i < lstPrograms.Count; i++)
                {
                    Expressao.headers = null;

                    ParserAFile program = new ParserAFile(lstPrograms[i]);
                    ProcessadorDeID compilador = new ProcessadorDeID(program.GetTokens());
                    compilador.Compilar();



                    ProgramaEmVM programaVM = new ProgramaEmVM(compilador.GetInstrucoes());
                    Escopo escopo = compilador.escopo.Clone();
                    programaVM.Run(escopo);

                }


            }

            public void TestesUnitariosCreate(AssercaoSuiteClasse assercao)
            {
                string code_0_0 = "public class classeB { public int propriedadeB;  public classeB(int x) { actual.propriedadeB=1; } };  classeB obj = create (1); ";
                string code_0_1 = "public class classeB { public int propriedadeB;  public classeB(int x) { actual.propriedadeB=-1; } };  classeB obj = create (1);";
                string code_0_2 = "public class classeB { public int propriedadeB;  public classeB(int x) { actual.propriedadeB=x; } };  classeB obj = create (4);";
                string code_0_3 = "public class classeB { public int propriedadeB;  public classeB(int x) { actual.propriedadeB=x+1; } };  classeB obj = create (1);";

                string valueExpected_0_0 = "1";
                string valueExpected_0_1 = "-1";
                string valueExpected_0_2 = "4";
                string valueExpected_0_3 = "2";




                List<string> codeClasses = new List<string>() { code_0_1, code_0_2, code_0_0, code_0_3 };
                List<string> valuesExpected = new List<string>() { valueExpected_0_1, valueExpected_0_2, valueExpected_0_0, valueExpected_0_3 };


                for (int i = 0; i < codeClasses.Count; i++)
                {
                    Expressao.headers = null;
                    ProcessadorDeID compilador = new ProcessadorDeID(codeClasses[i]);
                    compilador.Compilar();

                    ProgramaEmVM program = new ProgramaEmVM(compilador.GetInstrucoes());
                    program.Run(compilador.escopo);

                    try
                    {
                        assercao.IsTrue(ValidateValueObjects(compilador.escopo, "obj", "propriedadeB", valuesExpected[i]));
                    }
                    catch (Exception e)
                    {
                        string codeError = e.Message;
                        assercao.IsTrue(false, "TESTE FALHOU");
                    }

                }



            }

            public void TestesPequenosProgramas(AssercaoSuiteClasse assercao)
            {
                string pathProgramaNumeroParOuImpar = @"programasTestes\programaNumeroParOuImpar.txt";
                string pathProgramaFatorialRecursivo = "programasTestes\\programaFatorialRecursivo.txt";
                string pahtProgramaFatorial = "programasTestes\\programaFatorial.txt";
                string pathProgramaContagens = "programasTestes\\programContagens.txt";


                List<string> fileProgramasTestes = new List<string>() { pathProgramaFatorialRecursivo, pathProgramaNumeroParOuImpar, pahtProgramaFatorial, pathProgramaContagens };
                List<string> resultsExpected = new List<string>() { "120", "1", "120", "5" };
                List<string> objectsResult = new List<string>() { "y", "y", "b", "y" };



                for (int x = 0; x < fileProgramasTestes.Count; x++)
                {
                    try
                    {
                        Expressao.headers = null;

                        ParserAFile program = new ParserAFile(fileProgramasTestes[x]);
                        ProcessadorDeID compilador = new ProcessadorDeID(program.GetTokens());
                        compilador.Compilar();


                        ProgramaEmVM programaVM = new ProgramaEmVM(compilador.GetInstrucoes());
                        programaVM.Run(compilador.escopo);


                        assercao.IsTrue(ValidateValueObjects(compilador.escopo, objectsResult[x], resultsExpected[x]));
                    }
                    catch (Exception e)
                    {
                        string msgError = e.Message;
                        assercao.IsTrue(false, "TESTE FALHOU!");
                    }
                }

            }


 
 
            public void TestesUnitariosInstrucaoCasesOfUse(AssercaoSuiteClasse assercao)
            {
                // sintaxe: "casesOfUse y: { (case < x): { y = 2; }; } ";
                // instrucao cases of use.                           
                string code_0_0 = "int x = 5;  int y= 1;  casesOfUse y:  { (case < x): { y = 2; };} ";
                string code_0_1 = "int x = 5;  int y= 1;  casesOfUse y:  { (case >= x): { y = 2; };} ";
                string code_0_2 = "int x = -5; int y= 1;  casesOfUse y:  { (case == x): { y = 2; };} ";
                string code_0_3 = "int x = 1;  int y= 1;  casesOfUse y:  { (case == x): { y = 2; };} ";

                string value_expected_0_0 = "2";
                string value_expected_0_1 = "1";
                string value_expected_0_2 = "1";
                string value_expected_0_3 = "2";

                List<string> codesCaseOfUse = new List<string>() { code_0_0, code_0_1, code_0_2, code_0_3 };
                List<string> values_expected = new List<string>() { value_expected_0_0, value_expected_0_1, value_expected_0_2, value_expected_0_3 };

                for (int i = 0; i < codesCaseOfUse.Count; i++)
                {
                    ProcessadorDeID compilador = new ProcessadorDeID(codesCaseOfUse[i]);
                    compilador.Compilar();


                    ProgramaEmVM program = new ProgramaEmVM(compilador.GetInstrucoes());
                    program.Run(compilador.escopo);

                    try
                    {
                        assercao.IsTrue(ValidateValueObjects(compilador.escopo, "y", values_expected[i]));
                    }
                    catch (Exception ex)
                    {
                        string codeError = ex.Message;
                        assercao.IsTrue(false, "TESTE FALHOU");
                    }
                }
            }




    


            public void TesteInstrucaoIf(AssercaoSuiteClasse assercao)
            {

                string code_0_0 = "int x=1; if (x<2){x=5;}else{x=6;}";
                string code_0_1 = "int x=5; if (x>6){x=-1;} else {x=7;}";
                string code_0_2 = "int x=-1; if (x<-1){x=3;}else{x=5;}";
                string code_0_3 = "int x=5; if (x<6){x=-1;} else {x=2;}";

                string result_0_0 = "5";
                string result_0_1 = "7";
                string result_0_2 = "5";
                string result_0_3 = "-1";


                List<string> codes = new List<string>() { code_0_2, code_0_0, code_0_1, code_0_3 };
                List<string> result_expected = new List<string>() { result_0_2, result_0_0, result_0_1, result_0_3 };

                for (int x = 0; x < codes.Count; x++)
                {
                    ProcessadorDeID compilador = new ProcessadorDeID(codes[x]);
                    compilador.Compilar();

                    ProgramaEmVM program = new ProgramaEmVM(compilador.GetInstrucoes());
                    program.Run(compilador.escopo);


                    try
                    {
                        assercao.IsTrue(ValidateValueObjects(compilador.escopo, "x", result_expected[x]));
                    }
                    catch (Exception ex)
                    {
                        assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                    }
                }
            }

            public void TesteInstrucaoWhile(AssercaoSuiteClasse assercao)
            {
           
                string code_0_0 = "int x=3; while (x<4){x=x+1;}";
                string code_0_1 = "int x=3; while (x>4){x=x+1;}";
                string code_0_2 = "int x=5; while (x<=6){x=x+5;}";
                string code_0_3 = "int x=6; while (x>4){x=x-1;}";

                string result_0_0 = "4";
                string result_0_1 = "3";
                string result_0_2 = "10";
                string result_0_3 = "4";


                List<string> codes = new List<string>() { code_0_1, code_0_0, code_0_2, code_0_3 };
                List<string> resultExpected = new List<string>() { result_0_1, result_0_0, result_0_2, result_0_3 };


                for (int i = 0; i < codes.Count; i++)
                {
                    ProcessadorDeID compilador = new ProcessadorDeID(codes[i]);
                    compilador.Compilar();

                    ProgramaEmVM program = new ProgramaEmVM(compilador.GetInstrucoes());
                    program.Run(compilador.escopo);
                    try
                    {
                        assercao.IsTrue(ValidateValueObjects(compilador.escopo, "x", resultExpected[i]), codes[i]);

                    }
                    catch (Exception ex)
                    {
                        assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                    }

                }



            }

    

            public void TestesUnitariosFor(AssercaoSuiteClasse assercao)
            {


                string code_0_2 = "int x=-2; for (int i = 0; i< 5 ; i++){ x=x+1;}";
                string code_0_0 = "int x=1; for (int i = 0; i< -5 ; i++){ x=x+1;}";
                string code_0_1 = "int x=0; for (int i = 0; i< 5 ; i++){ x=x+1;}";


                string valueExpected_0_2 = "3";
                string valueExpected_0_0 = "1";
                string valueExpected_0_1 = "5";
                
                List<string> codesFor = new List<string>() { code_0_2, code_0_1, code_0_0 };
                List<string> valuesExpected = new List<string> { valueExpected_0_2, valueExpected_0_1, valueExpected_0_0 };

                for (int i = 0; i < codesFor.Count; i++)
                {
                    Expressao.headers = null;
                    ProcessadorDeID compilador= new ProcessadorDeID(codesFor[i]);
                    compilador.Compilar();




                    ProgramaEmVM program = new ProgramaEmVM(compilador.GetInstrucoes());
                    program.Run(compilador.escopo);

                    try
                    {
                        assercao.IsTrue(ValidateValueObjects(compilador.escopo, "x", valuesExpected[i]), codesFor[i]);
                    }
                    catch (Exception e)
                    {
                        assercao.IsTrue(false, "TESTE FALHOU: " + e.Message);
                    }
                }
            }



            /// <summary>
            /// funcao de validacao, tendo como verificacao de uma variavel que é modificada no cenario de teste.
            /// </summary>
            /// <param name="escopo">contexto onde a variavel esta.</param>
            /// <param name="nameObject">nome da variavel.</param>
            /// <param name="valueExpected">valor esperado</param>
            /// <returns>[true] se o valor esperado é o mesmo valor da variavel</returns>
            private bool ValidateValueObjects(Escopo escopo, string nameObject, string nomePropriedade, string valueExpected)
            {
                Objeto obj = escopo.tabela.GetObjeto(nameObject, escopo);

                return obj.GetField(nomePropriedade).valor.ToString() == valueExpected;
            }


            /// <summary>
            /// funcao de validacao, tendo como verificacao de uma variavel que é modificada no cenario de teste.
            /// </summary>
            /// <param name="escopo">contexto onde a variavel esta.</param>
            /// <param name="nameObject">nome da variavel.</param>
            /// <param name="valueExpected">valor esperado</param>
            /// <returns>[true] se o valor esperado é o mesmo valor da variavel</returns>
            private bool ValidateValueObjects(Escopo escopo, string nameObject, string valueExpected)
            {
                return escopo.tabela.GetObjeto(nameObject, escopo).valor.ToString() == valueExpected;
            }


  

    

            public void TesteInstrucaoChamadaDeMetodo(AssercaoSuiteClasse assercao)
            {

                Expressao.headers = null;

                // le o arquivo contendo o codigo para o teste.
                ParserAFile parser = new ParserAFile("helloWorldClass.txt");


                ProcessadorDeID compilador = new ProcessadorDeID(parser.GetTokens());
                compilador.Compilar();

                try
                {

                    assercao.IsTrue(RepositorioDeClassesOO.Instance().GetClasse("classeA").GetMetodos()[1].instrucoesFuncao.Count == 1, Utils.OneLineTokens(parser.GetTokens()));

                }
                catch(Exception ex)
                {
                    string str_codigo = ex.Message;
                    assercao.IsTrue(false, "Teste falhou");
                }



                // execucao de um programa compilado!
                try
                {
                    ProgramaEmVM programa = new ProgramaEmVM(compilador.GetInstrucoes());
                    programa.Run(compilador.escopo);
                    assercao.IsTrue(true, "chamada de metodo feito sem erros fatais");

                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }
                // teste nao automatizado.
                assercao.IsTrue(true);


            }


    
      
            public void TesteInstrucaoContrutorUP(AssercaoSuiteClasse assercao)
            {
                Expressao.headers = null;

                string codigo1 = "public class classeA { public classeA(int x, int y){int z=1;}}";
                string codigo2 = "public class classeB: + classeA { public classeB(){ int b=1; } }";
                string codigoInstrucao = "classeB.construtorUP(classeA, 1, 5);";

                ProcessadorDeID compilador = new ProcessadorDeID(codigo1 + " " + codigo2 + " " + codigoInstrucao);

                compilador.Compilar();


                try
                {
                    assercao.IsTrue(
                    RepositorioDeClassesOO.Instance().GetClasse("classeA") != null &&
                    RepositorioDeClassesOO.Instance().GetClasse("classeB") != null &&
                    compilador.GetInstrucoes()[0].code == ProgramaEmVM.codeConstructorUp, codigo1+"  "+codigo2);

                }
                catch(Exception ex)
                {
                    assercao.IsTrue(false, "teste falhou: " + ex.Message);
                }




                ProgramaEmVM programa = new ProgramaEmVM(compilador.GetInstrucoes());
                programa.Run(compilador.escopo);

                // teste executado sem erros fatais.
                assercao.IsTrue(true);
            }

            public void TesteDefinicaoDeMetodoEVerificacaoDeConstrucaoDoEscopoDoMetodo(AssercaoSuiteClasse assercao)
            {
                string codigo = "public class classeB { public classeB(){int x= 1;};  public int metodoB(int x, int y){ int z=1;  return x+y;} }";

                Expressao.headers = null;

                ProcessadorDeID compilador = new ProcessadorDeID(codigo);
                compilador.Compilar();

                try
                {
                    assercao.IsTrue(
                        RepositorioDeClassesOO.Instance().GetClasse("classeB").GetMetodo("metodoB")[0].escopo.tabela.GetObjeto(
                        "z", RepositorioDeClassesOO.Instance().GetClasse("classeB").GetMetodo("metodoB")[0].escopo) != null, codigo);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: "+ex.Message);
                }

            }



  
            public void TesteInstrucaoImporter(AssercaoSuiteClasse assercao)
            {
                // verifica se as classes base importadas foram realmente importadas, durante a compilacao.
                // numero de classes importada diminuida, devido melhor escopo da linguagem.
                string codigo = "public class classeB { public classeB(){int x= 1;};  public int metodoB(int x, int y){ int z=1;  return x+y;} }; importer (ParserLinguagemOrquidea.exe);";

                Expressao.headers = null;

                ProcessadorDeID compilador = new ProcessadorDeID(codigo);
                compilador.Compilar();

                try
                {
                    assercao.IsTrue(RepositorioDeClassesOO.Instance().GetClasses().Count > 8, codigo);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "teste falhou: "+ex.Message);
                }

            }

    
  

           

  

        } // class Testes

        
        
    } // class Instrucao


 
} //  namespace paser

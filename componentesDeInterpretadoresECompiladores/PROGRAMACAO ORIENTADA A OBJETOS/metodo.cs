using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using parser.ProgramacaoOrentadaAObjetos;
using System.Reflection;
using Modulos;
using Wrappers.DataStructures;

namespace parser
{

    public class Metodo: Objeto
    {
        /// <summary>
        /// acessor: public, protected, private.
        /// </summary>
        public string acessor;


        /// <summary>
        /// nome do metodo.
        /// </summary>
        public new string nome;


        /// <summary>
        /// nome longo do metodo (classe.nomeMetodo). para diferenciar em caso de metodos herdados de
        /// mesmo nome, possibilitando chamar tanto 1 quanto o outro, utilizando o metodo longo.
        /// </summary>
        public string nomeLongo;

        /// <summary>
        /// nome da classe do metodo.
        /// </summary>
        public string nomeClasse;
        


        public bool isCompiled = false;


        /// <summary>
        /// prioridade de execução em expressões.
        /// </summary>
        public int prioridade;


        public new string tipo;

        /// <summary>
        /// id do scopo, dentro do escopo da familia.
        /// </summary>
        public int idEscopo = -1;

        /// <summary>
        /// parâmetros do metodo.
        /// </summary>
        public Objeto[] parametrosDaFuncao;
        


        /// <summary>
        /// nome do tipo do objeto de retorno da função.
        /// </summary>
        public string tipoReturn;

        /// <summary>
        /// se true, o objeto caller (que chamou o metodo) é incluido na lista de parâmetros.
        /// </summary>
        public bool isToIncludeCallerIntoParameters = false;




        /// <summary>
        /// tokens das instrucoes do metodo, se for um metodo da linguagem orquidea, nao metodo importado.
        /// </summary>
        public List<string> tokens = new List<string>();



        /// <summary>
        /// retorna true se o metodo é importado.
        /// </summary>
        public bool isMethodImported
        {
            get
            {
                return InfoMethod != null;
            }
        }

        /// <summary>
        /// o indice deste metodo perante aos outros metodos da classe.
        /// </summary>
        public int indexInClass;





        //  PROTOTIPOS DE FUNÇÕES IMPLEMENTADORAS.
        
        // prototipo para funções genérica.
        public delegate object FuncaoGeral(params object[] parametros);


        // prototipo de execução da função, implementada na liguagem utilizada na construção do compilador;
        public delegate void CallFunction();






        /// <summary>
        /// metodo importado da linguagem base.
        /// </summary>
        public MethodInfo InfoMethod { get; set; }


        /// <summary>
        /// construtores da classe do metodo.
        /// </summary>
        public ConstructorInfo InfoConstructor;



        /// <summary>
        /// contém as instruções de código que compõe a função.
        /// </summary>
        public List<Instrucao> instrucoesFuncao;
        /// <summary>
        /// guarda que está chamando a execução da função.
        /// </summary>
        public object caller = null; 

        /// <summary>
        /// escopo interno do metodo, contendo variaveis, objetos e funcoes dentro do corpo do metodo.
        /// </summary>
        public Escopo escopo;



        /// <summary>
        /// adiciona um parametro na lista de parãmetros.
        /// util quando o parametro for um metodo, com tipo de retorno, classe, e parametros definidos.
        /// </summary>
        /// <param name="parameter">parametro a ser adicionado.</param>
        public void AddParameter(Objeto parameter)
        {
            if (this.parametrosDaFuncao == null)
            {
                this.parametrosDaFuncao = new Objeto[1];

            }
            List<Objeto> paramsList= this.parametrosDaFuncao.ToList();
            paramsList.RemoveAt(0);
            paramsList.Add(parameter);
            this.parametrosDaFuncao= paramsList.ToArray();
        }


        public new void SetNomeLongo(string classeDoMetodo)
        {
            // muda o nome do metodo para o nome longo (nomeDaClasseDoMetodo+"."+ nomeDoMetodo).
            this.nomeLongo = classeDoMetodo + "@" + nome;
            this.nome = this.nomeLongo;
        }


        /// <summary>
        ///  avalia uma função, tendo como parâmetros expressões, 
        ///  muito apreciada nas chamadas de função, que utiliza expressões como parâmetros.
        ///  o objeto "caller" é utilizado para funções de objetos importados.
        ///  o escopo não é copiado, porque:
        ///     1- para chamadas de função da classe base, a execução é imediata, não necessitando escopos.
        ///     2- para chamadas de função da linguagem orquidea, é feita por chamadas de metodo, que faz uma copia do escopo
        ///     da classe da função. Como todas chamadas de função são feitas como chamadas de método, a regra é preservado.
        /// </summary>
        public object ExecuteAFunction(List<Expressao> parametros, object caller, Escopo escopoDaFuncao)
        {

            if (this.InfoMethod == null)
            {

                return ExecuteAMethod(parametros, escopoDaFuncao, (Objeto)caller);
            }
            

            List<object> valoresParametro = new List<object>();
            EvalExpression eval = new EvalExpression();

            for (int x = 0; x < parametros.Count; x++)
            {
                // possivel inclusao do escopo como um dos parametros.
                if (parametrosDaFuncao[x].GetTipo()=="Escopo")
                {
                    valoresParametro.Add(escopoDaFuncao);
                }
                else
                {


                    parametros[x].isModify = true;
                    object umValor = eval.EvalPosOrdem(parametros[x], escopoDaFuncao);

                   

                    if ((umValor != null) && (umValor.GetType() == typeof(Objeto)))
                    {
                        valoresParametro.Add(((Objeto)umValor).GetValor());
                    }
                    else
                    {
                        valoresParametro.Add(umValor);
                    }
                        
                  
                        
                }
            } // for x

            // constroi a lista de parametros object, para UMA FUNCAO IMPORTADA.
            List<object> parametrosValoresAtuais = new List<object>();
            if (this.parametrosDaFuncao != null)
            {
                int parametrosDaChamada = 0;
                for (int x = 0; x < parametrosDaFuncao.Length;  x++, parametrosDaChamada++)
                {
                    // processamento de multi-argumentos.
                    if (parametrosDaFuncao[x].isMultArgument)
                    {
                        
                        string tipoDoMultiArgumento = parametrosDaFuncao[x].GetTipo();
                        List<object> paramsMult = new List<object>();
                        int indexBegin = parametrosDaChamada;
                        
                        // armazena os valores dos multi-argumentos numa lista-array
                        while ((indexBegin<parametros.Count) && (parametros[indexBegin].tipoDaExpressao == tipoDoMultiArgumento))
                        {
                            // calculo do valor da expressao parametro, afim de adicionar este valor numa lista contendo objetos multi-argumento.
                            object valorParam = eval.EvalPosOrdem(parametros[indexBegin], escopo);

                            paramsMult.Add(valorParam);
                            indexBegin++;
                        }

                        
                        // construcao de um vetor como container dos parametros multi-argumento.
                        Wrappers.DataStructures.Vector arrayParametrosMultiArgumentos = new Wrappers.DataStructures.Vector(tipoDoMultiArgumento);
                        arrayParametrosMultiArgumentos.SetNome(parametrosDaFuncao[x].GetNome());
                        for (int p = 0; p < paramsMult.Count; p++)
                        {
                            arrayParametrosMultiArgumentos.pushBack(paramsMult[p]);
                        }

                        // remove o parametro curretne, a fim de inserção do array de parametros multi-argumento. é provável que o parametro currente seja um vetor também.
                        parametrosDaFuncao.ToList<Objeto>().RemoveAt(x);
                        parametrosDaFuncao.ToList<Objeto>().Insert(x, arrayParametrosMultiArgumentos);


                        // adiciona o parametro para o escopo do metodo.
                        escopoDaFuncao.tabela.GetObjetos().Add(parametrosDaFuncao[x]);
                        // adiciona oo parametro para a lista de valores atuais.
                        parametrosValoresAtuais.Add(parametrosDaFuncao[x]);


                        // atualiza o indice de malha de parametros da chamada de metodo.
                        parametrosDaChamada += paramsMult.Count;


                        // se todos parametros da chamada foram processados, para a malha.
                        if (parametrosDaChamada >= parametros.Count)
                        {
                            break;
                        }
                    }
                    else
                    {   // caso em que não é um parametro multi-argumento.
                        parametrosDaFuncao[x].SetValor(valoresParametro[x]);
                        escopoDaFuncao.tabela.GetObjetos().Add(parametrosDaFuncao[x]);

                        parametrosValoresAtuais.Add(parametrosDaFuncao[x]);

                    }

                }

            }
                

            object resultCalcFuncao = null;
            // EXECUTA UMA FUNCAO IMPORTADA.
            if ((this.InfoMethod != null) && (caller != null))

            {
                resultCalcFuncao = this.InfoMethod.Invoke(caller, valoresParametro.ToArray());
            }
                




            return resultCalcFuncao;
        }


        /// <summary>
        /// Modos de execucao da função:
        ///    1- via instruções da linguagem orquidea.
        ///         é criado uma copia do escopo da classe, na chamada de método. como só há chamadas de métodos,
        ///         o escopo é sempre clonado, evitando chamadas intercaladas do método, resultando em erros de instanciação
        ///         dos objets do escopo.
        ///    2- via metodo via reflexao (é preciso setar o objeto que fará a chamada do método).
        /// </summary>
        public object ExecuteAFunction(List<object> valoresDosParametros, Escopo escopo)
        {

            // avaliação da função via instruções da linguagem orquidea.
            if ((this.instrucoesFuncao != null) && (this.instrucoesFuncao.Count > 0))
            {
                // cria uma instancia de processamento de codigo, com as instruções da função.
                ProgramaEmVM program = new ProgramaEmVM(this.instrucoesFuncao);
                program.Run(escopo);


                object result = program.lastReturn;

                return result;
            } // if 
            else
            // avaliação de função via método importado com API Reflexão.
            if (this.InfoMethod != null)
            {



                object result1 = new object();
                Type classObjectCaller = this.InfoMethod.DeclaringType;
                ConstructorInfo[] construtores = classObjectCaller.GetConstructors();
                if ((construtores.Length > 0) && (!(caller is Objeto)))
                {
                    // obtem um objeto da classe que construiu o operador.
                    this.caller = classObjectCaller.GetConstructor(new Type[] { }).Invoke(null);
                }
                if ((valoresDosParametros != null) && (valoresDosParametros.Count > 0))
                {

                    result1 = this.InfoMethod.Invoke(this.caller, valoresDosParametros.ToArray());
                }
                else
                {
                    result1 = this.InfoMethod.Invoke(this.caller, null);
                }

                return result1;
            }
            else
                return null;

        } // ExecuteAFunction()


        /// <summary>
        /// obtem o escopo do metodo que esta sendo avaliado.
        /// </summary>
        /// <returns>retorna o escopo do metodo.</returns>
        private Escopo FindEscopoOfMethod()
        {
            Classe classe = RepositorioDeClassesOO.Instance().GetClasse(this.nomeClasse);
            List<Metodo> metodos = classe.GetMetodo(this.nome);
            if (metodos == null)
            {
                return new Escopo("int x;");
            }
            else
            {
                List<Metodo> metodosMesQtdParametros = new List<Metodo>();
                for (int i = 0; i < metodos.Count; i++)
                {
                    // filtra metodos com quantidade de parametros diferentes.
                    if ((metodos[i]!=null) && (metodos[i].parametrosDaFuncao!=null) && (metodos[i].parametrosDaFuncao.Length==this.parametrosDaFuncao.Length))
                    {
                        metodosMesQtdParametros.Add(metodos[i]);    
                    }
                }
                // se nao haver nenhum metodo, retorna um escopo vazio.
                if (metodosMesQtdParametros.Count == 0)
                {
                    return new Escopo("int x;");
                }
                else
                // se houver 1 unico metodo com quantidades de parametros, validado, retorna o escopo deste metodo.
                if (metodosMesQtdParametros.Count == 1)
                {
                    return metodosMesQtdParametros[0].escopo;
                }
                else
                {
                    // filtra metodos com tipos de parametros nao iguais ao metodo currente.
                    List<Metodo> metodosMesmoTipoDeParametros= new List<Metodo>();  
                    for (int i = 0; i < metodos.Count; i++)
                    {
                        for (int j = 0; j<metodos[i].parametrosDaFuncao.Length; j++)
                        {
                            if (metodos[i].parametrosDaFuncao[j].tipo == this.parametrosDaFuncao[j].tipo)
                            {
                                metodosMesmoTipoDeParametros.Add(metodos[i]);
                            }
                        }
                    }
                    // se nao houver nenhum metodo que atenda os tipos de parametros, retorna um escopo vazio.
                    if (metodosMesmoTipoDeParametros.Count==0)
                    {
                        return new Escopo("int x;");
                    }
                    else
                    {   // retorna o escopo do metodo que atendeu todos requisitos de nome, quantidade e mesmo tipos de parametros.
                        return metodosMesmoTipoDeParametros[0].escopo;
                    }
                }
                    
            }
        }




        /// <summary>
        /// executa um metodo, com parametros e objeto caller que invoca a expressao chamada de metodo.
        /// </summary>
        /// <param name="paramsChamadaDeMetodo">parametros vindos da chamada de metodo, 
        /// ex.: a.metodoB(1,5), 1, e 5 são parametros da chamada de metodo metodB..</param>
        /// <param name="escopoCurrente">contexto onde os parametros expressao estão.</param>
        /// <param name="objetoCaller">objeto que fez a chamada de metodo, ex.: a.metodoA(1), "a" é o objeto caller.</param>
        /// <returns></returns>
        public object ExecuteAMethod(List<Expressao> paramsChamadaDeMetodo, Escopo escopoCurrente,  Objeto objetoCaller)
        {
            if ((this.instrucoesFuncao == null) || (this.instrucoesFuncao.Count == 0) || (this.escopo == null) ||
                (this.escopo.tabela.objetos == null) || (this.escopo.tabela.objetos.Count == 0))
            {
                
                Metodo fnc1 = UtilTokens.FindMethodCompatible(objetoCaller, this.nomeClasse, this.nome, this.nomeClasse, paramsChamadaDeMetodo, escopoCurrente, this.isStatic, this.isToIncludeCallerIntoParameters);
                // ajuste no 1o. parametro como o objeto caller.
                if (isToIncludeCallerIntoParameters)
                {
                    paramsChamadaDeMetodo.RemoveAt(0);
                }
                if (fnc1 != null)
                {
                    this.instrucoesFuncao = fnc1.instrucoesFuncao;
                    this.escopo = fnc1.escopo.Clone();

                }


            }


            Classe classeDoObjeto = RepositorioDeClassesOO.Instance().GetClasse(objetoCaller.GetTipo());

            // pode acontecer.. não havendo instrucoes da linguagem, nao há escopo do corpo do metodo.
            if (this.escopo == null)
            {
                // codigo qualquer, nao entra no processamento.
                this.escopo = new Escopo("");
            }

            // encontra o escopo do metodo, dentro da classe do metodo.
            Escopo escopoDoMetodo = this.escopo.Clone();
            if (escopoDoMetodo == null)
            {
                // pode nao haver escopo, caso O METODO SEJA IMPORTADO, NAO CONTENDO ESCOPO DE SEU CORPO..
                escopoDoMetodo = new Escopo("");
            }


            // contem os objetos parametros.
            // valoes dos parametros, calculado após a avaliação das expressões parâmetros.
            List<object> valoresDosParametros = new List<object>();


          
            // CONSTROI A LISTA DE PARAMETROS DA FUNÇÃO.
            if ((parametrosDaFuncao != null) && (parametrosDaFuncao.Length > 0))
            {
                EvalExpression evalP = new EvalExpression();
                int indexParamsCHAMADA_Metodo = 0;
                for (int i = 0; i < parametrosDaFuncao.Length; i++)
                {
                    // PROCESSAMENTO DE OBJETO CALLER INCLUIDO NA LISTA DE PARAMETOS.                   
                    if ((i == 0) && (isToIncludeCallerIntoParameters))
                    {
                        paramsChamadaDeMetodo.Insert(0, new ExpressaoObjeto(objetoCaller));
                    }

                    // PROCESSAMENTO DE PARAMETROS MULTI-ARGUMENTOS.
                    string tipoParametroMultiArgumento = parametrosDaFuncao[i].tipoElemento;
                    List<Expressao> listaVetorMultiArgument = new List<Expressao>();
                    int indexParamsBegin = i;
                    if ((parametrosDaFuncao[i] != null) && (parametrosDaFuncao[indexParamsCHAMADA_Metodo].isMultArgument))
                    {
                        int indexParams = i;
                        while ((indexParams < paramsChamadaDeMetodo.Count) &&
                            ((paramsChamadaDeMetodo[indexParams].tipoDaExpressao == tipoParametroMultiArgumento) ||
                            (parametrosDaFuncao[i].tipo == "Object")))
                        {
                            listaVetorMultiArgument.Add(paramsChamadaDeMetodo[indexParams]);
                            indexParams++;
                            indexParamsCHAMADA_Metodo++;
                        }

                        // instancia o vetor parametro, contendo os parametros multi-argumento.
                        Vector vetorMultiArgument = new Vector(tipoParametroMultiArgumento);

                        // obtem os valores de cada elemento do vetor multi-argumento.
                        for (int p = 0; p < listaVetorMultiArgument.Count; p++)
                        {
                            object objElemento = evalP.EvalPosOrdem(listaVetorMultiArgument[p], escopo);
                            vetorMultiArgument.insert(p, objElemento);
                        }
                        //atualiza a lista de expressoes parametros.
                        paramsChamadaDeMetodo.RemoveRange(indexParamsBegin, listaVetorMultiArgument.Count);
                        // atualiza a malha de expressoes parametros.
                        indexParamsCHAMADA_Metodo -= listaVetorMultiArgument.Count;

                        // seta o i-esimo parametro do método, como um vector, que contem os elementos multi-argumentos.
                        parametrosDaFuncao[i] = vetorMultiArgument;
                        // adiciona o vector construido, no escopo do metodo, funcionando como uma variavel, objeto, dentro do corpo do metodo.
                        escopoDoMetodo.tabela.GetObjetos().Add(parametrosDaFuncao[i]);
                        if (indexParamsCHAMADA_Metodo <= 0)
                        {
                            break;
                        }
                    }
                    else
                    // PROCESSAMENTO DE PARAMETROS-METODO.
                    if (paramsChamadaDeMetodo[indexParamsCHAMADA_Metodo].GetType() == typeof(ExpressaoChamadaDeMetodo)) 
                    {
                        ExpressaoChamadaDeMetodo exprssChamada = (ExpressaoChamadaDeMetodo)paramsChamadaDeMetodo[indexParamsCHAMADA_Metodo];
                        if (exprssChamada.isMethodParameter)
                        {
                            Metodo metodoParametro = exprssChamada.funcao;
                            escopoDoMetodo.tabela.RegistraFuncao(metodoParametro);
                            indexParamsCHAMADA_Metodo++;
                        }
                        else
                        {
                            // PROCESSAMENTO NORMAL DE UMA EXPRESSAO CHAMADA DE METODO: pode não ser uma expressao que retorne um valor, e sim
                            // gere alguma saida, como escrever na tela algo.
                            object valorParametroChamada = evalP.EvalPosOrdem(paramsChamadaDeMetodo[indexParamsCHAMADA_Metodo], escopoCurrente);
                            valoresDosParametros.Add(valorParametroChamada);
                            indexParamsCHAMADA_Metodo++;

                            this.parametrosDaFuncao[i].SetValor(valorParametroChamada);
                            valoresDosParametros.Add(valorParametroChamada);

                            // adiciona o i-ésimo parametro da funcao, no escopo do metodo.
                            escopoDoMetodo.tabela.GetObjetos().Add(parametrosDaFuncao[i]);


                        }

                    }
                    else
                    // PROCESSAMENTO DE PARAMETROS NORMAIS.
                    {   
          

                        // avalia a expressao parametro.
                        object valorParametro = evalP.EvalPosOrdem(paramsChamadaDeMetodo[indexParamsCHAMADA_Metodo], escopoCurrente);

                        // carrega o valor do parametro pelo valor do objeto, se o parametro for um Objeto
                        if ((valorParametro != null) && valorParametro.GetType() == typeof(Objeto))
                        {
                            Objeto objParametro = (Objeto)valorParametro;
                            valorParametro = objParametro.valor;
                        }
                                          

                        // atualiza o indice de expressoes-parametros.
                        indexParamsCHAMADA_Metodo++;


                        this.parametrosDaFuncao[i].SetValor(valorParametro);
                        valoresDosParametros.Add(valorParametro);

                        if (escopoDoMetodo.tabela.GetObjeto(parametrosDaFuncao[i].nome, escopoDoMetodo) != null)
                        {
                            escopoDoMetodo.tabela.GetObjeto(parametrosDaFuncao[i].nome, escopoDoMetodo).valor = valorParametro;
                        }
                        else
                        {
                            // adiciona o i-ésimo parametro da funcao, no escopo do metodo.
                            escopoDoMetodo.tabela.GetObjetos().Add(parametrosDaFuncao[i]);

                        }

                    }

                }
            }

            

            // copia os objetos do escopo currente, para dentro do escopo do método.
            if (escopoCurrente.tabela.GetObjetos() != null)
            {
                for (int i = 0; i < escopoCurrente.tabela.objetos.Count; i++)
                {
                    escopoDoMetodo.tabela.GetObjetos().Add(escopoCurrente.tabela.GetObjetos()[i].Clone());
                }
                
            }
                




            
            // adiciona o objeto "actual", em outras linguagens, é o objeto "this".
            if (escopoDoMetodo.tabela.GetObjeto("actual", escopoDoMetodo) == null)
            {
                // o objeto "actual" é uma referência ao objeto que chamou o metodo. é útil para chamada à construtores de classes herdadas
                Objeto actual = new Objeto(objetoCaller);
                // seta o nome do objeto "actual".
                actual.SetNome("actual");

                escopoDoMetodo.tabela.GetObjetos().Add(actual);
            }





            // copia os dados do objeto, para dentro do escopo do metodo..
            if (objetoCaller.GetFields() != null)
            {
                for (int x = 0; x < objetoCaller.GetFields().Count; x++)
                {
                    object valorCampo = objetoCaller.GetFields()[x].GetValor();
                    if (valorCampo == null)
                    {
                        valorCampo = new object();
                    }
                    Objeto objAField = escopoDoMetodo.tabela.GetObjeto(objetoCaller.GetFields()[x].GetNome(),escopoCurrente);
                    
                    
                    

                    if (objAField != null)
                    {
                        objAField.valor = valorCampo;
                    
                    }
                    else
                    {
                        objAField = new Objeto();
                        objAField.SetNome(objetoCaller.GetFields()[x].GetNome());
                        objAField.valor = valorCampo;
                    }

                    // registra os campos do objeto caller, no escopo do metodo.
                    escopoDoMetodo.tabela.RegistraObjeto(objAField);

                 
                }
            }

            // SETA O OBJETO QUE INVOCARÁ A EXECUÇÃO DA FUNÇÃO, EM CASOS DE METODOS IMPORTADOS DA LINGUAGEM BASE.
            this.caller = objetoCaller.valor;
                       
            //************************************************************************************************
            // faz a chamada do metodo, com parametros, escopo, retornando um object.
            object objetoValor = ExecuteAFunction(valoresDosParametros, escopoDoMetodo);
            //***********************************************************************************************


            // remove o objeto [actual].
            escopoDoMetodo.tabela.RemoveObjeto("actual");
            
            if (escopoDoMetodo.tabela.GetObjetos() != null)
            {

                // atualiza os valores de objetos do escopo currente, pois a função pode ter modificado seus valores,
                // e precisam ser repassados para o escopo acima do escopo da função.
                for (int i = 0; i < escopoDoMetodo.tabela.GetObjetos().Count; i++)
                {
                    string nomeObjeto = escopoDoMetodo.tabela.GetObjetos()[i].nome;
                    int index = escopoCurrente.tabela.GetObjetos().FindIndex(k => k.nome == nomeObjeto);
                    if (index >= 0)
                    {
                        if (escopoCurrente.tabela.GetObjeto(nomeObjeto, escopoCurrente) != null)
                        {

                           
                            Objeto obj = escopoDoMetodo.tabela.GetObjeto(nomeObjeto, escopoDoMetodo).Clone();

                            escopoCurrente.tabela.RemoveObjeto(nomeObjeto);
                            escopoCurrente.tabela.RegistraObjeto(obj);
                        }
                        
                    }
                    
                }
            }
            


            // repassa valores vindos da execução do metodo. as variaveis estaticas.
            if (classeDoObjeto.propriedadesEstaticas != null)
            {
                
                List<Objeto> propEstaticas = RepositorioDeClassesOO.Instance().GetClasse(objetoCaller.GetTipo()).propriedadesEstaticas;
                for (int x = 0; x < classeDoObjeto.propriedadesEstaticas.Count; x++)
                {
                    Objeto ObjEstatico = escopoDoMetodo.tabela.GetObjeto(classeDoObjeto.propriedadesEstaticas[x].GetNome(), escopoDoMetodo);
                    if (ObjEstatico != null)
                        classeDoObjeto.propriedadesEstaticas[x].SetValor(ObjEstatico.GetValor());
                }

            }

            // repassa o valor modificados no escopo do método, para as propriedades do objeto caller.
            if (objetoCaller.GetFields() != null) 
                for (int x = 0; x < objetoCaller.GetFields().Count; x++)
                {
                    Objeto propriedadeModificada = escopoDoMetodo.tabela.GetObjetos().Find(k => k.GetNome() == objetoCaller.GetFields()[x].GetNome());
                    if (propriedadeModificada != null)
                        objetoCaller.SetField(propriedadeModificada);

                }
            return objetoValor; 
        }
     
        
    

 
        /// <summary>
        /// executa um construtor de uma classe orquidea.
        /// </summary>
        /// <param name="parametros">lista de parametros.</param>
        /// <param name="nomeClasse">nome da classe do construtor.</param>
        /// <param name="escopoFuncao">escopo externo ao escopo da funcao construtor.</param>
        /// <param name="indexConstrutor">indice do metodo construtor, dentre a lista de construtores.</param>
        /// <returns></returns>
        public object ExecuteAConstructor(Objeto objCaller, List<Expressao> parametros, string nomeClasse, Escopo escopoFuncao, int indexConstrutor)
        {

            // executa a funcao do construtor, com lista de parametros, e escopo.
            object result = RepositorioDeClassesOO.Instance().GetClasse(nomeClasse).construtores[indexConstrutor].ExecuteAMethod(parametros, escopoFuncao, objCaller);
            return result;

        }

  
        public Metodo()
        {
            this.escopo = null;
            this.acessor = "protected"; // valor default para o acessor da função.
            this.nome = "";
            this.tipoReturn = null;

            this.prioridade = 300;  // seta a prioridade da função em avaliação de expressões. A regra de negócio é que a função sempre tem prioridade sobre os operadores.
   
            this.instrucoesFuncao = new List<Instrucao>();
        } //Funcao()

        /// <summary>
        /// seta propriedades particulares de metodos de certo tipo de classes.
        /// </summary>
        /// <param name="isExpressionEstatic">a expressao que contem o metodo é uma chamada estatica.</param>
        public void SetAtributesMethod(bool isExpressionEstatic)
        {
            if ((this.nomeClasse == "double") || (this.nomeClasse == "string"))
            {
                this.isStatic = isExpressionEstatic;
                this.isToIncludeCallerIntoParameters = true;
            }
            else
            {
                this.isStatic = false;
                this.isToIncludeCallerIntoParameters = false;
            }
            
            
        }
        public new Metodo Clone()
        {
            Metodo fncClone = new Metodo(this.nomeClasse, this.acessor, this.nome, this.InfoMethod, this.tipoReturn, this.parametrosDaFuncao);
            fncClone.isStatic = this.isStatic;
            fncClone.isToIncludeCallerIntoParameters= this.isToIncludeCallerIntoParameters;
            fncClone.isWrapperObject = this.isWrapperObject;
            fncClone.isCompiled = this.isCompiled;

            if (this.escopo != null)
            {
                fncClone.escopo = this.escopo.Clone();
            }
            else
            {
                fncClone.escopo = new Escopo("a=1;");
            }
            if (this.instrucoesFuncao != null)
            {
                fncClone.instrucoesFuncao = this.instrucoesFuncao.ToList<Instrucao>();
            }
            else
            {
                fncClone.instrucoesFuncao = new List<Instrucao>();
            }
                
            if (this.InfoConstructor != null)
            {
                fncClone.InfoConstructor = this.InfoConstructor;
            }
                

            return fncClone;
        }
        public Metodo(string acessor, string nome, FuncaoGeral fncImplementa, string tipoRetorno, params Objeto[] parametrosMetodo)
        {
            this.escopo = null;
            this.InfoMethod = null;
            this.InfoConstructor = null;
            this.acessor = acessor;
            if (acessor == null)
                this.acessor = "protected";
            this.nome = nome;
            this.tipoReturn = tipoRetorno;
            if (parametrosMetodo != null)
                this.parametrosDaFuncao = parametrosMetodo.ToArray<Objeto>();
            this.instrucoesFuncao = new List<Instrucao>();
        }


        public Metodo(string classe, string acessor, string nome, Objeto[] parametrosMetodo, string tipoRetorno, List<Instrucao> instrucoesCorpo, Escopo escopoDaFuncao)
        {
            
            this.InfoMethod = null;
            this.InfoConstructor = null;
            if (acessor == null)
                acessor = "protected"; // se nao tiver acessor, é uma função estruturada, seta o acessor para protected.
            else
                this.acessor = acessor; // acessor da função.
            this.nome = nome; // nome da função.
            this.parametrosDaFuncao = new Objeto[parametrosMetodo.Length]; // inicializa a lista de parâmetros da função.

            if ((parametrosMetodo != null) && (parametrosMetodo.Length > 0)) // obtém uma lista dos parâmetros da função. 
                this.parametrosDaFuncao = parametrosMetodo.ToArray<Objeto>();


            this.instrucoesFuncao = new List<Instrucao>(); // sem instruções (sem corpo de função).
            this.tipoReturn = tipoRetorno; // tipo do retorno da função.


            this.escopo = escopoDaFuncao.Clone();
    
            for (int x = 0; x < this.parametrosDaFuncao.Length; x++)
                escopo.tabela.GetObjetos().Add(new Objeto("private", parametrosDaFuncao[x].GetTipo(), parametrosDaFuncao[x].GetNome(), null, escopo, false));


            if (instrucoesCorpo != null)
                this.instrucoesFuncao = instrucoesCorpo.ToList<Instrucao>();

            this.nomeClasse = classe;
        } // Funcao()


        ///  construtor com método importado via API Reflexao.
        public Metodo(string nomeClasse, string acessor, string nome, MethodInfo metodoImportado, string tipoRetorno, params Objeto[] parametrosMetodo)
        {
            this.escopo = null;
            this.acessor = acessor;
            this.nome = nome;
            this.tipoReturn = tipoRetorno;
            this.parametrosDaFuncao = parametrosMetodo.ToArray<Objeto>();

            this.InfoMethod = metodoImportado;
            this.InfoConstructor = null;
            this.instrucoesFuncao = new List<Instrucao>();
            this.nomeClasse = nomeClasse;
     
            List<Type> tiposDosParametros = new List<Type>();
            List<object> nomesDosParametros = new List<object>();

        }

      
        public Metodo(string nomeClasse, string acessor, string nome, ConstructorInfo construtorImportado, string tipoRetorno, Escopo escopoDaFuncao, params Objeto[] parametrosMetodo)
        {
            this.acessor = acessor;
            this.nome = nome;
            this.InfoMethod = null;
            this.InfoConstructor = construtorImportado;
            this.tipoReturn = tipoRetorno;
            this.nomeClasse = nomeClasse;
            this.instrucoesFuncao = new List<Instrucao>();
            this.parametrosDaFuncao = parametrosMetodo.ToArray<Objeto>();
            if (escopoDaFuncao != null)
                this.escopo = escopoDaFuncao.Clone();
            
        }


        public static bool IguaisFuncoes(Metodo fncA, Metodo fncB)
        {
            if (fncA.nome != fncB.nome)
                return false;

            if ((fncA.parametrosDaFuncao == null) && (fncB.parametrosDaFuncao == null))
                return true;

            if ((fncA.parametrosDaFuncao == null) && (fncB.parametrosDaFuncao != null))
                return false;

            if ((fncA.parametrosDaFuncao != null) && (fncB.parametrosDaFuncao == null))
                return false;

            if (fncA.parametrosDaFuncao.Length != fncB.parametrosDaFuncao.Length)
                return false;

          
            for (int x = 0; x < fncA.parametrosDaFuncao.Length; x++)
                if (fncA.parametrosDaFuncao[x].GetTipo() != fncB.parametrosDaFuncao[x].GetTipo())
                    return false;

            if (fncA.tipoReturn != fncB.tipoReturn)
                return false;

            return true;
        }


        public override string ToString()
        {
            string str = "";
            if ((this.tipoReturn != null) && (this.tipoReturn != ""))
                str += this.tipoReturn.ToString() + "  ";

            if ((this.nome != null) && (this.nome != ""))
                str += this.nome + "( ";
            if ((this.parametrosDaFuncao != null) && (this.parametrosDaFuncao.Length > 0))
            {
                for (int x = 0; x < this.parametrosDaFuncao.Length; x++)
                {
                    str += this.parametrosDaFuncao[x] + " ";
                    if (x < (parametrosDaFuncao.Length - 1))
                        str += ",";
                } // for x
            } // if
            str += ")";
            return str;
        } // ToString()



        public new class Testes : SuiteClasseTestes
        {
            public Testes() : base("testes de chamada de metodos, e funcionalidades classe funcao.")
            {
            }


            public void TesteFindEscopo(AssercaoSuiteClasse assercao)
            {

                Expressao.headers = null;

               string codigoClasseA = "public class classeA { public classeA(int y) { int x=1; }  public int metodoA(int y){ int y= 1; }; };";





                ProcessadorDeID compilador = new ProcessadorDeID(codigoClasseA);
                compilador.Compilar();

                Classe classeA = RepositorioDeClassesOO.Instance().GetClasse("classeA");
                Metodo metodo1 = classeA.GetMetodos()[0];
                Escopo escopoMetodo1 = metodo1.FindEscopoOfMethod();
                try
                {
                    assercao.IsTrue(escopoMetodo1.tabela.GetObjetos().Count == 1, codigoClasseA);
                    assercao.IsTrue(RepositorioDeClassesOO.Instance().GetClasse("classeA").GetMetodos()[0].escopo.tabela.GetObjetos().Count > 0, codigoClasseA);
                }
                catch (Exception ex) 
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }



            }


            /*
            public void TesteChamadaDeMetodoHelloWorld(AssercaoSuiteClasse assercao)
            {

                Escopo escopo = new Escopo(new List<string>());

                Library lib = new Library();
                lib.Import("Prompt");
                List<object> parametrosWrite = new List<object>() { "Hello, World" };



                MethodInfo infoOutput = lib.metodosBibliotecas.Find(k => k.Name == "sWrite");

                // constroi a funcao write string.
                Metodo fncWrite = new Metodo("Prompt", "public", "sWrite", infoOutput, null, new Objeto[] { new Objeto() });


                fncWrite.ExecuteAFunction(parametrosWrite, escopo);



                // automatização inicial do teste: função executada sem erros fatais.
                assercao.IsTrue(true);

                System.Console.ReadLine();
            }
            public void TesteChamadaDeMetodoHelloWorldPelaBiblioteca(AssercaoSuiteClasse assercao)
            {

                Library lib = new Library();
                lib.RunMethod("Prompt", "sWrite", "Hello World");


                // metodo executado sem erros fatais.
                assercao.IsTrue(true);

                System.Console.ReadLine();
            }
            */

        }
    } // class Funcao

    public class Operador : Metodo
    {
        public new int prioridade { get;  set; } // prioridade do operador nas expressões.
        public string tipoRetorno { get; set; } // tipo de retorno da função

        internal int indexPosOrdem = 0; // utilizada para processamento de PosOrdem().


      
    

        // função com instrucoes orquidea.
        public Metodo funcaoImplementadoraDoOperador { get; set; }
    

        public int GetPrioridade()
        {
            return prioridade;
        }
    

        public string GetTipoFuncao()
        {
            return tipo;
        }

        public void SetTipo(string tipoNovo)
        {
            this.tipo = tipoNovo;
        }



        private Random aleatorizador = new Random(1000);

      
        public Operador(string nomeClasse, string nomeOperador, int prioridade, Objeto[] parametros, string tipoOperador, MethodInfo metodoImpl, Escopo escopoDoOperador):base()
        {
            this.nome = nomeOperador;
            this.nomeClasse = nomeClasse;
            if (parametros != null)
                this.parametrosDaFuncao = parametros.ToArray<Objeto>(); // faz uma copia em profundidade nos parametros.
            else
                this.parametrosDaFuncao = new Objeto[0];
            
            this.tipo = tipoOperador;
            this.tipoReturn = UtilTokens.Casting(metodoImpl.ReflectedType.Name.ToLower());
            
            this.InfoMethod = metodoImpl;
            this.caller = new object();



            this.instrucoesFuncao = null;
            this.prioridade = prioridade;
            LinguagemOrquidea.RegistraOperador(this); // adiciona o operador criado, para a lista de operadores da linguagem, atualizando a lista de operadores para processamento.

        }

        public Operador(string nomeClasse, string nomeOperador, int prioridade, string[] tiposParametros, string tipoOperador, Metodo funcaoDeImplementacaoDoOperador, Escopo escopoDoOperador) : base()
        {
            this.nome = nomeOperador;
            this.nomeClasse = nomeClasse;
            this.prioridade = prioridade;


            Objeto[] operandos = new Objeto[2];
            if (tiposParametros[0] != null)
            {
                if (tiposParametros.Length > 0)
                    operandos[0] = new Objeto("A", tiposParametros[0], null, false);

                if (tiposParametros.Length > 1)
                    operandos[1] = new Objeto("B", tiposParametros[1], null, false);

            }

            this.parametrosDaFuncao = operandos;

            this.tipo = tipoOperador;
            if (funcaoDeImplementacaoDoOperador != null)
            {
                this.funcaoImplementadoraDoOperador = funcaoDeImplementacaoDoOperador;
                this.instrucoesFuncao = funcaoDeImplementacaoDoOperador.instrucoesFuncao;

                // adiciona o operador criado, para a lista de operadores da linguagem, atualizando a lista de operadores para processamento.
                LinguagemOrquidea.RegistraOperador(this); 
            }

          
            
            

        } // Operador()

        public Operador(string nomeClase, string nomeOperador, int prioridade, string tipoRetorno, List<Instrucao> instrucoesCorpo, Objeto[] parametros, Escopo escopoDoOperador):base()
        {
            this.nome = nomeOperador;
            this.tipoRetorno = tipoRetorno;
            this.prioridade = prioridade;
            this.nomeClasse = nomeClase;
            this.instrucoesFuncao = instrucoesCorpo.ToList<Instrucao>();
            this.parametrosDaFuncao = parametros;
            LinguagemOrquidea.RegistraOperador(this); // adiciona o operador criado, para a lista de operadores da linguagem, atualizando a lista de operadores para processamento.

        }

        public new Operador Clone()
        {
            Operador operador = new Operador(this.nomeClasse, this.nome, this.prioridade, this.parametrosDaFuncao, this.tipo, this.InfoMethod, this.escopo);
            operador.tipoRetorno = this.tipoRetorno;
            operador.tipoReturn = this.tipoReturn;
            return operador;
        }

        public static bool IguaisOperadores(Operador op1, Operador op2)
        {
            if ((op1.parametrosDaFuncao == null) && (op2.parametrosDaFuncao == null))
                return true;
             
            if ((op1.parametrosDaFuncao == null) && (op2.parametrosDaFuncao != null))
                return false;

            if ((op1.parametrosDaFuncao != null) && (op2.parametrosDaFuncao == null))
                return false;

            if (op1.parametrosDaFuncao.Length != op2.parametrosDaFuncao.Length)
                return false;

            for (int x = 0; x < op1.parametrosDaFuncao.Length; x++)
                if (op1.parametrosDaFuncao[x].GetTipo() != op2.parametrosDaFuncao[x].GetTipo())
                    return false;

            return true;
        }


        public static Operador GetOperador(string nomeOperador, string classeOperador, string tipo, UmaGramaticaComputacional lng)
        {
            Classe classe = RepositorioDeClassesOO.Instance().GetClasse(classeOperador);
            if (classe == null)
                return null;
            int index = classe.GetOperadores().FindIndex(k => k.nome.Equals(nomeOperador));

            if (index != -1)
            {
                Operador op = classe.GetOperadores().Find(k => k.GetTipo().Contains(tipo));
                return classe.GetOperadores()[index];
            } // if
            return null;
        }


        public object ExecuteOperador(string nomeDoOperador, Escopo escopo, params object[] valoresParametros)
        {
            object result = null;
            if (caller == null)
                throw new Exception("objeto que chama a execucao de funcao eh nulo.");
            for (int x = 0; x < valoresParametros.Length; x++)
            {
                if (valoresParametros[x] != null)
                {
                    if (valoresParametros[x].GetType() == typeof(Objeto))
                    {
                        valoresParametros[x] = ((Objeto)valoresParametros[x]).GetValor();
                    }
                        


                    if ((valoresParametros[x] != null) && Expressao.Instance.IsNumero(valoresParametros[x].ToString()))
                    {
                        valoresParametros[x] = Expressao.Instance.ConverteParaNumero(valoresParametros[x].ToString(), escopo);
                    }
                        
                }

            }

            if (this.InfoMethod != null)
            {
                Type classeOperador = this.InfoMethod.DeclaringType;
                // obtem um objeto da classe que construiu o operador.
                this.caller = classeOperador.GetConstructor(new Type[] { }).Invoke(null); 
                result = InfoMethod.Invoke(caller, valoresParametros);
            }
            else
            if (this.instrucoesFuncao != null)
            {
                result = this.ExecuteAFunction(valoresParametros.ToList<object>(), escopo);
            }
            else
            {
                return Expressao.Instance.ConverteParaNumero(result.ToString(), escopo);
            }
                


            return result;
        } // ExecuteOperador()

        public override string ToString()
        {
            string str = "";
            if (this.nome != null)
                str += "Nome: " + this.nome + "  pri: " + this.prioridade.ToString();
            return str;
        }// ToString()

 
      
    } // class
   
} //namespace

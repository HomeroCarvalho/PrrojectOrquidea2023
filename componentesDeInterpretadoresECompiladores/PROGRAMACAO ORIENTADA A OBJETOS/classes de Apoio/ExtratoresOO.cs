using System;
using System.Collections.Generic;
using System.Linq;
using Util;
using parser.ProgramacaoOrentadaAObjetos;
using System.Security.Principal;



using System.Text;
using parser.LISP;
using Wrappers.DataStructures;
using System.Security.Policy;

namespace parser
{
    /// <summary>
    /// constroi a estrutura de uma classe ou interface, a partir de tokens vindo da compilação.
    /// </summary>
    public class ExtratoresOO
    {

        public string nomeClasse;


        // header da classe.
        private HeaderClass headerDaClasse;







        // escopo com as propriedades, métodos e operadores da classe.
        private Escopo escopoDaClasse;


        // escopo fora do escopo da classe.
        private Escopo escopo;
        









        // codigo contendo tokens de processamento.
        private List<string> codigo;
       
        // tokens da classe, com header, nomes de classes herdadas e deserdadas, e corpo da classe.
        private List<string> tokensDaClasse;
        

        // tokens vindo do processador, ao compilar a classe. tokens de entrada.
        private List<string> tokensRaw;
        
        
    






        public List<string> MsgErros;






        
        
        public ExtratoresOO(Escopo escopo, List<string> tokensRaw)
        {
            this.escopo = escopo;
            this.codigo = escopo.codigo;
            this.MsgErros = new List<string>();
            this.tokensRaw = tokensRaw.ToList<string>();
        } // ExtratoresOO()





        /// extrai uma interface do codigo fonte texto.
        public Classe ExtraiUmaInterface()
        {
            if (tokensRaw[1] == "interface")
            {
                Classe classeInterface = ExtaiUmaClasse(Classe.tipoBluePrint.EH_INTERFACE);
                if (classeInterface != null)
                {
                    if (classeInterface.GetPropriedades().Count > 0)
                    {
                        this.MsgErros.Add("interface: " + tokensRaw[2] + "  nao pode ter propriedades!");
                        return null;
                    }
                    else
                    {
                        for (int x = 0; x < classeInterface.GetMetodos().Count; x++)

                        {
                            if (classeInterface.GetMetodos()[x].acessor != "public")
                                this.MsgErros.Add("metodo: " + classeInterface.GetMetodos()[x].nome + " precisa ser public!");

                        }
                    }
                    return classeInterface;
                } // if
                else return null;
            }

            return null;
        }



        private Classe.tipoBluePrint templateBluePrint;

        /// constroi classe dentro do código especificado no construtor da classe.
        public Classe ExtaiUmaClasse(Classe.tipoBluePrint bluePrint)
        {

           

            this.templateBluePrint = bluePrint;

            List<string> tokensTotais = new List<string>();

            // nome da classe, a ser encontrada.
            string nomeDeClasseOuInterface = null;
            string nomeDaClasse = null;
            string nomeDaInterface = null;
            string acessorDaClasseOuInterface = null;
            List<string> tokensDoCabecalhoDaClasse = null;
            List<string> tokensDoCorpoDaClasse = new List<string>();

            // obtém o código da classe, incluindo nome, cabeçalho da herança, e o corpo da classe.
            this.ExtraiCodigoDeUmaClasse(this.tokensRaw, out nomeDaClasse, out nomeDaInterface, out tokensDoCabecalhoDaClasse, out acessorDaClasseOuInterface, out tokensDoCorpoDaClasse);

            this.nomeClasse = nomeDaClasse;


            // obtem o header da classe.
            if (Expressao.headers != null)
                this.headerDaClasse = Expressao.headers.cabecalhoDeClasses.Find(k => k.nomeClasse == this.nomeClasse);
            else
            {
                MsgErros.Add("Headers dont initiates!");
                throw new Exception("class: " + nomeClasse + " cannot to compile! Headers dont load!");
            }



            tokensTotais = tokensDoCabecalhoDaClasse.ToList<string>();
            tokensTotais.AddRange(tokensDoCorpoDaClasse.ToList<string>());


            if (nomeDaClasse != null)
                nomeDeClasseOuInterface = nomeDaClasse;
            else
                nomeDeClasseOuInterface = nomeDaInterface;


            if (tokensTotais == null)
                return null;


            tokensDoCorpoDaClasse.RemoveAt(0);
            tokensDoCorpoDaClasse.RemoveAt(tokensDoCorpoDaClasse.Count - 1);


            // inicializa o escopo da classe, visivel pelos tokens do corpo da classe.
            escopoDaClasse = new Escopo(tokensDoCorpoDaClasse);
            // seta o nome da classe, a qualo o escopo pertence.
            Escopo.nomeClasseCurrente = nomeDaClasse;

            List<Classe> interfacesHerdadas = new List<Classe>();

            // constroi uma classe vazia, com metodos, propriedades, operadores em listas vazias.
            Classe umaClasse = new Classe(acessorDaClasseOuInterface, nomeDeClasseOuInterface, new List<Metodo>(), new List<Operador>(), new List<Objeto>());


            // a nome de classes e interfaces herdados ou deserdados.
            this.ProcessamentoDeHerancaEDeseranca(tokensDoCabecalhoDaClasse, umaClasse);

            //********************************************************************************************************
            // obtém as propriedades da classe.
            this.ExtraiPropriedades(umaClasse);

            //**********************************************************************************************
            // obtém os operadores da classe.
            this.ExtraiOperadores(umaClasse, escopoDaClasse);
            //**********************************************************************************************
            // obtém os métodos da classe.
            this.ExtraiMetodos(umaClasse, escopoDaClasse);

            //***********************************************************************************************************

            // se houve erros no processamento da classe/interface, passa as mensagens de erro para o escopo principal, mais acima ao escopo da classe, mais perto do compilador.
            if ((this.MsgErros != null) && (this.MsgErros.Count > 0))
                this.escopo.GetMsgErros().AddRange(this.MsgErros);


            umaClasse.construtores = new List<Metodo>();
            List<Metodo> construtoresDestaClasse = umaClasse.GetMetodos().FindAll(k => k.nome == umaClasse.GetNome());

            if ((construtoresDestaClasse != null) && (construtoresDestaClasse.Count > 0))
                umaClasse.construtores.AddRange(construtoresDestaClasse);
            else
            if (bluePrint == Classe.tipoBluePrint.EH_CLASSE)
            {
                UtilTokens.WriteAErrorMensage("nao ha nenhum construtores codificado para esta classe!", codigo, escopoDaClasse);
                return null;
            }


            //********************************************************************************************************
            umaClasse.tokensDaClasse = tokensTotais;
            umaClasse.escopoDaClasse = escopoDaClasse.Clone(); // guarda o escopo da classe.
           
            // verifica se há conflitos de nomes de metodos, propriedaddes, e operadores, que tem o mesmo nome, mas vêem de classes herdadas diferentes.
            this.VerificaConflitoDeNomesEmPropriedadesEMetodosEOperadores(umaClasse);


            if (nomeDaClasse != null)
            {
                // registra a classe no repositorio, se ja  houver a classe remove a versao anterior e registra a classe.
                RepositorioDeClassesOO.Instance().RegistraUmaClasse(umaClasse);
          
                // recompoe os tokens consumidos pela construção da classe.
                umaClasse.tokensDaClasse = tokensTotais.ToList<string>();

            } // if
            else
            if (nomeDaInterface != null)
                // registra a interface no repositório de interfaces.
                RepositorioDeClassesOO.Instance().RegistraUmaInterface(umaClasse);

            if (tokensDoCorpoDaClasse.Count == 0)
                return umaClasse;


            // Faz a validacao de interfaces herdadas (se foram implementadas pela classe ou outra interface.
            if ((umaClasse.interfacesHerdadas != null) && (umaClasse.interfacesHerdadas.Count > 0))
            {
                for (int i = 0; i < umaClasse.interfacesHerdadas.Count; i++)
                {
                    bool valida = this.ValidaInterface(umaClasse, umaClasse.interfacesHerdadas[i]);
                    if (!valida)
                        this.MsgErros.Add("Interface:" + umaClasse.interfacesHerdadas[i].nome + " nao implementada completamente na classe: " + umaClasse.nome + ".");
                }
            }

            Escopo.nomeClasseCurrente = umaClasse.GetNome();

            return umaClasse;
        }  // ConstroiClasses() 



        /// extrai nomes de heranca e deseranca.
        private void ProcessamentoDeHerancaEDeseranca(
              List<string> tokens, Classe classeHerdeira)
        {

            List<string> classesDeserdadas = new List<string>();
            List<string> interfacesDeserdadas = new List<string>();
          

            int startTokensHeranca = tokens.IndexOf("+");
            int startTokensDeseheranca = tokens.IndexOf("-");
            int endTokensHeranca = codigo.IndexOf("{");

            if ((startTokensHeranca == -1) && (startTokensDeseheranca == -1)) // é o caso da classe não tiver heranca ou deseranca, volta, pois está certo.
                return;

            
            int posicaoTokenHeranca = tokens.IndexOf("+") + 1; // posiciona o ponteiro de inteiros para o primeiro nome da classe herdada.
            while (posicaoTokenHeranca >0)
            {
                if (EhClasse(tokens[posicaoTokenHeranca]))
                {
                    string nomeClasseHerdada = tokens[posicaoTokenHeranca];
                    Classe classeHerdada = RepositorioDeClassesOO.Instance().GetClasse(nomeClasseHerdada);
                    if (classeHerdada != null)
                        classeHerdeira.classesHerdadas.Add(classeHerdada);
                    else
                        MsgErros.Add("classe: " + nomeClasseHerdada + " inexistente. verifique a sintaxe do nome da classe.");
                }

                if (EhInterface(tokens[posicaoTokenHeranca]))
                {
                    Classe classeInterface = RepositorioDeClassesOO.Instance().GetInterface(tokens[posicaoTokenHeranca]);
                    if (classeInterface != null)
                        classeHerdeira.interfacesHerdadas.Add(classeInterface);
                    else
                        MsgErros.Add("interface: " + tokens[posicaoTokenHeranca] + "  nao existente.");
                }
                posicaoTokenHeranca = tokens.IndexOf("+", posicaoTokenHeranca + 1) + 1; // posiciona o ponteiro de inteiros para o proximo nome da classe herdada.
            } // while


            int posicaoTokenDeseranca = tokens.IndexOf("-") + 1; //posiciona o ponteiro de inteiros para o primeiro nome da classe deserdada.

            while (posicaoTokenDeseranca>0)
            {
                if (this.EhClasse(tokens[posicaoTokenDeseranca]))
                    classesDeserdadas.Add(tokens[posicaoTokenDeseranca]);

                if (this.EhInterface(tokens[posicaoTokenDeseranca]))
                    interfacesDeserdadas.Add(tokens[posicaoTokenDeseranca]);

                posicaoTokenDeseranca = tokens.IndexOf("-", posicaoTokenDeseranca) + 1; // posiciona o ponteiro de inteiros para o proximo nome de classe deserdada.
            } // while

            RemoveItensDeClassesDeserdadas(classeHerdeira, classesDeserdadas);
            RemoveItensDeClassesDeserdadas(classeHerdeira, interfacesDeserdadas);
        }  // ExtraiClassesHerdeirasEInterfaces()




        /// <summary>
        /// resolve problemas de conflito de nomes de metodos,propriedades, operadores, que vem de classes herdadas diferentes, mas que possuem nome igual.
        /// Os metodo, propriedades, e operaores de todas classes herdadas ja foram adicionadas a classe herdeira (classe currente que está sendo construida.
        /// </summary>
        /// <param name="classeHerdeira">classe que herda atributos de classes herdadas.</param>
        private void VerificaConflitoDeNomesEmPropriedadesEMetodosEOperadores(Classe classeHerdeira)
        {
            if ((classeHerdeira.classesHerdadas == null) || (classeHerdeira.classesHerdadas.Count == 0))
                return;
            if ((classeHerdeira.GetPropriedades() != null) && (classeHerdeira.classesHerdadas!=null))
            {

                if ((classeHerdeira.GetPropriedades() != null) && (classeHerdeira.GetPropriedades().Count > 0))
                {
                    List<Objeto> todasPropriedadesHerdadas = new List<Objeto>();
                    for (int x = 0; x < classeHerdeira.classesHerdadas.Count; x++)
                    {
                        List<Objeto> propriedadesDeUmaClasseHerdada = classeHerdeira.classesHerdadas[x].GetPropriedades();
                        if ((propriedadesDeUmaClasseHerdada != null) && (propriedadesDeUmaClasseHerdada.Count > 0))
                        {
                            // OBTEM METODOS PUBLICOS  OU PROTECTED.
                            for (int i = 0; i < propriedadesDeUmaClasseHerdada.Count; i++)
                            {
                                if ((propriedadesDeUmaClasseHerdada[i].GetAcessor() == "public") ||
                                    (propriedadesDeUmaClasseHerdada[i].GetAcessor() == "protected"))
                                {
                                    todasPropriedadesHerdadas.Add(propriedadesDeUmaClasseHerdada[i]);
                                }
                            }
                            
                        }
                            
                    }

                    for (int x = 0; x < todasPropriedadesHerdadas.Count; x++)
                        for (int y = 0; y < todasPropriedadesHerdadas.Count; y++)
                            if ((x != y) && (todasPropriedadesHerdadas[x].GetNome() == todasPropriedadesHerdadas[y].GetNome()))
                            {

                                // informa ao programador que as propriedades herdadas de nomes iguais, foram setadas para nomelongo: nomeClasse+nomePropriedade, para evitar conflitos de nomes.
                                string avisoNomeLongo = "Aviso: propriedades: " + todasPropriedadesHerdadas[x].GetNome() + " de classes herdadas:" +
                               " possuem nomes iguais. Fazendo nome longo para as propriedades, para evitar conflitos de chamada destas propriedades, pela classe herdeira. Utilize o nome longo ( nomeClasse+nomePropriedade) para acessar estas propriedades.";
                                UtilTokens.WriteAErrorMensage(avisoNomeLongo, escopoDaClasse.codigo, escopoDaClasse);

                                classeHerdeira.GetPropriedades().Remove(todasPropriedadesHerdadas[x]);
                                classeHerdeira.GetPropriedades().Remove(todasPropriedadesHerdadas[y]);

                                todasPropriedadesHerdadas[x].SetNomeLongo(todasPropriedadesHerdadas[x].GetTipo());
                                todasPropriedadesHerdadas[y].SetNomeLongo(todasPropriedadesHerdadas[y].GetTipo());

                                classeHerdeira.GetPropriedades().Add(todasPropriedadesHerdadas[x]);
                                classeHerdeira.GetPropriedades().Add(todasPropriedadesHerdadas[y]);

                            
                            }
                }

            }
            if ((classeHerdeira.GetMetodos() != null) && (classeHerdeira.classesHerdadas != null)) 
            {

                List<Metodo> todosMetodoHerdados = new List<Metodo>();
                for (int x = 0; x < classeHerdeira.classesHerdadas.Count; x++)
                {
                    List<Metodo> metodosDeUmaClasseHerdada = classeHerdeira.classesHerdadas[x].GetMetodos();
                    if ((metodosDeUmaClasseHerdada != null) && (metodosDeUmaClasseHerdada.Count > 0))
                    {
                        for (int m = 0; m < metodosDeUmaClasseHerdada.Count; m++)
                        {
                            if ((metodosDeUmaClasseHerdada[m].acessor=="public") || (metodosDeUmaClasseHerdada[m].acessor=="protected"))
                            {
                                metodosDeUmaClasseHerdada[m].nomeClasse = classeHerdeira.classesHerdadas[x].GetNome();
                                todosMetodoHerdados.Add(metodosDeUmaClasseHerdada[m]);
                            }
                        }
                        
                    }
                }

                for (int x = 0; x < todosMetodoHerdados.Count; x++)
                    for (int y = 0; y < todosMetodoHerdados.Count; y++) 
                        if ((x!=y) &&  (Metodo.IguaisFuncoes(todosMetodoHerdados[x], todosMetodoHerdados[y])))
                        {
                            string avisoNomeLongo = "Aviso: operador: " + todosMetodoHerdados[x].nome + " de classes herdadas:" +
                                                   " possuem nomes iguais. Fazendo nome longo para estes metodos, para evitar conflitos de chamada destes metodos, pela classe herdeira. Utilize o nome longo ( nomeClasse+nomeMetodo) para acessar estes metodos.";

                            UtilTokens.WriteAErrorMensage(avisoNomeLongo, escopoDaClasse.codigo, escopoDaClasse);

                            classeHerdeira.GetMetodos().Remove(todosMetodoHerdados[x]);
                            classeHerdeira.GetMetodos().Remove(todosMetodoHerdados[y]);

                            todosMetodoHerdados[x].SetNomeLongo(todosMetodoHerdados[x].nomeClasse);
                            todosMetodoHerdados[y].SetNomeLongo(todosMetodoHerdados[y].nomeClasse);

                            classeHerdeira.GetMetodos().Add(todosMetodoHerdados[x]);
                            classeHerdeira.GetMetodos().Add(todosMetodoHerdados[y]);


                        }
            }

            if (classeHerdeira.GetOperadores() != null)
            {

                List<Operador> todosOperadoresHerdados = new List<Operador>();

                for (int x = 0; x < classeHerdeira.classesHerdadas.Count; x++)
                {
                    List<Operador> operadoresDeUmaClasseHerdada = classeHerdeira.classesHerdadas[x].GetOperadores();
                    if ((operadoresDeUmaClasseHerdada != null) && (operadoresDeUmaClasseHerdada.Count > 0))
                    {
                        for (int op = 0; op < operadoresDeUmaClasseHerdada.Count; op++)
                        {
                            operadoresDeUmaClasseHerdada[op].nomeClasse = classeHerdeira.classesHerdadas[x].GetNome();
                            todosOperadoresHerdados.Add(operadoresDeUmaClasseHerdada[op]);
                        }
                    }
                }

                for (int x = 0; x < todosOperadoresHerdados.Count; x++)
                    for (int y = 0; y < todosOperadoresHerdados.Count; y++)
                        if ((x != y) && (Operador.IguaisOperadores(todosOperadoresHerdados[x], todosOperadoresHerdados[y])))
                        {
                            string avisoNomeLongo = "Aviso: metodos: " + todosOperadoresHerdados[y].nome + " de classes herdadas:" +
                                                            " possuem nomes iguais. Fazendo nome longo para estes metodos, para evitar conflitos de chamada destes metodos, pela classe herdeira. Utilize o nome longo ( nomeClasse+nomeMetodo) para acessar estes metodos.";

                            UtilTokens.WriteAErrorMensage(avisoNomeLongo, escopoDaClasse.codigo, escopoDaClasse);


                            classeHerdeira.GetOperadores().Remove(todosOperadoresHerdados[x]);
                            classeHerdeira.GetOperadores().Remove(todosOperadoresHerdados[y]);


                            todosOperadoresHerdados[x].SetNomeLongo(todosOperadoresHerdados[x].nomeClasse);
                            todosOperadoresHerdados[x].SetNomeLongo(todosOperadoresHerdados[y].nomeClasse);


                            
                            classeHerdeira.GetOperadores().Add(todosOperadoresHerdados[x]);
                            classeHerdeira.GetOperadores().Add(todosOperadoresHerdados[y]);

                        }

            }

        }
        private static void RemoveItensDeClassesDeserdadas(Classe classeHerdeira, List<string> txt_nomesDeseranca)
        {

            // retira os metodos e propriedades deserherdados.
            for (int indexClass = 0; indexClass < txt_nomesDeseranca.Count; indexClass++)
            {
                Classe umaClasseDeserdada = RepositorioDeClassesOO.Instance().GetClasse(txt_nomesDeseranca[indexClass]);
                if (umaClasseDeserdada != null)
                {

                    for (int m = 0; m < umaClasseDeserdada.GetMetodos().Count; m++)
                    {
                        // remove metodos que são publicos ou protegidos, com o mesmo nome de metodo da classe herdeira. (a classe que recebe a herança).
                        if ((umaClasseDeserdada.GetMetodos()[m].acessor == "public") || (umaClasseDeserdada.GetMetodos()[m].acessor == "protected"))
                        {
                            int indexRemocao = classeHerdeira.GetMetodos().FindIndex(k => k.nome == umaClasseDeserdada.GetNome());
                            if (indexRemocao != -1)
                                classeHerdeira.GetMetodos().RemoveAt(indexRemocao);
                        } // if

                    }
                    for (int p = 0; p < umaClasseDeserdada.GetPropriedades().Count; p++)
                        // remove propriedades publicas ou protegidas, com o mesmo nome de propriedade da classe herdeira (a classe que receber a herança).
                        if ((umaClasseDeserdada.GetPropriedades()[p].GetAcessor() == "public") ||
                            (umaClasseDeserdada.GetPropriedades()[p].GetAcessor() == "private")) 
                        {
                            int indexRemocao = classeHerdeira.GetPropriedades().FindIndex(k => k.GetNome() == umaClasseDeserdada.GetPropriedades()[p].GetNome());
                            if (indexRemocao == -1)
                                classeHerdeira.GetPropriedades().RemoveAt(indexRemocao);
                        }  // if

                    for (int op = 0; op < umaClasseDeserdada.GetOperadores().Count; op++)
                    {
                        // remove operadores com o mesmo nome de operador da classe herdeira.
                        int indexRemocao = classeHerdeira.GetOperadores().FindIndex(k => k.nome == umaClasseDeserdada.GetOperadores()[op].nome);
                        if (indexRemocao != -1)
                            classeHerdeira.GetOperadores().RemoveAt(indexRemocao);
                    }

                } // if

                classeHerdeira.classesHerdadas.Remove(umaClasseDeserdada);
            } // for x

           
        }

        /// <summary>
        /// valida o nome de uma classe, se existe no repositório de classes.
        /// </summary>
        private bool EhClasse(string nomeClasse)
        {

            return (RepositorioDeClassesOO.Instance().GetClasse(nomeClasse) != null);
        } // EhClasseHerdada()

        /// valida o nome de interface, se existe no repositório de interfaces.
        private bool EhInterface(string nomeInterface)
        {
            // trata do caso em que há classes no repositório.
            return (RepositorioDeClassesOO.Instance().GetInterface(nomeInterface) != null);

        } // EhClasseHerdada()


        /// Verifica se uma interface foi implementada na classe herdeira.
        private bool ValidaInterface(Classe _classe, Classe _interface)
        {
            if ((_interface.GetMetodos() == null) || (_interface.GetMetodos().Count == 0))
                return true;

            for (int x = 0; x < _interface.GetMetodos().Count; x++)
            {

                int index = _classe.GetMetodos().FindIndex(k => k.nome == _interface.GetMetodos()[x].nome);
                if (index == -1)
                {
                    this.MsgErros.Add("metodo: " + _interface.GetMetodos()[x].nome + "  da interface: " + _interface.nome + " nao implementado.");
                    return false;
                }
            } 


            if ((_interface.GetPropriedades() == null) || (_interface.GetPropriedades().Count == 0))
                return true;

            return true;
        } 







        /// extrai métodos e operadores a partir de codigo da clase.
        private void ExtraiMetodos(Classe classeCurrente, Escopo escopo)
        {
            Escopo.nomeClasseCurrente = classeCurrente.nome;

            this.headerDaClasse = Expressao.headers.cabecalhoDeClasses.Find(k => k.nomeClasse == classeCurrente.GetNome());
            List<HeaderMethod> headersMetodo = this.headerDaClasse.methods;
            if (this.headerDaClasse != null)
            {
                
                if ((headersMetodo != null) && (headersMetodo.Count > 0))
                {
                    for (int x = 0; x < headersMetodo.Count; x++)
                    {
                        string acessor = headersMetodo[x].acessor;
                        string classeMetodo = classeCurrente.nome;
                        string nameMetodo = headersMetodo[x].name;
                        string tipoDeRetorno = headersMetodo[x].typeReturn;
                        bool isMetodoParametro = headersMetodo[x].typeHeaderParameter == HeaderMethod.tiposParametros.parametroMetodo;
                        List<string> tokensMetodo = headersMetodo[x].tokens.ToList<string>();

                        if (acessor == null)
                        {
                            acessor = "public";
                        }

                        Escopo escopoDoMetodo = new Escopo(tokensMetodo);
                        escopoDoMetodo.escopoPai= escopoDaClasse;


                        // valida o tipo do metodo.
                        int indexTipo = Expressao.headers.cabecalhoDeClasses.FindIndex(k => k.nomeClasse == classeMetodo);
                        if (indexTipo == -1)
                        {
                            MsgErros.Add("classe " + classeMetodo + " do metodo: " + nameMetodo + " nao reconhecida!");
                            continue;
                        }

                        // valida o tipo de retorno.
                        int indexTipo2 = Expressao.headers.cabecalhoDeClasses.FindIndex(k => k.nomeClasse == tipoDeRetorno);
                        if ((indexTipo2 == -1) && (tipoDeRetorno != "void"))
                        {
                            MsgErros.Add("tipo de retorno " + tipoDeRetorno + " do metodo: " + nameMetodo + " nao reconhecida!");
                            continue;
                        }




                        // obtem os parametros do metodo.
                        List<Objeto> parametrosMetodo = new List<Objeto>();
                        if (headersMetodo[x].parameters != null)
                        {
                            for (int p = 0; p < headersMetodo[x].parameters.Count; p++)
                            {
                                HeaderProperty HeadersParametro = (HeaderProperty)headersMetodo[x].parameters[p];

                                

                                // valida os tipos dos parametros.
                                int indexTipo3 = Expressao.headers.cabecalhoDeClasses.FindIndex(k => k.nomeClasse == HeadersParametro.className);
                                if (indexTipo == -1)
                                {
                                    MsgErros.Add("classe " + HeadersParametro.className + " do metodo: " + HeadersParametro.name + " nao reconhecido!, em classe: " + headerDaClasse.nomeClasse + ".");
                                    continue;
                                }



                                Objeto umParametro = new Objeto(HeadersParametro.acessor, HeadersParametro.className, HeadersParametro.name, null);
                                umParametro.isMultArgument = HeadersParametro.isMultArgument;
                                umParametro.tipoElemento = HeadersParametro.tipoElemento;
                                umParametro.isFunctionParameter = HeadersParametro.isMethodParameter;
                                
                               

                                if (HeadersParametro.className == "Metodo")
                                {
                                    umParametro.isMethod = true;
                                }

                                parametrosMetodo.Add(umParametro);

                            }
                        }

                        escopoDoMetodo = new Escopo(headersMetodo[x].tokens);
                        Escopo.nomeClasseCurrente = this.nomeClasse;
                        List<Instrucao> instrucoesDoCorpoDoMetodo = new List<Instrucao>();
                        // inicializa a função do metodo, sem compilar.
                        Metodo fncMetodo = new Metodo(classeMetodo, acessor, nameMetodo, parametrosMetodo.ToArray(), tipoDeRetorno, instrucoesDoCorpoDoMetodo, escopoDoMetodo);
                        // seta o indice do metodo, perante aos outros metodos.
                        fncMetodo.indexInClass = x;

                        // validacao de parametros multi-argumentos.
                        if ((fncMetodo.parametrosDaFuncao != null) && (fncMetodo.parametrosDaFuncao.Length > 0))
                        {
                            for (int i = 0; i < fncMetodo.parametrosDaFuncao.Length; i++)
                            {

                                // PROCESSAMENTO DE VALIDACAO DE PARAMETRO MULTI-ARGUMENTO.
                                if ((fncMetodo.parametrosDaFuncao.Length > 1) && (fncMetodo.parametrosDaFuncao[i].isMultArgument) && (fncMetodo.parametrosDaFuncao[i - 1].isMultArgument))
                                {

                                    // VALIDACAO DE PARAMETROS EM SEQUENCIA EM QUE SAO MESMO TIPO, CAUSANDO IMPRECISAO DE QUAL É UM PARAMETRO, SE PARAMETRO NORMAL, OU UM PARAMETRO DO MULTI-ARGUMENTO.
                                    if (fncMetodo.parametrosDaFuncao[i].tipo == fncMetodo.parametrosDaFuncao[i - 1].tipo)
                                    {
                                        UtilTokens.WriteAErrorMensage(
                                            "warning: method: " + fncMetodo.nome + " has invalid multi-arguments parameters", classeCurrente.tokensDaClasse, escopoDaClasse);

                                    }

                                    // VALIDACAO DE WRAPPERS OBJECT.
                                    if ((fncMetodo.parametrosDaFuncao[i].isWrapperObject) && (fncMetodo.parametrosDaFuncao[i - 1].isWrapperObject) &&
                                        (fncMetodo.parametrosDaFuncao[i].tipoElemento == fncMetodo.parametrosDaFuncao[i - 1].tipoElemento))
                                    {
                                        UtilTokens.WriteAErrorMensage(
                                            "warning: method: " + fncMetodo.nome + " has invalid multi-arguments parameters", classeCurrente.tokensDaClasse, escopoDaClasse);

                                    }

                                    // VALIDACAO DE PARAMETROS EM SEQUENCIA.
                                    if ((i + 1 < fncMetodo.parametrosDaFuncao.Length) && (fncMetodo.parametrosDaFuncao[i].isMultArgument) && (fncMetodo.parametrosDaFuncao[i].tipo == fncMetodo.parametrosDaFuncao[i + 1].tipo))
                                    {
                                        UtilTokens.WriteAErrorMensage(
                                        "warning: method: " + fncMetodo.nome + " has invalid multi-arguments parameters", classeCurrente.tokensDaClasse, escopoDaClasse);

                                    }
                                }
                            }

                        }


                        /// seta algumas propriedades fora dos escopo.
                        fncMetodo.tokens = tokensMetodo.ToList<string>();
                        // compilação feita após todas classes forem obtidas.
                        fncMetodo.isCompiled = false; 
                        // seta o id do escopo para localizacao quando executa um metodo.
                        fncMetodo.idEscopo = x;
                        fncMetodo.escopo = escopoDoMetodo.Clone();
                        // seta se o metodo e um metodo-parametro.
                        fncMetodo.isMethod = isMetodoParametro;

                        // ADICIONA O METODO, para a lista de metodos da classe. 
                        classeCurrente.GetMetodos().Add(fncMetodo);

                    }
                }




            }



            // FAZ O PROCESSO de extração de metodos de classes herdadas.
            for (int x = 0; x < classeCurrente.classesHerdadas.Count; x++)
            {
                List<Metodo> metodosHerdados = classeCurrente.classesHerdadas[x].GetMetodos().FindAll(k => k.acessor == "public" || k.acessor == "protected");
                // obtem metodos herdados, mas só se o metodo herdado nao for um metodo herdeiro
                if ((metodosHerdados != null) && (metodosHerdados.Count > 0))
                {
                    for (int fnc = 0; fnc < metodosHerdados.Count; fnc++)
                        if (classeCurrente.GetMetodos().Find(k => Metodo.IguaisFuncoes(k, metodosHerdados[fnc])) == null)
                            classeCurrente.GetMetodos().Add(metodosHerdados[fnc]);
                }
            }



            // FAZ A COMPILACAO DAS INSTRUCOES DO CORPO DE METODOS.
            if ((classeCurrente.GetMetodos() != null) && (classeCurrente.GetMetodos().Count > 0))
            {
                List<Metodo> metodos = classeCurrente.GetMetodos();


                for (int x = 0; x < metodos.Count; x++)
                {
                    string nomeDoMetodo = metodos[x].nome;

                    // compila as instrucoes do corpo do metodo.
                    List<Instrucao> instrucoesDoCorpoDoMetodo = new List<Instrucao>();
                    if ((metodos[x].tokens != null) && (metodos[x].tokens.Count > 0))
                    {

                        ProcessadorDeID compiladorCorpoDoMetdo = new ProcessadorDeID(metodos[x].tokens);
                        Escopo.nomeClasseCurrente = this.nomeClasse;

                        List<Objeto> parametrosMetodo = metodos[x].parametrosDaFuncao.ToList<Objeto>();

                        // REGISTRA NO ESCOPO DO METODO OS PARAMETROS. entra nos calculos dentro do escopo da funcao!
                        if ((parametrosMetodo != null) && (parametrosMetodo.Count > 0))
                        {
                            for (int i = 0; i < parametrosMetodo.Count; i++)
                            {
                                compiladorCorpoDoMetdo.escopo.tabela.RegistraObjeto(parametrosMetodo[i]);
                            }
                        }

                        
                        // REGISTRA NO ESCOPO DO METODO AS FUNCOES DA CLASSE, para fins de processamento de funcoes dentro
                        // do corpo do metodo, o que nao necessita um objeto caller, p.ex.: obj1.funcao1(), é possível: funcao1();
                        if ((metodos != null) && (metodos.Count > 0))
                        {
                            foreach(Metodo funcoesDaClasse in metodos)
                            {
                                compiladorCorpoDoMetdo.escopo.tabela.RegistraFuncao(funcoesDaClasse);
                            }
                        }
                        


                        // COMPILA O CORPO DO METODO.
                        compiladorCorpoDoMetdo.Compilar();

                        // compoe o ESCOPO DO METODO e as INSTRUCOES DO corpo do metodo.
                        instrucoesDoCorpoDoMetodo = compiladorCorpoDoMetdo.GetInstrucoes();
                        metodos[x].escopo = compiladorCorpoDoMetdo.escopo.Clone();
                        metodos[x].instrucoesFuncao.Clear();
                        metodos[x].instrucoesFuncao.AddRange(compiladorCorpoDoMetdo.GetInstrucoes());
                    }
                }

            }

        }


        private void ExtraiOperadores(Classe classeCurrente, Escopo escopo)
        {
            Escopo.nomeClasseCurrente = classeCurrente.nome;
            this.headerDaClasse = Expressao.headers.cabecalhoDeClasses.Find(k => k.nomeClasse == classeCurrente.GetNome());
            if (this.headerDaClasse != null)
            {
                List<HeaderOperator> headersDeOperadores = this.headerDaClasse.operators;
                if ((headersDeOperadores != null) && (headersDeOperadores.Count > 0))
                {
                    for (int x = 0; x < headersDeOperadores.Count; x++)
                    {
                        HeaderOperator headerDeUmOperador = headersDeOperadores[x];

                        string nomeOperador = headerDeUmOperador.name;
                        string classeOperador = headerDeUmOperador.className;
                        string tipoRetorno = headerDeUmOperador.tipoRetorno;
                        int prioridade = headerDeUmOperador.prioridade;



                        int index1 = Expressao.headers.cabecalhoDeClasses.FindIndex(k => k.nomeClasse == tipoRetorno);
                        if (index1 == -1)
                        {
                            MsgErros.Add("operador: " + nomeOperador + " com tipo de retorno nao reconhecido!, na classe : " + headerDaClasse.nomeClasse + ".");
                            continue;
                        }



                        // CONSTROI A LISTA DE PARAMETROS DO OPERADOR.
                        List<Objeto> parametros = new List<Objeto>();
                        for (int i = 0; i < headerDeUmOperador.operands.Count; i++)
                        {
                            parametros.Add(new Objeto("private", classeOperador, nomeOperador, null));


                            int index2 = Expressao.headers.cabecalhoDeClasses.FindIndex(k => k.nomeClasse == classeOperador);
                            if (index2 == -1)
                            {
                                MsgErros.Add("operador: " + nomeOperador + " com classe nao reconhecido!, na classe : " + headerDaClasse.nomeClasse + ".");
                                continue;
                            }
                        }

                        // CONSTROI AS INSTRUCOES DO CORPO DO OPERADOR.
                        List<Instrucao> instrucoesDoCorpoDoOperador = new List<Instrucao>();
                        if ((headerDeUmOperador.tokens != null) && (headerDeUmOperador.tokens.Count > 0))
                        {
                            ProcessadorDeID compilador = new ProcessadorDeID(headerDeUmOperador.tokens);
                            Escopo.nomeClasseCurrente = this.nomeClasse;
                            compilador.Compilar();

                            instrucoesDoCorpoDoOperador = compilador.GetInstrucoes();
                        }



                        string tipoOperador = "";
                        if (headerDeUmOperador.tipoDoOperador == HeaderOperator.typeOperator.binary)
                            tipoOperador = "BINARIO";
                        else
                            tipoOperador = "UNARIO";



                        // constroi um operador que nao eh um operador importado.
                        Operador umOperador = new Operador(classeOperador, nomeOperador, prioridade, parametros.ToArray(), tipoOperador, null, escopoDaClasse);
                        // seta as instrucoes da funcao do operador.
                        umOperador.instrucoesFuncao = instrucoesDoCorpoDoOperador;


                        classeCurrente.GetOperadores().Add(umOperador);

                    }
                }

            }


            if (classeCurrente.classesHerdadas != null)
            {
                for (int x = 0; x < classeCurrente.classesHerdadas.Count; x++)
                {

                    List<Operador> operadoresHerdados = classeCurrente.classesHerdadas[x].GetOperadores();
                    if ((operadoresHerdados != null) && (operadoresHerdados.Count > 0))
                        classeCurrente.GetOperadores().AddRange(operadoresHerdados);

                }
            }
        }

 
        /// Extrai propriedades (campos) a partir do código da classe.
        private void ExtraiPropriedades(Classe classeCurrent)
        {
            Escopo.nomeClasseCurrente = this.nomeClasse;
            this.headerDaClasse = Expressao.headers.cabecalhoDeClasses.Find(k => k.nomeClasse == classeCurrent.GetNome());
            if (this.headerDaClasse != null)
            {

                List<HeaderProperty> headersDePropriedades = this.headerDaClasse.properties;
                if ((headersDePropriedades != null) && (headersDePropriedades.Count > 0))
                {
                    for (int x = 0; x < headersDePropriedades.Count; x++)
                    {
                        string nomePropriedade = headersDePropriedades[x].name;
                        string tipoPropriedade = headersDePropriedades[x].className;
                        string acessorPropriedade = headersDePropriedades[x].acessor;
                        string valor = null;


                        // validação do tipo da propriedade.
                        if (Expressao.headers.cabecalhoDeClasses.Find(k => k.nomeClasse == tipoPropriedade) == null)
                        {
                            MsgErros.Add("propriedade: " + tipoPropriedade + " com tipo nao reconhecido!, classe " + headerDaClasse.nomeClasse);
                            continue;
                        }

                        // objeto de uma propriedade.
                        Objeto umaPropriedade = new Objeto(acessorPropriedade, tipoPropriedade, nomePropriedade, valor);





                        // propriedades não estáticas.
                        if (!headersDePropriedades[x].isStatic)
                        {
                            classeCurrent.GetPropriedades().Add(umaPropriedade);
                        }
                        else
                        // propriedades estáticas.
                        if (headersDePropriedades[x].isStatic)
                        {
                            umaPropriedade.isStatic = true;
                            classeCurrent.propriedadesEstaticas.Add(umaPropriedade);
                        }




                    }

                }


                for (int x = 0; x < classeCurrent.classesHerdadas.Count; x++)
                {
                    List<Objeto> propriedadesHerdadas = classeCurrent.classesHerdadas[x].GetPropriedades().FindAll(k => k.GetAcessor() == "public" || (k.GetAcessor() == "protected"));
                    classeCurrent.GetPropriedades().AddRange(propriedadesHerdadas);
                }
            }
        } // ExtraiPropriedades()


      
        /// obtém o corpo da classe ou interface, a partir de uma lista de codigo, não tokens.
        private void ExtraiCodigoDeUmaClasse(List<string> codigo, out string nomeDaClasse, out string nomeDaInterface, out List<string> tokensCabecalhoDaClasse,out string acessor, out List<string> tokensCorpoDaClasse)
        {
            

            nomeDaClasse = null; 
            nomeDaInterface = null;

            List<string> tokens = new Tokens(codigo).GetTokens();


            List<string> acessorVAlidos = new List<string>() { "public", "private", "protected" };
            acessor = acessorVAlidos.Find(k => k.Equals(tokens[0]));
            if (acessor == null)
                acessor = "protected";

            
            
            int indexCorpo = tokens.IndexOf("{");
            if (indexCorpo == -1)
            {
                UtilTokens.WriteAErrorMensage("classe com erro de sintaxe ", escopoDaClasse.codigo, escopo);
              
                tokensCabecalhoDaClasse = new List<string>();
                this.tokensDaClasse = new List<string>();

            }

            tokensCorpoDaClasse = UtilTokens.GetCodigoEntreOperadores(indexCorpo, "{", "}", tokens);
            tokensCabecalhoDaClasse = tokens.GetRange(0, indexCorpo);
            



            int indexNomeDaClasse = tokens.IndexOf("class");
            if (indexNomeDaClasse != -1)
            {
                nomeDaClasse = tokens[indexNomeDaClasse + 1];
                nomeDaInterface = null;

            }
            int indexNomeDaInterface = tokens.IndexOf("interface");
            if (indexNomeDaInterface != -1)
            {
                nomeDaClasse = null;
                nomeDaInterface = tokens[indexNomeDaInterface + 1];

            }

        } // ExtraiCodigoDeUmaClasse()

        /// <summary>
        /// método utilizado para gerar mensagens de erro, no processamento de classes ou interfaces.
        /// </summary>
        /// <param name="nomeDaClasse">nome de uma classe, ou...</param>
        /// <param name="nomeDaInterface">... ou nome de uma interface...</param>
        /// <param name="msgErro">lista de erros.</param>
        private void EmiteMensagemDeErro(string nomeDaClasse, string nomeDaInterface, string msgErro)
        {
            if (nomeDaClasse != null)
                this.MsgErros.Add("Erro no processamento da classe: " + nomeDaClasse + msgErro + ".");
            else
                this.MsgErros.Add("Erro no processamento da interface: " + nomeDaInterface + msgErro + ".");

        } // EmiteMensagemDeErro()



        public class Testes : SuiteClasseTestes
        {
            public Testes() : base("testes para processamento de classes")
            {
            }

            public void TesteExtracaoClasseComFalhaDeTipos(AssercaoSuiteClasse assercao)
            {
                string codigoClasseB = "public class classeA { public static semTipo propriedadeA;  public classeA() { };  public int metodoA(){}; }";
                List<string> tokensClasse = new Tokens(codigoClasseB).GetTokens();

                // inicializa os headers, senão não compila as classes.
                Expressao.InitHeaders(codigoClasseB);

                Escopo escopo = new Escopo(tokensClasse);
                ExtratoresOO extrator = new ExtratoresOO(escopo, tokensClasse);
                extrator.ExtaiUmaClasse(Classe.tipoBluePrint.EH_CLASSE);


                assercao.IsTrue(escopo.GetMsgErros() != null && escopo.GetMsgErros().Count > 0);



            }

            public void TesteExtracaoClasseComMetodosEPropriedadeEstatica(AssercaoSuiteClasse assercao)
            {
                string codigoClasseB = "public class classeA { public static int propriedadeA;  public classeA() { };  public int metodoA(){}; }";
                List<string> tokensClasse = new Tokens(codigoClasseB).GetTokens();

                // inicializa os headers, senão não compila as classes.
                Expressao.InitHeaders(codigoClasseB);

                Escopo escopo = new Escopo(tokensClasse);
                ExtratoresOO extrator = new ExtratoresOO(escopo, tokensClasse);
                extrator.ExtaiUmaClasse(Classe.tipoBluePrint.EH_CLASSE);

                assercao.IsTrue(
                    RepositorioDeClassesOO.Instance().GetClasse("classeA") != null &&
                    RepositorioDeClassesOO.Instance().GetClasse("classeA").GetMetodos().Count == 2 &&
                    RepositorioDeClassesOO.Instance().GetClasse("classeA").GetMetodo("metodoA") != null &&
                    RepositorioDeClassesOO.Instance().GetClasse("classeA").propriedadesEstaticas != null &&
                    RepositorioDeClassesOO.Instance().GetClasse("classeA").propriedadesEstaticas.Count == 1);




            }

            public void TesteExtracaoClasseComMetodosEPropriedades(AssercaoSuiteClasse assercao)
            {
                string codigoClasseB = "public class classeA { public int propriedadeA;  public classeA() { };  public int metodoA(){}; }";
                List<string> tokensClasse = new Tokens(codigoClasseB).GetTokens();

                // inicializa os headers, senão não compila as classes.
                Expressao.InitHeaders(codigoClasseB);

                Escopo escopo = new Escopo(tokensClasse);
                ExtratoresOO extrator = new ExtratoresOO(escopo, tokensClasse);
                extrator.ExtaiUmaClasse(Classe.tipoBluePrint.EH_CLASSE);

                assercao.IsTrue(
                    RepositorioDeClassesOO.Instance().GetClasse("classeA") != null &&
                    RepositorioDeClassesOO.Instance().GetClasse("classeA").GetMetodos().Count == 2 &&
                    RepositorioDeClassesOO.Instance().GetClasse("classeA").GetMetodo("metodoA") != null &&
                    RepositorioDeClassesOO.Instance().GetClasse("classeA").GetPropriedades().Count == 1);



            }

            public void TesteExtracaoClasseComMetodos(AssercaoSuiteClasse assercao)
            {
                string codigoClasseB = "public class classeA { public classeA() { };  public int metodoA(){}; }";
                List<string> tokensClasse = new Tokens(codigoClasseB).GetTokens();

                // inicializa os headers, senão não compila as classes.
                Expressao.InitHeaders(codigoClasseB);

                Escopo escopo = new Escopo(tokensClasse);
                ExtratoresOO extrator = new ExtratoresOO(escopo, tokensClasse);
                extrator.ExtaiUmaClasse(Classe.tipoBluePrint.EH_CLASSE);

                assercao.IsTrue(
                    RepositorioDeClassesOO.Instance().GetClasse("classeA") != null &&
                    RepositorioDeClassesOO.Instance().GetClasse("classeA").GetMetodos().Count == 2);



            }


        }
    } // ExtratoresOO
  
} // namespace

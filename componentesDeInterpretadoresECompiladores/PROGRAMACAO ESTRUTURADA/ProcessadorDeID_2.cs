using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using parser.ProgramacaoOrentadaAObjetos;
namespace parser
{
    public class BuildInstrucoes
    {

        private List<string> codigo = new List<string>();
        private Escopo escopo = null;
        private static LinguagemOrquidea linguagem = null;
        public BuildInstrucoes(List<string> codigo)
        {
            if (linguagem == null)
                linguagem = LinguagemOrquidea.Instance();
            this.codigo = codigo.ToList<string>();
            this.escopo = new Escopo(codigo);
        }


        public BuildInstrucoes(string code)
        {
            if (linguagem == null)
                linguagem = LinguagemOrquidea.Instance();
            this.codigo = new Tokens(code).GetTokens();
            this.escopo = new Escopo(this.codigo);
        }

        protected Instrucao BuildInstrucaoModule(UmaSequenciaID sequencia, Escopo escopo)
        {
            // module nameLibrary;
            // .
            if (!sequencia.tokens.Contains("module"))
                return null;


            // faz uma copia dos tokens.
            List<string> tokensInstrucao = sequencia.tokens.ToList<string>();

            // remove o token "module".
            tokensInstrucao.RemoveAt(0);

            // remove o token ";", se tiver.
            if (tokensInstrucao[tokensInstrucao.Count - 1] == ";")
                tokensInstrucao.RemoveAt(tokensInstrucao.Count - 1);





            string nomeBiblioteca = Util.UtilString.UneLinhasLista(tokensInstrucao).Trim(' ');
            Classe classeBiblioteca = RepositorioDeClassesOO.Instance().GetClasse(nomeBiblioteca);
            if (classeBiblioteca == null)
            {
                ImportadorDeClasses importador = new ImportadorDeClasses(nomeBiblioteca + ".dll");
                if (RepositorioDeClassesOO.Instance().GetClasse(nomeBiblioteca) == null)
                {
                    UtilTokens.WriteAErrorMensage("internal error to load a file dll of module: " + nomeBiblioteca, sequencia.tokens, escopo);
                    return null;
                }

            }

            // registra a classe do modulo, no escopo.
            escopo.tabela.RegistraClasse(classeBiblioteca);




            Instrucao instrucaoImporter = new Instrucao();



            return instrucaoImporter;
        }  // BuildInstrucaoConstructor()


        protected Instrucao BuildInstrucaoImporter(UmaSequenciaID sequencia, Escopo escopo)
        {
            // importer ( nomeAssembly).
            if (!sequencia.tokens.Contains("importer"))
                return null;
            if (sequencia.tokens.Count < 3)
                return null;


            // faz uma copia dos tokens.
            List<string> tokensInstrucao = sequencia.tokens.ToList<string>();

            // remove o nome da instrução, e os parenteses abre e fecha da instrucao, e o ponto e virgula do comando.
            tokensInstrucao.RemoveRange(0, 2);
            tokensInstrucao.RemoveRange(tokensInstrucao.Count - 2, 2);






            string nomeArquivoAsssembly = Util.UtilString.UneLinhasLista(tokensInstrucao);
            nomeArquivoAsssembly = nomeArquivoAsssembly.Replace(" ", "");





            Expressao exprss_comando = new Expressao(new string[] { "importer", nomeArquivoAsssembly }, escopo);
            escopo.tabela.AdicionaExpressoes(escopo, exprss_comando);



            ImportadorDeClasses importador = new ImportadorDeClasses(nomeArquivoAsssembly);
            importador.ImportAllClassesFromAssembly(); // importa previamente as classes do arquivo assembly.

            Instrucao instrucaoImporter = new Instrucao(ProgramaEmVM.codeImporter, new List<Expressao> { exprss_comando }, new List<List<Instrucao>>());



            return instrucaoImporter;
        }  // BuildInstrucaoConstructor()


        protected Instrucao BuildInstrucaoConstrutorUP(UmaSequenciaID sequencia, Escopo escopo)
        {
            /// template= classeHerdeira.construtorUP(nomeClasseHerdada, List<Expressao> parametrosDoConstrutor).
            /// pode ser o objeto "actual";

            string nomeClasseHedeira = sequencia.tokens[0];
            string nomeClasseHerdada = sequencia.tokens[4];

            if (!RepositorioDeClassesOO.Instance().ExisteClasse(nomeClasseHedeira))
            {
                UtilTokens.WriteAErrorMensage("internal error, instruction construtorUP, super class do not exists, ou sintaxe error in name class.", sequencia.tokens, escopo);
                return null;
            }
            if (!RepositorioDeClassesOO.Instance().ExisteClasse(nomeClasseHerdada))
            {
                UtilTokens.WriteAErrorMensage("internal error, instruction construtorUP, sub class do not exists, ou sintax error in code.", sequencia.tokens, escopo);
                return null;
            }



            int indexStartParametros = sequencia.tokens.IndexOf("(");
            if (indexStartParametros == 1)
            {
                UtilTokens.WriteAErrorMensage("internal error in instruction construtorUP, sintaxe error, without parentesis initial or final.", sequencia.tokens, escopo);
                return null;
            }

            List<string> tokensParametros = sequencia.tokens.GetRange(indexStartParametros, sequencia.tokens.Count - indexStartParametros);
            List<Expressao> expressoesParametros = null;

            if ((tokensParametros != null) && (tokensParametros.Count > 0))
            {
                tokensParametros.Remove(";");
                tokensParametros.Remove(","); // remove da lista de parâmetros, o primeiro token ",", pois faz parte da especificação do token da classe herdada.
                tokensParametros.RemoveAt(0);
                tokensParametros.RemoveAt(tokensParametros.Count - 1);


                tokensParametros.Remove(nomeClasseHerdada);  // remove da lista de parâmetros, o token do nome da classe herdada..


                expressoesParametros = Expressao.ExtraiExpressoes(tokensParametros, escopo);
                if (expressoesParametros == null)
                {
                    UtilTokens.WriteAErrorMensage("" + "instruction construtorUP,  internal error in parameters expressions, in contrutorUP instruction", sequencia.tokens, escopo);
                    return null;
                }
            }
            else
                expressoesParametros = new List<Expressao>(); // sem parametros para passar ao construtor.

            int indexConstrutor = FoundACompatibleConstructor(nomeClasseHerdada, expressoesParametros);
            if (indexConstrutor < 0)
            {
                UtilTokens.WriteAErrorMensage("instruction constructor up, not found super class constructor, not match with types of parameters.",
                    sequencia.tokens, escopo);
                return null;
            }


            Expressao pacoteParametros = new Expressao();
            pacoteParametros.Elementos.AddRange(expressoesParametros);

            Expressao expressaoCabecalho = new Expressao();
            expressaoCabecalho.Elementos.Add(new ExpressaoElemento(nomeClasseHedeira));
            expressaoCabecalho.Elementos.Add(new ExpressaoElemento(nomeClasseHerdada));
            expressaoCabecalho.Elementos.Add(new ExpressaoElemento(indexConstrutor.ToString()));
            expressaoCabecalho.Elementos.Add(pacoteParametros);

            escopo.tabela.AdicionaExpressoes(escopo, expressoesParametros.ToArray()); // adiciona as expressoes-parametros, para fins de otimização.

            Instrucao instrucaoConstrutorUP = new Instrucao(ProgramaEmVM.codeConstructorUp, expressaoCabecalho.Elementos, new List<List<Instrucao>>());
            return instrucaoConstrutorUP;
        }


        /// <summary>
        /// Cria um novo objeto. pode criar um objeto simples, ou um objeto de variavel vetor.
        /// </summary>
        /// Estrutura de dados na lista de expressoes:
        /// 0- id "create".
        /// 1- tipo do objeto.
        /// 2- nome do objeto.
        /// 3- reservado.
        /// 4- Lista de expressoes-indice para objetos vetor.
        /// 5- lista de expressoes parametros, para o construtor.
        protected Instrucao BuildInstrucaoCreate(UmaSequenciaID sequencia, Escopo escopo)
        {


            /// EXPRESSOES: a lista de expressões da instrução foi feita na seguinte sequência:
            /// 0- NOME "create"
            /// 1- tipo do objeto
            /// 2- nome do objeto
            /// 3- tipo template do objeto: Objeto/Vetor.
            /// 4- expressoes indices vetor.
            /// 5- expressoes parametros.
            /// 6- indice do construtor.
            /// 7- nome do objeto caller.


            string tipoDoObjetoAReceberAInstantiacao = "";
            string nomeDoObjetoAReceberAInstanciacao = "";

            /// ID ID = create ( ID , ID ) --> exemplo: int m= create(1,1).
            if (RepositorioDeClassesOO.Instance().ExisteClasse(sequencia.tokens[0]))
            {
                tipoDoObjetoAReceberAInstantiacao = sequencia.tokens[0].ToString();
                nomeDoObjetoAReceberAInstanciacao = sequencia.tokens[1];

            }
            else
            {
                nomeDoObjetoAReceberAInstanciacao = sequencia.tokens[0];
                Objeto objetoJaInicializado = escopo.tabela.GetObjeto(nomeDoObjetoAReceberAInstanciacao, escopo);
                if (objetoJaInicializado != null)
                {
                    tipoDoObjetoAReceberAInstantiacao = objetoJaInicializado.GetTipo();
                }


            }


            ValidaTokensDaSintaxeDaInstrucao(sequencia, escopo);


            int indexFirstParenteses = sequencia.tokens.IndexOf("(");
            if (indexFirstParenteses == -1)
            {
                UtilTokens.WriteAErrorMensage("error in create instruction: none parentesis operator", sequencia.tokens, escopo);
                return null;
            }

            // obtem os tokens dos parametros.
            List<string> tokensParametros = UtilTokens.GetCodigoEntreOperadores(indexFirstParenteses, "(", ")", sequencia.tokens);
            if ((tokensParametros == null) || (tokensParametros.Count < 2))
            {
                UtilTokens.WriteAErrorMensage("error in create instruction: bad format in parameters", sequencia.tokens, escopo);
                return null;
            }

            tokensParametros.RemoveAt(0);
            tokensParametros.RemoveAt(tokensParametros.Count - 1);


            // EXTRAI OS PARAMETROS da instrução.
            List<Expressao> expressoesParametros = Expressao.ExtraiExpressoes(tokensParametros, escopo);
            if ((expressoesParametros == null) || (expressoesParametros.Count == 0) || ((expressoesParametros[0].Elementos != null) && (expressoesParametros[0].Elementos.Count == 0)))
            {
                expressoesParametros = new List<Expressao>();
            }




            int indexConstrutor = FoundACompatibleConstructor(tipoDoObjetoAReceberAInstantiacao, expressoesParametros);
            if (indexConstrutor < 0)
            {
                UtilTokens.WriteAErrorMensage("internal error, in instruction create, not found a compatible constructor to instantiate the object: " + nomeDoObjetoAReceberAInstanciacao + ".", sequencia.tokens, escopo);
                return null;
            }

            Objeto novoObjetoInstanciado = new Objeto();
            // tipo da variável: Objeto.
            if (escopo.tabela.GetObjeto(nomeDoObjetoAReceberAInstanciacao, escopo) == null)
            {

                novoObjetoInstanciado = new Objeto("private", tipoDoObjetoAReceberAInstantiacao, nomeDoObjetoAReceberAInstanciacao, null);

                escopo.tabela.GetObjetos().Add(novoObjetoInstanciado); // se o objeto não já foi criado, instancia o objeto, no escopo.
            }




            Expressao exprssDaIntrucao = new Expressao();
            Expressao parametros = new Expressao();
            parametros.Elementos.AddRange(expressoesParametros);


            exprssDaIntrucao.Elementos.Add(new ExpressaoElemento("create"));
            exprssDaIntrucao.Elementos.Add(new ExpressaoElemento(tipoDoObjetoAReceberAInstantiacao));
            exprssDaIntrucao.Elementos.Add(new ExpressaoElemento(nomeDoObjetoAReceberAInstanciacao));
            exprssDaIntrucao.Elementos.Add(new ExpressaoElemento("Objeto"));
            exprssDaIntrucao.Elementos.Add(new ExpressaoElemento("reservado para vetor"));
            exprssDaIntrucao.Elementos.Add(parametros);
            exprssDaIntrucao.Elementos.Add(new ExpressaoElemento(indexConstrutor.ToString()));
            exprssDaIntrucao.Elementos.Add(new ExpressaoElemento(novoObjetoInstanciado.GetNome()));




            // registra as expressões, a fim de otimização de modificação.
            if (expressoesParametros.Count > 0)
                escopo.tabela.AdicionaExpressoes(escopo, expressoesParametros.ToArray());

            // cria a instrucao do objeto.
            Instrucao instrucaoCreate = new Instrucao(ProgramaEmVM.codeCreateObject, new List<Expressao>() { exprssDaIntrucao }, new List<List<Instrucao>>());
            return instrucaoCreate;

        } // BuildInstrucaoCreate()





        protected bool ValidaTokensDaSintaxeDaInstrucao(UmaSequenciaID sequencia, Escopo escopo)
        {
            int indexSignalEquals = sequencia.tokens.IndexOf("=");
            if (indexSignalEquals == -1)
            {
                UtilTokens.WriteAErrorMensage("internal error in instruction create, not found operator equals for instantiation.", sequencia.tokens, escopo);
                return false;
            } // if


            int indexParenteses = sequencia.tokens.IndexOf("(");
            if (indexParenteses == -1)
            {
                UtilTokens.WriteAErrorMensage("internal error in instruction create, not found parentesis, sintaxe error.", sequencia.tokens, escopo);
                return false;
            } // if
            return true;
        } // ValidaTokensDaSintaxeDaInstrucao()


        private static int FoundACompatibleConstructor(string tipoObjeto, List<Expressao> parametros)
        {
            if ((tipoObjeto == null) || (tipoObjeto == ""))
            {
                return -1;
            }
            Classe classeComConstrutores = RepositorioDeClassesOO.Instance().GetClasse(tipoObjeto);

            if (classeComConstrutores == null)
            {
                return -1;
            }

            List<Metodo> construtores = classeComConstrutores.construtores;
            int contadorConstrutores = 0;

            if ((parametros == null) || (parametros.Count == 0))
            {
                int indexConstrutorSemParametros = construtores.FindIndex(k => k.parametrosDaFuncao == null || k.parametrosDaFuncao.Length == 0);
                return indexConstrutorSemParametros;

            }
            int x_parametros = 0;
            foreach (Metodo umConstrutor in construtores)
            {

                if (umConstrutor.parametrosDaFuncao == null)
                {
                    if ((parametros == null) || (parametros.Count == 0) || (parametros[x_parametros].Elementos.Count == 0))
                        return contadorConstrutores;
                }
                else
                if (parametros.Count != umConstrutor.parametrosDaFuncao.Length)
                    continue;

                bool isFoundConstrutor = true;
                for (int x = 0; x < parametros.Count; x++)
                {

                    if (parametros[x].tipoDaExpressao == umConstrutor.parametrosDaFuncao[x].GetTipo())
                    {
                        continue;
                    }
                    else
                    {
                        isFoundConstrutor = false;
                        break;
                    }


                }
                if (isFoundConstrutor)
                    return contadorConstrutores;

                contadorConstrutores++;
            }

            return -1;
        }



        protected Instrucao BuildInstrucaoSetVar(UmaSequenciaID sequencia, Escopo escopo)
        {
            // template: 
            // SetVar ( ID , ID)

            if ((sequencia == null) || (sequencia.tokens.Count == 0))
                return null;
            if (sequencia.tokens[0] != "SetVar")
                return null;
            string nomeVar = sequencia.tokens[2];
            string valorVar = sequencia.tokens[4];

            Objeto v = escopo.tabela.GetObjeto(nomeVar, escopo);
            if (v == null)
                return null;
            Expressao expressaoNumero = new Expressao(new string[] { valorVar }, escopo);
            ProcessadorDeID.SetValorNumero(v, expressaoNumero, escopo);

            Expressao exprss = new Expressao(sequencia.tokens.ToArray(), escopo);

            // adiciona a expressao para a lista de expressoes do escopo, para fins de otimização.
            escopo.tabela.AdicionaExpressoes(escopo, exprss);

            Instrucao instrucaoSet = new Instrucao(ProgramaEmVM.codeSetObjeto, new List<Expressao>() { exprss }, new List<List<Instrucao>>());
            return instrucaoSet;
        }


        protected Instrucao BuildInstrucaoGetObjeto(UmaSequenciaID sequencia, Escopo escopo)
        {
            // template: 
            // ID GetObjeto ( ID )

            if ((sequencia == null) || (sequencia.tokens.Count == 0))
                return null;
            if (sequencia.tokens[1] != "GetObjeto")
                return null;

            string nomeVar = sequencia.tokens[3];
            Objeto v = escopo.tabela.GetObjeto(nomeVar, escopo);
            if (v == null)
            {
                UtilTokens.WriteAErrorMensage("internal error in instruction GetObjeto, object not found.", sequencia.tokens, escopo);
                return null;
            }
            Expressao expressaoGetObjeto = new Expressao(sequencia.tokens.ToArray(), escopo);

            // adiciona a expressao para a lista de expressoes, para fins de otimizações.
            escopo.tabela.AdicionaExpressoes(escopo, expressaoGetObjeto);

            Instrucao instrucaoGet = new Instrucao(ProgramaEmVM.codeGetObjeto, new List<Expressao>() { expressaoGetObjeto }, new List<List<Instrucao>>());
            return instrucaoGet;
        }

        protected Instrucao BuildInstrucaoWhile(UmaSequenciaID sequencia, Escopo escopo)
        {
            List<Expressao> expressoesWhile = null;
            /// while (exprss) bloco .
            if (sequencia.tokens[0] == "while")
            {
                int indexStartExpressionCondicional = sequencia.tokens.IndexOf("(");

                if (indexStartExpressionCondicional == -1)
                {
                    UtilTokens.WriteAErrorMensage("internal error in instruction while, not found a conditional expression.", sequencia.tokens, escopo);
                    return null;
                }
                List<string> tokensExpressaoCondicional = UtilTokens.GetCodigoEntreOperadores(indexStartExpressionCondicional, "(", ")", sequencia.tokens);
                if ((tokensExpressaoCondicional == null) || (tokensExpressaoCondicional.Count == 0))
                {
                    UtilTokens.WriteAErrorMensage("internal error in instruction while, not found a conditional expression.", sequencia.tokens, escopo);
                    return null;
                }
                tokensExpressaoCondicional.RemoveAt(0);
                tokensExpressaoCondicional.RemoveAt(tokensExpressaoCondicional.Count - 1);


                expressoesWhile = Expressao.ExtraiExpressoes(tokensExpressaoCondicional, escopo);

                if ((expressoesWhile == null) || (expressoesWhile.Count == 0))
                {
                    UtilTokens.WriteAErrorMensage("internal error in instruction while, bad formation in conditional expression.", sequencia.tokens, escopo);
                    return null;
                }

                if (!Expressao.Instance.ValidaExpressaoCondicional(expressoesWhile[0], escopo))   // valida se a expressão contém um operador operacional.
                {

                    UtilTokens.WriteAErrorMensage("internal error in instruction while, bad formation in conditional control expression.", sequencia.tokens, escopo);
                    return null;
                }

                escopo.tabela.AdicionaExpressoes(escopo, expressoesWhile.ToArray()); // registra a expressão na lista de expressões do escopo currente.

                ProcessadorDeID processador = null;
                Instrucao instrucaoWhile = new Instrucao(ProgramaEmVM.codeWhile, new List<Expressao>() { expressoesWhile[0] }, new List<List<Instrucao>>());
                BuildBloco(0, sequencia.tokens, ref escopo, instrucaoWhile, ref processador); // constroi as instruções contidas num bloco.

                return instrucaoWhile;

            } // if
            return null;
        } // InstrucaoWhileSemBloco()

        protected Instrucao BuildInstrucaoFor(UmaSequenciaID sequencia, Escopo escopo)
        {

            // for (int x=0; x<3; x++){ }; 
            if ((sequencia.tokens[0] == "for") && (sequencia.tokens.IndexOf("(") != -1))
            {



                List<string> tokensDaInstrucao = sequencia.tokens.ToList<string>();
                tokensDaInstrucao.RemoveAt(0); // remove o termo-chave: "for"


                List<string> tokensExpressoes = UtilTokens.GetCodigoEntreOperadores(0, "(", ")", tokensDaInstrucao);
                tokensExpressoes.RemoveAt(0);
                tokensExpressoes.RemoveAt(tokensExpressoes.Count - 1);



                Objeto variavelMalha = null;
                int indexClasseVariavelMalha = sequencia.tokens.IndexOf("=");
                if ((indexClasseVariavelMalha - 2) >= 0) // verifica se a variavel de malha é definida dentro da instrução for.
                {
                    int indexFirstComma = sequencia.tokens.IndexOf(";");
                    if (RepositorioDeClassesOO.Instance().ExisteClasse(sequencia.tokens[indexClasseVariavelMalha - 2]))
                    {
                        string tipoDaVariavelMalha = sequencia.tokens[indexClasseVariavelMalha - 2];
                        string nomeVariavelMalha = sequencia.tokens[indexClasseVariavelMalha - 1]; // nome da variavel de malha.
                        string valorVariavelMalha = sequencia.tokens[indexClasseVariavelMalha + 1]; // consegue o valor inicial da variavel de malha.
                        variavelMalha = new Objeto("private", tipoDaVariavelMalha, nomeVariavelMalha, valorVariavelMalha);

                        escopo.tabela.GetObjetos().Add(variavelMalha);
                    }
                }

                // obtem as expressoes da instrução "for".
                List<Expressao> expressoesDaInstrucaoFor = Expressao.ExtraiExpressoes(tokensExpressoes, escopo);





                if ((expressoesDaInstrucaoFor == null) || (expressoesDaInstrucaoFor.Count == 0))
                {
                    UtilTokens.WriteAErrorMensage("internal error in instruction for, not found expressions of instruction", sequencia.tokens, escopo);
                    return null;
                }

                if (expressoesDaInstrucaoFor.Count == 1)
                {
                    // houve um nao processamento de todas expressoes, pois a instanciacao da  variavel de malha esta entre as expressoes. Faz
                    // o processamento da instanciacao da variavel de malha, e extrai as expressoes novamente.
                    ProcessadorDeID processadorVariavelDaMalha = new ProcessadorDeID(new List<string>() { expressoesDaInstrucaoFor[0].ToString() });
                    processadorVariavelDaMalha.Compilar();
                    expressoesDaInstrucaoFor = Expressao.ExtraiExpressoes(tokensExpressoes, escopo);
                }



                if (RepositorioDeClassesOO.Instance().ExisteClasse(expressoesDaInstrucaoFor[0].Elementos[0].ToString()))
                {
                    // se a Objeto malha for definida na instrucao for, extrai a Objeto e adiciona no escopo esta Objeto.
                    // as expressoes posteriorees da instrucao for utilizam esta Objeto, ela já foi registrada.
                    Classe tipoDaObjeto = RepositorioDeClassesOO.Instance().GetClasse(expressoesDaInstrucaoFor[0].Elementos[0].ToString());
                    string nomeObjeto = expressoesDaInstrucaoFor[0].Elementos[1].ToString();
                    object valorObjeto = expressoesDaInstrucaoFor[0].Elementos[3].ToString();

                    escopo.tabela.GetObjetos().Add(new Objeto("private", tipoDaObjeto.GetNome(), nomeObjeto, valorObjeto));
                }

                if (!Expressao.Instance.IsExpressionAtibuicao(expressoesDaInstrucaoFor[0])) // valida a expressao de atribuicao
                {
                    UtilTokens.WriteAErrorMensage("internal error in atribution expression in instruction for", expressoesDaInstrucaoFor[0].tokens, escopo);
                }



                if (!Expressao.Instance.ValidaExpressaoCondicional(expressoesDaInstrucaoFor[1], escopo)) // valida a expressão de controle condicional.
                {
                    UtilTokens.WriteAErrorMensage("internal error in conditional expressin in instruction for " + expressoesDaInstrucaoFor[1].ToString() + "is not a valid expression", sequencia.tokens, escopo);
                }

                if (!Expressao.Instance.IsExpressaoAritimeticoUnario(expressoesDaInstrucaoFor[2], escopo)) // valida a expressão de incremento/decremento.
                {
                    UtilTokens.WriteAErrorMensage("internal error in increment expression, not valid expression: " + expressoesDaInstrucaoFor[2].ToString(), sequencia.tokens, escopo);
                }


                // registra as expressões no escopo, para fins de otimização.
                for (int x = 0; x < 3; x++)
                    escopo.tabela.AdicionaExpressoes(escopo, expressoesDaInstrucaoFor[x]);

                Instrucao instrucaoFor = null;
                int offsetIndexBloco = sequencia.tokens.FindIndex(k => k == "{"); // calcula se há um token de operador bloco abre.
                if (offsetIndexBloco == -1)
                {
                    UtilTokens.WriteAErrorMensage("must provide a instructions block for expression for, uses { } operator for definition a block, include in none block instrutions.", sequencia.tokens, escopo);
                    return null;
                }
                else
                {
                    ProcessadorDeID processador = null;
                    instrucaoFor = new Instrucao(ProgramaEmVM.codeFor, expressoesDaInstrucaoFor, new List<List<Instrucao>>()); // cria a instrucao for principal.
                    BuildBloco(0, sequencia.tokens, ref escopo, instrucaoFor, ref processador); // adiciona as instruções do bloco.

                    instrucaoFor.expressoes = new List<Expressao>();

                    // adiciona as expressoes do "for" para a instrução VM "for".
                    for (int i = 0; i < expressoesDaInstrucaoFor.Count; i++)
                        instrucaoFor.expressoes.Add(expressoesDaInstrucaoFor[i]);


                    return instrucaoFor;

                } //if
            } // if
            return null;
        } // InstrucaoWhileSemBloco()

        protected Instrucao BuildInstrucaoIFsComOuSemElse(UmaSequenciaID sequencia, Escopo escopo)
        {

            /// while (exprss) {} .
            if (sequencia.tokens[0] == "if")
            {
                int indexInitTokens = sequencia.tokens.IndexOf("(");
                List<string> tokensDeExpressoes = UtilTokens.GetCodigoEntreOperadores(indexInitTokens, "(", ")", sequencia.tokens);
                if ((tokensDeExpressoes == null) || (tokensDeExpressoes.Count < 2))
                {
                    UtilTokens.WriteAErrorMensage("instruction if, bad sintax for conditional expression", sequencia.tokens, escopo);
                    return null;
                }
                tokensDeExpressoes.RemoveAt(0);
                tokensDeExpressoes.RemoveAt(tokensDeExpressoes.Count - 1);

                Expressao expressoesIf = new Expressao(tokensDeExpressoes.ToArray(), escopo);


                // valida se há expressões validas para a instrução.
                if (expressoesIf == null)
                {

                    UtilTokens.WriteAErrorMensage("erro de sintaxe da instrução if. ", sequencia.tokens, escopo);
                    return null;
                }
                // valida a expressão de atribuição da instrução "if".
                if ((expressoesIf == null) || (expressoesIf.Elementos.Count == 0))
                {
                    UtilTokens.WriteAErrorMensage("instruction if,  internal error, control expression is in bad formation.", sequencia.tokens, escopo);
                    return null;
                }// valida se a expressão contém um operador operacional.
                if (!Expressao.Instance.ValidaExpressaoCondicional(expressoesIf, escopo))
                {
                    UtilTokens.WriteAErrorMensage("instruction if, internal error, control expression is not a conditional expression:  " + Util.UtilString.UneLinhasLista(expressoesIf.Convert()), sequencia.tokens, escopo);
                    return null;
                }


                // adiciona a expressão codicional.
                escopo.tabela.AdicionaExpressoes(escopo, expressoesIf);



                // offset para o primeiro token de bloco.
                int offsetBlocoIf = sequencia.tokens.IndexOf("{");
                // se não for uma instrução com bloco, é uma instrução sem bloco, retornando null, pois a instrucao nao foi construida.
                if (offsetBlocoIf == -1)
                {
                    return null;
                }



                ProcessadorDeID processador = null;

                int offsetBlocoElse = sequencia.tokens.IndexOf("{", offsetBlocoIf + 1);
                if (offsetBlocoElse == -1) // instrução if sem bloco de uma instrução else.
                {

                    Instrucao instrucaoIfSemElse = new Instrucao(ProgramaEmVM.codeIfElse, new List<Expressao>() { expressoesIf }, new List<List<Instrucao>>());
                    BuildBloco(0, sequencia.tokens, ref escopo, instrucaoIfSemElse, ref processador);

                    return instrucaoIfSemElse; // ok , é um comando if sem instrução else.
                } // if
                else // instrução if com bloco de uma instrução else.
                {

                    // CONSTRUCAO DO BLOCO DE IF.
                    List<string> tokensIf = UtilTokens.GetCodigoEntreOperadores(offsetBlocoIf, "{", "}", sequencia.tokens);
                    if ((tokensIf == null) || (tokensIf.Count < 2))
                    {
                        UtilTokens.WriteAErrorMensage("sintax error in if block", sequencia.tokens, escopo);
                        return null;
                    }


                    // obtem o indice do bloco else.
                    offsetBlocoElse = sequencia.tokens.IndexOf("{", offsetBlocoIf + tokensIf.Count);
                    // CONSTRUCAO DE BLOCO DE ELSE;
                    List<string> tokensElse = UtilTokens.GetCodigoEntreOperadores(offsetBlocoElse, "{", "}", sequencia.tokens);
                    if ((tokensElse == null) || (tokensElse.Count < 2))
                    {
                        UtilTokens.WriteAErrorMensage("sintax error in else block", sequencia.tokens, escopo);
                        return null;
                    }

                    Instrucao instrucaoElse = new Instrucao(ProgramaEmVM.codeIfElse, new List<Expressao>() { expressoesIf }, new List<List<Instrucao>>());

                    // constroi o bloco da instrução else.
                    BuildBlocoIfElse(0, tokensIf, ref escopo, instrucaoElse, ref processador);
                    BuildBlocoIfElse(1, tokensElse, ref escopo, instrucaoElse, ref processador);
                    return instrucaoElse;
                } // else
            } // if
            return null;
        } // BuildInstrucaoIFsComOuSemElse

        protected Instrucao BuildInstrucaoCasesOfUse(UmaSequenciaID sequencia, Escopo escopo)
        {

            // sintaxe: "casesOfUse y: { (case < x): { y = 2; }; } ";
            int iCabecalho = sequencia.tokens.IndexOf("(");
            if (iCabecalho == -1)
            {
                UtilTokens.WriteAErrorMensage("instruction casesOfUse sintaxe error, not found parentesis", sequencia.tokens, escopo);
                return null;
            }

            // obtem as listas de cases, cada um contendo o bloco de um item case.
            List<List<List<string>>> listaDeCases = UtilTokens.GetCodigoEntreOperadoresCases(sequencia.tokens);

            // obtem a variavel principal, e valida.
            string nomeObjetoPrincipal = sequencia.tokens[1];
            Objeto vMain = escopo.tabela.GetObjeto(nomeObjetoPrincipal, escopo);
            if (vMain == null)
            {
                UtilTokens.WriteAErrorMensage("variavel principal: " + nomeObjetoPrincipal + " não definida.", sequencia.tokens, escopo);
                return null;
            } // if


            List<Expressao> expressaoHeaderDeCadaCase = new List<Expressao>();
      
            List<List<Instrucao>> blocoDeInstrucoesCase = new List<List<Instrucao>>(); // inicializa as listas de blocos de instrução.


            // percorre as listas, calculando: 1- a expressão condicional do case, 2- o bloco de instruções para o case.
            for (int UM_CASE = 0; UM_CASE < listaDeCases.Count; UM_CASE++)
            {
                // verifica se o case tem tokens suficientes.
                if (listaDeCases[UM_CASE][0].Count<5)
                {
                    UtilTokens.WriteAErrorMensage("error in cases of use instruction, index case: " + UM_CASE, sequencia.tokens, escopo);
                    return null;
                }

                int indexCabecalhoCase = listaDeCases[UM_CASE][0].IndexOf("(");
                string nameOperatorUM_CASE = listaDeCases[UM_CASE][0][indexCabecalhoCase + 2];

                // se o operador não pertencer ao tipo do objeto principal, retorna null.
                if (RepositorioDeClassesOO.Instance().GetClasse(vMain.GetTipo()).operadores.Find(k => k.nome == nameOperatorUM_CASE) == null)
                {
                    UtilTokens.WriteAErrorMensage("operator: " + nameOperatorUM_CASE + " not found at class: " + vMain.GetTipo() + ", of " + vMain.GetNome() + "object", sequencia.tokens, escopo);
                    return null;
                }


                // indice do comeco da expressao de comparacao (> expressoComparacao)
                int indexExpressaoHeader = indexCabecalhoCase + 3;

                List<string> tokensExprssHeader = listaDeCases[UM_CASE][0].GetRange(indexExpressaoHeader, listaDeCases[UM_CASE][0].Count - indexExpressaoHeader - 1); //-1 do fechamento com parenteses fecha.
                Expressao exprssDeComparacaoUM_CASE = null;
                if ((tokensExprssHeader != null) && (tokensExprssHeader.Count > 0))
                {
                    exprssDeComparacaoUM_CASE = new Expressao(tokensExprssHeader.ToArray(), escopo);
                    if (exprssDeComparacaoUM_CASE == null)
                    {
                        UtilTokens.WriteAErrorMensage("error in expression of instruction casesOfUse", sequencia.tokens, escopo);
                        return null;

                    }
                }
                else
                {
                    UtilTokens.WriteAErrorMensage("error in expression of instruction casesOfUse", sequencia.tokens, escopo);
                    return null;

                }


                //______________________________________________________________________________________________

                List<string> tokensCorpoDoCase = listaDeCases[UM_CASE][1];
                if (tokensCorpoDoCase[0] == "{")
                {
                    tokensCorpoDoCase.RemoveAt(0);
                    tokensCorpoDoCase.RemoveAt(tokensCorpoDoCase.Count - 1);
                }

                ProcessadorDeID compilador = new ProcessadorDeID(tokensCorpoDoCase);
                compilador.Compilar();
                if ((compilador.GetInstrucoes() == null) || (compilador.GetInstrucoes().Count == 0))
                {
                    UtilTokens.WriteAErrorMensage("error in instructions of body case, of casesOfUse", sequencia.tokens, escopo);
                    return null;
                }


                // adiciona as instrucoes do corpo do case currente.
                blocoDeInstrucoesCase.Add(compilador.GetInstrucoes());

               
                // constroi a expressao de comparacao, sem processamento, pois os valores nao podem ser calculados em tempo de compilacao.
                List<string> tokensExprssCondicionalDeUM_CASE = new List<string>();
                tokensExprssCondicionalDeUM_CASE.Add(vMain.GetNome());
                tokensExprssCondicionalDeUM_CASE.Add(nameOperatorUM_CASE);
                tokensExprssCondicionalDeUM_CASE.AddRange(exprssDeComparacaoUM_CASE.tokens);


                // forma a expressao mas sem o processamento, pois os valores mudam conforme o codigo e executado.
                Expressao exprssCompletaCondicional = new Expressao();
                exprssCompletaCondicional.tokens.AddRange(tokensExprssCondicionalDeUM_CASE);
                
                expressaoHeaderDeCadaCase.Add(exprssCompletaCondicional);

           
           

                // formato:
                /// exprss[umCaseIndex]:a expressao condicional do case.
                /// instrucao[umCaseIndex]: instrucoes.

            }

            Instrucao instrucaoCase = new Instrucao(ProgramaEmVM.codeCasesOfUse, expressaoHeaderDeCadaCase, blocoDeInstrucoesCase);
            return instrucaoCase;


        } // BuildInstrucaoCasesOfUse(()

        private static void ObtemNumeroOuTextoDeControle(string nameVarCase, ref object numero, ref object str_string, ref Objeto objetoCase, Escopo escopo)
        {
            // é o caso em que a variavel do case é um numero, ou string: caseOfUse a { case == 1: x++; case  < 5: y++;}
            if (Expressao.Instance.IsTipoInteiro(nameVarCase))
            {
                numero = int.Parse(nameVarCase);
                objetoCase = new Objeto("private", "int", "varCaseInt", numero);
            }
            else
            if (Expressao.Instance.IsTipoFloat(nameVarCase))
            {
                numero = float.Parse(nameVarCase);
                objetoCase = new Objeto("private", "float", "varCaseFloat", numero);
            }
            else
            if (Expressao.Instance.IsTipoDouble(nameVarCase))
            {
                numero = float.Parse(nameVarCase);
                objetoCase = new Objeto("private", "double", "varCaseFloat", numero);
            }
            else

            if (linguagem.VerificaSeEString(nameVarCase))
            {
                str_string = nameVarCase;
                objetoCase = new Objeto("private", "string", "varCaseString", str_string);
            }
        }


        /// <summary>
        /// constroi blocos de if/else,
        /// </summary>
        /// <param name="numeroDoBloco">indice do bloco.</param>
        /// <param name="tokens">tokens de if, ou tokens de else.</param>
        /// <param name="escopoDoMetodo">escopo da instrucao.</param>
        /// <param name="instrucaoPrincipal">instrucao if/else.</param>
        /// <param name="processadorBloco">compilador.</param>
        protected void BuildBlocoIfElse(int numeroDoBloco, List<string> tokens, ref Escopo escopoDoMetodo, Instrucao instrucaoPrincipal, ref ProcessadorDeID processadorBloco)
        {





            // remove os operadores bloco dos tokens do bloco.
            tokens.RemoveAt(0);
            tokens.RemoveAt(tokens.Count - 1);

            Escopo escopoBloco = new Escopo(tokens);

            processadorBloco = new ProcessadorDeID(tokens);
            processadorBloco.escopo.tabela = TablelaDeValores.Clone(escopoDoMetodo.tabela); // copia a tabela de valores do escopo currente.

            UtilTokens.LinkEscopoPaiEscopoFilhos(escopoDoMetodo, processadorBloco.escopo);
            processadorBloco.Compilar(); // faz a compilacao do bloco.


            List<Instrucao> instrucoesBLOCO = processadorBloco.GetInstrucoes();
            instrucaoPrincipal.blocos.Insert(numeroDoBloco, instrucoesBLOCO);

            List<Objeto> objetosDoBloco = processadorBloco.escopo.tabela.GetObjetos();

            // REPASSA PARA O ESCOPO DO METODO, OU ESCOPO LOGO ACIMA, OS OBJETOS FORMADO DENTRO DO BLOCO.
            if ((objetosDoBloco != null) && (objetosDoBloco.Count > 0))
            {
                for (int i = 0; i < objetosDoBloco.Count; i++)
                {
                    if (escopoDoMetodo.tabela.GetObjeto(objetosDoBloco[i].nome, escopoDoMetodo) == null)
                    {
                        escopoDoMetodo.tabela.RegistraObjeto(objetosDoBloco[i]);
                    }
                }
            }

        }



        protected void BuildBloco(int numeroDoBloco, List<string> tokens, ref Escopo escopoDoMetodo, Instrucao instrucaoPrincipal, ref ProcessadorDeID processadorBloco)
        {


            if ((!tokens.Contains("{")) || (!tokens.Contains("}")))
                return;

            int indexStart = 0;
            int offsetStart = 0;
            for (int x = 0; x <= numeroDoBloco; x++)
            {
                indexStart = tokens.IndexOf("{", offsetStart);
                if (indexStart == -1)
                    break;


                List<string> blocoAnterior = UtilTokens.GetCodigoEntreOperadores(indexStart, "{", "}", tokens);
                offsetStart = blocoAnterior.Count;

            }

            List<string> bloco = UtilTokens.GetCodigoEntreOperadores(indexStart, "{", "}", tokens);


            bloco.RemoveAt(0); // remove os operadores bloco dos tokens do bloco.
            bloco.RemoveAt(bloco.Count - 1);

            Escopo escopoBloco = new Escopo(bloco);

            processadorBloco = new ProcessadorDeID(bloco);
            processadorBloco.escopo.tabela = TablelaDeValores.Clone(escopoDoMetodo.tabela); // copia a tabela de valores do escopo currente.

            UtilTokens.LinkEscopoPaiEscopoFilhos(escopoDoMetodo, processadorBloco.escopo);
            processadorBloco.Compilar(); // faz a compilacao do bloco.


            List<Instrucao> instrucoesBLOCO = processadorBloco.GetInstrucoes();
            instrucaoPrincipal.blocos.Add(instrucoesBLOCO);

        }


        protected Instrucao BuildInstrucaoBreak(UmaSequenciaID sequencia, Escopo escopo)
        {
            Instrucao instrucaoBreak = new Instrucao(ProgramaEmVM.codeBreak, new List<Expressao>(), new List<List<Instrucao>>());
            return instrucaoBreak;
        }

        protected Instrucao BuildInstrucaoContinue(UmaSequenciaID sequencia, Escopo escopo)
        {
            Instrucao instrucaoContinue = new Instrucao(ProgramaEmVM.codeContinue, new List<Expressao>(), new List<List<Instrucao>>());
            return instrucaoContinue;
        }

        protected Instrucao BuildInstrucaoReturn(UmaSequenciaID sequencia, Escopo escopo)
        {

            List<string> tokensExpressoes = sequencia.tokens.ToList<string>();
            tokensExpressoes.RemoveAt(0); // retira o nome da instrucao: token "return", para compor o corpo da expressão.

            List<Expressao> exprssRetorno = Expressao.ExtraiExpressoes(tokensExpressoes, escopo);
            Expressao exprssWRAPPER = new Expressao();
            exprssWRAPPER.Elementos.AddRange(exprssRetorno[0].Elementos);


            if ((exprssRetorno == null) || (exprssRetorno.Count == 0))
            {
                Instrucao instrucaoRetornoSemExpressao = new Instrucao(ProgramaEmVM.codeReturn, new List<Expressao>(), new List<List<Instrucao>>());
                return instrucaoRetornoSemExpressao;
            }
            else
            {
                escopo.tabela.AdicionaExpressoes(escopo, exprssWRAPPER); // adiciona a expressao para a lista de expressao do escopo, para fins de otimização.

                Instrucao instrucaoReturn = new Instrucao(ProgramaEmVM.codeReturn, new List<Expressao>() { exprssWRAPPER }, new List<List<Instrucao>>());
                return instrucaoReturn;
            }
        }



        public class Testes : SuiteClasseTestes
        {
            public Testes() : base("testes para instrucoes da linguagem orquidea.")
            {
            }


            public void TesteInstrucaoCasesOfUse(AssercaoSuiteClasse assercao)
            {

                // sintaxe: "casesOfUse y: { (case < x): { y = 2; }; } ";
                string varsCreate = "int x=1; int y=2;";
                string instrucaoCase = "casesOfUse y: { (case < x): { y = 2; } (case < x): { y = 2; } };";

                List<string> tokensTeste = new Tokens(varsCreate + instrucaoCase).GetTokens();
                ProcessadorDeID compilador = new ProcessadorDeID(tokensTeste);
                compilador.Compilar();

                try
                {
                    Instrucao instCase = compilador.GetInstrucoes()[2];
                    assercao.IsTrue(instCase.blocos.Count == 2 && instCase.expressoes.Count == 2, "casesOfUse y:  (case < x): { y = 2; };");
       
                }
                catch (Exception ex)
                {
                    string msgError = ex.Message;
                }
                

            }
        } // class

    }
} // namespace

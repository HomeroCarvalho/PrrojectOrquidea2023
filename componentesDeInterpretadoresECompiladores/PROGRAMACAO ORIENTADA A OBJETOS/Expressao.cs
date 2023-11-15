using MathNet.Numerics.LinearAlgebra.Complex;
using ModuloTESTES;
using parser.ProgramacaoOrentadaAObjetos;
using parser.textoFormatado;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

using Util;
using Wrappers.DataStructures;
using Vector = Wrappers.DataStructures.Vector;

namespace parser
{

    public class ExpressaoElemento: Expressao
    {
        public string elemento;

        public new string GetElemento()
        {
            return elemento;
        }

        public override string GetTipoExpressao()
        {
            return "no type, token of expression not definided";
        }

        public ExpressaoElemento(string caption)
        {
            this.elemento = caption;
            this.tokens = new List<string>() { this.elemento };
        }

        public override string ToString()
        {
            return elemento;
        }
    }

    public class ExpressaoEntreParenteses : ExpressaoObjeto
    {
        public Expressao exprssParenteses;

        public ExpressaoEntreParenteses(Expressao exprss_entre_parentes, Escopo escopo)
        {
        
            if ((exprss_entre_parentes==null) || (exprss_entre_parentes.tokens==null) || (exprss_entre_parentes.tokens.Count==0))
            {
                UtilTokens.WriteAErrorMensage("bad formation in ExpressaoEntreParenteses, method: constructor ExpressaoEntreParenteses", escopo.codigo, escopo);
                return;

            }
            this.exprssParenteses = exprss_entre_parentes;
            this.tipoDaExpressao = exprss_entre_parentes.tipoDaExpressao;
            this.tokens = new List<string>();
            if ((exprss_entre_parentes.tokens != null) && (exprss_entre_parentes.tokens.Count > 0))
            {
                this.tokens.AddRange(exprss_entre_parentes.tokens);
            }
            
        }

        public override string GetTipoExpressao()
        {
            return this.exprssParenteses.tipoDaExpressao;
        }

        public override string ToString()
        {
            if (this.exprssParenteses != null)
            {
                return this.exprssParenteses.ToString();
            }
            else
            {
                return "()";
            }
        }
    }







    public class ExpressaoNumero : ExpressaoObjeto
    {
        public string numero;


        public ExpressaoNumero()
        {
            numero = "";
        }

       
        public ExpressaoNumero(string valueNumber)
        {
            this.numero = valueNumber;
            this.tipoDaExpressao = GetTypeNumber(this.numero);
            this.tokens = new List<string>() { this.numero };
        }


        public override string GetTipoExpressao()
        {
            return GetTypeNumber(numero);
        }

      
  
        public override string ToString()
        {
            if (this.numero != null)
            {
                return this.numero;
            }
            else
            {
                return "number is not instantiate.";
            }
                
        }


        public static bool isNumero(string token)
        {
            int n_int;
            float n_float;
            double n_double;

            if (int.TryParse(token, out n_int))
                return true;
            else
            if (float.TryParse(token, out n_float))
                return true;
            else
            if (double.TryParse(token, out n_double))
                return true;

            else
                return false;

        }
        public static string GetTypeNumber(string numero)
        {
            int n_int;
            float n_float;
            double n_double;

            if (int.TryParse(numero, out n_int))
                return "int";
            else
            if ((numero.Contains("f")) && (float.TryParse(numero, out n_float)))
                return "float";
            else
            if (double.TryParse(numero, out n_double))
                return "double";

            else
                return "float";

        }
    }


    public partial class ExpressaoChamadaDeMetodo : Expressao
    {


        /// <summary>
        /// funcao, metodo que sera executado.
        /// </summary>
        public Metodo funcao;
        /// <summary>
        /// classe do objeto que faz a chamada, ao metodo desta mesma classe.
        /// </summary>
        public string classeDoMetodo;

        /// <summary>
        /// nome da funcao-metodo.
        /// </summary>
        public string nomeMetodo;


        /// <summary>
        /// nome do objeto que faz a chamada.
        /// </summary>
        public string nomeObjeto;



        /// <summary>
        /// tipo do retorno da chamada do metodo.
        /// </summary>
        public string tipoRetornoMetodo;


        /// <summary>
        /// parametros do metodo chamado.
        /// </summary>
        public List<Expressao> parametros;
        
        /// <summary>
        /// objeto que chama o metodo.
        /// </summary>
        public Objeto objectCaller;


        /// <summary>
        /// se true a funcao da expressao e um metodo-parametro.
        /// </summary>
        public bool isMethodParameter = false;
        /// <summary>
        /// construtor.
        /// </summary>
        /// <param name="obj_caller">objeto que chama o metodo.</param>
        /// <param name="metodo">funcao chamada.</param>
        /// <param name="parametros">parametros da chamada, parametros da funcao.</param>
        public ExpressaoChamadaDeMetodo(Objeto obj_caller, Metodo metodo, List<Expressao> parametros)
        {
            this.objectCaller = obj_caller.Clone();
            this.nomeObjeto = objectCaller.GetNome();
            this.nomeMetodo = metodo.nome;
            this.classeDoMetodo = metodo.nomeClasse;

            this.funcao = metodo;
            this.funcao.instrucoesFuncao = metodo.instrucoesFuncao;
            this.parametros = parametros;
            this.tipoRetornoMetodo = metodo.tipoReturn;
            
            if (objectCaller.isWrapperObject)
            {
                this.tipoDaExpressao = objectCaller.tipoElemento;
            }
            else
            {
                this.tipoDaExpressao = metodo.tipoReturn;
            }


            // tokens do objeto, operador dot, parenteses abre.
            this.tokens = new List<string>() { nomeObjeto, ".", nomeMetodo, "(" };

            // obtem os tokens de parametros.
            if ((parametros != null) && (parametros.Count > 0))
            {
                for (int i = 0; i < parametros.Count-1; i++)
                {
                    tokens.AddRange(parametros[i].tokens);
                    tokens.Add(",");

                }
                if (parametros.Count - 1 >= 0)
                {
                    tokens.AddRange(parametros[parametros.Count-1].tokens);

                }
            }
            // token do parenteses fecha.
            this.tokens.Add(")");
        }

     


        public override string GetTipoExpressao()
        {
            return this.tipoRetornoMetodo;
        }


        public override string ToString()
        {
            string str = "";
            str += nomeObjeto + "." + nomeMetodo;
            if ((parametros != null) || (parametros.Count == 0))
            {
                str += "()";
            }
            else
            {
                str += "(";
                for (int x = 0; x < parametros.Count - 1; x++)
                    str += parametros[x].ToString() + ",";
                str += parametros[parametros.Count - 1].ToString();

                str += ")";
            }

            return str;
        }

    }











    public class ExpressaoPropriedadesAninhadas : Expressao
    {

        public string propriedade;
        public Objeto objetoInicial;
        public string nomeObjeto;
        public string classeObjeto;

        public Objeto objectCaller;
        public Expressao expresaoAtribuicao;
        public List<Objeto> aninhamento = new List<Objeto>();
        public List<string> propriedadesAninhadas = new List<string>();
        public ExpressaoPropriedadesAninhadas(List<Objeto> aninhamento, List<string> propriedadesANINHADAS)
        {
            this.classeObjeto = aninhamento[0].GetTipo();
            this.nomeObjeto = aninhamento[0].GetNome();
            this.objectCaller = aninhamento[0];
            this.tipoDaExpressao = aninhamento[aninhamento.Count - 1].GetTipo();
            this.aninhamento = aninhamento;
            this.propriedadesAninhadas = propriedadesANINHADAS;
            for (int i = 0; i < aninhamento.Count; i++)
            {
                tokens.Add(aninhamento[i].GetNome());

                if (i < propriedadesAninhadas.Count)
                {
                    tokens.Add(".");
                    tokens.Add(propriedadesAninhadas[i]);
                }
                else
                {
                    tokens.Add(propriedadesAninhadas[i]);
                }

            }
        }

        public ExpressaoPropriedadesAninhadas()
        {
            this.classeObjeto = null;
            this.nomeObjeto = null;
            this.propriedade = null;

            this.objectCaller = null;
        }

        public override string ToString()
        {
            string str = "";
            if ((aninhamento == null) || (aninhamento.Count == 0))
            {
                return str;
            }

            for (int j = 0; j < aninhamento.Count; j++)
            {
                str += aninhamento[j].GetNome() + ".";
            }
            str = str.Remove(str.Length - 1);

            if ((Elementos == null) || (Elementos.Count == 0))
            {
                return str;
            }

            for (int j = 0; j < Elementos.Count; j++)
            {
                str += Elementos[j].ToString() + ".";
            }
            str = str.Remove(str.Length - 1);
            return str;
        }

    }

    public class ExpressaoAtribuicao : Expressao
    {
       
     

        /// <summary>
        /// expressao do valor a atribuir.
        /// </summary>
        public Expressao exprssAtribuicao;

        /// <summary>
        /// expressao que contem o objeto a receber a atribuicao.
        /// </summary>
        public Expressao exprssObjetoAAtribuir;

        /// <summary>
        /// expressao de atribuicao. contem uma expressao de objeto a atribuir, e uma expressao de atribuicao.
        /// </summary>
        /// <param name="exprssObjeto">expressao que contem o objeto a atribuir, podendo ser ExpressaoObjeto, ou ExpressaoAtribuir.</param>
        /// <param name="exprssAtribuicao">expressao que contem o valor a atribuir.</param>
        /// <param name="escopo">contexto onde as expressoes estão.</param>
        public ExpressaoAtribuicao(Expressao exprssObjeto, Expressao exprssAtribuicao, Escopo escopo)
        {
            this.exprssAtribuicao = exprssAtribuicao;
            this.exprssObjetoAAtribuir = exprssObjeto;
            this.tipoDaExpressao = exprssAtribuicao.tipoDaExpressao;

            this.tokens = new List<string>();
            if (exprssObjetoAAtribuir != null)
            {
                this.tokens = exprssObjetoAAtribuir.tokens.ToList<string>();
            }
            if (exprssAtribuicao != null)
            {
                this.tokens.AddRange(exprssAtribuicao.tokens);
            }
            
        }

        public override string ToString()
        {
            string str = "";
            
            if (exprssAtribuicao == null)
            {
                return str;
            }
            else
            {
                str += exprssAtribuicao.ToString();
            }
            

            return str;
        }
    }


    public class ExpressaoInstanciacao: Expressao
    {
        public string nomeObjeto;
        public string nomeClasseDoObjeto;
       
        public Expressao exprssAtribuicao;
        

        


        public ExpressaoInstanciacao(string nomeObjeto, string nomeClasse, Expressao exprssAtribuicao, Escopo escopo)
        {
            this.nomeClasseDoObjeto = nomeClasse;
            this.nomeObjeto= nomeObjeto;
            this.exprssAtribuicao= exprssAtribuicao;
            this.tipoDaExpressao = nomeClasse;

            // INSTANCIA O OBJETO DE INSTANCIAÇÃO, se não houver instanciado.
            if (escopo.tabela.GetObjeto(nomeObjeto, escopo) == null)
            {
                Objeto obj = new Objeto("public", nomeClasseDoObjeto, nomeObjeto, null);
                escopo.tabela.GetObjetos().Add(obj);
            }
            this.tokens = new List<string>() { nomeObjeto,".","="};
            this.tokens.AddRange(exprssAtribuicao.tokens);

            
        }

        public override string ToString()
        {
            string str = "";
            if (nomeObjeto == null)
            {
                return str;
            }
            str += nomeObjeto + "= create";
            if (exprssAtribuicao == null)
            {
                return str;
            }
            str += "(";
            str += exprssAtribuicao.ToString();
            str+= ")";

            return str;

        }
    }



    public class ExpressaoObjeto : Expressao
    {
        public string classeObjeto;
        public string nomeObjeto;

        public Objeto objectCaller;



        public List<string> classesParametroObjeto = new List<string>();



        public bool isFunction = false;

        public static int contadorNomes = 0;
       
        public ExpressaoObjeto()
        {

        }

        /// <summary>
        /// construtor APENAS PARA METODOS-PARAMETROS.
        /// </summary>
        /// <param name="objetoDaExpressao">objeto metodo-parameto.</param>
        /// <param name="classesDeMetodoParametro">classe em que o metodo-parametro, tem uma entrada.</param>
        public ExpressaoObjeto(Objeto objetoDaExpressao, List<string>classesDeMetodoParametro)
        {
            this.objectCaller = objetoDaExpressao;
            this.isFunction = true;
            this.tipoDaExpressao = objetoDaExpressao.tipo;
            this.classesParametroObjeto = classesDeMetodoParametro;
        }
      
        /// <summary>
        /// expressao contendo um objeto. 
        /// </summary>
        /// <param name="objetoDaExpressao">objeto da expressao.</param>
        public ExpressaoObjeto(Objeto objetoDaExpressao)
        {
            objectCaller = objetoDaExpressao;
             
            
            
            // caso que o objeto é um metodo.
            if (objetoDaExpressao is Metodo)
            {
                this.isFunction = true;
                
            }
            else
            {
                this.isFunction = false;
               
            }
            this.nomeObjeto = objetoDaExpressao.nome;
            classeObjeto = objetoDaExpressao.tipo;
            tipoDaExpressao = objetoDaExpressao.tipo;

            


            Elementos = new List<Expressao>();
            tokens = new List<string>() { objectCaller.GetNome() };
           
        }



 
        public override object GetElemento()
        {
            return objectCaller;
        }

        public override string ToString()
        {
            string str = "";
            if (nomeObjeto == null)
            {
                return "";
            }
            str += nomeObjeto;

            if ((Elementos != null) && (Elementos.Count > 0))
            {
                for (int x = 0; x < Elementos.Count; x++)
                {
                    str += Elementos[x].ToString() + " ";
                }
                    

            }

            return str;

        }

        public override string GetTipoExpressao()
        {
            return this.tipoDaExpressao;
        }
    }

    public class ExpressaoOperador : Expressao
    {

        /// <summary>
        ///  classe onde esta o operador.
        /// </summary>
        public string classeOperador;
        /// <summary>
        /// nome do operador.
        /// </summary>
        public string nomeOperador;
  




        public HeaderOperator.typeOperator typeOperandos = HeaderOperator.typeOperator.binary;

        /// <summary>
        /// operador da expressão.
        /// </summary>
        public Operador operador;
        /// <summary>
        /// primeiro operando.
        /// </summary>
        public Expressao operando1;
        /// <summary>
        /// segundo operando.
        /// </summary>
        public Expressao operando2;
        


        public ExpressaoOperador(Operador op)
        {
            if (op != null)
            {
                if (op.nomeClasse != null)
                {
                    this.classeOperador = op.nomeClasse;
                }
                if (op.nome != null)
                {
                    this.nomeOperador = op.nome;
                }
                if (op != null)
                {
                    this.operador = op;
                }
                
                if (op.nomeClasse != null)
                {
                    this.tipoDaExpressao = op.nomeClasse;
                }

                if (nomeOperador != null)
                {
                    this.tokens = new List<string>() { nomeOperador };
                }
                
            }
            else
            {
                this.tokens = new List<string>();
            }
        }

        public override string ToString()
        {
            if (nomeOperador == null)
            {
                return "";
            }
            else
            {
                return nomeOperador;
            }
           
        }

        public override string GetTipoExpressao()
        {
            return this.operador.tipoRetorno;
        }
    }


    public class ExpressaoNILL: Expressao
    {
        public string nill = "NILL";

        public ExpressaoNILL()
        {
            this.tokens = new List<string>() { "NILL" };
        }
        public override string GetTipoExpressao()
        {
            return "NILL";
        }

        public override string ToString()
        {
            return "NILL";
        }


    }
    public class ExpressaoLiteralText: Expressao
    {
        public string literalText;

        /// <summary>
        /// caracter de literais constantes.
        /// </summary>
        public static new char aspas = '\u0022';

        /// <summary>
        /// caracter delimitador para char constantes.
        /// </summary>
        public static char singleQuote = '\'';




        public ExpressaoLiteralText(string literalText)
        {
            this.literalText = literalText;
            this.tipoDaExpressao = "string";
        }

        public override string GetTipoExpressao()
        {
            return "string";
        }


        /// <summary>
        /// verifica se um token é uma literal.
        /// </summary>
        /// <param name="token">token a verificar.</param>
        /// <returns>[true] se o token é uma literal.</returns>
        public static bool isConstantLiteral(string token)
        {
            if ((token==null) || (token.Length==0)) 
            {
                return false;
            }
            else
            {
                return (token.IndexOf("\"") == 0) && (token.LastIndexOf("\"") == token.Length - 1);
            }
        }



        public override string ToString()
        {
            if (this.literalText != null)
            {
                return this.literalText;
            }
            else
            {
                return "";
            }

        }
    }


    public partial class Expressao
    {

        /// <summary>
        ///  indice do token currente no processamento da expressao.
        /// </summary>
        public int indexToken;

        /// <summary>
        /// lista de tokens que formam a expressao.
        /// </summary>
        public List<string> tokens = new List<string>();

        /// <summary>
        /// lista de classes orquidea, na formacao de headers
        /// </summary>
        public static List<Classe> classesDaLinguagem = null;

        /// <summary>
        /// tipo da expressão: int, string, classe,...
        /// </summary>
        public string tipoDaExpressao;


        /// <summary>
        /// utilizado para processamento por expressoes regex.
        /// </summary>
        public Type typeExpressionBySearcherRegex;
        

        /// <summary>
        /// partes da expressao.
        /// </summary>
        public List<Expressao> Elementos = new List<Expressao>();


        /// <summary>
        /// determina se a expressao foi modificada com alguma avaliacao de expressao.
        /// </summary>
        public bool isModify = true;

        /// <summary>
        /// determina se a expressao está em pos-ordem ou não.
        /// </summary>
        public bool isPosOrdem = false;

        /// <summary>
        /// valor apos um processamento em [Eval].
        /// </summary>
        public object oldValue;



        public virtual object GetElemento() { return this.Elementos; }



        /// <summary>
        /// contem nome de classes, com propriedades, metodos, operadores de cada classe, registrado.
        /// </summary>
        public static FileHeader headers = null;
        /// <summary>
        /// contem os header de classes base da linguagem.
        /// </summary>
        private static List<HeaderClass> headersDaLinguagem = new List<HeaderClass>();
  
        
        /// <summary>
        /// gerenciador de objetos wrapper.
        /// </summary>
        public static GerenciadorWrapper wrapperManager = new GerenciadorWrapper();


        /// <summary>
        /// caracter de literais constantes.
        /// </summary>
        public static char aspas = '\u0022';
     
        private static Expressao singletonExpressao;
        public static Expressao Instance
        {
            get
            {
                if (singletonExpressao == null)
                    singletonExpressao = new Expressao();

                return singletonExpressao;

            }

        }
 
        public Expressao()
        {
            this.tokens = new List<string>();
            this.indexToken = 0;
            this.tipoDaExpressao = "";
        }

        public Expressao(string codigoExpressao, Escopo escopo)
        {
            // se nao inicializou os headers, inicializa. headers são essencial
            // na validação de objetos, metodos, operadores, propriedades..
            if (Expressao.headers == null)
            {
                Expressao.InitHeaders("");
            }
            List<string> tokensExpressao= new Tokens(codigoExpressao).GetTokens();
            this.InitPROCESSO(tokensExpressao.ToArray(), escopo);



            
        }

        public Expressao(string[] tokensExpressao, Escopo escopo)
        {
            // se nao inicializou os headers, inicializa. headers são essencial
            // na validação de objetos, metodos, operadores, propriedades..
            if (Expressao.headers == null)
            {
                Expressao.InitHeaders("");
            }
            InitPROCESSO(tokensExpressao, escopo);
        }


        private void InitPROCESSO(string[] tokensExpressao, Escopo escopo)
        {
       
            this.tokens = tokensExpressao.ToList<string>();
            string str_tokens = UtilTokens.FormataEntrada(Utils.OneLineTokens(this.tokens));



            // PROCESSAMENTO DE OBJETO WRAPPER DATA: VECTOR, MATRIZ, DICTIONARYTEXT, JAGGEDARRAY
            Expressao exprssIntanciacaoWrapper= ProcessamentoDeInstanciacaoWrappersObjectData(tokensExpressao, escopo);
            if (exprssIntanciacaoWrapper != null)
            {
                this.Elementos.Add(exprssIntanciacaoWrapper);
                return;
            }


            // instancia o extrator de EXPRESSOES NAO WRAPPER DATA,  por grupos de tokens sem elementos dentro de parenteses.
            ExpressaoGrupos exprssGrupo = new ExpressaoGrupos();

            // encontra expressoes, atraves do extrator de expressoes.
            List<Expressao> expressionsFound = exprssGrupo.ExtraiExpressoes(str_tokens, escopo);


            if ((expressionsFound != null) && (expressionsFound.Count > 0))
            {
                // compoe um container de expressao, compatibilidade com codigo de todo o projeto.
                this.Elementos.AddRange(expressionsFound);

                for (int i = 0; i < expressionsFound.Count; i++)
                {
                    // adiciona a expressão ao escopo, para fins de otimizaçao de expressões..
                    escopo.tabela.AdicionaExpressoes(escopo, expressionsFound[i]);

                }
                // se houver sub-expressoes, é preciso determinar o tipo da expressao envelope.
                if (expressionsFound.Count > 0)
                {
                    this.tipoDaExpressao = GetTipoExpressaoDoEnvelope(expressionsFound);
                }

            }



        }

        /// <summary>
        /// obtem o tipo da expressao, a partir de sub-expressoes, para a expressao envelope.
        /// </summary>
        /// <param name="expressionsFound">lista de sub-expressoes.</param>
        /// <returns>retorna o tipo da expressao, de acordoo com o tipo das sub-expressoes.</returns>
        private string GetTipoExpressaoDoEnvelope(List<Expressao> expressionsFound)
        {
            // varias sub-expressoes, é preciso encontrar o tipo da expressao do envelope procurando pelos tipos da sub-expressões.
            List<Expressao> exprssOp = expressionsFound.FindAll(k => k.GetType() == typeof(ExpressaoOperador));
            // EXPRESSOES OPERADORES ENTRE AS EXPRESSOES.
            if ((exprssOp != null) && (exprssOp.Count > 0))
            {
                
                return exprssOp[exprssOp.Count - 1].tipoDaExpressao;
            }

            List<Expressao> exprssObjeto = expressionsFound.FindAll(k => k.GetType() == typeof(ExpressaoObjeto));
            if ((exprssObjeto != null) && (exprssObjeto.Count > 0))
            {
                return exprssObjeto[exprssObjeto.Count - 1].tipoDaExpressao;
            }
                 

            // EXPRESSOES PROPRIEDADE ANINHADA.
            List<Expressao> exprssPROP = expressionsFound.FindAll(k => k.GetType() == typeof(ExpressaoPropriedadesAninhadas));
            if ((exprssPROP != null) && (exprssPROP.Count > 0))
            {
                
                return exprssPROP[exprssPROP.Count - 1].tipoDaExpressao;
            }
            List<Expressao> exprssChamada = expressionsFound.FindAll(k => k.GetType() == typeof(ExpressaoChamadaDeMetodo));
            if ((exprssChamada != null) && (exprssChamada.Count > 0))
            {
                return exprssChamada[exprssChamada.Count - 1].tipoDaExpressao; ;
            }

            
            List<Expressao> exprssLiteral = expressionsFound.FindAll(k => k.GetType() == typeof(ExpressaoLiteralText));
            if ((exprssLiteral != null) && (exprssLiteral.Count > 0))
            {
                return "string";
            }
            List<Expressao> expssNumero= expressionsFound.FindAll(k=>k.GetType()==typeof(ExpressaoNumero));
            if ((expssNumero != null) && (expssNumero.Count > 0))
            {
                return ExpressaoNumero.GetTypeNumber(expssNumero[0].ToString());
            }

            return this.tipoDaExpressao;
        }

        /// <summary>
        /// faz o processamento de instanciacao de objetos wrapper.
        /// </summary>
        /// <param name="tokensExpressao">tokens da expressao para o processamento.</param>
        /// <param name="escopo">contexto onde a expressao está.</param>
        /// <returns></returns>
        public static Expressao ProcessamentoDeInstanciacaoWrappersObjectData(string[] tokensExpressao, Escopo escopo)
        {
            // verifica se INSTANCIACAO OBJETOS WRAPPER.
            List<string> tokensCreate = Expressao.wrapperManager.IsToWrapperInstantiate(tokensExpressao, escopo);
            if ((tokensCreate != null) && (tokensCreate.Count >= 2)) 
            {
                ExpressaoGrupos expressao = new ExpressaoGrupos();

                string nomeObjetoCaller = tokensCreate[0];
                Objeto objCaller = escopo.tabela.GetObjeto(nomeObjetoCaller, escopo);

                if (objCaller != null)
                {
                    string nomeClasse = objCaller.GetTipo();
                    string nomeMetodo = "Create";
                    List<ExpressaoGrupos.GruposEntreParenteses> grupos = new List<ExpressaoGrupos.GruposEntreParenteses>();
                    List<string> tokensResumidos = ExpressaoGrupos.GruposEntreParenteses.RemoveAndRegistryGroups(UtilString.UneLinhasLista(tokensCreate), ref grupos, escopo);

                    // obtem a expressao chamada de metodo, lisinho, vindo de [ExpressaoGrupos].
                    Expressao expressaoChamadaDeMetodo = expressao.BuildCallingMethod(objCaller, tokensCreate, tokensResumidos, nomeClasse, nomeMetodo, escopo, false);
                    if (expressaoChamadaDeMetodo != null)
                    {
                        expressaoChamadaDeMetodo.tipoDaExpressao = objCaller.GetTipoElement();
                        return expressaoChamadaDeMetodo;
                    }
                    else
                    {
                        UtilTokens.WriteAErrorMensage("error in object: " + objCaller.GetNome() + " in creation", tokensResumidos, escopo);
                        return null;
                    }
                    

                }
            }

            return null;
        }


        public Expressao Clone()
        {
            Expressao expressaoClonada = new Expressao();
            expressaoClonada.indexToken = this.indexToken;
            expressaoClonada.tokens = this.tokens.ToList<string>();
            expressaoClonada.Elementos = this.Elementos.ToList<Expressao>();
            expressaoClonada.tipoDaExpressao = this.tipoDaExpressao;
            
            return expressaoClonada;
        }


        /// <summary>
        /// extrai sub-expressoes, a partir de uma lista de tokens.
        /// utilizado muito em extração de lista de parametros.
        /// </summary>
        /// <param name="tokensDasSubExpressoes">tokens das sub-expressoes.</param>
        /// <param name="escopo">contexto onde a sub-expressao está.</param>
        /// <returns></returns>
        public static List<Expressao> ExtraiExpressoes(List<string> tokensDasSubExpressoes, Escopo escopo)
        {
            string codigo= Util.UtilString.UneLinhasLista(tokensDasSubExpressoes);
            ExpressaoGrupos exprssPorGrupos = new ExpressaoGrupos();
            List<Expressao> listaExpressao = exprssPorGrupos.ExtraiMultipasExpressoesIndependentes(codigo, escopo);

            if (listaExpressao == null)
            {
                UtilTokens.WriteAErrorMensage("error in processing expression, code: " + codigo, tokensDasSubExpressoes, escopo);
                return null;
            }
            else
            {
                return listaExpressao;
            }
        }

        public  void PosOrdemExpressao()
        {

            if ((this.Elementos == null) || (this.Elementos.Count == 0) || (Elementos[0] == null))
            {
                return;
            }

            Expressao expressaoRetorno = new Expressao();

            Pilha<Operador> pilha = new Pilha<Operador>("operadores");
            List<Operador> operadoresPresentes = new List<Operador>();
            int index = 0;
    

            while (index < Elementos.Count)
            {
               
                if (this.Elementos[index].GetType()==typeof(Expressao))
                {
                    this.Elementos[index].PosOrdemExpressao();
                    return;
                }

                if (this.Elementos[index].GetType() == typeof(ExpressaoAtribuicao))
                {
                    ((ExpressaoAtribuicao)this.Elementos[index]).exprssAtribuicao.PosOrdemExpressao();
                    return;
                }
                else
                if (this.Elementos[index].GetType() == typeof(ExpressaoEntreParenteses))
                {
                    ((ExpressaoEntreParenteses)Elementos[index]).exprssParenteses.PosOrdemExpressao();
                    expressaoRetorno.Elementos.Add(Elementos[index]);
                }
                else
                if (RepositorioDeClassesOO.Instance().GetClasse(Elementos[index].ToString()) != null)
                {
                    index++;
                    continue; // definição do tipo da variavel não é avaliado em Expressao.PosOrdem.    
                }
                else
                if (Elementos[index].GetType() == typeof(ExpressaoNumero))
                {
                    expressaoRetorno.Elementos.Add(Elementos[index]);
                }
                else
                if (this.Elementos[index].GetType() == typeof(ExpressaoNILL))
                {
                    expressaoRetorno.Elementos.Add(this.Elementos[index]);
                }
                else
                if (ExpressaoNumero.isNumero(this.Elementos[index].ToString()))
                {
                    expressaoRetorno.Elementos.Add(this.Elementos[index]);
                }
                else
                if (this.Elementos[index].GetType() == (typeof(ExpressaoObjeto)))
                {
                    expressaoRetorno.Elementos.Add(this.Elementos[index]);
                }
                else
                if (this.Elementos[index].GetType() == typeof(ExpressaoChamadaDeMetodo))
                {
                    ExpressaoChamadaDeMetodo chamada = (ExpressaoChamadaDeMetodo)this.Elementos[index];
                    if (chamada.parametros != null)
                        for (int x = 0; x < chamada.parametros.Count; x++)
                            ((Expressao)chamada.parametros[x]).PosOrdemExpressao(); // coloca em pos ordem cada expressao que eh  um parametro da chamada de funcao.

                    expressaoRetorno.Elementos.Add(chamada);
                }
                else
                if (this.Elementos[index].GetType() == typeof(ExpressaoPropriedadesAninhadas))
                {
                    expressaoRetorno.Elementos.Add(this.Elementos[index]);
                }
                else
                if (this.Elementos[index].GetType() == typeof(ExpressaoOperador))
                {

                    Operador op = ((ExpressaoOperador)this.Elementos[index]).operador;

                    // verificar o mecanismo de prioridade.
                    while ((!pilha.Empty()) && (pilha.Peek().prioridade >= op.prioridade))
                    {
                        Operador op_topo = pilha.Pop();
                        expressaoRetorno.Elementos.Add(new ExpressaoOperador(op_topo));
                    }
                    pilha.Push(op);
                }
                else
                if (this.Elementos[index].GetType() == typeof(ExpressaoLiteralText))
                {
                    expressaoRetorno.Elementos.Add(this.Elementos[index]);
                }
                index++;
            }

            while (!pilha.Empty())
            {
                Operador operador = pilha.Pop();
                ExpressaoOperador expressaoOperador = new ExpressaoOperador(operador);
                expressaoRetorno.Elementos.Add(expressaoOperador);
            }

            this.Elementos = expressaoRetorno.Elementos;
            this.isModify = expressaoRetorno.isModify;

          


        }



        /// <summary>
        /// obtem o tipo da expressão, analisando a primeira sub-expressao. retorna null, se não houver sub-expressao.
        /// </summary>
        /// <param name="expressaoCurrente">expressao a obter o tipo da expressao.</param>
        /// <param name="escopo">contexto onde a expressao esta.</param>
        /// <returns></returns>
        public virtual string GetTipoExpressao(Expressao expressaoCurrente, Escopo escopo)
        {
            return expressaoCurrente.GetTipoExpressao();
        }

       



        public override string ToString()
        {
            string str = "";
            if ((this.tokens != null) && (this.tokens.Count > 0))
                str = Utils.OneLineTokens(this.tokens);
            return str;
        }


        public virtual string GetTipoExpressao()
        {
            return this.tipoDaExpressao;
        }


        public static void InitHeaders(string codigoClasses)
        {
            List<string> tokensClasses = null;

            if (codigoClasses != "")
                // obtem os tokens de classes do codigo do programa.
                tokensClasses = new Tokens(codigoClasses).GetTokens();
            else
                tokensClasses = new List<string>();
        
            InitHeaders(tokensClasses);

        }


        public static void InitHeaders(List<string> tokensClasses)
        {

            if ((tokensClasses != null) && (tokensClasses.Count > 0))
            {

                // REMOVE CLASSE ANTIGAS, COM MESMO NOME DA INSTANCIACAO DE CLASSES ATUAIS.
                if ((Expressao.headers != null) && (Expressao.headers.cabecalhoDeClasses != null) && (Expressao.headers.cabecalhoDeClasses.Count > 0)) 
                {
                    FileHeader headerPrevious = new FileHeader();
                    headerPrevious.ExtractCabecalhoDeClasses(tokensClasses);
                    if ((headerPrevious.cabecalhoDeClasses != null) && (headerPrevious.cabecalhoDeClasses.Count > 0))
                    {
                        for (int i = 0;i<headerPrevious.cabecalhoDeClasses.Count;i++)
                        {
                            int index = headers.cabecalhoDeClasses.FindIndex(k => k.nomeClasse == headerPrevious.cabecalhoDeClasses[i].nomeClasse);
                            if (index != -1)
                            {
                                // remover a classe antiga, com o mesmo nome da classe atual.
                                headers.cabecalhoDeClasses.RemoveAt(index);
                                i--;
                            }
                        }
                    }

                    Expressao.headers.ExtractCabecalhoDeClasses(tokensClasses);

                }


            }

            if (Expressao.headers == null)
            {

                

                // constroi os headers de classes base da linguagem, armazenadas no objeto LinguagemOrquiea.
                Expressao.headers = new FileHeader();
                if ((classesDaLinguagem == null) || (classesDaLinguagem.Count == 0))
                {
                    classesDaLinguagem = LinguagemOrquidea.Instance().GetClasses();


                    Expressao.headers.ExtractHeadersClassFromClassesOrquidea(classesDaLinguagem, headersDaLinguagem);

                }
                else
                {
                    Expressao.headers.cabecalhoDeClasses.AddRange(headersDaLinguagem);
                }

                Expressao.headers.ExtractCabecalhoDeClasses(tokensClasses);



            }
        }

        public class Testes : SuiteClasseTestes
        {
            public Testes() : base("testes classe [Expressao]")
            {


               
            
            }


            public void TestCreatSeElementGetElementVector(AssercaoSuiteClasse assercao)
            {
                //Expressao.headers = null;

                string codigoAtribuicao = "x=v5[1];";
                string codigoGetElement = "v5[1];";
                string codigoSetElement = "v5[1]=5;";
                string codigoInstanciacao1 = "int[] v5 [ 6 ];";
                string codigoInstanciacao2 = "int[] v5 [ x+1 ];";


                Escopo escopo = new Escopo(codigoInstanciacao1);
               
                escopo.tabela.GetObjetos().Add(new Objeto("private", "int", "x", 1));
                //escopo.tabela.RegistraObjeto(objetoVetor);

                Expressao exprssCreate1 = new Expressao(codigoInstanciacao1, escopo);
                Expressao exprssCreate2 = new Expressao(codigoInstanciacao2, escopo);

                Expressao epxrssAtribui = new Expressao(codigoAtribuicao, escopo);
                Expressao exprssGET = new Expressao(codigoGetElement, escopo);
                Expressao exprssSET = new Expressao(codigoSetElement, escopo);

                try
                {
                    assercao.IsTrue(
                        epxrssAtribui.Elementos[0].GetType() == typeof(ExpressaoAtribuicao) &&
                        ((ExpressaoAtribuicao)epxrssAtribui.Elementos[0]).exprssAtribuicao.Elementos[0].tokens.Contains("GetElement"), codigoAtribuicao);

                    assercao.IsTrue(AssertCreate(exprssCreate2), codigoInstanciacao2);
                    assercao.IsTrue(AssertCreate(exprssCreate1), codigoInstanciacao1);
                    assercao.IsTrue(AssertGetElement(exprssGET), codigoGetElement);
                    assercao.IsTrue(AssertSetElement(exprssSET), codigoSetElement);
                }
                catch (Exception ex)
                {
                    LoggerTests.AddMessage("Fatal error, classe vector: " + ex.Message);
                }



            }





            public void TesteExpressaoOperadorVariosOperadores(AssercaoSuiteClasse assercao)
            {

                //Expressao.headers = null;

                string codigoClasseX = "public class classeX { public classeX() { int y; }  public int metodoX(int x, int y) { int x; }; };";
                string codigoClasseA = "public class classeA { public classeA() { int x=1; }  public int metodoA(){ int y= 1; }; };";



                string codigoClasseC = "public class classeC {  public int propriedadeC;  public classeC() { int x =0; } }; ";
                string codigoClasseD = "public class classeD {  public classeC propriedadeD; public classeD() { int y=0; }  };";


                string codigoObjetos1 = "classeA a_1= create(); classeX x_0= create();";
                string codigoObjetos2 = "int objA=1; int objB=4;";
                string codigoObjeto4 = "classeD d= create();";
                string codigoObjeto5 = "int x=1; int y=1; int z=5; int w=5; int b=5; int a=5; string s;";


                string nomeLiteral = "fruta vermelha";

                ProcessadorDeID compilador = new ProcessadorDeID(codigoClasseA + " " + codigoClasseX + " " + codigoClasseC + " " + codigoClasseD + " " + codigoObjetos1 + codigoObjetos2 + codigoObjeto4 + codigoObjeto5);
                compilador.Compilar();


                // testes unitarios de expressoes.
                string codigo_expressaoAritimetica = "x+y+z*w";
                string codigo_0_1_chamada = "b + a_1.metodoA();";
                string codigoOperadorUnario = "b++";
                string codigoPropriedadesAninhadas = "d.propriedadeD.propriedadeC;";
                string codigoOperador = "objA+objB;";
                string codigoAtribuicao = "a = a+b;";
                string codigoEntreParenteses = "(a+b)";
                string codigoSomaNumeros = "1+2";

                string codigoLiteral = '\u0022' + nomeLiteral + '\u0022';
                string codigoOperadorLiteral = "s=" + codigoLiteral;

                Expressao exprssPropriedadesAninhadas = new Expressao(codigoPropriedadesAninhadas, compilador.escopo);


                Expressao exprss_0_1_chamada = new Expressao(codigo_0_1_chamada, compilador.escopo);
                Expressao exprssAtribuicao = new Expressao(codigoAtribuicao, compilador.escopo);
                Expressao exprssOperadorSomaLiteral = new Expressao(codigoOperadorLiteral, compilador.escopo);
                Expressao exprssOpUnario = new Expressao(codigoOperadorUnario, compilador.escopo);
                Expressao exprss_Aritimetica = new Expressao(codigo_expressaoAritimetica, compilador.escopo);
                Expressao exprssOperador = new Expressao(codigoOperador, compilador.escopo);
                Expressao exprssEntreParenteses = new Expressao(codigoEntreParenteses, compilador.escopo);
                Expressao exprssOperadorSoma = new Expressao(codigoSomaNumeros, compilador.escopo);
                Expressao exprssLiteral = new Expressao(codigoLiteral, compilador.escopo);


                try
                {

                    assercao.IsTrue(exprssOperador.Elementos[0].GetType() == typeof(ExpressaoObjeto), codigoOperador);
                    assercao.IsTrue(exprssPropriedadesAninhadas.Elementos[0].GetType() == typeof(ExpressaoPropriedadesAninhadas), codigoPropriedadesAninhadas);
                    assercao.IsTrue(exprss_Aritimetica.Elementos[0].GetType() == typeof(ExpressaoObjeto) && exprss_Aritimetica.Elementos[1].GetType() == typeof(ExpressaoOperador), codigo_expressaoAritimetica);
                    assercao.IsTrue(exprss_0_1_chamada.Elementos[2].GetType() == typeof(ExpressaoChamadaDeMetodo), codigo_0_1_chamada);
                    assercao.IsTrue(exprssOpUnario.Elementos[1].GetType() == typeof(ExpressaoOperador), codigoOperadorUnario);
                    assercao.IsTrue(exprssAtribuicao.Elementos[0].GetType() == typeof(ExpressaoAtribuicao), codigoPropriedadesAninhadas);
                    assercao.IsTrue(exprssEntreParenteses.Elementos[0].GetType() == typeof(ExpressaoEntreParenteses), codigoOperador);
                    assercao.IsTrue(exprssOperadorSoma.Elementos[1].GetType() == typeof(ExpressaoOperador), codigoSomaNumeros);
                    assercao.IsTrue(exprssLiteral.Elementos[0].GetType() == typeof(ExpressaoLiteralText), codigoLiteral);
                    assercao.IsTrue(exprssOperadorSomaLiteral.Elementos[0].GetType() == typeof(ExpressaoAtribuicao), codigoOperadorLiteral);

                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                    assercao.IsTrue(false, "Fail tests");
                }



            }



            public void TestCreateSetElementGetElementMatriz(AssercaoSuiteClasse assercao)
            {
                //Expressao.headers = null;

                Escopo escopo1 = new Escopo("int x=1;");
                escopo1.tabela.AddObjeto("private", "x", "int", 1, escopo1);
                escopo1.tabela.AddObjeto("private", "y", "int", 5, escopo1);


                Matriz m1 = new Matriz();
                m1.SetNome("m1");
                m1.isWrapperObject = true;
                escopo1.tabela.GetObjetos().Add(m1);
                escopo1.tabela.AddObjeto("private", "x", "int", 1, escopo1);
                escopo1.tabela.AddObjeto("public", "a", "double", 2, escopo1);



                string codigo_atribuicao_02 = "a= m1[x+1,y+5]+a";
                string codigo_atribuicao_03 = "a= m1[x+1,y+5]+1";
                string codigo_atribuicao_01 = "a= m1[1,5]";

                string codigo_instanciacao_01 = "Matriz m1 [ 1 , 2 ]";

                string codigoGet = "m1[x+1,y+5]";
                string codigoSet = "m1[x+1,y+5]=1";



                Expressao exprss_get = new Expressao(codigoGet, escopo1);
                Expressao exprss_set = new Expressao(codigoSet, escopo1);
                Expressao exprss_instanciacao_01 = new Expressao(codigo_instanciacao_01, escopo1);


                Expressao exprss_atribuicao_03 = new Expressao(codigo_atribuicao_03, escopo1);
                Expressao exprss_atribuicao_01 = new Expressao(codigo_atribuicao_01, escopo1);
                Expressao exprss_atribuicao_02 = new Expressao(codigo_atribuicao_02, escopo1);





                assercao.IsTrue(AssertGetElement(exprss_get), codigoGet);
                assercao.IsTrue(AssertSetElement(exprss_set), codigoSet);
                assercao.IsTrue(AssertExpressaoAtribuicao(exprss_atribuicao_03), codigo_atribuicao_03);
                assercao.IsTrue(AssertExpressaoAtribuicao(exprss_atribuicao_01), codigo_atribuicao_01);
                assercao.IsTrue(AssertExpressaoAtribuicao(exprss_atribuicao_02), codigo_atribuicao_02);
                assercao.IsTrue(AssertCreate(exprss_instanciacao_01), codigo_instanciacao_01);





            }


            private bool AssertExpressaoAtribuicao(Expressao exprss)
            {
                string codeError = "";
                try
                {
                    return exprss.Elementos[0].GetType() == typeof(ExpressaoAtribuicao);
                }
                catch (Exception e)
                {

                    codeError = e.Message;
                    return false;
                }
            }
            private bool AssertSetElement(Expressao exprssResult)
            {
                string codeError = "";
                try
                {
                    return (exprssResult.Elementos[0].tokens.Contains("SetElement") &&
                            exprssResult.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo));
                }
                catch (Exception e)
                {
                    codeError = e.Message;
                    return false;
                }
            }
            private bool AssertGetElement(Expressao exprssResult)
            {
                string codeError = "";
                try
                {
                    return (exprssResult.Elementos[0].tokens.Contains("GetElement") &&
                            exprssResult.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo));
                }
                catch (Exception e)
                {
                    codeError = e.Message;
                    return false;
                }
            }
            private bool AssertCreate(Expressao exprssResult)
            {
                string codeError = "";
                try
                {
                    return (exprssResult.Elementos[0].tokens.Contains("Create") &&
                            exprssResult.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo));
                }
                catch (Exception ex)
                {
                    codeError = ex.Message;
                    return false;
                }
            }

            public void TesteExpressaoProcessamentoObjetoDictionaryText(AssercaoSuiteClasse assercao)
            {
                //Expressao.headers = null;
                char aspas = '\u0022';

                string codigoCreate = "DictionaryText m = { string }";
                string codigoGet = "m{" + aspas + "fruta" + aspas + "}";
                string codigoSet = "m{" + aspas + "fruta" + aspas + "," + "maca}";


                Escopo escopo1 = new Escopo(codigoCreate);

                DictionaryText m = new DictionaryText();
                m.tipoElemento = "string";
                m.tipo = "DictionaryText";
                m.SetNome("m");
                m.isWrapperObject = true;

                escopo1.tabela.RegistraObjeto(m);



                Expressao exprssCreate = new Expressao(codigoCreate, escopo1);
                Expressao exprssSet = new Expressao(codigoSet, escopo1);
                Expressao exprssGet = new Expressao(codigoGet, escopo1);





                try
                {

                    assercao.IsTrue(exprssCreate.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo), codigoCreate);
                    assercao.IsTrue(exprssGet.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo), codigoGet);
                    assercao.IsTrue(exprssSet.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo), codigoSet);
                    assercao.IsTrue(escopo1.tabela.GetObjeto("m", escopo1).isWrapperObject);
                }
                catch (Exception e)
                {
                    assercao.IsTrue(false, "erro na validacao dos resultados: " + e.Message);
                }
            }


            public void TesteLiteraisEVariaveisString(AssercaoSuiteClasse assercao)
            {
                //Expressao.headers = null;

                // constroi um texto constante.
                string literal = ExpressaoLiteralText.aspas + "Oi, homero " + ExpressaoLiteralText.aspas;

                // constroi  variaveis string.
                Escopo escopo = new Escopo("int x;");
                escopo.tabela.RegistraObjeto(new Objeto("private", "string", "nome", "homero"));
                escopo.tabela.RegistraObjeto(new Objeto("private", "string", "result", null));

                try
                {
                    Expressao exprss = new Expressao(literal+ " +  nome" , escopo);
                }
                catch (Exception e)
                {
                    string errorCode = e.Message;
                    assercao.IsTrue(false, "TESTE FALHOU");
                }

                

            }

            public void TesteParametrosMultiArgumentos(AssercaoSuiteClasse assercao)
            {

                //Expressao.headers = null;

                string codigoClasse = "public class classeA { public int propriedadeA;  public classeA(){ int x=1; } ;public int metodoB(int x, ! Vector y){ int x=2;} ;};";
                string codigoCreate = "classeA objA= create(); double x=1;";
                string codigoChamadaDeMetodo = "objA.metodoB(x,1,1,1);";

                Escopo escopo = new Escopo(codigoClasse + codigoCreate);
                ProcessadorDeID compilador = new ProcessadorDeID(codigoClasse + codigoCreate);
                compilador.Compilar();

                Expressao exprss = new Expressao(codigoChamadaDeMetodo, compilador.escopo);

                try
                {
                    assercao.IsTrue(RepositorioDeClassesOO.Instance().GetClasse("classeA") != null &&
                                    compilador.escopo.tabela.GetObjeto("objA", compilador.escopo) != null &&
                                    RepositorioDeClassesOO.Instance().GetClasse("classeA").GetMetodo("metodoB")[0].parametrosDaFuncao[1].isMultArgument == true, codigoClasse);
                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "falha no teste: " + ex.Message);
                }
            }


 

    
 

            public void TesteOperadorUnarioEBinarioMasFuncionandoComoBinario(AssercaoSuiteClasse assercao)
            {

                //Expressao.headers = null;

                string code_binary = "x=x+1;";
                string code_unary = "x= -1;";

                Escopo escopo = new Escopo(code_unary + code_binary); ;
                escopo.tabela.RegistraObjeto(new Objeto("private", "int", "x", 1));

                Expressao exprss_binary = new Expressao(code_binary, escopo);
                Expressao exprss_unary = new Expressao(code_unary, escopo);


                assercao.IsTrue(true, "execucao do teste feito sem erros fatais.");
                try
                {
                    assercao.IsTrue(exprss_binary.Elementos[0].GetType() == typeof(ExpressaoAtribuicao));
                    assercao.IsTrue(exprss_unary.Elementos[0].GetType() == typeof(ExpressaoAtribuicao));
                }
                catch (Exception e)
                {
                    assercao.IsTrue(false, "falha na validacao do teste" + e.Message);
                }
            }


            public void TesteExpressaoProcessamentoObjetoJaggedArray(AssercaoSuiteClasse assercao)
            {
                //Expressao.headers = null;

                // "JaggedArray id = [ exprss ] [ ];
                string codeCreate = "JaggedArray m=[20][];";
                string codigoGet = "m[1][2]";
                string codigoSet = "m[1][1]=5";
                Escopo escopo1 = new Escopo(codeCreate);

                Expressao exprssCreate = new Expressao(codeCreate, escopo1);
                Expressao exprssGET = new Expressao(codigoGet, escopo1);
                Expressao exprssSET = new Expressao(codigoSet, escopo1);

                try
                {

                    assercao.IsTrue(exprssGET.Elementos.Count == 1, "m[1][2]");
                    assercao.IsTrue(exprssSET.Elementos.Count == 1, "m[1][1]=5");
                    assercao.IsTrue(escopo1.tabela.GetObjeto("m", escopo1).isWrapperObject, "JaggedArray m=[20][]"); ;
                }
                catch (Exception e)
                {
                    assercao.IsTrue(false, "erro na validacao de resultados." + e.Message);
                }

            }


            public void TestesExpressoesMaisDe1ElementoWrapper(AssercaoSuiteClasse assercao)
            {
                //Expressao.headers = null;

                string codigo_2_wrappers_01 = "a= v1[0]+v2[1]";
                string codigo_2_wrappers_02 = "a= v1[0]+v2[1]+ 1";
                string codigo_create_01 = "int[] v1[10];";
                string codigo_create_02 = "int[] v2[15];";

                Vector v1 = new Vector("int");
                Vector v2 = new Vector("int");
                v1.SetNome("v1");
                v2.SetNome("v2");
                v1.tipoElemento = "int";
                v2.tipoElemento = "int";

                Escopo escopo = new Escopo(codigo_create_01 + codigo_create_02 + codigo_2_wrappers_01 + codigo_2_wrappers_02);
                escopo.tabela.RegistraObjeto(v1);
                escopo.tabela.RegistraObjeto(v2);
                escopo.tabela.RegistraObjeto(new Objeto("private", "int", "a", 5));


                Expressao exprss2_wrappers_01 = new Expressao(codigo_2_wrappers_01, escopo);
                Expressao exprss2_wrappers_02 = new Expressao(codigo_2_wrappers_02, escopo);
                Expressao exprss_create_01 = new Expressao(codigo_create_01, escopo);
                Expressao exprss_create_02 = new Expressao(codigo_create_02, escopo);
               
                try
                {
                    assercao.IsTrue(
                        exprss2_wrappers_01.Elementos[0].GetType() == typeof(ExpressaoAtribuicao) &&
                        ((ExpressaoAtribuicao)exprss2_wrappers_01.Elementos[0]).exprssAtribuicao.Elementos[0].tokens.Contains("GetElement"), codigo_2_wrappers_01);


                    assercao.IsTrue(
                    exprss2_wrappers_02.Elementos[0].GetType() == typeof(ExpressaoAtribuicao) &&
                        ((ExpressaoAtribuicao)exprss2_wrappers_02.Elementos[0]).exprssAtribuicao.Elementos[0].tokens.Contains("GetElement"), codigo_2_wrappers_01);

                    assercao.IsTrue(exprss_create_01.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo) && exprss_create_01.Elementos[0].tokens.Contains("Create"), codigo_create_01);
                    assercao.IsTrue(exprss_create_02.Elementos[0].GetType() == typeof(ExpressaoChamadaDeMetodo) && exprss_create_02.Elementos[0].tokens.Contains("Create"), codigo_create_02);


                }
                catch (Exception ex)
                {
                    string error = ex.Message;
                    assercao.IsTrue(false, "TESTE FALHOU");
                }
            }





 
  









        }
    }


 


} 

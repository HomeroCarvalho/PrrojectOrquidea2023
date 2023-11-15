using System.Collections.Generic;
using System.Linq;
using System;
using System.Diagnostics.Tracing;
using System.Security.Policy;
using System.Runtime.CompilerServices;
using parser.ProgramacaoOrentadaAObjetos;
namespace parser
{
    public class Escopo
    {

        public enum tipoEscopo { escopoGlobal, escopoNormal};


        /// <summary>
        ///  tipo do escopo currente.
        /// </summary>
        public tipoEscopo ID = tipoEscopo.escopoNormal;

        /// <summary>
        /// escopo da linguagem. É o escopo raiz.
        /// </summary>
        public static Escopo EscopoRaiz = null;


        /// <summary>
        /// escopos de mesmo nivel horizontal.
        /// </summary>
        public List<Escopo> escopoFolhas = null;

        /// <summary>
        /// escopo um nivel acima.
        /// </summary>
        private Escopo _escopoPai = null;

        /// <summary>
        /// lista de sequencias id encontradas neste escopo.
        /// </summary>
        public List<UmaSequenciaID> sequencias;



        /// <summary>
        /// escopo raiz do escopo currente. Se o escopo pai for null, e o escopo não for escopoGlobal, retorna o escopoGlobal.
        /// </summary>
        public Escopo escopoPai
        {
            get
            {
                if (this.ID == tipoEscopo.escopoNormal)
                    return _escopoPai;
                if (this.ID == tipoEscopo.escopoGlobal)
                    return EscopoRaiz;
                return _escopoPai;
            }
            set
            {
                _escopoPai = value;
            } 
        } // escopoPai

        /// <summary>
        /// nome da classe sendo compilada.
        /// </summary>
        public static string nomeClasseCurrente;


        /// <summary>
        /// contém as variáveis, funções, métodos, propriedades, classes registradas neste escopo.       
        /// </summary>
        public TablelaDeValores tabela { get; set; }



        /// <summary>
        /// lista de erros na escrita de codigo feito pelo programador quando utiliza a linguagem orquidea.
        /// </summary>
        private List<string> MsgErros = new List<string>();

        /// <summary>
        /// codigo contido no contexto do escopo;
        /// </summary>
        public List<string> codigo;

        private static  UmaGramaticaComputacional linguagem = LinguagemOrquidea.Instance();
   
        public List<string> GetMsgErros()
        {
            return MsgErros;
        }


        public Escopo(string code)
        {
            if (code == null)
                codigo = new List<string>();
            else
                codigo = new Tokens(code).GetTokens();

            this.ID = tipoEscopo.escopoNormal;
            this.MsgErros = new List<string>();


            this.tabela = new TablelaDeValores(codigo);
            this.escopoFolhas = new List<Escopo>();
            ConstroiEscopoRaiz();
            this._escopoPai = EscopoRaiz;
            this.sequencias = new List<UmaSequenciaID>();

            if (PosicaoECodigo.lineCurrentProcessing == 0)
                PosicaoECodigo.AddLineOfCode(Utils.OneLineTokens(codigo));

        }


        /// <summary>
        /// constroi a rede de escopos para um programa.
        /// </summary>
        /// <param name="codigo">trecho de código bruto, sem conversao de tokens.</param>
        /// 
        public Escopo(List<string> codigo)
        {
            if (codigo != null)
                this.codigo = codigo.ToList<string>();
            else
                this.codigo = new List<string>();
      
            this.ID = tipoEscopo.escopoNormal;
            this.MsgErros = new List<string>();


            this.tabela = new TablelaDeValores(codigo);
            this.escopoFolhas = new List<Escopo>();
            ConstroiEscopoRaiz();
            this._escopoPai = EscopoRaiz;
            this.sequencias = new List<UmaSequenciaID>();

            if (PosicaoECodigo.lineCurrentProcessing == 0)
                PosicaoECodigo.AddLineOfCode(Utils.OneLineTokens(codigo));
        } // ContextoEscopo()

        public Escopo(Escopo escopo)
        {
           
            this.ID = escopo.ID;
            this.tabela = new TablelaDeValores(codigo);
            this.MsgErros = new List<string>();
            
            
            
            this.codigo = escopo.codigo.ToList<string>();

            
            this.tabela = escopo.tabela.Clone();
           
            this.escopoFolhas = escopo.escopoFolhas.ToList<Escopo>();
            this.sequencias = escopo.sequencias.ToList<UmaSequenciaID>();

            for (int x = 0; x < escopo.escopoFolhas.Count; x++)
                this.escopoFolhas.Add(new Escopo(escopo.escopoFolhas[x]));

            ConstroiEscopoRaiz();
            this._escopoPai = EscopoRaiz;
        } // Escopo()

        private Escopo()
        {
           
        }

        public void ConstroiEscopoRaiz()
        {
            if (EscopoRaiz == null)
            {
                Escopo.EscopoRaiz = new Escopo()
                {
                    codigo = this.codigo,
                    MsgErros = new List<string>(),
                    tabela = new TablelaDeValores(this.codigo),
                    ID = tipoEscopo.escopoGlobal
                };

                List<Classe> classes = ((LinguagemOrquidea)linguagem).GetClasses();

                foreach (Classe umaClasse in classes)
                {
                    // registra classes presentes na linguagem orquidea.
                    Escopo.EscopoRaiz.tabela.RegistraClasse(umaClasse);
                } // foreach

                // não há escopo anterior ao escopo raiz, então é setado para null.
                this.escopoPai = null;
            } //if
        } //ConstroiEscopoRaiz()

   
        public Escopo Clone()
        {
            Escopo escopo = new Escopo(this.codigo);
            if (this.tabela != null)
            {
                escopo.tabela = this.tabela.Clone();
            }
            else
            {
                escopo.tabela = new TablelaDeValores(this.codigo);
            }
            
            return escopo;
        } // Clone()

        public void Dispose()
        {
            this.codigo = null;
            this.tabela = null;
            this.sequencias = null;
            this.MsgErros = null;
            this.escopoFolhas = null;

        }
        public string WriteCallAtMethod(List<List<string>> chamadaAMetodo, int indexChamada)
        {
            string str = "";
            string nomeFuncaoChamada = chamadaAMetodo[indexChamada][0];
            str += nomeFuncaoChamada;
            str += "( ";
            for (int x = 1; x < chamadaAMetodo[indexChamada].Count - 1; x++)
                str += chamadaAMetodo[indexChamada][x] + ",";
            str += chamadaAMetodo[indexChamada][chamadaAMetodo[indexChamada].Count - 1] + ")";
            return str;
        }  //WriteCallMethods()


 
    } // class ContextoEscopo
} // namespace

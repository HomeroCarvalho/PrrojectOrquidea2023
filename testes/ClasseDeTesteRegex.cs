using parser.ProgramacaoOrentadaAObjetos;
using parser.textoFormatado;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static parser.textoFormatado.TextExpression;

namespace parser
{
    public class ClasseDeTesteRegex : SuiteClasseTestes
    {

        // inicializa o compilador.
        ProcessadorDeID compilador;

        public ClasseDeTesteRegex() : base("testes de sequencias com match por regex expressions")
        {
        }


        public void TesteCompilarComSequenciasRegex(AssercaoSuiteClasse assercao)
        {
            string str_codigo = "int a= 1;";

            ProcessadorDeID compilador= new ProcessadorDeID(str_codigo);
            compilador.Compilar();

            assercao.IsTrue(
                compilador.GetInstrucoes() != null &&
                compilador.GetInstrucoes().Count==1,"validacao para instrucao de atribuicao.");



            string str_codigoClasse = "public class classeA { public int propriedade1=1; public classeA() { int x=1; }  public int metodoA(){ int y= 1; }; }";

            ProcessadorDeID compiladoClasse = new ProcessadorDeID(str_codigoClasse);
            compiladoClasse.Compilar();

            Classe classeTeste = RepositorioDeClassesOO.Instance().GetClasse("classeA");
            

            // teste automatizado.
            assercao.IsTrue(
                classeTeste != null &&
                classeTeste.GetPropriedades() != null &&
                classeTeste.GetPropriedades().Count == 1 &&
                classeTeste.GetMetodos() != null &&
                classeTeste.GetMetodos().Count == 2);
        }


        public void TesteSequenciasRegex(AssercaoSuiteClasse assercao)
        {
            string str_while = "while (x<1)";
            string str_create = "int x= create();";
            string str_definicaoDeMetodo = "int metodoA(int x, int y)";

            string str_testeFor = "for (int a= 1; a<20; a++)";
            string str_testeIf = "if (a<20)";
            
           
            string str_return = "return x+y;";


            string str_atribuicao = "a=1;";
            string str_atribuicao2 = "int a=5;";
            string str_chamadaDeMetodo = "metodoA(x,y)";


            this.compilador = new ProcessadorDeID(str_while);

            CompilaCodigoTeste(str_create, ref assercao);
            CompilaCodigoTeste(str_while, ref assercao);
            CompilaCodigoTeste(str_definicaoDeMetodo, ref assercao);


            CompilaCodigoTeste(str_atribuicao, ref assercao);
            CompilaCodigoTeste(str_atribuicao2, ref assercao);


            CompilaCodigoTeste(str_chamadaDeMetodo, ref assercao);



            CompilaCodigoTeste(str_testeFor, ref assercao);
            CompilaCodigoTeste(str_testeIf, ref assercao);
            CompilaCodigoTeste(str_return, ref assercao);


         
        }

        private  void CompilaCodigoTeste(string str_teste, ref AssercaoSuiteClasse assercao)
        {
            
            // extrai uma sequencia currente.
            UmaSequenciaID sequenciaCurrente = UmaSequenciaID.ObtemUmaSequenciaID(0, str_teste, str_teste);


            // procura um match na sequencia currente.
            compilador.MatchSequencias(sequenciaCurrente, compilador.escopo);


            // valida o match de sequencia.
            assercao.IsTrue(sequenciaCurrente.indexHandler != -1, "validacao para: " + str_teste);

            if (sequenciaCurrente.indexHandler != -1)
            {
                System.Console.WriteLine("input: " + str_teste + "  pattern code:  " + compilador.GetHandlers()[sequenciaCurrente.indexHandler].patterResumed);
            }
            

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using parser.ProgramacaoOrentadaAObjetos;
using ParserLinguagemOrquidea.Wrappers;
using Wrappers;
using Wrappers.DataStructures;

namespace parser
{

   

    public class Objeto
    {
        /// <summary>
        /// private, protected, ou public.
        /// </summary>
        private string acessor;

        /// <summary>
        /// classe do objeto.
        /// </summary>
        public string tipo;

        /// <summary>
        /// classe de elementos constituintes desse objeeto.
        /// </summary>
        public string tipoElemento;
        /// <summary>
        /// nome do objeto.
        /// </summary>
        public string nome;

        /// <summary>
        /// valor do objeto.
        /// </summary>
        public object valor;




    



 
        /// <summary>
        /// obtem todos tipos de wrapper.
        /// deve conter TODOS tipos de wrapper.
        /// </summary>
        /// <returns>retorna uma lista de todos wrapper data.</returns>
        public static List<WrapperData> GetAllTypeWrappers()
        {
            WrapperDataVector dataVector = new WrapperDataVector();
            WrapperDataDictionaryText dataDictionaryText = new WrapperDataDictionaryText();
            WrapperDataJaggedArray dataJaggedArray = new WrapperDataJaggedArray();
            WrapperDataMatriz dataMatriz = new WrapperDataMatriz();

            return new List<WrapperData>() { dataVector, dataDictionaryText, dataMatriz, dataJaggedArray };
        }

        /// <summary>
        /// se true o objeto é também um metodo.
        /// </summary>
        public bool isMethod = false;



        /// <summary>
        /// se true o objeto é estático.
        /// </summary>
        public bool isStatic;
        /// <summary>
        /// se true o objeto é multi-argumento.
        /// </summary>
        public bool isMultArgument = false;

        /// <summary>
        /// se true o objeto é um wrapper data object.
        /// </summary>
        public bool isWrapperObject = false;

        /// <summary>
        /// se trye o objeto é um objeto- metodo.
        /// </summary>
        public bool isFunctionParameter = false;



        /// <summary>
        /// lista de campos do objeto.
        /// </summary>
        public List<Objeto> campos = new List<Objeto>();

        /// <summary>
        /// lista de expressoes do objeto.
        /// </summary>
        private List<Expressao> expressoes = new List<Expressao>();

      

        public List<Metodo> construtores = new List<Metodo>();
        public Objeto()
        {
            this.nome = "";
            this.tipo = "";
            this.tipoElemento = "";
            this.valor = null;
            this.campos = new List<Objeto>();
            this.isStatic = false;
        }
        public Objeto(Objeto objeto)
        {
            this.nome = objeto.nome;
            this.tipo = objeto.tipo;
            this.tipoElemento = objeto.tipoElemento;
            this.campos = new List<Objeto>();
            this.valor = objeto.valor;
            this.isStatic = objeto.isStatic;
            if ((objeto.campos != null) && (objeto.campos.Count > 0))
                this.campos = objeto.campos.ToList<Objeto>();

          
        }


        public Objeto(string nomeAcessor, string nomeClasse, string nomeObjeto, string nomeCampo, object valorCampo)
        {
            InitObjeto(nomeAcessor, nomeClasse, nomeObjeto, null);
            Objeto campoModificar = this.campos.Find(k => k.GetNome() == nomeCampo);
            campoModificar.SetValor(valorCampo); // aciona a otimização de cálculo de expressões.
            this.isStatic = false;
        }

        // inicializa uma instância de um objeto, criando memória para a lista de propriedade, nome do objeto, e o tipo do objeto.
        public Objeto(string nomeAcessor, string nomeClasse, string nomeObjeto, object valor)
        {
            InitObjeto(nomeAcessor, nomeClasse, nomeObjeto, valor);
            this.isStatic = false;
        }// Objeto()

        // inicializa uma instância de um objeto, criando memória para a lista de propriedade, nome do objeto, e o tipo do objeto.
        public Objeto(string nomeAcessor, string nomeClasse, string nomeObjeto, object valor, Escopo escopo, bool isStatic)
        {
            InitObjeto(nomeAcessor, nomeClasse, nomeObjeto, valor);
            this.isStatic = isStatic;
        }// Objeto()

        public Objeto(string nomeAcessor, string nomeClasse, string nomeOObjeto, object valor, List<Objeto> campos, Escopo escopo)
        {
            InitObjeto(nomeAcessor, nomeClasse, nomeOObjeto, valor);
            if (campos != null)
            {
                this.campos = campos.ToList<Objeto>();
            }
            else
            {
                this.campos = new List<Objeto>();
            }
            
            this.isStatic = false;
        }
        private void InitObjeto(string nomeAcessor, string nomeClasse, string nomeObjeto, object valor)
        {
         

            this.acessor = nomeAcessor;
            this.nome = nomeObjeto;
            this.tipo = nomeClasse;
            this.tipoElemento = this.tipo;
            this.valor = valor;
            Classe classe = RepositorioDeClassesOO.Instance().GetClasse(nomeClasse);
            
            if (classe != null)
            {
                if ((classe.GetPropriedades() != null) && (classe.GetPropriedades().Count > 0))
                    this.campos = classe.GetPropriedades().ToList<Objeto>();
                else
                    this.campos = new List<Objeto>();
            } 
        }

        /// <summary>
        /// clona o objeto que chamou o metodo.
        /// </summary>
        /// <returns></returns>
        public Objeto Clone()
        {
            Objeto objClone=new Objeto();
            objClone.acessor = this.acessor;
            objClone.nome = this.nome;
            objClone.tipo = this.tipo;
            objClone.valor= this.valor;
            objClone.tipoElemento = this.tipoElemento;
            objClone.isWrapperObject= this.isWrapperObject;
            objClone.isStatic = this.isStatic;
            objClone.isMultArgument = this.isMultArgument;
            objClone.isMethod = this.isMethod;

            if ((this.campos != null) && (this.campos.Count > 0))
            {
                for (int i = 0; i < this.campos.Count; i++)
                {
                    objClone.campos.Add(this.campos[i]);
                }
            }
            return objClone;
        }

        /// <summary>
        /// copia para o objeto que chamou, os dados do objeto parametro.
        /// </summary>
        /// <param name="objToCopy">objeto a ser copiado.</param>
        public void CopyObjeto(Objeto objToCopy)
        {
            this.acessor=objToCopy.acessor;
            this.nome=objToCopy.nome;
            this.tipo=objToCopy.tipo;
            this.valor=objToCopy.valor;
            this.tipoElemento=objToCopy.tipoElemento;
            this.isWrapperObject = objToCopy.isWrapperObject;
            this.isStatic = objToCopy.isStatic;
            this.isMultArgument = objToCopy.isMultArgument;
            this.isMethod = objToCopy.isMethod;

            if ((objToCopy.campos != null) && (objToCopy.campos.Count > 0))
            {
                this.campos = new List<Objeto>();
                for (int i = 0;i < objToCopy.campos.Count;i++)
                {
                    this.campos.Add(objToCopy.campos[i]);
                }
            }

        }

        /// <summary>
        /// retorna o tipo dos elementos constituinte do objeto.
        /// </summary>
        /// <returns></returns>
        public string GetTipoElement()
        {
            return this.tipoElemento; 
        }


        /// <summary>
        /// seta o tipo dos elementos constituintes do objeto.
        /// </summary>
        /// <param name="tipo"></param>
        public void SetTipoElement(string tipo)
        {
            this.tipoElemento = tipo;
        }
        public List<Expressao> exprssPresentes()
        {
            return this.expressoes;
        }

        public Metodo GetMetodo(string nome)
        {
            return  RepositorioDeClassesOO.Instance().GetClasse(this.tipo).GetMetodos().Find(k => k.nome == nome);
        }

        public Objeto GetField(string nome)
        {
            Objeto afield = this.campos.Find(k => k.GetNome() == nome);

            if (afield == null)
            {
                // seta as propriedades do objeto, de acordo com sua classe.
                return SetPropriedadesDoObjeto(nome);

            }
            else
                return afield;
        }

        public string GetAcessor()
        {
            return this.acessor;
        }
              
        public void SetAcessor(string acessor)
        {
            this.acessor = acessor;
        }
        public void SetNomeLongo(string nomeClasseDaPropriedade)
        {
            this.nome = nomeClasseDaPropriedade + "@" + this.GetNome();
        }
   
        public void SetValorObjeto(object newValue)
        {
            this.valor = newValue;
        }

        public void SetField(Objeto novoField)
        {
            int index=this.campos.FindIndex(k => k.GetNome() == novoField.GetNome());
            if (index == -1)
            {
                // tenta obter as propriedades do objeeto, a partir da sua classe.
                Objeto field = SetPropriedadesDoObjeto(novoField.GetNome());


                // se tiver a propriedade requerida, seta esta propriedade com o valor-parametro.
                if (field != null)
                    SetField(novoField);
            }
            if (index != -1)
            {
                // a propriedade já existe, seta para o valor-parametro.
                this.campos[index] = novoField;
                if (campos[index].expressoes != null)
                    for (int x = 0; x < this.campos[index].expressoes.Count; x++)
                        this.campos[index].expressoes[x].isModify = true;
            }
        }


        private Objeto SetPropriedadesDoObjeto(string nome)
        {
            Classe classeObjeto = RepositorioDeClassesOO.Instance().GetClasse(this.tipo);
            if ((classeObjeto != null) && (classeObjeto.GetPropriedades() != null)) 
            {
                // seta as propriedades do objeto, de acordo com as propriedades de sua classe,
                // fazendo uma copia da lista de propriedades da classe./
                this.campos = classeObjeto.GetPropriedades().ToList<Objeto>();
                return this.campos.Find(k => k.nome == nome);
            }
            else
                return null;
        }


        /// <summary>
        /// implementa a otimizacao de expressoes. Se uma expressao conter a variavel
        /// que está sendo modificada, a expressao é setada para modificacao=true.
        /// isso auxilia nos calculos de valor da expressao, que é avaliada apenas se 
        /// se alguma variavel-componente da expressao for modificada. Util para
        /// expressoes com variaveis que mudam de valor em tempo de reação humana, ou em tempo-real.
        /// </summary>
        public void SetValorField(string nome, object novoValor)
        {
            if (this.GetField(nome) == null)
                return;

            this.GetField(nome).valor = novoValor;
            int index = this.campos.FindIndex(k => k.tipo == nome);
            if (index != -1)
                for (int x = 0; x < this.campos[index].expressoes.Count; x++)
                    if (campos[index].expressoes[x] != null)
                        this.campos[index].expressoes[x].isModify = true;
    
        } // SetValor()

        /// <summary>
        /// implementa a otimizacao de expressoes. Se uma expressao conter a variavel
        /// que está sendo modificada, a expressao é setada para modificacao=true.
        /// isso auxilia nos calculos de valor da expressao, que é avaliada apenas se 
        /// se alguma variavel-componente da expressao for modificada. Util para
        /// expressoes com variaveis que mudam de valor em tempo de reação humana, ou em tempo-real.
        /// </summary>
        public void SetValor(object novoValor)
        {
            this.valor = novoValor;



            // faz a otimização de expressões. expressoes contendo objetos que são modificados, tem que ser avaliadas novamente.
            List<Expressao> expressoesTipoObjeto = TablelaDeValores.expressoes.FindAll(k => k.GetType() == typeof(ExpressaoObjeto));


            if ((expressoesTipoObjeto != null) && (expressoesTipoObjeto.Count > 0))
            {
                for (int x = 0; x < expressoesTipoObjeto.Count; x++)
                {

                    ExpressaoObjeto exprss = (ExpressaoObjeto)expressoesTipoObjeto[x];


                    if ((exprss.objectCaller != null) && (exprss.objectCaller.GetNome() == this.nome))
                        expressoesTipoObjeto[x].isModify = true;
                } // for x
                      
            } // if


        } 


        public static Objeto GetCampo(string classeObjeto, string nomeCampo)
        {
            Classe classe = RepositorioDeClassesOO.Instance().GetClasse(classeObjeto);
            if (classe == null)
                return null;
            else
            {
                Objeto objetoCampo = classe.GetPropriedades().Find(k => k.GetNome() == nomeCampo);
                if (objetoCampo == null)
                   return null;
                else
                    return new Objeto(objetoCampo);
            }
        }
     
        public object GetValor()
        {
            return this.valor;
        }


        public string GetNome()
        {
            return this.nome;
        }

        public void SetNome(string nome)
        {
            this.nome = nome;
        }
        public string GetTipo()
        {
            return this.tipo;
        }

      


        public List<Objeto> GetFields()
        {

            return this.campos;
        }


        /// <summary>
        /// factory de objeto, util para criar objetos de um tipo, para chamadas de metodo, p.ex.
        /// </summary>
        /// <param name="tipo">classe do objeto.</param>
        /// <returns>retorna um object do tipo parâmetro.</returns>
        public static object FactoryObjetos(string tipo)
        {
            if (tipo== null) return null;
            if (tipo=="string")
            {
                return "";
            }
            else
            if (tipo=="double")
            {
                return 0.0;
            }
            else
            if (tipo=="float")
            {
                return 0.0f;
            }
            else
            if (tipo =="int")
            {
                return 0;
            }
            else
            if (tipo=="char")
            {
                return ' ';
            }
            else
            {   // OBTEM O OBJETO A PARTIR DE UM CONSTRUTOR SEM PARAMETROS!
                object resultConstructor = new object();
                object result1 = new object();
                Type classObjectCaller = Type.GetType(tipo);
                ConstructorInfo[] construtores = classObjectCaller.GetConstructors();
                if ((construtores.Length > 0) && (!(resultConstructor is Objeto)))
                {
                    // obtem um objeto da classe que construiu o operador.
                    resultConstructor = classObjectCaller.GetConstructor(new Type[] { }).Invoke(null);
                    return resultConstructor;
                }
                else
                {
                    return new object();
                }
            }
        }

        public override string ToString()
        {
            if ((this.nome == null) || (this.tipo == null))
                return "without name, or type";
            else
                return this.nome + ": " + this.tipo; 
        }

        public class Testes:SuiteClasseTestes
        {
            public Testes() : base("testes classe Objeto")
            {

            }

            public void TestesCastingObjetos(AssercaoSuiteClasse assercao)
            {
                Matriz mt_0_0= new Matriz();
                Matriz mt_0_1 = new Matriz();

                mt_0_1.SetElement(0, 0, 1);
                object objetoMATRIZComValorDoCasting = mt_0_1;

                try
                {
                    WrapperData.CastingObjeto(objetoMATRIZComValorDoCasting, mt_0_0);
                    assercao.IsTrue(mt_0_0.GetElement(0, 0).ToString() == "1", "casting de matriz");

                }
                catch (Exception e)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + e.Message);
                }
                



                JaggedArray j_0_0 = new JaggedArray();
                JaggedArray j_0_1 = new JaggedArray();

                j_0_0.InsertRow(j_0_0, 0);
                j_0_1.InsertRow(j_0_1, 0);

                j_0_1.AddElement(0, 1);
                object objetoJAGGEDARRAYComValorDoCasting = j_0_1;
                
                try
                {
                    WrapperData.CastingObjeto(objetoJAGGEDARRAYComValorDoCasting, j_0_0);
                    assercao.IsTrue(j_0_0.GetElement(0, 0).ToString() == "1", "casting de jagged array");

                }
                catch (Exception ex)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + ex.Message);
                }
        


                DictionaryText dict_0_0 = new DictionaryText();
                dict_0_0.tipoElemento = "string";
                dict_0_0.SetElement("carro", "Renault");
                DictionaryText dict_0_1= new DictionaryText();
                dict_0_1.tipoElemento = "string";
                object objetoDICTIONARYComOValorDoCasting= dict_0_1;

                try
                {
                    WrapperData.CastingObjeto(objetoDICTIONARYComOValorDoCasting, dict_0_0);
                    assercao.IsTrue(dict_0_0.GetElement("carro").ToString() == "Renault");
                }
                catch(Exception e)
                {
                    assercao.IsTrue(false, "TESTE FALHOU: " + e.Message);
                }

            }
        }
    } 
} 

﻿using System.Collections.Generic;
using System.Collections;
using System.Linq;
using parser.ProgramacaoOrentadaAObjetos;
using System.Security.Principal;
using System;
using System.IO;
using System.Reflection;

using parser.LISP;

using Microsoft.SqlServer.Server;
using Util;

namespace parser
{

    public class TablelaDeValores
    {


        /*
         * 
        // MÉTODOS DA CLASSE TabelaDeValores

        /// PARA FUNÇÕES:
        public Funcao GetFuncao(string nomeFuncao, string tipoRetornoFuncao, escopo)
        public Funcao GetFuncao(string nomeFuncao)
        public bool RegistraFuncao(Funcao funcao)
        public string GetTypeFunction(string nomeClasseDaFuncao, string nomeFuncao, Escopo escopo)
        public Funcao IsFunction(string nomeFuncao, Escopo escopo)
        
 
        // PARA CLASSES        
        public List<Classe> GetClasses()
        public void RegistraClasse(Classe umaClasse)
        public propriedade GetPropriedade(string nomeClasse, string nomePropriedade)
        public bool ValidaTipo(string tipo)
    
        
        // PARA OBJETOS
        public Objeto GetObjeto(string nomeObjeto)
        public void RegistraObjeto(Objeto objeto)
        public void RemoveObjeto(string nomeObjeto)
        private propriedade ObtemPropriedadeDeUmObjeto(string tipoObjeto, string propriedadeDoObjeto)
        private Funcao ObtemMetodoDeUmObjeto(string tipoObjeto, string metodoDoObjeto)
        public Objeto IsObjetoRegistrado(string nomeObjeto, Escopo escopo)
        public bool ValidaObjeto(string nomeObjeto, Escopo escopo)
       
        
        // PARA VETORES:
            public GetVetor(string nome, Escopo escopo)
            public AddVetor(string acessor, string nome,string tipo, int[]dims, Escopo escopo, bool isStatic )
        
        // PARA OPERADORES:
        public bool ValidaOperador(string nomeOperador, string tipoDoOPerador, Escopo escopoCurrente)
        public static int VerificaSeEhOperador(string operador, string tipoDeOperando)
       
       
        // PARA PROPRIEDADES:
        private List<propriedade> ObtemPropriedadesEncadeadas(List<string> tokens, Escopo escopo, int indiceInicio)

        // PARA EXPRESSOES:       
        private string ObtemOTipoDaExpressao(List<string> tokens, Escopo escopo)
        public static int VerificaSeEhTermoChave(string tokenChave, List<string> termosChave)
 
         
        // PROPRIEDADES PUBLICAS:
        public List<Operador> Operadores = new List<Operador>();
         
         */



        private List<Classe> Classes = new List<Classe>();
        private List<Operador> operadores = new List<Operador>();


        /// <summary>
        /// contém as expressões validadas no escopo currente.
        /// </summary>
        public static List<Expressao> expressoes { get; set; } 

        /// <summary>
        /// contém as funções do escopo currente.
        /// </summary>
        private List<Metodo> Funcoes = new List<Metodo>(); 

        /// <summary>
        /// contem variaveis vetor (obsoleto, substituido pela classe Wrapper [Vector].
        /// </summary>
        private List<Vetor> VariaveisVetor;


        /// <summary>
        /// contém uma lista de objetos instanciados, em um escopo.
        /// </summary>
        public List<Objeto> objetos = new List<Objeto>(); 
        
        private List<string> codigo { get; set; }


        public TablelaDeValores Clone()
        {
            TablelaDeValores tabelaClone = new TablelaDeValores(this.codigo);
            if (tabelaClone != null)
            {
                tabelaClone.Classes = this.Classes.ToList<Classe>();
                tabelaClone.operadores = this.operadores.ToList<Operador>();
                TablelaDeValores.expressoes = expressoes.ToList<Expressao>();
                tabelaClone.Funcoes = this.Funcoes.ToList<Metodo>();
                tabelaClone.VariaveisVetor = new List<Vetor>();
            
                if ((this.objetos != null) && (this.objetos.Count > 0))
                {
                    for (int i = 0; i < this.objetos.Count; i++)
                    {
                        tabelaClone.objetos.Add(this.objetos[i].Clone());
                    }
                }

                tabelaClone.codigo = this.codigo.ToList<string>();
                
            }
            return tabelaClone;
        }

        private static LinguagemOrquidea lng = LinguagemOrquidea.Instance();
        public TablelaDeValores(List<string> _codigo)
        {
            lng = LinguagemOrquidea.Instance();
            if ((_codigo != null) && (_codigo.Count > 0))
                this.codigo = _codigo.ToList<string>();
            else
                this.codigo = new List<string>();
            if (expressoes == null)
                expressoes = new List<Expressao>();
        } //TabelaDeValores()

        /// <summary>
        /// adiciona as expressoes formadas, para fins de otimização.
        /// </summary>
        /// <param name="escopo">escopo onde as expressoes estao.</param>
        /// <param name="expressoesAIncluir">expressoes a adicionar.</param>
        public void AdicionaExpressoes(Escopo escopo, params Expressao[] expressoesAIncluir)
        {
            List<Expressao> expressFound = new List<Expressao>();
            expressoes.AddRange(expressoesAIncluir);
        }

      

        public void AdicionaObjetos(Escopo escopo, params Objeto[] objetos)
        {
            if ((objetos != null) && (objetos.Length > 0))
            {
                this.objetos.AddRange(objetos);
                if ((expressoes != null) && (expressoes.Count > 0))
                    for (int exprss = 0; exprss < expressoes.Count; exprss++)
                    {
                        for (int umObjeto = 0; umObjeto < objetos.Length; umObjeto++)
                        {
                            if (objetos[umObjeto].exprssPresentes().IndexOf(expressoes[exprss]) == -1)
                                objetos[umObjeto].exprssPresentes().Add(expressoes[exprss]);
                        }
                    }
            }
        }

        private List<Expressao> GetExpressoes()
        {
            return expressoes;
        } // GetExpressoes()

        public List<Operador> GetOperadores()
        {
            return this.operadores;
        }
        public List<Objeto> GetObjetos()
        {
            return this.objetos;
        }

        public List<Metodo> GetFuncoes()
        {
            return Funcoes;
        } // GetFuncoes()

        /// <summary>
        /// obtém a função nesta tabela de valores, e se não encontrar, obtém a função no repositorio de classes orquidea.
        /// </summary>
        public Metodo GetFuncao(string nomeFuncao, string classeDaFuncao, Escopo escopo)
        {
            if (escopo.ID == Escopo.tipoEscopo.escopoGlobal)
                  return FindFuncao(classeDaFuncao, nomeFuncao, escopo);

            Metodo fuctionFound = FindFuncao(classeDaFuncao, nomeFuncao, escopo);
            if (fuctionFound != null) 
                return fuctionFound;
            else
                return GetFuncao(nomeFuncao, classeDaFuncao, escopo.escopoPai);
        } // GetFuncao()



        private  Metodo FindFuncao(string classeDaFuncao, string nomeFuncao, Escopo escopo)
        {
        
            Metodo umaFuncaoDasTabelas = escopo.tabela.Funcoes.Find(k => k.nome == nomeFuncao);
            if (umaFuncaoDasTabelas != null)
                return escopo.tabela.Funcoes.Find(k => k.nome == nomeFuncao);

            if (RepositorioDeClassesOO.Instance().GetClasse(classeDaFuncao)!=null)
            {
                Metodo umaFuncaoDoRepositorio = RepositorioDeClassesOO.Instance().GetClasse(classeDaFuncao).GetMetodos().Find(k => k.nome.Equals(nomeFuncao));
                if (umaFuncaoDoRepositorio != null)
                    return umaFuncaoDoRepositorio;
            }
            return null;
        }


        /// <summary>
        /// obtem funcoes com o nome parâmetro, não importando a classe em que a função foi definida.
        /// </summary>
        public List<Metodo> GetFuncao(string nomeFuncao)        {

            List<Metodo> funcoesDaTabelaComMesmoNome = this.Funcoes.FindAll(k => k.nome.Equals(nomeFuncao));

            if (funcoesDaTabelaComMesmoNome != null)
                return funcoesDaTabelaComMesmoNome;

            List<Metodo> funcoesDoRepositorioDeClassesComMesmoNome = new List<Metodo>();
            List<Classe> lstClasses = RepositorioDeClassesOO.Instance().GetClasses();
            foreach (Classe umaClasse in lstClasses)
            {
                List<Metodo> lstFuncoes = umaClasse.GetMetodos().FindAll(k => k.nome.Equals(nomeFuncao));
               if (lstFuncoes!=null)
                {
                    funcoesDoRepositorioDeClassesComMesmoNome.AddRange(lstFuncoes);
                } // if
            } // foreach
            return funcoesDoRepositorioDeClassesComMesmoNome;
        } // GetFuncao()

      
        public List<Vetor> GetVetores()
        {
            return VariaveisVetor;
        }
        public List<Classe> GetClasses()
        {
           return Classes;
        }

        public Classe GetClasse(string nomeDaClasse, Escopo escopo)
        {
            if (escopo.ID == Escopo.tipoEscopo.escopoGlobal)
                return escopo.tabela.Classes.Find(k => k.nome.Equals(nomeDaClasse));
            else
            {
                Classe classe = escopo.tabela.Classes.Find(k => k.GetNome().Equals(nomeDaClasse));
                if (classe != null)
                    return classe;

                if (classe == null)
                    return GetClasse(nomeDaClasse, escopo.escopoPai);
            }
            return null;
        }


        public static TablelaDeValores Clone(TablelaDeValores tabela)
        {
            TablelaDeValores tabelaClone = new TablelaDeValores(tabela.codigo);
            if (tabela.GetClasses().Count > 0)
                tabelaClone.GetClasses().AddRange(tabela.GetClasses().ToList<Classe>());
            
            if (tabela.GetExpressoes().Count > 0)
                tabelaClone.GetExpressoes().AddRange(tabela.GetExpressoes().ToList<Expressao>());

            if (tabela.GetFuncoes().Count > 0)
                tabelaClone.GetFuncoes().AddRange(tabela.GetFuncoes().ToList<Metodo>());

            if (tabela.GetObjetos().Count > 0)
                tabelaClone.GetObjetos().AddRange(tabela.GetObjetos().ToList<Objeto>());

            if (tabela.GetOperadores().Count > 0)
                tabelaClone.GetOperadores().AddRange(tabela.GetOperadores().ToList<Operador>());

          
            return tabelaClone;
            
        }


        public void RegistraClasse(Classe umaClasse)
        {
            if (Classes.Find(k => k.nome == umaClasse.nome) == null)
                Classes.Add(umaClasse); 

            this.operadores.AddRange(umaClasse.GetOperadores());
        } // RegistraClasse()
        public Objeto GetObjeto(string nomeObjeto, Escopo escopo)
        {
            if (escopo == null)
                return null;
            Objeto objetoRetorno = escopo.tabela.objetos.Find(k => k.GetNome() == nomeObjeto);
            if (objetoRetorno != null)
                return objetoRetorno;

            if (escopo.ID != Escopo.tipoEscopo.escopoGlobal)
                return GetObjeto(nomeObjeto, escopo.escopoPai);

            return null;
        } // GetObjeto().


        public Objeto GetObjeto(string nomeObjeto, string tipoDoObjeto, Escopo escopo)
        {
            if (escopo == null)
                return null;
            Objeto objetoRetorno = escopo.tabela.objetos.Find(k => k.GetNome() == nomeObjeto && k.GetTipo()==tipoDoObjeto);
            if (objetoRetorno != null)
                return objetoRetorno;

            if (escopo.ID != Escopo.tipoEscopo.escopoGlobal)
                return GetObjeto(nomeObjeto,tipoDoObjeto, escopo.escopoPai);

            return null;
        } // GetObjeto().


        public Objeto GetPropriedade(string nomeClasse, string nomePropriedade)
        {
            // tenta localizar a classe da propriedade.
            Classe umaClasse = RepositorioDeClassesOO.Instance().GetClasse(nomeClasse);

            // se não localizar, retorna null.
            if (umaClasse == null)
                return null;
            
            // tenta localizar a propriedade da classe encontrada.
            int indexPropriedade = umaClasse.GetPropriedades().FindIndex(k => k.GetNome()== nomePropriedade.Trim(' '));
            // se não localizar, retorna null.
            if (indexPropriedade == -1)
            {
                indexPropriedade = umaClasse.GetPropriedades().FindIndex(k => k.GetNome() == "static." + nomePropriedade.Trim(' '));
                if (indexPropriedade == -1)
                    return null;
                else
                    return umaClasse.GetPropriedades().Find(k => k.GetNome() == "static." + nomePropriedade.Trim(' '));
            }
            // retorna a propriedade encontrada.
            return umaClasse.GetPropriedades()[indexPropriedade];

        } // GetPropriedade()
        public void RegistraObjeto(Objeto objeto)
        {
            this.objetos.Add(objeto);
        } // RegistraObjeto().


        // remove a ultima instancia do objeto com o nome de entrada.
        public void RemoveObjeto(string nomeObjeto)
        {

            int index = this.objetos.FindLastIndex(k => k.GetNome() == nomeObjeto);
            if (index != -1)
                this.objetos.RemoveAt(index);

        } // RemoveObjeto()
        public bool RegistraFuncao(Metodo funcao)
        {
            this.Funcoes.Add(funcao);
            return true;
        }
     

        public bool ValidaTipo(string tipo)
        {
            for (int x = 0; x < this.Classes.Count; x++)
                if (this.Classes[x].GetNome() == tipo)
                    return true;
            return false;
        } // ValidaTipo()


        public bool ValidaOperador(string nomeOperador, string tipoDoOPerador, Escopo escopo)
        {

            if (RepositorioDeClassesOO.Instance().ExisteClasse(tipoDoOPerador))
            {
                if (Expressao.headers != null)
                {
                    HeaderClass header = Expressao.headers.cabecalhoDeClasses.Find(k => k.nomeClasse == tipoDoOPerador);
                    if (header == null)
                        return false;
                    else
                    {
                        return header.operators.Find(k => k.name == nomeOperador) != null;
                    }
                }

            }
            if (escopo.tabela.GetOperadores().Find(k => k.nome.Equals(nomeOperador) && (k.tipoRetorno.Equals(tipoDoOPerador))) != null)
                return true;

            if (escopo.ID != Escopo.tipoEscopo.escopoGlobal)
                return ValidaOperador(nomeOperador, tipoDoOPerador, escopo.escopoPai);
            return false;
        } // ValidaOperador()
    
        
        /// <summary>
        ///  obtém uma propriedade, que é de uma sequencia de propriedades aninhadas.
        /// </summary>
        internal Objeto ValidaPropriedadesAninhadas(string classeDaPropriedade, string nomeDaPropriedade)
        {
            Classe clsObjeto = RepositorioDeClassesOO.Instance().GetClasse(classeDaPropriedade);
            if (clsObjeto == null)
                return null;

            // "class A { int varA;} class B {A varB;}" };
            // propriedade prop = escopo.tabela.ObtemPropriedadeAninhada("B", "varB.varA");

            List<Objeto> props = new List<Objeto>();

            string[] proppriedadesAninhadas = nomeDaPropriedade.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            for (int x = 0; x < proppriedadesAninhadas.Length; x++)
            {
                string nomeLongo = clsObjeto.GetNome() + "." + proppriedadesAninhadas[x];

                Objeto proprSendoVerificada = clsObjeto.GetPropriedades().Find(k => k.GetNome().Equals(nomeLongo));
                if (proprSendoVerificada == null)
                    return null;
                props.Add(proprSendoVerificada);

                clsObjeto = RepositorioDeClassesOO.Instance().GetClasse(props[props.Count - 1].GetTipo());
            } // for x
            if (props.Count < 0)
                return null;
            return props[props.Count - 1];
        } //GetPropriedadeEncadeadaDeUmaExpressao()


        public bool IsClasse(string nomeClasse, Escopo escopo)
        {
            if (escopo == null)
                return false;
            int indexClasse = escopo.tabela.GetClasses().FindIndex(k => k.GetNome() == nomeClasse);
            if (indexClasse != -1)
                return true;
            if (escopo.ID == Escopo.tipoEscopo.escopoNormal)
                return IsClasse(nomeClasse, escopo.escopoPai);
            return false;

        }  // IsClasse()

    
 

    

        public void AddObjetoVetor(string acessor, string nome, string tipo, int[] dims, Escopo escopo, bool isStatic, string tipoElemento)
        {
            Vetor v = new Vetor(acessor, nome, tipo, escopo, dims);
            v.tipoElemento = tipoElemento;
            v.SetAcessor(acessor);
            v.isStatic = isStatic;
            escopo.tabela.VariaveisVetor.Add(v);
        } // RegistraVetor()

        public void AddObjeto(string acessor,string nome, string tipo, object valor, Escopo escopo)
        {
            Objeto objeto = new Objeto(acessor, tipo, nome, valor);
            objeto.SetTipoElement(tipo);
            escopo.tabela.objetos.Add(objeto);
        }

   
        public Vetor GetObjetoVetor(string nomeVariavel, Escopo escopo, params int[] indices)
        {
            if (escopo == null)
                return null;
            Vetor v = escopo.tabela.VariaveisVetor.Find(k => k.GetNome() == nomeVariavel);
            if (v != null)
            {
                return GetElementoVetor(escopo, indices, ref v);
            } // if
            Vetor vEstatica = escopo.tabela.VariaveisVetor.Find(k => k.GetNome() == "static." + nomeVariavel);
            if (vEstatica != null)
            {
                return GetElementoVetor(escopo, indices, ref v);
            }

            if (escopo.ID != Escopo.tipoEscopo.escopoGlobal)
                return GetVetor(nomeVariavel, escopo.escopoPai);
            return null;
        } // GetObjetoVetor()

        public Vetor GetVetor(string nomeVariavel, Escopo escopo)
        {
            if (escopo == null)
                return null;
            Vetor v = this.GetVetores().Find(k => k.GetNome() == nomeVariavel);
            if (v != null)
                return v;
            else
            if (escopo.ID == Escopo.tipoEscopo.escopoGlobal)
                return null;
            else
                return GetVetor(nomeVariavel, escopo.escopoPai);
        } // GetObjetoVetor()


        private static Vetor GetElementoVetor(Escopo escopo, int[] indices, ref Vetor v)
        {
            Vetor vt_result = new Vetor(v.GetAcessor(), v.GetNome(), v.GetTiposElemento(), escopo, v.dimensoes);
            for (int index = 0; index < indices.Length; index++)
                vt_result = vt_result.tailVetor[indices[index]];
            return vt_result;
        }

        public string GetTypeFunction(string nomeClasseDaFuncao, string nomeFuncao, Escopo escopo)
        {
            if (escopo == null)
                return null;
            Metodo funcao = escopo.tabela.Funcoes.Find(k => k.nome == nomeFuncao);
            if (funcao != null)
                return funcao.tipoReturn;

            Metodo umaFuncaoDoRepositorioDeClasses = RepositorioDeClassesOO.Instance().GetClasse(nomeClasseDaFuncao).GetMetodos().Find(k => k.nome.Equals(nomeFuncao));
            if (umaFuncaoDoRepositorioDeClasses != null)
                return umaFuncaoDoRepositorioDeClasses.tipoReturn;

            if (escopo.escopoPai.ID != Escopo.tipoEscopo.escopoGlobal)
                return GetTypeFunction(nomeClasseDaFuncao, nomeFuncao, escopo.escopoPai);
            return null;
        }

      

        public Objeto IsObjetoRegistrado(string nomeObjeto, Escopo escopo)
        {
            if (escopo == null)
                return null;
            Objeto objeto = escopo.tabela.objetos.Find(k => k.GetNome() == nomeObjeto);
            if (objeto != null)
                return objeto;
            if (escopo.ID != Escopo.tipoEscopo.escopoGlobal)
                return IsObjetoRegistrado(nomeObjeto, escopo.escopoPai);
            return null;
        } // IsObjectRegistrade()



        public bool IsFunction(Escopo escopo, string nameFuncion)
        {
            return (IsFunction(nameFuncion, escopo) != null);
        }


        /// <summary>
        /// verifica se um token é nome de uma função.
        /// </summary>
        public Metodo IsFunction(string nomeFuncao, Escopo escopo)
        {
            if (escopo == null)
                return null;
            Metodo funcao = escopo.tabela.Funcoes.Find(k => k.nome == nomeFuncao);
            if (funcao != null)
                return funcao;

            if (escopo.ID != Escopo.tipoEscopo.escopoGlobal)
                return IsFunction(nomeFuncao, escopo.escopoPai);
            return null;
        } // IsFunction()

       

        public bool ValidaObjeto(string nomeObjeto, Escopo escopo)
        {

            if (escopo == null)
                return false;
            if (escopo.tabela.objetos.Find(k => k.GetNome() == nomeObjeto) != null)
                return true;

            if (escopo.ID != Escopo.tipoEscopo.escopoGlobal)
                return ValidaObjeto(nomeObjeto, escopo.escopoPai);
            return false;
        } // ValidaVariavel()

        public bool ValidaTipoObjeto( string tipoVariavel, Escopo escopo)
        {
            if (escopo == null)
                return false;
            foreach (Classe umaClasse in escopo.tabela.Classes)
            {
                if (umaClasse.GetNome() == tipoVariavel)
                    return true;
            }// foreach

            
            if (escopo.ID != Escopo.tipoEscopo.escopoGlobal)
                return (ValidaTipoObjeto(tipoVariavel, escopo.escopoPai));
            return false;
        } // ValidaTipoVariavel()

        

    } // class TabelaDeValores

    public class Vetor: Objeto
    {
        // o valor de um elemento do vetor é o valor do Objeto associado.
        public new string nome;
        private new string tipo;

        public List<Vetor> tailVetor { get; set; }

        public int[] dimensoes;


        public string GetTiposElemento()
        {
            return tipo;
        }


        public Vetor()
        {
            this.tailVetor = new List<Vetor>();
            this.nome = "";
            this.tipo = "";
           
            this.dimensoes = new int[1]; 
        }


        public Vetor(string acessor, string nome, string tipoElementoVetor, Escopo escopo, params int[] dims) : base(acessor, "Vetor", nome, null)
        {
            Init(nome, tipoElementoVetor, dims);

            for (int x = 0; x < dims.Length; x++) // inicializa as variaveis vetor de elementos, para evitar recursão inifinita.
            {
                this.tailVetor.Add(new Vetor());
                this.tailVetor[tailVetor.Count - 1].Init(nome, tipoElementoVetor, dims);
            }

        }
        public void AddElements(int qtdDeElementos)
        {
            if (this.tailVetor == null)
                this.tailVetor = new List<Vetor>();
            if (qtdDeElementos <= 0)
                return;
            else
                for (int x = 0; x < qtdDeElementos; x++)
                    this.tailVetor.Add(new Vetor());
        }



        private void Init(string nomeVariavel, string tipoElemento, int[] dims)
        {
            this.tipo = tipoElemento;
            this.nome = nomeVariavel;
            this.dimensoes = dims;

            this.tailVetor = new List<Vetor>();

            if (this.dimensoes == null)
                this.dimensoes = new int[1];
        }



        /// <summary>
        /// seta o elemento com os indices matriciais de entrada. 
        /// Util para modificar um elemento com dimensões bem definidas: M[1,5,8] um vetor, e queremos acessar
        /// a variavel: m[0,0,3].
        /// </summary>
        public void SetElementoPorOffset(List<Expressao> exprssIndices, object newValue, Escopo escopo)
        {
            EvalExpression eval = new EvalExpression();
            List<int> indices = new List<int>();
            for (int x = 0; x < exprssIndices.Count; x++)
            {
                exprssIndices[x].isModify = true;
                indices.Add(int.Parse(eval.EvalPosOrdem(exprssIndices[x], escopo).ToString()));
            }
            int indexOffet = this.BuildIndex(indices.ToArray());
            this.tailVetor[indexOffet].SetValor(newValue);
            
        }

        /// <summary>
        /// seta elemento com elementos vetor dentro de vetores, como: [[1,5],2,6,8,[1,3,5]].
        /// Para futuros novos tipos de vetor, como JaggedArray.
        /// </summary>
        public void SetElementoAninhado(object newValue, Escopo escopo, params Expressao[] exprssoesIndices)
        {
            List<int> indices = new List<int>();
            EvalExpression eval = new EvalExpression();
            for (int k = 0; k < exprssoesIndices.Length; k++)
            {
                exprssoesIndices[k].isModify = true;
                indices.Add(int.Parse(eval.EvalPosOrdem(exprssoesIndices[k], escopo).ToString()));
            }

            Vetor vDinamico = this;
            for (int x = 0; x < indices.Count - 1; x++)
                if (vDinamico.tailVetor[indices[x]].GetType() == typeof(Vetor))
                  vDinamico = vDinamico.tailVetor[indices[x]]; // o elemento do vetor eh outro vetor.
                else
                {
                    vDinamico.tailVetor[indices[x]].SetValor(newValue); // o elemento eh um objeto, não um Vetor.
                    return;
                }

            vDinamico.tailVetor[indices.Count - 1].SetValor(newValue);
        }
        

        /// <summary>
        /// constroi um indice de acessso de vetores com varias dimensoes. Eh um offset de
        /// endereço onde esta localizado a variavel que queremos acessar, dentro da variavel vetor.
        /// Util quando temos um vetor como: vetor[4,5,7], e queremos o elemento vetor[1,2,5].
        /// </summary>
        public int BuildIndex(int[] indices)
        {
            if (indices.Length != this.dimensoes.Length)
                return -1;

            int indiceTotal = 0;
            for (int x = 0; x < this.dimensoes.Length; x++)
            {
                indiceTotal += (this.dimensoes[x] - 1) * indices[x];
            }
            return indiceTotal;
        }


        public object GetElemento(Escopo escopo, params int[] indices)
        {

            int indexElemento = BuildIndex(indices);
            return this.tailVetor[indexElemento].GetValor();
        }


        public override string ToString()
        {
            string str = this.GetTiposElemento() + " " + this.nome + " [ ";

            int x = 0;
            for (x = 0; x < this.tailVetor.Count - 1; x++)
                str += this.dimensoes[x] + ",";

            str += this.dimensoes[x] + " ]";


            return str;
        }
    }
} // namespace

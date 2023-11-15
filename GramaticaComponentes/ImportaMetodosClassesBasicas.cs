using parser.ProgramacaoOrentadaAObjetos;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace parser
{
    public abstract class ImportaMetodosClassesBasicas
    {



        /// <summary>
        /// carrega os metodos do tipo importado.
        /// </summary>
        /// <param name="nomeClasseBasica">nome da classe importada.</param>
        /// <param name="classeImportada">contem os metodos importados,da classe parametro.</param>
        public void LoadMethods(string nomeClasseBasica, Type classeImportada)
        {

            MethodInfo[] metodos = classeImportada.GetMethods();
            if (metodos != null)
            {
                for (int x = 0; x < metodos.Length; x++)
                {

                    ParameterInfo[] parametrosImportados = metodos[x].GetParameters();
                    List<Objeto> parametrosObjeto = new List<Objeto>();


                    if (parametrosImportados != null)
                    {




                        /// constroi os parâmetros objetos, dos parâmetros do método importado.
                        for (int i = 0; i < parametrosImportados.Length; i++)
                        {
                            Objeto umObjParametro = new Objeto("private", UtilTokens.Casting(parametrosImportados[i].ParameterType.ToString()), parametrosImportados[i].Name, null);
                            parametrosObjeto.Add(umObjParametro);
                        }

                        // constroi o metodo importado.
                        Metodo umMetodo = new Metodo(nomeClasseBasica, "public", metodos[x].Name, metodos[x], UtilTokens.Casting(metodos[x].ReturnType.Name), parametrosObjeto.ToArray());

                        // sinaliza que deve incluir na lista de parâmetros do método, o objeto caller (o que chamou o metodo, uma chamada de metodo).
                        umMetodo.isToIncludeCallerIntoParameters = false;

                        /// adiciona o metodo à classe "string" orquidea.
                        RepositorioDeClassesOO.Instance().GetClasse(nomeClasseBasica).GetMetodos().Add(umMetodo);
                    }


                }
            }


        }
    }
}
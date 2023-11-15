using parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wrappers.DataStructures
{

    public class AtomList: MultList
    {
        public Objeto atom;

        public AtomList()
        {
            this.atom = null;
        }
        public AtomList(Objeto atom)
        {
            this.atom = atom;
        }

        
    }
    public class MultList
    {
        private List<AtomList> list
        {
            get;
            set;
        }


        public MultList()
        {
            this.list = new List<AtomList>();
        }



        public void InsertList(AtomList listToAdd)
        {
            this.list.Add(listToAdd);
        }

        public void InsertAtomList(Objeto objToAdd)
        {
            AtomList atom = new AtomList(objToAdd);
            this.list.Add(atom);
        }


        /// <summary>
        /// insere um objeto nas coordenadas de lista do parametro de entrada.
        /// </summary>
        /// <param name="objToAdd">objeto a ser adicionado na lista.</param>
        /// <param name="indices">vetor contendo os indices de inserção.</param>
        public void InsertAt(Objeto objToAdd, Vector indices)
        {
            AtomList lista = (AtomList)this;
            for (int x = 0; x < indices.VectorObjects.Length; x++)
            {
                lista = lista.list[(int)indices.Get(x)];
            }

            if (lista != null)
            {
                lista.list.Add(new AtomList(objToAdd));
            }

        }


        /// <summary>
        /// obtem o elemento na posicao de listas do parametro de entrada.
        /// </summary>
        /// <param name="indices">vetor contendo os indices.</param>
        /// <returns></returns>
        public Objeto Get(Vector indices)
        {
            AtomList lista = (AtomList)this;
            for (int x = 0; x < indices.VectorObjects.Length-1; x++)
            {
                lista = lista.list[(int)indices.Get(x)];
            }

            return lista.list[indices.VectorObjects.Length - 1].atom;
        }


    }
}

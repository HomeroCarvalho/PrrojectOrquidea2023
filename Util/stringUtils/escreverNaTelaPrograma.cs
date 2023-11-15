using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace parser
{
    public class Utils
    {
        /// <summary>
        /// constoi uma string com os tokens de entrada.
        /// </summary>
        /// <param name="tokens">tokens de processamento.</param>
        /// <returns></returns>
        public static string OneLineTokens(List<string> tokens)
        {
            if ((tokens == null) || (tokens.Count==0))
            {
                return null;
            }
            string textTotal = "";
            foreach (string line in tokens)
                textTotal += line + " ";
            return textTotal;
        } // UneLinhasPrograma()
    } // class Utils
} // namespace

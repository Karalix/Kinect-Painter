using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReconnaissancePosture
{
    public class PostureChangéeEventArgs : EventArgs
    {
        /// <summary>
        /// Le nom de la posture en question.
        /// </summary>
        public String NomPosture
        {
            get;
            private set;
        }

        /// <summary>
        /// Indique si le squelette est entré dans la posture.
        /// </summary>
        public bool EntréeEnPosture
        {
            get;
            private set;
        }

        /// <summary>
        /// Indique si le squelette a quitté la posture.
        /// </summary>
        public bool SortieDePosture
        {
            get;
            private set;
        }

        /// <summary>
        /// Le numéro du squelette dont la posture à changé.
        /// </summary>
        public int NuméroSquelette
        {
            get;
            set;
        }

        /// <summary>
        /// Contruit un PostureChangéeEventArgs.
        /// </summary>
        /// <param name="nomPosture">Le nom de la posture en question.</param>
        /// <param name="entréeEnPosture">Indique si le squelette est entré dans la posture.</param>
        /// <param name="sortieDePosture">Indique si le squelette a quitté la posture.</param>
        /// <param name="numéroSquelette">Le numéro du squelette dont la posture à changé.</param>
        public PostureChangéeEventArgs(String nomPosture, 
                                        bool entréeEnPosture,
                                        bool sortieDePosture,
                                        int numéroSquelette)
        {
            this.NomPosture = nomPosture;
            this.EntréeEnPosture = entréeEnPosture;
            this.SortieDePosture = sortieDePosture;
            this.NuméroSquelette = numéroSquelette;
        }
    }
}

using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReconnaissancePosture
{
    /// <summary>
    /// Permet de reconnaitre la posture main gauche dans le dos.
    /// </summary>
    public class POMainGaucheDansLeDos : PostureAbstraite
    {
        private static readonly String NOM_POSTURE = "MAIN_GAUCHE_DANS_LE_DOS";

        /// <summary>
        /// Construite un objet POMainGaucheDansLeDos
        /// avec comme nom "MAIN_GAUCHE_DANS_LE_DOS".
        /// </summary>
        public POMainGaucheDansLeDos()
             :base(NOM_POSTURE)
        {}

        /// <summary>
        /// Permet de reconnaitre cette posture.
        /// </summary>
        /// <param name="squelette">Le squelette à tester.</param>
        /// <returns>Vrai si le suqelette est dans cette posture, faux sinon.</returns>
        public override bool reconnaitre(Skeleton squelette)
        {
            throw new NotImplementedException();
        }
    }
}

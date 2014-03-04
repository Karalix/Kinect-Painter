using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReconnaissancePosture
{
    public abstract class PostureAbstraite : IEquatable<PostureAbstraite>
    {
        /// <summary>
        /// Donne le nom de cette posture.
        /// </summary>
        public String Nom
        {
            get;
            private set;
        }

        /// <summary>
        /// Construit une PostureAbstraite d'après son nom.
        /// </summary>
        /// <param name="nom">Le nom de la posture à construire.</param>
        public PostureAbstraite(String nom)
        {
            Nom = nom;
        }

        /// <summary>
        /// Donne le HashCode de cette posture.
        /// Soit : Nom.getHashCode()
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Nom.GetHashCode();
        }

        /// <summary>
        /// Permet de reconnaître cette posture chez un squelette donné.
        /// </summary>
        /// <param name="squelette">Le squelette à tester.</param>
        /// <returns>Vrai si le squelette est dans cette posture. Faux sinon.</returns>
        public abstract Boolean reconnaitre(Skeleton squelette);

        /// <summary>
        /// Teste l'égalité avec un autre posture.
        /// Soit this.Nom.Equals(other.Nom)
        /// </summary>
        /// <param name="other">La posture avec laquelle on veut comparer.</param>
        /// <returns>Vrai si les deux postures sont égalles. Faux sinon.</returns>
        public bool Equals(PostureAbstraite other)
        {
            return this.Nom.Equals(other.Nom);
        }

        /// <summary>
        /// Teste l'égalité avec un autre objet.
        /// </summary>
        /// <param name="right">L'objet à tester.</param>
        /// <returns>Vrai si oui, faux sinon.</returns>
        public override bool Equals(object right)
        {
            if (object.ReferenceEquals(right, null))
            {
                return false;
            }

            if (object.ReferenceEquals(this, right))
            {
                return true;
            }

            if (this.GetType() != right.GetType())
            {
                return false;
            }

            return this.Equals(right as PostureAbstraite);
        }
    }
}

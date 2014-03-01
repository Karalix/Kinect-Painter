using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Samples.Kinect.ColorBasics
{
    abstract class PostureAbstraite : IEquatable<PostureAbstraite>
    {
        public readonly String Nom
        {
            get;
            private set;
        }

        public PostureAbstraite(String nom)
        {
            Nom = nom;
        }

        public override int GetHashCode()
        {
            return Nom.GetHashCode();
        }



        public abstract Boolean reconnaitre(Skeleton squelette);

        public bool Equals(PostureAbstraite other)
        {
            return this.Nom.Equals(other.Nom);
        }

        public override bool Equals(object right)
        {
            //check null
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

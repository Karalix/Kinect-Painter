using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Samples.Kinect.ColorBasics
{
    class PostureChangéeEventArgs : EventArgs
    {
        public String NomPosture
        {
            get;
            private set;
        }

        public bool EntréeEnPosture
        {
            get;
            private set;
        }

        public bool SortieDePosture
        {
            get;
            private set;
        }

        public int NuméroSquelette
        {
            get;
            set;
        }

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

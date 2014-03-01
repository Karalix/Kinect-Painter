using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReconnaissancePosture
{
    class ControleurPosture
    {
        private HashSet<PostureAbstraite> postures = new HashSet<PostureAbstraite>();
        private KinectSensor sensor;
        private int maxSquelettes;
        private Skeleton[] tabSquelettes;
        private PostureAbstraite[] postureActuellesParSquelette;

        /// <summary>
        /// Lorsque la posture d'un des squelettes traités change.
        /// </summary>
        public event EventHandler<PostureChangéeEventArgs> PostureChangée;

        /// <summary>
        /// Construit un nouveau controleur de posture.
        /// </summary>
        /// <param name="sensor">Le capteur Kinect utilisé</param>
        /// <param name="maxSquelettes">Nombre maximum de squelettes à traiter</param>
        public ControleurPosture(KinectSensor sensor, int maxSquelettes)
        {
            this.sensor = sensor;
            this.maxSquelettes = maxSquelettes;
            postureActuellesParSquelette = new PostureAbstraite[maxSquelettes];
        }

        /// <summary>
        /// Démarre la reconnaissance de posture.
        /// </summary>
        public void DemarrerReconnaissance()
        {
            this.sensor.SkeletonFrameReady += OnSkeletonFrameReady;
        }

        /// <summary>
        /// Arrête la reconnaissance de posture.
        /// </summary>
        public void StopperReconnaissance()
        {
            this.sensor.SkeletonFrameReady -= OnSkeletonFrameReady;
        }
        
        private void OnSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                frame.CopySkeletonDataTo(tabSquelettes);

                for (int i = 0; i < tabSquelettes.Length && i < maxSquelettes; i++)
                {
                    PostureAbstraite postureReconnue = getPostureReconnue(tabSquelettes[i]);

                    testerEntréesEtSortiesDePostures(postureActuellesParSquelette[i], postureReconnue, i);

                    postureActuellesParSquelette[i] = postureReconnue;
                }
            }
        }

        private void testerEntréesEtSortiesDePostures(PostureAbstraite postureActuelle, 
                                                      PostureAbstraite postureReconnue,
                                                      int numSquelette)
        {
            if (postureReconnue == null)
            {
                if (postureActuelle != null)
                {
                    onPostureChangée(
                        new PostureChangéeEventArgs(postureActuelle.Nom, false, true, numSquelette));
                }
            }
            else
            {
                if (postureActuelle == null)
                {
                    onPostureChangée(
                        new PostureChangéeEventArgs(postureReconnue.Nom, true, false, numSquelette));
                }
                else if (!postureActuelle.Equals(postureReconnue))
                {
                    onPostureChangée(
                        new PostureChangéeEventArgs(postureActuelle.Nom, false, true, numSquelette));

                    onPostureChangée(
                        new PostureChangéeEventArgs(postureReconnue.Nom, true, false, numSquelette));
                }
            }
        }

        private PostureAbstraite getPostureReconnue(Skeleton squelette)
        {
            foreach (PostureAbstraite p in postures)
            {
                if (p.reconnaitre(squelette))
                    return p;
            }

            return null;
        }

        private void onPostureChangée(PostureChangéeEventArgs args)
        {
            if (PostureChangée != null)
            {
                PostureChangée(this, args);
            }
        }

        /// <summary>
        /// Ajouter une posture à reconnaître.
        /// </summary>
        /// <param name="posture">La posture en question.</param>
        public void ajouterPosture(PostureAbstraite posture)
        {
            postures.Add(posture);
        }

        /// <summary>
        /// Retirer une posture de la liste des postures à reconnaître.
        /// </summary>
        /// <param name="posture"></param>
        public void retirerPosture(PostureAbstraite posture)
        {
            postures.Remove(posture);
        }
    }
}

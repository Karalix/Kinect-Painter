using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HelixToolkit.Wpf;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.Controls;
using Microsoft.Kinect.Toolkit.Interaction;
using System.Windows.Forms;

namespace TestHelix
{
    public partial class MainWindow : Window
    {
        LinesVisual3D lignes = new LinesVisual3D();
        PointsVisual3D tmpPoints;
        LinesVisual3D tmpArrete;

        List<PointsVisual3D> listePoints = new List<PointsVisual3D>();
        List<LinesVisual3D> listeLignes = new List<LinesVisual3D>();

        MeshVisual3D mv = new MeshVisual3D();

        Point3DCollection bufSkel = new Point3DCollection();
        /*
        Point3DCollection bufPoint = new Point3DCollection();
        Point3DCollection bufArrete = new Point3DCollection();
        */
        Boolean isDessin = false;
        private KinectSensor kinect = null;
        private Timer timer = new Timer();
        private Skeleton[] players = new Skeleton[2];

        UserInfo[] userInfos = new UserInfo[InteractionFrame.UserInfoArrayLength];
        InteractionStream interStream;
        InteractionHandEventType last;

        public MainWindow()
        {
            InitializeComponent();

            ViewPort.Children.Add(lignes);
            ViewPort.Children.Add(mv);

            initKinect();
        }

        private void initKinect()
        {
            if (KinectSensor.KinectSensors.Count > 0)
                kinect = KinectSensor.KinectSensors[0];
            else
            {
                info.Content = "Pas de kinect connectée";
            }
            try
            {
                kinect.SkeletonStream.Enable();
                kinect.DepthStream.Enable();

                interStream = new InteractionStream(kinect, new DummyInteraction());

                kinect.DepthFrameReady += KinectOnDepthFrameReady;
                kinect.SkeletonFrameReady += KinectOnSkeletonFrameReady;

                interStream.InteractionFrameReady += interStream_InteractionFrameReady;
               
                kinect.Start();
            }
            catch(Exception e)
            {
               // throw e;
            }
            
        }

        private void interStream_InteractionFrameReady(object sender, InteractionFrameReadyEventArgs e)
        {
            using (InteractionFrame inf = e.OpenInteractionFrame())
            {
                if (inf == null)
                {
                    return;
                }
                inf.CopyInteractionDataTo(userInfos);
            }

            foreach (UserInfo ui in userInfos)
            {
                var hands = ui.HandPointers;
                if (ui.SkeletonTrackingId == 0)
                    continue;

                foreach (InteractionHandPointer hand in hands)
                {
                    last = hand.HandEventType == InteractionHandEventType.None ? last : hand.HandEventType;
                    if (last == InteractionHandEventType.Grip)
                    {
                        checkDraw.IsChecked = true;
                    }
                    else 
                    {
                        if(checkDraw.IsChecked == true)
                            checkDraw.IsChecked = false;
                    }
                }

            }
        }

        private void KinectOnDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame dif = e.OpenDepthImageFrame())
            {
                if (dif == null)
                {
                    return;
                }
                try
                {
                    interStream.ProcessDepth(dif.GetRawPixelData(), dif.Timestamp);
                }
                catch (InvalidOperationException)
                {

                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (kinect != null)
            {
                if (kinect.SkeletonStream.IsEnabled)
                    kinect.SkeletonStream.Disable();

                if (kinect.IsRunning)
                    kinect.Stop();
            }
        }

        private void KinectOnSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs eventArgs)
        {
            using (SkeletonFrame skeletonFrame = eventArgs.OpenSkeletonFrame())
            {

                if (skeletonFrame != null)
                {
                    int i = 0;
                    Skeleton[] squelettes = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(squelettes);

                    try
                    {
                        var accelerometerReading = kinect.AccelerometerGetCurrentReading();
                        interStream.ProcessSkeleton(squelettes, accelerometerReading, skeletonFrame.Timestamp);
                    }
                    catch (InvalidOperationException)
                    {}

                    bufSkel.Clear();

                    foreach (
                        Skeleton squelette in
                            squelettes.Where(skel => skel.TrackingState == SkeletonTrackingState.Tracked))
                    {
                        players[i] = squelette;
                        drawBone(squelette, JointType.Head, JointType.ShoulderCenter, Brushes.AliceBlue);

                        drawBone(squelette, JointType.ShoulderCenter, JointType.ShoulderLeft, Brushes.AliceBlue);
                        drawBone(squelette, JointType.ShoulderCenter, JointType.ShoulderRight, Brushes.AliceBlue);

                        drawBone(squelette, JointType.ShoulderCenter, JointType.Spine, Brushes.AliceBlue);

                        drawBone(squelette, JointType.Spine, JointType.HipCenter, Brushes.AliceBlue);

                        drawBone(squelette, JointType.HipCenter, JointType.HipLeft, Brushes.AliceBlue);
                        drawBone(squelette, JointType.HipCenter, JointType.HipRight, Brushes.AliceBlue);

                        drawBone(squelette, JointType.ShoulderLeft, JointType.ElbowLeft, Brushes.AliceBlue);
                        drawBone(squelette, JointType.ElbowLeft, JointType.WristLeft, Brushes.AliceBlue);
                        drawBone(squelette, JointType.WristLeft, JointType.HandLeft, Brushes.AliceBlue);

                        drawBone(squelette, JointType.ShoulderRight, JointType.ElbowRight, Brushes.AliceBlue);
                        drawBone(squelette, JointType.ElbowRight, JointType.WristRight, Brushes.AliceBlue);
                        drawBone(squelette, JointType.WristRight, JointType.HandRight, Brushes.AliceBlue);
                        
                        // Left Leg
                        drawBone(squelette, JointType.HipLeft, JointType.KneeLeft, Brushes.AliceBlue);
                        drawBone(squelette, JointType.KneeLeft, JointType.AnkleLeft, Brushes.AliceBlue);
                        drawBone(squelette, JointType.AnkleLeft, JointType.FootLeft, Brushes.AliceBlue);

                        // Right Leg
                        drawBone(squelette, JointType.HipRight, JointType.KneeRight, Brushes.AliceBlue);
                        drawBone(squelette, JointType.KneeRight, JointType.AnkleRight, Brushes.AliceBlue);
                        drawBone(squelette, JointType.AnkleRight, JointType.FootRight, Brushes.AliceBlue);

                        ++i;

                    }

                    lignes.Points = bufSkel;

                }
            }
        }

        private void drawBone(Skeleton squelette, JointType articulation1, JointType articulation2, SolidColorBrush color)
        {
            Joint art1 = squelette.Joints[articulation1];
            Joint art2 = squelette.Joints[articulation2];

            if (art1.TrackingState == JointTrackingState.NotTracked || art2.TrackingState == JointTrackingState.NotTracked)
                return;

            ajouterLigne(new Point3D(art1.Position.X, art1.Position.Y, art1.Position.Z), 
                new Point3D(art2.Position.X, art2.Position.Y, art2.Position.Z), color.Color, 0.05f);
        }

        private void ajouterLigne(Point3D p1, Point3D p2, Color color, float thickness)
        {
            bufSkel.Add(p1);
            bufSkel.Add(p2);
            lignes.Color = color;
            lignes.Thickness = thickness;
        }

        private void SetDraw(object sender, RoutedEventArgs e)
        {
            isDessin = true;
            if (checkDraw.IsChecked.GetValueOrDefault())
            {

                tmpArrete = new LinesVisual3D();
                tmpArrete.Color = Brushes.Orange.Color;
                tmpArrete.Thickness = 0.01;

                tmpPoints = new PointsVisual3D();
                tmpPoints.Color = Brushes.Orange.Color;
                tmpPoints.Size = 0.05;

                listePoints.Add(tmpPoints);
                listeLignes.Add(tmpArrete);

                ViewPort.Children.Add(listeLignes.Last());
                ViewPort.Children.Add(listePoints.Last());
                if (isDessin == true)
                    nbTraits.Content = "Nombre de traits : " + listeLignes.Count;

                    timer.Tick += new EventHandler(drawPoints);
                timer.Interval = 41;
                timer.Start();
            }

            

        }

        private void drawPoints(object sender, EventArgs e)
        {
            Skeleton skel = players[0];
                if (skel != null)
                {
                    

                    Joint main = skel.Joints[JointType.HandRight];
                    Point3D newP = new Point3D(main.Position.X, main.Position.Y, main.Position.Z);
                    /*
                    if( bufPoint.Count > 0)
                    {
                        bufArrete.Add(bufPoint.Last());
                        bufArrete.Add(newP);
                    }
                    bufPoint.Add(newP);*/


                    tmpArrete.Points.Add(newP);
                    tmpPoints.Points.Add(newP);
                }
            
        }

        private void ClearSpace(object sender, RoutedEventArgs e)
        {
            listeLignes.Clear();
            listePoints.Clear();
        }

        //changer la couleur des points et arretes
        private void ChangeColor(Color color)
        {
            tmpPoints.Color = color;
            tmpArrete.Color = color;
        }

        //changer la taille des points
        private void changeSize(double size)
        {
            try
            {
                tmpPoints.Size = size;
                tmpArrete.Thickness = size - 0.04;
            }
            catch
            {
            }
        }

        /**
         * Ceci ne marche pas encore
         * 
         * Cette méthode créée un cube à la position de la main droite de l'utilisateur
         * 
         */
        private void buildCube(object sender, RoutedEventArgs e)
        {
            Skeleton s = players[0];
                if (s != null)
                {
                    Joint main = s.Joints[JointType.HandRight];
                    Point3D newP = new Point3D(main.Position.X, main.Position.Y, main.Position.Z);
                    
                

            }
        }


        private void setCameraPerso(object sender, RoutedEventArgs e)
        {
            if (players[0] != null)
            {
                Joint cible = players[0].Joints[JointType.Spine];
                Point3D positionCamera = new Point3D(cible.Position.X, cible.Position.Y, cible.Position.Z + 3);
                ViewPort.Camera = new PerspectiveCamera(new Point3D(cible.Position.X, cible.Position.Y, cible.Position.Z + 3), new Vector3D(0, 0, -1), new Vector3D(0, 1, 0), 100);

                //ViewPort.CameraController.CameraTarget = new Point3D(cible.Position.X, cible.Position.Y, cible.Position.Z);

            }
        }

        private void resetCam(object sender, RoutedEventArgs e)
        {
            ViewPort.ResetCamera();
        }

        private void buildBrosse(object sender, RoutedEventArgs e)
        {
            changeSize(0.10);
        }

        private void buildCrayon(object sender, RoutedEventArgs e)
        {
            changeSize(0.05);
        }

        private void buildPinceau(object sender, RoutedEventArgs e)
        {
            changeSize(0.08);
        }

        private void kinectRegion_HandPointerGrip(object sender, Microsoft.Kinect.Toolkit.Controls.HandPointerEventArgs e)
        {
                timer.Tick += new EventHandler(drawPoints);
                timer.Interval = 41;
                timer.Start();
        }

        private void kinectRegion_HandPointerGripRelease(object sender, Microsoft.Kinect.Toolkit.Controls.HandPointerEventArgs e)
        {
            timer.Tick -= drawPoints;
            timer.Stop();

        }

        private void unsetDraw(object sender, RoutedEventArgs e)
        {
            if (!checkDraw.IsChecked.GetValueOrDefault())
            {
                timer.Tick -= drawPoints;
                isDessin = false;
            }
        }
    }
}

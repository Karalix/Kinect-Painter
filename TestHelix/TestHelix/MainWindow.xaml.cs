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
using System.Windows.Forms;

namespace TestHelix
{
    public partial class MainWindow : Window
    {
        LinesVisual3D lignes = new LinesVisual3D();
        PointsVisual3D points = new PointsVisual3D();
        LinesVisual3D arretes = new LinesVisual3D();
        MeshVisual3D mv = new MeshVisual3D();

        Point3DCollection bufSkel = new Point3DCollection();
        Point3DCollection bufPoint = new Point3DCollection();
        Point3DCollection bufArrete = new Point3DCollection();


        private KinectSensor kinect = null;
        private Timer timer = new Timer();
        private Skeleton[] players = new Skeleton[2];

        public MainWindow()
        {
            InitializeComponent();

            ViewPort.Children.Add(lignes);
            ViewPort.Children.Add(points);
            ViewPort.Children.Add(arretes);
            ViewPort.Children.Add(mv);

            points.Color = Brushes.Orange.Color;
            points.Size = 3;
            arretes.Color = Brushes.Orange.Color;
            points.Size = 2;

            initKinect();
        }

        private void initKinect()
        {
            if (KinectSensor.KinectSensors.Count > 0)
                kinect = KinectSensor.KinectSensors[0];

            kinect.SkeletonStream.Enable();
            kinect.SkeletonFrameReady += KinectOnSkeletonFrameReady;

            kinect.Start();
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

                    //ViewPort.Camera = ViewPort.Camera;
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
                new Point3D(art2.Position.X, art2.Position.Y, art2.Position.Z), color.Color, 3);
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
            if (checkDraw.IsChecked.GetValueOrDefault())
            {
                timer.Tick += new EventHandler(drawPoints);
                timer.Interval = 41;
                timer.Start();
            }

            if (!checkDraw.IsChecked.GetValueOrDefault())
            {
                timer.Tick -= drawPoints;
            }

        }

        private void drawPoints(object sender, EventArgs e)
        {
            Skeleton skel = players[0];
                if (skel != null)
                {
                    Joint main = skel.Joints[JointType.HandRight];
                    Point3D newP = new Point3D(main.Position.X, main.Position.Y, main.Position.Z);
                    if( bufPoint.Count > 0)
                    {
                        bufArrete.Add(bufPoint.Last());
                        bufArrete.Add(newP);
                    }
                    bufPoint.Add(newP);
                    
                }
                arretes.Points = bufArrete;
                points.Points = bufPoint;
            
        }

        private void ClearSpace(object sender, RoutedEventArgs e)
        {
            bufPoint.Clear();
            bufArrete.Clear();
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
                    MeshBuilder mb = new MeshBuilder();
                    mb.AddBox(newP, 3, 3, 3);

                    mv.Mesh = new Mesh3D();

                    foreach (Point3D pd in mb.Positions)
                    {
                        
                        mv.Mesh.Vertices.Add(pd);
                        //Console.WriteLine(pd.ToString());
                    }
                

            }
        }

       
    }
}

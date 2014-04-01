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

        Skeleton squelette;

        Squelette2PerspectiveCameraConverter squelette2CameraConverter;
        Boolean focusSquelette;

        Point3DCollection bufSkel = new Point3DCollection();
        
        Point3DCollection bufPoint = new Point3DCollection();
        Point3DCollection bufArrete = new Point3DCollection();
        
        private KinectSensor kinect = null;
        private Timer timer = new Timer();
        private Skeleton[] players = new Skeleton[2];

        UserInfo[] userInfos = new UserInfo[InteractionFrame.UserInfoArrayLength];
        InteractionStream interStream;
        InteractionHandEventType last;

        Brush colorBack = Brushes.Gray;
        SolidColorBrush colorSkel = Brushes.AliceBlue;
        Color colorLines = Brushes.Orange.Color;
        Color colorPoints = Brushes.Orange.Color;

        public MainWindow()
        {
            InitializeComponent();

            ViewPort.Children.Add(lignes);
            ViewPort.Children.Add(mv);
            ViewPort.Background = colorBack;

            squelette2CameraConverter = new Squelette2PerspectiveCameraConverter();
            focusSquelette = false;


            timer.Interval = 41;

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

                    if (skeletonFrame.SkeletonArrayLength == 0)
                        return;

                    var trackedSquelettes = squelettes.Where(skel => skel.TrackingState == SkeletonTrackingState.Tracked).OrderBy<Skeleton, float>(skel => skel.Position.Z);

                    if (trackedSquelettes.Count() == 0)
                    {
                        return;
                    }

                    squelette = trackedSquelettes.First();

                    if (squelette != null)
                    {
                        players[i] = squelette;
                        drawBone(squelette, JointType.Head, JointType.ShoulderCenter, colorSkel);

                        drawBone(squelette, JointType.ShoulderCenter, JointType.ShoulderLeft, colorSkel);
                        drawBone(squelette, JointType.ShoulderCenter, JointType.ShoulderRight, colorSkel);

                        drawBone(squelette, JointType.ShoulderCenter, JointType.Spine, colorSkel);

                        drawBone(squelette, JointType.Spine, JointType.HipCenter, colorSkel);

                        drawBone(squelette, JointType.HipCenter, JointType.HipLeft, colorSkel);
                        drawBone(squelette, JointType.HipCenter, JointType.HipRight, colorSkel);

                        drawBone(squelette, JointType.ShoulderLeft, JointType.ElbowLeft, colorSkel);
                        drawBone(squelette, JointType.ElbowLeft, JointType.WristLeft, colorSkel);
                        drawBone(squelette, JointType.WristLeft, JointType.HandLeft, colorSkel);

                        drawBone(squelette, JointType.ShoulderRight, JointType.ElbowRight, colorSkel);
                        drawBone(squelette, JointType.ElbowRight, JointType.WristRight, colorSkel);
                        drawBone(squelette, JointType.WristRight, JointType.HandRight, colorSkel);
                        
                        // Left Leg
                        drawBone(squelette, JointType.HipLeft, JointType.KneeLeft, colorSkel);
                        drawBone(squelette, JointType.KneeLeft, JointType.AnkleLeft, colorSkel);
                        drawBone(squelette, JointType.AnkleLeft, JointType.FootLeft, colorSkel);

                        // Right Leg
                        drawBone(squelette, JointType.HipRight, JointType.KneeRight, colorSkel);
                        drawBone(squelette, JointType.KneeRight, JointType.AnkleRight, colorSkel);
                        drawBone(squelette, JointType.AnkleRight, JointType.FootRight, colorSkel);

                        if (focusSquelette)
                            ViewPort.Camera = (PerspectiveCamera)squelette2CameraConverter.Convert(players[0], null, null, null);

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
            if (checkDraw.IsChecked.GetValueOrDefault())
            {

                tmpArrete = new LinesVisual3D();
                tmpArrete.Color = colorLines;
                tmpArrete.Thickness = 0.01;

                tmpPoints = new PointsVisual3D();
                tmpPoints.Color = colorPoints;
                tmpPoints.Size = 0.05;

                bufArrete = new Point3DCollection();
                bufPoint = new Point3DCollection();

                listePoints.Add(tmpPoints);
                listeLignes.Add(tmpArrete);

                ViewPort.Children.Add(listeLignes.Last());
                ViewPort.Children.Add(listePoints.Last());
                nbTraits.Content = "Nombre de traits : " + listeLignes.Count;

                timer.Tick += new EventHandler(drawPoints);
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
                    
                    if( tmpPoints.Points.Count > 0)
                    {
                        bufArrete.Add(tmpPoints.Points.Last());
                        bufArrete.Add(newP);
                    }

                    bufPoint.Add(newP);
                    

                    tmpPoints.Points = bufPoint;
                    tmpArrete.Points = bufArrete;
                    
                }
            
        }

        private void ClearSpace(object sender, RoutedEventArgs e)
        {
            foreach (LinesVisual3D lv in listeLignes)
            {
                lv.Points.Clear();
            }

            foreach (PointsVisual3D lp in listePoints)
            {
                lp.Points.Clear();
            }
        }

        //changer la couleur des points et arretes
        private void ChangeBackColor()
        {
            ViewPort.Background = colorBack;
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
            Skeleton s = squelette;

                if (s != null)
                {
                    Joint main = s.Joints[JointType.HandRight];
                    Point3D point = new Point3D(main.Position.X, main.Position.Y, main.Position.Z);
                   
                    MeshBuilder mesh = new MeshBuilder(true, true);

                    mesh.AddBox(point, 1, 1, 1);
                    
                }
        }


        private void toggleFocusSquelette(object sender, RoutedEventArgs e)
        {
            focusSquelette = !focusSquelette;
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
            }
        }

        private void Constellation_Unchecked(object sender, RoutedEventArgs e)
        {
            colorBack = Brushes.Gray;
            colorLines = Brushes.Orange.Color;
            colorPoints = Brushes.Orange.Color;
            colorSkel = Brushes.AliceBlue;
            ChangeBackColor();
            timer.Stop();
            timer.Interval = 41;
            timer.Start();
        }

        private void Constellation_Checked(object sender, RoutedEventArgs e)
        {
            colorBack = Brushes.Black;
            colorLines = Brushes.AntiqueWhite.Color;
            colorPoints = Brushes.AntiqueWhite.Color;
            colorSkel = Brushes.Red;
            ChangeBackColor();
            timer.Stop();
            timer.Interval = 500;
            timer.Start();

        }
    }
}

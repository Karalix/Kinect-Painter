﻿using System;
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

namespace TestHelix
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        LinesVisual3D lignes = new LinesVisual3D();
        private KinectSensor kinect = null; 

        public MainWindow()
        {
            InitializeComponent();

            ViewPort.Children.Add(lignes);

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
                    Skeleton[] squelettes = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(squelettes);

                    lignes.Points.Clear();

                    foreach (
                        Skeleton squelette in
                            squelettes.Where(skel => skel.TrackingState == SkeletonTrackingState.Tracked))
                    {
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
                        /*
                        // Left Leg
                        drawBone(squelette, JointType.HipLeft, JointType.KneeLeft, Brushes.AliceBlue);
                        drawBone(squelette, JointType.KneeLeft, JointType.AnkleLeft, Brushes.AliceBlue);
                        drawBone(squelette, JointType.AnkleLeft, JointType.FootLeft, Brushes.AliceBlue);

                        // Right Leg
                        drawBone(squelette, JointType.HipRight, JointType.KneeRight, Brushes.AliceBlue);
                        drawBone(squelette, JointType.KneeRight, JointType.AnkleRight, Brushes.AliceBlue);
                        drawBone(squelette, JointType.AnkleRight, JointType.FootRight, Brushes.AliceBlue);*/


                        ViewPort.UpdateLayout();
                    }

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
            lignes.Points.Add(p1);
            lignes.Points.Add(p2);
            lignes.Color = color;
            lignes.Thickness = thickness;
        }
    }
}

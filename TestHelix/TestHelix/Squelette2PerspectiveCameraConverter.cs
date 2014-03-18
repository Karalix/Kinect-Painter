using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Media3D;

namespace TestHelix
{
    [System.Windows.Data.ValueConversion(typeof(Skeleton), typeof(PerspectiveCamera))]
    class Squelette2PerspectiveCameraConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Skeleton squelette = (value as Skeleton);

            Point3D shoulderCenter = join2Point3D(squelette, JointType.ShoulderCenter);
            Point3D shoulderLeft = join2Point3D(squelette, JointType.ShoulderLeft);
            Point3D shoulderRight = join2Point3D(squelette, JointType.ShoulderRight);
            Point3D spine = join2Point3D(squelette, JointType.Spine);

            Vector3D spineToShoulderLeft = shoulderLeft - spine;
            Vector3D spineToShoulderRight = shoulderRight - spine;

            Vector3D normaleSquelette = Vector3D.CrossProduct(spineToShoulderLeft, spineToShoulderRight);
            normaleSquelette.Normalize();

            Vector3D cameraUp = shoulderCenter - spine;

            Point3D cameraPosition = spine + (normaleSquelette * (-3));

            return new PerspectiveCamera(cameraPosition, normaleSquelette, cameraUp, 50.0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public static Point3D join2Point3D(Skeleton skel, JointType join)
        {
            Joint j = skel.Joints[join];
            return new Point3D(j.Position.X, j.Position.Y, j.Position.Z);
        }
    }
}

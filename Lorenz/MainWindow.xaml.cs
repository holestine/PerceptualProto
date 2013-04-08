using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Lorenz
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Constants
        private const double DEFAULT_BRUSH_OPACITY = 0.9;
        private const double SQRT3 = 1.73205080757f;
        #endregion Constants

        #region Private Data
        private GeometryModel3D m_GeometryModel;
        private Model3DGroup m_Model3DGroup;
        private ModelVisual3D m_ModelVisual3D;
        private Viewport3D m_Viewport3D;
        private Model3DGroup m_pyramid;
        private int m_Angle;
        #endregion Private Data

        #region Initializaion
        public MainWindow()
        {
            InitializeComponent();

            m_Angle = 0;

            // Declare scene objects.
            m_Viewport3D = new Viewport3D();
            m_Model3DGroup = new Model3DGroup();
            m_GeometryModel = new GeometryModel3D();
            m_ModelVisual3D = new ModelVisual3D();

            // Set up camera
            var camera = new PerspectiveCamera
            {
                Position = new Point3D(0, 10, -10),
                LookDirection = new Vector3D(0, -1, 1),
                FieldOfView = 90
            };
            m_Viewport3D.Camera = camera;

            // Set up lights
            var light = new DirectionalLight
            {
                Color = Colors.White,
                Direction = new Vector3D(-0.6, -0.5, -0.6)
            };
            m_Model3DGroup.Children.Add(light);

            light = new DirectionalLight
            {
                Color = Colors.White,
                Direction = new Vector3D(0, 0, 1)
            };
            m_Model3DGroup.Children.Add(light);

           var ambientLight = new AmbientLight()
              {
                 Color = Colors.CornflowerBlue
              };
           m_Model3DGroup.Children.Add(ambientLight);

           //WireGestures();           
        }

       private void WireGestures()
       {
          // Wire gesture handler
          PXCMSession session;
          pxcmStatus sts = PXCMSession.CreateInstance(out session);
          if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR)
          {
             Console.WriteLine("Failed to create the SDK session");
             return;
          }

          // Gesture Module
          PXCMBase gesture_t;
          sts = session.CreateImpl(PXCMGesture.CUID, out gesture_t);
          if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR)
          {
             Console.WriteLine("Failed to load any gesture recognition module");
             session.Dispose();
             return;
          }
          PXCMGesture gesture = (PXCMGesture)gesture_t;

          PXCMGesture.ProfileInfo pinfo;
          sts = gesture.QueryProfile(0, out pinfo);

          UtilMCapture capture = new UtilMCapture(session);
          sts = capture.LocateStreams(ref pinfo.inputs);
          if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR)
          {
             Console.WriteLine("Failed to locate a capture module");
             gesture.Dispose();
             capture.Dispose();
             session.Dispose();
             return;
          }
          sts = gesture.SetProfile(ref pinfo);
          sts = gesture.SubscribeGesture(100, OnGesure);

          bool device_lost = false;
          PXCMImage[] images = new PXCMImage[PXCMCapture.VideoStream.STREAM_LIMIT];
          PXCMScheduler.SyncPoint[] sps = new PXCMScheduler.SyncPoint[2];
          for (int nframes = 0; nframes < 50000; nframes++)
          {
             sts = capture.ReadStreamAsync(images, out sps[0]);
             if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR)
             {
                if (sts == pxcmStatus.PXCM_STATUS_DEVICE_LOST)
                {
                   if (!device_lost) Console.WriteLine("Device disconnected");
                   device_lost = true; nframes--;
                   continue;
                }
                Console.WriteLine("Device failed\n");
                break;
             }
             if (device_lost)
             {
                Console.WriteLine("Device reconnected");
                device_lost = false;
             }

             sts = gesture.ProcessImageAsync(images, out sps[1]);
             if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR) break;

             PXCMScheduler.SyncPoint.SynchronizeEx(sps);
             if (sps[0].Synchronize(0) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
             {
                PXCMGesture.GeoNode data;
                sts = gesture.QueryNodeData(0, PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_PRIMARY | PXCMGesture.GeoNode.Label.LABEL_HAND_MIDDLE, out data);
                if (sts >= pxcmStatus.PXCM_STATUS_NO_ERROR)
                   Console.WriteLine("[node] {0}, {1}, {2}", data.positionImage.x, data.positionImage.y, data.timeStamp);
             }

             foreach (PXCMScheduler.SyncPoint s in sps) if (s != null) s.Dispose();
             foreach (PXCMImage i in images) if (i != null) i.Dispose();
          }

          gesture.Dispose();
          capture.Dispose();
          session.Dispose();
       }

       #endregion Initializaion

        #region Mouse Events
        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            m_Model3DGroup.Children.Remove(m_pyramid);
            m_Viewport3D.Children.Remove(m_ModelVisual3D);

            var pyramidColor = Color.FromRgb(0x0F, 0xF0, 0x00);

            var position = new Point3D(1, 0, 0);
            m_pyramid = GetNewPyramindModel(ref position, ref pyramidColor, DEFAULT_BRUSH_OPACITY);

            var myRotateTransform3D = new RotateTransform3D();
            var myAxisAngleRotation3D = new AxisAngleRotation3D { Axis = new Vector3D(0, 1, 0), Angle = m_Angle+=10 };
            myRotateTransform3D.Rotation = myAxisAngleRotation3D;
            m_pyramid.Transform = myRotateTransform3D;

            // Add the geometry model to the viewport
            m_Model3DGroup.Children.Add(m_pyramid);
            m_ModelVisual3D.Content = m_Model3DGroup;
            m_Viewport3D.Children.Add(m_ModelVisual3D);
        }
        
        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var p0 = new Point3D(0, 0, 1);
            var p1 = new Point3D(5, 0, 1);
            var p2 = new Point3D(0, 5, 1);
            var color = Color.FromRgb(0x0F, 0xF0, 0xF0);
            var triangle = CreateTriangleModel(ref p0, ref p1, ref p2, ref color,
            DEFAULT_BRUSH_OPACITY);

            m_Angle += 10;
            var myRotateTransform3D = new RotateTransform3D();
            var myAxisAngleRotation3D = new AxisAngleRotation3D { Axis = new Vector3D(0, 1, 0), Angle = m_Angle };
            myRotateTransform3D.Rotation = myAxisAngleRotation3D;
            triangle.Transform = myRotateTransform3D;

            m_Model3DGroup.Children.Add(triangle);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var pyramidColor = Color.FromRgb(0xFF, 0xF0, 0x00);
            var position = new Point3D(0, 0, 0);

            m_pyramid = GetNewPyramindModel(ref position, ref pyramidColor, DEFAULT_BRUSH_OPACITY);

            // Add the geometry model to the viewport
            m_Model3DGroup.Children.Add(m_pyramid);
            m_ModelVisual3D.Content = m_Model3DGroup;
            m_Viewport3D.Children.Add(m_ModelVisual3D);

            m_Viewport3D.MouseDown += OnMouseDown;
            m_Viewport3D.MouseEnter += OnMouseEnter;

            x_page.Content = m_Viewport3D;
        }
        #endregion MouseEvents

        #region Gesture Events
        static void OnGesure(ref PXCMGesture.Gesture data)
        {
           Console.WriteLine("[gesture] label={0}", data.label);
        }
        #endregion Gesture Events

        #region Workers
        private Model3DGroup GetNewPyramindModel(ref Point3D center, ref Color color, double opacity)
        {
            var p0 = new Point3D(center.X, center.Y, center.Z - 0.5);
            var p1 = new Point3D(center.X, center.Y + 1.0, center.Z);
            var p2 = new Point3D(center.X - 0.5, center.Y, center.Z - 0.5);
            var p3 = new Point3D(center.X + 0.5, center.Y, center.Z - 0.5);

            Model3DGroup pyramid = new Model3DGroup();

            var red = Color.FromRgb(0xFF, 0x00, 0x00);
            var green = Color.FromRgb(0x00, 0xFF, 0x00);
            var blue = Color.FromRgb(0x00, 0x00, 0xFF);
            var yellow = Color.FromRgb(0xFF, 0xF0, 0x00);

            pyramid.Children.Add(CreateTriangleModel(ref p2, ref p1, ref p3, ref red, opacity));
            pyramid.Children.Add(CreateTriangleModel(ref p0, ref p1, ref p2, ref green, opacity));
            pyramid.Children.Add(CreateTriangleModel(ref p3, ref p1, ref p0, ref blue, opacity));
            pyramid.Children.Add(CreateTriangleModel(ref p0, ref p2, ref p3, ref yellow, opacity));
           
           /*
            pyramid.Children.Add(CreateTriangleModel(ref p2, ref p1, ref p3, ref color, opacity));
            pyramid.Children.Add(CreateTriangleModel(ref p0, ref p1, ref p2, ref color, opacity));
            pyramid.Children.Add(CreateTriangleModel(ref p3, ref p1, ref p0, ref color, opacity));
            pyramid.Children.Add(CreateTriangleModel(ref p0, ref p2, ref p3, ref color, opacity));
           */
            ModelVisual3D model = new ModelVisual3D();
            return pyramid;
        }

        private Model3DGroup CreateTriangleModel(ref Point3D p0, ref Point3D p1, ref Point3D p2, ref Color color, double opacity)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.Positions.Add(p0);
            mesh.Positions.Add(p1);
            mesh.Positions.Add(p2);
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);
            
            Vector3D normal = CalculateNormal(ref p0, ref p1, ref p2);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);

            Material material = new DiffuseMaterial(new SolidColorBrush(color) { Opacity = opacity });
            GeometryModel3D model = new GeometryModel3D(mesh, material);
            Model3DGroup group = new Model3DGroup();
            group.Children.Add(model);
            return group;
        }

        private Vector3D CalculateNormal(ref Point3D p0, ref Point3D p1, ref Point3D p2)
        {
            Vector3D v0 = new Vector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
            Vector3D v1 = new Vector3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
            return Vector3D.CrossProduct(v0, v1);
        }
        #endregion Workers
    }
}

using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

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
      //private GeometryModel3D m_GeometryModel;
      private Model3DGroup m_Model3DGroup;
      private readonly ModelVisual3D m_ModelVisual3D;
      private Model3DGroup m_Pyramid;
      private int m_Angle;
      private Thread m_PipelineThread;
      private Pipeline m_pipeline;
      #endregion Private Data

      
      public delegate void PopulateTextBoxDelegate(string text);

      #region Initializaion
      public MainWindow()
      {
         InitializeComponent();
         m_Angle = 0;

         // Declare scene objects.
         m_Model3DGroup = new Model3DGroup();
         m_ModelVisual3D = new ModelVisual3D();

         // Set up camera
         var camera = new PerspectiveCamera
         {
            Position = new Point3D(0, 10, -10),
            LookDirection = new Vector3D(0, -1, 1),
            FieldOfView = 90
         };
         XViewport.Camera = camera;

         // Set up lights
         var light = new DirectionalLight
         {
            Color = Colors.White,
            Direction = new Vector3D(-0.6, -0.5, -0.6)
         };
         m_Model3DGroup.Children.Add(light);
         /*
         light = new DirectionalLight
         {
            Color = Colors.White,
            Direction = new Vector3D(0, 0, 1)
         };
         m_Model3DGroup.Children.Add(light);
         */
         var ambientLight = new AmbientLight
         {
            Color = Colors.CornflowerBlue
         };
         m_Model3DGroup.Children.Add(ambientLight);

         m_pipeline = new Pipeline();
         m_pipeline.SetInitialPos(MouseUtilities.GetPosition(this));
         m_PipelineThread = new Thread(new ThreadStart(m_pipeline.Start));
         m_PipelineThread.Start();
      }
      
      #endregion Initializaion

      #region Mouse Events
      private void OnMouseEnter(object sender, MouseEventArgs e)
      {
         var workerThread = new BackgroundWorker();
         workerThread.DoWork += new DoWorkEventHandler(SpinModel);
         workerThread.RunWorkerAsync();
      }

      private void SpinModel(object sender, DoWorkEventArgs e)
      {
         for (int i = 0; i < 36; i++)
         {
            //Rotate model
            var myAxisAngleRotation3D = new AxisAngleRotation3D { Axis = new Vector3D(0, 1, 1), Angle = m_Angle += 10 };
            var myRotateTransform3D = new RotateTransform3D { Rotation = myAxisAngleRotation3D };
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {
               m_Model3DGroup.Transform = myRotateTransform3D;
            }));
            Thread.Sleep(100);
         }
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

      #endregion Mouse Events

      #region Window Events
      private void OnLoaded(object sender, RoutedEventArgs e)
      {
         var pyramidColor = Color.FromRgb(0xFF, 0xF0, 0x00);
         var position = new Point3D(0, 0, 0);

         m_Pyramid = GetNewPyramindModel(ref position, ref pyramidColor, DEFAULT_BRUSH_OPACITY);

         // Add the geometry model to the viewport
         m_Model3DGroup.Children.Add(m_Pyramid);
         m_ModelVisual3D.Content = m_Model3DGroup;
         XViewport.Children.Add(m_ModelVisual3D);

         XViewport.MouseDown += OnMouseDown;
         XViewport.MouseEnter += OnMouseEnter;
      }

      private void OnClosed(object sender, System.EventArgs e)
      {
         m_PipelineThread.Abort();
      }
      #endregion Window Events

      #region Workers
      private Model3DGroup GetNewPyramindModel(ref Point3D center, ref Color color, double opacity)
      {
         var p0 = new Point3D(center.X, center.Y, center.Z);
         var p1 = new Point3D(center.X, center.Y + 1.0, center.Z + 0.5);
         var p2 = new Point3D(center.X + 1.0, center.Y, center.Z + 1.0);
         var p3 = new Point3D(center.X - 1.0, center.Y, center.Z + 1.0);

         var pyramid = new Model3DGroup();

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

         return pyramid;
      }

      private Model3DGroup CreateTriangleModel(ref Point3D p0, ref Point3D p1, ref Point3D p2, ref Color color, double opacity)
      {
         var mesh = new MeshGeometry3D();
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

         var material = new DiffuseMaterial(new SolidColorBrush(color) { Opacity = opacity });
         var model = new GeometryModel3D(mesh, material);
         var group = new Model3DGroup();
         group.Children.Add(model);

         return group;
      }

      private Vector3D CalculateNormal(ref Point3D p0, ref Point3D p1, ref Point3D p2)
      {
         var v0 = new Vector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
         var v1 = new Vector3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
         return Vector3D.CrossProduct(v0, v1);
      }
      #endregion Workers

   }
}

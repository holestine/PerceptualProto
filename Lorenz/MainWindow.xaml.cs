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
      //private const double SQRT3 = 1.73205080757f;
      #endregion Constants

      #region Private Data
      private Model3DGroup m_Model3DGroup;
      private ModelVisual3D m_ModelVisual3D;
      private Model3DGroup m_Lorenz;
      private int m_Angle;
      private Thread m_PipelineThread;
      private GestureEngine m_GestureEngine;
      #endregion Private Data

      #region Accessors

      public string Messages
      {
         get
         {
            return XTextbox.Text;
         }
         set
         {
            Dispatcher.BeginInvoke(
            (Action)delegate
            {
               XTextbox.Text += value + "\n";
               XTextbox.ScrollToEnd();
            });
         } 
      }

      #endregion Accessors

      #region Initializaion
      public MainWindow()
      {
         InitializeComponent();
         InitializeGraphics();
         InitializeGestureEngine();
      }

      private void InitializeGraphics()
      {
         m_Angle = 0;

         // Declare scene objects.
         m_Model3DGroup = new Model3DGroup();
         m_ModelVisual3D = new ModelVisual3D();

         // Set up camera
         var camera = new PerspectiveCamera
         {
            Position = new Point3D(1, 5, -20),
            LookDirection = new Vector3D(-1, -5, 20),
            FieldOfView = 90
         };
         XViewport.Camera = camera;

         // Set up lights
         var light = new DirectionalLight
         {
            Color = Color.FromRgb(0xA0, 0xA0, 0xA0),
            Direction = new Vector3D(0, 0, 1)
         };
         m_Model3DGroup.Children.Add(light);
         
         var ambientLight = new AmbientLight
         {
             Color = Color.FromRgb(0x40, 0x40, 0x40)
         };
         m_Model3DGroup.Children.Add(ambientLight);
      }

      private void InitializeGestureEngine()
      {
         m_GestureEngine = new GestureEngine(this);
         m_PipelineThread = new Thread(m_GestureEngine.Start);
         m_PipelineThread.Start();
      }

      #endregion Initializaion

      #region Mouse Events

      private void OnMouseEnter(object sender, MouseEventArgs e)
      {
         var workerThread = new BackgroundWorker();
         workerThread.DoWork += DoAsyncWork;
         workerThread.RunWorkerCompleted += UpdateUI;
         workerThread.RunWorkerAsync();
      }

      private void DoAsyncWork(object sender, DoWorkEventArgs e)
      {

      }

      private void UpdateUI(object sender, RunWorkerCompletedEventArgs e)
      {
         //Rotate model
         var myAxisAngleRotation3D = new AxisAngleRotation3D { Axis = new Vector3D(0, 1, 1), Angle = m_Angle += 10 };
         var myRotateTransform3D = new RotateTransform3D { Rotation = myAxisAngleRotation3D };
         Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
         {
            m_Model3DGroup.Transform = myRotateTransform3D;
         }));
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
         m_GestureEngine.SetOrigin(GetCanvasCenter());

         var color = Color.FromRgb(0xFF, 0xF0, 0x00);

         var position = new Point3D(-100, -100, -500);

         // Add the geometry model to the viewport
         m_Model3DGroup.Children.Add(GetNewLorenzModel(ref position, ref color, DEFAULT_BRUSH_OPACITY));
         m_ModelVisual3D.Content = m_Model3DGroup;
         XViewport.Children.Add(m_ModelVisual3D);

         // Wire up mouse events
         XViewport.MouseDown += OnMouseDown;
         XViewport.MouseEnter += OnMouseEnter;
      }

      /// <summary>
      /// Shut everything down
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnClosed(object sender, EventArgs e)
      {
         m_PipelineThread.Abort();
      }

      private void OnLocationChanged(object sender, EventArgs e)
      {
         m_GestureEngine.SetOrigin(GetCanvasCenter());// (MouseUtilities.GetPosition(this));
      }

      #endregion Window Events

      #region Graphics

      private Model3DGroup GetNewLorenzModel(ref Point3D pos, ref Color color, double opacity)
      {
         var lorenz = new Model3DGroup();
         const int NUMPOINTS = 1000;
         double mStepSize = .005;

    	
    	   for (int i = 0; i < NUMPOINTS; i++)
    	   {
            lorenz.Children.Add(GetNewPyramindModel(ref pos, ref color, opacity));
        	   pos = RK4Lorenz(pos, mStepSize); 
    	   }

         m_Lorenz = lorenz;
         return lorenz;
      }

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

         pyramid.Children.Add(CreateTriangleModel(ref p2, ref p1, ref p3, ref red, opacity));
         pyramid.Children.Add(CreateTriangleModel(ref p0, ref p1, ref p2, ref green, opacity));
         pyramid.Children.Add(CreateTriangleModel(ref p3, ref p1, ref p0, ref blue, opacity));
         pyramid.Children.Add(CreateTriangleModel(ref p0, ref p2, ref p3, ref color, opacity));

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
         normal = CalculateNormal(ref p1, ref p0, ref p2);
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
      #endregion Graphics

      #region Math
  
      private Point3D RK4Lorenz(Point3D pos, double dt)
    {
	   // Obtain and store first set of slopes
	   Point3D f1 = Lorenz(pos);
	
	   // Compute next Euler position with first set of slopes and half time step
	   Point3D xyz = Euler(pos, f1, dt/2);
	
	   // Obtain and store second set of slopes
	   Point3D f2 = Lorenz(xyz);
	
	   // Compute next Euler position with second set of slopes and half time step
	   xyz = Euler(pos, f2, dt/2);
	
	   // Obtain and store third set of slopes
	   Point3D f3 = Lorenz(xyz);
	
	   // Compute next Euler position with third set of slopes and full time step
	   xyz = Euler(pos, f3, dt);
	
	   // Obtain and store fourth set of slopes
	   Point3D f4 = Lorenz(xyz);
	
	   // Compute weighted average of slopes according to Runge-Kutta fourth order algorithm
	   Point3D rkSlope = new Point3D(f1.X/6 + f2.X/3 + f3.X/3 + f4.X/6,
                                    f1.Y/6 + f2.Y/3 + f3.Y/3 + f4.Y/6,
                                    f1.Z/6 + f2.Z/3 + f3.Z/3 + f4.Z/6);
	
	   // Return next position using Euler with Runge-Kutta slope
	   return Euler(pos, rkSlope, dt);
    }
    

	private Point3D Lorenz(Point3D pos)
	{
      double sigma = 10;
		double rho   = 28;
		double beta  = 8/3;
		
		return new Point3D(sigma * (pos.Y - pos.X),
				pos.X * (rho - pos.Z) - pos.Y, 
				pos.X * pos.Y - beta * pos.Z);
	}
	
	   private Point3D Euler(Point3D pos, Point3D slope, double dt)
	   {
		   return new Point3D(pos.X + slope.X * dt, pos.Y + slope.Y * dt, pos.Z + slope.Z * dt);
	   }

      #endregion Math

      #region Private Methods
      private Point GetCanvasCenter()
      {
         return new Point(Left + XCanvas.ActualWidth/2 + XTextbox.Width, Top + XCanvas.ActualHeight/2);
      }
      #endregion Private Methods
   }
}




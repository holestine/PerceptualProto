using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
      //private const double SQRT3 = 1.73205080757f;
      private Color RED = Color.FromRgb(0xFF, 0x00, 0x00);
      private Color GREEN = Color.FromRgb(0x00, 0xFF, 0x00);
      private Color BLUE = Color.FromRgb(0x00, 0x00, 0xFF);
      private Color WHITE = Color.FromRgb(0xFF, 0xFF, 0xFF);
      private Color AMBIENT_LIGHT = Color.FromRgb(0x30, 0x30, 0x30);
      private Color POINT_LIGHT = Color.FromRgb(0xDD, 0xDD, 0xDD);
      #endregion Constants

      #region Enumerations
      private enum State
      {
         Rotating,
         Idle,
         Animating
      };
      #endregion Enumerations

      #region Private Data
      private Model3DGroup m_Model3DGroup;
      private ModelVisual3D m_ModelVisual3D;
      private LorenzVisual m_Lorenz;
      private Thread m_PipelineThread;
      private GestureEngine m_GestureEngine;

      private State m_State;

      #endregion Private Data

      #region Public Methods

      public void Write(string message)
      {
         Dispatcher.BeginInvoke(
            (Action)delegate
            {
               XTextbox.Text += message + "\n";
               XTextbox.ScrollToEnd();
            });
      }

      public void Rotate(Vector3D axis, double angle)
      {
         if (m_State != State.Idle) return;

         m_State = State.Rotating;

         Dispatcher.BeginInvoke(
            (Action) delegate
               {
                  var rotation = new AxisAngleRotation3D {Axis = axis, Angle = angle};
                  var transform = new RotateTransform3D {Rotation = rotation};
                  var t = new Transform3DGroup();
                  t.Children.Add(m_Lorenz.Transform);
                  t.Children.Add(transform);
                  m_Lorenz.Transform = t;
                  m_State = State.Idle;
               });
      }

      public void Animate(Point3D newStart)
      {
         if (m_State != State.Idle) return;

         m_State = State.Animating;

         Dispatcher.BeginInvoke(
            (Action)delegate
               {
                  //Write(String.Format("New Start Point ({0}, {1}, {2})", newStart.X, newStart.Y, newStart.Z));
                  m_Lorenz.Recalculate(newStart);
                  m_State = State.Idle;
            });
      }

      #endregion Public Methods

      #region Initializaion
      public MainWindow()
      {
         InitializeComponent();
         InitializeGraphics();
         InitializeGestureEngine();
      }

      private void InitializeGraphics()
      {
         // Declare scene objects.
         m_Model3DGroup = new Model3DGroup();
         m_ModelVisual3D = new ModelVisual3D();

         // Set up camera
         var camera = new PerspectiveCamera
         {
            Position = new Point3D(1, 5, -40),
            LookDirection = new Vector3D(-1, -5, 20),
            FieldOfView = 120
         };
         XViewport.Camera = camera;

         // Set up lights
         var ambientLight = new AmbientLight
         {
            Color = AMBIENT_LIGHT
         };
         m_Model3DGroup.Children.Add(ambientLight);

         var pointLight = new PointLight
            {
               Color = POINT_LIGHT,
               Position = new Point3D(10, 20, -10)
            };
         m_Model3DGroup.Children.Add(pointLight);
      }

      private void InitializeGestureEngine()
      {
         m_State = State.Idle;
         m_GestureEngine = new GestureEngine(this);
         m_PipelineThread = new Thread(m_GestureEngine.Start);
         m_PipelineThread.Start();
      }

      #endregion Initializaion

      #region Mouse Events

      private void OnMouseEnter(object sender, MouseEventArgs e)
      {
         // Set selected item
         var myAxisAngleRotation3D = new AxisAngleRotation3D { Axis = new Vector3D(0, 0, 1), Angle = 10 };
         var myRotateTransform3D = new RotateTransform3D { Rotation = myAxisAngleRotation3D };
         m_Model3DGroup.Transform = myRotateTransform3D;

          
         int i = 0;
         foreach (var item in XViewport.Children)
         {
            if (i%2 == 0)
            {
               item.Transform = myRotateTransform3D;   
            }
            i++;
         }
      }

      private void OnMouseDown(object sender, MouseButtonEventArgs e)
      {
         var myRotateTransform3D = new RotateTransform3D();
         var myAxisAngleRotation3D = new AxisAngleRotation3D { Axis = new Vector3D(0, 1, 0), Angle = 20 };
         myRotateTransform3D.Rotation = myAxisAngleRotation3D;

         /*
         var doubleAnimation = new DoubleAnimation()
            { 
               From = 500,
               To = 600,
               Duration = TimeSpan.FromSeconds(2),
               AutoReverse = true,
               RepeatBehavior = RepeatBehavior.Forever
            };
         Storyboard.SetTargetName(doubleAnimation, "XCanvas");
         //Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(XCanvas.Width));
         */
         var glyph = new Glyph
                {
                    Transform = new TranslateTransform3D(new Vector3D(5*Math.Sin(0), 10*Math.Cos(0), Math.Cos(0)))
                };
          XViewport.Children.Add(glyph);
      }

      #endregion Mouse Events

      #region Window Events

      private void OnLoaded(object sender, RoutedEventArgs e)
      {
         m_GestureEngine.SetOrigin(GetCanvasCenter());

         // Add the geometry model to the viewport
         m_ModelVisual3D.Content = m_Model3DGroup;
         XViewport.Children.Add(m_ModelVisual3D);

         m_Lorenz = new LorenzVisual(new Point3D(-75, 75, 100));
         XViewport.Children.Add(m_Lorenz);
         //XViewport.Children.Add(new LorenzVisual(new Point3D(50, -30, 20), Color.FromRgb(0x00, 0xFF, 0x00)));
         //XViewport.Children.Add(new LorenzVisual(new Point3D(-50, 50, -10), Color.FromRgb(0x00, 0x00, 0xFF)));
         
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
         m_GestureEngine.SetOrigin(GetCanvasCenter());
      }

      private Point GetCanvasCenter()
      {
          return new Point(Left + XCanvas.ActualWidth / 2 + XTextbox.Width, Top + XCanvas.ActualHeight / 2);
      }

      #endregion Window Events
   }
}

#region Notes
/*
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
 */
#endregion Notes
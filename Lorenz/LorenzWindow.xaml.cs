using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Lorenz
{
   /// <summary>
   /// Interaction logic for LorenzWindow.xaml
   /// </summary>
   public partial class LorenzWindow : Window
   {
      #region Constants
      private Color AMBIENT_LIGHT = Color.FromRgb(0x22, 0x22, 0x22);
      private Color POINT_LIGHT = Color.FromRgb(0xDD, 0xDD, 0xDD);
      private const int NUM_ANIMATION_STEPS = 250;
      #endregion Constants

      #region Enumerations
      private enum State
      {
         Idle,
         Animating
      };
      #endregion Enumerations

      #region Private Data
      private Model3DGroup m_Model3DGroup;
      private ModelVisual3D m_ModelVisual3D;
      private LorenzVisual m_Lorenz;
      private Thread m_GestureEngineThread;
      private GestureEngine m_GestureEngine;
      private State m_State;
      #endregion Private Data

      #region Initializaion
      public LorenzWindow()
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
         m_GestureEngineThread = new Thread(m_GestureEngine.Start);
         m_GestureEngineThread.Start();
      }

      #endregion Initializaion

      #region Public Methods
      public void Notify(string message)
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
         Dispatcher.BeginInvoke(
            (Action)delegate
            {
               var transformGroup = new Transform3DGroup();
               // Get current transform
               transformGroup.Children.Add(m_Lorenz.Transform);
               // Add new transform
               transformGroup.Children.Add(new RotateTransform3D { Rotation = new AxisAngleRotation3D { Axis = axis, Angle = angle } });
               // Apply changes
               m_Lorenz.Transform = transformGroup;
            });
      }

      public void Move(Point3D delta)
      {
         Dispatcher.BeginInvoke((Action)(() => m_Lorenz.Move(delta)));
      }

      public void Animate()
      {
         if (m_State == State.Animating) return;

         m_State = State.Animating;

         Point3D pos = m_Lorenz.StartingPoint;
         Point3D pos2 = m_Lorenz.StartingPoint;

         double phi = Math.Atan(pos.Y / pos.X);

         for (double i = 0; i < NUM_ANIMATION_STEPS; i++)
         {
            pos.X = pos2.X * Math.Cos(phi + i / 50);
            pos.Y = pos2.Y * Math.Sin(phi + i / 50);
            pos.Z = pos2.Z * Math.Cos(i / 50) + i;

            Dispatcher.BeginInvoke((Action)(() => m_Lorenz.Recalculate(pos)));
            Thread.Sleep(100);
         }
         m_State = State.Idle;
      }

      #endregion Public Methods

      #region Mouse Events

      private void OnMouseEnter(object sender, MouseEventArgs e)
      {
         var rotateTransform3D = new RotateTransform3D { Rotation = new AxisAngleRotation3D { Axis = new Vector3D(0, 0, 1), Angle = 90 }};
         m_Model3DGroup.Transform = rotateTransform3D;

         foreach (var item in XViewport.Children)
         {
            item.Transform = rotateTransform3D;
         }
      }

      private void OnMouseDown(object sender, MouseButtonEventArgs e)
      {
         var myRotateTransform3D = new RotateTransform3D();
         var myAxisAngleRotation3D = new AxisAngleRotation3D { Axis = new Vector3D(0, 1, 0), Angle = 20 };
         myRotateTransform3D.Rotation = myAxisAngleRotation3D;

         var glyph = new Glyph
         {
            Transform = new TranslateTransform3D(new Vector3D(5 * Math.Sin(0), 10 * Math.Cos(0), Math.Cos(0)))
         };
         XViewport.Children.Add(glyph);
      }

      #endregion Mouse Events

      #region Window Events

      private void OnLoaded(object sender, RoutedEventArgs e)
      {
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
         m_GestureEngineThread.Abort();
      }

      #endregion Window Events

      private void OnZoomIn(object sender, RoutedEventArgs e)
      {
         //var rotation = new AxisAngleRotation3D { Axis = axis, Angle = angle };
         var transform = new ScaleTransform3D(1.25, 1.25, 1.25); //RotateTransform3D { Rotation = rotation };
         var t = new Transform3DGroup();
         t.Children.Add(m_Lorenz.Transform);
         t.Children.Add(transform);
         m_Lorenz.Transform = t;
      }

      private void OnZoomOut(object sender, RoutedEventArgs e)
      {
         var transform = new ScaleTransform3D(.75, .75, .75); //RotateTransform3D { Rotation = rotation };
         var t = new Transform3DGroup();
         t.Children.Add(m_Lorenz.Transform);
         t.Children.Add(transform);
         m_Lorenz.Transform = t;
      }

      private void OnRotate(object sender, RoutedEventArgs e)
      {
         var rotation = new AxisAngleRotation3D { Axis = new Vector3D(0, 1, 0), Angle = 5 };
         var transform = new RotateTransform3D { Rotation = rotation };
         var t = new Transform3DGroup();
         t.Children.Add(m_Lorenz.Transform);
         t.Children.Add(transform);
         m_Lorenz.Transform = t;
      }
   }
}

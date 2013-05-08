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

         // Add the geometry model to the viewport
         m_ModelVisual3D = new ModelVisual3D { Content = m_Model3DGroup };
         XViewport.Children.Add(m_ModelVisual3D);
         m_Lorenz = new LorenzVisual(new Point3D(75, 75, 100));
         XViewport.Children.Add(m_Lorenz);
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

         var start = m_Lorenz.StartingPoint;
         Point3D pos = m_Lorenz.StartingPoint;

         double phi = Math.Atan(pos.Y / pos.X);
         var radius = Math.Sqrt(Math.Pow(m_Lorenz.StartingPoint.X, 2) + Math.Pow(m_Lorenz.StartingPoint.Y, 2) + Math.Pow(m_Lorenz.StartingPoint.Z, 2));
            
         for (double i = 0; i < NUM_ANIMATION_STEPS; i++)
         {
            pos.X = radius * Math.Cos(phi + i / 50);
            pos.Y = radius * Math.Sin(phi + i / 50);
            pos.Z = start.Z + 50 * Math.Sin(i / 50);
            Dispatcher.BeginInvoke((Action)(() => m_Lorenz.Recalculate(pos)));
            Thread.Sleep(100);
         }
         m_State = State.Idle;
      }
      #endregion Public Methods

      #region Event Handlers

      private void OnMouseEnter(object sender, MouseEventArgs e)
      {
         var transformGroup = new Transform3DGroup();
         transformGroup.Children.Add(m_Lorenz.Transform);
         transformGroup.Children.Add(new RotateTransform3D { Rotation = new AxisAngleRotation3D { Axis = new Vector3D(0, 1, 0), Angle = 30 } });
         transformGroup.Children.Add(new TranslateTransform3D(new Vector3D(0, 0, 0)));

         m_Model3DGroup.Transform = transformGroup;

         foreach (var item in XViewport.Children)
         {
            item.Transform = m_Model3DGroup.Transform;
         }
      }

      private void OnMouseDown(object sender, MouseButtonEventArgs e)
      {
         var transformGroup = new Transform3DGroup();
         transformGroup.Children.Add(m_Lorenz.Transform);
         transformGroup.Children.Add(new RotateTransform3D { Rotation = new AxisAngleRotation3D { Axis = new Vector3D(0, 1, 0), Angle = 180 } });
         transformGroup.Children.Add(new TranslateTransform3D(new Vector3D(0, 0, 0)));
         
         var glyph = new Glyph
         {
            Transform = transformGroup
         };
         XViewport.Children.Add(glyph);
      }

      private void OnClosed(object sender, EventArgs e)
      {
         m_GestureEngineThread.Abort();
      }

      private void OnZoomIn(object sender, RoutedEventArgs e)
      {
         var transformGroup = new Transform3DGroup();
         transformGroup.Children.Add(m_Lorenz.Transform);
         transformGroup.Children.Add(new ScaleTransform3D(1.25, 1.25, 1.25));
         m_Lorenz.Transform = transformGroup;
      }

      private void OnZoomOut(object sender, RoutedEventArgs e)
      {
         var transformGroup = new Transform3DGroup();
         transformGroup.Children.Add(m_Lorenz.Transform);
         transformGroup.Children.Add(new ScaleTransform3D(.75, .75, .75));
         m_Lorenz.Transform = transformGroup;
      }

      private void OnRotate(object sender, RoutedEventArgs e)
      {
         var transformGroup = new Transform3DGroup();
         transformGroup.Children.Add(m_Lorenz.Transform);
         transformGroup.Children.Add(new RotateTransform3D
            {
               Rotation = new AxisAngleRotation3D { Axis = new Vector3D(0, 1, 0), Angle = 5 }
            });
         m_Lorenz.Transform = transformGroup;
      }
      #endregion Event Handlers
   }
}

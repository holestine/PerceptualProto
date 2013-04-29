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
      private Color DIRECTION_LIGHT = Color.FromRgb(0xA0, 0xA0, 0xA0);
      private Color AMBIENT_LIGHT = Color.FromRgb(0x40, 0x40, 0x40);
      #endregion Constants

      #region Private Data
      private Model3DGroup m_Model3DGroup;
      private ModelVisual3D m_ModelVisual3D;
      private LorenzVisual m_Lorenz;
      private int m_Angle;
      private Thread m_PipelineThread;
      private GestureEngine m_GestureEngine;

      private double m_XRotation;
      private double m_YRotation;
      private double m_ZRotation;
      #endregion Private Data

      #region Public Methods

      public void SendMessage(string message)
      {
         Dispatcher.BeginInvoke(
            (Action)delegate
            {
               XTextbox.Text += message + "\n";
               XTextbox.ScrollToEnd();
            });
      }

      public void RotateX()
      {
          Dispatcher.BeginInvoke(
             (Action)delegate
             {
                 m_XRotation+=.1;
                 var xRotation = new AxisAngleRotation3D { Axis = new Vector3D(1, 0, 0), Angle = m_XRotation };
                 var transform = new RotateTransform3D { Rotation = xRotation };
                 var t = new Transform3DGroup();
                 
                 
                 foreach (var visual in XViewport.Children)
                 {
                     t.Children.Add(visual.Transform);
                     t.Children.Add(transform);
                     visual.Transform = t;
                 }
             });
      }

      public void RotateY()
      {
          Dispatcher.BeginInvoke(
             (Action)delegate
             {
                 m_YRotation+=.1;
                 var yRotation = new AxisAngleRotation3D { Axis = new Vector3D(0, 1, 0), Angle = m_YRotation };
                 var transform = new RotateTransform3D { Rotation = yRotation };
                  var t = new Transform3DGroup();


                  foreach (var visual in XViewport.Children)
                  {
                      t.Children.Add(visual.Transform);
                      t.Children.Add(transform);
                      visual.Transform = t;
                  }
             });
      }

      public void RotateZ()
      {
          Dispatcher.BeginInvoke(
             (Action)delegate
             {
                 m_ZRotation+=.1;
                 var zRotation = new AxisAngleRotation3D { Axis = new Vector3D(0, 0, 1), Angle = m_ZRotation };
                 var transform = new RotateTransform3D { Rotation = zRotation };
                 var t = new Transform3DGroup();


                 foreach (var visual in XViewport.Children)
                 {
                     t.Children.Add(visual.Transform);
                     t.Children.Add(transform);
                     visual.Transform = t;
                 }
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
         m_Angle = 0;
         m_XRotation = 0;
         m_YRotation = 0;
         m_ZRotation = 0;
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
            Color = DIRECTION_LIGHT,
            Direction = new Vector3D(0, 0, 1)
         };
         m_Model3DGroup.Children.Add(light);
         
         var ambientLight = new AmbientLight
         {
            Color = AMBIENT_LIGHT
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
         // Set selected item
         var myAxisAngleRotation3D = new AxisAngleRotation3D { Axis = new Vector3D(0, 0, 1), Angle = m_Angle += 10 };
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
         m_Angle += 10;
         var myRotateTransform3D = new RotateTransform3D();
         var myAxisAngleRotation3D = new AxisAngleRotation3D { Axis = new Vector3D(0, 1, 0), Angle = m_Angle };
         myRotateTransform3D.Rotation = myAxisAngleRotation3D;


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

         var glyph = new Glyph
                {
                    Transform = new TranslateTransform3D(new Vector3D(5*Math.Sin(m_XRotation), 10*Math.Cos(m_YRotation), Math.Cos(m_ZRotation)))
                };
          XViewport.Children.Add(glyph);
      }

      #endregion Mouse Events

      #region Window Events

      private void OnLoaded(object sender, RoutedEventArgs e)
      {
         m_GestureEngine.SetOrigin(GetCanvasCenter());

         var position = new Point3D(-100, -100, -500);

         // Add the geometry model to the viewport
         //m_Lorenz = CreateNewLorenzModel(ref position, ref RED, ref GREEN, ref BLUE, ref WHITE, DEFAULT_BRUSH_OPACITY);
         //m_Model3DGroup.Children.Add();
         m_ModelVisual3D.Content = m_Model3DGroup;
          m_Lorenz = new LorenzVisual();
         XViewport.Children.Add(m_Lorenz);
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
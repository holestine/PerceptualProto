using System;
using System.Windows;
using SharpGL.SceneGraph;
using SharpGL;
using System.Runtime.InteropServices;

namespace PerceptualComputingDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            //CreatePyramid();
        }

        /// <summary>
        /// Handles the OpenGLDraw event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        {
            //  Get the OpenGL object.
            OpenGL gl = openGLControl.OpenGL;

            //  Clear the color and depth buffer.
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            //  Load the identity matrix.
            gl.LoadIdentity();

            //  Rotate around the Y axis.
            gl.Rotate(rotation, 0.0f, 1.0f, 0.0f);

            DrawPyramid(gl);

            //  Nudge the rotation.
            rotation += speed;

            //SetPosition(100, 100);
        }

        /// <summary>
        /// Handles the OpenGLInitialized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            //  TODO: Initialise OpenGL here.

            //  Get the OpenGL object.
            OpenGL gl = openGLControl.OpenGL;

            //  Set the clear color.
            gl.ClearColor(0, 0, 0, 0);
        }

        /// <summary>
        /// Handles the Resized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_Resized(object sender, OpenGLEventArgs args)
        {
            //  TODO: Set the projection matrix here.

            //  Get the OpenGL object.
            OpenGL gl = openGLControl.OpenGL;

            //  Set the projection matrix.
            gl.MatrixMode(OpenGL.GL_PROJECTION);

            //  Load the identity.
            gl.LoadIdentity();

            //  Create a perspective transformation.
            gl.Perspective(60.0f, (double)Width / (double)Height, 0.01, 100.0);

            //  Use the 'look at' helper function to position and aim the camera.
            gl.LookAt(-5, 5, -5, 0, 0, 0, 0, 1, 0);

            //  Set the modelview matrix.
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
        }

        #region Cursor
        private void SetPosition(int a, int b)
        {
            SetCursorPos(a, b);
        }
        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);
        #endregion Cursor

        /// <summary>
        /// The current rotation.
        /// </summary>
        private float rotation = 0.0f;
        private float speed = 1.0f;

        private void On_Faster(object sender, RoutedEventArgs e)
        {
            speed += 0.5f;
        }

        private void On_Slower(object sender, RoutedEventArgs e)
        {
            speed -= 0.5f;
        }

        private void On_Listen(object sender, RoutedEventArgs e)
        {
            MyPipeline pp = new MyPipeline();
            pp.LoopFrames();
            pp.Dispose();
        }

        private void DrawPyramid(OpenGL gl)
        {
           
           gl.EnableClientState(OpenGL.GL_VERTEX_ARRAY);
           gl.EnableClientState(OpenGL.GL_NORMAL_ARRAY);

           // Point to first vertex in buffer
           //mPyramid.position(0);

           /*
           //gl.VertexPointer(3, OpenGL.GL_FLOAT, 0, mPyramid);
           gl.NormalPointer(OpenGL.GL_FLOAT, 0, mPyramid);
           gl.DrawArrays(OpenGL.GL_TRIANGLE_STRIP, 0, mNumPyramidVertices);
           gl.DisableClientState(OpenGL.GL_VERTEX_ARRAY);
           gl.DisableClientState(OpenGL.GL_NORMAL_ARRAY);
           */
           
           //  Draw a coloured pyramid.
           gl.Begin(OpenGL.GL_TRIANGLES);
           gl.Color(1.0f, 0.0f, 0.0f);
           gl.Vertex(0.0f, 1.0f, 0.0f);
           gl.Color(0.0f, 1.0f, 0.0f);
           gl.Vertex(-1.0f, -1.0f, 1.0f);
           gl.Color(0.0f, 0.0f, 1.0f);
           gl.Vertex(1.0f, -1.0f, 1.0f);
           gl.Color(1.0f, 0.0f, 0.0f);
           gl.Vertex(0.0f, 1.0f, 0.0f);
           gl.Color(0.0f, 0.0f, 1.0f);
           gl.Vertex(1.0f, -1.0f, 1.0f);
           gl.Color(0.0f, 1.0f, 0.0f);
           gl.Vertex(1.0f, -1.0f, -1.0f);
           gl.Color(1.0f, 0.0f, 0.0f);
           gl.Vertex(0.0f, 1.0f, 0.0f);
           gl.Color(0.0f, 1.0f, 0.0f);
           gl.Vertex(1.0f, -1.0f, -1.0f);
           gl.Color(0.0f, 0.0f, 1.0f);
           gl.Vertex(-1.0f, -1.0f, -1.0f);
           gl.Color(1.0f, 0.0f, 0.0f);
           gl.Vertex(0.0f, 1.0f, 0.0f);
           gl.Color(0.0f, 0.0f, 1.0f);
           gl.Vertex(-1.0f, -1.0f, -1.0f);
           gl.Color(0.0f, 1.0f, 0.0f);
           gl.Vertex(-1.0f, -1.0f, 1.0f);
           gl.End();
        }
       
        /**
  * initialize vertices for a pyramid
  */
       /*
          private void CreatePyramid()
       {
         float sqrt3 = 1.73205080757f;
    	
         // Create vertices for a pyramid
         float[][] v = new float[4][3];

         v[0][0] = -0.5f;
         v[0][1] = 0;
         v[0][2] = 0;
		
         v[1][0] = 0.5f;
         v[1][1] = 0;
         v[1][2] = 0;
		
         v[2][0] = 0;
         v[2][1] = 0f;
         v[2][2] = sqrt3/2;
		
         v[3][0] = 0;
         v[3][1] = .75f;
         v[3][2] = sqrt3/4;
		
         // Create float buffer and assign vertices
         mNumPyramidVertices = 6;
         ByteBuffer bb;
         bb = ByteBuffer.allocateDirect(v.length * v[0].length * mNumPyramidVertices);
         bb.order(ByteOrder.nativeOrder());
         mPyramid = bb.asFloatBuffer();
        
         mPyramid.put(v[0]);
         mPyramid.put(v[1]);
         mPyramid.put(v[2]);
         mPyramid.put(v[3]);
         mPyramid.put(v[0]);
         mPyramid.put(v[1]);
         
       }
           
       private  float[] mPyramid;
       private int mNumPyramidVertices;
       */
    }

    class MyPipeline : UtilMPipeline
    {
        public MyPipeline()
            : base()
        {
            EnableVoiceRecognition();
        }
        public override void OnRecognized(ref PXCMVoiceRecognition.Recognition data)
        {
            
            Console.WriteLine("Recognized: " + data.dictation);
        }
    }

}

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

        #region Workers
        private Model3DGroup GetNewPyramindModel(ref Point3D center, ref Color color, double opacity)
        {
            var p0 = new Point3D(center.X, center.Y, center.Z - 0.5);
            var p1 = new Point3D(center.X, center.Y + 1.0, center.Z);
            var p2 = new Point3D(center.X - 0.5, center.Y, center.Z - 0.5);
            var p3 = new Point3D(center.X + 0.5, center.Y, center.Z - 0.5);

            Model3DGroup pyramid = new Model3DGroup();

            pyramid.Children.Add(CreateTriangleModel(ref p2, ref p1, ref p3, ref color, opacity));
            pyramid.Children.Add(CreateTriangleModel(ref p0, ref p1, ref p2, ref color, opacity));
            pyramid.Children.Add(CreateTriangleModel(ref p3, ref p1, ref p0, ref color, opacity));
            pyramid.Children.Add(CreateTriangleModel(ref p0, ref p2, ref p3, ref color, opacity));

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

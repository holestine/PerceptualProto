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
        private const double DEFAULT_BRUSH_OPACITY = 0.9;
        private const double SQRT3 = 1.73205080757f;
        private GeometryModel3D m_GeometryModel;
        private Model3DGroup m_Model3DGroup;
        private ModelVisual3D m_ModelVisual3D;
        private Viewport3D m_Viewport3D;
        private Model3DGroup m_pyramid;
        private int m_Angle;

        public MainWindow()
        {
            InitializeComponent();

            m_Angle = 0;

            // Declare scene objects.
            m_Viewport3D = new Viewport3D();
            m_Model3DGroup = new Model3DGroup();
            m_GeometryModel = new GeometryModel3D();
            m_ModelVisual3D = new ModelVisual3D();

            // Defines the camera used to view the 3D object. In order to view the 3D object, 
            // the camera must be positioned and pointed such that the object is within view  
            // of the camera.
            var myPCamera = new PerspectiveCamera
            {
                Position = new Point3D(0, 20, -20),
                LookDirection = new Vector3D(0, -1, 1),
                FieldOfView = 90
            };
            m_Viewport3D.Camera = myPCamera;

            var myDirectionalLight = new DirectionalLight
            {
                Color = Colors.White,
                Direction = new Vector3D(-0.6, -0.5, -0.6)
            };
            m_Model3DGroup.Children.Add(myDirectionalLight);

            myDirectionalLight = new DirectionalLight
            {
                Color = Colors.White,
                Direction = new Vector3D(0, 0, 1)
            };
            m_Model3DGroup.Children.Add(myDirectionalLight);
        }

        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            m_Model3DGroup.Children.Remove(m_pyramid);
            m_Viewport3D.Children.Remove(m_ModelVisual3D);

            var pyramidColor = Color.FromRgb(0x0F, 0xF0, 0x00);
            m_pyramid = GetNewPyramindModel(new Point3D(0, 0, 0), pyramidColor, DEFAULT_BRUSH_OPACITY);
            // Add the geometry model to the model group.
            m_Model3DGroup.Children.Add(m_pyramid);

            // Add the group of models to the ModelVisual3d.
            m_ModelVisual3D.Content = m_Model3DGroup;

            m_Viewport3D.Children.Add(m_ModelVisual3D);

            //m_GeometryModel = DrawPyramid(new Point3D(0, 0, 0), pyramidColor, DEFAULT_BRUSH_OPACITY);
            /*
            var myRotateTransform3D = new RotateTransform3D();
            var myAxisAngleRotation3D = new AxisAngleRotation3D { Axis = new Vector3D(0, 1, 0), Angle = m_Angle += 10 };
            myRotateTransform3D.Rotation = myAxisAngleRotation3D;
            m_GeometryModel.Transform = myRotateTransform3D;

            // Add the geometry model to the model group.
            m_Model3DGroup.Children.Add(m_GeometryModel);

            // Add the group of models to the ModelVisual3d.
            //m_ModelVisual3D.Content = m_Model3DGroup;
            var model = new ModelVisual3D();
            model.Content = m_Model3DGroup;
            
            m_Viewport3D.Children.Add(model);

            x_page.Content = m_Viewport3D;
            */
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var p0 = new Point3D(0, 0, 1);
            var p1 = new Point3D(5, 0, 1);
            var p2 = new Point3D(0, 5, 1);
            var triangle = CreateTriangleModel(ref p0, ref p1, ref p2, Color.FromRgb(0x0F, 0xF0, 0xF0),
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
            m_pyramid = GetNewPyramindModel(new Point3D(0, 0, 0), pyramidColor, DEFAULT_BRUSH_OPACITY);

            // Add the geometry model to the model group.
            m_Model3DGroup.Children.Add(m_pyramid);

            // Add the group of models to the ModelVisual3d.
            m_ModelVisual3D.Content = m_Model3DGroup;

            m_Viewport3D.Children.Add(m_ModelVisual3D);

            m_Viewport3D.MouseDown += OnMouseDown;
            m_Viewport3D.MouseEnter += OnMouseEnter;

            x_page.Content = m_Viewport3D;
        }

        private Model3DGroup GetNewPyramindModel(Point3D center, Color color, double opacity)
        {
            var p0 = new Point3D(center.X, center.Y, center.Z - 0.5);
            var p1 = new Point3D(center.X, center.Y + 1.0, center.Z);
            var p2 = new Point3D(center.X - 0.5, center.Y, center.Z - 0.5);
            var p3 = new Point3D(center.X + 0.5, center.Y, center.Z - 0.5);

            Model3DGroup pyramid = new Model3DGroup();

            pyramid.Children.Add(CreateTriangleModel(ref p2, ref p1, ref p3, color, opacity));
            pyramid.Children.Add(CreateTriangleModel(ref p0, ref p1, ref p2, color, opacity));
            pyramid.Children.Add(CreateTriangleModel(ref p3, ref p1, ref p0, color, opacity));
            pyramid.Children.Add(CreateTriangleModel(ref p0, ref p2, ref p3, color, opacity));

            ModelVisual3D model = new ModelVisual3D();
            return pyramid;
        }

        private Model3DGroup CreateTriangleModel(ref Point3D p0, ref Point3D p1, ref Point3D p2, Color color, double opacity)
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
    }
}

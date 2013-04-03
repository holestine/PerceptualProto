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
      private const double DEFAULT_BRUSH_OPACITY = 0.9;            // Default brush opacity    
      private GeometryModel3D m_GeometryModel;
      private Model3DGroup m_Model3DGroup;
      private ModelVisual3D m_ModelVisual3D;
      private Viewport3D m_Viewport3D;
      private int m_Angle;

      public MainWindow()
      {
         InitializeComponent();

         m_Angle = 0;

         // Declare scene objects.
         m_Viewport3D = new Viewport3D();
         m_Model3DGroup = new Model3DGroup();
         //m_GeometryModel = new GeometryModel3D();
         m_ModelVisual3D = new ModelVisual3D();

         // Defines the camera used to view the 3D object. In order to view the 3D object, 
         // the camera must be positioned and pointed such that the object is within view  
         // of the camera.
         var myPCamera = new PerspectiveCamera
         {
            Position = new Point3D(0, 0, -20),
            LookDirection = new Vector3D(0, 0, 1),
            FieldOfView = 90
         };
         m_Viewport3D.Camera = myPCamera;

         var myDirectionalLight = new DirectionalLight
         {
            Color = Colors.White,
            Direction = new Vector3D(-0.6, -0.5, -0.6)
         };
         m_Model3DGroup.Children.Add(myDirectionalLight);
      }

      /// <summary>
      /// This method is used to creat a triangle taking input Points, Color and Opacity.
      /// </summary>
      /// <param name="p0"></param>
      /// <param name="p1"></param>
      /// <param name="p2"></param>
      /// <param name="color"></param>
      /// <param name="opacity"></param>
      /// <returns></returns>
      private GeometryModel3D DrawTriangle(Point3D p0, Point3D p1, Point3D p2,
                                          Color color, double opacity)
      {
         var geometry = new MeshGeometry3D();

         // Adding Positions
         geometry.Positions.Add(p0);
         geometry.Positions.Add(p1);
         geometry.Positions.Add(p2);

         // Adding triangle indices
         geometry.TriangleIndices.Add(0);
         geometry.TriangleIndices.Add(1);
         geometry.TriangleIndices.Add(2);

         // Adding normals
         Vector3D normal = CalculateNormal(p0, p1, p2);
         geometry.Normals.Add(normal);
         geometry.Normals.Add(normal);
         geometry.Normals.Add(normal);


         // Adding Positions
         geometry.Positions.Add(p2);
         geometry.Positions.Add(p1);
         geometry.Positions.Add(p0);

         // Adding triangle indices
         geometry.TriangleIndices.Add(2);
         geometry.TriangleIndices.Add(1);
         geometry.TriangleIndices.Add(0);

         // Adding normals
         normal = CalculateNormal(p2, p1, p0);
         geometry.Normals.Add(normal);
         geometry.Normals.Add(normal);
         geometry.Normals.Add(normal);

         // At last brush.
         var materialGroup = new MaterialGroup();

         var plane0Brush = new SolidColorBrush(color) {Opacity = opacity};
         var material = new DiffuseMaterial(plane0Brush);

         // Create the geometry to be added to the viewport
         materialGroup.Children.Add(material);
         var model = new GeometryModel3D(geometry, materialGroup);

         return model;
      }

      /// <summary>
      /// This function takes 3 parameters of a triangle.  Then calculates its normal
      /// </summary>
      /// <param name="p0"></param>
      /// <param name="p1"></param>
      /// <param name="p2"></param>
      /// <returns></returns>
      private Vector3D CalculateNormal(Point3D p0, Point3D p1, Point3D p2)
      {
         var vec0 = new Vector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
         var vec1 = new Vector3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
         return Vector3D.CrossProduct(vec0, vec1);
      }



      private GeometryModel3D DrawPyramid(Point3D p0, Point3D p1, Point3D p2, Point3D p3,
                                    Color color, double opacity)
      {
         var geometry = new MeshGeometry3D();

         // Vertices for pyramid
         geometry.Positions.Add(p0);
         geometry.Positions.Add(p1);
         geometry.Positions.Add(p2);
         geometry.Positions.Add(p3);

         // Side 1
         geometry.TriangleIndices.Add(0);
         geometry.TriangleIndices.Add(1);
         geometry.TriangleIndices.Add(2);

         // Side 2
         geometry.TriangleIndices.Add(2);
         geometry.TriangleIndices.Add(1);
         geometry.TriangleIndices.Add(3);

         // Side 3 
         geometry.TriangleIndices.Add(3);
         geometry.TriangleIndices.Add(1);
         geometry.TriangleIndices.Add(0);

         // Side 4
         geometry.TriangleIndices.Add(0);
         geometry.TriangleIndices.Add(2);
         geometry.TriangleIndices.Add(3);

         // Adding normals
         Vector3D normal = CalculateNormal(p0, p1, p2);
         geometry.Normals.Add(normal);
         geometry.Normals.Add(normal);
         geometry.Normals.Add(normal);

         normal = CalculateNormal(p2, p1, p3);
         geometry.Normals.Add(normal);
         geometry.Normals.Add(normal);
         geometry.Normals.Add(normal);

         normal = CalculateNormal(p3, p1, p0);
         geometry.Normals.Add(normal);
         geometry.Normals.Add(normal);
         geometry.Normals.Add(normal);

         normal = CalculateNormal(p0, p2, p3);
         geometry.Normals.Add(normal);
         geometry.Normals.Add(normal);
         geometry.Normals.Add(normal);
         /*
         // Adding Positions
         geometry.Positions.Add(p2);
         geometry.Positions.Add(p1);
         geometry.Positions.Add(p0);

         // Adding triangle indices
         geometry.TriangleIndices.Add(2);
         geometry.TriangleIndices.Add(1);
         geometry.TriangleIndices.Add(0);

         // Adding normals
         normal = CalculateNormal(p2, p1, p0);
         geometry.Normals.Add(normal);
         geometry.Normals.Add(normal);
         geometry.Normals.Add(normal);
         */
         // At last brush.
         var materialGroup = new MaterialGroup();

         var brush = new SolidColorBrush(color) { Opacity = opacity };
         var material = new DiffuseMaterial(brush);

         // Create the geometry to be added to the viewport
         materialGroup.Children.Add(material);
         var model = new GeometryModel3D(geometry, materialGroup);

         return model;
      }

      private void OnMouseEnter(object sender, MouseEventArgs e)
      {
         var pyramidColor = Color.FromRgb(0xFF, 0xF0, 0x00);
         m_GeometryModel = DrawPyramid(
         new Point3D(0, 0, 0),
         new Point3D(10, 0, 0),
         new Point3D(10, 10, 0),
         new Point3D(10, 10, 10),
         pyramidColor,
         0.9);

         m_Angle += 10;
         var myRotateTransform3D = new RotateTransform3D();
         var myAxisAngleRotation3D = new AxisAngleRotation3D { Axis = new Vector3D(0, 1, 0), Angle = m_Angle };
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
      }
      private void OnMouseDown(object sender, MouseButtonEventArgs e)
      {
         m_GeometryModel = DrawTriangle(
         new Point3D(0, 0, 1),
         new Point3D(5, 0, 1),
         new Point3D(0, 5, 1),
         Color.FromRgb(0x0F, 0xF0, 0xF0),
         DEFAULT_BRUSH_OPACITY);

         m_Angle += 10;
         var myRotateTransform3D = new RotateTransform3D();
         var myAxisAngleRotation3D = new AxisAngleRotation3D { Axis = new Vector3D(1, 0, 1), Angle = m_Angle };
         myRotateTransform3D.Rotation = myAxisAngleRotation3D;
         m_GeometryModel.Transform = myRotateTransform3D;

         m_Model3DGroup.Children.Add(m_GeometryModel);
      }

      private void OnLoaded(object sender, RoutedEventArgs e)
      {
         var pyramidColor = Color.FromRgb(0xFF, 0xF0, 0x00);
         m_GeometryModel = DrawPyramid(
         new Point3D(0, 0, 0),
         new Point3D(10, 0, 0),
         new Point3D(10, 10, 0),
         new Point3D(10, 10, 10),
         pyramidColor,
         0.9);

         // Add the geometry model to the model group.
         m_Model3DGroup.Children.Add(m_GeometryModel);

         // Add the group of models to the ModelVisual3d.
         m_ModelVisual3D.Content = m_Model3DGroup;

         m_Viewport3D.Children.Add(m_ModelVisual3D);

         m_Viewport3D.MouseDown += OnMouseDown;
         m_Viewport3D.MouseEnter += OnMouseEnter;

         x_page.Content = m_Viewport3D;
      }

   }
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace _3D
{
   public class Basic3DShapeExample : Page
   {
      private GeometryModel3D m_GeometryModel;
      private Model3DGroup m_Model3DGroup;
      private ModelVisual3D m_ModelVisual3D;
      private Viewport3D m_Viewport3D;
      private int m_Angle;

      public Basic3DShapeExample()
      {
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
               Position = new Point3D(0, 0, 2),
               LookDirection = new Vector3D(0, 0, -1),
               FieldOfView = 60
            };
         m_Viewport3D.Camera = myPCamera;
         
         var myDirectionalLight = new DirectionalLight
            {
               Color = Colors.White,
               Direction = new Vector3D(-0.61, -0.5, -0.61)
            };
         m_Model3DGroup.Children.Add(myDirectionalLight);

         // The geometry specifes the shape of the 3D plane. In this sample, a flat sheet  
         // is created.
         var myMeshGeometry3D = new MeshGeometry3D();

         // Create a collection of normal vectors for the MeshGeometry3D.
         var myNormalCollection = new Vector3DCollection
            {
               new Vector3D(0, 0, 1),
               new Vector3D(0, 0, 1),
               new Vector3D(0, 0, 1),
               new Vector3D(0, 0, 1),
               new Vector3D(0, 0, 1),
               new Vector3D(0, 0, 1)
            };
         myMeshGeometry3D.Normals = myNormalCollection;

         // Create a collection of vertex positions for the MeshGeometry3D. 
         var myPositionCollection = new Point3DCollection
            {
               new Point3D(-0.5, -0.5, 0.5),
               new Point3D(0.5, -0.5, 0.5),
               new Point3D(0.5, 0.5, 0.5),
               new Point3D(0.5, 0.5, 0.5),
               new Point3D(-0.5, 0.5, 0.5),
               new Point3D(-0.5, -0.5, 0.5)
            };
         myMeshGeometry3D.Positions = myPositionCollection;

         // Create a collection of texture coordinates for the MeshGeometry3D.
         var myTextureCoordinatesCollection = new PointCollection
            {
               new Point(0, 0),
               new Point(1, 0),
               new Point(1, 1),
               new Point(1, 1),
               new Point(0, 1),
               new Point(0, 0)
            };
         myMeshGeometry3D.TextureCoordinates = myTextureCoordinatesCollection;

         // Create a collection of triangle indices for the MeshGeometry3D.
         var myTriangleIndicesCollection = new Int32Collection {0, 1, 2, 3, 4, 5};
         myMeshGeometry3D.TriangleIndices = myTriangleIndicesCollection;

         // Apply the mesh to the geometry model.
         m_GeometryModel.Geometry = myMeshGeometry3D;

         // Create a horizontal linear gradient with four stops.   
         var myHorizontalGradient = new LinearGradientBrush
            {
               StartPoint = new Point(0, 0.5),
               EndPoint = new Point(1, 0.5)
            };
         myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.Yellow, 0.0));
         myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.Red, 0.25));
         myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.Blue, 0.75));
         myHorizontalGradient.GradientStops.Add(new GradientStop(Colors.LimeGreen, 1.0));

         // Define material and apply to the mesh geometries.
         var myMaterial = new DiffuseMaterial(myHorizontalGradient);
         m_GeometryModel.Material = myMaterial;

         // Apply a transform to the object. In this sample, a rotation transform is applied,   
         // rendering the 3D object rotated.
         var myRotateTransform3D = new RotateTransform3D();
         var myAxisAngleRotation3D = new AxisAngleRotation3D {Axis = new Vector3D(0, 3, 0), Angle = 40};
         myRotateTransform3D.Rotation = myAxisAngleRotation3D;
         m_GeometryModel.Transform = myRotateTransform3D;

         // Add the geometry model to the model group.
         m_Model3DGroup.Children.Add(m_GeometryModel);

         // Add the group of models to the ModelVisual3d.
         m_ModelVisual3D.Content = m_Model3DGroup;

         m_Viewport3D.Children.Add(m_ModelVisual3D);

         // Apply the viewport to the page so it will be rendered. 
         Content = m_Viewport3D;
      }

      public void rotate(int angle)
      {
         var myRotateTransform3D = new RotateTransform3D();
         var myAxisAngleRotation3D = new AxisAngleRotation3D { Axis = new Vector3D(1, 0, 1), Angle = (double)m_Angle++ };
         myRotateTransform3D.Rotation = myAxisAngleRotation3D;
         m_GeometryModel.Transform = myRotateTransform3D;

         // Add the geometry model to the model group.
         m_Model3DGroup.Children.Add(m_GeometryModel);

         // Add the group of models to the ModelVisual3d.
         m_ModelVisual3D.Content = m_Model3DGroup;

         m_Viewport3D.Children.Clear();
         m_Viewport3D.Children.Add(m_ModelVisual3D);

         // Apply the viewport to the page so it will be rendered. 
         Content = m_Viewport3D;
      }
   }
}
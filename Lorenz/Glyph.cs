using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Lorenz
{
    class Glyph : ModelVisual3D
    {
        private const double DEFAULT_BRUSH_OPACITY = 0.5;
        private Color RED = Color.FromRgb(0xFF, 0x00, 0x00);
        private Color GREEN = Color.FromRgb(0x00, 0xFF, 0x00);
        private Color BLUE = Color.FromRgb(0x00, 0x00, 0xFF);
        private Color LIME = Color.FromRgb(0xAA, 0xFF, 0xCC);

       private Color m_Color;

        public Glyph()
        {
            var pos = new Point3D(0, 0, 0);
            Content = GetNewPyramindModel(ref pos, ref RED, ref GREEN, ref BLUE, ref LIME, DEFAULT_BRUSH_OPACITY);
        }

        public Glyph(Color c)
        {
           var pos = new Point3D(0, 0, 0);
           Content = GetNewPyramindModel(ref pos, ref c, ref c, ref c, ref c, DEFAULT_BRUSH_OPACITY);
        }

       public Color Color
       {
          get { return m_Color; }
          set { m_Color = value; }
       }

       private Model3DGroup GetNewPyramindModel(ref Point3D center, ref Color color1, ref Color color2, ref Color color3, ref Color color4, double opacity)
        {
            var pyramid = new Model3DGroup();

            var p0 = new Point3D(center.X, center.Y, center.Z);
            var p1 = new Point3D(center.X, center.Y + 1.0, center.Z + 0.5);
            var p2 = new Point3D(center.X + 1.0, center.Y, center.Z + 1.0);
            var p3 = new Point3D(center.X - 1.0, center.Y, center.Z + 1.0);

            pyramid.Children.Add(CreateTriangleModel(ref p2, ref p1, ref p3, ref color1, opacity));
            pyramid.Children.Add(CreateTriangleModel(ref p0, ref p1, ref p2, ref color2, opacity));
            pyramid.Children.Add(CreateTriangleModel(ref p3, ref p1, ref p0, ref color3, opacity));
            pyramid.Children.Add(CreateTriangleModel(ref p0, ref p2, ref p3, ref color4, opacity));

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
    }
}

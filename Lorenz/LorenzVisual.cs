using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Lorenz
{
   class LorenzVisual : ModelVisual3D
   {
      const int NUM_POINTS = 1000;
      const double STEP_SIZE = .005;
      
      private Point3D m_StartPos;

      public LorenzVisual()
      {
         m_StartPos = new Point3D(-75, 75, 100);
         CreateVisual();
      }

      public LorenzVisual(Point3D start)
      {
         m_StartPos = start;
         CreateVisual();
      }

      public LorenzVisual(Point3D start, Color color)
      {
         m_StartPos = start;
         CreateVisual(color);
      }

      public Point3D StartingPoint
      {
         get { return m_StartPos; }
      }

      private void CreateVisual()
      {
         var pos = m_StartPos;
         for (var i = 0; i < NUM_POINTS; i++)
         {
            var glyph = new Glyph
               {
                  Transform = new TranslateTransform3D(pos.X, pos.Y, pos.Z)
               };

            Children.Add(glyph);
            pos = RK4Lorenz(pos, STEP_SIZE);
         }
      }

      private void CreateVisual(Color color)
      {
         for (var i = 0; i < NUM_POINTS; i++)
         {
            var glyph = new Glyph(color)
            {
               Transform = new TranslateTransform3D(m_StartPos.X, m_StartPos.Y, m_StartPos.Z),
            };

            Children.Add(glyph);
            m_StartPos = RK4Lorenz(m_StartPos, STEP_SIZE);
         }
      }

      private Point3D RK4Lorenz(Point3D pos, double dt)
      {
         // Obtain and store first set of slopes
         var f1 = Lorenz(pos);

         // Compute next Euler position with first set of slopes and half time step
         var xyz = Euler(pos, f1, dt / 2);

         // Obtain and store second set of slopes
         var f2 = Lorenz(xyz);

         // Compute next Euler position with second set of slopes and half time step
         xyz = Euler(pos, f2, dt / 2);

         // Obtain and store third set of slopes
         var f3 = Lorenz(xyz);

         // Compute next Euler position with third set of slopes and full time step
         xyz = Euler(pos, f3, dt);

         // Obtain and store fourth set of slopes
         var f4 = Lorenz(xyz);

         // Compute weighted average of slopes according to Runge-Kutta fourth order algorithm
         var rkSlope = new Point3D(f1.X / 6 + f2.X / 3 + f3.X / 3 + f4.X / 6,
                                       f1.Y / 6 + f2.Y / 3 + f3.Y / 3 + f4.Y / 6,
                                       f1.Z / 6 + f2.Z / 3 + f3.Z / 3 + f4.Z / 6);

         // Return next position using Euler with Runge-Kutta slope
         return Euler(pos, rkSlope, dt);
      }

      private Point3D Lorenz(Point3D pos)
      {
         const double SIGMA = 10;
         const double RHO = 28;
         const double BETA = 8 / 3;

         return new Point3D(SIGMA * (pos.Y - pos.X),
               pos.X * (RHO - pos.Z) - pos.Y,
               pos.X * pos.Y - BETA * pos.Z);
      }

      private Point3D Euler(Point3D pos, Point3D slope, double dt)
      {
         return new Point3D(pos.X + slope.X * dt, pos.Y + slope.Y * dt, pos.Z + slope.Z * dt);
      }

      public void Recalculate(Point3D start)
      {
         // TODO Modify existing children instead of creating new ones
         m_StartPos = start;
         Children.Clear();
         CreateVisual();
      }
   }
}

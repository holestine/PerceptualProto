using System.Windows.Media.Media3D;

namespace Lorenz
{
   class LorenzVisual : ModelVisual3D
   {
      #region Constants
      const int NUM_POINTS = 500;
      const double STEP_SIZE = .01;
      #endregion Constants

      #region Private Data
      private Point3D m_StartPos;
      #endregion Private Data

      #region Properties
      public Point3D StartingPoint
      {
         get { return m_StartPos; }
      }
      #endregion Properties

      #region Initialization
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
      #endregion Initialization

      #region Public Methods
      public void Move(Point3D delta)
      {
         Recalculate(new Point3D(m_StartPos.X + delta.X, m_StartPos.Y + delta.Y, m_StartPos.Z + delta.Z));
      }

      public void Recalculate(Point3D start)
      {
         m_StartPos = start;
         Point3D pos = start;

         foreach (Glyph glyph in Children)
         {
            glyph.Transform = new TranslateTransform3D(pos.X, pos.Y, pos.Z);
            pos = RK4Lorenz(pos, STEP_SIZE);
         }

         /* it would be better to get this approach working for the animation
          * http://msdn.microsoft.com/en-us/library/ms743217(v=vs.85).aspx
         var animation = new DoubleAnimation()
           {
              From = 0.0,
              To = start.X,
              BeginTime = TimeSpan.FromSeconds(1)
           };
             
         glyph.SetAnimation(animation);
         */
      }
      #endregion Public Methods

      #region Private Methods
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
      
      private Point3D RK4Lorenz(Point3D start, double dt)
      {
         // Obtain and store slopes at starting point
         var f1 = Lorenz(start);

         // Obtain and store second set of slopes with first set of slopes and half time step
         var f2 = Lorenz(Euler(start, f1, dt / 2));

         // Obtain and store third set of slopes with second set of slopes and half time step
         var f3 = Lorenz(Euler(start, f2, dt / 2));

         // Obtain and store fourth set of slopes with third set of slopes and full time step
         var f4 = Lorenz(Euler(start, f3, dt));

         // Compute weighted average of slopes according to Runge-Kutta fourth order algorithm
         var rkSlope = new Point3D(f1.X / 6 + f2.X / 3 + f3.X / 3 + f4.X / 6,
                                   f1.Y / 6 + f2.Y / 3 + f3.Y / 3 + f4.Y / 6,
                                   f1.Z / 6 + f2.Z / 3 + f3.Z / 3 + f4.Z / 6);

         // Return next position using Euler, Runge-Kutta slope and full time step
         return Euler(start, rkSlope, dt);
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
      #endregion Private Methods
   }
}

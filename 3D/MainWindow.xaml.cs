using System.Windows.Input;
using System.Windows.Shapes;

namespace _3D
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow
   {
      private Basic3DShapeExample m_Shape;

      public MainWindow()
      {
         InitializeComponent();

         m_Shape = new Basic3DShapeExample();
         m_Shape.Width = 400;
         m_Shape.Height = 300;
         m_Shape.MouseDown += onMouseDown;

         x_window.AddChild(m_Shape);
      }

      private void onMouseDown(object sender, MouseButtonEventArgs e)
      {
         m_Shape.rotate(10);
      }
   }
}

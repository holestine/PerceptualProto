using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace Lorenz
{
    partial class Glyph : ModelVisual3D
    {
       public Glyph()
        {
           InitializeComponent();
        }

       public void SetAnimation(DoubleAnimation animation)
       {
          var storyBoard = new Storyboard();
          Storyboard.SetTargetName(animation, "XTransform");
          Storyboard.SetTargetProperty(animation, new PropertyPath(TranslateTransform3D.OffsetXProperty));
          storyBoard.Children.Add(animation);
          storyBoard.Begin();

          //http://msdn.microsoft.com/en-us/library/ms743217(v=vs.85).aspx
       }
    }
}
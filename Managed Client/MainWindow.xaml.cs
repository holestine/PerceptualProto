using System.Windows;
using ManagedCPP;

namespace Managed_Client
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      public MainWindow()
      {
         InitializeComponent();
         var x = ManagedClass.add(1, 2);

         var managedClass = new ManagedClass();
         var y = managedClass.getX();
         var z = managedClass.Count("asdf");
      }
   }
}

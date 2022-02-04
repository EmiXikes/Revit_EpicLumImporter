using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EpicLumi.UI.View
{
    /// <summary>
    /// Interaction logic for ChangeElevationUI.xaml
    /// </summary>
    public partial class ChangeElevationUI : UserControl
    {
        public ChangeElevationUI()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            tBox.Focus();
            tBox.SelectAll();
        }

        private void tBox_KeyUp_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                okBtn.Command.Execute(null);
            }
        }
    }
}

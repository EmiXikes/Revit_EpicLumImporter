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
    /// Interaction logic for UniqueLumSelectItem.xaml
    /// </summary>
    public partial class UniqueLumSelectItem : UserControl
    {
        public UniqueLumSelectItem()
        {
            InitializeComponent();
        }

        private void DWGName_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var tb = sender as TextBox;
            tb.SelectAll();
            tb.Copy();
            ToolTip tt = tb.ToolTip as ToolTip;
            tt.Content = "Copied!";
            tt.IsOpen = true;
        }

        private void DWGName_MouseLeave(object sender, MouseEventArgs e)
        {
            var tb = sender as TextBox;
            ToolTip tt = tb.ToolTip as ToolTip;
            tt.IsOpen = false;
        }

        private void DWGName_MouseEnter(object sender, MouseEventArgs e)
        {
            var tb = sender as TextBox;
            ToolTip tt = tb.ToolTip as ToolTip;
            tt.Content = "Double-click to copy.";
        }
    }
}

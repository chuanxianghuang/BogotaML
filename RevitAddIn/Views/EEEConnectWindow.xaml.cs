using RevitAddIn.UICommon;
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
using System.Windows.Shapes;

namespace RevitAddIn.Views
{
    /// <summary>
    /// EEEConnectWindow.xaml 的交互逻辑
    /// </summary>
    public partial class EEEConnectWindow : Window
    {
        public EEEConnectWindow()
        {
            InitializeComponent();
        }

        private void BtnCancle_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            KeysPress.SetESC();
        }
    }
}

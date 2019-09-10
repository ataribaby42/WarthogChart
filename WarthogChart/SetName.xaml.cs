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

namespace WarthogChart
{
    /// <summary>
    /// Interakční logika pro SetName.xaml
    /// </summary>
    public partial class SetName : Window
    {
        private bool canClose = false;
        MainWindow parent = null;

        public SetName(MainWindow parent, string name)
        {
            InitializeComponent();

            this.parent = parent;

            if(!string.IsNullOrEmpty(name))
            {
                tb_name.Text = name.Trim();
                tb_name.Select(tb_name.Text.Length, 0);
            }

            tb_name.Focus();
        }

        private void Button_Ok_Click(object sender, RoutedEventArgs e)
        {
            if (parent != null)
            {
                parent.SetName(tb_name.Text);
                parent.Changed = true;
            }

            canClose = true;
            Close();
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            canClose = true;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!canClose)
            {
                e.Cancel = true;
            }
            else
            {
                parent = null;
            }
        }

        private void Tb_name_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                Button_Ok_Click(null, null);
            }
            else if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Button_Cancel_Click(null, null);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Interakční logika pro SetFont.xaml
    /// </summary>
    public partial class SetFont : Window
    {
        private bool canClose = false;
        MainWindow parent = null;

        public SetFont(MainWindow parent, string fontName, double fontSize, double lineHeight)
        {
            InitializeComponent();

            this.parent = parent;

            tb_name.Text = fontName;
            tb_size.Text = fontSize.ToString();
            tb_height.Text = lineHeight.ToString();

            tb_name.Select(tb_name.Text.Length, 0);
            tb_name.Focus();
        }

        private void Button_Ok_Click(object sender, RoutedEventArgs e)
        {
            if (parent != null)
            {
                double fontSize;
                double.TryParse(tb_size.Text, out fontSize);

                if (fontSize <= 0)
                {
                    fontSize = 7;
                }

                double fontheight;
                double.TryParse(tb_height.Text, out fontheight);

                if (fontheight <= 0)
                {
                    fontheight = fontSize;
                }

                parent.SetFont(string.IsNullOrEmpty(tb_name.Text.Trim()) ? "Arial Narrow" : tb_name.Text.Trim(), fontSize, fontheight);
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

        private void Tb_size_KeyDown(object sender, KeyEventArgs e)
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

        private void Tb_height_KeyDown(object sender, KeyEventArgs e)
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

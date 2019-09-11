using System;
using System.Reflection;
using System.Windows;

namespace WarthogChart
{
    /// <summary>
    /// Interakční logika pro About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();

            Version ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            AssemblyCopyrightAttribute copyright = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0] as AssemblyCopyrightAttribute;
            string copy = copyright.Copyright;

            versionText.Text = "v" + ver.ToString(4);
            copyrightText.Text = copy;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WarthogChart
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            ResourceDictionary rd;
            rd = ((ResourceDictionary)Application.LoadComponent(new Uri(@"WarthogChart;;;component\Themes/Generic.xaml", UriKind.Relative)));
            Application.Current.Resources.MergedDictionaries.Add(rd);
        }
    }
}

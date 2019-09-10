using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace WarthogChart
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SerializableDictionary<string, string> controls = new SerializableDictionary<string, string>();
        IControlSet controlSet = null;

        private bool changed = false;
        public bool Changed
        {
            get { return changed; }
            set { changed = value; UpdateStatusLine(); }
        }

        private string profileFileName = string.Empty;
        public string ProfileFileName
        {
            get { return profileFileName; }
            set { profileFileName = value; UpdateStatusLine(); }
        }

        public MainWindow()
        {
            InitializeComponent();

            SwitchToStick();
            Clear();
        }

        private bool TestChanged()
        {
            if (changed)
            {
                System.Windows.MessageBoxResult answer = MessageBox.Show(this, "There are unsaved changes. Do you really want to exit?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

                if (answer != MessageBoxResult.Yes)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private void UpdateStatusLine()
        {
            statusBarText.Text = string.Empty;

            if (changed)
            {
                statusBarText.Text += "*";
            }

            statusBarText.Text += profileFileName;
            menuSave.IsEnabled = !string.IsNullOrEmpty(ProfileFileName);
        }

        private void SwitchToStick()
        {
            if (controlSet != null)
            {
                UpdateControls(controlSet.GetSet());
                controlSet.CleanUp();
            }

            grd_diagram.Children.Clear();
            Stick stick = new Stick(this);
            controlSet = stick;
            controlSet.SetSet(controls);
            grd_diagram.Children.Add(stick);
        }

        private void SwitchToThrottle()
        {
            if (controlSet != null)
            {
                UpdateControls(controlSet.GetSet());
                controlSet.CleanUp();
            }

            grd_diagram.Children.Clear();
            Throttle throttle = new Throttle(this);
            controlSet = throttle;
            controlSet.SetSet(controls);
            grd_diagram.Children.Add(throttle);
        }

        private void Save(string fileName)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SerializableDictionary<string, string>), new XmlRootAttribute("WarthogChart"));
            TextWriter textWriter = new StreamWriter(fileName);
            serializer.Serialize(textWriter, controls);
            textWriter.Close();
        }

        private void Load(string fileName)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SerializableDictionary<string, string>), new XmlRootAttribute("WarthogChart"));
            TextReader textReader = new StreamReader(fileName);
            controls = serializer.Deserialize(textReader) as SerializableDictionary<string, string>;
            textReader.Close();
        }

        private void Clear()
        {
            if (controlSet == null) return;

            Changed = false;
            ProfileFileName = string.Empty;

            List<string> keys = new List<string>(controls.Keys);

            foreach (string key in keys)
            {
                controls[key] = string.Empty;
            }

            controlSet.SetSet(controls);
        }

        private void Print(bool stick)
        {
            PrintDialog printDlg = new PrintDialog();
            if (printDlg.ShowDialog() == true)
            {
                Size pageSize = new Size(printDlg.PrintableAreaWidth, printDlg.PrintableAreaHeight);
                Transform transform = new RotateTransform(0); ;

                /*
                if(printDlg.PrintTicket.PageOrientation == System.Printing.PageOrientation.Landscape)
                {
                    transform = new RotateTransform(90);
                }
                else
                {
                    transform = new RotateTransform(0);
                }
                */

                UserControl print;

                if (stick)
                {
                    print = new Stick(null);
                }
                else
                {
                    print = new Throttle(null);
                }

                print.LayoutTransform = transform;
                ((IControlSet)print).SetSet(controls);
                print.Measure(pageSize);
                print.Arrange(new Rect(0, 0, pageSize.Width, pageSize.Height));
                printDlg.PrintVisual(print, "WarthogChart " + (print is Stick ? "Stick" : "Throttle"));
                ((IControlSet)print).CleanUp();
            }
        }

        private void Export(bool stick)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.DefaultExt = ".png";
            dlg.Filter = "PNG files (.png)|*.png";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                UserControl export;

                if (stick)
                {
                    export = new Stick(null);
                }
                else
                {
                    export = new Throttle(null);
                }

                try
                {
                    Size pageSize = new Size(453 * 10, 605 * 10);

                    ((IControlSet)export).SetSet(controls);
                    export.Measure(pageSize);
                    export.Arrange(new Rect(0, 0, pageSize.Width, pageSize.Height));

                    RenderTargetBitmap rtb = new RenderTargetBitmap((int)export.ActualWidth, (int)export.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                    DrawingVisual dv = new DrawingVisual();
                    using (DrawingContext ctx = dv.RenderOpen())
                    {
                        VisualBrush vb = new VisualBrush(export);
                        ctx.DrawRectangle(vb, null, new Rect(new Point(), pageSize));
                    }
                    rtb.Render(dv);

                    PngBitmapEncoder png = new PngBitmapEncoder();
                    png.Frames.Add(BitmapFrame.Create(rtb));

                    using (Stream fileStream = new FileStream(dlg.FileName, FileMode.Create))
                    {
                        png.Save(fileStream);
                    }
                }
                catch
                {
                    MessageBox.Show(this, "Error occured during chart export saving!", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                }
                finally
                {
                    ((IControlSet)export).CleanUp();
                }
            }
        }

        private void MenuItemAbout_Click(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.Owner = this;
            about.ShowDialog();
        }

        private void MenuItemOpen_Click(object sender, RoutedEventArgs e)
        {
            if (TestChanged()) return;
            if (controlSet == null) return;

            OpenFileDialog dlg = new OpenFileDialog();

            dlg.DefaultExt = ".wch";
            dlg.Filter = "Warthog Chart files (.wch)|*.wch";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                try
                {
                    Load(dlg.FileName);
                    ProfileFileName = dlg.FileName;
                    controlSet.SetSet(controls);
                    SwitchToStick();
                }
                catch
                {
                    MessageBox.Show(this, "Error occured during chart loading!", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                    ProfileFileName = string.Empty;
                }

                Changed = false;
            }
        }

        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            if (controlSet == null) return;

            if (string.IsNullOrEmpty(ProfileFileName)) return;

            try
            {
                UpdateControls(controlSet.GetSet());
                Save(ProfileFileName);
                Changed = false;
            }
            catch
            {
                MessageBox.Show(this, "Error occured during chart saving!", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                ProfileFileName = string.Empty;
            }
        }

        private void MenuItemSaveAs_Click(object sender, RoutedEventArgs e)
        {
            if (controlSet == null) return;

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.DefaultExt = ".wch";
            dlg.Filter = "Warthog Chart files (.wch)|*.wch";
            dlg.FileName = profileFileName;

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                ProfileFileName = dlg.FileName;

                try
                {
                    UpdateControls(controlSet.GetSet());
                    Save(dlg.FileName);
                    Changed = false;
                }
                catch
                {
                    MessageBox.Show(this, "Error occured during chart saving!", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                    ProfileFileName = string.Empty;
                }
            }
        }

        private void MenuItemSetChartName_Click(object sender, RoutedEventArgs e)
        {
            if (controlSet == null) return;

            string name = string.Empty;

            if (controls.ContainsKey("profile_name"))
            {
                name = controls["profile_name"];
            }

            SetName setName = new SetName(this, name);
            setName.Owner = this;
            setName.ShowDialog();
        }

        private void MenuItemViewStick_Click(object sender, RoutedEventArgs e)
        {
            if (controlSet == null) return;

            SwitchToStick();
        }

        private void MenuItemViewThrottle_Click(object sender, RoutedEventArgs e)
        {
            if (controlSet == null) return;

            SwitchToThrottle();
        }

        private void MenuItemNew_Click(object sender, RoutedEventArgs e)
        {
            if (TestChanged()) return;
            if (controlSet == null) return;

            Clear();
            SwitchToStick();
        }

        private void MenuItemPrint_Click(object sender, RoutedEventArgs e)
        {
            if (controlSet == null) return;

            Print(controlSet is Stick);
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuItemExport_Click(object sender, RoutedEventArgs e)
        {
            if (controlSet == null) return;

            Export(controlSet is Stick);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (TestChanged())
            {
                e.Cancel = true;
                return;
            }
        }

        public void UpdateControls(SerializableDictionary<string, string> set)
        {
            UpdateControls(set, false);
        }

        public void UpdateControls(SerializableDictionary<string, string> set, bool changed)
        {
            if (changed)
            {
                this.Changed = true;
            }

            foreach (var item in set)
            {
                if (controls.ContainsKey(item.Key))
                {
                    controls[item.Key] = item.Value;
                }
                else
                {
                    controls.Add(item.Key, item.Value);
                }
            }
        }

        public void SetName(string name)
        {
            if (controlSet == null) return;

            {
                if (controls.ContainsKey("profile_name"))
                {
                    controls["profile_name"] = name.Trim();
                }
                else
                {
                    controls.Add("profile_name", name.Trim());
                }
            }

            controlSet.SetSet(controls);
        }

    }
}

﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        enum device { stick, stickHornet, throttle }
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

        private void SwitchToStickHornet()
        {
            if (controlSet != null)
            {
                UpdateControls(controlSet.GetSet());
                controlSet.CleanUp();
            }

            grd_diagram.Children.Clear();
            StickHornet stick = new StickHornet(this);
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

        private void Print(device target)
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
                string name = string.Empty;

                if (target == device.stick)
                {
                    print = new Stick(null);
                    name = "Warthog Stick";
                }
                else if (target == device.stickHornet)
                {
                    print = new StickHornet(null);
                    name = "F/A-18 Hornet Stick";
                }
                else
                {
                    print = new Throttle(null);
                    name = "Warthog Throttle";
                }

                print.Margin = new Thickness(printDlg.PrintableAreaWidth * 0.03);
                print.LayoutTransform = transform;
                ((IControlSet)print).SetSet(controls);
                print.Measure(pageSize);
                print.Arrange(new Rect(0, 0, pageSize.Width, pageSize.Height));



                printDlg.PrintVisual(print, "WarthogChart " + name);
                ((IControlSet)print).CleanUp();
            }
        }

        private void Export(device target)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.DefaultExt = ".png";
            dlg.Filter = "PNG files (.png)|*.png";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                UserControl export;

                if (target == device.stick)
                {
                    export = new Stick(null);
                }
                else if (target == device.stickHornet)
                {
                    export = new StickHornet(null);
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

            if (!string.IsNullOrEmpty(profileFileName))
            {
                dlg.InitialDirectory = Path.GetDirectoryName(profileFileName);
                dlg.FileName = Path.GetFileName(profileFileName);
            }

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

        private void MenuItemViewStickHornet_Click(object sender, RoutedEventArgs e)
        {
            if (controlSet == null) return;

            SwitchToStickHornet();
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

            device target = device.stick;

            if (controlSet is StickHornet)
            {
                target = device.stickHornet;
            }
            else if (controlSet is Throttle)
            {
                target = device.throttle;
            }

            Print(target);
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuItemExport_Click(object sender, RoutedEventArgs e)
        {
            if (controlSet == null) return;

            device target = device.stick;

            if (controlSet is StickHornet)
            {
                target = device.stickHornet;
            }
            else if (controlSet is Throttle)
            {
                target = device.throttle;
            }

            Export(target);
        }

        private void MenuItemSetFont_Click(object sender, RoutedEventArgs e)
        {
            if (controlSet == null) return;

            string fontName = "Arial Narrow";
            double fontSize = 0;
            double fontHeight = 0;

            if (controls.ContainsKey("font_family"))
            {
                fontName = controls["font_family"];
            }

            if (controls.ContainsKey("font_size"))
            {
                double.TryParse(controls["font_size"], NumberStyles.Float, CultureInfo.InvariantCulture, out fontSize);
            }

            if (fontSize <= 0)
            {
                fontSize = 7;
            }

            if (controls.ContainsKey("font_height"))
            {
                double.TryParse(controls["font_height"], NumberStyles.Float, CultureInfo.InvariantCulture, out fontHeight);
            }

            if (fontHeight <= 0)
            {
                fontHeight = fontSize;
            }

            SetFont setFont = new SetFont(this, fontName, fontSize, fontHeight);
            setFont.Owner = this;
            setFont.ShowDialog();
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

        public void SetFont(string fontName, double fontSize, double fontHeight)
        {
            if (controlSet == null) return;

            if (controls.ContainsKey("font_family"))
            {
                controls["font_family"] = fontName;
            }
            else
            {
                controls.Add("font_family", fontName);
            }

            if (controls.ContainsKey("font_size"))
            {
                controls["font_size"] = fontSize.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                controls.Add("font_size", fontSize.ToString(CultureInfo.InvariantCulture));
            }

            if (controls.ContainsKey("font_height"))
            {
                controls["font_height"] = fontHeight.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                controls.Add("font_height", fontHeight.ToString(CultureInfo.InvariantCulture));
            }

            controlSet.SetSet(controls);
        }

    }
}

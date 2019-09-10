using System.Windows.Controls;

namespace WarthogChart
{
    /// <summary>
    /// Interaction logic for Stick.xaml
    /// </summary>
    public partial class Stick : UserControl, IControlSet
    {
        MainWindow parent = null;
        bool inhibitChangeHandling = false;

        public Stick(MainWindow parent)
        {
            InitializeComponent();

            this.parent = parent;

            if(parent != null)
            {
                for (int i = 0; i < grd_controls.Children.Count; i++)
                {
                    TextBox box = grd_controls.Children[i] as TextBox;

                    if (box != null)
                    {
                        box.TextChanged += Box_TextChanged;
                    }
                }
            }
        }

        private void Box_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (parent == null) return;

            if (inhibitChangeHandling) return;

            SerializableDictionary<string, string> set = new SerializableDictionary<string, string>();
            TextBox box = sender as TextBox;

            if (box != null)
            {
                set.Add(box.Name, box.Text.Trim());
            }

            parent.UpdateControls(set, true);
        }

        public SerializableDictionary<string, string> GetSet()
        {
            SerializableDictionary<string, string> set = new SerializableDictionary<string, string>();

            for (int i = 0; i < grd_controls.Children.Count; i++)
            {
                TextBox box = grd_controls.Children[i] as TextBox;

                if(box != null)
                {
                    set.Add(box.Name, box.Text.Trim());
                }
            }

            return set;
        }

        public void SetSet(SerializableDictionary<string, string> set)
        {
            inhibitChangeHandling = true;

            if (set.ContainsKey("profile_name"))
            {
                tb_name.Text = set["profile_name"];
            }

            foreach (var item in set)
            {
                TextBox textBox = (TextBox)grd_controls.FindName(item.Key);

                if(textBox != null)
                {
                    textBox.Text = item.Value;
                }
            }

            inhibitChangeHandling = false;
        }

        public void CleanUp()
        {
            parent = null;
        }

    }
}

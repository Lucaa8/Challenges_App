using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Challenges_App.Pages.Meta
{
    /// <summary>
    /// Interaction logic for Skull.xaml
    /// </summary>
    public partial class Skull : Page, IMeta
    {
        private Item parent;

        private int minCache = 0;
        private int maxCache = 2_419_200; //1 week / 7 days
        private Func<int, bool> ValidateCache => cache => cache >= minCache && cache <= maxCache;

        public Skull(Item parent)
        {
            this.parent = parent;
            InitializeComponent();
            lblTitle.Content = lblTitle.Content.ToString().Replace("[I]", this.parent.getName());
        }

        public Skull(Item parent, JObject json) : this(parent)
        {
            if (json.ContainsKey("Type"))
            {
                foreach(ComboBoxItem i in cbxType.Items)
                {
                    if (i.Content.ToString().Equals(json.Value<String>("Type")))
                    {
                        cbxType.SelectedItem = i;
                        break;
                    }
                }
            }
            if (json.ContainsKey("Owner"))
            {
                tbxValue.Text = json.Value<String>("Owner");
            }
            if(json.ContainsKey("UseCache"))
            {
                tbxCache.Text = json.Value<Int32>("UseCache").ToString();
            }
        }

        IMeta.Meta IMeta.getMeta()
        {
            return IMeta.Meta.Skull;
        }

        void IMeta.navigate()
        {
            MainWindow.Instance.MainFrame.Navigate(this);
        }

        JObject IMeta.toJson()
        {
            JObject json = new JObject();
            String type = ((ComboBoxItem)cbxType.SelectedItem).Content.ToString();
            json["Type"] = type;
            if (!type.Equals("PLAYER"))
            {
                json["Owner"] = tbxValue.Text;
            }
            if(type.Equals("PSEUDO"))
            {
                if(Int32.TryParse(tbxCache.Text, out var cache) && ValidateCache(cache))
                {
                    json["UseCache"] = cache;
                } else {
                    json["UseCache"] = 3600;
                    // Just update the GUI field in case the player saves this item (either by saving it locally or sending it to the server by sending a challenge using it)
                    // and then later on come back on this page, to avoid desync
                    tbxCache.Text = "3600";
                }
            }
            return json;
        }

        private void tbxCache_TextChanged(object sender, TextChangedEventArgs e)
        {
            cvCacheError.Visibility = Int32.TryParse(tbxCache.Text, out var cache) && ValidateCache(cache) ? Visibility.Hidden : Visibility.Visible;
        }

        private void cbxType_SelecChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tbxValue == null) return;
            switch (cbxType.SelectedIndex)
            {
                case 0: //PLAYER
                    {
                        tbxValue.Width = 190;
                        tbxValue.Height = 25;
                        tbxValue.IsEnabled = false;
                        tbxValue.Text = "";
                        cvCache.Visibility = Visibility.Hidden;
                        tbxCache.Text = "";
                    }
                    break;
                case 1: //PSEUDO
                    {
                        tbxValue.Width = 190;
                        tbxValue.Height = 25;
                        tbxValue.IsEnabled = true;
                        cvCache.Visibility = Visibility.Visible;
                        tbxCache.Text = "3600";
                    }
                    break;
                case 2: //MCHEADS
                    {
                        tbxValue.Width = 500;
                        tbxValue.Height = 90;
                        tbxValue.IsEnabled = true;
                        cvCache.Visibility = Visibility.Hidden;
                        tbxCache.Text = "";
                    }
                    break;
            }
        }

        private void btnBack_MouseEnter(object sender, MouseEventArgs e)
        {
            ScaleTransform st = (ScaleTransform)btnBack.RenderTransform;
            st.ScaleX = 1.1;
            st.ScaleY = 1.1;
            btnBack.Margin = new Thickness(btnBack.Margin.Left - 2.5, btnBack.Margin.Top - 2.5, 0, 0);
        }

        private void btnBack_MouseLeave(object sender, MouseEventArgs e)
        {
            ScaleTransform st = (ScaleTransform)btnBack.RenderTransform;
            st.ScaleX = 1;
            st.ScaleY = 1;
            btnBack.Margin = new Thickness(btnBack.Margin.Left + 2.5, btnBack.Margin.Top + 2.5, 0, 0);
        }

        private void btnBack_Click(object sender, MouseButtonEventArgs e)
        {
            MainWindow.Instance.MainFrame.Navigate(parent);
        }

    }
}

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
            return json;
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
                    }
                    break;
                case 1: //PSEUDO
                    {
                        tbxValue.Width = 190;
                        tbxValue.Height = 25;
                        tbxValue.IsEnabled = true;
                    }
                    break;
                case 2: //MCHEADS
                    {
                        tbxValue.Width = 540;
                        tbxValue.Height = 90;
                        tbxValue.IsEnabled = true;
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

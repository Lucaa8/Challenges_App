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
    /// Interaction logic for LeatherArmor.xaml
    /// </summary>
    public partial class LeatherArmor : Page, IMeta
    {
        private Item parent;
        public LeatherArmor(Item parent)
        {
            this.parent = parent;
            InitializeComponent();
            lblTitle.Content = lblTitle.Content.ToString().Replace("[I]", this.parent.getName());
            cbxColor.SelectedIndex = 0;
        }

        public LeatherArmor(Item parent, JObject json) : this(parent)
        {
            if (json.ContainsKey("Color"))
            {
                object color = json.Value<object>("Color");
                if (color is JObject)
                {
                    JObject rgb = color as JObject;
                    sliderRed.Value = rgb.Value<Int32>("r");
                    sliderGreen.Value = rgb.Value<Int32>("g");
                    sliderBlue.Value = rgb.Value<Int32>("b");
                    ComboBoxItem? cbx = retrieveColorFromRGB(cbxColor.Items, (int)sliderRed.Value, (int)sliderGreen.Value, (int)sliderBlue.Value);
                    if (cbx != null)
                    {
                        cbxColor.SelectedItem = cbx;
                    }
                    cbCustom.IsChecked = true;
                    cbCustom_Click(cbCustom, null);
                }
                updateColor();
            }
        }

        IMeta.Meta IMeta.getMeta()
        {
            return IMeta.Meta.LeatherArmor;
        }

        void IMeta.navigate()
        {
            MainWindow.Instance.MainFrame.Navigate(this);
        }

        JObject IMeta.toJson()
        {
            JObject json = new JObject();
            if (isCustomChecked())
            {
                JObject jrgb = new JObject();
                jrgb["r"] = (byte)sliderRed.Value;
                jrgb["g"] = (byte)sliderGreen.Value;
                jrgb["b"] = (byte)sliderBlue.Value;
                json["Color"] = jrgb;
            }
            return json;
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

        private void Slider_Value(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider s = (Slider)sender;
            Label lbl = (Label)PageLeatherArmor.FindName(s.Tag.ToString());
            lbl.Content = lbl.Content.ToString().Split(":")[0] + ": " + s.Value;
            updateColor();
        }

        private void updateColor()
        {
            if (isCustomChecked())
            {
                rectColor.Fill = new SolidColorBrush(Color.FromRgb((byte)sliderRed.Value, (byte)sliderGreen.Value, (byte)sliderBlue.Value));
            }
            else
            {
                rectColor.Fill = new SolidColorBrush(Color.FromRgb(160, 101, 64));
            }
        }

        public static Brush getColorValues(ComboBoxItem cbx, out byte red, out byte green, out byte blue, out String hexValue)
        {
            byte r = 0, g = 0, b = 0;
            if (cbx != null && cbx.Tag!=null)
            {
                int rgb = Int32.Parse(cbx.Tag.ToString());
                r = (byte)((rgb & 0xFF0000) >> 16);
                g = (byte)((rgb & 0xFF00) >> 8);
                b = (byte)(rgb & 0xFF);
            }
            red = r;
            green = g;
            blue = b;
            hexValue = red.ToString("X") + green.ToString("X") + blue.ToString("X");
            return new SolidColorBrush(Color.FromRgb(r, g, b));
        }

        public static ComboBoxItem? retrieveColorFromRGB(ItemCollection elements, int r, int g, int b)
        {
            int rgb = (r << 16) | (g << 8) | b;
            foreach (Object element in elements)
            {
                if (element is ComboBoxItem)
                {
                    ComboBoxItem cbx = (ComboBoxItem)element;
                    if (cbx.Tag != null && cbx.Tag.Equals(rgb.ToString()))
                    {
                        return cbx;
                    }
                }
            }
            return null;
        }

        private void cbCustom_Click(object sender, RoutedEventArgs e)
        {
            if (isCustomChecked())
            {
                cbCustom.Content = "Oui";
            }
            else
            {
                cbCustom.Content = "Non";
            }
            updateColor();
        }

        private Boolean isCustomChecked()
        {
            return cbCustom.IsChecked.HasValue && cbCustom.IsChecked.Value;
        }

        private void cbxChanged(object sender, SelectionChangedEventArgs e)
        {
            byte r, g, b;
            getColorValues((ComboBoxItem)cbxColor.SelectedItem, out r, out g, out b, out _);
            sliderRed.Value = r;
            sliderGreen.Value = g;
            sliderBlue.Value = b;
            updateColor();
        }
    }
}

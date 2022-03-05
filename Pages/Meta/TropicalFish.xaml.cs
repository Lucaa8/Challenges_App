using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Challenges_App.Pages.Meta
{
    /// <summary>
    /// Interaction logic for TropicalFish.xaml
    /// </summary>
    public partial class TropicalFish : Page, IMeta
    {
        private Item parent;
        private List<Color> dyeColors = new List<Color>();
        public TropicalFish(Item parent)
        {
            this.parent= parent;
            InitializeComponent();
            lblTitle.Content = lblTitle.Content.ToString().Replace("[I]", this.parent.getName());
            Ressource.fillRect(rectFish1, Ressource.getImage(Files.ResxFile.Fish1));
            Ressource.fillRect(rectFish2, Ressource.getImage(Files.ResxFile.Fish2));
            dyeColors.Add(new Color("BLACK", "Noir", 1908001));
            dyeColors.Add(new Color("BLUE", "Bleu", 3949738));
            dyeColors.Add(new Color("BROWN", "Brun", 8606770));
            dyeColors.Add(new Color("CYAN", "Cyan", 1481884));
            dyeColors.Add(new Color("GRAY", "Gris", 4673362));
            dyeColors.Add(new Color("GREEN", "Vert", 6192150));
            dyeColors.Add(new Color("LIGHT_BLUE", "Bleu clair", 3847130));
            dyeColors.Add(new Color("LIGHT_GRAY", "Gris clair", 10329495));
            dyeColors.Add(new Color("LIME", "Lime", 8439583));
            dyeColors.Add(new Color("MAGENTA", "Magenta", 13061821));
            dyeColors.Add(new Color("ORANGE", "Orange", 16351261));
            dyeColors.Add(new Color("PINK", "Rose", 15961002));
            dyeColors.Add(new Color("PURPLE", "Violet", 8991416));
            dyeColors.Add(new Color("RED", "Rouge", 11546150));
            dyeColors.Add(new Color("WHITE", "Blanc", 16383998));
            dyeColors.Add(new Color("YELLOW", "Jaune", 16701501));
            foreach(Color c in dyeColors)
            {
                cbxBodyColor.Items.Add(c.getName());
                cbxPatternColor.Items.Add(c.getName());
            }
            cbxBodyColor.SelectedIndex = 0;
            cbxPatternColor.SelectedIndex = 0;
            cbxPattern.SelectedIndex = 0;
        }

        public TropicalFish(Item parent, JObject json) : this(parent)
        {
            if (json.ContainsKey("BodyColor"))
            {
                cbxBodyColor.SelectedItem = retrieveColorByID(json.Value<String>("BodyColor"));
            }
            if (json.ContainsKey("PatternColor"))
            {
                cbxPatternColor.SelectedItem = retrieveColorByID(json.Value<String>("PatternColor"));
            }
            if (json.ContainsKey("Pattern"))
            {
                String pattern = json.Value<String>("Pattern");
                cbxPattern.SelectedItem = pattern.Substring(0,1).ToUpper() + pattern.Substring(1).ToLower();
            }
        }

        IMeta.Meta IMeta.getMeta()
        {
            return IMeta.Meta.TropicalFish;
        }

        void IMeta.navigate()
        {
            MainWindow.Instance.MainFrame.Navigate(this);
        }

        JObject IMeta.toJson()
        {
            JObject json = new JObject();
            json["BodyColor"] = retrieveColor(cbxBodyColor.SelectedItem.ToString()).getID();
            json["PatternColor"] = retrieveColor(cbxPatternColor.SelectedItem.ToString()).getID();
            json["Pattern"] = ((ComboBoxItem)cbxPattern.SelectedItem).Content.ToString().ToUpper();
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

        private void cbxChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbx = (ComboBox)sender;
            ((Rectangle)PageTropicalFish.FindName(cbx.Tag.ToString())).Fill = retrieveColor(cbx.SelectedItem.ToString()).getColor();
        }

        private Color retrieveColor(String name)
        {
            foreach(Color c in dyeColors)
            {
                if (c.getName().Equals(name)) return c;
            }
            return new Color("BLACK", "Noir", 1908001);
        }

        private Color retrieveColorByID(String ID)
        {
            foreach (Color c in dyeColors)
            {
                if (c.getID().Equals(ID)) return c;
            }
            return new Color("BLACK", "Noir", 1908001);
        }

        private class Color
        {
            private String ID;
            private String name;
            private int value;
            public Color(String ID, String name, int value)
            {
                this.ID = ID;
                this.name = name;
                this.value = value;
            }
            public String getID()
            {
                return ID;
            }
            public String getName()
            {
                return name;
            }
            public Brush getColor()
            {
                return new SolidColorBrush(System.Windows.Media.Color.FromRgb((byte)((value & 0xFF0000) >> 16), (byte)((value & 0xFF00) >> 8), (byte)(value & 0xFF)));
            }
        }
    }
}

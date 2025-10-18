using Newtonsoft.Json.Linq;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Challenges_App.Controls
{
    /// <summary>
    /// Interaction logic for TrimArmorControl.xaml
    /// </summary>
    public partial class TrimArmorControl : UserControl
    {

        public static readonly DependencyProperty Spacing1Property = DependencyProperty.Register(nameof(Spacing1), typeof(double), typeof(TrimArmorControl), new PropertyMetadata(50.0));
        public static readonly DependencyProperty Spacing2Property = DependencyProperty.Register(nameof(Spacing2), typeof(double), typeof(TrimArmorControl), new PropertyMetadata(110.0));

        public double Spacing1 { get => (double)GetValue(Spacing1Property); set => SetValue(Spacing1Property, value); }
        public double Spacing2 { get => (double)GetValue(Spacing2Property); set => SetValue(Spacing2Property, value); }

        public enum ControlType
        {
            PATTERN, MATERIAL
        }

        private Action<ControlType, string>? cbxChanged;

        public TrimArmorControl()
        {
            InitializeComponent();
        }

        public void fromJSON(JObject j)
        {
            if(j.ContainsKey("Pattern") && j.ContainsKey("Material"))
            {
                cbActive.IsChecked = true;
                cbActive_Click(cbActive, null);
                string pattern = j.Value<string>("Pattern");
                foreach(ComboBoxItem item in cbxPattern.Items)
                {
                    if(item.Tag.ToString().ToLower().Equals(pattern))
                    {
                        cbxPattern.SelectedItem = item;
                        break;
                    }
                }
                string material = j.Value<string>("Material");
                foreach (ComboBoxItem item in cbxMaterial.Items)
                {
                    if (item.Tag.ToString().ToLower().Equals(material))
                    {
                        cbxMaterial.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        public JObject toJson()
        {
            JObject j = new JObject();
            if(isCustomChecked())
            {
                j["Pattern"] = ((ComboBoxItem)cbxPattern.SelectedItem).Tag.ToString().ToLower();
                j["Material"] = ((ComboBoxItem)cbxMaterial.SelectedItem).Tag.ToString().ToLower();
            }
            return j;
        }

        public string getCurrentValue(ControlType type)
        {
            switch (type)
            {
                case ControlType.PATTERN: return (string)((ComboBoxItem)cbxPattern.SelectedItem).Tag;
                case ControlType.MATERIAL: return (string)((ComboBoxItem)cbxMaterial.SelectedItem).Tag;
                default: return String.Empty;
            }
        }

        public void setSelectionChangedEvent(Action<ControlType, string> ev)
        {
            this.cbxChanged = ev;
        }

        private void cbxPattern_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cbxChanged?.Invoke(ControlType.PATTERN, getCurrentValue(ControlType.PATTERN));
        }

        private void cbxMaterial_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cbxChanged?.Invoke(ControlType.MATERIAL, getCurrentValue(ControlType.MATERIAL));
        }

        private void cbActive_Click(object sender, RoutedEventArgs e)
        {
            bool isChecked = isCustomChecked();
            if (isChecked)
            {
                cbActive.Content = "Oui";
            }
            else
            {
                cbActive.Content = "Non";
            }
            cbxMaterial.IsEnabled = isChecked;
            cbxPattern.IsEnabled = isChecked;
            cbxMaterial.SelectedIndex = isChecked ? 0 : -1;
            cbxPattern.SelectedIndex = isChecked ? 0 : -1;
        }

        private Boolean isCustomChecked()
        {
            return cbActive.IsChecked.HasValue && cbActive.IsChecked.Value;
        }

    }
}

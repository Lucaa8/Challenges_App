using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Challenges_App.Pages.Meta
{
    /// <summary>
    /// Interaction logic for Potion.xaml
    /// </summary>
    public partial class Potion : Page, IMeta
    {
        private Item parent;
        private Dictionary<String, String> effectsName = new Dictionary<String, String>();
        private List<PotionEffect> effects = new List<PotionEffect>();
        public delegate void PresetColorCallback(String? color);

        public Potion(Item parent)
        {
            this.parent = parent;
            InitializeComponent();
            lblTitle.Content = lblTitle.Content.ToString().Replace("[I]", this.parent.getName());
            effectsName.Add("ABSORPTION", "Absorption");
            effectsName.Add("BAD_OMEN", "Mauvais présage");
            effectsName.Add("BLINDNESS", "Cécité");
            effectsName.Add("CONDUIT_POWER", "Force de conduit");
            effectsName.Add("CONFUSION", "Nausée");
            effectsName.Add("DAMAGE_RESISTANCE", "Résistance");
            effectsName.Add("DOLPHINS_GRACE", "Grâce du dauphin");
            effectsName.Add("FAST_DIGGING", "Célérité");
            effectsName.Add("FIRE_RESISTANCE", "Résistance au feu");
            effectsName.Add("GLOWING", "Surbrillance");
            effectsName.Add("HARM", "Dégâts instantanés");
            effectsName.Add("HEAL", "Soin instantané");
            effectsName.Add("HEALTH_BOOST", "Bonus de vie");
            effectsName.Add("HERO_OF_THE_VILLAGE", "Héros du village");
            effectsName.Add("HUNGER", "Faim");
            effectsName.Add("INCREASE_DAMAGE", "Force");
            effectsName.Add("INVISIBILITY", "Invisibilité");
            effectsName.Add("JUMP", "Saut");
            effectsName.Add("LEVITATION", "Lévitation");
            effectsName.Add("LUCK", "Chance");
            effectsName.Add("NIGHT_VISION", "Nyctalopie");
            effectsName.Add("POISON", "Poison");
            effectsName.Add("REGENERATION", "Régénération");
            effectsName.Add("SATURATION", "Saturation");
            effectsName.Add("SLOW", "Lenteur");
            effectsName.Add("SLOW_DIGGING", "Fatigue");
            effectsName.Add("SLOW_FALLING", "Chute lente");
            effectsName.Add("SPEED", "Rapidité");
            effectsName.Add("UNLUCK", "Malchance");
            effectsName.Add("WATER_BREATHING", "Apnée");
            effectsName.Add("WEAKNESS", "Faiblesse");
            effectsName.Add("WITHER", "Wither");
            foreach(String s in effectsName.Values)
            {
                cbxEffect.Items.Add(s);
            }
            cbxEffect.SelectedIndex = 0;
            updateBox();
            sliderRed.Value = 248;
            sliderBlue.Value = 248;
            updateColor();
            btnAdd.addMouseClick(c =>
            {
                if (tbxName.Text.Length > 0)
                {
                    int duration;
                    int amplifier;
                    if(Int32.TryParse(tbxDuration.Text, out duration))
                    {
                        if(Int32.TryParse(tbxLevel.Text, out amplifier))
                        {
                            if (duration > 0 && duration < 10000)
                            {
                                if (amplifier > 0 && amplifier < 256)
                                {
                                    addEffect(tbxName.Text, effectsName.FirstOrDefault(x => x.Value == cbxEffect.SelectedItem.ToString()).Key, duration, amplifier, (cbAmbient.IsChecked.HasValue && cbAmbient.IsChecked.Value), (cbParticles.IsChecked.HasValue && cbParticles.IsChecked.Value));
                                }
                                else
                                {
                                    MessageBox.Show("Le niveau doit être contenu entre 0 et 256", "Error amplifier value");
                                }
                            }
                            else
                            {
                                MessageBox.Show("La durée(secondes) doit être contenue entre 0 et 9999", "Error duration value");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Le niveau doit être un réel entier", "Error amplifier not an int");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Le durée(secondes) doit être un réel entier", "Error duration not an int");
                    }
                }
                else
                {
                    MessageBox.Show("Le nom ne peut pas être vide.", "Error name is empty");
                }
            });
            btnRem.addMouseClick(c =>
            {
                if(boxSecondaryEffects.SelectedIndex != -1)
                {
                    remEffect(boxSecondaryEffects.SelectedItem.ToString());
                }
            });
            btnPresets.addMouseClick(c =>
            {
                promptColor((color) =>
                {
                    if (color != null)
                    {
                        int rgb = Int32.Parse(color);
                        sliderRed.Value = (byte)((rgb & 0xFF0000) >> 16);
                        sliderGreen.Value = (byte)((rgb & 0xFF00) >> 8);
                        sliderBlue.Value = (byte)(rgb & 0xFF);
                    }
                });
            });
        }

        public Potion(Item parent, JObject json) : this(parent)
        {
            if (json.ContainsKey("BaseEffect"))
            {
                JObject mainEffect = json["BaseEffect"] as JObject;
                foreach(ComboBoxItem item in cbxMainEffect.Items)
                {
                    if (item.Name.Equals(mainEffect.Value<String>("Type")))
                    {
                        cbxMainEffect.SelectedItem = item;
                    }
                }
                cbMainEffectExtended.IsChecked = mainEffect.ContainsKey("Extended") && mainEffect.Value<Boolean>("Extended");
                CbExtended_Click(cbMainEffectExtended, null);
                cbMainEffectUpgraded.IsChecked = mainEffect.ContainsKey("Upgraded") && mainEffect.Value<Boolean>("Upgraded");
                CbUpgraded_Click(cbMainEffectUpgraded, null);
            }
            if (json.ContainsKey("Color"))
            {
                object color = json.Value<object>("Color");
                if (color is JObject)
                {
                    JObject rgb = color as JObject;
                    sliderRed.Value = rgb.Value<Int32>("r");
                    sliderGreen.Value = rgb.Value<Int32>("g");
                    sliderBlue.Value = rgb.Value<Int32>("b");
                }
                updateColor();
            }
            else
            {
                cbDefColor.IsChecked = true;
                cbDefColor_Click(cbDefColor, null);
            }
            if (json.ContainsKey("CustomEffects"))
            {
                JArray jarr = json["CustomEffects"] as JArray;
                foreach(JObject jobj in jarr)
                {
                    PotionEffect newEffect = new PotionEffect(jobj, "empty");
                    newEffect.Name = Ressource.duplicateString(names(), newEffect.Type.ToLower());
                    effects.Add(newEffect);
                }
                updateBox();
            }
        }

        IMeta.Meta IMeta.getMeta()
        {
            return IMeta.Meta.Potion;
        }

        void IMeta.navigate()
        {
            MainWindow.Instance.MainFrame.Navigate(this);
        }

        JObject IMeta.toJson()
        {
            JObject json = new JObject();
            if(!isDefaultColorChecked())
            {
                JObject jrgb = new JObject();
                jrgb["r"] = (Int32)sliderRed.Value;
                jrgb["g"] = (Int32)sliderGreen.Value;
                jrgb["b"] = (Int32)sliderBlue.Value;
                json["Color"] = jrgb;
            }
            if (cbxMainEffect.SelectedIndex != -1)
            {
                JObject mainEffect = new JObject();
                mainEffect["Type"] = ((ComboBoxItem)cbxMainEffect.SelectedItem).Name;
                mainEffect["Extended"] = isExtendedChecked();
                mainEffect["Upgraded"] = isUpgradedChecked();
                json["BaseEffect"] = mainEffect;
            }
            if (effects.Count > 0)
            {
                JArray jarr = new JArray();
                foreach(PotionEffect effect in effects)
                {
                    jarr.Add(effect.toJson());
                }
                json["CustomEffects"] = jarr;
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

        private void addEffect(String name, String type, int duration, int amplifier, Boolean ambient, Boolean particles)
        {
            if (name.Equals("Nouvel effet...")) return;
            PotionEffect? effect = getEffect(name);
            if(effect != null)
            {
                if (MessageBox.Show("Un effet avec le même nom existe déjà.\nEn faisant ça tu vas écraser son contenu.\nContinuer?", "Error name already exists", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    effects.Remove(effect);
                }
                else return;
            }
            effect = new PotionEffect();
            effect.Name = name;
            effect.Type = type;
            effect.Duration = duration;
            effect.Amplifier = amplifier;
            effect.Ambient = ambient;
            effect.Particles = particles;
            effects.Add(effect);
            updateBox();
        }

        private Boolean remEffect(String name)
        {
            PotionEffect? effect = getEffect(name);
            if (effect!=null&&effects.Remove(effect))
            {
                updateBox();
                return true;
            }
            return false;
        }

        private void updateBox()
        {
            boxSecondaryEffects.Items.Clear();
            boxSecondaryEffects.Items.Add("Nouvel effet...");
            foreach (PotionEffect effect in effects)
            {
                boxSecondaryEffects.Items.Add(effect.Name);
            }
        }

        private void boxDbl_Click(object sender, MouseButtonEventArgs e)
        {
            if (boxSecondaryEffects.SelectedIndex == -1) return;
            if (boxSecondaryEffects.SelectedIndex == 0)
            {
                tbxName.Text = "";
                cbxEffect.SelectedIndex = 0;
                tbxDuration.Text = "";
                tbxLevel.Text = "";
                cbAmbient.IsChecked = false;
                cbParticles.IsChecked = false;
                cbAmbient.Content = "Non";
                cbParticles.Content = "Non";
            }
            else
            {
                String? selected = boxSecondaryEffects.SelectedItem as String;
                if (selected != null)
                {
                    PotionEffect? effect = getEffect((String)selected);
                    if (effect != null)
                    {
                        tbxName.Text = effect.Name;
                        cbxEffect.SelectedItem = effectsName[effect.Type];
                        tbxDuration.Text = effect.Duration + "";
                        tbxLevel.Text = effect.Amplifier + "";
                        cbAmbient.IsChecked = effect.Ambient;
                        cbParticles.IsChecked = effect.Particles;
                        CbValue_Click(cbAmbient, null);
                        CbValue_Click(cbParticles, null);
                    }
                }
            }
        }

        private PotionEffect? getEffect(String name)
        {
            foreach(PotionEffect effect in effects)
            {
                if (effect.Name.Equals(name)) return effect;
            }
            return null;
        }

        private List<String> names()
        {
            List<String> names = new List<String>();
            foreach(PotionEffect e in effects)
            {
                names.Add(e.Name);
            }
            return names;
        }

        private void NumericInput(object sender, TextCompositionEventArgs e)
        {
            int result = 0;
            if (!Int32.TryParse(e.Text, out result))
            {
                e.Handled = true;
            }
            else
            {
                if (((TextBox)sender).Text.Length > 3)
                {
                    e.Handled = true;
                }
            }
        }
        private void Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Deny paste
            if (e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        private void CbExtended_Click(object sender, RoutedEventArgs e)
        {
            if (isExtendedChecked())
            {
                cbMainEffectExtended.Content = "Longue";
            }
            else
            {
                cbMainEffectExtended.Content = "Courte";
            }
        }

        private void CbUpgraded_Click(object sender, RoutedEventArgs e)
        {
            if (isUpgradedChecked())
            {
                cbMainEffectUpgraded.Content = "Niveau II";
            }
            else
            {
                cbMainEffectUpgraded.Content = "Niveau I";
            }
        }

        private void cbxMainEffect_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (isDefaultColorChecked())
            {
                updateColor();
            }
        }

        private Boolean isExtendedChecked()
        {
            return cbMainEffectExtended.IsChecked.HasValue && cbMainEffectExtended.IsChecked.Value;
        }
        private Boolean isUpgradedChecked()
        {
            return cbMainEffectUpgraded.IsChecked.HasValue && cbMainEffectUpgraded.IsChecked.Value;
        }
        private Boolean isDefaultColorChecked()
        {
            return cbDefColor!=null && cbDefColor.IsChecked.HasValue && cbDefColor.IsChecked.Value;
        }

        private void cbDefColor_Click(object sender, RoutedEventArgs e)
        {
            Boolean enabled = !isDefaultColorChecked();
            btnPresets.IsEnabled = enabled;
            sliderRed.IsEnabled = enabled;
            sliderRed.Value = 0;
            sliderGreen.IsEnabled = enabled;
            sliderGreen.Value = 0;
            sliderBlue.IsEnabled = enabled;
            sliderBlue.Value = 0;
            updateColor();
        }

        private void updateColor()
        {
            byte r, g, b;
            if (isDefaultColorChecked())
            {
                int rgb = Int32.Parse(((ComboBoxItem)cbxMainEffect.SelectedItem).Tag.ToString());
                r = (byte)((rgb & 0xFF0000) >> 16);
                g = (byte)((rgb & 0xFF00) >> 8);
                b = (byte)(rgb & 0xFF);
            }
            else
            {
                r = (byte)sliderRed.Value;
                g = (byte)sliderGreen.Value;
                b = (byte)sliderBlue.Value;
            }
            rectColor.Fill = new SolidColorBrush(Color.FromRgb(r, g, b));
        }

        private void Slider_Value(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider s = (Slider)sender;
            Label lbl = (Label)PagePotion.FindName(s.Tag.ToString());
            lbl.Content = lbl.Content.ToString().Split(":")[0] + ": " + s.Value;
            updateColor();
        }

        private void CbValue_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            if(cb.IsChecked.HasValue && cb.IsChecked.Value)
            {
                cb.Content = "Oui";
            }
            else
            {
                cb.Content = "Non";
            }
        }

        public void promptColor(PresetColorCallback callback)
        {
            Style s = (Style)App.Current.FindResource("lblFont1");
            Window w = new Window();
            w.Uid = "promptColor";
            w.Width = 300;
            w.Height = 140;
            w.Left = (MainWindow.Instance.Left + MainWindow.Instance.Width / 2) - w.Width / 2;
            w.Top = (MainWindow.Instance.Top + MainWindow.Instance.Height / 2) - w.Height / 2;
            w.WindowStyle = WindowStyle.None;
            w.ResizeMode = ResizeMode.NoResize;
            w.BorderBrush = Brushes.Black;
            w.BorderThickness = new Thickness(2);
            Grid g = new Grid();
            g.Name = "promptColorGrid";
            g.Background = Brushes.DarkGray;
            w.Content = g;
            Label lblChoose = new Label();
            lblChoose.Name = "lblChoose";
            lblChoose.Content = "Choisir une couleur";
            lblChoose.FontSize = 25;
            lblChoose.Style = s;
            lblChoose.HorizontalAlignment = HorizontalAlignment.Center;
            lblChoose.VerticalAlignment = VerticalAlignment.Top;
            lblChoose.Margin = new Thickness(0, 10, 0, 0);
            ComboBox cbxColor = new ComboBox();
            cbxColor.Name = "cbxColor";
            cbxColor.SelectedIndex = 0;
            cbxColor.FontSize = 20;
            cbxColor.Style = s;
            cbxColor.Height = 30;
            cbxColor.MinWidth = 50;
            cbxColor.HorizontalAlignment = HorizontalAlignment.Center;
            cbxColor.VerticalAlignment = VerticalAlignment.Top;
            /*<ComboBox x:Name="cbxColor" Visibility="Hidden" HorizontalAlignment="Center" Margin="0,10,0,0" Style="{StaticResource lblFont1}" FontSize="20" VerticalAlignment="Top" Width="190" Height="28">*/
            cbxColor.Items.Add(getColorItem("Uncraftable", "16253176"));
            cbxColor.Items.Add(getColorItem("Eau", "3694022"));
            cbxColor.Items.Add(getColorItem("Résitance au feu", "14981690"));
            cbxColor.Items.Add(getColorItem("Dégat", "4393481"));
            cbxColor.Items.Add(getColorItem("Soin", "16262179"));
            cbxColor.Items.Add(getColorItem("Invisibilité", "8356754"));
            cbxColor.Items.Add(getColorItem("Saut", "2293580"));
            cbxColor.Items.Add(getColorItem("Chance", "3381504"));
            cbxColor.Items.Add(getColorItem("Vision nocture", "2039713"));
            cbxColor.Items.Add(getColorItem("Poison", "5149489"));
            cbxColor.Items.Add(getColorItem("Régénération", "13458603"));
            cbxColor.Items.Add(getColorItem("Chute lente", "16773073"));
            cbxColor.Items.Add(getColorItem("Lenteur", "5926017"));
            cbxColor.Items.Add(getColorItem("Rapidité", "8171462"));
            cbxColor.Items.Add(getColorItem("Force", "9643043"));
            cbxColor.Items.Add(getColorItem("Maître Tortue", "7691106"));
            cbxColor.Items.Add(getColorItem("Respiration aquatique", "3035801"));
            cbxColor.Items.Add(getColorItem("Faiblesse", "4738376"));
            double width, heigth;
            Ressource.size(lblChoose, out width, out heigth);
            cbxColor.Margin = new Thickness(0, lblChoose.Margin.Top + heigth + 5, 0, 0);
            Controls.ButtonControl btnSave = new Controls.ButtonControl();
            btnSave.Name = "btnSave";
            btnSave.Text = "Confirmer";
            btnSave.HorizontalAlignment = HorizontalAlignment.Center;
            btnSave.VerticalAlignment = VerticalAlignment.Bottom;
            btnSave.BorderColor = Brushes.Black;
            btnSave.BorderSize = 2;
            btnSave.Margin = new Thickness(0, 0, 0, 10);
            btnSave.addMouseClick(c =>
            {
                if (cbxColor.SelectedIndex == -1)
                {
                    callback(null);
                }
                else
                {
                    callback((string?)((ComboBoxItem)cbxColor.SelectedItem).Tag);
                }
                w.Close();
            });
            w.KeyUp += (s, e) =>
            {
                if (e != null)
                {
                    if (e.Key == Key.Escape)
                    {
                        callback(null);
                        w.Close();
                    }
                }
            };
            g.Children.Add(lblChoose);
            g.Children.Add(cbxColor);
            g.Children.Add(btnSave);
            w.Show();
        }

        //Only for promptColor
        private ComboBoxItem getColorItem(String name, String color)
        {
            ComboBoxItem cbx = new ComboBoxItem();
            cbx.Content = name;
            cbx.Tag = color;
            return cbx;
        }

        private class PotionEffect
        {
            public String Name { set; get; } = "Vitesse";
            public String Type { set; get; } = "SPEED";
            public int Duration { set; get; } = 30;
            public int Amplifier { set; get; } = 0;
            public Boolean Ambient { set; get; } = false;
            public Boolean Particles { set; get; } = false;

            public PotionEffect() { }
            public PotionEffect(JObject json, String name)
            {
                Name = name;
                Type = json.Value<String>("Type");
                Duration = (Int32)(json.Value<Int32>("Duration") / 20);
                Amplifier = json.Value<Int32>("Amplifier");
                if (json.ContainsKey("Ambient"))
                {
                    Ambient = json.Value<Boolean>("Ambient");
                }
                if (json.ContainsKey("Particles"))
                {
                    Particles = json.Value<Boolean>("Particles");
                }
            }

            public JObject toJson()
            {
                JObject json = new JObject();
                json["Type"] = Type;
                json["Duration"] = (Int32)(Duration * 20);
                json["Amplifier"] = Amplifier;
                if (Ambient)
                {
                    json["Ambient"] = true;
                }
                if (Particles)
                {
                    json["Particles"] = true;
                }
                return json;
            }
        }
    }
}

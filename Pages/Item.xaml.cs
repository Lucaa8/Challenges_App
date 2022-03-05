using Challenges_App.Pages.Meta;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Newtonsoft.Json.Linq;
using Microsoft.Win32;
using System.IO;
using System.Text;

namespace Challenges_App.Pages
{
    /// <summary>
    /// Interaction logic for Item.xaml
    /// </summary>
    public partial class Item : Page
    {
        private String uName;
        private Dictionary<String, int> enchants = new Dictionary<String, int>();
        private Attribute attributes;
        private IMeta? itemmeta;
        public delegate void MaterialCallback(String? material);

        public String getName()
        {
            return uName;
        }
        public Item(String itemname)
        {
            this.uName = itemname;
            InitializeComponent();
            lblTitle.Content = lblTitle.Content.ToString().Replace("[I]", uName);
            Ressource.fillRect(btnSave, Ressource.getImage(Files.ResxFile.SaveButton));
            btnEnchAdd.addMouseClick((c) =>
            {
                if(cbxEnchName.SelectedIndex != -1 && cbxEnchName.SelectedItem is ListBoxItem)
                {
                    if(tbxEnchLevel.Text.Length > 0)
                    {
                        int lvl = -1;
                        Boolean isInt = Int32.TryParse(tbxEnchLevel.Text, out lvl);
                        if (isInt)
                        {
                            String? ench = (cbxEnchName.SelectedItem as ListBoxItem).Content.ToString();
                            if (!addEnchant(ench, lvl))
                            {
                                MessageBox.Show("L'item contient déjà l'enchantement " + ench + "!", "Error enchant already exists");
                            }
                            else
                            {
                                updateEnchants();
                                tbxEnchLevel.Text = "1";
                                cbxEnchName.SelectedIndex = -1;
                                boxEnchants.Focus();
                            }
                        }
                        else
                        {
                            MessageBox.Show("La valeur spécifiée du niveau n'est pas un entier! (" + tbxEnchLevel.Text + ")", "Error wrong level type");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Aucun niveau n'a été spécifié.", "Error no value");
                    }
                }
                else
                {
                    MessageBox.Show("Aucun enchantement n'a été spécifié.", "Error no enchant");
                }
            });
            btnEnchRem.addMouseClick((c) =>
            {

                if(boxEnchants.SelectedIndex != -1)
                {
                    ListBoxItem item = boxEnchants.SelectedItem as ListBoxItem;
                    if (!enchants.Remove(item.Content.ToString()))
                    {
                        MessageBox.Show("L'enchantement n'a pas pû être retiré.", "Error can't remove");
                    }
                    else
                    {
                        updateEnchants();
                    }
                }
                else
                {
                    MessageBox.Show("Tu n'as séléctionné aucun enchantement.", "Error no enchant");
                }
            });
            btnAttributes.addMouseClick(c =>
            {
                if (attributes == null)
                {
                    attributes = new Attribute(this);
                }
                MainWindow.Instance.MainFrame.Navigate(attributes);
            });
            btnMeta.addMouseClick(c =>
            {
                IMeta.Meta m = IMeta.forItem(tbxMaterial.Text);
                if (m != IMeta.Meta.None)
                {
                    if (itemmeta == null)
                    {
                        itemmeta = IMeta.loadMeta(this, m, null);
                        if (itemmeta != null) itemmeta.navigate();
                    }
                    else
                    {
                        if (m != itemmeta.getMeta())
                        {
                            MessageBoxResult reset = MessageBox.Show("Une meta existe déjà pour cet item mais elle ne correspond plus au présent materiel.\n" +
                                "Meta actuelle: " + itemmeta.getMeta().ToString() + "\n" +
                                "Meta requise: " + m.ToString() + "\n" +
                                "Écraser l'ancienne meta ? Cette action est irréversible.", "Wrong meta type", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                            if (reset==MessageBoxResult.Yes)
                            {
                                itemmeta = IMeta.loadMeta(this, m, null);
                                if (itemmeta != null) itemmeta.navigate();
                            }
                        }
                        else
                        {
                            itemmeta.navigate();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Cet item ne contient pas de meta personalisée.", "Error no custom meta");
                }
            });
        }

        public Item(JObject json) : this("")
        {
            if (json.ContainsKey("PrivateName"))
            {
                uName = json.Value<String>("PrivateName");
                lblTitle.Content = "Item : " + uName;
            }
            if (json.ContainsKey("Id"))
            {
                tbxID.Text = json.Value<String>("Id");
            }
            if (json.ContainsKey("Material"))
            {
                tbxMaterial.Text = json.Value<String>("Material");
            }
            if (json.ContainsKey("Name"))
            {
                tbxName.Text = json.Value<String>("Name");
            }
            if (json.ContainsKey("Lore"))
            {
                JArray jarr = json["Lore"] as JArray;
                String lore = "";
                foreach (String line in jarr)
                {
                    lore += line+"\n";
                }
                new TextRange(rtbLore.Document.ContentStart, rtbLore.Document.ContentEnd).Text = lore.Substring(0,lore.Length-1);
            }
            if (json.ContainsKey("Enchants"))
            {
                JArray jarr = json["Enchants"] as JArray;
                foreach(JObject enchant in jarr)
                {
                    addEnchant(fromMinecraft(enchant.Value<String>("Key")), Int32.Parse(enchant.Value<String>("Level")));
                }
                updateEnchants();
            }
            if (json.ContainsKey("Flags"))
            {
                JArray jarr = json["Flags"] as JArray;
                foreach(String flag in jarr)
                {
                    foreach(ListBoxItem item in boxFlags.Items)
                    {
                        if (item.Tag.ToString().Equals(flag))
                        {
                            boxFlags.SelectedItem = item;
                            boxFlags_DoubleClick(boxFlags, null);
                        }
                    }
                }
                boxFlags.SelectedIndex = -1;
            }
            if (json.ContainsKey("Attributes"))
            {
                attributes = new Attribute(this, json["Attributes"] as JArray);
            }
            if (json.ContainsKey("ItemMeta"))
            {
                JObject j = json["ItemMeta"] as JObject;
                itemmeta = IMeta.loadMeta(this, Enum.Parse<IMeta.Meta>(j.Value<String>("MetaType"), false), j["Meta"] as JObject);
            }
            if (json.ContainsKey("RepairCost"))
            {
                tbxRepairCost.Text = "" + json.Value<Int32>("RepairCost");
            }
            if (json.ContainsKey("CustomData"))
            {
                tbxCustomData.Text = "" + json.Value<Int32>("CustomData");
            }
            if (json.ContainsKey("Durability"))
            {
                tbxDamage.Text = "" + json.Value<Int32>("Durability");
            }
            if (json.ContainsKey("Invulnerable"))
            {
                cbInvulnerable.IsChecked = json.Value<Boolean>("Invulnerable");
                cbInvulnerable_Click(cbInvulnerable, null);
            }
        }

        public JObject toJson()
        {
            JObject json = new JObject();
            if (uName != null && uName.Length > 0)
            {
                json["PrivateName"] = uName;
            }
            if (tbxID.Text.Length > 0)
            {
                json["Id"] = tbxID.Text;
            }
            if (tbxMaterial.Text.Length > 0 && Ressource.materials.ContainsKey(tbxMaterial.Text))
            {
                json["Material"] = tbxMaterial.Text;
            }
            if (tbxName.Text.Length > 0)
            {
                json["Name"] = tbxName.Text;
            }
            if (getLore().Length > 0)
            {
                JArray lore = new JArray();
                foreach (String s in getLore().Split('\n'))
                {
                    lore.Add(s);
                }
                if (lore.Count >= 1)
                {
                    json["Lore"] = lore;
                }
            }
            if (enchants.Count > 0)
            {
                JArray jarr = new JArray();
                foreach(String ench in enchants.Keys)
                {
                    String? namespacekey = minecraft(ench);
                    if(namespacekey != null)
                    {
                        JObject enchant = new JObject();
                        enchant["Key"] = namespacekey;
                        enchant["Level"] = enchants[ench];
                        jarr.Add(enchant);
                    }
                }
                if (jarr.Count > 0)
                {
                    json["Enchants"] = jarr;
                }
            }
            if (getFlags().Count > 0)
            {
                JArray jarr = new JArray();
                foreach (ListBoxItem f in getFlags())
                {
                    jarr.Add(f.Tag.ToString());
                }
                json["Flags"] = jarr;
            }
            if (attributes != null)
            {
                json["Attributes"] = attributes.toJson();
            }
            if(itemmeta != null)
            {
                JObject j = new JObject();
                j["Meta"] = itemmeta.toJson();
                j["MetaType"] = itemmeta.getMeta().ToString();
                json["ItemMeta"] = j;
            }
            if (tbxRepairCost.Text.Length > 0)
            {
                int value = Ressource.getInt(tbxRepairCost.Text, -1);
                if (value >= 0)
                {
                    json["RepairCost"] = value;
                }
            }
            if (tbxCustomData.Text.Length > 0)
            {
                int value = Ressource.getInt(tbxCustomData.Text, -1);
                if (value >= 0)
                {
                    json["CustomData"] = value;
                }
            }
            if (tbxDamage.Text.Length > 0)
            {
                int value = Ressource.getInt(tbxDamage.Text, -1);
                if (value >= 0)
                {
                    json["Durability"] = value;
                }
            }
            json["Invulnerable"] = cbInvulnerable.IsChecked.HasValue && cbInvulnerable.IsChecked.Value;
            return json;
        }

        public static Item fromFile(String path)
        {
            return new Item(JObject.Parse(Encoding.UTF8.GetString(File.ReadAllBytes(path))));
        }

        public String getLore()
        {
            TextRange tr = new TextRange(rtbLore.Document.ContentStart, rtbLore.Document.ContentEnd);
            String lore = tr.Text.Replace("\r\n", "\n");
            return lore.Substring(0,lore.Length-1); //remove the auto added \n by richtextboxes
        }

        public String getMaterial()
        {
            return tbxMaterial.Text;
        }

        public List<ListBoxItem> getFlags()
        {
            List<ListBoxItem> flags = new List<ListBoxItem>();
            foreach(ListBoxItem item in boxFlags.Items)
            {
                String? flag = item.Content.ToString();
                if (flag != null && flag.Length > 0)
                {
                    if (flag.StartsWith("[x]"))
                    {
                        flags.Add(item);
                    }
                }
            }
            return flags;
        }

        private String? minecraft(String key)
        {
            foreach(ComboBoxItem i in cbxEnchName.Items)
            {
                if (i.Content.Equals(key)) return i.Tag.ToString();
            }
            return null;
        }

        private String? fromMinecraft(String key)
        {
            foreach (ComboBoxItem i in cbxEnchName.Items)
            {
                if (i.Tag.ToString().Equals(key)) return i.Content.ToString();
            }
            return null;
        }

        private Boolean addEnchant(String enchant, int lvl)
        {
            if (enchant == null) return false;
            if (!enchants.ContainsKey(enchant))
            {
                enchants.Add(enchant, lvl);
                return true;
            }
            return false;
        }

        private void updateEnchants()
        {
            boxEnchants.Items.Clear();
            foreach(String enchant in enchants.Keys)
            {
                ListBoxItem item = new ListBoxItem();
                item.Content = enchant;
                item.ToolTip = "Niveau " + enchants[enchant];
                item.SetValue(ToolTipService.InitialShowDelayProperty, 0);
                item.SetValue(ToolTipService.BetweenShowDelayProperty, 0);
                boxEnchants.Items.Add(item);
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
            MainWindow.Instance.MainFrame.Navigate(MainWindow.Instance.menu.getItemManager());
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

        private void cbInvulnerable_Click(object sender, RoutedEventArgs e)
        {
            if (cbInvulnerable.IsChecked.HasValue)
            {
                if (cbInvulnerable.IsChecked.Value)
                {
                    cbInvulnerable.Content = "Oui";
                }
                else
                {
                    cbInvulnerable.Content = "Non";
                }
            }
        }

        private void boxFlags_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            object obj = boxFlags.SelectedItem;
            if (obj!=null&&obj is ListBoxItem)
            {
                ListBoxItem flagItem = obj as ListBoxItem;
                String flag = flagItem.Content.ToString();
                if (flag.StartsWith("[]"))
                {
                    flagItem.Content = "[x] " + flag.Substring(flag.IndexOf(' ')+1);
                }
                else
                {
                    flagItem.Content = "[] " + flag.Substring(flag.IndexOf(' ')+1);
                }
            }
        }

        private void btnSave_Enter(object sender, MouseEventArgs e)
        {
            btnSave.Width += 5;
            btnSave.Height += 5;
            btnSave.Margin = new Thickness(btnSave.Margin.Left-2.5, btnSave.Margin.Top-2.5, 0, 0);
        }

        private void btnSave_Leave(object sender, MouseEventArgs e)
        {
            btnSave.Width -= 5;
            btnSave.Height -= 5;
            btnSave.Margin = new Thickness(btnSave.Margin.Left + 2.5, btnSave.Margin.Top + 2.5, 0, 0);
        }

        private void btnSave_Click(object sender, MouseButtonEventArgs e)
        {
            SaveFileDialog fc = new SaveFileDialog();
            fc.OverwritePrompt = false;
            fc.Filter = "JSON Files (*.json)|*.json";
            fc.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            fc.FileName = uName+".json";
            if (fc.ShowDialog().Value)
            {
                if (File.Exists(fc.FileName))
                {
                    if (MessageBox.Show("Ce fichier existe déjà.\nSon contenu sera écrasé pour y mettre l'item actuel.\nConfirmer?", "Error file already exists", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        File.Delete(fc.FileName);
                    }
                    else return;
                }
                using (Stream s = File.OpenWrite(fc.FileName))
                {
                    byte[] item = Encoding.UTF8.GetBytes(toJson().ToString());
                    s.Write(item, 0, item.Length);
                }
            }
        }

        private void onMaterialClick(object sender, MouseButtonEventArgs e)
        {
            promptMaterial((mat) =>
            {
                if(mat != null)
                {
                    tbxMaterial.Text = mat.ToString();
                }
            });
        }

        public void promptMaterial(MaterialCallback callback)
        {
            Style s = (Style)App.Current.FindResource("lblFont1");
            Window w = new Window();
            w.Uid = "promptMaterial";
            w.Width = 300;
            w.Height = 170;
            w.Left = (MainWindow.Instance.Left+MainWindow.Instance.Width/2)-w.Width/2;
            w.Top = (MainWindow.Instance.Top+MainWindow.Instance.Height/2) - w.Height/2;
            w.WindowStyle = WindowStyle.None;
            w.ResizeMode = ResizeMode.NoResize;
            w.BorderBrush = Brushes.Black;
            w.BorderThickness = new Thickness(2);
            Grid g = new Grid();
            g.Name = "promptMaterialGrid";
            g.Background = Brushes.DarkGray;
            w.Content = g;
            Label lblChoose = new Label();
            lblChoose.Name = "lblChoose";
            lblChoose.Content = "Choisir un matériau";
            lblChoose.FontSize = 25;
            lblChoose.Style = s;
            lblChoose.HorizontalAlignment = HorizontalAlignment.Center;
            lblChoose.VerticalAlignment = VerticalAlignment.Top;
            lblChoose.Margin = new Thickness(0,10,0,0);
            ComboBox cbxMaterial = new ComboBox();
            cbxMaterial.Name = "cbxMaterial";
            cbxMaterial.SelectedIndex = 0;
            cbxMaterial.FontSize = 20;
            cbxMaterial.Style = s;
            cbxMaterial.Height = 30;
            cbxMaterial.MinWidth = 50;
            cbxMaterial.HorizontalAlignment = HorizontalAlignment.Center;
            cbxMaterial.VerticalAlignment = VerticalAlignment.Top;
            double width, heigth;
            Ressource.size(lblChoose, out width, out heigth);
            cbxMaterial.Margin = new Thickness(0, lblChoose.Margin.Top+heigth+5, 0, 0);
            TextBox tbxSearch = new TextBox();
            tbxSearch.Name = "tbxSearch";
            tbxSearch.Text = "Rechercher...";
            tbxSearch.FontSize = 25;
            tbxSearch.Style = s;
            tbxSearch.Foreground = Brushes.DarkSlateGray;
            tbxSearch.HorizontalAlignment = HorizontalAlignment.Center;
            tbxSearch.VerticalAlignment = VerticalAlignment.Top;
            tbxSearch.HorizontalContentAlignment = HorizontalAlignment.Center;
            tbxSearch.MinWidth = 50;
            tbxSearch.MaxWidth = 270;
            tbxSearch.Margin = new Thickness(0, cbxMaterial.Margin.Top + cbxMaterial.Height + 10, 0, 0);
            tbxSearch.Padding = new Thickness(7,0,7,0); //add little space before and after text
            tbxSearch.GotFocus += (s, e) =>
            {
                if (tbxSearch.Text.Equals("Rechercher...") && tbxSearch.Foreground == Brushes.DarkSlateGray)
                {
                    tbxSearch.Text = "";
                    tbxSearch.Foreground = Brushes.Black;
                }
            };
            tbxSearch.LostFocus += (s, e) =>
            {
                if (tbxSearch.Text.Length == 0)
                {
                    tbxSearch.Text = "Rechercher...";
                    tbxSearch.Foreground = Brushes.DarkSlateGray;
                }
            };
            tbxSearch.TextChanged += (s, e) =>
            {
                cbxMaterial.Items.Clear();
                foreach (KeyValuePair<String, String> materials in Ressource.materials)
                {
                    if (materials.Value.ToLower().Contains(tbxSearch.Text.ToLower()) || tbxSearch.Text.Equals("Rechercher..."))
                    {
                        ComboBoxItem cbxItem = new ComboBoxItem();
                        cbxItem.Name = materials.Key;
                        cbxItem.Content = materials.Value;
                        cbxMaterial.Items.Add(cbxItem);
                    }
                }
                if(cbxMaterial.Items.Count > 0)
                {
                    cbxMaterial.SelectedIndex = 0;
                }
            };
            tbxSearch.RaiseEvent(new TextChangedEventArgs(TextBox.TextChangedEvent, UndoAction.None)); //to add materials the first time
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
                if(cbxMaterial.SelectedIndex == -1)
                {
                    callback(null);
                }
                else
                {
                    callback(((ComboBoxItem)cbxMaterial.SelectedItem).Name);
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
            g.Children.Add(cbxMaterial);
            g.Children.Add(tbxSearch);
            g.Children.Add(btnSave);
            w.Show();
        }
    }
}

using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Challenges_App.Pages
{
    /// <summary>
    /// Interaction logic for CategorySession.xaml
    /// </summary>
    public partial class CategorySession : Page
    {
        private List<String> selected = new List<String>();
        private Boolean isOnReloadBtn = false;
        public CategorySession(String uuid)
        {
            InitializeComponent();
            tbxResearch_TextChanged(tbxResearch, null);
            Ressource.fillRect(rectReload, Ressource.getImage(Files.ResxFile.RlButton));
            Ressource.fillRect(btnSave, Ressource.getImage(Files.ResxFile.SaveButton));
            lblTitle.Tag = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            tbxUUID.Text = uuid;
            tbxName.Text = "new";
            updateCbxItems();
            if (cbxIcon.Items.Count > 0)
            {
                cbxIcon.SelectedIndex = 0;
            }
            btnPush.addMouseClick((c) =>
            {
                MainMenu? menu = MainWindow.Instance.menu;
                if (menu != null)
                {
                    if (!menu.ChallengesState)
                    {
                        push(menu);
                    }
                    else
                    {
                        if (MessageBox.Show("Les challenges ne sont pas désactivés!\nDésactiver les challenges et effectuer le push?", "Error challenges enabled", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                        {
                            btnPush.Animation = true;
                            btnPush.Enabled = false;
                            menu.toggleChallengesState((state) =>
                            {
                                btnPush.Animation = false;
                                btnPush.Enabled = true;
                                if (!state)
                                {
                                    push(menu);
                                }
                                else
                                {
                                    MessageBox.Show("Il semble y avoir un problème lorsque le client veut désactiver les challenges.\nLe push n'a pas fonctionné.", "Error can't send category");
                                }
                            });
                        }
                    }
                }
            });
        }

        public CategorySession(JObject json) : this("")
        {
            if (json.ContainsKey("uuid"))
            {
                tbxUUID.Text = json.Value<String>("uuid");
            }
            if (json.ContainsKey("name"))
            {
                tbxName.Text = json.Value<String>("name");
            }
            if (json.ContainsKey("description"))
            {
                tbxDescription.Text = json.Value<String>("description").Replace("\n", "\\n");
            }
            cbActive.IsChecked = json.ContainsKey("active")&&json.Value<Boolean>("active");
            cbActive_Click(cbActive, null);
            if (json.ContainsKey("color"))
            {
                object c = PageCategorySession.FindName(json.Value<String>("color"));
                if(c!=null&&c is ComboBoxItem)
                {
                    cbxColor.SelectedItem = (ComboBoxItem)c;
                }
            }
            if (json.ContainsKey("icon"))
            {
                JObject item = json["icon"] as JObject;
                if (item != null)
                {
                    Item i = new Item(item);
                    MainWindow.Instance.menu.getItemManager().forceAdd(i);
                    updateCbxItems();
                    cbxIcon.SelectedItem = i.getName();
                }
            }
            if (json.ContainsKey("page"))
            {
                tbxPage.Text = json.Value<Int32>("page")+"";
            }
            if (json.ContainsKey("slot"))
            {
                tbxSlot.Text = json.Value<Int32>("slot") + "";
            }
            if (json.ContainsKey("lastEdited"))
            {
                lblTitle.Tag = json.Value<long>("lastEdited");
            }
            if (json.ContainsKey("requiredChallenges"))
            {
                JArray jarr = json["requiredChallenges"] as JArray;
                MainMenu menu = MainWindow.Instance.menu;
                foreach (String uuid in jarr)
                {
                    if (menu.getChallenge(uuid) != null)
                    {
                        selected.Add(uuid);
                    }
                }
                tbxResearch_TextChanged(tbxResearch, null);
            }
        }

        public static CategorySession fromFile(String path)
        {
            return new CategorySession(JObject.Parse(Encoding.UTF8.GetString(File.ReadAllBytes(path))));
        }

        public JObject toJson()
        {
            JObject json = new JObject();
            json["uuid"] = tbxUUID.Text;
            if (tbxName.Text.Length > 0)
            {
                json["name"] = tbxName.Text;
            }
            if (tbxDescription.Text.Length > 0)
            {
                json["description"] = tbxDescription.Text.Replace("\\n", "\n");
            }
            if (cbActive.IsChecked.HasValue && cbActive.IsChecked.Value)
            {
                json["active"] = true;
            }
            json["color"] = ((ComboBoxItem)cbxColor.SelectedItem).Name;
            if (cbxIcon.SelectedIndex != -1)
            {
                String sItem = cbxIcon.SelectedItem.ToString();
                Item? item = MainWindow.Instance.menu.getItemManager().getItem(sItem);
                if (item != null)
                {
                    json["icon"] = item.toJson();
                }
            }
            if (tbxPage.Text.Length > 0)
            {
                int value = Ressource.getInt(tbxPage.Text, -1);
                if (value >= 0)
                {
                    json["page"] = value;
                }
            }
            if (tbxSlot.Text.Length > 0)
            {
                int value = Ressource.getInt(tbxSlot.Text, -1);
                if (value >= 0)
                {
                    json["slot"] = value;
                }
            }
            json["lastEdited"] = long.Parse(lblTitle.Tag.ToString());
            if (selected.Count > 0)
            {
                JArray jarr = new JArray();
                MainMenu menu = MainWindow.Instance.menu;
                foreach (String s in selected)
                {
                    if (menu.getChallenges().ContainsKey(s))
                    {
                        jarr.Add(s);
                    }
                }
                json["requiredChallenges"] = jarr;
            }
            return json;
        }

        public String getName()
        {
            return tbxName.Text;
        }

        public String getUUID()
        {
            return tbxUUID.Text;
        }

        private void push(MainMenu menu)
        {
            JObject j = toJson();
            if (j != null && j.Count > 0)
            {
                if (menu.send(new Packet.Packets.UpdateRequestPacket(tbxUUID.Text, Packet.IPacket.Target.CATEGORY, j)))
                {
                    menu.getLogger().addText(LogManager.LogType.INFO, "La catégorie " + tbxName.Text + " a bien été envoyée au serveur.");
                }
                else
                {
                    MessageBox.Show("Une erreur est survenue lors de la tentative d'envoi.\nLa session est-elle en active et valide?", "Error session unavailable");
                }
            }
        }

        private void tbxResearch_GotFocus(object sender, RoutedEventArgs e)
        {
            if (tbxResearch.Text.Equals("Rechercher...") && tbxResearch.Foreground == Brushes.DarkSlateGray)
            {
                tbxResearch.Text = "";
                tbxResearch.Foreground = Brushes.Black;
            }
        }

        private void tbxResearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (tbxResearch.Text.Length == 0)
            {
                tbxResearch.Text = "Rechercher...";
                tbxResearch.Foreground = Brushes.DarkSlateGray;
            }
        }

        private void tbxResearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            boxReqChallenges.Items.Clear();
            foreach (KeyValuePair<String, String> s in MainWindow.Instance.menu.getChallenges())
            {
                if (s.Value.ToLower().StartsWith(tbxResearch.Text.ToLower()) || tbxResearch.Text.Equals("Rechercher..."))
                {
                    ListBoxItem lb = new ListBoxItem();
                    lb.Tag = s.Key;
                    if (selected.Contains(s.Key))
                    {
                        lb.Content = "[x] " + s.Value;
                    }
                    else
                    {
                        lb.Content = "[] " + s.Value;
                    }
                    boxReqChallenges.Items.Add(lb);
                }
            }
        }

        private void boxDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (boxReqChallenges.SelectedIndex == -1) return;
            ListBoxItem select = (ListBoxItem)boxReqChallenges.SelectedItem;
            String uuid = select.Tag.ToString();
            if (selected.Contains(uuid))
            {
                selected.Remove(uuid);
            }
            else
            {
                selected.Add(uuid);
            }
            tbxResearch_TextChanged(tbxResearch, null);
        }

        private void btnBack_MouseEnter(object sender, MouseEventArgs e)
        {
            ScaleTransform st = (ScaleTransform)btnBack.RenderTransform;
            st.ScaleX = 1.1;
            st.ScaleY = 1.1;
            btnBack.Margin = new Thickness(btnBack.Margin.Left-2.5, btnBack.Margin.Top-2.5, 0, 0);
        }

        private void btnBack_MouseLeave(object sender, MouseEventArgs e)
        {
            ScaleTransform st = (ScaleTransform)btnBack.RenderTransform;
            st.ScaleX = 1;
            st.ScaleY = 1;
            btnBack.Margin = new Thickness(btnBack.Margin.Left + 2.5, btnBack.Margin.Top+2.5, 0, 0);
        }

        private void btnBack_Click(object sender, MouseButtonEventArgs e)
        {
            MainWindow.Instance.MainFrame.Navigate(MainWindow.Instance.menu);
        }

        private void NumericInput(object sender, TextCompositionEventArgs e)
        {
            int result = 0;
            if(!Int32.TryParse(e.Text, out result))
            {
                e.Handled = true;
            }
            else
            {
                if (((TextBox)sender).Text.Length > 2)
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

        private void cbActive_Click(object sender, RoutedEventArgs e)
        {
            if (cbActive.IsChecked.HasValue)
            {
                if (cbActive.IsChecked.Value)
                {
                    cbActive.Content = "Oui";
                }
                else
                {
                    cbActive.Content = "Non";
                }
            }
        }

        private void Name_Changed(object sender, TextChangedEventArgs e)
        {
            lblTitle.Content = "Catégorie : " + (tbxName.Text.Length == 0 ? "null" : tbxName.Text);
            MainWindow.Instance.menu.changeCategoryName(getUUID(), tbxName.Text);
        }

        private void updateCbxItems()
        {
            cbxIcon.Items.Clear();
            ItemsManager m = MainWindow.Instance.menu.getItemManager();
            foreach (String item in m.getItems())
            {
                cbxIcon.Items.Add(item);
            }
            if (cbxIcon.Items.Count > 0)
            {
                cbxIcon.SelectedIndex = 0;
            }
        }

        private async void rotate()
        {
            while (isOnReloadBtn)
            {
                double angle = rectReloadRotate.Angle;
                if (angle <= 0)
                {
                    angle = 360;
                }
                else
                {
                    angle -= 4;
                }
                Ressource.synchronize(() => rectReloadRotate.Angle = angle);
                await Task.Delay(1);
            }
            rectReloadRotate.Angle = 360;
        }

        private void rectReload_Enter(object sender, MouseEventArgs e)
        {
            isOnReloadBtn = true;
            rotate();
        }

        private void rectReload_Leave(object sender, MouseEventArgs e)
        {
            isOnReloadBtn = false;
        }

        private void rectReload_Click(object sender, MouseButtonEventArgs e)
        {
            isOnReloadBtn = false;
            updateCbxItems();
        }

        private void btnSave_Enter(object sender, MouseEventArgs e)
        {
            btnSave.Width += 5;
            btnSave.Height += 5;
            btnSave.Margin = new Thickness(btnSave.Margin.Left - 2.5, btnSave.Margin.Top - 2.5, 0, 0);
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
            fc.FileName = tbxName.Text + ".json";
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
    }
}

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
using System.Windows.Shapes;

namespace Challenges_App.Pages
{
    /// <summary>
    /// Interaction logic for ChallengeSession.xaml
    /// </summary>
    public partial class ChallengeSession : Page
    {
        private List<String> selected = new List<String>();
        private Required.IRequired? requiredSession;
        private Reward rewardSession;
        private Boolean isOnReloadBtn = false;

        public ChallengeSession(String uuid)
        {
            InitializeComponent();
            tbxResearch_TextChanged(tbxResearch, null);
            Ressource.fillRect(rectReloadIcon, Ressource.getImage(Files.ResxFile.RlButton));
            Ressource.fillRect(rectReloadCategories, Ressource.getImage(Files.ResxFile.RlButton));
            Ressource.fillRect(btnSave, Ressource.getImage(Files.ResxFile.SaveButton));
            lblTitle.Tag = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            tbxUUID.Text = uuid;
            tbxName.Text = "new";
            updateCbxItems();
            updateCbxCategories();
            btnPush.addMouseClick((c) =>
            {
                MainMenu? menu = MainWindow.Instance.menu;
                if(menu != null)
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
                                btnPush.Enabled = true;
                                btnPush.Animation = false;
                                if (!state)
                                {
                                    push(menu);
                                }
                                else
                                {
                                    MessageBox.Show("Il semble y avoir un problème lorsque le client veut désactiver les challenges.\nLe push n'a pas fonctionné.", "Error can't send challenge");
                                }
                            });
                        }
                    }
                }
            });
            btnRequired.addMouseClick(c =>
            {
                if (requiredSession != null)
                {
                    requiredSession.navigate();
                }
                else
                {
                    if (cbxType.SelectedIndex == -1)
                    {
                        MessageBox.Show("Le type est vide. Impossible d'obtenir une instance de requis.", "Error type field empty");
                    }
                    else
                    {
                        Required.IRequired.Required type = Required.IRequired.Required.Items;
                        switch (((ComboBoxItem)cbxType.SelectedItem).Name)
                        {
                            case "INVENTORY":
                                {
                                    type = Required.IRequired.Required.Items;
                                }
                                break;
                            case "ISLAND":
                                {
                                    type = Required.IRequired.Required.Island;
                                }
                                break;
                            case "STAT":
                                {
                                    type = Required.IRequired.Required.Stats;
                                }
                                break;
                            case "OTHER":
                                {
                                    type = Required.IRequired.Required.Others;
                                }
                                break;
                            default:
                                {
                                    MessageBox.Show("Le type spécifié n'est pas pris en charge par les challenges.", "Error invalid type");
                                    return;
                                }
                        }
                        requiredSession = Required.IRequired.loadRequired(type, this, null);
                        if (requiredSession != null)
                        {
                            cbxType.IsEnabled = false;
                            requiredSession.navigate();
                        }
                    }
                }
            });
            btnReward.addMouseClick(c =>
            {
                if (rewardSession == null)
                {
                    rewardSession = new Reward(this);
                }
                rewardSession.navigate();
            });
        }

        public ChallengeSession(JObject json) : this("")
        {
            if (json.ContainsKey("uuid"))
            {
                tbxUUID.Text = json.Value<String>("uuid");
            }
            if (json.ContainsKey("name"))
            {
                tbxName.Text = json.Value<String>("name");
            }
            if (json.ContainsKey("category"))
            {
                foreach(ComboBoxItem item in cbxCategory.Items)
                {
                    if (item.Tag == null) continue;
                    if (item.Tag.ToString().Equals(json.Value<String>("category")))
                    {
                        cbxCategory.SelectedItem = item;
                        break;
                    }
                }
            }
            if (json.ContainsKey("description"))
            {
                tbxDescription.Text = json.Value<String>("description").Replace("\n","\\n");
            }
            if (json.ContainsKey("type"))
            {
                object retrieve = PageChallengeSession.FindName(json.Value<String>("type"));
                if(retrieve != null && retrieve is ComboBoxItem)
                {
                    cbxType.SelectedItem = (ComboBoxItem)retrieve;
                }
            }
            cbActive.IsChecked = json.ContainsKey("active") && json.Value<Boolean>("active");
            cbActive_Click(cbActive, null);
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
                tbxPage.Text = "" + json.Value<Int32>("page");
            }
            if (json.ContainsKey("slot"))
            {
                tbxSlot.Text = "" + json.Value<Int32>("slot");
            }
            if (json.ContainsKey("redoneLimit"))
            {
                tbxRedoLimit.Text = "" + json.Value<Int32>("redoneLimit");
            }
            if (json.ContainsKey("lastEdited"))
            {
                lblTitle.Tag = json.Value<long>("lastEdited");
            }
            if (json.ContainsKey("required"))
            {
                JObject required = json["required"] as JObject;
                requiredSession = Required.IRequired.loadRequired(Enum.Parse<Required.IRequired.Required>(required.Value<String>("RequiredType")), this, required["Required"] as JObject);
                if (requiredSession != null)
                {
                    cbxType.IsEnabled = false;
                }
            }
            if (json.ContainsKey("reward"))
            {
                rewardSession = new Reward(this, json["reward"] as JObject);
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

        public static ChallengeSession fromFile(String path)
        {
            return new ChallengeSession(JObject.Parse(Encoding.UTF8.GetString(File.ReadAllBytes(path))));
        }

        public JObject toJson()
        {
            JObject json = new JObject();
            json["uuid"] = tbxUUID.Text;
            if (tbxName.Text.Length > 0)
            {
                json["name"] = tbxName.Text;
            }
            if (cbxCategory.SelectedIndex > 0)
            {
                json["category"] = ((ComboBoxItem)cbxCategory.SelectedItem).Tag.ToString();
            }
            if (tbxDescription.Text.Length > 0)
            {
                json["description"] = tbxDescription.Text.Replace("\\n","\n");
            }
            if(cbxType.SelectedIndex != -1)
            {
                json["type"] = ((ComboBoxItem)cbxType.SelectedItem).Name;
            }
            if(cbActive.IsChecked.HasValue && cbActive.IsChecked.Value)
            {
                json["active"] = true;
            }
            if (cbxIcon.SelectedIndex != -1)
            {
                Item? item = MainWindow.Instance.menu.getItemManager().getItem(cbxIcon.SelectedItem.ToString());
                if(item != null)
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
            if (tbxRedoLimit.Text.Length > 0)
            {
                int value = Ressource.getInt(tbxRedoLimit.Text, -1); 
                if (value >= 0) //La limite peut etre -1 = n'a pas de limite. Mais c'est la valeur par défaut
                {
                    json["redoneLimit"] = value;
                }
            }
            json["lastEdited"] = long.Parse(lblTitle.Tag.ToString());
            if (requiredSession != null)
            {
                JObject required = new JObject();
                required["RequiredType"] = requiredSession.getType().ToString();
                required["Required"] = requiredSession.toJson();
                json["required"] = required;
            }
            if (rewardSession != null)
            {
                json["reward"] = rewardSession.toJson();
            }
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
                if (menu.send(new Packet.Packets.UpdateRequestPacket(tbxUUID.Text, Packet.IPacket.Target.CHALLENGE, j)))
                {
                    menu.getLogger().addText(LogManager.LogType.INFO, "Le challenge " + tbxName.Text + " a bien été envoyé au serveur.");
                }
                else
                {
                    MessageBox.Show("Une erreur est survenue lors de la tentative d'envoi.\nLa session est-elle en active et valide?", "Error session unavailable");
                }
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
            MainWindow.Instance.MainFrame.Navigate(MainWindow.Instance.menu);
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

        private void tbxResearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (tbxResearch.Text.Length == 0)
            {
                tbxResearch.Text = "Rechercher...";
                tbxResearch.Foreground = Brushes.DarkSlateGray;
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

        private void tbxResearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            boxReqChallenges.Items.Clear();
            foreach (KeyValuePair<String,String> s in MainWindow.Instance.menu.getChallenges())
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

        private void NumericInput(object sender, TextCompositionEventArgs e)
        {
            int result = 0;
            if (!Int32.TryParse(e.Text, out result))
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

        private void Name_Changed(object sender, TextChangedEventArgs e)
        {
            lblTitle.Content = "Challenge : " + (tbxName.Text.Length==0?"null":tbxName.Text);
            MainWindow.Instance.menu.changeChallengeName(getUUID(), tbxName.Text);
        }

        private void updateCbxCategories()
        {
            cbxCategory.Items.Clear();
            MainMenu menu = MainWindow.Instance.menu;
            ComboBoxItem cbxNone = new ComboBoxItem();
            cbxNone.Content = "Aucune";
            cbxCategory.Items.Add(cbxNone);
            foreach (KeyValuePair<String,String> c in menu.getCategories())
            {
                ComboBoxItem cbx = new ComboBoxItem();
                cbx.Content = c.Value;
                cbx.Tag = c.Key;
                cbxCategory.Items.Add(cbx);
            }
            cbxCategory.SelectedIndex = 0;
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

        private async void rotate(RotateTransform rotate)
        {
            while (isOnReloadBtn)
            {
                double angle = rotate.Angle;
                if (angle <= 0)
                {
                    angle = 360;
                }
                else
                {
                    angle -= 4;
                }
                Ressource.synchronize(() => rotate.Angle = angle);
                await Task.Delay(1);
            }
            rotate.Angle = 360;
        }

        private void rectReload_Enter(object sender, MouseEventArgs e)
        {
            isOnReloadBtn = true;
            rotate((((Rectangle)sender).RenderTransform as TransformGroup).Children[0] as RotateTransform);
        }

        private void rectReload_Leave(object sender, MouseEventArgs e)
        {
            isOnReloadBtn = false;
        }

        private void rectReload_Click(object sender, MouseButtonEventArgs e)
        {
            isOnReloadBtn = false;
            if (((FrameworkElement)sender).Name.Equals("rectReloadIcon"))
            {
                updateCbxItems();
            }
            else
            {
                updateCbxCategories();
            }
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

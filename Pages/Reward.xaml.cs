using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Challenges_App.Pages
{
    /// <summary>
    /// Interaction logic for Reward.xaml
    /// </summary>
    public partial class Reward : Page
    {
        private ChallengeSession parent;
        private Boolean isOnReloadBtn = false;
        private _Reward first;
        private _Reward next;
        public Reward(ChallengeSession session)
        {
            this.parent = session;
            InitializeComponent();
            lblTitle.Content = lblTitle.Content.ToString().Replace("[C]", parent.getName());
            Ressource.fillRect(rectReload, Ressource.getImage(Files.ResxFile.RlButton));
            cbxReward.SelectedIndex = 0;
            updateCbxItems();
            updateBoxItems();
            btnAdd.addMouseClick(c => add());
            btnRem.addMouseClick(c => rem());
        }
        
        public Reward(ChallengeSession session, JObject json) : this(session)
        {
            if (json.ContainsKey("First"))
            {
                first = new _Reward(json["First"] as JObject);
            }
            if (json.ContainsKey("Next"))
            {
                next = new _Reward(json["Next"] as JObject);
            }
            updateRewards();
            updateCbxItems();
        }

        public JObject toJson()
        {
            JObject json = new JObject();
            cbxReward.SelectedIndex = 0;
            cbxReward.SelectedIndex = 1;
            if (first != null)
            {
                json["First"] = first.toJson();
            }
            if(next != null)
            {
                json["Next"] = next.toJson();
            }
            return json;
        }

        public void navigate()
        {
            MainWindow.Instance.MainFrame.Navigate(this);
        }

        private void add()
        {
            if(getCurrent() == null)
            {
                MessageBox.Show("Une erreur inattendue est apparue.", "Error current reward = null");
                return;
            }
            if (cbxItem.SelectedIndex != -1)
            {
                int count, luck, sort;
                if (Int32.TryParse(tbxItemCount.Text, out count))
                {
                    Boolean l = Int32.TryParse(tbxLuck.Text, out luck);
                    Boolean s = Int32.TryParse(tbxSortOrder.Text, out sort);
                    if (!l) luck = -1;
                    if (!s) sort = -1;
                    if (luck == -1 || (luck > 0 && luck <= 100))
                    {
                        _Reward r = getCurrent();
                        Item? item = retrieveItem(r, cbxItem.SelectedItem.ToString());
                        String? uuid = null;
                        if (item != null)
                        {
                            uuid = item.UUID;
                            if (MessageBox.Show("Cet item existe déjà.\nEn faisant ça tu vas écraser son contenu.\nContinuer?", "Error type already exists", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
                            {
                                r.removeItem(item);
                            }
                            else return;
                        }
                        item = new Item(uuid==null?Guid.NewGuid().ToString():uuid);
                        item.Name = cbxItem.SelectedItem.ToString();
                        item.Count = count;
                        item.Luck = luck;
                        item.SortOrder = sort;
                        r.addItem(item);
                        updateBoxItems();
                    }
                    else
                    {
                        MessageBox.Show("La chance doit se situer entre 0 (exclus) et 100 (inclus). (" + luck + ")", "Error wrong value");
                    }
                }
                else
                {
                    MessageBox.Show("La quantité spécifiée n'est pas un réel entier.", "Error wrong type");
                }
            }
            else
            {
                MessageBox.Show("Aucun item n'a été séléctionné.", "Error item empty");
            }
        }

        private void rem()
        {
            if (getCurrent() == null)
            {
                MessageBox.Show("Une erreur inattendue est apparue.", "Error current reward = null");
                return;
            }
            if (boxItems.SelectedIndex != -1)
            {
                _Reward r = getCurrent();
                Item? item = retrieveItem(r, boxItems.SelectedItem.ToString());
                if (item != null)
                {
                    r.removeItem(item);
                    updateBoxItems();
                }
                else
                {
                    MessageBox.Show("Item introuvable...", "Error not found");
                }
            }
            else
            {
                MessageBox.Show("Aucun item n'a été séléctionné...", "Error no item selected");
            }
        }

        private void updateBoxItems()
        {
            boxItems.Items.Clear();
            boxItems.Items.Add("Nouvel item...");
            _Reward r = getCurrent();
            if (r != null)
            {
                foreach(Item i in r.Items)
                {
                    boxItems.Items.Add(i.Name);
                }
            }
        }

        private _Reward getCurrent()
        {
            if(cbxReward.SelectedIndex == 0)
            {
                return first;
            }
            else
            {
                return next;
            }
        }

        private Item? retrieveItem(_Reward r, String name)
        {
            if (r == null) return null;
            foreach(Item i in r.Items)
            {
                if (i.Name.Equals(name)) return i;
            }
            return null;
        }

        private void AppendText(String Text, Brush Color)
        {
            TextRange range = new TextRange(rtbCommands.Document.ContentEnd, rtbCommands.Document.ContentEnd);
            range.Text = Text+"\r";
            range.ApplyPropertyValue(TextElement.ForegroundProperty, Color);
            rtbCommands.ScrollToEnd();
        }

        private List<String> getCommands(String text)
        {
            List<String> cmds = new List<String>();
            foreach (String cmd in text.Replace("\r\n","\r").Split("\r"))
            {
                if (cmd.Length > 0)
                {
                    cmds.Add(cmd);
                }
            }
            return cmds;
        }

        private void updateRewards()
        {
            _Reward r = getCurrent();
            if (r != null)
            {
                if (r.Message != null)
                {
                    tbxMsg.Text = r.Message;
                }
                else
                {
                    tbxMsg.Text = (r == first ? "Challenge-Completed-First" : "Challenge-Completed-Next");
                }
                foreach (String cmd in r.Commands)
                {
                    AppendText(cmd, Brushes.Black);
                }
                if (r.Money != -1)
                {
                    tbxMoneyCount.Text = r.Money.ToString();
                }
                if (r.Experience != -1)
                {
                    tbxExpCount.Text = r.Experience.ToString();
                }
                cbxExpType.SelectedIndex = r.ExpType.Equals("ORB")?0:1;
                updateBoxItems();
            }
        }

        private _Reward saveReward(_Reward reward)
        {
            _Reward r = reward;
            if (reward == null)
            {
                r = new _Reward();
            }

            r.Message = tbxMsg.Text.Length==0?null:tbxMsg.Text;
            tbxMsg.Text = "";

            TextRange textCmd = new TextRange(rtbCommands.Document.ContentStart, rtbCommands.Document.ContentEnd);
            r.Commands = getCommands(textCmd.Text);
            textCmd.Text = "";

            int money, experience;
            if (tbxMoneyCount.Text.Length > 0)
            {
                if (Int32.TryParse(tbxMoneyCount.Text, out money))
                {
                    r.Money = money;
                }
                else r.Money = -1;
            }
            else r.Money = -1;
            tbxMoneyCount.Text = "";

            if (tbxExpCount.Text.Length > 0)
            {
                if (Int32.TryParse(tbxExpCount.Text, out experience))
                {
                    r.Experience = experience;
                }
                else r.Experience = -1;
            }
            else r.Experience = -1;

            tbxExpCount.Text = "";
            r.ExpType = ((ComboBoxItem)cbxExpType.SelectedItem).Content.ToString();
            cbxExpType.SelectedIndex = 0;
            tbxItemCount.Text = "";
            tbxLuck.Text = "";
            tbxSortOrder.Text = "";
            if (cbxItem.Items.Count > 0) cbxItem.SelectedIndex = 0;
            return r;
        }

        private void updateCbxItems()
        {
            cbxItem.Items.Clear();
            ItemsManager m = MainWindow.Instance.menu.getItemManager();
            foreach (String item in m.getItems())
            {
                cbxItem.Items.Add(item);
            }
            if (cbxItem.Items.Count > 0)
            {
                cbxItem.SelectedIndex = 0;
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

        private void NumericInput(object sender, TextCompositionEventArgs e)
        {
            int result = 0;
            if (!Int32.TryParse(e.Text, out result))
            {
                e.Handled = true;
            }
            else
            {
                if (((TextBox)sender).Text.Length > 5)
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

        private void cbxReward_Changed(object sender, SelectionChangedEventArgs e)
        {
            if(cbxReward.SelectedIndex == 0)
            {
                next = saveReward(next);
                lblMsg.Content = "Bmsg :";
                lblMsg.ToolTip = "Il faut en réalité spécifier la clé(des fichiers Lang) du message à envoyer.\nLe message sera envoyé à tout les joueurs en ligne.";
                if (first == null)
                {
                    first = new _Reward();
                }
                updateRewards();
            }
            else
            {
                first = saveReward(first);
                lblMsg.Content = "Msg :";
                lblMsg.ToolTip = "Il faut en réalité spécifier la clé(des fichiers Lang) du message à envoyer.\nLe message sera envoyé uniquement au joueur qui complète le challenge.";
                if (next == null)
                {
                    next = new _Reward();
                }
                updateRewards();
            }
        }

        private void boxItems_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (boxItems.SelectedIndex == -1) return;
            if (boxItems.SelectedIndex == 0)
            {
                tbxItemCount.Text = "";
                tbxLuck.Text = "";
                tbxSortOrder.Text = "";
                if (cbxItem.Items.Count > 0) cbxItem.SelectedIndex = 0;
            }
            else
            {
                _Reward r = getCurrent();
                if (getCurrent() == null)
                {
                    MessageBox.Show("Une erreur inattendue est apparue.", "Error current reward = null");
                    return;
                }
                Item? i = retrieveItem(r, boxItems.SelectedItem.ToString());
                if (i != null)
                {
                    if (cbxItem.Items.Contains(i.Name))
                    {
                        cbxItem.SelectedItem = i.Name;
                    }
                    else
                    {
                        if (cbxItem.Items.Count > 0)
                        {
                            cbxItem.SelectedIndex = 0;
                        }
                        MessageBox.Show("L'item '" + i.Name + "' précédemment séléctionné pour cette entrée est introuvable!\nPeut-être faudrait-il actualiser la liste?", "Error item not found");
                    }
                    tbxItemCount.Text = i.Count.ToString();
                    tbxLuck.Text = i.Luck == -1 ? "" : i.Luck.ToString();
                    tbxSortOrder.Text = i.SortOrder == -1 ? "" : i.SortOrder.ToString();
                }
            }
        }

        private class Item
        {
            public String UUID { get; private set; }
            public String Name { get; set; }
            public int Count { get; set; }
            public int Luck { get; set; } = -1;//-1 pour vide (100%)
            public int SortOrder { get; set; } = -1;//-1 pour vide
            public Item(String uuid) 
            {
                UUID = uuid;
            }
            public Item(JObject json)
            {
                if (json.ContainsKey("UUID"))
                {
                    UUID = json.Value<String>("UUID");
                }
                if (json.ContainsKey("Item"))
                {
                    JObject item = json["Item"] as JObject;
                    if (item != null)
                    {
                        Pages.Item i = new Pages.Item(item);
                        MainWindow.Instance.menu.getItemManager().forceAdd(i);
                        Name = i.getName();
                    }
                }
                if (json.ContainsKey("Count"))
                {
                    Count = json.Value<Int32>("Count");
                }
                if (json.ContainsKey("Increment")) //Laissé Increment car le serveur utilisera le meme champ en fonction de si c'est un requis ou une récompense
                {
                    Luck = json.Value<Int32>("Increment");
                }
                if (json.ContainsKey("SortOrder"))
                {
                    SortOrder = json.Value<Int32>("SortOrder");
                }
            }
            public JObject toJson()
            {
                JObject json = new JObject();
                Pages.Item? i = MainWindow.Instance.menu.getItemManager().getItem(Name);
                if (i == null)
                {
                    return json;
                }
                json["UUID"] = UUID;
                json["Item"] = i.toJson();
                json["Count"] = Count;
                if (Luck != -1)
                {
                    json["Increment"] = Luck; //Laissé Increment car le serveur utilisera le meme champ en fonction de si c'est un requis ou une récompense
                }
                if (SortOrder != -1)
                {
                    json["SortOrder"] = SortOrder;
                }
                return json;
            }
        }

        private class _Reward
        {
            public String? Message { get; set; }
            public List<String> Commands { get; set; } = new List<String>();
            public int Money { get; set; } = -1;
            public int Experience { get; set; } = -1;
            public String ExpType { get; set; } = "ORB";
            public List<Item> Items { get; } = new List<Item>();
            public _Reward() { }
            public _Reward(JObject json)
            {
                if (json.ContainsKey("Items"))
                {
                    JArray jarr = json["Items"] as JArray;
                    foreach(JObject item in jarr)
                    {
                        addItem(new Item(item));
                    }
                }
                if (json.ContainsKey("Commands"))
                {
                    JArray jarr = json["Commands"] as JArray;
                    foreach(String cmd in jarr)
                    {
                        Commands.Add(cmd);
                    }
                }
                if (json.ContainsKey("Message"))
                {
                    Message = json.Value<String>("Message");
                }
                if (json.ContainsKey("Money"))
                {
                    Money = (json["Money"] as JObject).Value<Int32>("Count");
                }
                if (json.ContainsKey("Experience"))
                {
                    JObject exp = json["Experience"] as JObject;
                    Experience = exp.Value<Int32>("Count");
                    if (exp.ContainsKey("Type"))
                    {
                        ExpType = exp.Value<String>("Type");
                    }
                }
            }
            public JObject toJson()
            {
                JObject json = new JObject();
                if (Items.Count > 0)
                {
                    JArray jarr = new JArray();
                    foreach (Item item in Items)
                    {
                        JObject j = item.toJson();
                        if (j.Count > 0)
                        {
                            jarr.Add(j);
                        }
                    }
                    if(jarr.Count > 0)
                    {
                        json["Items"] = jarr;
                    }
                }
                if (Commands.Count > 0)
                {
                    JArray jarr = new JArray();
                    foreach(String cmd in Commands)
                    {
                        jarr.Add(cmd);
                    }
                    json["Commands"] = jarr;
                }
                if (Message != null)
                {
                    json["Message"] = Message;
                }
                if (Money > 0)
                {
                    JObject money = new JObject();
                    money["UUID"] = Guid.NewGuid().ToString();
                    money["Count"] = Money;
                    json["Money"] = money;
                }
                if (Experience > 0)
                {
                    JObject exp = new JObject();
                    exp["UUID"] = Guid.NewGuid().ToString();
                    exp["Count"] = Experience;
                    exp["Type"] = ExpType;
                    json["Experience"] = exp;
                }
                return json;
            }
            public Boolean addItem(Item item)
            {
                if (!this.Items.Contains(item))
                {
                    this.Items.Add(item);
                    return true;
                }
                return false;
            }
            public Boolean removeItem(Item item)
            {
                return this.Items.Remove(item);
            }
        }
    }
}

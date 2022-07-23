using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Challenges_App.Pages.Required
{
    /// <summary>
    /// Interaction logic for Items.xaml
    /// </summary>
    public partial class Items : Page, IRequired
    {
        private ChallengeSession parent;
        private List<Item> items = new List<Item>();
        private Boolean isOnReloadBtn = false;
        public Items(ChallengeSession session)
        {
            this.parent = session;
            InitializeComponent();
            lblTitle.Content = lblTitle.Content.ToString().Replace("[C]", parent.getName());
            updateCbxItems();
            if (cbxItem.Items.Count > 0)
            {
                cbxItem.SelectedIndex = 0;
            }
            Ressource.fillRect(rectReload, Ressource.getImage(Files.ResxFile.RlButton));
            updateBoxItems();
            btnAdd.addMouseClick(c =>
            {
                add();
            });
            btnRem.addMouseClick(c =>
            {
                rem();
            });
        }

        public Items(ChallengeSession session, JObject json) : this(session)
        {
            if (json.ContainsKey("Items"))
            {
                JArray jarr = json["Items"] as JArray;
                foreach(JObject item in jarr)
                {
                    items.Add(new Item(item));
                }
                updateBoxItems();
                updateCbxItems();
            }
        }

        JObject IRequired.toJson()
        {
            JObject json = new JObject();
            if (items.Count > 0)
            {
                JArray jarr = new JArray();
                foreach (Item item in items)
                {
                    JObject j = item.toJson();
                    if (j.Count > 0)//empty if pages.item == null
                    {
                        jarr.Add(j);
                    }
                }
                json["Items"] = jarr;
            }
            return json;
        }

        IRequired.Required IRequired.getType()
        {
            return IRequired.Required.Items;
        }

        void IRequired.navigate()
        {
            MainWindow.Instance.MainFrame.Navigate(this);
        }

        private void add()
        {
            if (cbxItem.SelectedIndex != -1)
            {
                int count, increment, sort;
                if(Int32.TryParse(tbxCount.Text, out count))
                {
                    Boolean inc = Int32.TryParse(tbxIncrement.Text, out increment);
                    Boolean s = Int32.TryParse(tbxSortOrder.Text, out sort);
                    if (!inc) increment = -1;
                    if (!s) sort = -1;
                    Item? item = retrieveItem(cbxItem.SelectedItem.ToString());
                    String? uuid = null;
                    if(item != null)
                    {
                        uuid = item.UUID;
                        if (MessageBox.Show("Cet item existe déjà.\nEn faisant ça tu vas écraser son contenu.\nContinuer?", "Error type already exists", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
                        {
                            items.Remove(item);
                        }
                        else return;
                    }
                    item = new Item(uuid==null?Guid.NewGuid().ToString():uuid);
                    item.Name = cbxItem.SelectedItem.ToString();
                    item.Count = count;
                    item.Increment = increment;
                    item.SortOrder = sort;
                    items.Add(item);
                    updateBoxItems();
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
            if (boxItems.SelectedIndex != -1)
            {
                Item? item = retrieveItem(boxItems.SelectedItem.ToString());
                if (item != null)
                {
                    items.Remove(item);
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

        private void updateCbxItems()
        {
            cbxItem.Items.Clear();
            ItemsManager m = MainWindow.Instance.menu.getItemManager();
            foreach(String item in m.getItems())
            {
                cbxItem.Items.Add(item);
            }
            if (cbxItem.Items.Count > 0)
            {
                cbxItem.SelectedIndex = 0;
            }
        }

        private void updateBoxItems()
        {
            boxItems.Items.Clear();
            boxItems.Items.Add("Nouvel item...");
            foreach(Item i in items)
            {
                boxItems.Items.Add(i.Name);
            }
        }

        private Item? retrieveItem(String name)
        {
            foreach(Item i in items)
            {
                if (i.Name.Equals(name)) return i;
            }
            return null;
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

        private void box_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (boxItems.SelectedIndex == -1) return;
            if(boxItems.SelectedIndex == 0)
            {
                if (cbxItem.Items.Count > 0)
                {
                    cbxItem.SelectedIndex = 0;
                }
                tbxCount.Text = "";
                tbxIncrement.Text = "";
                tbxSortOrder.Text = "";
            }
            else
            {
                Item? item = retrieveItem(boxItems.SelectedItem.ToString());
                if(item != null)
                {
                    if (cbxItem.Items.Contains(item.Name))
                    {
                        cbxItem.SelectedItem = item.Name;
                    }
                    else
                    {
                        if (cbxItem.Items.Count > 0)
                        {
                            cbxItem.SelectedIndex = 0;
                        }
                        MessageBox.Show("L'item '" + item.Name + "' précédemment séléctionné pour cette entrée est introuvable!\nPeut-être faudrait-il actualiser la liste?", "Error item not found");
                    }
                    tbxCount.Text = item.Count.ToString();
                    tbxIncrement.Text = item.Increment == -1 ? "" : item.Increment.ToString();
                    tbxSortOrder.Text = item.SortOrder == -1 ? "" : item.SortOrder.ToString();
                }
            }
        }

        private class Item
        {
            public String UUID { get; private set; }
            public String Name { get; set; }
            public int Count { get; set; }
            public int Increment { get; set; } = -1;//-1 pour vide
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
                if(json.ContainsKey("Item"))
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
                if (json.ContainsKey("Increment"))
                {
                    Increment = json.Value<Int32>("Increment");
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
                if (Increment != -1)
                {
                    json["Increment"] = Increment;
                }
                if (SortOrder != -1)
                {
                    json["SortOrder"] = SortOrder;
                }
                return json;
            }
        }
    }
}

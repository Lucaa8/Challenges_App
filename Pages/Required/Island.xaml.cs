using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Challenges_App.Pages.Required
{
    /// <summary>
    /// Interaction logic for IslandRequired.xaml
    /// </summary>
    public partial class Island : Page, IRequired
    {
        private ChallengeSession session;
        enum Type
        {
            NONE, MATERIAL, ENTITY
        }
        public Island(ChallengeSession parent)
        {
            this.session = parent;
            InitializeComponent();
            lblTitle.Content = lblTitle.Content.ToString().Replace("[C]", parent.getName());
            Research_Changed(tbxResearch, null);
            btnAdd.addMouseClick(c =>
            {
                add();
            });
            btnRem.addMouseClick(c =>
            {
                rem();
            });
        }

        public Island(ChallengeSession parent, JObject json) : this(parent)
        {
            if (json.ContainsKey("Radius"))
            {
                tbxRadius.Text = json.Value<Int32>("Radius") + "";
            }
            if (json.ContainsKey("Blocks"))
            {
                JArray blocks = json["Blocks"] as JArray;
                foreach(JObject block in blocks)
                {
                    add(Ressource.materials[block.Value<String>("Material")], block.Value<Int32>("Count"), block.Value<String>("UUID"));
                }
            }
            if (json.ContainsKey("Entities"))
            {
                JArray entities = json["Entities"] as JArray;
                foreach (JObject entity in entities)
                {
                    add(Ressource.entitiesType[entity.Value<String>("Entity")], entity.Value<Int32>("Count"), entity.Value<String>("UUID"));
                }
            }
        }

        JObject IRequired.toJson()
        {
            JObject json = new JObject();
            if (tbxRadius.Text.Length > 0)
            {
                int value = Ressource.getInt(tbxRadius.Text, -1);
                if (value >= 0)
                {
                    json["Radius"] = value;
                }
            }
            if (boxBlocks.Items.Count > 0)
            {
                JArray jarr = new JArray();
                foreach(ListBoxItem block in boxBlocks.Items)
                {
                    JObject b = new JObject();
                    String[] values = block.Tag.ToString().Split(":");
                    b["UUID"] = values[0];
                    b["Material"] = values[1];
                    b["Count"] = Int32.Parse(values[2]);
                    jarr.Add(b);
                }
                json["Blocks"] = jarr;
            }
            if (boxEntities.Items.Count > 0)
            {
                JArray jarr = new JArray();
                foreach (ListBoxItem entity in boxEntities.Items)
                {
                    JObject b = new JObject();
                    String[] values = entity.Tag.ToString().Split(":");
                    b["UUID"] = values[0];
                    b["Entity"] = values[1];
                    b["Count"] = Int32.Parse(values[2]);
                    jarr.Add(b);
                }
                json["Entities"] = jarr;
            }
            return json;
        }

        IRequired.Required IRequired.getType()
        {
            return IRequired.Required.Island;
        }

        void IRequired.navigate()
        {
            MainWindow.Instance.MainFrame.Navigate(this);
        }

        //Cette méthode est utilisée uniquement pour ajouter/modifier un block/entité non connue par le serveur (sans UUID connu)
        private void add()
        {
            if(boxType.SelectedIndex != -1)
            {
                int count = Int32.Parse(tbxCount.Text);
                if(count > 0 && count < 1000)
                {
                    ListBoxItem? item = getByName(boxType.SelectedItem.ToString());
                    String? uuid = null;
                    if (item != null)
                    {
                        uuid = item.Tag.ToString().Split(":")[0];
                        int value = Int32.Parse(item.Tag.ToString().Split(":")[1]);
                        if (MessageBox.Show("Ce type existe déjà (Valeur="+value+").\nEn faisant ça tu vas écraser son contenu.\nContinuer?", "Error type already exists", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
                        {
                            getBox(item.Content.ToString()).Items.Remove(item);
                        }
                        else return;
                    }
                    add(boxType.SelectedItem.ToString(), count, uuid == null ? Guid.NewGuid().ToString() : uuid);
                }
                else
                {
                    MessageBox.Show("La valeur doit être contenue entre 0 et 1000.", "Error wrong value");
                }
            }
            else
            {
                MessageBox.Show("Aucun type n'a été séléctionné.", "Error type empty");
            }
        }

        private void add(String subject, int count, String uuid)
        {
            ListBoxItem item = new ListBoxItem();
            item.Content = subject;
            item.Tag = uuid + ":" + subject.ToUpper().Replace(' ','_') + ":" + count;
            item.ToolTip = "Valeur : " + count;
            item.SetValue(ToolTipService.InitialShowDelayProperty, 0);
            item.SetValue(ToolTipService.BetweenShowDelayProperty, 0);
            getBox(item.Content.ToString()).Items.Add(item);
        }

        private void rem()
        {
            if (boxBlocks.SelectedIndex != -1)
            {
                boxBlocks.Items.Remove(boxBlocks.SelectedItem);
            }
            else if(boxEntities.SelectedIndex != -1)
            {
                boxEntities.Items.Remove(boxEntities.SelectedItem);
            }
            else
            {
                MessageBox.Show("Aucun bloc ni entité n'est séléctionnée!", "Error no selection");
            }
        }

        private ListBoxItem? getByName(String name)
        {
            Type type = getType(name);
            if (type == Type.MATERIAL)
            {
                foreach(ListBoxItem item in boxBlocks.Items)
                {
                    if (item.Content.Equals(name)) return item;
                }
            }
            else if (type == Type.ENTITY)
            {
                foreach (ListBoxItem item in boxEntities.Items)
                {
                    if (item.Content.Equals(name)) return item;
                }
            }
            return null;
        }

        private ListBox? getBox(String name)
        {
            Type type = getType(name);
            if (type == Type.ENTITY) return boxEntities;
            if (type == Type.MATERIAL) return boxBlocks;
            return null;
        }

        private Type getType(String type)
        {
            if (Ressource.blocks.Values.Contains(type)) return Type.MATERIAL;
            if (Ressource.entitiesType.Values.Contains(type)) return Type.ENTITY;
            return Type.NONE;
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
            MainWindow.Instance.MainFrame.Navigate(session);
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

        private void Research_Changed(object sender, TextChangedEventArgs e)
        {
            boxType.Items.Clear();
            foreach(String mat in Ressource.blocks.Values)
            {
                if (tbxResearch.Text.Equals("Rechercher...")||mat.ToLower().Contains(tbxResearch.Text.ToLower()))
                {
                    boxType.Items.Add(mat);
                }
            }
            foreach (String ent in Ressource.entitiesType.Values)
            {
                if (tbxResearch.Text.Equals("Rechercher...")||ent.ToLower().Contains(tbxResearch.Text.ToLower()))
                {
                    boxType.Items.Add(ent);
                }
            }
            if(boxType.Items.Count > 0)
            {
                boxType.ScrollIntoView(boxType.Items[0]);
            }
        }

        private void Research_LostFocus(object sender, RoutedEventArgs e)
        {
            if (tbxResearch.Text.Length == 0)
            {
                tbxResearch.Text = "Rechercher...";
                tbxResearch.Foreground = Brushes.DarkSlateGray;
            }
        }

        private void Research_GotFocus(object sender, RoutedEventArgs e)
        {
            if (tbxResearch.Text.Equals("Rechercher...") && tbxResearch.Foreground == Brushes.DarkSlateGray)
            {
                tbxResearch.Text = "";
                tbxResearch.Foreground = Brushes.Black;
            }
        }

        private void boxBlocks_GotFocus(object sender, RoutedEventArgs e)
        {
            boxEntities.SelectedIndex = -1;
        }

        private void boxEntities_GotFocus(object sender, RoutedEventArgs e)
        {
            boxBlocks.SelectedIndex = -1;
        }
    }
}

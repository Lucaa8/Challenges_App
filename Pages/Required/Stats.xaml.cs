using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Challenges_App.Pages.Required
{
    /// <summary>
    /// Interaction logic for Stats.xaml
    /// </summary>
    public partial class Stats : Page, IRequired
    {
        private ChallengeSession parent;
        private List<Stat> statList = new List<Stat>();
        private String? currentSelection;
        public Stats(ChallengeSession session)
        {
            this.parent = session;
            InitializeComponent();
            lblTitle.Content = lblTitle.Content.ToString().Replace("[C]", parent.getName());
            foreach(String statistic in Ressource.statistics.Keys)
            {
                cbxStatistic.Items.Add(statistic);
            }
            cbxStatistic.SelectedIndex = 0;
            Research_Changed(tbxResearch, null);
            updateStats();
            lblStatistic.Tag = Guid.NewGuid().ToString();
            lblStatistic.ToolTip = "UUID: " + lblStatistic.Tag.ToString();
            lblStatistic.SetValue(ToolTipService.InitialShowDelayProperty, 0);
            btnAdd.addMouseClick(c =>
            {
                add();
            });
            btnRem.addMouseClick(c =>
            {
                rem();
            });
        }

        public Stats(ChallengeSession session, JObject json) : this(session)
        {
            if (json.ContainsKey("Statistics"))
            {
                JArray jarr = json["Statistics"] as JArray;
                foreach(JObject stat in jarr)
                {
                    statList.Add(new Stat(stat));
                }
                updateStats();
            }
        }

        JObject IRequired.toJson()
        {
            JObject json = new JObject();
            if (statList.Count > 0)
            {
                JArray jarr = new JArray();
                foreach (Stat stat in statList)
                {
                    jarr.Add(stat.toJson());
                }
                json["Statistics"] = jarr;
            }
            return json;
        }

        IRequired.Required IRequired.getType()
        {
            return IRequired.Required.Stats;
        }

        void IRequired.navigate()
        {
            MainWindow.Instance.MainFrame.Navigate(this);
        }

        private void add()
        {
            if(cbxStatistic.SelectedIndex != -1)
            {
                String stat = cbxStatistic.SelectedItem.ToString();
                String type = Ressource.statistics[stat];
                if (type.Equals("UNTYPED") || currentSelection != null)
                {
                    int count;
                    if(Int32.TryParse(tbxCount.Text, out count))
                    {
                        if(count > 0 && count < 100000000)
                        {
                            Stat? s = retrieveStat(lblStatistic.Tag.ToString());
                            if(s != null)
                            {
                                if (MessageBox.Show("Une statistique avec cet UUID existe déjà.\n(Stat: "+s.Statistic+", "+(s.Substatistic!=null?"Substat: "+s.Substatistic+", ":"")+"Quantité: "+s.Count+")\nEn faisant ça tu vas écraser son contenu.\nContinuer?", "Error type already exists", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
                                {
                                    statList.Remove(s);
                                }
                                else return;
                            }
                            s = new Stat(lblStatistic.Tag.ToString());
                            s.Statistic = stat;
                            if (!type.Equals("UNTYPED"))
                            {
                                s.Substatistic = currentSelection;
                            }
                            s.Count = count;
                            statList.Add(s);
                            updateStats();
                            cbxStatistic.IsEnabled = false;
                        }
                        else
                        {
                            MessageBox.Show("La valeur doit se situer entre 0 et 100'000'000","Error wrong value");
                        }
                    }
                    else
                    {
                        MessageBox.Show("La valeur doit être un réel entier.", "Error wrong value type");
                    }
                }
                else
                {
                    MessageBox.Show("La statistique " + stat + " est de type " + type + " mais aucune sous-statistique n'a été séléctionnée!", "Error no substat");
                }
            }
            else
            {
                MessageBox.Show("Aucune statistique n'a été séléctionnée!", "Error no stat");
            }
        }

        private void rem()
        {
            if (boxStatistics.SelectedIndex != -1 && boxStatistics.SelectedItem is ListBoxItem)
            {
                Stat? stat = retrieveStat(((ListBoxItem)boxStatistics.SelectedItem).Tag.ToString());
                if (stat != null)
                {
                    statList.Remove(stat);
                    updateStats();
                }
                else
                {
                    MessageBox.Show("Statistique introuvable...", "Error not found");
                }
            }
            else
            {
                MessageBox.Show("Aucune statistique n'a été séléctionnée...", "Error no stat selected");
            }
        }

        private List<String> getEntriesForStatistic()
        {
            if (cbxStatistic.SelectedIndex != -1)
            {
                String? statistic = cbxStatistic.SelectedItem.ToString();
                if (statistic!=null && Ressource.statistics.ContainsKey(statistic))
                {
                    String type = Ressource.statistics[statistic];
                    if (type.Equals("ITEM")) return new List<String>(Ressource.items.Values);
                    else if (type.Equals("BLOCK")) return new List<String>(Ressource.blocks.Values);
                    else if (type.Equals("ENTITY")) return new List<String>(Ressource.entitiesType.Values);
                }
            }
            return new List<String>();
        }

        private Stat? retrieveStat(String uuid)
        {
            foreach(Stat s in statList)
            {
                if (s.UUID.Equals(uuid)) return s;
            }
            return null;
        }

        private void updateStats()
        {
            boxStatistics.Items.Clear();
            boxStatistics.Items.Add("Nouvelle statistique...");
            foreach(Stat s in statList)
            {
                ListBoxItem lb = new ListBoxItem();
                lb.Tag = s.UUID;
                lb.Content = s.Statistic;
                lb.ToolTip = (s.Substatistic != null ? "Substat: " + s.Substatistic + ", " : "") + "Quantité: " + s.Count + ", UUID: "+s.UUID;
                lb.SetValue(ToolTipService.InitialShowDelayProperty, 0);
                lb.SetValue(ToolTipService.BetweenShowDelayProperty, 0);
                boxStatistics.Items.Add(lb);
            }
        }

        private String getSubStatSelected()
        {
            String selection = boxSubStatistic.SelectedItem.ToString();
            return selection.Substring(selection.IndexOf(' ')+1);
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
                if (((TextBox)sender).Text.Length > 7)
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

        private void Research_GotFocus(object sender, RoutedEventArgs e)
        {
            if (tbxResearch.Text.Equals("Rechercher...") && tbxResearch.Foreground == Brushes.DarkSlateGray)
            {
                tbxResearch.Text = "";
                tbxResearch.Foreground = Brushes.Black;
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

        private void Research_Changed(object sender, TextChangedEventArgs e)
        {
            boxSubStatistic.Items.Clear();
            foreach (String s in getEntriesForStatistic())
            {
                if (tbxResearch.Text.Equals("Rechercher...") || s.ToLower().Contains(tbxResearch.Text.ToLower()))
                {
                    boxSubStatistic.Items.Add("["+(currentSelection!=null&&s.Equals(currentSelection)?"x":"")+"] "+s);
                }
            }
            if (boxSubStatistic.Items.Count > 0)
            {
                if (currentSelection != null)
                {
                    String toFind = "[x] " + currentSelection;
                    if (boxSubStatistic.Items.Contains(toFind))
                    {
                        boxSubStatistic.ScrollIntoView(toFind);
                    }
                    else
                    {
                        boxSubStatistic.ScrollIntoView(boxSubStatistic.Items[0]);
                    }
                }
                else
                {
                    boxSubStatistic.ScrollIntoView(boxSubStatistic.Items[0]);
                }
            }
        }

        private void cbxStatistic_Changed(object sender, SelectionChangedEventArgs e)
        {
            Research_Changed(tbxResearch, null);
        }

        private void boxSubstatistic_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (boxSubStatistic.SelectedIndex == -1) return;
            String current = getSubStatSelected();
            if (current.Equals(currentSelection))
            {
                currentSelection = null;
            }
            else
            {
                currentSelection = current;
            }
            Research_Changed(tbxResearch, null);
        }

        private void box_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (boxStatistics.SelectedIndex == -1) return;
            currentSelection = null;
            tbxResearch.Text = "";
            Research_LostFocus(tbxResearch, null);
            if (boxStatistics.SelectedIndex == 0)
            {
                cbxStatistic.SelectedIndex = 0;
                tbxCount.Text = "";
                lblStatistic.Tag = Guid.NewGuid().ToString();
                cbxStatistic.IsEnabled = true;
            }
            else
            {
                Stat? stat = retrieveStat(((ListBoxItem)boxStatistics.SelectedItem).Tag.ToString());
                if(stat != null)
                {
                    cbxStatistic.IsEnabled = false;
                    lblStatistic.Tag = stat.UUID;
                    cbxStatistic.SelectedItem = stat.Statistic;
                    currentSelection = stat.Substatistic;
                    tbxCount.Text = stat.Count.ToString();
                    Research_Changed(tbxResearch, null);
                }
            }
            lblStatistic.ToolTip = "UUID: " + lblStatistic.Tag.ToString();
        }

        private class Stat
        {
            public String UUID { get; private set; }
            public String Statistic { get; set; }
            public String? Substatistic { get; set; }
            public int Count { get; set; }

            public Stat(String uuid)
            {
                UUID = uuid;
            }
            public Stat(JObject json)
            {
                if (json.ContainsKey("UUID"))
                {
                    UUID = json.Value<String>("UUID");
                }
                if (json.ContainsKey("Statistic"))
                {
                    Statistic = json.Value<String>("Statistic");
                }
                if (json.ContainsKey("Substatistic"))
                {
                    Substatistic = Ressource.materials[json.Value<String>("Substatistic")];
                }
                if (json.ContainsKey("Count"))
                {
                    Count = json.Value<Int32>("Count");
                }
            }
            public JObject toJson()
            {
                JObject json = new JObject();
                json["UUID"] = UUID;
                json["Statistic"] = Statistic;
                if (Substatistic != null)
                {
                    foreach(KeyValuePair<String,String> entry in Ressource.materials)
                    {
                        if (entry.Value.Equals(Substatistic))
                        {
                            json["Substatistic"] = entry.Key;
                            break;
                        }
                    }
                }
                json["Count"] = Count;
                return json;
            }
        }
    }
}

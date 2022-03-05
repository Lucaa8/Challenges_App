using Newtonsoft.Json.Linq;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Challenges_App.Pages.Required
{
    /// <summary>
    /// Interaction logic for Others.xaml
    /// </summary>
    public partial class Others : Page, IRequired
    {
        private ChallengeSession parent;
        public Others(ChallengeSession session)
        {
            this.parent = session;
            InitializeComponent();
            lblTitle.Content = lblTitle.Content.ToString().Replace("[C]", parent.getName());
            lblMoney.Tag = Guid.NewGuid().ToString();
            lblExp.Tag = Guid.NewGuid().ToString();
            lblIsLvl.Tag = Guid.NewGuid().ToString();
        }

        public Others(ChallengeSession session, JObject json) : this(session)
        {
            if (json.ContainsKey("Money"))
            {
                JObject money = json["Money"] as JObject;
                lblMoney.Tag = money.Value<String>("UUID");
                if (money.ContainsKey("Count"))
                {
                    tbxMoneyCount.Text = money.Value<Int32>("Count")+"";
                }
                if (money.ContainsKey("Delete"))
                {
                    cbMoneyDelete.IsChecked = money.Value<Boolean>("Delete");
                    cb_Click(cbMoneyDelete, null);
                }
            }
            if (json.ContainsKey("Exp"))
            {
                JObject exp = json["Exp"] as JObject;
                lblExp.Tag = exp.Value<String>("UUID");
                if (exp.ContainsKey("Count"))
                {
                    tbxExpCount.Text = exp.Value<Int32>("Count") + "";
                }
                if (exp.ContainsKey("Type")&&!exp.Value<String>("Type").Equals("ORB"))
                {
                    cbxExpType.SelectedIndex = 1;
                }
                if (exp.ContainsKey("Delete"))
                {
                    cbExpDelete.IsChecked = exp.Value<Boolean>("Delete");
                    cb_Click(cbExpDelete, null);
                }
            }
            if (json.ContainsKey("IsLevel"))
            {
                JObject islvl = json["IsLevel"] as JObject;
                lblIsLvl.Tag = islvl.Value<String>("UUID");
                if (islvl.ContainsKey("Count"))
                {
                    tbxIsLvlCount.Text = islvl.Value<Int32>("Count") + "";
                }
            }
        }

        JObject IRequired.toJson()
        {
            JObject json = new JObject();
            if (tbxMoneyCount.Text.Length > 0)
            {
                JObject money = new JObject();
                int value = Ressource.getInt(tbxMoneyCount.Text, -1);
                if (value > 0)
                {
                    money["UUID"] = lblMoney.Tag.ToString();
                    money["Count"] = value;
                    if (cbMoneyDelete.IsChecked.HasValue && cbMoneyDelete.IsChecked.Value)
                    {
                        money["Delete"] = true;
                    }
                    json["Money"] = money;
                }
            }
            if(tbxExpCount.Text.Length > 0)
            {
                JObject exp = new JObject();
                int value = Ressource.getInt(tbxExpCount.Text, -1);
                if (value > 0)
                {
                    exp["UUID"] = lblExp.Tag.ToString();
                    exp["Count"] = value;
                    exp["Type"] = ((ComboBoxItem)cbxExpType.SelectedItem).Content.ToString();
                    if (cbExpDelete.IsChecked.HasValue && cbExpDelete.IsChecked.Value)
                    {
                        exp["Delete"] = true;
                    }
                    json["Exp"] = exp;
                }
            }
            if (tbxIsLvlCount.Text.Length > 0)
            {
                JObject islvl = new JObject();
                int value = Ressource.getInt(tbxIsLvlCount.Text, -1);
                if (value > 0)
                {
                    islvl["UUID"] = lblIsLvl.Tag.ToString();
                    islvl["Count"] = value;
                    json["IsLevel"] = islvl;
                }
            }
            return json;
        }

        IRequired.Required IRequired.getType()
        {
            return IRequired.Required.Others;
        }

        void IRequired.navigate()
        {
            MainWindow.Instance.MainFrame.Navigate(this);
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

        private void cb_Click(object sender, RoutedEventArgs e)
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

        private void NumericInput(object sender, TextCompositionEventArgs e)
        {
            int result = 0;
            if (!Int32.TryParse(e.Text, out result))
            {
                e.Handled = true;
            }
            else
            {
                if (((TextBox)sender).Text.Length > 8) //100'000'000
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
    }
}

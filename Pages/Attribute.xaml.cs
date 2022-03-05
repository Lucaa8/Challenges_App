using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Newtonsoft.Json.Linq;

namespace Challenges_App.Pages
{
    /// <summary>
    /// Interaction logic for Attribute.xaml
    /// </summary>
    public partial class Attribute : Page
    {
        public enum AttributeType
        {
            GENERIC_ARMOR, GENERIC_ARMOR_TOUGHNESS, GENERIC_ATTACK_DAMAGE, GENERIC_ATTACK_KNOCKBACK, GENERIC_ATTACK_SPEED, GENERIC_FLYING_SPEED, GENERIC_FOLLOW_RANGE, GENERIC_KNOCKBACK_RESISTANCE, GENERIC_LUCK, GENERIC_MAX_HEALTH, GENERIC_MOVEMENT_SPEED, HORSE_JUMP_STRENGTH, ZOMBIE_SPAWN_REINFORCEMENTS
        }
        public enum Operation
        {
            ADD_NUMBER, ADD_SCALAR, MULTIPLY_SCALAR_1
        }
        public enum Slot
        {
            AUCUN, HEAD, CHEST, LEGS, FEET, HAND, OFF_HAND
        }

        private Item parent;
        private Dictionary<String, Attr> attributes = new Dictionary<String, Attr>();

        public Attribute(Item parent)
        {
            this.parent = parent;
            InitializeComponent();
            lblTitle.Content = lblTitle.Content.ToString().Replace("[I]", parent.getName());
            foreach(AttributeType type in Enum.GetValues(typeof(AttributeType))){
                cbxAttribute.Items.Add(type.ToString());
            }
            foreach (Operation op in Enum.GetValues(typeof(Operation))){
                cbxOperation.Items.Add(op.ToString());
            }
            foreach (Slot slot in Enum.GetValues(typeof(Slot))){
                cbxSlot.Items.Add(slot.ToString());
            }
            cbxAttribute.SelectedIndex = 0;
            cbxOperation.SelectedIndex = 0;
            cbxSlot.SelectedIndex = 0;
            updateBox();
            btnAdd.addMouseClick(c =>
            {
                if (tbxName.Text.Length > 0)
                {
                    double value;
                    if(Double.TryParse(tbxValue.Text, out value))
                    {
                        if(value > 0)
                        {
                            addAttribute(tbxName.Text, cbxAttribute.SelectedItem.ToString(), value, cbxOperation.SelectedItem.ToString(), cbxSlot.SelectedItem.ToString());
                        }
                        else
                        {
                            MessageBox.Show("La valeur doit être positive.", "Error value not positive");
                        }
                    }
                    else
                    {
                        MessageBox.Show("La valeur doit appartenir aux réels.", "Error value not reel");
                    }
                }
                else
                {
                    MessageBox.Show("Le nom ne peut pas être vide.", "Error name is empty");
                }
            });
            btnRem.addMouseClick(c =>
            {
                if(boxAttributes.SelectedIndex != -1)
                {
                    remAttribute(boxAttributes.SelectedItem.ToString());
                }
            });
        }

        public Attribute(Item parent, JArray json) : this(parent)
        {
            foreach(JObject attr in json)
            {
                Attr a = new Attr(attr);
                attributes.Add(a.getName(), a);
            }
            updateBox();
        }

        public JArray toJson()
        {
            JArray jarr = new JArray();
            foreach(Attr a in getAttributes())
            {
                jarr.Add(a.toJson());
            }
            return jarr;
        }

        private void addAttribute(String name, String attribute, double value, String operation, String slot)
        {
            if (name.Equals("Nouvel attribut...")) return;
            if (attributes.ContainsKey(name))
            {
                if (MessageBox.Show("Un attribut avec le même nom existe déjà.\nEn faisant ça tu vas écraser son contenu.\nContinuer?", "Error name already exists", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    attributes.Remove(name);
                }
                else return;
            }
            attributes.Add(name, new Attr(fromString(attribute), name, value, operation, slot));
            updateBox();
        }

        private Boolean remAttribute(String name)
        {
            if (attributes.Remove(name))
            {
                updateBox();
                return true;
            }
            return false;
        }

        private List<Attr> getAttributes()
        {
            return new List<Attr>(attributes.Values);
        }

        private void updateBox()
        {
            boxAttributes.Items.Clear();
            boxAttributes.Items.Add("Nouvel attribut...");
            foreach(String attr in attributes.Keys)
            {
                boxAttributes.Items.Add(attr);
            }
        }

        private AttributeType fromString(String attribute)
        {
            AttributeType type;
            if (Enum.TryParse(attribute, out type))
            {
                return type;
            }
            else return AttributeType.GENERIC_ARMOR;
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

        private void boxDbl_Click(object sender, MouseButtonEventArgs e)
        {
            if (boxAttributes.SelectedIndex == -1) return;
            if (boxAttributes.SelectedIndex == 0)
            {
                tbxName.Text = "";
                tbxName.IsEnabled = true;
                cbxAttribute.SelectedIndex = 0;
                tbxValue.Text = "";
                cbxOperation.SelectedIndex = 0;
                cbxSlot.SelectedIndex = 0;
            }
            else
            {
                String attrib = boxAttributes.SelectedItem as String;
                if (attrib != null && attributes.ContainsKey(attrib))
                {
                    Attr a = attributes[attrib];
                    tbxName.Text = a.getName();
                    tbxName.IsEnabled = false;
                    cbxAttribute.SelectedItem = a.getAttribute().ToString();
                    tbxValue.Text = ""+a.getValue();
                    cbxOperation.SelectedItem = a.getOperation().ToString();
                    cbxSlot.SelectedItem = a.getSlot().ToString();
                }
            }
        }

        private class Attr
        {
            private AttributeType attribute = AttributeType.GENERIC_ARMOR;
            private String name = "emptyName";
            private double value = 1.0;
            private Operation operation = Operation.ADD_NUMBER;
            private Slot slot = Slot.AUCUN;
            public Attr(AttributeType attribute, String name, double value, String operation, String slot)
            {
                this.attribute = attribute;
                if (name == null || name.Length == 0)
                {
                    this.name = attribute.ToString();
                }
                else
                {
                    this.name = name;
                }
                this.value = value;
                if(!Enum.TryParse(operation, out this.operation))
                {
                    this.operation = Operation.ADD_NUMBER;
                }
                if(slot!=null)
                {
                    Enum.TryParse(slot, out this.slot);
                }
            }
            public Attr(JObject json)
            {
                if (json.ContainsKey("Name"))
                {
                    name = json.Value<String>("Name");
                }
                if (json.ContainsKey("Attribute"))
                {
                    Enum.TryParse(json.Value<String>("Attribute"), out attribute);
                }
                if (json.ContainsKey("Value"))
                {
                    value = json.Value<Double>("Value");
                }
                if (json.ContainsKey("Operation"))
                {
                    Enum.TryParse(json.Value<String>("Operation"), out operation);
                }
                if (json.ContainsKey("Slot"))
                {
                    Enum.TryParse(json.Value<String>("Slot"), out slot);
                }
            }
            public String getName()
            {
                return name;
            }
            public AttributeType getAttribute()
            {
                return attribute;
            }
            public double getValue()
            {
                return value;
            }
            public Operation getOperation()
            {
                return operation;
            }
            public Slot getSlot()
            {
                return slot;
            }
            public override bool Equals(Object obj)
            {
                if (obj == null || !(obj is Attr)) return false;
                return (obj as Attr).getName().Equals(name);
            }
            public override int GetHashCode()
            {
                return HashCode.Combine(name);
            }
            public JObject toJson()
            {
                JObject json = new JObject();
                json["Name"] = name;
                json["Attribute"] = attribute.ToString();
                json["Value"] = value;
                json["Operation"] = operation.ToString();
                if (slot != Slot.AUCUN)
                {
                    json["Slot"] = slot.ToString();
                }
                return json;
            }
        }
    }
}

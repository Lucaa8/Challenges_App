using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Challenges_App.Pages
{
    /// <summary>
    /// Interaction logic for ItemsManager.xaml
    /// </summary>
    public partial class ItemsManager : Page
    {
        private Dictionary<String, Item> entries = new Dictionary<String, Item>();
        public ItemsManager()
        {
            InitializeComponent();
            btnRemove.addMouseClick((c) =>
            {
                if (boxItems.SelectedItems != null && boxItems.SelectedItems.Count > 0)
                {
                    List<String> toRemove = new List<String>();
                    foreach(String s in boxItems.SelectedItems)
                    {
                        toRemove.Add(s);
                    }
                    foreach(String s in toRemove)
                    {
                        if (!removeItem(s))
                        {
                            MessageBox.Show("Cet item n'existe pas!\n" + s, "Internal error already deleted");
                        }
                    }
                    toRemove.Clear();
                }
                else
                {
                    MessageBox.Show("Aucun item n'est séléctionné ?", "Error no selection");
                }
            });
            btnCreate.addMouseClick((c) =>
            {
                if (tbxNewItem.Text.Length > 0 && !tbxNewItem.Text.Equals((String)tbxNewItem.Tag))
                {
                    if(addItem(new Item(tbxNewItem.Text))){
                        MainWindow.Instance.MainFrame.Navigate(entries[tbxNewItem.Text]);
                        tbxNewItem.Text = "";
                        tbx_LostFocus(tbxNewItem, null);
                    }
                    else
                    {
                        MessageBox.Show("Un item avec le même nom existe déjà.", "Error item already exists");
                    }
                }
                else
                {
                    MessageBox.Show("Impossible de créer un item sans nom.", "Error item no name");
                }
            });
        }

        public Boolean addItem(Item item)
        {
            if (!entries.ContainsKey(item.getName()))
            {
                entries.Add(item.getName(), item);
                updateBox();
                return true;
            }
            return false;
        }

        //Remplace si existant
        public void forceAdd(Item item)
        {
            removeItem(item);
            addItem(item);
        }

        public Boolean removeItem(Item item)
        {
            if (entries.ContainsKey(item.getName()))
            {
                entries.Remove(item.getName());
                updateBox();
                return true;
            }
            return false;
        }

        public Boolean removeItem(String name)
        {
            return removeItem(getItem(name));
        }

        public List<String> getItems()
        {
            return new List<String>(entries.Keys);
        }

        public Item? getItem(String item)
        {
            foreach (String s in entries.Keys)
            {
                if (s.Equals(item))
                {
                    return entries[s];
                }
            }
            return null;
        }

        private void updateBox()
        {
            boxItems.Items.Clear();
            foreach (String s in entries.Keys)
            {
                if (s.ToLower().Contains(tbxResearch.Text.ToLower()) || tbxResearch.Text.Equals("Rechercher..."))
                {
                    boxItems.Items.Add(s);
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

        private void boxItems_Drop(object sender, DragEventArgs e)
        {
            foreach (String path in (string[])e.Data.GetData(DataFormats.FileDrop, false)){
                try
                {
                    if (Path.HasExtension(path) && Path.GetExtension(path).ToLower().Equals(".json"))
                    {
                        String file = Path.GetFileNameWithoutExtension(path);
                        if (!addItem(Item.fromFile(path)))
                        {
                            MessageBox.Show("Un item avec le même nom est-il déjà chargé ?\n"+file, "File already exists");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Uniquement les fichiers JSON sont reconnus!\n"+path, "Wrong file type");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Impossible de retrouver le fichier drop.\n\nErreur:\n" + ex.Message, "Erreur de fichiers.");
                }
            }
        }

        private void tbxResearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            updateBox();
        }

        private void tbx_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tbx = (TextBox)sender;
            if (tbx.Text.Length == 0)
            {
                tbx.Text = (String)tbx.Tag;
                tbx.Foreground = Brushes.DarkSlateGray;
            }
        }

        private void tbx_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tbx = (TextBox)sender;
            if (tbx.Text.Equals((String)tbx.Tag) && tbx.Foreground == Brushes.DarkSlateGray)
            {
                tbx.Text = "";
                tbx.Foreground = Brushes.Black;
            }
        }

        private void boxDbClick(object sender, MouseButtonEventArgs e)
        {
            if (boxItems.SelectedIndex == -1) return;
            Item i = getItem(boxItems.SelectedItem.ToString());
            if (i != null)
            {
                MainWindow.Instance.MainFrame.Navigate(i);
            }
        }
    }
}

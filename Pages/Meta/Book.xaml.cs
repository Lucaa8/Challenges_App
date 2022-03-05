using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Challenges_App.Pages.Meta
{
    /// <summary>
    /// Interaction logic for Book.xaml
    /// </summary>
    public partial class Book : Page, IMeta
    {
        private Item parent;
        private int page = 0;
        private List<String> pages = new List<String>();
        public Book(Item parent)
        {
            this.parent = parent;
            InitializeComponent();
            lblTitle.Content = lblTitle.Content.ToString().Replace("[I]",this.parent.getName());
            Ressource.fillRect(rectRemove, Ressource.getImage(Files.ResxFile.RemButton));
            pages.Add("");
            FormatText();
        }

        public Book(Item parent, JObject json) : this(parent)
        {
            if (json.ContainsKey("Author"))
            {
                tbxAuthor.Text = json.Value<String>("Author");
            }
            if (json.ContainsKey("Title"))
            {
                tbxBookTitle.Text = json.Value<String>("Title");
            }
            if (json.ContainsKey("Generation"))
            {
                foreach(ComboBoxItem i in cbxGeneration.Items)
                {
                    if (i.Content.ToString().Equals(json.Value<String>("Generation")))
                    {
                        cbxGeneration.SelectedItem = i;
                        break;
                    }
                }
            }
            if (json.ContainsKey("Pages"))
            {
                JObject jpages = json["Pages"] as JObject;
                Dictionary<String, String> pages = jpages.ToObject<Dictionary<String, String>>();
                foreach(String page in pages.Keys)
                {
                    Int32 index;
                    if(Int32.TryParse(page, out index))
                    {
                        this.pages.Insert(index, pages[page].Replace("\n", "\r\n")+"\r\n");
                    }
                }
                if (this.pages.Count > pages.Count)//Le premier insert lorsque la liste est vide rajoute 2 entrées au lieu d'une :v
                {
                    this.pages.RemoveAt(this.pages.Count - 1);
                }
                lblPage.Content = "(" + (page + 1) + "/" + this.pages.Count + ")";
                FormatText();
            }
        }

        IMeta.Meta IMeta.getMeta()
        {
            return IMeta.Meta.Book;
        }

        void IMeta.navigate()
        {
            MainWindow.Instance.MainFrame.Navigate(this);
        }

        JObject IMeta.toJson()
        {
            JObject json = new JObject();
            if(tbxAuthor.Text.Length > 0)
            {
                json["Author"] = tbxAuthor.Text;
            }
            if(tbxBookTitle.Text.Length > 0)
            {
                json["Title"] = tbxBookTitle.Text;
            }
            if(cbxGeneration.SelectedIndex != -1)
            {
                json["Generation"] = ((ComboBoxItem)cbxGeneration.SelectedItem).Content.ToString();
            }
            JObject p = new JObject();
            for (int i = 0; i < pages.Count; i++)
            {
                if (i == page && isFormatChecked())
                {
                    cbFormat.IsChecked = false;
                    cbFormat_Click(cbFormat, null);
                }
                savePage();
                String pa = pages[i].Replace("\r", "");
                p[i + ""] = pa.Substring(0, pa.Length - 1);
            }
            json["Pages"] = p;
            return json;
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

        private void FormatText()
        {
            TextRange range = new TextRange(rtbPages.Document.ContentStart, rtbPages.Document.ContentEnd);
            if (isFormatChecked())
            {
                String full = range.Text;
                range.Text = "";
                String current = "";
                char color = '\0';
                String format = "";
                for (int i = 0; i < full.Length; i++)
                {
                    char c = full[i];
                    if (c == '§'&&full.Length>i+1)
                    {
                        char code = full[i+1];
                        if ((code >= 48 && code <= 57) || (code>=97 && code <=102))//Couleur (0-9,a-f)
                        {
                            if(current.Length>0)addText(current, format, color);
                            current = "";
                            color = code;
                            format = "";
                        }
                        else if(code>=107&&code<=111)//Format (k-o)
                        {
                            if (color == '\0'&&current.Length>0)
                            {
                                addText(current, format, color);
                                current = "";
                            }
                            format += code;
                        }
                        else if (code == 'r')//Reset
                        {
                            if (current.Length > 0) addText(current, format, color);
                            current = "";
                            color = '\0';
                            format = "";
                        }
                        else
                        {
                            current += c+""+code;
                        }
                        i++;
                    }
                    else
                    {
                        current += c+"";
                    }
                }
                if (current.Length>0)
                {
                    addText(current, format, color);
                }
            }
            else
            {
                range.Text = pages[page];
                range.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
                range.ApplyPropertyValue(Inline.TextDecorationsProperty, new TextDecorationCollection());
            }
        }

        private void addText(String text, String format, char color)
        {
            text = text.Replace("\n", ""); //Sinon la 1ere ligne ne s'affiche pas correctement... c# va savoir....
            TextRange txt = new TextRange(rtbPages.Document.ContentEnd, rtbPages.Document.ContentEnd);
            txt.Text = text;
            txt.ApplyPropertyValue(TextElement.ForegroundProperty, color=='\0'?Brushes.Black:LogManager.getColorByCode(color+""));
            TextDecorationCollection deco = new TextDecorationCollection();
            if(format.Contains('m'))deco.Add(TextDecorations.Strikethrough);
            if(format.Contains('n'))deco.Add(TextDecorations.Underline);
            txt.ApplyPropertyValue(Inline.TextDecorationsProperty, deco);
            txt.ApplyPropertyValue(TextElement.FontFamilyProperty, format.Contains('l') ? format.Contains('o') ? MainWindow.MC_Bold_Italic : MainWindow.MC_Bold : format.Contains('o') ? MainWindow.MC_Italic : MainWindow.MC_Normal);
        }

        private void savePage()
        {
            pages[page] = new TextRange(rtbPages.Document.ContentStart, rtbPages.Document.ContentEnd).Text;
        }

        private void btnNext_MouseEnter(object sender, MouseEventArgs e)
        {
            ScaleTransform st = (ScaleTransform)btnNextPage.RenderTransform;
            st.ScaleX = 0.9;
            st.ScaleY = 0.9;
            btnNextPage.Margin = new Thickness(btnNextPage.Margin.Left - 3, btnNextPage.Margin.Top - 1.5, 0, 0);
        }

        private void btnNext_MouseLeave(object sender, MouseEventArgs e)
        {
            ScaleTransform st = (ScaleTransform)btnNextPage.RenderTransform;
            st.ScaleX = 0.8;
            st.ScaleY = 0.8;
            btnNextPage.Margin = new Thickness(btnNextPage.Margin.Left + 3, btnNextPage.Margin.Top + 1.5, 0, 0);
        }

        private void btnPrev_MouseEnter(object sender, MouseEventArgs e)
        {
            ScaleTransform st = (ScaleTransform)btnPrevPage.RenderTransform;
            st.ScaleX = 0.9;
            st.ScaleY = 0.9;
            btnPrevPage.Margin = new Thickness(btnPrevPage.Margin.Left - 1.5, btnPrevPage.Margin.Top - 1.5, 0, 0);
        }

        private void btnPrev_MouseLeave(object sender, MouseEventArgs e)
        {
            ScaleTransform st = (ScaleTransform)btnPrevPage.RenderTransform;
            st.ScaleX = 0.8;
            st.ScaleY = 0.8;
            btnPrevPage.Margin = new Thickness(btnPrevPage.Margin.Left + 1.5, btnPrevPage.Margin.Top + 1.5, 0, 0);
        }

        private void btnNext_Click(object sender, MouseButtonEventArgs e)
        {
            if (!isFormatChecked())
            {
                savePage();
                page++;
                if (pages.Count <= page)
                {
                    pages.Add("");
                }
                lblPage.Content = "(" + (page + 1) + "/" + pages.Count + ")";
                FormatText();
            }
            else
            {
                MessageBox.Show("Impossible de changer la page actuelle lorsque le format est activé!", "Error format enabled");
            }
        }

        private void btnPrev_Click(object sender, MouseButtonEventArgs e)
        {
            if (page > 0)
            {
                if (!isFormatChecked())
                {
                    savePage();
                    page--;
                    lblPage.Content = "(" + (page + 1) + "/" + pages.Count + ")";
                    FormatText();
                }
                else
                {
                    MessageBox.Show("Impossible de changer la page actuelle lorsque le format est activé!", "Error format enabled");
                }
            }
        }

        private void cbFormat_Click(object sender, RoutedEventArgs e)
        {
            if (isFormatChecked())
            {
                rtbPages.Focusable = false;
                savePage();
            }
            else
            {
                rtbPages.Focusable = true;
            }
            FormatText();
        }

        private Boolean isFormatChecked()
        {
            return cbFormat.IsChecked.HasValue && cbFormat.IsChecked.Value;
        }

        private void Remove_Enter(object sender, MouseEventArgs e)
        {
            rectRemove.Width += 5;
            rectRemove.Height += 5;
            rectRemove.Margin = new Thickness(rectRemove.Margin.Left - 2.5, rectRemove.Margin.Top - 2.5, 0, 0);
        }

        private void Remove_Leave(object sender, MouseEventArgs e)
        {
            rectRemove.Width -= 5;
            rectRemove.Height -= 5;
            rectRemove.Margin = new Thickness(rectRemove.Margin.Left + 2.5, rectRemove.Margin.Top + 2.5, 0, 0);
        }

        private void Remove_Click(object sender, MouseButtonEventArgs e)
        {
            if (pages.Count > 1)
            {
                pages.RemoveAt(page);
                if (page == pages.Count)
                {
                    page--;
                }
                lblPage.Content = "(" + (page + 1) + "/" + pages.Count + ")";
                cbFormat.IsChecked = false;
                cbFormat_Click(cbFormat, null);
            }
            else
            {
                MessageBox.Show("La meta ne peut pas contenir 0 pages.", "Error not enough pages");
            }
        }
    }
}

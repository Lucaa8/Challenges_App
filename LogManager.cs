using System;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Challenges_App
{
    public class LogManager
    {
        public enum LogType
        {
            INFO, WARN, ERROR, IN, OUT
        }
        RichTextBox rtb;

        public LogManager(RichTextBox rtb)
        {
            this.rtb = rtb;
            rtb.IsReadOnly = true;
            rtb.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
        }

        public void addText(LogType type, String text)
        {
            addText(type, text, type==LogType.INFO?Brushes.Black:type==LogType.WARN?Brushes.OrangeRed:type==LogType.ERROR?Brushes.Red:type==LogType.IN?Brushes.Green:Brushes.Blue);
        }
        public void addText(LogType type, String text, Brush color)
        {
            Ressource.synchronize(()=>AppendText("[" + Ressource.getTimeNow() + "] [" + type + "] " + text + "\n", color));
        }
        //Fonction permettant de rajouter du texte coloré dans une RichTextBox //!!\\ Dispatcher.BeginInvoke pour thread pas prioritaires
        //Source: https://www.codeproject.com/Questions/41607/WPF-RichTextBox-Appending-Colored-text
        private void AppendText(String Text, Brush Color)
        {
            TextRange range = new TextRange(rtb.Document.ContentEnd, rtb.Document.ContentEnd);
            range.Text = Text;
            range.ApplyPropertyValue(TextElement.ForegroundProperty, Color);
            rtb.ScrollToEnd();
        }

        //Donne une certaine couleur en fonction d'un caractère (0-9/a-f)
        public static Brush getColorByCode(String colorCode)
        {
            //Noir: Black
            if (colorCode == "0") return new SolidColorBrush(Color.FromRgb(0, 0, 0));
            //Bleu foncé: DarkBlue
            if (colorCode == "1") return new SolidColorBrush(Color.FromRgb(0, 0, 170));
            //Bleu foncé: DarkGreen
            if (colorCode == "2") return new SolidColorBrush(Color.FromRgb(0, 170, 0));
            //Aqua foncé: DarkAqua
            if (colorCode == "3") return new SolidColorBrush(Color.FromRgb(0, 170, 170));
            //Rouge foncé: DarkRed
            if (colorCode == "4") return new SolidColorBrush(Color.FromRgb(170, 0, 0));
            //Violet foncé: DarkPurple
            if (colorCode == "5") return new SolidColorBrush(Color.FromRgb(170, 0, 170));
            //Doré: Gold
            if (colorCode == "6") return new SolidColorBrush(Color.FromRgb(255, 170, 0));
            //Gris: Gray
            if (colorCode == "7") return new SolidColorBrush(Color.FromRgb(170, 170, 170));
            //Gris foncé: DarkGray
            if (colorCode == "8") return new SolidColorBrush(Color.FromRgb(85, 85, 85));
            //Bleu: Blue
            if (colorCode == "9") return new SolidColorBrush(Color.FromRgb(85, 85, 255));
            //Vert: Green
            if (colorCode == "a") return new SolidColorBrush(Color.FromRgb(85, 255, 85));
            //Aqua: Aqua
            if (colorCode == "b") return new SolidColorBrush(Color.FromRgb(85, 255, 255));
            //Rouge: Red
            if (colorCode == "c") return new SolidColorBrush(Color.FromRgb(255, 85, 85));
            //Violet clair: LightPurple
            if (colorCode == "d") return new SolidColorBrush(Color.FromRgb(255, 85, 255));
            //Jaune: Yellow
            if (colorCode == "e") return new SolidColorBrush(Color.FromRgb(190, 190, 0));
            //Blanc: White
            if (colorCode == "f") return new SolidColorBrush(Color.FromRgb(255, 255, 255));

            //Retourne noir par défaut si il ne trouve pas de couleur attribuée
            return Brushes.Black;
        }
    }
}

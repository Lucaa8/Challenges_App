using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Challenges_App.Packet;

namespace Challenges_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }
        public SocketManager? packetManager;
        public Pages.MainMenu? menu;
        public static FontFamily MC_Normal = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#Minecraft");
        public static FontFamily MC_Bold = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/MC_Bold/#Minecraft");
        public static FontFamily MC_Italic = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/MC_Italic/#Minecraft");
        public static FontFamily MC_Bold_Italic = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/MC_Bold_Italic/#Minecraft");
        private static string LastIPFile = "config";
        public static Version v = new Version(1, 3, 1);
        public MainWindow()
        {
            Ressource.initTypes();
            InitializeComponent();

            //true uniquement lorsque l'application doit être débug (se passe de l'étape de connexion au serveur. Permet d'importer des items, challenges, etc.. localement.
            //(Suppression désactivée mais possibilité d'importer le meme challenge en le glissant dans la boxes + ecraser)
            init(true);
            //TODO tester sans le mode debug sur le serv local dans les challenges et créer des leather/iron armor avec et sans trim.
        }

        private void onClose(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (packetManager != null && packetManager.isOnline)
            {
                if(MessageBox.Show("Es-tu sûr de vouloir te déconnecter?\nTout ce qui n'a pas été explicitiment envoyé au serveur sera perdu.", "Disconnecting...", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    packetManager.sendStop("Closing app");
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        public void init(Boolean debug)
        {
            Instance = this;
            packetManager = null;
            if (!File.Exists(LastIPFile))
            {
                File.Create(LastIPFile).Close();
                updateLastIP("localhost:25575");
            }
            if (debug)
            {
                menu = new Pages.MainMenu();
                MainFrame.Navigate(menu);
            }
            else
            {
                string? lastIp;
                using (StreamReader sr = File.OpenText(LastIPFile))
                {
                    lastIp = sr.ReadLine();
                }
                menu = null;
                MainFrame.Navigate(new Pages.Connect(lastIp));
            }
        }

        public void updateLastIP(string lastip)
        {
            using (FileStream fs = File.OpenWrite(LastIPFile))
            {
                fs.SetLength(0); //clear old content
                byte[] info = new UTF8Encoding(true).GetBytes(lastip);
                fs.Write(info, 0, info.Length);
            }
        }

    }
}

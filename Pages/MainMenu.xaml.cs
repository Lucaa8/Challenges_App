using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Challenges_App.Packet;
using Challenges_App.Packet.Packets;
using System.Threading.Tasks;

namespace Challenges_App.Pages
{
    /// <summary>
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Page
    {
        private LogManager log;
        private Dictionary<String, CategorySession> categorySessions = new Dictionary<String, CategorySession>();
        private Dictionary<String, ChallengeSession> challengeSessions = new Dictionary<String, ChallengeSession>();
        private ItemsManager itemsManager;
        private Boolean challengesState;
        public Boolean ChallengesState
        {
            set { 
                challengesState = value;
                if (value)
                {
                    lblChallengesStatut.Content = "Activés";
                    lblChallengesStatut.Foreground = Brushes.Green;
                }
                else
                {
                    lblChallengesStatut.Content = "Désactivés";
                    lblChallengesStatut.Foreground = Brushes.Red;
                }
            }
            get { return challengesState; }
        }
        private Dictionary<String, String> categories = new Dictionary<String, String>();
        private Dictionary<String, String> challenges = new Dictionary<String, String>();
        public MainMenu()
        {
            InitializeComponent();
            log = new LogManager(rtbLogs);
            SocketManager? socketManager = MainWindow.Instance.packetManager;
            if (socketManager != null)
            {
                log.addText(LogManager.LogType.INFO, "La connexion avec l'hôte " + socketManager.getAdress() + " a été établie.");
                sendCallback(new ChallengeStatePacket(ChallengeStatePacket.Type.INFO, false), (packet) =>
                {
                    ChallengesState = ((ChallengeStatePacket)packet).getState();
                });
                sendCallback(new ChallengesListPacket(), (packet) =>
                {
                    ChallengesListPacket p = (ChallengesListPacket)packet;
                    foreach (KeyValuePair<String, String> c in p.getChallenges())
                    {
                        challenges.Add(c.Key, c.Value);
                    }
                    foreach (KeyValuePair<String, String> c in p.getCategories())
                    {
                        categories.Add(c.Key, c.Value);
                    }
                    lblCategoriesTitle.Content = p.getCategories().Count + " Catégories";
                    lblChallengesTitle.Content = p.getChallenges().Count + " Challenges";
                    tbxResearchField_TextChanged(tbxResearchField, null);
                });
                serverInfo(socketManager.getAdress());
            }
            else
            {
                log.addText(LogManager.LogType.ERROR, "La connexion avec l'hôte distant a échoué! Impossible de communiquer. Mode Debug.");
            }
            itemsManager = new ItemsManager();
            btnToggleChallengesState.addMouseClick((c) =>
            {
                btnToggleChallengesState.Enabled = false;
                toggleChallengesState((state) => //Enable button only after the server received and treated packet so client can't flood. 
                {
                    btnToggleChallengesState.Enabled = true;
                });
            });
            btnItemsManager.addMouseClick((c) =>
            {
                MainWindow.Instance.MainFrame.Navigate(itemsManager);
            });
        }

        public ItemsManager getItemManager()
        {
            return itemsManager;
        }

        public LogManager getLogger()
        {
            return log;
        }

        private SocketManager? canSend()
        {
            SocketManager? socket = MainWindow.Instance.packetManager;
            return (socket != null && socket.isOnline && socket.isSessionValid)?socket:null;
        }

        public Boolean send(IPacket packet)
        {
            return sendCallback(packet, null);
        }

        public Boolean sendCallback(IPacket packet, CallbackManager.Handle? callback)
        {
            SocketManager? socket = canSend();
            if (socket!=null)
            {
                socket.queuePacket(packet, callback);
                return true;
            }
            return false;
        }

        public void addCategory(String uuid, String name, CategorySession? session)
        {
            removeCategory(uuid);
            categories.Add(uuid, name);
            if (session != null)
            {
                categorySessions.Add(uuid, session);
                log.addText(LogManager.LogType.INFO, "Création d'une session pour la catégorie " + session.getName());
            }
        }

        public void changeCategoryName(String uuid, String newName)
        {
            if (categories.ContainsKey(uuid) && categorySessions.ContainsKey(uuid))
            {
                categories[uuid] = newName;
                tbxResearchField_TextChanged(tbxResearchField, null);
            }
        }

        public void removeCategory(String uuid)
        {
            if (categories.ContainsKey(uuid))
            {
                categories.Remove(uuid);
            }
            if (categorySessions.ContainsKey(uuid))
            {
                categorySessions.Remove(uuid);
            }
            tbxResearchField_TextChanged(tbxResearchField, null);
        }

        public String? getCategory(String uuid)
        {
            if(categories.ContainsKey(uuid))return categories[uuid];
            return null;
        }

        public Dictionary<String,String> getCategories()
        {
            return categories;
        }

        public void addChallenge(String uuid, String name, ChallengeSession? session)
        {
            removeChallenge(uuid);
            challenges.Add(uuid, name);
            if (session != null)
            {
                challengeSessions.Add(uuid, session);
                log.addText(LogManager.LogType.INFO, "Création d'une session pour le challenge " + session.getName());
            }
        }

        public void changeChallengeName(String uuid, String newName)
        {
            if (challenges.ContainsKey(uuid) && challengeSessions.ContainsKey(uuid))
            {
                challenges[uuid] = newName;
                tbxResearchField_TextChanged(tbxResearchField, null);
            }
        }

        public void removeChallenge(String uuid)
        {
            if (challenges.ContainsKey(uuid))
            {
                challenges.Remove(uuid);
            }
            if (challengeSessions.ContainsKey(uuid))
            {
                challengeSessions.Remove(uuid);
            }
            tbxResearchField_TextChanged(tbxResearchField, null);
        }

        public String? getChallenge(String uuid)
        {
            if (challenges.ContainsKey(uuid)) return challenges[uuid];
            return null;
        }

        public Dictionary<String, String> getChallenges()
        {
            return challenges;
        }

        private void tbxResearchField_TextChanged(object sender, TextChangedEventArgs e)
        {
            boxCategories.Items.Clear();
            boxChallenges.Items.Clear();
            boxCategories.Items.Add("Nouvelle catégorie...");
            boxChallenges.Items.Add("Nouveau challenge...");
            foreach (String s in categories.Keys)
            {
                if (categories[s].ToLower().Contains(tbxResearchField.Text.ToLower())||tbxResearchField.Text.Equals("Texte à trouver..."))
                {
                    ListBoxItem lb = new ListBoxItem();
                    lb.Content = categories[s];
                    lb.Tag = s;
                    boxCategories.Items.Add(lb);
                }
            }
            foreach(String s in challenges.Keys)
            {
                if (challenges[s].ToLower().Contains(tbxResearchField.Text.ToLower())||tbxResearchField.Text.Equals("Texte à trouver..."))
                {
                    ListBoxItem lb = new ListBoxItem();
                    lb.Content = challenges[s];
                    lb.Tag = s;
                    boxChallenges.Items.Add(lb);
                }
            }
        }

        private void tbx_Research_Lostfocus(object sender, RoutedEventArgs e)
        {
            if (tbxResearchField.Text.Length == 0)
            {
                tbxResearchField.Text = "Texte à trouver...";
                tbxResearchField.Foreground = Brushes.DarkSlateGray;
            }
        }

        private void tbx_Research_Gotfocus(object sender, RoutedEventArgs e)
        {
            if (tbxResearchField.Text.Equals("Texte à trouver...")&&tbxResearchField.Foreground==Brushes.DarkSlateGray)
            {
                tbxResearchField.Text = "";
                tbxResearchField.Foreground = Brushes.Black;
            }
        }

        private void Box_Lostfocus(object sender, RoutedEventArgs e)
        {
            ListBox box = (ListBox)sender;
            box.SelectedIndex = -1;
        }

        private void BoxCat_DblClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (boxCategories.SelectedIndex == -1) return;
            if (boxCategories.SelectedIndex == 0)
            {
                String uuid = Guid.NewGuid().ToString();
                //Ajoute une nouvelle session vierge
                addCategory(uuid, "new", new CategorySession(uuid));
                tbxResearchField_TextChanged(tbxResearchField, null);
            }
            else
            {
                ListBoxItem selected = (ListBoxItem)boxCategories.SelectedItem;
                String uuid = selected.Tag.ToString();
                if (categories.ContainsKey(uuid))
                {
                    if(e.ChangedButton == System.Windows.Input.MouseButton.Right)//delete
                    {
                        if(MessageBox.Show("Supprimer la catégorie '" + uuid + ":" + selected.Content.ToString() + "' ?\nCette action est irréversible.", "Confirm delete", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                        {
                            sendCallback(new InfoRequestPacket(uuid, IPacket.Target.CATEGORY), (packet) =>
                            {
                                if (((InfoRequestPacket)packet).doesExist())
                                {
                                    if (!ChallengesState)
                                    {
                                        sendCallback(new DeleteRequestPacket(uuid, IPacket.Target.CATEGORY), (packet) =>
                                        {
                                            DeleteRequestPacket p = (DeleteRequestPacket)packet;
                                            if (!p.withSuccess())
                                            {
                                                log.addText(LogManager.LogType.ERROR, "La catégorie n'a pas pu être supprimée!");
                                                log.addText(LogManager.LogType.ERROR, "Erreur: " + p.getErrorMsg());
                                            }
                                            else
                                            {
                                                log.addText(LogManager.LogType.INFO, "La catégorie a bien été supprimée (client+server)");
                                                removeCategory(uuid);
                                            }
                                        });
                                    }
                                    else
                                    {
                                        log.addText(LogManager.LogType.ERROR, "Les challenges doivent être désactivés pour que cette action soit autorisée.");
                                    }
                                }
                                else
                                {
                                    log.addText(LogManager.LogType.INFO, "La catégorie a bien été supprimée (client)");
                                    removeCategory(uuid);
                                }
                            });
                        }
                    }
                    else//add
                    {
                        if (!categorySessions.ContainsKey(uuid))
                        {
                            //Fetch le serveur pour récupérer la catégorie
                            sendCallback(new InfoRequestPacket(uuid, IPacket.Target.CATEGORY), (packet) =>
                            {
                                InfoRequestPacket info = (InfoRequestPacket)packet;
                                if (info.doesExist())
                                {
                                    CategorySession session = new CategorySession(info.getJson());
                                    addCategory(session.getUUID(), session.getName(), session);
                                    tbxResearchField_TextChanged(tbxResearchField, null);
                                    MainWindow.Instance.MainFrame.Navigate(categorySessions[uuid]);
                                }
                                else
                                {
                                    log.addText(LogManager.LogType.ERROR, "La catégorie n'existe pas sur le serveur distant.");
                                    return;
                                }
                            });
                        }
                        else
                        {
                            MainWindow.Instance.MainFrame.Navigate(categorySessions[uuid]);
                        }
                    }
                }
            }
        }

        private void BoxCha_DblClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (boxChallenges.SelectedIndex == -1) return;
            if (boxChallenges.SelectedIndex == 0)
            {
                String uuid = Guid.NewGuid().ToString();
                //Ajoute une nouvelle session vierge
                addChallenge(uuid, "new", new ChallengeSession(uuid));
                tbxResearchField_TextChanged(tbxResearchField, null);
            }
            else
            {
                ListBoxItem selected = (ListBoxItem)boxChallenges.SelectedItem;
                String uuid = selected.Tag.ToString();
                if (challenges.ContainsKey(uuid))
                {
                    if(e.ChangedButton == System.Windows.Input.MouseButton.Right)//delete
                    {
                        if (MessageBox.Show("Supprimer le challenge '" + uuid + ":" + selected.Content.ToString() + "' ?\nCette action est irréversible.", "Confirm delete", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                        {
                            sendCallback(new InfoRequestPacket(uuid, IPacket.Target.CHALLENGE), (packet) =>
                            {
                                if (((InfoRequestPacket)packet).doesExist())
                                {
                                    if (!ChallengesState)
                                    {
                                        sendCallback(new DeleteRequestPacket(uuid, IPacket.Target.CHALLENGE), (packet) =>
                                        {
                                            DeleteRequestPacket p = (DeleteRequestPacket)packet;
                                            if (!p.withSuccess())
                                            {
                                                log.addText(LogManager.LogType.ERROR, "Le challenge n'a pas pu être supprimé!");
                                                log.addText(LogManager.LogType.ERROR, "Erreur: " + p.getErrorMsg());
                                            }
                                            else
                                            {
                                                log.addText(LogManager.LogType.INFO, "Le challenge a bien été supprimé (client+server)");
                                                removeChallenge(uuid);
                                            }
                                        });
                                    }
                                    else
                                    {
                                        log.addText(LogManager.LogType.ERROR, "Les challenges doivent être désactivés pour que cette action soit autorisée.");
                                    }
                                }
                                else
                                {
                                    log.addText(LogManager.LogType.INFO, "Le challenge a bien été supprimé (client)");
                                    removeChallenge(uuid);
                                }
                            });
                        }
                    }
                    else//add
                    {
                        if (!challengeSessions.ContainsKey(uuid))
                        {
                            //Fetch le serveur pour récupérer le challenge
                            sendCallback(new InfoRequestPacket(uuid, IPacket.Target.CHALLENGE), (packet) =>
                            {
                                InfoRequestPacket info = (InfoRequestPacket)packet;
                                if (info.doesExist())
                                {
                                    ChallengeSession session = new ChallengeSession(info.getJson());
                                    addChallenge(session.getUUID(), session.getName(), session);
                                    tbxResearchField_TextChanged(tbxResearchField, null);
                                    MainWindow.Instance.MainFrame.Navigate(challengeSessions[uuid]);
                                }
                                else
                                {
                                    log.addText(LogManager.LogType.ERROR, "Le challenge n'existe pas sur le serveur distant.");
                                    return;
                                }
                            });
                        }
                        else
                        {
                            MainWindow.Instance.MainFrame.Navigate(challengeSessions[uuid]);
                        }
                    }
                }
            }
        }

        private void Box_Drop(object sender, DragEventArgs e)
        {
            foreach (String path in (string[])e.Data.GetData(DataFormats.FileDrop, false))
            {
                try
                {
                    if (Path.HasExtension(path) && Path.GetExtension(path).ToLower().Equals(".json"))
                    {
                        Boolean success = false;
                        if (((FrameworkElement)sender).Name.Contains("Categories"))
                        {
                            success = addCategoryFromDrop(path);
                        }
                        else
                        {
                            success = addChallengeFromDrop(path);
                        }
                        if (success)
                        {
                            tbxResearchField_TextChanged(tbxResearchField, null);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Uniquement les fichiers JSON sont reconnus!\n" + path, "Wrong file type");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Impossible de retrouver le fichier drop.\n\nErreur:\n" + ex.Message, "Erreur de fichiers.");
                }
            }
        }

        private Boolean addCategoryFromDrop(String path)
        {
            CategorySession session = CategorySession.fromFile(path);
            if (categories.ContainsKey(session.getUUID()))
            {
                if (MessageBox.Show("Une catégorie avec le même uuid (" + session.getName() + ") est déjà chargée.\nRemplacer?\n!! Le remplacement se fait uniquement sur le client. !!", "File already loaded", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    removeCategory(session.getUUID());
                }
                else return false;
            }
            addCategory(session.getUUID(), session.getName(), session);
            return true;
        }

        private Boolean addChallengeFromDrop(String path)
        {
            ChallengeSession session = ChallengeSession.fromFile(path);
            if (challenges.ContainsKey(session.getUUID()))
            {
                if (MessageBox.Show("Un challenge avec le même uuid (" + session.getName() + ") est déjà chargé.\nRemplacer?\n!! Le remplacement se fait uniquement sur le client. !!", "File already loaded", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    removeChallenge(session.getUUID());
                }
                else return false;
            }
            addChallenge(session.getUUID(), session.getName(), session);
            return true;
        }

        private async void serverInfo(String adr)
        {
            int sec = 0;
            long ping = -1;
            Brush bPing = Brushes.Black;
            Brush bAlive = Brushes.Black;
            int lastChangeSec = -1;
            while(MainWindow.Instance.packetManager!=null && MainWindow.Instance.packetManager.isOnline)
            {
                sec++;
                if (sec == 5)
                {
                    sec = 0;
                    ping = Ressource.getPing(adr);
                    if (ping == -1)
                    {
                        bPing = Brushes.DarkRed;
                    }
                    if (ping >= 0 && ping < 60)
                    {
                        bPing = Brushes.Green;
                    }
                    if (ping >= 60 && ping < 100)
                    {
                        bPing = Brushes.OrangeRed;
                    }
                    if (ping >= 100)
                    {
                        bPing = Brushes.Red;
                    }
                }
                long sent = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()-MainWindow.Instance.packetManager.getLastSent();
                long received = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - MainWindow.Instance.packetManager.getLastReceived();
                if (received < sent)
                {
                    lastChangeSec = (int)received/1000;
                }
                else
                {
                    lastChangeSec = (int)sent/1000;
                }
                if (lastChangeSec == -1)
                {
                    bAlive = Brushes.DarkRed;
                }
                if (lastChangeSec >= 0 && lastChangeSec < 5)
                {
                    bAlive = Brushes.Green;
                }
                if (lastChangeSec >= 5 && lastChangeSec < 15)
                {
                    bAlive = Brushes.Gold;
                }
                if (lastChangeSec >= 15)
                {
                    bAlive = Brushes.Red;
                }
                Ressource.synchronize(() =>
                {
                    lblConnLatency.Content = ping + "ms";
                    lblConnLatency.Foreground = bPing;
                    lblConnKeepAlive.Content = lastChangeSec + "s";
                    lblConnKeepAlive.Foreground = bAlive;
                });
                await Task.Delay(1000);
            }
            Ressource.synchronize(() =>
            {
                lblConnKeepAlive.Content = "-1s";
                lblConnKeepAlive.Foreground = Brushes.Black;
                lblConnLatency.Content = "-1ms";
                lblConnLatency.Foreground = Brushes.Black;
            });
        }

        public void toggleChallengesState(ChallengeStatePacket.Callback? currentServerState)
        {
            SocketManager? socket = canSend();
            if (socket != null)
            {
                socket.queuePacket(new ChallengeStatePacket(ChallengeStatePacket.Type.INFO, false), (check) =>
                {
                    socket.queuePacket(new ChallengeStatePacket(ChallengeStatePacket.Type.UPDATE, !((ChallengeStatePacket)check).getState()), (answer) =>
                    {
                        ChallengesState = ((ChallengeStatePacket)answer).getState();
                        if (currentServerState != null)
                        {
                            currentServerState.Invoke(ChallengesState);
                        }
                    });
                });
            }
        }
    }
}

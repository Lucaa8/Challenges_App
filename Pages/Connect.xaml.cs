﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Challenges_App.Packet;
using Challenges_App.Packet.Packets;

namespace Challenges_App.Pages
{
    /// <summary>
    /// Interaction logic for Connect.xaml
    /// </summary>
    public partial class Connect : Page
    {
        private Boolean animate = true;
        private Version lastVersion = new Version(1,0,0);
        public Connect(string? lastIp)
        {
            InitializeComponent();
            lblAnimation();
            lblInfo.Content = lblInfo.Content.ToString().Replace("{V}", MainWindow.v.ToString());
            tbxAddrIP.Text = lastIp==null?"localhost:25575":lastIp;
            btnAccess.addMouseClick((c) =>
            {
                if(tbxKey.Text.Length == 0)
                {
                    MessageBox.Show("La clé d'accès ne peut pas être vide!", "Error empty key");
                    return;
                }
                String? ip = getIP();
                int port = getPort();
                if (ip == null)
                {
                    MessageBox.Show("L'adresse IP spécifiée n'est pas valide.\nL'adresse doit contenir un port!", "Error empty ip");
                    return;
                }
                if(port == -1)
                {
                    MessageBox.Show("Le port n'a pas été spécifié ou n'est pas un nombre valide.", "Error empty or NaN port");
                    return;
                }
                if(port < 1024 || port > 65535)
                {
                    MessageBox.Show("Le port spécifié n'est pas valide. Il doit se situer entre 1024 et 65535", "Error invalid port");
                    return;
                }
                MainWindow.Instance.updateLastIP(ip + ":" + port);
                SocketManager? manager = MainWindow.Instance.packetManager;
                if(manager == null || !manager.isOnline)
                {
                    manager = new SocketManager(ip, port);
                }
                if (manager.isOnline)
                {
                    manager.queuePacket(new LoginPacket(tbxKey.Text), (packet) =>
                    {
                        LoginPacket lp = (LoginPacket)packet;
                        if (!lp.isAllowed())
                        {
                            MessageBox.Show("Le serveur n'a pas autorisé l'accès à cette ressource.\nRaison: " + lp.getReason(), "Error disallowed", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            manager.isSessionValid = true;
                            animate = false;
                            btnAccess.Animation = false;
                            MainWindow.Instance.menu = new MainMenu();
                            MainWindow.Instance.MainFrame.Navigate(MainWindow.Instance.menu);
                        }
                    });
                }
                MainWindow.Instance.packetManager = manager;
            });
            btnAccess.addMouseEnter(c => btnAccess.Animation = true);
            btnAccess.addMouseLeave(c => btnAccess.Animation = false);
            new Thread(() => checkVersion()).Start();
        }

        private String? getIP()
        {
            if (tbxAddrIP.Text.Length > 0 && tbxAddrIP.Text.Contains(':'))
            {
                String ip = tbxAddrIP.Text.Split(':')[0];
                if (ip.ToLower().Equals("localhost"))
                {
                    return "127.0.0.1";
                }
                else
                {
                    return ip;
                }
            }
            return null;
        }

        private int getPort()
        {
            if (tbxAddrIP.Text.Length > 0 && tbxAddrIP.Text.Contains(':'))
            {
                String p = tbxAddrIP.Text.Split(':')[1];
                int port;
                if(Int32.TryParse(p, out port))
                {
                    return port;
                }
            }
            return -1;
        }

        private void txt_Changed(object sender, TextCompositionEventArgs e)
        {
            if (tbxKey.Text.Length >= 10)
            {
                e.Handled = true;
            }
        }

        private void lbl_Copy(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText("/cadmin editor new");
        }

        private void btnPlus_MouseEnter(object sender, MouseEventArgs e)
        {
            Scale.ScaleX = 1.1;
            Scale.ScaleY = 1.1;
            btnPlus.Margin = new Thickness(btnPlus.Margin.Left, btnPlus.Margin.Top + 2.5, 0, 0);
        }

        private void btnPlus_MouseLeave(object sender, MouseEventArgs e)
        {
            Scale.ScaleX = 1;
            Scale.ScaleY = 1;
            btnPlus.Margin = new Thickness(btnPlus.Margin.Left, btnPlus.Margin.Top - 2.5, 0, 0);
        }

        private void btnPlus_Click(object sender, MouseButtonEventArgs e)
        {
            if (Rotate.Angle > 0)
            {
                Rotate.Angle = -90;
                btnPlus.Margin = new Thickness(btnPlus.Margin.Left, btnPlus.Margin.Top - 80, 0, 0);
                lblAddrIP.Visibility = Visibility.Hidden;
                tbxAddrIP.Visibility = Visibility.Hidden;
            }
            else
            {
                Rotate.Angle = 90;
                btnPlus.Margin = new Thickness(btnPlus.Margin.Left, btnPlus.Margin.Top + 80, 0, 0);
                lblAddrIP.Visibility = Visibility.Visible;
                tbxAddrIP.Visibility = Visibility.Visible;
            }
        }

        private async void lblAnimation()
        {
            Ressource.size(lblSplash, out double w, out double h);
            lblSplashScale.CenterX = w/2;
            lblSplashScale.CenterY = h/2;
            while (animate)
            {
                double scale = (2.2F - Math.Abs(Math.Sin(DateTimeOffset.Now.Millisecond % 1000 / 1000.0f * ((float)Math.PI * 2F)) * 0.1F)) * 100.0 / (w + 32);
                Dispatcher.Invoke(() =>
                {
                    lblSplashScale.ScaleX = scale;
                    lblSplashScale.ScaleY = scale;
                });
                await Task.Delay(10);
            }
        }

        private void lblNewVersion_MouseUp(object sender, MouseButtonEventArgs e)
        {
            String url = "https://github.com/Lucaa8/Challenges_App/releases/tag/csharp_app_" + lastVersion.ToString();
            Ressource.OpenBrowser(url);
        }

        private void checkVersion()
        {
            String? strLastVersion = Ressource.getLastVersion();
            if (strLastVersion == null)
            {
                Ressource.synchronize(() =>
                {
                    lblNewVersion.Content = "Impossible de vérifier si l'application est à jour...";
                    lblNewVersion.Foreground = Brushes.DarkRed;
                });
                return;
            }
            String[] versionValues = strLastVersion.Split('.');
            if (versionValues.Length != 3)
            {
                Ressource.synchronize(() =>
                {
                    lblNewVersion.Content = "Impossible de vérifier si l'application est à jour...";
                    lblNewVersion.Foreground = Brushes.DarkRed;
                });
                return;
            }
            Action sync = () => { };
            lastVersion = new Version(Int32.Parse(versionValues[0]), Int32.Parse(versionValues[1]), Int32.Parse(versionValues[2]));
            if (!lastVersion.Equals(MainWindow.v))
            {
                sync = () =>
                {
                    lblNewVersion.Content = "La nouvelle version " + lastVersion.ToString() + " est disponible! Clique-moi pour t'y rendre";
                    lblNewVersion.Cursor = Cursors.Hand;
                    lblNewVersion.MouseUp += lblNewVersion_MouseUp;
                };
            }
            else
            {
                sync = () => lblNewVersion.Content = "Le client est à jour!";
            }
            Ressource.synchronize(sync);
        }

    }
}

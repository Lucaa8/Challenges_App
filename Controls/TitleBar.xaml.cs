using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System;

namespace Challenges_App.Controls
{
    /// <summary>
    /// Logique d'interaction pour TitleBar.xaml
    /// </summary>
    public partial class TitleBar : UserControl
    {
        private static TitleBar TB;
        public TitleBar()
        {
            InitializeComponent();
            Ressource.fillRect(rectCross, Ressource.getImage(Files.ResxFile.Close));
            Ressource.fillRect(rectMinimize, Ressource.getImage(Files.ResxFile.Minimize));
            lblTitle.Content = Title;
            TB = this;
        }

        public Visibility Cross
        {
            set { SetValue(CrossProperty, value); }
            get  {return (Visibility)GetValue(CrossProperty); }
        }
        public Boolean CrossEnabled
        {
            set { SetValue(CrossEnabledProperty, value); }
            get { return (Boolean)GetValue(CrossEnabledProperty); }
        }

        public Visibility Minimize
        {
            set { SetValue(MinimizeProperty, value); }
            get { return (Visibility)GetValue(MinimizeProperty); }
        }
        public Boolean MinimizeEnabled
        {
            set { SetValue(MinimizeEnabledProperty, value); }
            get { return (Boolean)GetValue(MinimizeEnabledProperty); }
        }

        public Boolean Move
        {
            set { SetValue(MoveProperty, value); }
            get { return (Boolean)GetValue(MoveProperty); }
        }

        public String Title
        {
            set { SetValue(TitleProperty, value); }
            get { return (String)GetValue(TitleProperty); }
        }

        public static readonly DependencyProperty CrossProperty = DependencyProperty.Register("Cross", typeof(Visibility), typeof(TitleBar), new PropertyMetadata(Visibility.Hidden));
        public static readonly DependencyProperty CrossEnabledProperty = DependencyProperty.Register("CrossEnabled", typeof(Boolean), typeof(TitleBar), new PropertyMetadata(true));

        public static readonly DependencyProperty MinimizeProperty = DependencyProperty.Register("Minimize", typeof(Visibility), typeof(TitleBar), new PropertyMetadata(Visibility.Hidden));
        public static readonly DependencyProperty MinimizeEnabledProperty = DependencyProperty.Register("MinimizeEnabled", typeof(Boolean), typeof(TitleBar), new PropertyMetadata(true));

        public static readonly DependencyProperty MoveProperty = DependencyProperty.Register("Move", typeof(Boolean), typeof(TitleBar), new PropertyMetadata(true));

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(String), typeof(TitleBar), new PropertyMetadata("MyApp", OnTitlePropertyChanged));
        private static void OnTitlePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TB.lblTitle.Content = e.NewValue;
        }

        private void Mouse_Enter(object sender, MouseEventArgs e)
        {
            Rectangle r = (Rectangle)sender;
            byte[] ressource = (r == rectCross) ? Files.ResxFile.CloseHover : Files.ResxFile.MinimizeHover;
            Ressource.fillRect(r, Ressource.getImage(ressource));
        }

        private void Mouse_Leave(object sender, MouseEventArgs e)
        {
            Rectangle r = (Rectangle)sender;
            byte[] ressource = (r == rectCross) ? Files.ResxFile.Close : Files.ResxFile.Minimize;
            Ressource.fillRect(r, Ressource.getImage(ressource));
        }

        private Window getWindow()
        {
            return (Window)((Grid)Parent).Parent;
        }

        private void rectMinimize_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!MinimizeEnabled) return;
            getWindow().WindowState = WindowState.Minimized;
        }

        private void rectCross_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!CrossEnabled) return;
            getWindow().Close();
        }

        private void MoveBar(object sender, RoutedEventArgs e)
        {
            if (Move)
            {
                try
                {
                    getWindow().DragMove();
                    e.Handled = true;
                }
                catch (Exception) { }
            }
        }
    }
}

using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace P2PChat
{
    /// <summary>
    /// Interaction logic for LoginForm.xaml
    /// </summary>
    public partial class LoginForm : Window
    {

        public string Nikname
        {
            get { return TbNikname.Text; }
        }

        public LoginForm()
        {
            InitializeComponent();
        }

        private void BtOk_OnClick(object sender, RoutedEventArgs e)
        {
            if (TbNikname.Text.Length < 3)
            {
                MessageBox.Show("The name must be at least 3 characters", "Bad input data",
                     MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            this.DialogResult = true;
        }

        private void BtCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TbNikname.Focus();
        }

        private void TbNikname_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                BtOk.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }
    }
}

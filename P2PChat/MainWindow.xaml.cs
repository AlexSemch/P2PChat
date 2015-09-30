using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using P2PChat.Annotations;
using P2PChat.Model;

namespace P2PChat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        #region ctor

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        #endregion

        #region prop and fields

        private ObservableCollection<User> _users = new ObservableCollection<User>();

        public ObservableCollection<User> Users
        {
            get { return _users; }
            set
            {
                _users = value;
                OnPropertyChanged();
            }
        }


        private ObservableCollection<Chat> _chats = new ObservableCollection<Chat>();

        public ObservableCollection<Chat> Chats
        {
            get { return _chats; }
            set
            {
                _chats = value;
                OnPropertyChanged();
            }
        }


        private Chat _selectedChat;
        private User _selectdUser;

        public Chat SelectedChat
        {
            get { return _selectedChat; }
            set
            {
                _selectedChat = value;
                OnPropertyChanged();
            }
        }


        public User SelectedUser
        {
            get { return _selectdUser; }
            set
            {
                _selectdUser = value;
                OnPropertyChanged();
            }
        }

        private NetWorker _netWorker;

        #endregion

        #region methods


        private async void SendMessage()
        {
            try
            {
                if (SelectedChat == null || string.IsNullOrWhiteSpace(TbMessage.Text)) return;
                var message = new Message
                {
                    Author = SessionParams.CurrentUser,
                    Text = TbMessage.Text,
                    Time = DateTime.Now
                };
                await _netWorker.SendMessage(SelectedChat.AddresseeUser, message);
                SelectedChat.AddMessage(message);
                TbMessage.Clear();
            }
            catch (Exception ex)
            {
                DisplayError(ex);
            }
        }



        private void DisplayError(Exception ex)
        {
            MessageBox.Show(ex.Message, "Exception",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        private void AddMessageToChat(Message ms)
        {
            var chat = Chats.FirstOrDefault(ch => Equals(ch.AddresseeUser, ms.Author));
            if (chat == null)
            {
                chat = new Chat() {AddresseeUser = ms.Author};
                Chats.Add(chat);
                chat.AddMessage(ms);
            }
            else chat.AddMessage(ms);
        }

        private void RefreshUserList(Message ms)
        {
            if (Users.All(us => !Equals(us, ms.Author)))
                Users.Add(ms.Author);
        }

        #endregion

        #region hendlers

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Hide();
                var loginForm = new LoginForm();
                if (loginForm.ShowDialog() == true)
                {
                    SessionParams.CurrentUser = new User(SessionParams.GetLocalIpAddress(), loginForm.Nikname);
                    this.Title = String.Format("P2PChat [{0}]", SessionParams.CurrentUser.Name);
                    _netWorker = NetWorker.Instance();
                    _netWorker.InitializeSender();
                    _netWorker.InitializeUserListener(ms =>
                    {
                        if (ms.SystemMessage)
                            RefreshUserList(ms);
                        else
                            AddMessageToChat(ms);
                    });
                    this.Show();
                }
                else
                    this.Close();
            }
            catch (Exception ex)
            {
                DisplayError(ex);
            }
        }

        private void ContactList_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var chat = Chats.FirstOrDefault(ch => Equals(ch.AddresseeUser, SelectedUser));
            if (chat == null)
            {
                chat = new Chat() {AddresseeUser = SelectedUser};
                Chats.Add(chat);
            }
            SelectedChat = chat;
        }

        private void BtSend_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void TbMessage_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SendMessage();
        }

        #endregion

        #region INotify

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }
}

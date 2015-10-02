using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using P2PChat.Annotations;
using P2PChat.Model;

namespace P2PChat
{
    /// <summary>
    /// Interaction logic for Chat.xaml
    /// </summary>
    public partial class Chat : UserControl, INotifyPropertyChanged
    {

        public User AddresseeUser { get; set; }

        private ObservableCollection<Message> _messages = new ObservableCollection<Message>();

        public ObservableCollection<Message> Messages
        {
            get { return _messages; }
            set
            {
                _messages = value;
                OnPropertyChanged();
            }
        }

        public Chat()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public void AddMessage(Message message)
        {
            Messages.Add(message);
            LvMessage.SelectedIndex = LvMessage.Items.Count - 1;
            LvMessage.ScrollIntoView(LvMessage.SelectedItem);
        }
    }
}

using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using P2PChat.Model;

namespace P2PChat
{
    internal static class SessionParams
    {
        private static User _currentUser;

        public static User CurrentUser
        {
            get { return _currentUser; }
            set
            {
                _currentUser = value;
                SystemMessage = new Message {Author = _currentUser, SystemMessage = true};
                var bf = new BinaryFormatter();
                using (var memoryStream = new MemoryStream())
                {
                    bf.Serialize(memoryStream, SystemMessage);
                    SystemMessageBytes = memoryStream.ToArray();
                }
            }
        }

        public static byte[] SystemMessageBytes { get; private set; }
        public static Message SystemMessage { get; private set; }

        public static IPAddress GetLocalIpAddress()
        {
            var localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            return localIPs.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
        }
    }
}

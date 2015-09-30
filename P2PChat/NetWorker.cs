using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Timers;
using System.Configuration;
using P2PChat.Model;

namespace P2PChat
{
    /// <summary>
    /// Singleton for working with UDP protocol
    /// </summary>
    public class NetWorker
    {
        public static NetWorker Instance()
        {
            return _instance;
        }

        private NetWorker()
        {

        }

        #region prop and fields

        private static readonly NetWorker _instance = new NetWorker();
        private UdpClient _senderUdpClient;
        private UdpClient _recieverUdpClient;
        private IPEndPoint _endPointForSender;

        public IPEndPoint EndPointForSender
        {
            get
            {
                if (_endPointForSender != null)
                    return _endPointForSender;
                _endPointForSender = new IPEndPoint(GetIpSetting(), GetPortSetting());
                return _endPointForSender;
            }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Initialize UdpClient for sending messages and system messages of the presence in network
        /// </summary>
        public void InitializeSender()
        {
            var onlineTimer = new Timer(2000);
            onlineTimer.Elapsed += SendSystemMessage;
            onlineTimer.Start();
            _senderUdpClient = new UdpClient();
        }

        /// <summary>
        /// Initialization UdpClient for asynchronous listening network port, set in App.config
        /// </summary>
        /// <param name="onMessageReceive">Method for processing a received message</param>
        public async void InitializeUserListener(Action<Message> onMessageReceive)
        {
            _recieverUdpClient = new UdpClient(GetPortSetting());
            while (true)
            {
                var message = await _recieverUdpClient.ReceiveAsync();
                Message ms;
                using (var memoryStream = new MemoryStream(message.Buffer))
                {
                    var bf = new BinaryFormatter();
                    ms = bf.Deserialize(memoryStream) as Message;
                }
                if (ms == null || ms.Author.Equals(SessionParams.CurrentUser))
                    break;
                onMessageReceive(ms);
            }
        }

        /// <summary>
        /// Send message async
        /// </summary>
        /// <param name="addresseeUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendMessage(User addresseeUser, Message message)
        {
            var bf = new BinaryFormatter();
            using (var memoryStream = new MemoryStream())
            {
                bf.Serialize(memoryStream, message);
                await _senderUdpClient.SendAsync(memoryStream.ToArray(), Convert.ToInt32(memoryStream.Length),
                    new IPEndPoint(addresseeUser.IpAddress, GetPortSetting()));

            }
        }

        #endregion

        #region private methods

        private async void SendSystemMessage(object sender, ElapsedEventArgs e)
        {
            await
                _senderUdpClient.SendAsync(SessionParams.SystemMessageBytes, SessionParams.SystemMessageBytes.Length,
                    EndPointForSender);
        }



        private static IPAddress GetIpSetting()
        {
            return ConfigurationManager.AppSettings["AppMode"].Contains("local")
                ? IPAddress.Loopback
                : IPAddress.Broadcast;
        }

        private static int GetPortSetting()
        {
            int port;
            if (int.TryParse(ConfigurationManager.AppSettings["UdpPort"], out port))
            {
                return port;
            }
            throw new ConfigurationErrorsException("UdpPort parameter is incorrect");
        }

        #endregion
    }
}

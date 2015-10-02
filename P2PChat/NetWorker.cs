using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Timers;
using System.Configuration;
using System.Threading;
using P2PChat.Model;
using Timer = System.Timers.Timer;

namespace P2PChat
{
    /// <summary>
    /// Singleton for working with UDP protocol
    /// </summary>
    public sealed class NetWorker
    {
        public static NetWorker GetInstance()
        {
            if (_instance != null)
            {
                return _instance;
            }
            _instance = new NetWorker
            {
                _udpClient = new UdpClient(GetIntParamFromConfig("LocalPort"))
                {
                    EnableBroadcast = true,
                    MulticastLoopback = false
                }
            };
            _instance._udpClient.JoinMulticastGroup(GetGroupAddress(), GetIntParamFromConfig("TTL"));
            return _instance;
        }

        private NetWorker()
        {

        }

        #region prop and fields

        private static NetWorker _instance;
        private UdpClient _udpClient;
        private UdpClient _udpSystemClient =  new UdpClient {EnableBroadcast = true, MulticastLoopback = false};
        private IPEndPoint _endPointForSender;
        private Timer _onlineTimer;

        public IPEndPoint EndPointForSender
        {
            get
            {
                if (_endPointForSender != null)
                    return _endPointForSender;
                _endPointForSender = new IPEndPoint(GetGroupAddress(), GetIntParamFromConfig("RemotePort"));
                return _endPointForSender;
            }
        }

        #endregion

        #region public methods


        /// <summary>
        /// Initialize UdpClient for sending system messages of the presence in network
        /// </summary>
        public void StartSendingAvailabilityMessage()
        {
            _onlineTimer = new Timer(GetIntParamFromConfig("OnlineMarkerPeriod"));
            _onlineTimer.Elapsed += SendSystemMessage;
            _onlineTimer.Start();
        }

        /// <summary>
        /// Initialization UdpClient for asynchronous listening network port, set in App.config
        /// </summary>
        /// <param name="onMessageReceive">Method for processing a received message</param>
        public async void StartListening(Action<Message> onMessageReceive)
        {
            while (true)
            {
                var message = await _udpClient.ReceiveAsync();
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
                await _udpClient.SendAsync(memoryStream.ToArray(), Convert.ToInt32(memoryStream.Length),
                    new IPEndPoint(addresseeUser.IpAddress, GetIntParamFromConfig("RemotePort")));
            }
        }

        #endregion

        #region private methods


        private async void SendSystemMessage(object sender, ElapsedEventArgs e)
        {
            await
                _udpSystemClient.SendAsync(SessionParams.SystemMessageBytes, SessionParams.SystemMessageBytes.Length,
                    EndPointForSender);
        }


        private static IPAddress GetGroupAddress()
        {
            IPAddress ip;
            if (IPAddress.TryParse(ConfigurationManager.AppSettings["GroupAddress"], out ip))
                return ip;
            throw new ConfigurationErrorsException("GroupAddress is incorrect");
        }

        private static int GetIntParamFromConfig(string paramName)
        {
            int port;
            if (int.TryParse(ConfigurationManager.AppSettings[paramName], out port))
            {
                return port;
            }
            throw new ConfigurationErrorsException(String.Format("{0} parameter is incorrect", paramName));
        }


        public void Stop()
        {
           _udpClient.DropMulticastGroup(GetGroupAddress());
            _onlineTimer.Stop();
        }

        #endregion
    }
}

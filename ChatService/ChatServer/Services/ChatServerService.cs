using ChatServer.Contracts;
using ChatServer.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ChatServer.Services
{
    public class ChatServerService : IChatServerService
    {
        private readonly ChatServerOptions _options;
        private Hashtable _users { get; set; }
        private Hashtable _connections;
        private Hashtable _timeLimits;
        private TcpListener _tcpListener;
        private Thread _threadListener;
        private bool _serverStatus;

        public ChatServerService(ChatServerOptions options)
        {
            //Set config of server
            _options = options;

            //Hash table with the max users
            _users = new Hashtable(options.MaximumUsers);

            //Has table with the max connections
            _connections = new Hashtable(options.MaximumUsers);

            //Has table with the max connections
            _timeLimits = new Hashtable(options.MaximumUsers);

            //Set server status
            _serverStatus = true;
        }

        /// <summary>
        /// Start the TCP Listener
        /// </summary>
        public void Start()
        {
            try
            {
                //set the host
                var ipAddress = IPAddress.Parse(_options.Host);

                //set tcp listener
                _tcpListener = new TcpListener(ipAddress, _options.Port);

                _tcpListener.Start();

                // Starta a new thread to Keep the Server running and accepting hte new connections
                _threadListener = new Thread(KeepServer);
                _threadListener.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Stop the TCP Listener
        /// </summary>
        public void Stop()
        {
            _threadListener.Abort();
            _tcpListener.Stop();
            _serverStatus = false;
        }

        public void KeepServer()
        {
            Console.WriteLine("*** Server started successfully.");

            while (_serverStatus)
            {
                //Accept a new connection
                var tcpClient = _tcpListener.AcceptTcpClient();

                //Create a new instance of connection
                var newConnection = new ChatConnectionService(tcpClient, this);
            }
        }

        /// <summary>
        /// Check if user exists in the User Hash table
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool CheckIfUserExists(string user)
        {
            var hashUser = _users[user];
            if (hashUser == null)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Add user in the Hash Tables and inform the new connection to all users.
        /// </summary>
        /// <param name="tcpClient"></param>
        /// <param name="nickname"></param>
        public void CreateUser(TcpClient tcpClient, string nickname)
        {
            _users.Add(nickname, tcpClient);
            _connections.Add(tcpClient, nickname);
            _timeLimits.Add(nickname, DateTime.Today);

            string newConnectionText = $"{_connections[tcpClient]} has joined.";
            UpdateServerAndUsers(newConnectionText);
        }

        /// <summary>
        /// Remove user from the Hash Tables and inform all users about it
        /// </summary>
        /// <param name="tcpClient"></param>
        public void DeleteUser(TcpClient tcpClient)
        {
            // If user already exists
            if (_connections[tcpClient] != null)
            {
                var user = _connections[tcpClient];

                _users.Remove(_connections[tcpClient]);
                _timeLimits.Remove(_connections[tcpClient]);
                _connections.Remove(tcpClient);

                string userLefttext = $"{user} left.";
                UpdateServerAndUsers(userLefttext);
            }
        }

        /// <summary>
        /// Send message based on the configuration (public or privately)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="message"></param>
        public void SendMessage(string source, string message)
        {
            // Public message
            if (!string.IsNullOrEmpty(message))
            {
                var timeLimit = (DateTime)_timeLimits[source];
                if (timeLimit != DateTime.Today && DateTime.Now.Subtract(timeLimit).TotalSeconds < 60)
                {
                    if (_users[source] is TcpClient tcpClient)
                    {
                        DeleteUser(tcpClient);
                        tcpClient.Close();
                    }
                }
                else
                {
                    var fullMessage = $"{source} says: {message}";
                    UpdateServerInformation(fullMessage);
                    SendPublicMessage(fullMessage);

                    _timeLimits[source] = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// Send the message to all users in the Hash Table
        /// </summary>
        /// <param name="message"></param>
        private void SendPublicMessage(string message)
        {
            StreamWriter SenderWriter;

            // Creates a new array with the correct size (based on the Hash Table)
            TcpClient[] tcpClientes = new TcpClient[_users.Count];

            // Copy all the objects from the Hash to the array
            _users.Values.CopyTo(tcpClientes, 0);

            // For each element in the array, try to send the message
            for (int i = 0; i < tcpClientes.Length; i++)
            {
                try
                {
                    if (message.Trim() == "" || tcpClientes[i] == null)
                    {
                        continue;
                    }

                    // Send the message to the user
                    SenderWriter = new StreamWriter(tcpClientes[i].GetStream());
                    SenderWriter.WriteLine(message);
                    SenderWriter.Flush();
                    SenderWriter = null;
                }
                catch // If any problem, probably the user does not exists, so remove him
                {
                    DeleteUser(tcpClientes[i]);
                }
            }
        }

        /// <summary>
        /// Update Server Console
        /// </summary>
        /// <param name="message"></param>
        private void UpdateServerInformation(string message)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        /// Update Server Console and All users
        /// </summary>
        /// <param name="message"></param>
        private void UpdateServerAndUsers(string message)
        {
            UpdateServerInformation(message);
            SendPublicMessage(message);
        }
    }
}

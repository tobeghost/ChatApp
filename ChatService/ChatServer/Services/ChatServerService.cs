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
            string fullMessage = string.Empty;

            // Split the messages using |||
            // Count = 1 => Public Message
            // Count = 2 => Public Message to One User
            // Count = 3 => Private Message to Specific User
            var messageContent = message.Split("|||");
            var mesageContentCount = messageContent.Count();

            // Public message
            if (mesageContentCount == 1)
            {
                fullMessage = $"{source} says to all: {message}";
                UpdateServerInformation(fullMessage);
                SendPublicMessage(fullMessage);
            }
            // Public message to one user
            else if (mesageContentCount == 2)
            {
                string publicUser = messageContent[0];
                string publicMessage = messageContent[1];
                fullMessage = $"{source} says to {publicUser}: {publicMessage}";

                var userExists = CheckIfUserExists(publicUser);
                if (userExists)
                {
                    UpdateServerInformation(fullMessage);
                    SendPublicMessage(fullMessage);
                }
                else
                {
                    SendServerPrivateMessage("Not found user", source);
                }
            }
            // Private message to specific user
            else if (mesageContentCount == 3)
            {
                string privateUser = messageContent[0];
                string privateMessage = messageContent[1];
                fullMessage = $"{source} says to {privateUser} (privately): {privateMessage}";

                var userExists = CheckIfUserExists(privateUser);
                if (userExists)
                {
                    UpdateServerInformation(fullMessage);
                    SendPrivateMessage(fullMessage, source, messageContent[0]);
                }
                else
                {
                    SendServerPrivateMessage("Not found user", source);
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
        /// Send Server message to Source user.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sourceUser"></param>
        private void SendServerPrivateMessage(string message, string sourceUser)
        {
            var findSourceUser = _users[sourceUser];
            if (findSourceUser != null && findSourceUser is TcpClient)
            {
                var tcpClient = findSourceUser as TcpClient;

                try
                {
                    if (message.Trim() == "")
                    {
                        return;
                    }

                    // Send the message to the source user that has sent
                    var writer = new StreamWriter(tcpClient.GetStream());
                    writer.WriteLine(message);
                    writer.Flush();
                }
                catch (Exception ex)
                {
                    DeleteUser(tcpClient);
                }
            }
        }


        /// <summary>
        /// Send private message to specific user, based on the message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sourceUser"></param>
        /// <param name="destinationUser"></param>
        private void SendPrivateMessage(string message, string sourceUser, string destinationUser)
        {
            var findDestinationUser = _users[destinationUser];
            if (findDestinationUser != null && findDestinationUser is TcpClient)
            {
                var tcpClient = findDestinationUser as TcpClient;

                try
                {

                    if (message.Trim() == "")
                    {
                        return;
                    }

                    // Send the message to the specific user
                    var writer = new StreamWriter(tcpClient.GetStream());
                    writer.WriteLine(message);
                    writer.Flush();
                    writer = null;
                }
                catch (Exception ex)
                {
                    DeleteUser(tcpClient);
                }
            }

            var findSourceUser = _users[sourceUser];
            if (findSourceUser != null && findSourceUser is TcpClient)
            {
                var tcpClient = findSourceUser as TcpClient;

                try
                {
                    if (message.Trim() == "")
                    {
                        return;
                    }

                    // Send the message to the source user that has sent
                    var writer = new StreamWriter(tcpClient.GetStream());
                    writer.WriteLine(message);
                    writer.Flush();
                }
                catch (Exception ex)
                {
                    DeleteUser(tcpClient);
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

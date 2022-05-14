using ChatClient.Contracts;
using ChatClient.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ChatClient.Services
{
    public class ChatClientService : IChatClientService
    {
        private readonly ChatClientOptions _options;
        private bool _connected;
        private StreamWriter _writer;
        private TcpClient _tcpClient;
        private bool _userAlreadyTaken;

        public ChatClientService(ChatClientOptions options)
        {
            _options = options;

            //Initiate a new TCP connection with the server
            _tcpClient = new TcpClient();
        }

        public bool UserAlreadyTaken
        {
            get { return _userAlreadyTaken; }
            set { _userAlreadyTaken = value; }
        }

        /// <summary>
        /// Connect to the server
        /// </summary>
        public void Connect(string username)
        {
            try
            {
                //Connect to the server
                _tcpClient.Connect(_options.Host, _options.Port);

                //Property responsible to manage the connectivity
                _connected = true;

                //Send the nickname to the server
                _writer = new StreamWriter(_tcpClient.GetStream());
                _writer.WriteLine(username);
                _writer.Flush();

                //Starts a thread to receiving messages e new conversations
                var _threadMessage = new Thread(new ThreadStart(GetMessages));

                _threadMessage.Start();

                Thread.Sleep(2000);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Disconnect from server
        /// </summary>
        public void Disconnect()
        {
            _connected = false;
            _writer.Close();
            _tcpClient.Close();

            _writer.Dispose();
            _tcpClient.Dispose();

            _writer = null;
            _tcpClient = null;
        }

        /// <summary>
        /// Send to message
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                _writer.WriteLine(message);
                _writer.Flush();
            }
        }

        /// <summary>
        /// Get messages from server
        /// </summary>
        private void GetMessages()
        {
            // Receive the response from the server
            var reader = new StreamReader(_tcpClient.GetStream());

            string response = reader.ReadLine();

            // If the first character is 1, the connection was done successfully
            if (response[0] == '1')
            {
                Console.WriteLine("Connected successfully to the chat!");

                _connected = true;
            }
            else // If not so, connection was not successfully
            {
                string reason = "Not connected: ";

                // Get the reason
                reason += response.Substring(2, response.Length - 2);

                _connected = false;

                // Check if the reason was based on the "User Already Taken" to restart the process
                if (reason.Contains("This is nickname already exists in the chat"))
                    _userAlreadyTaken = true;

                return;
            }

            // While connected, show the messages in the Server Console
            while (_connected)
            {
                var list = reader.ReadLine();
                Console.WriteLine(list);
            }
        }
    }
}

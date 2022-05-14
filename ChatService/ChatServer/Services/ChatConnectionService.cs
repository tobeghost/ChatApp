using ChatServer.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ChatServer.Services
{
    public class ChatConnectionService
    {
        private readonly IChatServerService _chatServerService;

        private TcpClient _tcpClient;
        private Thread _threadSender;
        private StreamReader _reader;
        private StreamWriter _writer;

        /// <summary>
        /// The class Constructor that receives the new TCP Connection, accepting the new client and waiting
        /// for the messages.
        /// </summary>
        /// <param name="tcpClient"></param>
        /// <param name="chatServerService"></param>
        public ChatConnectionService(TcpClient tcpClient, IChatServerService chatServerService)
        {
            _tcpClient = tcpClient;

            _threadSender = new Thread(AcceptClient);

            _threadSender.Start();

            _chatServerService = chatServerService;
        }

        /// <summary>
        /// Accept the Client inserting him in the Server Hash Tables and starts
        /// to listening user messages.
        /// </summary>
        private void AcceptClient()
        {
            try
            {
                _reader = new StreamReader(_tcpClient.GetStream());
                _writer = new StreamWriter(_tcpClient.GetStream());

                // Read the user information
                var _userInformation = _reader.ReadLine();

                // If the information exists
                if (string.IsNullOrEmpty(_userInformation))
                {
                    CloseConnection();
                    return;
                }

                // Check if the user already exists in the current connection
                if (_chatServerService.CheckIfUserExists(_userInformation))
                {
                    // 0: not connected
                    _writer.WriteLine("0|This is nickname already exists in the chat.");
                    _writer.Flush();
                    CloseConnection();
                    return;
                }
                else
                {
                    // 1: Connected successfully
                    _writer.WriteLine("1");
                    _writer.Flush();

                    // Add the user in the HashTable and starts the message listener
                    _chatServerService.CreateUser(_tcpClient, _userInformation);
                }

                try
                {
                    string message;

                    //Still waiting for new user messages
                    while ((message = _reader.ReadLine()) != "")
                    {
                        // If invalid, remove the user
                        if (message == null)
                        {
                            _chatServerService.DeleteUser(_tcpClient);
                        }
                        else
                        {
                            // If OK, send the message to the Server treatment
                            _chatServerService.SendMessage(_userInformation, message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                    // If any problem with the user, user is disconnected
                    _chatServerService.DeleteUser(_tcpClient);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void CloseConnection()
        {
            _tcpClient.Close();
            _reader.Close();
            _writer.Close();
        }
    }
}

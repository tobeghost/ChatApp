using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ChatServer.Contracts
{
    public interface IChatServerService
    {
        void Start();
        void Stop();
        void KeepServer();
        void CreateUser(TcpClient tcpClient, string nickname);
        void DeleteUser(TcpClient tcpClient);
        void SendMessage(string source, string message);
        bool CheckIfUserExists(string user);
    }
}

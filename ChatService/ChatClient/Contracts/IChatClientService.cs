using System;
using System.Collections.Generic;
using System.Text;

namespace ChatClient.Contracts
{
    public interface IChatClientService
    {
        bool UserAlreadyTaken { get; set; }
        void Connect(string username);
        void Disconnect();
        void SendMessage(string message);
    }
}

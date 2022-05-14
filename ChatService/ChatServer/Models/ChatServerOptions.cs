using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatServer.Models
{
    public class ChatServerOptions
    {
        [Option('h', "host", Required = true, HelpText = "Set server ip address.")]
        public string Host { get; set; }

        [Option('p', "port", Required = true, HelpText = "Set server port.")]
        public int Port { get; set; }

        [Option('m', "max_users", Required = false, HelpText = "Set maximum users for server.")]
        public int MaximumUsers { get; set; } = 30;
    }
}

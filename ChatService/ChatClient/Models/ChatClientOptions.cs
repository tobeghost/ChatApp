using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatClient.Models
{
    public class ChatClientOptions
    {
        [Option('h', "host", Required = true, HelpText = "Set server ip address.")]
        public string Host { get; set; }

        [Option('p', "port", Required = true, HelpText = "Set server port.")]
        public int Port { get; set; }
    }
}

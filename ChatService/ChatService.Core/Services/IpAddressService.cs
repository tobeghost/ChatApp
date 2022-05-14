using ChatService.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatService.Core.Services
{
    public class IpAddressService : IIpAddressService
    {
        /// <summary>
        /// Get local IP of the machine.
        /// </summary>
        /// <returns></returns>
        public string GetLocalIp()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var ipAddress = host.AddressList.FirstOrDefault(i => i.AddressFamily == AddressFamily.InterNetwork);
            if (ipAddress == null)
                throw new Exception("Not found IP Address");

            return ipAddress.ToString();
        }
    }
}

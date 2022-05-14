using System;
using System.Collections.Generic;
using System.Text;

namespace ChatService.Core.Contracts
{
    public interface IIpAddressService
    {
        string GetLocalIp();
    }
}

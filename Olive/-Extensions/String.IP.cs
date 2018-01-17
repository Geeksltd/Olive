using System;
using System.Linq;
using System.Net;

namespace Olive
{
    partial class OliveExtensions
    {
        static readonly Range<uint>[] PrivateIpRanges = new[] {
             //new Range<uint>(0u, 50331647u),              // 0.0.0.0 to 2.255.255.255
             new Range<uint>(167772160u, 184549375u),     // 10.0.0.0 to 10.255.255.255
             new Range<uint>(2130706432u, 2147483647u),   // 127.0.0.0 to 127.255.255.255
             new Range<uint>(2851995648u, 2852061183u),   // 169.254.0.0 to 169.254.255.255
             new Range<uint>(2886729728u, 2887778303u),   // 172.16.0.0 to 172.31.255.255
             new Range<uint>(3221225984u, 3221226239u),   // 192.0.2.0 to 192.0.2.255
             new Range<uint>(3232235520u, 3232301055u),   // 192.168.0.0 to 192.168.255.255
             new Range<uint>(4294967040u, 4294967295u)    // 255.255.255.0 to 255.255.255.255
        };

        /// <summary>
        /// Determines if the given ip address is in any of the private IP ranges
        /// </summary>
        public static bool IsPrivateIp(string address)
        {
            if (address.IsEmpty()) return false;

            var bytes = IPAddress.Parse(address).GetAddressBytes();
            if (BitConverter.IsLittleEndian)
                bytes = bytes.Reverse().ToArray();

            var ip = BitConverter.ToUInt32(bytes, 0);

            return PrivateIpRanges.Any(range => range.Contains(ip));
        }
    }
}
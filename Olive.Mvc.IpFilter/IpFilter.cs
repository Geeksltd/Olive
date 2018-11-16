using Microsoft.AspNetCore.Http;
using Olive.Csv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Mvc
{
    public class IpFilter
    {
        const int IP_4_SIZE = 8;
        static AsyncLock SyncLock = new AsyncLock();
        static List<string> BlockedCountryCodes = new List<string>();

        static FileInfo CountryIpsFile
            => AppDomain.CurrentDomain.WebsiteRoot().GetOrCreateSubDirectory("--IPFilter").GetFile("dbip-country.csv");

        static List<Range<BigInteger>> BlockedIpRanges;

        public static List<BigInteger> SpecificallyAllowedIps = new List<BigInteger>();
        public static List<BigInteger> SpecificallyDisallowedIps = new List<BigInteger>();

        public static string BlockedAttemptResponse = "This website is not available in your region.";
        public static Func<Task> OnBlockedAccessAttempt = EndWithMessage;

        #region Country and Region codes

        public static readonly string[] CountryCodes = "US,AU,CN,JP,TH,IN,MY,KR,SG,TW,HK,PH,VN,FR,DE,ES,IL,AT,NL,GB,SE,IT,AR,BE,FI,RU,GR,IE,BR,DK,PL,AE,UA,KZ,PT,SA,IR,NO,AS,CA,BA,EE,HU,RS,BG,ZA,VU,SY,KW,BH,LB,QA,OM,JO,CZ,PK,CH,IQ,TR,RO,BZ,MX,CL,CO,GE,MA,LV,AF,VG,EG,CY,HR,NG,LU,GT,UZ,KE,UY,MK,MT,PA,AZ,BI,ZM,ZW,PS,LT,SK,IS,SI,MD,AO,LI,SC,SN,JE,PY,BY,KG,RE,IM,BS,GG,GI,LY,AM,YE,CU,CR,BD,BO,GP,MQ,GY,ID,LK,XK,CX,DZ,EH,GN,KI,SO,AW,BB,BF,BJ,BL,BM,BN,BV,BW,CC,CD,CF,CG,CI,CK,CM,CV,CW,DJ,DM,DO,EC,ET,FJ,FK,FM,FO,GA,GD,GF,GH,GL,GM,GQ,GS,GU,GW,HT,JM,KM,KN,KY,LA,LC,LR,LS,MC,ME,MG,MH,ML,MM,MN,MO,MR,MS,MU,MV,MW,MZ,NA,NC,NE,NF,NI,NP,NR,NU,NZ,PG,PM,PN,PR,RW,SD,SH,SL,SM,SR,ST,SV,SZ,TC,TD,TG,TJ,TM,TN,TO,TT,TV,TZ,UG,UM,VA,VC,VE,VI,WF,WS,YT,AD,HN,IO,PE,AL,AI,KH,TK,AG,AQ,AX,BQ,BT,ER,HM,KP,MP,PF,PW,SB,SJ,TL,SS,TF,SX,MF".Split(',');

        public static readonly Dictionary<string, string> CountriesInRegions = "US:NA,AU:OC,CN:AS,JP:AS,TH:AS,IN:AS,MY:AS,KR:AS,SG:AS,TW:AS,HK:AS,PH:AS,VN:AS,FR:EU,DE:EU,ES:EU,IL:AS,AT:EU,NL:EU,GB:EU,SE:EU,IT:EU,AR:SA,BE:EU,FI:EU,RU:EU,GR:EU,IE:EU,BR:SA,DK:EU,PL:EU,AE:AS,UA:EU,KZ:AS,PT:EU,SA:AS,IR:AS,NO:EU,AS:OC,CA:NA,BA:EU,EE:EU,HU:EU,RS:EU,BG:EU,ZA:AF,VU:OC,SY:AS,KW:AS,BH:AS,LB:AS,QA:AS,OM:AS,JO:AS,CZ:EU,PK:AS,CH:EU,IQ:AS,TR:EU,RO:EU,BZ:NA,MX:NA,CL:SA,CO:SA,GE:AS,MA:AF,LV:EU,AF:AS,VG:NA,EG:AF,CY:AS,HR:EU,NG:AF,LU:EU,GT:NA,UZ:AS,KE:AF,UY:SA,MK:EU,MT:EU,PA:NA,AZ:AS,BI:AF,ZM:AF,ZW:AF,PS:AS,LT:EU,SK:EU,IS:EU,SI:EU,MD:EU,AO:AF,LI:EU,SC:AF,SN:AF,JE:EU,PY:SA,BY:EU,KG:AS,RE:AF,IM:EU,BS:NA,GG:EU,GI:EU,LY:AF,AM:AS,YE:AS,CU:NA,CR:NA,BD:AS,BO:SA,GP:NA,MQ:NA,GY:SA,ID:AS,LK:AS,XK:EU,CX:AS,DZ:AF,EH:AF,GN:AF,KI:OC,SO:AF,AW:NA,BB:NA,BF:AF,BJ:AF,BL:NA,BM:NA,BN:AS,BV:AN,BW:AF,CC:AS,CD:AF,CF:AF,CG:AF,CI:AF,CK:OC,CM:AF,CV:AF,CW:SA,DJ:AF,DM:NA,DO:NA,EC:SA,ET:AF,FJ:OC,FK:SA,FM:OC,FO:EU,GA:AF,GD:NA,GF:SA,GH:AF,GL:NA,GM:AF,GQ:AF,GS:AN,GU:OC,GW:AF,HT:NA,JM:NA,KM:AF,KN:NA,KY:NA,LA:AS,LC:NA,LR:AF,LS:AF,MC:EU,ME:EU,MG:AF,MH:OC,ML:AF,MM:AS,MN:AS,MO:AS,MR:AF,MS:NA,MU:AF,MV:AS,MW:AF,MZ:AF,NA:AF,NC:OC,NE:AF,NF:OC,NI:NA,NP:AS,NR:OC,NU:OC,NZ:OC,PG:OC,PM:NA,PN:OC,PR:NA,RW:AF,SD:AF,SH:AF,SL:AF,SM:EU,SR:SA,ST:AF,SV:NA,SZ:AF,TC:NA,TD:AF,TG:AF,TJ:AS,TM:AS,TN:AF,TO:OC,TT:NA,TV:OC,TZ:AF,UG:AF,UM:OC,VA:EU,VC:NA,VE:SA,VI:NA,WF:OC,WS:OC,YT:AF,AD:EU,HN:NA,IO:AS,PE:SA,AL:EU,AI:NA,KH:AS,TK:OC,AG:NA,AQ:AN,AX:EU,BQ:SA,BT:AS,ER:AF,HM:AN,KP:AS,MP:OC,PF:OC,PW:OC,SB:OC,SJ:EU,TL:AS,SS:AF,TF:AN,SX:SA,MF:NA".Split(',').ToDictionary(x => x.Split(':').First(), x => x.Split(':').Last());

        #endregion

        public static string[] GetCountryCodes(string regionCode)
        {
            if (!CountryIpsFile.Exists())
                throw new Exception($"Could not find the file '{CountryIpsFile.FullName}'.\r\nYou can download it from https://db-ip.com/db/download/country");

            return CountriesInRegions.Where(i => i.Value == regionCode).Select(x => x.Key).ToArray();
        }

        /// <summary>
        /// Sets the default policy for all IP addresses.
        /// </summary>
        public static void SetGlobalPolicy(Policy policy)
        {
            if (policy == Policy.Disallow) BlockedCountryCodes = CountryCodes.ToList();
            else if (policy == Policy.Allow) BlockedCountryCodes = new List<string>();
        }

        /// <summary>
        /// Sets the policy for specific IP addresses. These will override the global, region and country policies.
        /// </summary>
        public static void SetSpecificIpPolicy(Policy policy, params string[] ipAddresses)
        {
            foreach (var ip in ipAddresses)
            {
                var value = ToIpValue(ip);

                if (policy == Policy.Allow)
                {
                    SpecificallyAllowedIps.Add(value);
                    SpecificallyDisallowedIps.Remove(value);
                }

                if (policy == Policy.Disallow)
                {
                    SpecificallyDisallowedIps.Add(value);
                    SpecificallyAllowedIps.Remove(value);
                }
            }
        }

        /// <summary>
        /// Sets the IP Filter policy. All Disallow policies should be set first, then all Allow policies.
        /// </summary>
        public static void SetCountryPolicy(Policy policy, params string[] countryCodes)
        {
            if (policy == Policy.Disallow)
                BlockedCountryCodes = BlockedCountryCodes.Concat(countryCodes).Distinct().ToList();

            if (policy == Policy.Allow)
                BlockedCountryCodes = BlockedCountryCodes.Except(countryCodes).ToList();
        }

        /// <summary>
        /// Sets the IP Filter policy. All Disallow policies should be set first, then all Allow policies.
        /// </summary>
        public static void SetRegionPolicy(Policy policy, params string[] regionCodes) =>
            regionCodes.Do(r => SetCountryPolicy(policy, GetCountryCodes(r)));

        /// <summary>
        ///  If the IP address of the current user is in a blocked list, then it will terminate the request with a response saying:
        ///  This website is not available in your region.
        /// </summary>
        public static async Task BlockIfNecessary(HttpContext httpContext)
        {
            if (!await IsAllowed(httpContext.Connection.RemoteIpAddress.ToStringOrEmpty()))
                await OnBlockedAccessAttempt();
        }

        public static async Task<bool> IsAllowed(string ipAddress)
        {
            if (BlockedIpRanges == null) await LoadBlockedIpRanges();

            var address = ToIpValue(ipAddress);

            if (SpecificallyDisallowedIps.Contains(address)) return false;
            if (SpecificallyAllowedIps.Contains(address)) return true;

            return BlockedIpRanges.None(range => range.Contains(address));
        }

        static Task EndWithMessage() => Context.Current.Response().EndWith(BlockedAttemptResponse);

        static async Task LoadBlockedIpRanges()
        {
            using (await SyncLock.Lock())
            {
                if (BlockedIpRanges != null) return;
                else BlockedIpRanges = new List<Range<BigInteger>>();

                var table = await CsvReader.ReadAsync(CountryIpsFile, isFirstRowHeaders: false);

                foreach (var row in table.GetRows())
                {
                    if (!BlockedCountryCodes.Contains((string)row[2]))
                        continue;

                    var from = ToIpValue((string)row[0]);
                    var to = ToIpValue((string)row[1]);

                    BlockedIpRanges.Add(new Range<BigInteger>(from, to));
                }
            }
        }

        static BigInteger ToIpValue(string ipAddress)
        {
            try
            {
                var bytes = System.Net.IPAddress.Parse(ipAddress).GetAddressBytes();
                if (BitConverter.IsLittleEndian)
                    bytes = bytes.Reverse().ToArray();

                if (bytes.Length > IP_4_SIZE)
                    return new BigInteger(BitConverter.ToUInt64(bytes, IP_4_SIZE), BitConverter.ToUInt64(bytes, 0));

                return new BigInteger(0, BitConverter.ToUInt32(bytes, 0));
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot convert the specified IP address string of '{ipAddress}' to unit IP address value.", ex);
            }
        }

        public enum Policy { Allow, Disallow }

        public class Region
        {
            public readonly static string Antarctica = "AN";
            public readonly static string Africa = "AF";
            public readonly static string Asia = "AS";
            public readonly static string Europe = "EU";
            public readonly static string NorthAmerica = "NA";
            public readonly static string Oceania = "OC";
            public readonly static string SouthAmerica = "SA";
        }

        public struct BigInteger : IComparable, IComparable<BigInteger>
        {
            ulong Value1;
            ulong Value2;

            public BigInteger(ulong value1, ulong value2)
            {
                Value1 = value1;
                Value2 = value2;
            }

            public int CompareTo(BigInteger other)
            {
                if (Value1 == other.Value1 && Value2 == other.Value2) return 0;
                else return IsGreaterThan(other) ? 1 : -1;
            }

            bool IsGreaterThan(BigInteger other)
            {
                if (Value1 > other.Value1) return true;
                else if (Value1 == other.Value1 && Value2 > other.Value2) return true;
                return false;
            }

            int IComparable.CompareTo(object obj) => CompareTo((BigInteger)obj);

            public static bool operator >=(BigInteger b1, BigInteger b2) => b1.CompareTo(b2) >= 0;

            public static bool operator <=(BigInteger b1, BigInteger b2) => b1.CompareTo(b2) <= 0;

            public static bool operator >(BigInteger b1, BigInteger b2) => b1.CompareTo(b2) > 0;

            public static bool operator <(BigInteger b1, BigInteger b2) => b1.CompareTo(b2) < 0;
        }
    }
}
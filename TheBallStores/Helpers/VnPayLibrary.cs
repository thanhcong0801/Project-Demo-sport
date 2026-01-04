using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace TheBallStores.Helpers
{
    public class VnPayLibrary
    {
        private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());
        private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayCompare());

        public void AddRequestData(string key, string? value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        public void AddResponseData(string key, string? value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData.Add(key, value);
            }
        }

        public string GetResponseData(string key)
        {
            return _responseData.TryGetValue(key, out var retValue) ? retValue : string.Empty;
        }

        public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
        {
            StringBuilder data = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in _requestData)
            {
                if (data.Length > 0)
                {
                    data.Append('&');
                }
                data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value));
            }
            string queryString = data.ToString();
            string vnp_SecureHash = Utils.HmacSHA512(vnp_HashSecret, queryString);
            baseUrl += "?" + queryString;
            string signData = vnp_SecureHash;
            if (signData.Length > 0)
            {
                baseUrl += "&vnp_SecureHash=" + signData;
            }
            return baseUrl;
        }

        public bool ValidateSignature(string inputHash, string vnp_HashSecret)
        {
            StringBuilder data = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in _responseData)
            {
                if (kv.Key != "vnp_SecureHash" && kv.Key != "vnp_SecureHashType")
                {
                    if (data.Length > 0)
                    {
                        data.Append('&');
                    }
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value));
                }
            }
            string checkSum = Utils.HmacSHA512(vnp_HashSecret, data.ToString());

            // Console Log để debug lỗi chữ ký trên Render
            Console.WriteLine($"[VNPAY DEBUG] InputHash: {inputHash}");
            Console.WriteLine($"[VNPAY DEBUG] Calculated: {checkSum}");

            return checkSum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }
    }

    public class VnPayCompare : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            var vnpCompare = CompareInfo.GetCompareInfo("en-US");
            return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
        }
    }

    public static class Utils
    {
        public static string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }
            return hash.ToString();
        }

        public static string GetIpAddress(HttpContext context)
        {
            var ipAddress = string.Empty;
            try
            {
                var remoteIp = context.Connection.RemoteIpAddress;
                if (remoteIp != null)
                {
                    if (remoteIp.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                    {
                        remoteIp = System.Net.Dns.GetHostEntry(remoteIp).AddressList
                            .FirstOrDefault(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                    }
                    ipAddress = remoteIp?.ToString();
                }
            }
            catch (Exception ex)
            {
                return "Invalid IP: " + ex.Message;
            }
            return string.IsNullOrEmpty(ipAddress) ? "127.0.0.1" : ipAddress;
        }
    }
}
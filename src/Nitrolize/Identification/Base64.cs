using System;
using System.Text;

namespace Nitrolize.Identification
{
    public static class Base64
    {
        public static string Encode(string stringToEncode)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(stringToEncode));
        }

        public static string Decode(string stringToDecode)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(stringToDecode));
        }
    }
}

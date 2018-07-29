using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrimsonlandTrainer {
    public static class HexUtility {
        public static string BytesToHexString(byte[] bytes) {
            var hex = new StringBuilder(bytes.Length * 2);
            string alphabet = "0123456789ABCDEF";

            foreach (byte b in bytes) {
                hex.Append(alphabet[(int) (b >> 4)]);
                hex.Append(alphabet[(int) (b & 0xF)]);
            }

            return hex.ToString();
        }

        /// <summary>
        /// Converts a hex string to an array of bytes.
        /// </summary>
        /// <param name="hexStr">Hex string to convert, it may optionally start with the "0x" prefix.</param>
        /// <returns>Array of bytes.</returns>
        public static byte[] HexStringToBytes(string hexStr) {
            if (hexStr.StartsWith("0x")) {
                return FromHexString(hexStr.Substring(2));
            }

            return FromHexString(hexStr);
        }

        public static byte[] FromHexString(string hexString) {
            if (hexString == null)
                return null;

            if (hexString.Length % 2 != 0)
                throw new FormatException("The hex string is invalid because it has an odd length");

            byte[] numArray = new byte[hexString.Length / 2];
            for (int index = 0; index < numArray.Length; ++index)

                numArray[index] = Convert.ToByte(hexString.Substring(index * 2, 2), 16);
            return numArray;
        }

        public static string FormatAsHex(this IntPtr ptr) {
            return "0x" + ptr.ToString("X8");
        }
    }
}

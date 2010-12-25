/*********************************************************************************
    This file is part of Open Mobile.

    Open Mobile is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Open Mobile is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Open Mobile.  If not, see <http://www.gnu.org/licenses/>.
 
    There is one additional restriction when using this framework regardless of modifications to it.
    The About Panel or its contents must be easily accessible by the end users.
    This is to ensure all project contributors are given due credit not only in the source code.
*********************************************************************************/
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace OpenMobile.helperFunctions
{
    /// <summary>
    /// Handles encryption and decryption
    /// </summary>
    public static class Encryption
    {
        /// <summary>
        /// Encrypt a string using AES encryption
        /// </summary>
        /// <param name="PlainText">string to encrypt</param>
        /// <param name="salt">salt bytes (aka password)</param>
        /// <returns></returns>
        public static string AESEncrypt(string PlainText, string salt)
        {
            byte[] SaltValueBytes = Encoding.ASCII.GetBytes(salt);
            byte[] InitialVectorBytes = Encoding.ASCII.GetBytes("DOMNITZSOLUTIONS");
            byte[] PlainTextBytes = Encoding.UTF8.GetBytes(PlainText);
            PasswordDeriveBytes DerivedPassword = new PasswordDeriveBytes(new byte[] { 0x78, 0x38, 0xA1, 0x45, 0x68 }, SaltValueBytes, "SHA", 3);
            byte[] KeyBytes = DerivedPassword.GetBytes(32);
            RijndaelManaged SymmetricKey = new RijndaelManaged();
            SymmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform Encryptor = SymmetricKey.CreateEncryptor(KeyBytes, InitialVectorBytes);
            MemoryStream MemStream = new MemoryStream();
            CryptoStream CryptoStream = new CryptoStream(MemStream, Encryptor, CryptoStreamMode.Write);
            CryptoStream.Write(PlainTextBytes, 0, PlainTextBytes.Length);
            CryptoStream.FlushFinalBlock();
            byte[] CipherTextBytes = MemStream.ToArray();
            MemStream.Close();
            CryptoStream.Close();
            return Convert.ToBase64String(CipherTextBytes);
        }
        /// <summary>
        /// Decrypts the given string encoded in AES
        /// </summary>
        /// <param name="CipherText">The encoded string</param>
        /// <param name="salt">the initial salt (password)</param>
        /// <returns></returns>
        public static string AESDecrypt(string CipherText, string salt)
        {
            if (CipherText == null)
                return String.Empty;
            byte[] SaltValueBytes = Encoding.ASCII.GetBytes(salt);
            byte[] InitialVectorBytes = Encoding.ASCII.GetBytes("DOMNITZSOLUTIONS");
            byte[] CipherTextBytes = Convert.FromBase64String(CipherText);
            PasswordDeriveBytes DerivedPassword = new PasswordDeriveBytes(new byte[] { 0x78, 0x38, 0xA1, 0x45, 0x68 }, SaltValueBytes, "SHA", 3);
            byte[] KeyBytes = DerivedPassword.GetBytes(32);
            RijndaelManaged SymmetricKey = new RijndaelManaged();
            SymmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform Decryptor = SymmetricKey.CreateDecryptor(KeyBytes, InitialVectorBytes);
            MemoryStream MemStream = new MemoryStream(CipherTextBytes);
            CryptoStream cryptoStream = new CryptoStream(MemStream, Decryptor, CryptoStreamMode.Read);
            byte[] PlainTextBytes = new byte[CipherTextBytes.Length];
            int ByteCount=0;
            try
            {
                ByteCount = cryptoStream.Read(PlainTextBytes, 0, PlainTextBytes.Length);
            }
            catch (CryptographicException) { return String.Empty; }
            if (ByteCount == 0)
                return String.Empty;
            MemStream.Close();
            cryptoStream.Close();
            return Encoding.UTF8.GetString(PlainTextBytes, 0, ByteCount);
        }
        /// <summary>
        /// Encrypt a string using MD5 Encryption
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string md5Encode(string value)
        {
            MD5 client = MD5.Create();
            byte[] md5 = client.ComputeHash(System.Text.ASCIIEncoding.ASCII.GetBytes(value));
            string ret = String.Empty;
            foreach (byte b in md5)
                ret += b.ToString("X2");
            return ret;
        }
    }
}

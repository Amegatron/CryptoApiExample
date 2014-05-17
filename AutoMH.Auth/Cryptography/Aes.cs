using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AutoMH.Auth.Utility;

namespace AutoMH.Auth.Cryptography
{
    internal class Aes
    {
        private RijndaelManaged _aes = new RijndaelManaged();

        public Aes()
        {
            // AES initialization
            var aesKey = new byte[256 / 8];
            var aesIv = new byte[128 / 8];

            var random = new RNGCryptoServiceProvider();
            random.GetBytes(aesKey);
            random.GetBytes(aesIv);

            _aes.Padding = PaddingMode.PKCS7;
            _aes.Mode = CipherMode.CBC;
            _aes.KeySize = 256;
            _aes.Key = aesKey;
            _aes.IV = aesIv;
        }

        public byte[] Key
        {
            get
            {
                return _aes.Key;
            }
        }

        public byte[] Iv
        {
            get
            {
                return _aes.IV;
            }
        }

        public string Encrypt(string plainText)
        {
            var encryptor = _aes.CreateEncryptor(_aes.Key, _aes.IV);

            var msEncrypt = new MemoryStream();
            var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            var swEncrypt = new StreamWriter(csEncrypt);

            swEncrypt.Write(plainText);

            swEncrypt.Close();
            csEncrypt.Close();

            return Base64.ToUrlSafeBase64(msEncrypt.ToArray());
        }

        public string Decrypt(string encryptedText)
        {
            var decryptor = _aes.CreateDecryptor(_aes.Key, _aes.IV);

            var msDecrypt = new MemoryStream(Base64.FromUrlSafeBase64(encryptedText));
            var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            var srDecrypt = new StreamReader(csDecrypt);

            var plainText = srDecrypt.ReadToEnd();

            srDecrypt.Close();
            csDecrypt.Close();
            msDecrypt.Close();

            return plainText;
        }
    }
}

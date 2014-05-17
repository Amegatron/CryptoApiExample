using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using AutoMH.Auth.Utility;

namespace AutoMH.Auth.Cryptography
{
    internal class Rsa
    {
        private RSACryptoServiceProvider _publicKey;

        public Rsa(string publicKey)
        {
            var cert = GetCertificate(publicKey);
            _publicKey = (RSACryptoServiceProvider)cert.PublicKey.Key;
        }

        private X509Certificate2 GetCertificate(string key)
        {
            try
            {
                if (key.Contains("-----"))
                {
                    // Get just the base64 encoded part of the file then trim off the beginning and ending -----BLAH----- tags
                    key = key.Split(new string[] { "-----" }, StringSplitOptions.RemoveEmptyEntries)[1];
                }

                // Remove "new line" characters
                key = key.Replace("\n", "");

                // Convert the key to a certificate for encryption
                return new X509Certificate2(Convert.FromBase64String(key));
            }
            catch (Exception ex)
            {
                throw new FormatException("The certificate key was not in the expected format.", ex);
            }
        }

        public string Encrypt(string plainText)
        {
            var bytes = Encoding.ASCII.GetBytes(plainText);
            return Base64.ToUrlSafeBase64(_publicKey.Encrypt(bytes, false));
        }

        public bool Verify(byte[] data, byte[] sign)
        {
            string sha1Oid = CryptoConfig.MapNameToOID("SHA1");
            return _publicKey.VerifyData(data, sha1Oid, sign);
        }


    }
}

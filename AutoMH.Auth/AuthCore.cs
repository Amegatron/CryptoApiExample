using System;
using System.Collections.Generic;
using System.Text;
using AutoMH.Auth.Cryptography;
using AutoMH.Auth.Network;
using AutoMH.Auth.Responses;
using AutoMH.Auth.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Aes = AutoMH.Auth.Cryptography.Aes;

namespace AutoMH.Auth
{
    public class AuthCore
    {
        private readonly Rsa _rsa;

        private readonly Aes _aes;

        private readonly HttpClient _httpClient = new HttpClient();

        private readonly string _gateway;

        public LicenseValidationResponse LicenseValidationResponse;

        public AuthCore(string gateway, string certificate)
        {
            _gateway = gateway;        
    
            _rsa = new Rsa(certificate);
            _aes = new Aes();

            //_httpClient.IsDebug = true;
        }

        public void Init()
        {
            var aesKeyEncrypted = _rsa.Encrypt(Convert.ToBase64String(_aes.Key));
            var aesIvEncrypted = _rsa.Encrypt(Convert.ToBase64String(_aes.Iv));

            var postData = new List<KeyValuePair<string, string>>
                               {
                                   new KeyValuePair<string, string>(
                                       "key",
                                       aesKeyEncrypted),
                                   new KeyValuePair<string, string>(
                                       "iv",
                                       aesIvEncrypted)
                               };
            var response = _httpClient.Post(_gateway + "/api/init", postData);
            if (response != "OK")
            {
                throw new Exception("Handshake failed: " + response);
            }
        }

        public bool ValidateLicense(string licenseKey)
        {
            licenseKey = _aes.Encrypt(licenseKey);

            var postData = new List<KeyValuePair<string, string>>
                               {
                                   new KeyValuePair<string, string>(
                                       "licenseKey",
                                       licenseKey)
                               };
            var response = _httpClient.Post(_gateway + "/api/checklicense", postData);
            var obj = JsonConvert.DeserializeObject(response) as JObject;
            var validationResult = ValidateResponse(obj);
            if (!validationResult)
            {
                throw new Exception("Failed to verify received data.");
            }

            if (obj != null)
            {
                var data = (string)obj["data"];
                data = _aes.Decrypt(data);
                LicenseValidationResponse = JsonConvert.DeserializeObject<LicenseValidationResponse>(data);
                return LicenseValidationResponse.isLicenseValid;
            }

            return false;
        }

        private bool ValidateResponse(JObject response)
        {
            var data = Encoding.UTF8.GetBytes((string)response["data"]);
            var sign = Base64.FromUrlSafeBase64((string)response["sign"]);
            return _rsa.Verify(data, sign);
        }

    }
}

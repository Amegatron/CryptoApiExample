using System;
using Newtonsoft.Json;

namespace AutoMH.Auth.Responses
{
    [Serializable]
    public class LicenseValidationResponse
    {
        [JsonProperty("licenseIsValid")]
        public bool isLicenseValid { get; set; }
    }
}

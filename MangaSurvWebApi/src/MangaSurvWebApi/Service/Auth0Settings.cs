﻿namespace MangaSurvWebApi.Service
{
    public class Auth0Settings
    {
        public string Domain { get; set; }

        public string CallbackUrl { get; set; }

        public string ClientId { get; set; }

        public string SecretKey { get; set; }
    }
}

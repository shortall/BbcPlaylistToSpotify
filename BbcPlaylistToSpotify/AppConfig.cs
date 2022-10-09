﻿using Microsoft.Extensions.Configuration;

namespace BbcPlaylistToSpotify
{
    internal class AppConfig
    {
        public string SpotifyUsername { get; set; }
        public string SpotifyApiToken { get; set; }
        public string[] BbcPlaylistUrls { get; set; }

        internal static AppConfig Get()
        {
            var configuration = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile($"appsettings.json");

            var config = configuration.Build();
            var appConfig = new AppConfig();
            config.Bind(appConfig);
            return appConfig;
        }
    }
}

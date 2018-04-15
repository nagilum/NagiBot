namespace NagiBot {
    public class Config {
        public string Nickname { get; set; }
        public string Password { get; set; }
        public string Channel { get; set; }

        public ConfigSpotify Spotify { get; set; }

        public class ConfigSpotify {
            public string AccessToken { get; set; }
            public string RefreshToken { get; set; }
        }
    }
}
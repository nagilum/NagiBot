using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace NagiBot.Modules {
    public class Spotify : IBotModule {
        #region IBotModule

        public Dictionary<string, string> AvailableCommands(IRC irc) {
            return new Dictionary<string, string> {
                {"song", "Get the User's Currently Playing Track"}
            };
        }

        public bool HandleUserCommand(IRC irc, IRC.ChannelMessageEventArgs e) {
            var sections = e.Message.Split(' ');

            switch (sections[0]) {
                case "!song":
                    this.GetUsersCurrentlyPlayingTrack(irc, e);
                    return true;
            }

            return false;
        }

        #endregion

        #region Helper functions

        private void GetUsersCurrentlyPlayingTrack(IRC irc, IRC.ChannelMessageEventArgs e) {
            var res = Tools.Request(
                new Tools.RequestArgs {
                    URL = "https://api.spotify.com/v1/me/player/currently-playing",
                    Headers = new Dictionary<string, string> {
                        {"Authorization", "Bearer " + Program.Config.Spotify.AccessToken}
                    }
                });

            switch (res.StatusCode) {
                case HttpStatusCode.NoContent:
                    irc.WriteToChannel(
                        e.Channel,
                        "Spotify doesn't seem to be running right now.");

                    break;

                case HttpStatusCode.OK:
                    var entry = res.CastTo<SpotifyEntry>();

                    if (entry == null) {
                        irc.WriteToChannel(
                            e.Channel,
                            "Unable to get information from Spotify at the moment.");

                        return;
                    }

                    irc.WriteToChannel(
                        e.Channel,
                        string.Format(
                            "The current track is {0} by {1} which you can find here {2}",
                            entry.item.name,
                            entry.item.artists.First().name,
                            entry.item.external_urls.First().Value));

                    break;
            }
        }

        #endregion

        #region Helper classes

        private class SpotifyEntry {
            public SpotifyEntryItem item { get; set; }

            public class SpotifyEntryItem {
                public List<SpotifyEntryItemArtist> artists { get; set; }
                public Dictionary<string, string> external_urls { get; set; }
                public string name { get; set; }

                public class SpotifyEntryItemArtist {
                    public string name { get; set; }
                }
            }
        }

        #endregion
    }
}
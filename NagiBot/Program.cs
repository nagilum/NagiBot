using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace NagiBot {
    public class Program {
        private static Config Config { get; set; }
        private static Bot Bot { get; set; }

        private static void Main(string[] args) {
            var configFile = args.Any()
                ? string.Join(" ", args)
                : Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "config.json");

            if (!File.Exists(configFile)) {
                Console.WriteLine("Could not find config file: {0}", configFile);
                return;
            }

            try {
                Config = JsonConvert.DeserializeObject<Config>(
                    File.ReadAllText(configFile));

                if (Config == null) {
                    throw new Exception("Error while reading config");
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                return;
            }

            Console.CancelKeyPress += ConsoleOnCancelKeyPress;

            Console.WriteLine("Bot is up and running.");
            Console.WriteLine("Press CTRL+C to abort.");

            try {
                Bot = new Bot(Config);

                if (Bot == null) {
                    throw new Exception("Unable to initialize the bot");
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                return;
            }

            Bot.Connect();
        }

        #region Program Events

        private static void ConsoleOnCancelKeyPress(object sender, ConsoleCancelEventArgs e) {
            if (Bot == null) {
                return;
            }

            Bot.Disconnect();
        }

        #endregion

        
    }
}
using System;

namespace NagiBot {
    public class Bot {
        public Config Config { get; set; }
        private IRC IRC { get; set; }

        public Bot(Config config) {
            this.Config = config;
        }

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        public void Connect() {
            this.IRC = new IRC(
                this.Config.Nickname,
                this.Config.Password,
                this.Config.Channel);

            if (this.IRC == null) {
                throw new Exception("Could not initialize IRC client");
            }

            this.IRC.OnChannelMessage += IrcOnChannelMessage;
            this.IRC.OnConnected += IrcOnConnected;
            this.IRC.OnException += IrcOnException;
            this.IRC.OnReadFromClient += IrcOnReadFromClient;
            this.IRC.OnWriteToClient += IrcOnWriteToClient;

            this.IRC.Connect();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Disconnect() {
            this.IRC.WriteToChannel(Config.Channel, "is going offline");

            this.IRC.WriteToClient(string.Format("PART #{0}", Config.Channel));
            this.IRC.WriteToClient("QUIT");

            this.IRC.Disconnect();
        }

        #endregion

        #region IRC Events
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IrcOnChannelMessage(object sender, IRC.ChannelMessageEventArgs e) {
            // Check for dot-command.
            if (this.HandleDotCommand(e)) {
                // Do Nothing..
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IrcOnConnected(object sender, EventArgs e) {
            this.IRC.WriteToChannel(
                this.Config.Channel,
                "is online");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IrcOnException(object sender, IRC.ExceptionEventArgs e) {
            var color = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e.Exception.Message);

            Console.ForegroundColor = color;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IrcOnReadFromClient(object sender, IRC.ReadFromClientEventArgs e) {
            var color = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("< ");

            Console.ForegroundColor = color;
            Console.WriteLine(e.String);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IrcOnWriteToClient(object sender, IRC.WriteToClientEventArgs e) {
            var color = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("> ");

            Console.ForegroundColor = color;
            Console.WriteLine(e.String);
        }

        #endregion

        #region Channel Handlers

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public bool HandleDotCommand(IRC.ChannelMessageEventArgs e) {
            if (!string.Equals(e.Nickname, this.Config.Channel) ||
                !e.Message.StartsWith("@" + this.Config.Nickname)) {

                return false;
            }

            var message = e.Message
                .Substring(this.Config.Nickname.Length + 2)
                .Trim();

            if (!message.StartsWith(".")) {
                return false;
            }

            var sections = message
                .Substring(1)
                .Split(' ');

            switch (sections[0]) {
                // Tell the bot to disconnect from IRC and close the app.
                case "quit":
                    this.IRC.WriteToChannel(
                        e.Channel,
                        string.Format(
                            "@{0} kk thnx bye",
                            e.Nickname));

                    this.Disconnect();

                    break;

                // Unknown dot-command given, give feedback.
                default:
                    this.IRC.WriteToChannel(
                        e.Channel,
                        string.Format(
                            "@{0} I have no idea what you're talking about..",
                            e.Nickname));

                    break;
            }

            return true;
        }

        #endregion
    }
}
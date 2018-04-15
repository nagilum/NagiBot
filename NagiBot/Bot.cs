using System;

namespace NagiBot {
    public class Bot {
        private IRC IRC { get; set; }

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        public void Connect() {
            this.IRC = new IRC(
                Program.Config.Nickname,
                Program.Config.Password,
                Program.Config.Channel);

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
            this.IRC.WriteToChannel(Program.Config.Channel, "is going offline");

            this.IRC.WriteToClient(string.Format("PART #{0}", Program.Config.Channel));
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
                // Do nothing..
            }

            // Check for user-command.
            else if (this.HandleUserCommand(e)) {
                // Do nothing..
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IrcOnConnected(object sender, EventArgs e) {
            this.IRC.WriteToChannel(
                Program.Config.Channel,
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
            if (!string.Equals(e.Nickname, Program.Config.Channel) ||
                !e.Message.StartsWith("@" + Program.Config.Nickname)) {

                return false;
            }

            var message = e.Message
                .Substring(Program.Config.Nickname.Length + 2)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public bool HandleUserCommand(IRC.ChannelMessageEventArgs e) {
            if (!e.Message.StartsWith("!")) {
                return false;
            }

            var sections = e.Message.Split(' ');

            if (sections[0] == "!help") {
                foreach (var inst in BotModule.GetAll<IBotModule>()) {
                    foreach (var command in inst.AvailableCommands(this.IRC)) {
                        this.IRC.WriteToChannel(
                            e.Channel,
                            string.Format(
                                " {0} - {1}",
                                command.Key,
                                command.Value));
                    }
                }

                return true;
            }

            foreach (var inst in BotModule.GetAll<IBotModule>()) {
                var found = false;

                foreach (var command in inst.AvailableCommands(this.IRC)) {
                    if (sections[0] != "!" + command.Key) {
                        continue;
                    }

                    found = true;
                    break;
                }

                if (!found) {
                    continue;
                }

                if (inst.HandleUserCommand(this.IRC, e)) {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
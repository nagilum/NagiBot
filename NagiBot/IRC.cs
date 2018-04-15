using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;

namespace NagiBot {
    public class IRC {
        public string Nickname { get; set; }
        public string Password { get; set; }
        public string Channel { get; set; }

        public string Hostname = "irc.twitch.tv";
        public int Port = 6667;

        private TcpClient Client { get; set; }
        private NetworkStream Stream { get; set; }
        private StreamReader Reader { get; set; }
        private StreamWriter Writer { get; set; }

        private bool ExitLoop { get; set; }

        public delegate void ChannelMessageEventHandler(object sender, ChannelMessageEventArgs e);
        public delegate void ConnectedEventHandler(object sender, EventArgs e);
        public delegate void ExceptionEventHandler(object sender, ExceptionEventArgs e);
        public delegate void ReadFromClientEventHandler(object sender, ReadFromClientEventArgs e);
        public delegate void WriteToClientEventHandler(object sender, WriteToClientEventArgs e);

        public event ChannelMessageEventHandler OnChannelMessage;
        public event ConnectedEventHandler OnConnected;
        public event ExceptionEventHandler OnException;
        public event ReadFromClientEventHandler OnReadFromClient;
        public event WriteToClientEventHandler OnWriteToClient;

        public IRC(string nickname, string password, string channel) {
            this.Nickname = nickname;
            this.Password = password;
            this.Channel = channel;
        }

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        public void Connect() {
            this.Client = new TcpClient(this.Hostname, this.Port);
            this.Stream = this.Client.GetStream();
            this.Reader = new StreamReader(this.Stream);
            this.Writer = new StreamWriter(this.Stream);

            this.WriteToClient(string.Format("PASS {0}", this.Password));
            this.WriteToClient(string.Format("NICK {0}", this.Nickname));
            this.WriteToClient(string.Format("USER {0} 0 * {0}", this.Nickname));
            this.WriteToClient(string.Format("JOIN #{0}", this.Channel));

            this.OnConnected?.Invoke(
                this,
                new EventArgs());

            while (this.Client.Connected) {
                try {
                    string input;

                    while (this.Client.Connected &&
                           (input = this.Reader.ReadLine()) != null) {

                        if (this.ExitLoop) {
                            break;
                        }

                        this.OnReadFromClient?.Invoke(
                            this,
                            new ReadFromClientEventArgs(input));

                        var sections = input.Split(' ');

                        // PING? PONG?
                        if (sections.Length > 1 &&
                            sections[0] == "PING") {

                            this.WriteToClient(string.Format("PONG {0}", sections[1]));
                        }

                        // PRIVMSG
                        else if (sections.Length > 3 &&
                                 sections[1] == "PRIVMSG") {

                            var message = sections.ToList();

                            message.RemoveAt(0); // Remove the :nick!nick@nick.tmi.twitch.tv part.
                            message.RemoveAt(0); // Remove the PRIVMSG part.
                            message.RemoveAt(0); // Remove the #channel part.

                            this.OnChannelMessage?.Invoke(
                                this,
                                new ChannelMessageEventArgs(
                                    sections[0].Substring(1, sections[0].IndexOf('!') - 1),
                                    sections[2].Substring(1),
                                    string.Join(" ", message).Substring(1)));
                        }
                    }
                }
                catch (Exception ex) {
                    this.OnException?.Invoke(
                        this,
                        new ExceptionEventArgs(ex));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Disconnect() {
            this.Writer.Close();
            this.Reader.Close();
            this.Client.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        public void WriteToChannel(string channel, string message) {
            this.WriteToClient(
                string.Format(
                    "PRIVMSG #{0} :{1}",
                    channel,
                    message));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        public void WriteToClient(string str) {
            Writer.WriteLine(str);
            Writer.Flush();

            this.OnWriteToClient?.Invoke(
                this,
                new WriteToClientEventArgs(str));
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// 
        /// </summary>
        public class ChannelMessageEventArgs : EventArgs {
            public string Nickname { get; set; }
            public string Channel { get; set; }
            public string Message { get; set; }

            public ChannelMessageEventArgs(string nickname, string channel, string message) {
                this.Nickname = nickname;
                this.Channel = channel;
                this.Message = message;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class ExceptionEventArgs : EventArgs {
            public Exception Exception { get; set; }

            public ExceptionEventArgs(Exception exception) {
                this.Exception = exception;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class ReadFromClientEventArgs : EventArgs {
            public string String { get; set; }

            public ReadFromClientEventArgs(string str) {
                this.String = str;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class WriteToClientEventArgs : EventArgs {
            public string String { get; set; }

            public WriteToClientEventArgs(string str) {
                this.String = str;
            }
        }

        #endregion
    }
}
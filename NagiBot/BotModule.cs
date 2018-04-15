using System;
using System.Collections.Generic;
using System.Linq;

namespace NagiBot {
    /// <summary>
    /// 
    /// </summary>
    public interface IBotModule {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Dictionary<string, string> AvailableCommands(IRC irc);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        bool HandleUserCommand(IRC irc, IRC.ChannelMessageEventArgs e);
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class BotModule {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> GetAll<T>() where T : class {
            var type = typeof(T);
            var list = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(a => type.IsAssignableFrom(a) &&
                            a.IsClass)
                .ToList();

            var modules = new List<T>();

            foreach (var module in list) {
                var inst = Activator.CreateInstance(module) as T;

                if (inst != null) {
                    modules.Add(inst);
                }
            }

            return modules;
        }
    }
}
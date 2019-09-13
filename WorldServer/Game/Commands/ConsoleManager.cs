using System;
using System.Collections.Generic;
using System.Threading;
using Common.Logging;
using WorldServer.Game.Objects;

namespace WorldServer.Game.Commands
{
    public class ConsoleManager
    {
        public static Dictionary<string, HandleCommand> CommandHandlers = new Dictionary<string, HandleCommand>();
        public delegate void HandleCommand(Player player, string[] args);

        public static void InitCommands()
        {
            LoadCommandDefinitions();

            while (true)
            {
                Thread.Sleep(5);
                InvokeHandler(Console.ReadLine(), null);
            }
        }

        public static void DefineCommand(string command, HandleCommand handler)
        {
            CommandHandlers[command.ToLower()] = handler;
        }

        public static bool InvokeHandler(string command, Player player)
        {
            if (command != null && player.IsGM)
            {
                string[] lines = command.Split(' ');
                string[] args = new string[lines.Length - 1];
                Array.Copy(lines, 1, args, 0, lines.Length - 1);
                return InvokeHandler(lines[0].ToLower(), player, args);
            }
            return false;      
        }

        public static bool InvokeHandler(string command, Player player, params string[] args)
        {
            command = command.TrimStart('.'); //In game commands forced dot
            if (CommandHandlers.ContainsKey(command))
            {
                CommandHandlers[command].Invoke(player, args);
                return true;
            }
            else
                return false;
        }

        public static void LoadCommandDefinitions()
        {
            DefineCommand("additem", GameMasterCommands.AddItem);
            DefineCommand("addskill", GameMasterCommands.AddSkill);
            DefineCommand("setskill", GameMasterCommands.SetSkill);
            DefineCommand("kill", GameMasterCommands.Kill);
            DefineCommand("level", GameMasterCommands.SetLevel);
            DefineCommand("kick", GameMasterCommands.Kick);
            DefineCommand("money", GameMasterCommands.Money);
            DefineCommand("setpower", GameMasterCommands.SetPower);
            DefineCommand("gps", GameMasterCommands.GPS);
            DefineCommand("tel", GameMasterCommands.Teleport);
            DefineCommand("port", GameMasterCommands.Port);
            DefineCommand("speed", GameMasterCommands.Speed);
            DefineCommand("swim", GameMasterCommands.Swim);
            DefineCommand("morph", GameMasterCommands.Morph);
            DefineCommand("morphf", GameMasterCommands.MorphForward);
            DefineCommand("morphb", GameMasterCommands.MorphBackwards);
            DefineCommand("litickets", GameMasterCommands.ListTickets);
            DefineCommand("ticket", GameMasterCommands.ReadTicket);
            DefineCommand("delticket", GameMasterCommands.DeleteTicket);
        }
    }
}

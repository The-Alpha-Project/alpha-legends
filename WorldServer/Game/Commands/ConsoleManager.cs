using System;
using System.Collections.Generic;
using System.Threading;
using Common.Logging;
using WorldServer.Game.Objects;

namespace WorldServer.Game.Commands
{
    public static class ConsoleManager
    {
        public static Dictionary<string, HandleCommand> CommandHandlers = new Dictionary<string, HandleCommand>();
        public delegate void HandleCommand(Player player, string[] args);

        public static Dictionary<string, ConsoleHandleCommand> ConsoleCommandHandlers = new Dictionary<string, ConsoleHandleCommand>();
        public delegate void ConsoleHandleCommand(string[] args);

        public static void InitCommands()
        {
            LoadCommandDefinitions();

            while (true)
            {
                Thread.Sleep(5);
                InvokeConsoleHandler(Console.ReadLine());
            }
        }

        public static void DefineConsoleCommand(string command, ConsoleHandleCommand handler)
        {
            ConsoleCommandHandlers[command.ToLower()] = handler;
        }

        public static bool InvokeConsoleHandler(string command)
        {
            if (command != null)
            {
                string[] lines = command.Split(' ');
                string[] args = new string[lines.Length - 1];
                Array.Copy(lines, 1, args, 0, lines.Length - 1);
                if (ConsoleCommandHandlers.ContainsKey(lines[0]))
                {
                    ConsoleCommandHandlers[lines[0]].Invoke(args);
                    return true;
                }
            }
            return false;
        }

        public static void DefineCommand(string command, HandleCommand handler)
        {
            CommandHandlers[command.ToLower()] = handler;
        }

        public static bool InvokeHandler(string command, Player player)
        {
            if (command != null && command.StartsWith(".", StringComparison.Ordinal) && player != null && player.IsGM)
            {
                command = command.TrimStart('.'); //In game commands forced dot
                string[] lines = command.Split(' ');
                string[] args = new string[lines.Length - 1];
                Array.Copy(lines, 1, args, 0, lines.Length - 1);
                if (CommandHandlers.ContainsKey(lines[0]))
                {
                    CommandHandlers[lines[0]].Invoke(player, args);
                    return true;
                }
            }
            return false;      
        }

        public static void LoadCommandDefinitions()
        {
            DefineCommand("additem", GameMasterCommands.AddItem);
            DefineCommand("addskill", GameMasterCommands.AddSkill);
            DefineCommand("setskill", GameMasterCommands.SetSkill);
            DefineCommand("die", GameMasterCommands.Kill);
            DefineCommand("level", GameMasterCommands.SetLevel);
            DefineCommand("kick", GameMasterCommands.Kick);
            DefineCommand("money", GameMasterCommands.Money);
            DefineCommand("setpower", GameMasterCommands.SetPower);
            DefineCommand("gps", GameMasterCommands.GPS);
            DefineCommand("tele", GameMasterCommands.Teleport);
            DefineCommand("port", GameMasterCommands.Port);
            DefineCommand("speed", GameMasterCommands.Speed);
            DefineCommand("swim", GameMasterCommands.Swim);
            DefineCommand("morph", GameMasterCommands.Morph);
            DefineCommand("morphf", GameMasterCommands.MorphForward);
            DefineCommand("morphb", GameMasterCommands.MorphBackwards);
            DefineCommand("litickets", GameMasterCommands.ListTickets);
            DefineCommand("ticket", GameMasterCommands.ReadTicket);
            DefineCommand("delticket", GameMasterCommands.DeleteTicket);
            DefineCommand("uinfo", GameMasterCommands.UnitInfo);
            DefineCommand("mount", GameMasterCommands.Mount);
            DefineCommand("unmount", GameMasterCommands.Unmount);
            DefineCommand("goinfo", GameMasterCommands.GObjectInfo);

            DefineConsoleCommand("shutdown", ConsoleCommands.Shutdown);
            DefineConsoleCommand("gmset", ConsoleCommands.GMSet);
            DefineConsoleCommand("createacc", ConsoleCommands.CreateAcc);
        }
    }
}

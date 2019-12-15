using Common.Database.DBC;
using Common.Network.Packets;
using WorldServer.Game.Objects;
using WorldServer.Game.Objects.PlayerExtensions;
using WorldServer.Game.Structs;
using WorldServer.Game.Managers;
using WorldServer.Packets.Handlers;
using WorldServer.Storage;
using Common.Constants;
using WorldServer.Game.Objects.PlayerExtensions.Skill;
using System.Threading.Tasks;
using System;
using Common.Helpers;
using Common.Logging;

namespace WorldServer.Game.Commands
{
    public class GameMasterCommands : CommandParser
    {
        public static void AddItem(Player player, string[] args)
        {
            bool success = false;
            uint itemid = Read<uint>(args, 0);
            uint amount = (args.Length > 1 ? Read<uint>(args, 1) : 1); //optional

            if (!Database.ItemTemplates.ContainsKey(itemid) || player == null)
                return;

            ItemTemplate template = Database.ItemTemplates.TryGet(itemid);
            
            if (amount <= template.MaxStackCount)
            {
                Item item = Database.ItemTemplates.CreateItemOrContainer(itemid);
                item.StackCount = amount;
                player.AddItem(item);
                success = true;
            }
            else
            {
                uint stackcount = amount;
                for (int i = 0; i < Math.Ceiling((float)amount / template.MaxStackCount); i++)
                {
                    uint amt = (stackcount >= template.MaxStackCount ? template.MaxStackCount : stackcount);

                    Item item = Database.ItemTemplates.CreateItemOrContainer(itemid);
                    item.StackCount = amt;
                    player.AddItem(item);
                    stackcount -= amt;
                }

                success = true;
            }

            player.Dirty |= success;
        }

        public static void AddSkill(Player player, string[] args)
        {
            ushort skillid = Read<ushort>(args, 0);

            if (!DBC.SkillLine.ContainsKey(skillid) ||
                 player == null ||
                 player?.Skills.ContainsKey(skillid) == true)
                return;

            if (player.AddSkill(skillid)) //Success
                player.Dirty = true;
        }

        public static void SetSkill(Player player, string[] args)
        {
            bool success = false;
            ushort skillid = Read<ushort>(args,0);
            ushort curvalue = Read<ushort>(args, 1);
            ushort maxvalue = (args.Length > 1 ? Read<ushort>(args, 2) : (ushort)0); //optional

            if (!DBC.SkillLine.ContainsKey(skillid) || player == null)
                return;

            if (args.Length > 3)
                success = player.SetSkill(skillid, curvalue, maxvalue);
            else
                success = player.SetSkill(skillid, curvalue);

            player.Dirty |= success;
        }

        public static void Kill(Player player, string[] args)
        {
            if (player.CurrentSelection == 0 || !Database.Creatures.ContainsKey(player.CurrentSelection))
                return;

            Creature target = Database.Creatures.TryGet(player.CurrentSelection);
            target.Attackers.TryAdd(player.Guid, player);
            target.Die(player);
            player.Dirty = true;
        }

        public static void SetLevel(Player player, string[] args)
        {
            byte level = Read<byte>(args, 0);
            if (level > 0 && level != player.Level)
            {
                if (level > Globals.MAX_LEVEL)
                    level = Globals.MAX_LEVEL;

                player.GiveLevel(level);

                GridManager.Instance.SendSurrounding(player.BuildUpdate(), player);
            }
        }

        public static void Kick(Player player, string[] args)
        {
            string playername = Read<string>(args, 0);
            Player target = Database.Players.TryGetName(playername);
            if (target != null)
                target.Kick();
        }

        public static void Money(Player player, string[] args)
        {
            int money = Read<int>(args, 0);
            uint moneyabs = (uint)Math.Abs(money);

            if (money < 0 && player.Money >= moneyabs)
                player.Money -= moneyabs;
            else if (money < 0 && player.Money < moneyabs)
                player.Money = 0;
            else
                player.Money += (uint)money;

            player.Dirty = true;
        }

        public static void SetPower(Player player, string[] args)
        {
            uint power = Read<uint>(args, 0);
            player.SetPowerValue(power, false);
            player.Dirty = true;
        }

        public static void GPS(Player player, string[] args)
        {
            ChatManager.Instance.SendSystemMessage(player, string.Format("X: {0}, Y: {1}, Z: {2}, Orientation: {3} Map: {4}", 
                player.Location.X, player.Location.Y, player.Location.Z, player.Orientation, player.Map));
        }

        public static void Teleport(Player player, string[] args)
        {
            if(args.Length >= 4)
            {
                float x = Read<float>(args, 0);
                float y = Read<float>(args, 1);
                float z = Read<float>(args, 2);
                uint map = Read<uint>(args, 3);
                player.Teleport(map, new Quaternion(x, y, z, 0));
            } 
            else
            {
                ChatManager.Instance.SendSystemMessage(player, "Usage: .tel X Y Z MapID");
            }
        }

        public static void Port(Player player, string[] args)
        {
            if(args.Length > 0)
            {
                string name = Read<string>(args, 0);
                foreach (Worldports port in Database.Worldports.Values)
                    if (port.Name.StartsWith(name, StringComparison.InvariantCultureIgnoreCase))
                        player.Teleport(port.Map, new Quaternion(port.X, port.Y, port.Z, 0));
            }
        }

        private static void ApplySpeedAction(Player player, float speed, Boolean IsRun)
        {
            MiscHandler.HandleForceSpeedChange(ref player.Client, speed, IsRun);
        }

        public static void Speed(Player player, string[] args) 
        {
           ApplySpeedAction(player, 7.0f * Read<float>(args, 0), true);
        }

        public static void Swim(Player player, string[] args)
        {
            ApplySpeedAction(player, 4.7222223f * Read<float>(args, 0), false);
        }

        private static void ApplyMorphAction(Player player, uint morph, Boolean announce = false)
        {
            player.DisplayID = morph;
            player.Client.Send(player.BuildUpdate());
            if (announce)
                ChatManager.Instance.SendSystemMessage(player, string.Format("Current Display ID: {0}.", player.DisplayID));
        }

        public static void Morph(Player player, string[] args)
        {
            ApplyMorphAction(player, Read<uint>(args, 0));
        }

        public static void MorphForward(Player player, string[] args)
        {
            ApplyMorphAction(player, player.DisplayID += 1, true);
        }

        public static void MorphBackwards(Player player, string[] args)
        {
            ApplyMorphAction(player, player.DisplayID -= 1, true);
        }

        public static void ListTickets(Player player, string[] args)
        {
            if (Database.Tickets.Values.Count > 0)
                foreach(Ticket ticket in Database.Tickets.Values)
                    ChatManager.Instance.SendSystemMessage(player, string.Format("[{0}] from {1} ({2}).", ticket.Id, ticket.CharacterName, 
                        ticket.SubmitTime.ToString("dd-MM-yyyy HH:mm")));
            else
                ChatManager.Instance.SendSystemMessage(player, "There are no open tickets.");
        }

        public static void ReadTicket(Player player, string[] args)
        {
            uint id = Read<uint>(args, 0);
            Ticket ticket = Database.Tickets.TryGet(id);
            if (ticket != null)
                ChatManager.Instance.SendSystemMessage(player, string.Format("Player \"{0}\" from account \"{1}\" says ({2}): {3}", ticket.CharacterName,
                    ticket.AccountName, ticket.IsBug ? "bug" : "suggestion", ticket.TextBody));
            else
                ChatManager.Instance.SendSystemMessage(player, "Can't find that ticket.");
        }

        public static void DeleteTicket(Player player, string[] args)
        {
            uint id = Read<uint>(args, 0);
            if (Database.Tickets.TryRemove(id))
            {
                ChatManager.Instance.SendSystemMessage(player, string.Format("Ticket {0} succesfully removed.", id));
                Database.Tickets.UpdateChanges();
            }
            else
                ChatManager.Instance.SendSystemMessage(player, "Can't find that ticket.");

        }

        public static void UnitInfo(Player player, string[] args)
        {
            Unit unit = Database.Creatures.TryGet(player.CurrentSelection);
            if (unit == null)
                unit = Database.Players.TryGet(player.CurrentSelection); // Trying to get player instead
            if (unit != null)
            {
                if (unit.IsTypeOf(ObjectTypes.TYPE_PLAYER))
                {
                    ChatManager.Instance.SendSystemMessage(player, string.Format("Guid: {0}, Name: {1}, Account: {2}", 
                        unit.Guid, ((Player)unit).Name, ((Player)unit).AccountId));
                } else
                {
                    ChatManager.Instance.SendSystemMessage(player, string.Format("Guid: {0}, Entry: {1}, Display ID: {2} X: {3}, Y: {4}, Z: {5}, Orientation: {6} Map: {7}", 
                        unit.Guid & ~(ulong)HIGH_GUID.HIGHGUID_UNIT, unit is Creature ? ((Creature)unit).Entry : 0, ((Creature)unit).DisplayID, unit.Location.X, unit.Location.Y, unit.Location.Z, unit.Orientation, unit.Map));
                }
            }
        }

        public static void Mount(Player player, string[] args)
        {
            uint display_id = Read<uint>(args, 0);
            player.Mount(display_id);
        }

        public static void Unmount(Player player, string[] args)
        {
            player.Unmount();
        }

        public static void GObjectInfo(Player player, string[] args)
        {
            foreach(WorldObject obj in GridManager.Instance.GetSurroundingObjects(player))
            {
                if (obj is GameObject)
                {
                    uint max_distance = Read<uint>(args, 0);
                    float distance = player.Location.Distance(new Vector(obj.Location.X, obj.Location.Y, obj.Location.Z));
                    if (distance <= max_distance)
                    {
                        ChatManager.Instance.SendSystemMessage(player, string.Format("Name: {0}, Guid: {1}, Entry: {2}, Display ID: {3} X: {4}, Y: {5}, Z: {6}, Orientation: {7} Map: {8}, Distance: {9}",
                                ((GameObject)obj).Template.Name, obj.Guid & ~(ulong)HIGH_GUID.HIGHGUID_GAMEOBJECT, ((GameObject)obj).Entry, obj.DisplayID, obj.Location.X, obj.Location.Y, obj.Location.Z, obj.Orientation, obj.Map, distance));
                    }
                }
            }
        }
    }

    public class ConsoleCommands : CommandParser
    {
        public static void Shutdown(string[] args)
        {
            // TODO: Make this really a safe shutdown
            Log.Message(LogType.NORMAL, "Shutting down the server...");
            Database.SaveChanges();
            GC.Collect();
            Environment.Exit(0);
        }

        public static void GMSet(string[] args)
        {
            string accountname = Read<string>(args, 0);
            Account account = Database.Accounts.GetByName(accountname);
            if (account != null)
            {
                account.GMLevel = 1;
                Database.Accounts.UpdateChanges();
                Log.Message(LogType.NORMAL, "Enabled GM mode for account {0}, reloading.", accountname);
                Database.Accounts.Reload();
            } else
            {
                Log.Message(LogType.ERROR, "Can't find that account.");
            }
        }

        public static void CreateAcc(string[] args)
        {
            string name = Read<string>(args, 0);
            string pass = Read<string>(args, 1);
            uint gmlevel = Read<uint>(args, 2);
            Account account = Database.Accounts.GetByName(name);
            if (account == null)
            {
                if (!string.IsNullOrWhiteSpace(pass) && !string.IsNullOrWhiteSpace(name))
                {
                    account = new Account
                    {
                        Name = name,
                        GMLevel = (byte)(gmlevel > 0 ? 1 : 0),
                        IP = "0.0.0.0"
                    };
                    account.SetPassword(pass);

                    if (Database.Accounts.TryAdd(account))
                    {
                        Database.Accounts.Save(account);
                        Log.Message(LogType.NORMAL, "Account succesfully created, reloading.");
                        Database.Accounts.Reload();
                    }
                    else
                    {
                        Log.Message(LogType.ERROR, "Error creating account.");
                    }
                }
            }
            else
            {
                Log.Message(LogType.ERROR, "Account already exists.");
            }
        }
    }


}

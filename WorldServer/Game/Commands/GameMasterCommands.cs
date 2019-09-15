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
using System;
using Common.Helpers;

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

            if (success) //Success
                player.Dirty = true;
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

            if (success) //Success
                player.Dirty = true;
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
            float x = Read<float>(args, 0);
            float y = Read<float>(args, 1);
            float z = Read<float>(args, 2);
            uint map = Read<uint>(args, 3);
            player.Teleport(map, new Quaternion(x, y, z, 0));
        }

        public static void Port(Player player, string[] args)
        {
            string name = Read<string>(args, 0);
            foreach (Worldports port in Database.Worldports.Values)
                if (port.Name.StartsWith(name, StringComparison.InvariantCultureIgnoreCase))
                    player.Teleport(port.Map, new Quaternion(port.X, port.Y, port.Z, 0));
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
    }
}

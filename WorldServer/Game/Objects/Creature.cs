using Common.Constants;
using Common.Helpers;
using Common.Helpers.Extensions;
using Common.Network;
using Common.Network.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using WorldServer.Game.Structs;
using WorldServer.Storage;
using WorldServer.Game.Managers;
using WorldServer.Game.Objects.PlayerExtensions.Quests;
using MySql.Data.MySqlClient;
using Common.Database;

namespace WorldServer.Game.Objects
{
    [Table("spawns_creatures")]
    public class Creature : Unit
    {
        public uint Entry;
        public byte NPCFlags;
        public uint MobType;
        public bool isElite = false;
        public TRandom Levels;
        public CreatureTemplate Template;
        public int RespawnTime;
        public int RespawnDistance;

        public bool IsSkinned = false;
        public List<LootObject> Loot = new List<LootObject>();
        public List<LootObject> SkinningLoot = new List<LootObject>();
        public List<VendorItem> VendorLoot = new List<VendorItem>();

        private Vector RespawnLocation = new Vector();
        private Vector CombatStartLocation = new Vector();
        private Vector MoveLocation = null;

        private long CorpseRespawnTime;
        private long CorpseRemoveTime;

        public Creature() { }

        public Creature(ref MySqlDataReader dr)
        {
            this.ObjectType |= ObjectTypes.TYPE_UNIT;
            this.Guid = Convert.ToUInt64(dr["spawn_id"]) | (ulong)HIGH_GUID.HIGHGUID_UNIT;
            this.Entry = Convert.ToUInt32(dr["spawn_entry"]);
            this.Map = Convert.ToUInt32(dr["spawn_map"]);
            this.DisplayID = Convert.ToUInt32(dr["spawn_displayid"]);
            this.Location = new Vector(Convert.ToSingle(dr["spawn_positionX"]),
                                       Convert.ToSingle(dr["spawn_positionY"]),
                                       Convert.ToSingle(dr["spawn_positionZ"]));
            this.Orientation = Convert.ToSingle(dr["spawn_orientation"]);
            this.Health = new TStat { BaseAmount = Convert.ToUInt32(dr["spawn_curhealth"]) };
            this.Mana = new TStat { BaseAmount = Convert.ToUInt32(dr["spawn_curmana"]) };
            this.RespawnTime = Convert.ToInt32(dr["spawn_spawntime"]);
            this.RespawnDistance = Convert.ToInt32(dr["spawn_spawndist"]);

            this.OnDbLoad();
        }

        #region Database Functions
        public void OnDbLoad()
        {
            this.Template = Database.CreatureTemplates.TryGet(this.Entry);
            SetRespawn();
            this.Health.SetAll(this.Template.Health.Maximum);
            this.Mana.SetAll(this.Template.Mana.Maximum);
            this.BaseAttackTime = (uint)this.Template.AttackTime;
            this.DynamicFlags = this.Template.DynamicFlags;
            this.NPCFlags = (byte)this.Template.NPCFlags;
            this.Faction = this.Template.Faction;
            this.VendorLoot = this.Template.VendorItems;
            this.Level = this.Template.Level.GetRandom();
            GridManager.Instance.AddOrGet(this, true);
        }

        public void Save()
        {
            List<string> columns = new List<string> {
                "spawn_id", "spawn_entry", "spawn_map", "spawn_displayid", "spawn_positionX",
                "spawn_positionY", "spawn_positionZ", "spawn_orientation", "spawn_curhealth",
                "spawn_curmana", "spawn_spawntime", "spawn_spawndist"
            };

            List<MySqlParameter> parameters = new List<MySqlParameter>
            {
                new MySqlParameter("@spawn_id", this.Guid & ~(ulong)HIGH_GUID.HIGHGUID_UNIT),
                new MySqlParameter("@spawn_entry", this.Entry),
                new MySqlParameter("@spawn_map", this.Map),
                new MySqlParameter("@spawn_displayid", this.DisplayID),
                new MySqlParameter("@spawn_positionX", Location.X),
                new MySqlParameter("@spawn_positionY", Location.Y),
                new MySqlParameter("@spawn_positionZ", Location.Z),
                new MySqlParameter("@spawn_orientation", Orientation),
                new MySqlParameter("@spawn_curhealth", Health.BaseAmount),
                new MySqlParameter("@spawn_curmana", Mana.BaseAmount),
                new MySqlParameter("@spawn_spawntime", RespawnTime),
                new MySqlParameter("@spawn_spawndist", RespawnDistance)
            };

            BaseContext.SaveEntity("spawns_creatures", columns, parameters, Globals.CONNECTION_STRING);
        }
        #endregion

        #region Packet Functions
        public override PacketWriter BuildUpdate(UpdateTypes type = UpdateTypes.UPDATE_PARTIAL, bool self = false)
        {
            //Send update packet
            PacketWriter writer = CreateObject(false);
            UpdateClass uc = new UpdateClass();

            //Object Fields
            uc.UpdateValue<ulong>(ObjectFields.OBJECT_FIELD_GUID, this.Guid);
            uc.UpdateValue<uint>(ObjectFields.OBJECT_FIELD_TYPE, (uint)this.ObjectType); // UpdateType, 0x9 - (Unit + Object)
            uc.UpdateValue<uint>(ObjectFields.OBJECT_FIELD_ENTRY, this.Entry);
            uc.UpdateValue<float>(ObjectFields.OBJECT_FIELD_SCALE_X, this.Scale);

            //Unit Fields
            uc.UpdateValue<uint>(UnitFields.UNIT_CHANNEL_SPELL, this.ChannelSpell);
            uc.UpdateValue<ulong>(UnitFields.UNIT_FIELD_CHANNEL_OBJECT, this.ChannelSpell);
            uc.UpdateValue<uint>(UnitFields.UNIT_FIELD_HEALTH, this.Health.Current);
            uc.UpdateValue<uint>(UnitFields.UNIT_FIELD_MAXHEALTH, this.Health.Maximum);
            uc.UpdateValue<uint>(UnitFields.UNIT_FIELD_LEVEL, this.Level);
            uc.UpdateValue<uint>(UnitFields.UNIT_FIELD_FACTIONTEMPLATE, this.Template.Faction);
            uc.UpdateValue<uint>(UnitFields.UNIT_FIELD_FLAGS, this.UnitFlags);
            uc.UpdateValue<float>(UnitFields.UNIT_FIELD_BASEATTACKTIME, this.Template.AttackTime); //Main hand
            uc.UpdateValue<float>(UnitFields.UNIT_FIELD_BASEATTACKTIME, 0f, 1); //Offhand
            uc.UpdateValue<int>(UnitFields.UNIT_FIELD_RESISTANCES, this.Template.Armor.BaseAmount);
            uc.UpdateValue<int>(UnitFields.UNIT_FIELD_RESISTANCES, this.Template.Holy.BaseAmount, 1);
            uc.UpdateValue<int>(UnitFields.UNIT_FIELD_RESISTANCES, this.Template.Fire.BaseAmount, 2);
            uc.UpdateValue<int>(UnitFields.UNIT_FIELD_RESISTANCES, this.Template.Nature.BaseAmount, 3);
            uc.UpdateValue<int>(UnitFields.UNIT_FIELD_RESISTANCES, this.Template.Frost.BaseAmount, 4);
            uc.UpdateValue<int>(UnitFields.UNIT_FIELD_RESISTANCES, this.Template.Shadow.BaseAmount, 5);
            uc.UpdateValue<float>(UnitFields.UNIT_FIELD_BOUNDINGRADIUS, this.Template.BoundingRadius);
            uc.UpdateValue<float>(UnitFields.UNIT_FIELD_COMBATREACH, this.Template.CombatReach);
            uc.UpdateValue<uint>(UnitFields.UNIT_FIELD_DISPLAYID, this.Template.ModelID1);
            uc.UpdateValue<uint>(UnitFields.UNIT_FIELD_COINAGE, this.Money);
            uc.UpdateValue<float>(UnitFields.UNIT_MOD_CAST_SPEED, 1f);
            uc.UpdateValue<uint>(UnitFields.UNIT_FIELD_DAMAGE, ByteConverter.ConvertToUInt32((ushort)this.Template.Damage.Current, (ushort)this.Template.Damage.Maximum));
            uc.UpdateValue<uint>(UnitFields.UNIT_DYNAMIC_FLAGS, this.DynamicFlags);
            uc.UpdateValue<uint>(UnitFields.UNIT_FIELD_BYTES_1, ByteConverter.ConvertToUInt32(this.StandState, this.NPCFlags, 0, 0));
            uc.BuildPacket(ref writer, true);

            writer.Compress();
            return writer;
        }

        public void TurnTo(Vector location)
        {
            this.Orientation = GetOrientation(this.Location.X, location.X, this.Location.Y, location.Y);
            PacketWriter pkt = new PacketWriter(Opcodes.MSG_MOVE_HEARTBEAT);
            pkt.WriteUInt64(this.Guid);
            pkt.WriteUInt32((uint)Globals.TimeTicks);
            pkt.WriteVector(this.Location);
            pkt.WriteFloat(this.Orientation);
            pkt.WriteUInt32(this.MovementFlags); // I'm not 100% sure about this one.
            GridManager.Instance.SendSurrounding(pkt, this);
        }

        public void MoveTo(Vector loc, bool run, float distance = 0)
        {
            if (Math.Abs(distance) < float.Epsilon)
                distance = this.Location.Distance(loc);
            uint moveTime = (uint)((distance / this.Template.Speed * 1000) / RunningSpeed);

            this.MoveLocation = loc;

            SendMoveToPacket(loc, moveTime, run, this.Orientation);
        }
         
        public void SendMoveToPacket(Vector loc, uint time, bool run, float orientation = 0)
        {
            PacketWriter pw = new PacketWriter(Opcodes.SMSG_MONSTER_MOVE);
            pw.WriteUInt64(this.Guid);
            pw.WriteFloat(this.Location.X);
            pw.WriteFloat(this.Location.Y);
            pw.WriteFloat(this.Location.Z);
            pw.WriteFloat(orientation);
            pw.WriteUInt8(0);
            pw.WriteUInt32((uint)(run ? 0x100 : 0x000)); //Flags : 0x000 - Walk, 0x100 - Run
            pw.WriteUInt32(time);
            pw.WriteUInt32(1);
            pw.WriteFloat(loc.X);
            pw.WriteFloat(loc.Y);
            pw.WriteFloat(loc.Z);

            GridManager.Instance.SendSurrounding(pw, this);

            this.Location = loc;
            //this.Orientation = this.Location.Angle(loc);
        }
        #endregion

        #region Life Functions
        public void SetRespawn()
        {
            this.RespawnLocation = this.Location;
        }

        public void SetCombatStartLocation()
        {
            this.CombatStartLocation = this.Location;
        }

        public void Die(WorldObject killer)
        {
            if (this.IsDead)
                return;

            GenerateLoot();

            this.IsDead = true;
            this.IsAttacking = false;
            this.InCombat = false;
            this.CombatTarget = 0;
            this.Health.Current = 0;

            this.UnitFlags = (uint)Common.Constants.UnitFlags.UNIT_FLAG_DEAD;
            this.DynamicFlags = (uint)UnitDynamicTypes.UNIT_DYNAMIC_DEAD;
            this.StandState = (byte)Common.Constants.StandState.UNIT_DEAD;

            this.CorpseRespawnTime = Globals.GetFutureTime(this.RespawnTime);
            this.CorpseRemoveTime = Globals.GetFutureTime(this.RespawnTime * 0.9f);

            if (killer.IsTypeOf(ObjectTypes.TYPE_PLAYER))
            {
                RewardKillXP((Player)killer);

                foreach (Unit unit in InvolvedPlayers((Player)killer))
                {
                    if (unit.IsTypeOf(ObjectTypes.TYPE_PLAYER))
                        ((Player)unit).CheckQuestCreatureKill(this.Guid);

                    if (unit.Attackers.ContainsKey(this.Guid))
                        unit.Attackers.TryRemove(this.Guid, out Unit dump);
                }
            }
            this.Attackers.Clear();

            GridManager.Instance.SendSurrounding(this.BuildUpdate(), this);
        }

        public void Respawn()
        {
            this.Location = this.RespawnLocation;
            this.IsDead = false;
            this.IsAttacking = false;
            this.InCombat = false;
            this.Attackers.Clear();
            this.Health.SetAll(this.Health.Maximum);
            this.UnitFlags = this.Template.UnitFlags;
            this.DynamicFlags = this.Template.DynamicFlags;
            this.NPCFlags = (byte)this.Template.NPCFlags;
            this.StandState = 0;
            this.IsSkinned = false;
            this.Loot.Clear();
            this.SkinningLoot.Clear();

            GridManager.Instance.SendSurrounding(this.BuildUpdate(UpdateTypes.UPDATE_FULL), this);
        }
        #endregion

        #region QuestAndXp Functions
        private uint CalculateXPReward(Player p) //TODO: Group values
        {
            byte plevel = p.Level;

            int graylevel = 0;
            if (plevel > 5 && plevel < 50)
                graylevel = (int)(p.Level - Math.Floor(plevel / 10f) - 5);
            else if (plevel == 50)
                graylevel = 40;
            else if (plevel > 50 && plevel < 60)
                graylevel = (int)(p.Level - Math.Floor(plevel / 5f) - 1);
            else if (plevel == 60)
                return 0;

            if (Level <= graylevel)
                return 0;

            uint basexp = (uint)((plevel * 5) + 45);
            int multi = (isElite && Level - plevel <= 4 ? 2 : 1); //Elites produce double XP
            if (plevel < Level)
            {
                if (Level - plevel > 4)
                    plevel = (byte)(Level - 4); // Red mobs cap out at the same experience as orange.
                basexp = (uint)(basexp * (1 + (0.05 * (Level - plevel))));
            }
            else if (plevel > Level)
                basexp = (uint)(basexp * (1 - (plevel - Level) / FormulaData.ZeroDifferenceValue(plevel)));
            return (uint)(basexp * multi);
        }

        public void RewardKillXP(Player player)
        {
            //Individual XP
            if (player.Group == null)
            {
                player.GiveXp(CalculateXPReward(player), this.Guid);
                return;
            }

            //Group XP
            //  Get players in group eligable for XP, in range and not dead
            HashSet<Player> group = player.Group.GetGroupInRange(player, Globals.MAX_GROUP_XP_DISTANCE);
            Player highestlevel = group.OrderByDescending(x => x.Level).First(); //Get highest level
            byte sumlevels = (byte)group.Sum(x => x.Level); //Get sum of levels
            uint basexp = CalculateXPReward(highestlevel);

            foreach (Player p in group)
                p.GiveXp(basexp * p.Level / sumlevels, this.Guid); //Reward each player with their own calc
        }

        public void SendLootRelease(Player p)
        {
            PacketWriter pw = new PacketWriter(Opcodes.SMSG_LOOT_RELEASE_RESPONSE);
            pw.WriteUInt64(this.Guid);
            pw.WriteUInt8(1);
            p.Client.Send(pw);
        }

        private HashSet<Player> InvolvedPlayers(Player killer)
        {
            HashSet<Player> players = new HashSet<Player>();

            if (this.Attackers.Count == 0)
                this.Attackers.TryAdd(killer.Guid, killer);

            //No mob tagging so presumably any player that hit this creature gets the check?
            foreach (Unit unit in this.Attackers.Values)
            {
                if (unit.IsTypeOf(ObjectTypes.TYPE_PLAYER))
                {
                    if (((Player)unit).Group != null)
                        players.UnionWith(((Player)unit).Group.GetGroupInRange(this, Globals.MAX_GROUP_XP_DISTANCE)); //Get inrange group
                    else if (unit.Location.Distance(this.Location) <= Globals.MAX_GROUP_XP_DISTANCE) //Get inrange others
                        players.Add((Player)unit);
                }
            }

            return players;
        }

        #endregion

        #region Item Functions
        public void GenerateLoot()
        {
            this.Money = this.Template.Gold.GetRandom();
            this.Loot.Clear();
            HashSet<LootItem> loot = new HashSet<LootItem>();
            Dictionary<int, List<LootItem>> lootgroups = Database.CreatureLoot.TryGet(this.Entry)?
                                                         .GroupBy(x => x.GroupId).ToDictionary(gr => gr.Key, gr => gr.ToList());

            if (lootgroups.Count == 0)
                return;

            int maxKey = lootgroups.Max(x => x.Key);

            for (int i = 0; i <= maxKey; i++)
            {
                if (!lootgroups.ContainsKey(i))
                    continue;

                float rollchance = (float)new Random().NextDouble() * 100f;
                for (int x = 0; x < lootgroups[i].Count; x++)
                {
                    if (lootgroups[i][x].Chance >= 100 && Database.ItemTemplates.ContainsKey(lootgroups[i][x].Item))
                    {
                        loot.Add(lootgroups[i][x]);
                        break;
                    }

                    rollchance -= lootgroups[i][x].Chance;
                    if (rollchance <= 0 && Database.ItemTemplates.ContainsKey(lootgroups[i][x].Item))
                    {
                        loot.Add(lootgroups[i][x]);
                        break;
                    }
                }
            }

            foreach (LootItem li in loot.ToArray()) //Generate loot based on item chance
            {
                if (!Database.ItemTemplates.ContainsKey(li.Item))
                    continue;

                Item item = Database.ItemTemplates.CreateItemOrContainer(li.Item);
                item.CurrentSlot = item.EquipSlot;
                item.Owner = this.Guid;
                item.Contained = this.Guid;
                item.Type = (InventoryTypes)item.Template.InvType;
                item.DisplayID = item.Template.DisplayID;
                item.StackCount = (uint)(new Random().Next(1, (int)item.Template.MaxStackCount));
                Database.Items.TryAdd(item);

                this.Loot.Add(new LootObject
                {
                    Item = item,
                    Count = (uint)(new Random().Next(li.MinCount, li.MaxCount)),
                    IsQuestItem = li.QuestItem
                });
            }

            if (this.Loot.Count > 0 || this.Money > 0)
                Flag.SetFlag(ref DynamicFlags, (uint)UnitDynamicTypes.UNIT_DYNAMIC_LOOTABLE);
        }

        public void UpdateInventoryItem(uint entry, int count)
        {
            foreach (VendorItem vi in VendorLoot)
            {
                if (vi.Item == entry)
                {
                    vi.CurCount -= count;
                    if (vi.CurCount <= 0)
                        vi.UpdateTime = Globals.GetFutureTime(vi.RespawnSeconds);
                    break;
                }
            }
        }

        public void UpdateInventory(long time)
        {
            if (VendorLoot == null)
                return;

            foreach (VendorItem vi in VendorLoot)
                if (vi.MaxCount > 0 && vi.CurCount == 0 && vi.UpdateTime <= time)
                    vi.CurCount = vi.MaxCount;
        }

        public PacketWriter ListInventory(Player p)
        {
            byte itemcount = (byte)(Template.VendorItems?.Count() ?? 0);

            PacketWriter pw = new PacketWriter(Opcodes.SMSG_LIST_INVENTORY);
            pw.WriteUInt64(Guid);
            pw.WriteUInt8(itemcount); //Item count? If 0 reset of data ignored

            if (itemcount == 0) //No items to send
            {
                pw.WriteUInt8(0);
            }
            else //Add all items
            {
                foreach (VendorItem itm in VendorLoot)
                {
                    if (!Database.ItemTemplates.ContainsKey(itm.Item))
                        continue;

                    ItemTemplate tmp = Database.ItemTemplates.TryGet(itm.Item);
                    pw.WriteUInt32(1); //MUID
                    pw.WriteUInt32(itm.Item);
                    pw.WriteUInt32(tmp.DisplayID);
                    pw.WriteUInt32((itm.MaxCount <= 0 ? 0xFFFFFFFF : (uint)itm.CurCount));
                    pw.WriteUInt32(tmp.BuyPrice);
                    pw.WriteUInt32(0); //Durability
                    pw.WriteUInt32(0); //Stack Count

                    p.QueryItemCheck(tmp.Entry);
                }
            }

            return pw;
        }

        #endregion

        #region Trainer Functions
        public bool IsTrainerOfType(Player p, bool msg)
        {

            switch (Template.TrainerType)
            {
                case (uint)TrainerTypes.TRAINER_TYPE_GENERAL:
                    return p.Class == Template.TrainerClass;

                //Should this be checked - surely everyone can talk to any skill trainer?
                //case (uint)TRAINER_TYPE.TRAINER_TYPE_TRADESKILLS:
                //    return (Template.TrainerSpell >= 0 /* && !p.HasSpell(Template.TrainerSpell)*/);

                case (uint)TrainerTypes.TRAINER_TYPE_PET:
                    return p.Class == (byte)Classes.CLASS_HUNTER;
            }

            return true;
        }
        #endregion

        #region Update Functions
        public override void Update(long time)
        {
            base.Update(time);

            this.UpdateInventory(time);

            if (this.IsAttacking && !this.IsDead)
                AttackUpdate();

            if (this.IsDead)
                DeathUpdate();
        }

        public override PacketWriter QueryDetails()
        {
            return this.Template.QueryDetails();
        }

        private void AttackUpdate()
        {
            Unit closestTarget = null;
            if (this.IsDead)
                return;

            //Remove out of range attackers
            foreach (Unit victim in this.Attackers.Values.ToList())
                if (victim.Location.DistanceSqrd(this.Location) > Math.Pow(Globals.UPDATE_DISTANCE, 2) || victim.IsDead)
                    this.Attackers.TryRemove(victim.Guid, out Unit dump); //Out of range
                else if (closestTarget == null)
                    closestTarget = victim;
                else if (victim.Location.DistanceSqrd(this.Location) < closestTarget.Location.DistanceSqrd(this.Location))
                    closestTarget = victim;

            if (!this.Attackers.Any()) //No one left to kill
            {
                this.IsAttacking = false;
                this.InCombat = false;
                this.Health.SetAll(this.Health.Maximum);
                this.CombatTarget = 0;
                MoveTo(CombatStartLocation, true);
                return;
            }
            else
            {
                this.CombatTarget = closestTarget.Guid;

                if (Database.Players.ContainsKey(this.CombatTarget)) //Victim exists
                {
                    Player victim = Database.Players.TryGet(this.CombatTarget);

                    float distance = Location.Distance(victim.Location);
                    if (distance > this.Template.CombatReach && MoveLocation != victim.Location) //If not going to location already
                        MoveTo(victim.Location, true, distance); //Move to victim's location
                    else
                        this.UpdateMeleeAttackingState(); //Victim in range so attack

                    return;
                }
                else
                {
                    if (this.Attackers.Count > 0) //Add next attacker next iteration
                        return;
                    else //No one left to attack
                    {
                        this.IsAttacking = false;
                        this.InCombat = false;
                        this.CombatTarget = 0;
                        this.Health.SetAll(this.Health.Maximum);
                        MoveTo(CombatStartLocation, true);
                        return;
                    }
                }
            }
        }

        private void DeathUpdate()
        {
            if (CorpseRespawnTime > 0 && Globals.TimeTicks >= CorpseRespawnTime)
            {
                this.Respawn();
                CorpseRespawnTime = 0;
            }

            if (CorpseRemoveTime > 0 && Globals.TimeTicks >= CorpseRemoveTime)
            {
                GridManager.Instance.SendSurrounding(this.BuildDestroy(), this);
                CorpseRemoveTime = 0;
            }
        }
        #endregion
    }
}

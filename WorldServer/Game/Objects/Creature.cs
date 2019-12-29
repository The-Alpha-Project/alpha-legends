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
using Common.Database.DBC;

public enum CastFlags
{
    CF_INTERRUPT_PREVIOUS     = 0x01,                     //Interrupt any spell casting
    CF_TRIGGERED              = 0x02,                     //Triggered (this makes spell cost zero mana and have no cast time)
    CF_FORCE_CAST             = 0x04,                     //Forces cast even if creature is out of mana or out of range
    CF_MAIN_RANGED_SPELL      = 0x08,                     //To be used by ranged mobs only. Creature will not chase target until cast fails.
    CF_TARGET_UNREACHABLE     = 0x10,                     //Will only use the ability if creature cannot currently get to target
    CF_AURA_NOT_PRESENT       = 0x20,                     //Only casts the spell if the target does not have an aura from the spell
    CF_ONLY_IN_MELEE          = 0x40,                     //Only casts if the creature is in melee range of the target
    CF_NOT_IN_MELEE           = 0x80,                     //Only casts if the creature is not in melee range of the target
};

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

        public const long CREATURE_CASTING_DELAY = 1200;
        private long castingDelay;
        CreatureSpellsList spellsList;
        public bool combatMovementEnabled;
        public bool meleeAttackEnabled;
        public long lastUpdateTime;

        public float CombatReach;

        public Creature() { }

        public Creature(ref MySqlDataReader dr)
        {
            this.ObjectType |= ObjectTypes.TYPE_UNIT;
            this.Guid = Convert.ToUInt64(dr["spawn_id"]) | (ulong)HIGH_GUID.HIGHGUID_UNIT;
            this.Entry = Convert.ToUInt32(dr["spawn_entry"]); 
            this.Template = Database.CreatureTemplates.TryGet(this.Entry);
            this.Map = Convert.ToUInt32(dr["spawn_map"]);
            this.DisplayID = Template.ModelID.ToList().Shuffle().Find(num => num > 0 && num <= 4185);
            if (DisplayID == 0) // This way we identify creatures with out of bounds displayids easily
                this.DisplayID = 46;
            this.Location = new Vector(Convert.ToSingle(dr["spawn_positionX"]),
                                       Convert.ToSingle(dr["spawn_positionY"]),
                                       Convert.ToSingle(dr["spawn_positionZ"]));
            this.Orientation = Convert.ToSingle(dr["spawn_orientation"]);
            this.Health = new TStat { BaseAmount = Convert.ToUInt32(dr["spawn_curhealth"]) };
            this.Mana = new TStat { BaseAmount = Convert.ToUInt32(dr["spawn_curmana"]) };
            this.RespawnTime = Convert.ToInt32(dr["spawn_spawntime"]);
            this.RespawnDistance = Convert.ToInt32(dr["spawn_spawndist"]);
            this.castingDelay = 0;
            this.combatMovementEnabled = true;
            this.meleeAttackEnabled = true;

            this.OnDbLoad();
        }

        #region Database Functions
        public void OnDbLoad()
        {
            SetRespawn();
            this.Health.SetAll(this.Template.Health.Maximum);
            this.Mana.SetAll(this.Template.Mana.Maximum);
            this.BaseAttackTime = (uint)this.Template.AttackTime;
            this.DynamicFlags = this.Template.DynamicFlags;
            this.NPCFlags = (byte)this.Template.NPCFlags;
            this.Faction = this.Template.Faction;
            this.VendorLoot = this.Template.VendorItems;
            this.Level = this.Template.Level.GetRandom();
            CreatureSpellsListTemplate spellListTemplate = Database.GetSpellsListForCreatureId(this.Template.SpellListID);
            this.spellsList = spellListTemplate != null ? new CreatureSpellsList(spellListTemplate) : null;

            CreatureModelInfo cmi = Database.CreatureModelInfo.TryGet(DisplayID);
            if (cmi != null)
            {
                this.BoundingRadius = cmi.BoundingRadius;
                this.CombatReach = cmi.CombatReach;
            }

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
                new MySqlParameter("@spawn_displayid", DisplayID),
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
        public override PacketWriter QueryDetails()
        {
            PacketWriter pw = new PacketWriter(Opcodes.SMSG_CREATURE_QUERY_RESPONSE);
            pw.WriteUInt32(this.Entry);
            pw.WriteString(this.Template.Name);

            for (int i = 0; i < 3; i++)
                pw.WriteString(this.Template.Name); //Other names - never implemented

            pw.WriteString(this.Template.SubName);
            pw.WriteUInt32(this.Template.CreatureTypeFlags); //Creature Type i.e tameable
            pw.WriteUInt32(this.Template.CreatureType);
            pw.WriteUInt32(this.Template.Family);
            pw.WriteUInt32(this.Template.Rank);
            pw.WriteUInt32(0);
            pw.WriteUInt32(this.Template.PetSpellDataID);
            pw.WriteUInt32(this.DisplayID);
            pw.WriteUInt16(0); //??
            return pw;
        }

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
            uc.UpdateValue<int>(UnitFields.UNIT_FIELD_RESISTANCES, this.Template.Armor);
            uc.UpdateValue<int>(UnitFields.UNIT_FIELD_RESISTANCES, this.Template.Holy.BaseAmount, 1);
            uc.UpdateValue<int>(UnitFields.UNIT_FIELD_RESISTANCES, this.Template.Fire.BaseAmount, 2);
            uc.UpdateValue<int>(UnitFields.UNIT_FIELD_RESISTANCES, this.Template.Nature.BaseAmount, 3);
            uc.UpdateValue<int>(UnitFields.UNIT_FIELD_RESISTANCES, this.Template.Frost.BaseAmount, 4);
            uc.UpdateValue<int>(UnitFields.UNIT_FIELD_RESISTANCES, this.Template.Shadow.BaseAmount, 5);
            uc.UpdateValue<float>(UnitFields.UNIT_FIELD_BOUNDINGRADIUS, this.BoundingRadius);
            uc.UpdateValue<float>(UnitFields.UNIT_FIELD_COMBATREACH, this.CombatReach);
            uc.UpdateValue<uint>(UnitFields.UNIT_FIELD_DISPLAYID, this.DisplayID);
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
            Flag.SetFlag(ref DynamicFlags, (uint)UnitDynamicTypes.UNIT_DYNAMIC_DEAD);
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

                    Unit dump;
                    if (unit.Attackers.ContainsKey(this.Guid))
                        unit.Attackers.TryRemove(this.Guid, out dump);
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

        public void Reset()
        {
            this.IsAttacking = false;
            this.InCombat = false;
            this.Health.SetAll(this.Health.Maximum);
            this.CombatTarget = 0;
            Flag.RemoveFlag(ref UnitFlags, (uint)Common.Constants.UnitFlags.UNIT_FLAG_IN_COMBAT);
            GridManager.Instance.SendSurrounding(this.BuildUpdate(), this);
            MoveTo(CombatStartLocation, true);

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
            {
                DeathUpdate();
                return;
            }

            if (this.InCombat && spellsList != null)
                UpdateSpellsList(time);

            lastUpdateTime = time;
        }

        public void UpdateSpellsList(long time)
        {
            time = time - lastUpdateTime;
            time = time / TimeSpan.TicksPerMillisecond;
            if (castingDelay <= time)
            {
                long uiDesync = (time - castingDelay);
                DoSpellsListCasts(CREATURE_CASTING_DELAY + uiDesync);
                castingDelay = uiDesync < CREATURE_CASTING_DELAY ? CREATURE_CASTING_DELAY - uiDesync : 0;
            }
            else
                castingDelay -= time;
        }

        void DoSpellsListCasts(long diff)
        {
            bool bDontCast = false;
            foreach (CreatureSpellsEntry spell in spellsList.spells)
            {
                if (spell.cooldown <= diff)
                {
                    // Cooldown has expired.
                    spell.cooldown = 0;

                    // Prevent casting multiple spells in the same update. Only update timers.
                    if ((spell.data.castFlags & ((byte)CastFlags.CF_TRIGGERED | (byte)CastFlags.CF_INTERRUPT_PREVIOUS)) == 0)
                    {
                        if (bDontCast || IsNonMeleeSpellCasted())
                            continue;
                    }

                    if (!DBC.Spell.ContainsKey(spell.data.spellId))
                        continue;

                    Unit target = ScriptMgr.GetTargetByType(this, this, spell.data.castTarget, spell.data.targetParam1 != 0 ? spell.data.targetParam1 : (uint)DBC.Spell[spell.data.spellId].GetMaxCastRange(), spell.data.targetParam2)?.ToUnit();
                    SpellCheckCastResult result = ForceCastSpell(spell.data.spellId, target);
                    Console.WriteLine("[Creature " + this.Entry.ToString() + "][Spell " + spell.data.spellId.ToString() + "] Cast Result is " + result.ToString());

                    switch (result)
                    {
                        case SpellCheckCastResult.SPELL_CAST_OK:
                        {
                            bDontCast = (spell.data.castFlags & (byte)CastFlags.CF_TRIGGERED) == 0;
                            spell.cooldown = (long)(new Random().Next((int)spell.data.delayRepeatMin, (int)spell.data.delayRepeatMax));

                            if ((spell.data.castFlags & (byte)CastFlags.CF_MAIN_RANGED_SPELL) != 0)
                            {
                                //if (IsMoving())
                                //    StopMoving();

                                SetCombatMovement(false);
                                SetMeleeAttack(false);
                            }

                            // If there is a script for this spell, run it.
                            // if (spell.data.scriptId != 0)
                            //     m_creature->GetMap()->ScriptsStart(sCreatureSpellScripts, spell.data.scriptId, this, pTarget);
                            break;
                        }
                        //case SpellCheckCastResult.SPELL_FAILED_FLEEING:
                        case SpellCheckCastResult.SPELL_FAILED_SPELL_IN_PROGRESS:
                        {
                            // Do nothing so it will try again on next update.
                            break;
                        }
                        case SpellCheckCastResult.SPELL_FAILED_TRY_AGAIN:
                        {
                            // Chance roll failed, so we reset cooldown.
                            spell.cooldown = (long)(new Random().Next((int)spell.data.delayRepeatMin, (int)spell.data.delayRepeatMax));
                            if ((spell.data.castFlags & (byte)CastFlags.CF_MAIN_RANGED_SPELL) != 0)
                            {
                                SetCombatMovement(true);
                                SetMeleeAttack(true);
                            }
                            break;
                        }
                        default:
                        {
                            // other error
                            if ((spell.data.castFlags & (byte)CastFlags.CF_MAIN_RANGED_SPELL) != 0)
                            {
                                SetCombatMovement(true);
                                SetMeleeAttack(true);
                            }
                            break;
                        }
                    }
                }
                else
                    spell.cooldown -= diff;
            }
        }

        public void SetCombatMovement(bool state)
        {
            combatMovementEnabled = state;
        }

        public void SetMeleeAttack(bool state)
        {
            meleeAttackEnabled = state;
        }
        
        private void AttackUpdate()
        {
            Unit closestTarget = null;
            Unit dump;
            if (this.IsDead)
                return;

            //Remove out of range attackers
            foreach (Unit victim in this.Attackers.Values.ToList())
                if (victim.Location.DistanceSqrd(this.Location) > Math.Pow(Globals.UPDATE_DISTANCE, 2) || victim.IsDead)
                    this.Attackers.TryRemove(victim.Guid, out dump); //Out of range
                else if (closestTarget == null)
                    closestTarget = victim;
                else if (victim.Location.DistanceSqrd(this.Location) < closestTarget.Location.DistanceSqrd(this.Location))
                    closestTarget = victim;

            if (!this.Attackers.Any()) //No one left to kill
            {
                Reset();

                return;
            }
            else
            {
                this.CombatTarget = closestTarget.Guid;

                if (Database.Players.ContainsKey(this.CombatTarget)) //Victim exists
                {
                    Player victim = Database.Players.TryGet(this.CombatTarget);

                    float distance = Location.Distance(victim.Location);
                    if (distance > this.CombatReach && MoveLocation != victim.Location) //If not going to location already
                    {
                        if (combatMovementEnabled)
                            MoveTo(victim.Location, true, distance); // Move to victim's location
                    }
                    else
                    {
                        if (meleeAttackEnabled)
                            UpdateMeleeAttackingState(); // Victim in range so attack
                    }

                    return;
                }
                else
                {
                    if (this.Attackers.Count > 0) //Add next attacker next iteration
                        return;
                    else //No one left to attack
                    {
                        Reset();
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

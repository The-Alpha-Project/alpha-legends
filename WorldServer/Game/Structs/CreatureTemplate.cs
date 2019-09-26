using Common.Database;
using Common.Helpers.Extensions;
using Common.Network.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorldServer.Storage;
using static WorldServer.Game.Objects.UnitExtensions.TalentExtension;

namespace WorldServer.Game.Structs
{
    [Table("creatures")]
    public class CreatureTemplate
    {
        [Key]
        [Column("entry")]
        public uint Entry { get; set; }
        [Column("display_id1")]
        public uint ModelID1 { get; set; }
        [Column("display_id2")]
        public uint ModelID2 { get; set; }
        [Column("display_id3")]
        public uint ModelID3 { get; set; }
        [Column("display_id4")]
        public uint ModelID4 { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("subname")]
        public string SubName { get; set; }
        [Column("npc_flags")]
        public uint NPCFlags { get; set; }
        [Column("speed_run")]
        public float Speed { get; set; }
        [Column("scale")]
        public float Scale { get; set; }
        [Column("base_attack_time")]
        public int AttackTime { get; set; }
        [Column("unit_flags")]
        public uint UnitFlags { get; set; }
        [Column("dynamic_flags")]
        public uint DynamicFlags { get; set; }
        [Column("type")]
        public uint CreatureType { get; set; }
        [Column("type_flags")]
        public uint CreatureTypeFlags { get; set; }
        [ColumnList("spell", 4)]
        public ulong[] Spells = new ulong[4];
        [Column("movement_type")]
        public bool MovementType { get; set; }
        [Column("equipment_id")]
        public int EquipmentID { get; set; }
        [Column("pet_spell_list_id")]
        public uint PetSpellDataID { get; set; }
        [Column("rank")]
        public uint Rank { get; set; }
        [Column("beast_family")]
        public uint Family { get; set; }
        [Column("faction")]
        public uint Faction { get; set; }
        [ColumnList(new[] { "health_min", "health_max" })]
        public TStat Health { get; set; }
        [ColumnList(new[] { "level_min", "level_max" })]
        public TRandom Level { get; set; }
        [ColumnList(new[] { "gold_min", "gold_max" })]
        public TRandom Gold { get; set; }
        [ColumnList(new[] { "mana_min", "mana_max" })]
        public TRandom Mana { get; set; }
        [ColumnList(new[] { "dmg_min", "dmg_max" })]
        public TStat Damage { get; set; }

        [ColumnList(new[] { "armor", "arcane_res" })] // TODO: There's no arcane res in 0.5.3
        public TResistance Armor { get; set; }
        [Column("holy_res")]
        public TResistance Holy { get; set; }
        [Column("fire_res")]
        public TResistance Fire { get; set; }
        [Column("nature_res")]
        public TResistance Nature { get; set; }
        [Column("frost_res")]
        public TResistance Frost { get; set; }
        [Column("shadow_res")]
        public TResistance Shadow { get; set; }        

        //Internal use only
        [Column("trainer_type")]
        public uint TrainerType { get; set; }
        [Column("trainer_spell")]
        public uint TrainerSpell { get; set; }
        [Column("trainer_class")]
        public uint TrainerClass { get; set; }
        [Column("trainer_race")]
        public uint TrainerRace { get; set; }

        public float BoundingRadius;
        public float CombatReach;
        public List<VendorItem> VendorItems;
        public Dictionary<uint, VendorSpell> VendorSpells;

        public PacketWriter QueryDetails()
        {
            PacketWriter pw = new PacketWriter(Opcodes.SMSG_CREATURE_QUERY_RESPONSE);
            pw.WriteUInt32(this.Entry);
            pw.WriteString(this.Name);

            for (int i = 0; i < 3; i++)
                pw.WriteString(this.Name); //Other names - never implemented

            pw.WriteString(this.SubName);
            pw.WriteUInt32(this.CreatureTypeFlags); //Creature Type i.e tameable
            pw.WriteUInt32(this.CreatureType);
            pw.WriteUInt32(this.Family);
            pw.WriteUInt32(this.Rank);
            pw.WriteUInt32(0);
            pw.WriteUInt32(this.PetSpellDataID);
            pw.WriteUInt32(this.ModelID1);
            pw.WriteUInt16(0); //??
            return pw;
        }

        public void OnDbLoad()
        {
            CreatureModelInfo cmi = Database.CreatureModelInfo.TryGet(this.ModelID1);
            if (cmi != null)
            {
                this.BoundingRadius = cmi.BoundingRadius;
                this.CombatReach = cmi.CombatReach;
            }
            
            this.VendorItems = Database.VendorItems.TryGet(this.Entry)?.ToList(); //Shallow copy
            this.VendorSpells = Database.VendorSpells.TryGet(this.Entry)?.ToDictionary(x => x.SpellId, y => y);
        }
    }
}

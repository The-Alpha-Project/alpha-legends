using Common.Database;
using Common.Helpers.Extensions;
using Common.Network.Packets;

namespace WorldServer.Game.Structs
{
    [Table("item_template")]
    public class ItemTemplate
    {
        [Key]
        [Column("entry")]
        public uint Entry { get; set; }
        [Column("class")]
        public uint Type { get; set; }
        [Column("subclass")]
        public uint Subtype { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("display_id")]
        public uint DisplayID { get; set; }
        [Column("quality")]
        public uint OverallQuality { get; set; }
        [Column("flags")]
        public uint Flags { get; set; }
        [Column("buy_price")]
        public uint BuyPrice { get; set; }// Player buys
        [Column("sell_price")]
        public uint SellPrice { get; set; }// Player sells
        [Column("inventory_type")]
        public uint InvType { get; set; }
        [Column("allowable_class")]
        public int AllowableClass { get; set; }
        [Column("allowable_race")]
        public int AllowableRace { get; set; }
        [Column("item_level")]
        public uint Level { get; set; }
        [Column("required_level")]
        public uint LevelReq { get; set; }
        [Column("required_skill")]
        public uint SkillReq { get; set; }
        [Column("required_skill_rank")]
        public uint SkillRankReq { get; set; }
        [Column("max_count")]
        public uint MaxCount { get; set; }
        [Column("stackable")]
        public uint MaxStackCount { get; set; }
        [Column("container_slots")]
        public uint ContainerSlots { get; set; }
        [Column("armor")]
        public int ResistPhysical { get; set; } //Resitances are ints as some items can actually remove stats - Corrupted Ashbringer
        [Column("holy_res")]
        public int ResistHoly { get; set; }
        [Column("fire_res")]
        public int ResistFire { get; set; }
        [Column("nature_res")]
        public int ResistNature { get; set; }
        [Column("frost_res")]
        public int ResistFrost { get; set; }
        [Column("shadow_res")]
        public int ResistShadow { get; set; }
        [Column("delay")]
        public uint WeaponSpeed { get; set; }
        [Column("ammo_type")]
        public uint AmmoType { get; set; }
        [Column("bonding")]
        public uint Bonding { get; set; }
        [Column("description")]
        public string Description { get; set; }
        [Column("page_text")]
        public uint PageText { get; set; }
        [Column("page_language")]
        public uint PageLanguage { get; set; }
        [Column("page_material")]
        public uint PageMaterial { get; set; }
        [Column("start_quest")]
        public uint StartQuest { get; set; }
        [Column("lock_id")]
        public uint Lock { get; set; }
        [Column("material")]
        public int Material { get; set; }
        [Column("sheath")]
        public uint SheatheType { get; set; }
        [ColumnList("spellcharges_", 5)]
        public int[] SpellCharges { get; set; }

        [ColumnList("", 5)]
        public SpellStat[] SpellStats { get; set; }
        [ColumnList("", 10)]
        public ItemAttribute[] Attributes { get; set; }
        [ColumnList("", 5)]
        public DamageStat[] DamageStats { get; set; }

        public PacketWriter QueryDetails()
        {
            PacketWriter pw = new PacketWriter(Opcodes.SMSG_ITEM_QUERY_SINGLE_RESPONSE);
            pw.WriteUInt32(this.Entry);
            pw.WriteUInt32(this.Type); //Class
            pw.WriteUInt32(this.Subtype);
            pw.WriteString(this.Name);
            pw.WriteString(string.Empty);
            pw.WriteString(string.Empty);
            pw.WriteString(string.Empty);
            pw.WriteUInt32(this.DisplayID);
            pw.WriteUInt32(this.OverallQuality);
            pw.WriteUInt32(this.Flags);
            pw.WriteUInt32(this.BuyPrice);
            pw.WriteUInt32(this.SellPrice);
            pw.WriteUInt32(this.InvType);
            pw.WriteInt32(this.AllowableClass);
            pw.WriteInt32(this.AllowableRace);
            pw.WriteUInt32(this.Level); //ItemLevel
            pw.WriteUInt32(this.LevelReq);
            pw.WriteUInt32(this.SkillReq);
            pw.WriteUInt32(this.SkillRankReq);
            pw.WriteUInt32(this.MaxCount);
            pw.WriteUInt32(this.MaxStackCount);
            pw.WriteUInt32(this.ContainerSlots);

            foreach (ItemAttribute ia in this.Attributes)
            {
                pw.WriteUInt32(ia.ID);
                pw.WriteInt32(ia.Value);
            }

            foreach (DamageStat ds in this.DamageStats)
            {
                pw.WriteInt32(ds.Min);
                pw.WriteInt32(ds.Max);
                pw.WriteInt32(ds.Type);
            }

            pw.WriteInt32(this.ResistPhysical);
            pw.WriteInt32(this.ResistHoly);
            pw.WriteInt32(this.ResistFire);
            pw.WriteInt32(this.ResistNature);
            pw.WriteInt32(this.ResistFrost);
            pw.WriteInt32(this.ResistShadow);

            pw.WriteUInt32(this.WeaponSpeed);
            pw.WriteUInt32(this.AmmoType);
            pw.WriteUInt32(0); //Durability - not implemented

            foreach (SpellStat ss in this.SpellStats)
            {
                pw.WriteInt32(ss.ID);
                pw.WriteInt32(ss.Trigger);
                pw.WriteInt32(ss.Charges);
                pw.WriteInt32(ss.Cooldown);
                pw.WriteInt32(ss.Category);
                pw.WriteInt32(ss.CategoryCoolDown);
            }

            pw.WriteUInt32(this.Bonding);
            pw.WriteString(this.Description + ""); //Description needs to be something
            pw.WriteUInt32(this.PageText);
            pw.WriteUInt32(this.PageLanguage);
            pw.WriteUInt32(this.PageMaterial);
            pw.WriteUInt32(this.StartQuest);
            pw.WriteUInt32(this.Lock);
            pw.WriteInt32(this.Material);
            pw.WriteUInt32(this.SheatheType);
            return pw;
        }
    }
}

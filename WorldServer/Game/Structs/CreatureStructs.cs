using Common.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldServer.Game.Structs
{
    [Table("creature_model_info")]
    public class CreatureModelInfo
    {
        [Key]
        [Column("modelid")]
        public uint ModelId { get; set; }
        [Column("bounding_radius")]
        public float BoundingRadius { get; set; }
        [Column("combat_reach")]
        public float CombatReach { get; set; }
        [Column("gender")]
        public byte Gender { get; set; }
    }

    public class CreatureQuest
    {
        [Key]
        [Column("entry")]
        public uint CreatureEntry { get; set; }
        [Column("quest")]
        public uint QuestEntry { get; set; }
    }

    public class CreatureSpellsEntryTemplate
    {
        public UInt16 spellId;
        public byte probability;
        public byte castTarget;
        public UInt32 targetParam1;
        public UInt32 targetParam2;
        public byte castFlags;
        public UInt32 delayInitialMin;
        public UInt32 delayInitialMax;
        public UInt32 delayRepeatMin;
        public UInt32 delayRepeatMax;
        public UInt32 scriptId;
        public CreatureSpellsEntryTemplate(UInt16 Id, byte Probability, byte CastTarget, UInt32 TargetParam1, UInt32 TargetParam2, byte CastFlags, UInt32 InitialMin, UInt32 InitialMax, UInt32 RepeatMin, UInt32 RepeatMax, UInt32 ScriptId)
        {
            spellId = Id;
            probability = Probability;
            castTarget = CastTarget;
            targetParam1 = TargetParam1;
            targetParam2 = TargetParam2;
            castFlags = CastFlags;
            delayInitialMin = InitialMin;
            delayInitialMax = InitialMax;
            delayRepeatMin = RepeatMin;
            delayRepeatMax = RepeatMax;
            scriptId = ScriptId;
        }
    };

    public class CreatureSpellsEntry
    {
        public CreatureSpellsEntryTemplate data;
        public long cooldown;
        public CreatureSpellsEntry(CreatureSpellsEntryTemplate template)
        {
            cooldown = 0;
            data = template;
        }
    }

    public class CreatureSpellsListTemplate
    {
        public UInt32 entry;
        public List<CreatureSpellsEntryTemplate> spells;
        public CreatureSpellsListTemplate()
        {
            entry = 0;
            spells = new List<CreatureSpellsEntryTemplate>();
        }
    }

    public class CreatureSpellsList
    {
        public UInt32 entry;
        public List<CreatureSpellsEntry> spells;
        public CreatureSpellsList(CreatureSpellsListTemplate template)
        {
            entry = template.entry;
            spells = new List<CreatureSpellsEntry>();
            foreach (CreatureSpellsEntryTemplate spell in template.spells)
            {
                spells.Add(new CreatureSpellsEntry(spell));
            }
        }
    }
}

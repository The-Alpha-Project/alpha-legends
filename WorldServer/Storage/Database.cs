using Common.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorldServer.Game;
using WorldServer.Game.Objects;
using WorldServer.Game.Structs;

namespace WorldServer.Storage
{
    public static class Database
    {
        public static DbSet<uint, AreaTrigger> AreaTriggers;
        public static DbSet<uint, ClassLevelStat> ClassLevelStats;
        public static DbSet<uint, KnownSpells> KnownSpells;
        public static DbSet<uint, KnownTalents> KnownTalents;
        public static DbSet<uint, CreateActionButton> CreateActionButtons;
        public static DbSet<uint, CreatePlayerInfo> CreatePlayerInfo;
        public static DbSet<uint, CreateSkillInfo> CreateSkillInfo;
        public static DbSet<uint, CreateSpellInfo> CreateSpellInfo;
        public static DbSet<uint, LevelStatsInfo> LevelStatsInfo;
        public static GroupedDbSet<uint, List<LootItem>> PickPocketLoot;
        public static GroupedDbSet<uint, List<LootItem>> SkinningLoot;
        public static GroupedDbSet<uint, List<LootItem>> CreatureLoot;
        public static GroupedDbSet<uint, List<LootItem>> GameObjectLoot;
        public static DbSet<uint, QuestTemplate> QuestTemplates;
        public static DbSet<uint, CreatureModelInfo> CreatureModelInfo;
        public static GroupedDbSet<uint, List<CreatureQuest>> CreatureQuests;
        public static GroupedDbSet<uint, List<CreatureQuest>> CreatureInvolvedQuests;
        public static DbSet<uint, GameObjectTemplate> GameObjectTemplates;
        public static GroupedDbSet<uint, List<VendorItem>> VendorItems;
        public static GroupedDbSet<uint, List<VendorSpell>> VendorSpells;
        public static DbSet<uint, CreatureTemplate> CreatureTemplates;
        public static List<CreatureSpellsListTemplate> CreatureSpells;
        public static DbSet<uint, ItemTemplate> ItemTemplates;
        public static GroupedDbSet<ulong, List<SocialList>> SocialList;
        public static DbSet<uint, Worldports> Worldports;
        public static DbSet<uint, Ticket> Tickets;

        public static DbSet<uint, Account> Accounts;
        public static DbSet<ulong, Creature> Creatures;
        public static DbSet<ulong, GameObject> GameObjects;
        public static DbSet<ulong, Item> Items;
        public static DbSet<ulong, Player> Players;
        
        private const string QueryDefault = "SELECT * FROM ";
        private const string QueryLootTemplate = "SELECT entry, item, CASE WHEN chanceorquestchance < 0 then 1 else 0 end as questitem," +
                                                 "ABS(chanceorquestchance) AS chance, mincountorref, maxcount, groupid FROM ";

        public static void Initialize()
        {
            AreaTriggers = new DbSet<uint, AreaTrigger>();
            ClassLevelStats = new DbSet<uint, ClassLevelStat>();
            KnownSpells = new DbSet<uint, KnownSpells>();
            KnownTalents = new DbSet<uint, KnownTalents>();
            CreateActionButtons = new DbSet<uint, CreateActionButton>();
            CreatePlayerInfo = new DbSet<uint, CreatePlayerInfo>();
            CreateSkillInfo = new DbSet<uint, CreateSkillInfo>();
            CreateSpellInfo = new DbSet<uint, CreateSpellInfo>();
            LevelStatsInfo = new DbSet<uint, LevelStatsInfo>();
            PickPocketLoot = new GroupedDbSet<uint, List<LootItem>>(QueryLootTemplate + "pickpocketing_loot_template", "PickPocket Loot");
            SkinningLoot = new GroupedDbSet<uint, List<LootItem>>(QueryLootTemplate + "skinning_loot_template", "Skinning Loot");
            CreatureLoot = new GroupedDbSet<uint, List<LootItem>>(QueryLootTemplate + "creature_loot_template", "Creature Loot");
            GameObjectLoot = new GroupedDbSet<uint, List<LootItem>>(QueryLootTemplate + "gameobject_loot_template", "GameObject Loot");
            QuestTemplates = new DbSet<uint, QuestTemplate>();
            CreatureModelInfo = new DbSet<uint, CreatureModelInfo>();
            CreatureQuests = new GroupedDbSet<uint, List<CreatureQuest>>(QueryDefault + "creature_questrelation", "Creature Quest Relations");
            CreatureInvolvedQuests = new GroupedDbSet<uint, List<CreatureQuest>>(QueryDefault + "creature_involvedrelation", "Creature Involved Relations");
            GameObjectTemplates = new DbSet<uint, GameObjectTemplate>();
            VendorItems = new GroupedDbSet<uint, List<VendorItem>>(QueryDefault + "npc_vendor", "Vendor Items");
            VendorSpells = new GroupedDbSet<uint, List<VendorSpell>>(QueryDefault + "npc_trainer", "Vendor Spells");
            LoadCreatureSpellLists();
            CreatureTemplates = new DbSet<uint, CreatureTemplate>();
            ItemTemplates = new DbSet<uint, ItemTemplate>();
            Worldports = new DbSet<uint, Worldports>();
            Tickets = new DbSet<uint, Ticket>(false, true);

            Accounts = new DbSet<uint, Account>(false, true);
            Creatures = new DbSet<ulong, Creature>(true);
            GameObjects = new DbSet<ulong, GameObject>(true);
            Items = new DbSet<ulong, Item>(true, true);
            Players = new DbSet<ulong, Player>(true, true);
            SocialList = new GroupedDbSet<ulong, List<SocialList>>(QueryDefault + "character_social", "Social Lists", true);
        }

        public static void LoadCreatureSpellLists()
        {
            Log.Message(LogType.NORMAL, "Loading creature spell lists ...");
            //                            0        1            2                3               4                 5                 6              7                    8                    9                   10                  11
            string queryString = "SELECT `entry`, `spellId_1`, `probability_1`, `castTarget_1`, `targetParam1_1`, `targetParam2_1`, `castFlags_1`, `delayInitialMin_1`, `delayInitialMax_1`, `delayRepeatMin_1`, `delayRepeatMax_1`, `scriptId_1`, " +
            //                                     12           13               14              15                16                17             18                   19                   20                  21                  22
                                                 "`spellId_2`, `probability_2`, `castTarget_2`, `targetParam1_2`, `targetParam2_2`, `castFlags_2`, `delayInitialMin_2`, `delayInitialMax_2`, `delayRepeatMin_2`, `delayRepeatMax_2`, `scriptId_2`, " +
            //                                     23           24               25              26                27                28             29                   30                   31                  32                  33
                                                 "`spellId_3`, `probability_3`, `castTarget_3`, `targetParam1_3`, `targetParam2_3`, `castFlags_3`, `delayInitialMin_3`, `delayInitialMax_3`, `delayRepeatMin_3`, `delayRepeatMax_3`, `scriptId_3`, " +
            //                                     34           35               36              37                38                39             40                   41                   42                  43                  44
                                                 "`spellId_4`, `probability_4`, `castTarget_4`, `targetParam1_4`, `targetParam2_4`, `castFlags_4`, `delayInitialMin_4`, `delayInitialMax_4`, `delayRepeatMin_4`, `delayRepeatMax_4`, `scriptId_4`, " +
            //                                     45           46               47              48                49                50             51                   52                   53                  54                  55
                                                 "`spellId_5`, `probability_5`, `castTarget_5`, `targetParam1_5`, `targetParam2_5`, `castFlags_5`, `delayInitialMin_5`, `delayInitialMax_5`, `delayRepeatMin_5`, `delayRepeatMax_5`, `scriptId_5`, " +
            //                                     56           57               58              59                60                61             62                   63                   64                  65                  66
                                                 "`spellId_6`, `probability_6`, `castTarget_6`, `targetParam1_6`, `targetParam2_6`, `castFlags_6`, `delayInitialMin_6`, `delayInitialMax_6`, `delayRepeatMin_6`, `delayRepeatMax_6`, `scriptId_6`, " +
            //                                     67           68               69              70                71                72             73                   74                   75                  76                  77
                                                 "`spellId_7`, `probability_7`, `castTarget_7`, `targetParam1_7`, `targetParam2_7`, `castFlags_7`, `delayInitialMin_7`, `delayInitialMax_7`, `delayRepeatMin_7`, `delayRepeatMax_7`, `scriptId_7`, " +
            //                                     78           79               80              81                82                83             84                   85                   86                  87                  88
                                                 "`spellId_8`, `probability_8`, `castTarget_8`, `targetParam1_8`, `targetParam2_8`, `castFlags_8`, `delayInitialMin_8`, `delayInitialMax_8`, `delayRepeatMin_8`, `delayRepeatMax_8`, `scriptId_8` FROM `creature_spells`";
            string connectionString = Globals.CONNECTION_STRING;
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand command = new MySqlCommand(queryString, connection);
            command.Connection.Open();
            MySqlDataReader reader = command.ExecuteReader();

            // Number of spells in one template
            const byte CREATURE_SPELLS_MAX_SPELLS = 8;
            // Columns in the db for each spell
            const byte CREATURE_SPELLS_MAX_COLUMNS = 11;
            CreatureSpells = new List<CreatureSpellsListTemplate>();
            while (reader.Read())
            {
                CreatureSpellsListTemplate spellsList = new CreatureSpellsListTemplate();

                UInt32 entry = (UInt32)reader.GetInt32(0);
                spellsList.entry = entry;
                for (byte i = 0; i < CREATURE_SPELLS_MAX_SPELLS; i++)
                {
                    UInt16 spellId = (UInt16)reader.GetInt16(1 + i * CREATURE_SPELLS_MAX_COLUMNS);
                    if (spellId != 0)
                    {
                        byte probability = (byte)reader.GetByte(2 + i * CREATURE_SPELLS_MAX_COLUMNS);
                        byte castTarget = (byte)reader.GetByte(3 + i * CREATURE_SPELLS_MAX_COLUMNS);
                        UInt32 targetParam1 = (UInt32)reader.GetInt32(4 + i * CREATURE_SPELLS_MAX_COLUMNS);
                        UInt32 targetParam2 = (UInt32)reader.GetInt32(5 + i * CREATURE_SPELLS_MAX_COLUMNS);
                        byte castFlags = (byte)reader.GetByte(6 + i * CREATURE_SPELLS_MAX_COLUMNS);
                        // in the database we store timers as seconds
                        // based on screenshot of blizzard creature spells editor
                        UInt32 delayInitialMin = (UInt32)reader.GetInt32(7 + i * CREATURE_SPELLS_MAX_COLUMNS) * 1000;
                        UInt32 delayInitialMax = (UInt32)reader.GetInt32(8 + i * CREATURE_SPELLS_MAX_COLUMNS) * 1000;
                        UInt32 delayRepeatMin = (UInt32)reader.GetInt32(9 + i * CREATURE_SPELLS_MAX_COLUMNS) * 1000;
                        UInt32 delayRepeatMax = (UInt32)reader.GetInt32(10 + i * CREATURE_SPELLS_MAX_COLUMNS) * 1000;
                        UInt32 scriptId = (UInt32)reader.GetInt32(11 + i * CREATURE_SPELLS_MAX_COLUMNS);
                        spellsList.spells.Add(new CreatureSpellsEntryTemplate(spellId, probability, castTarget, targetParam1, targetParam2, castFlags, delayInitialMin, delayInitialMax, delayRepeatMin, delayRepeatMax, scriptId));
                    }
                }
                CreatureSpells.Add(spellsList);
            }
            reader.Close();
            connection.Close();
            Log.Message(LogType.NORMAL, "Loaded " + CreatureSpells.Count.ToString() + " creature spell lists.");
        }

        public static CreatureSpellsListTemplate GetSpellsListForCreatureId(uint listId)
        {
            foreach (CreatureSpellsListTemplate list in CreatureSpells)
            {
                if (list.entry == listId)
                    return list;
            }
            return null;
        }

        public static void SaveChanges()
        {
            Accounts.UpdateChanges();
            Items.UpdateChanges();
            Players.UpdateChanges();
        }
    }
}

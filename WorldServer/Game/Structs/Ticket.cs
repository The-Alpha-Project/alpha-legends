using System.Collections.Generic;
using Common.Database;
using MySql.Data.MySqlClient;

namespace WorldServer.Game.Structs
{
    [Table("tickets")]
    public class Ticket
    {
        [Key]
        [Column("id")]
        public uint Id { get; set; }

        [Column("is_bug")]
        public bool IsBug { get; set; }

        [Column("account_name")]
        public string AccountName { get; set; }

        [Column("account_id")]
        public uint AccountId { get; set; }

        [Column("character_name")]
        public string CharacterName { get; set; }

        [Column("submit_time")]
        public System.DateTime SubmitTime { get; set; }

        [Column("text_body")]
        public string TextBody { get; set; }

        public void Save()
        {
            List<string> columns = new List<string>() {
                "is_bug", "account_name", "account_id", "character_name", "text_body"
            };

            List<MySqlParameter> parameters = new List<MySqlParameter>()
            {
                new MySqlParameter("@is_bug", this.IsBug),
                new MySqlParameter("@account_name", this.AccountName),
                new MySqlParameter("@account_id", this.AccountId),
                new MySqlParameter("@character_name", this.CharacterName),
                new MySqlParameter("@text_body", this.TextBody)
            };

            BaseContext.SaveEntity("tickets", columns, parameters, Globals.CONNECTION_STRING);
        }
    }
}

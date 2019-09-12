using Common.Database;

namespace WorldServer.Game.Structs
{
    [Table("worldports")]
    public class Worldports
    {
        [Key]
        [Column("entry")]
        public uint Entry { get; set; }

        [Column("x")]
        public float X { get; set; }

        [Column("y")]
        public float Y { get; set; }

        [Column("z")]
        public float Z { get; set; }

        [Column("map")]
        public uint Map { get; set; }

        [Column("name")]
        public string Name { get; set; }
    }
}

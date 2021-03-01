using Jypeli;

namespace JTD
{
    /// <summary>
    /// Keeps track of a bunch of things (eventually)
    /// </summary>
    public struct GameManager
    {
        public static IntMeter Money { get; set; }
        public static IntMeter KillCount { get; set; }
        public static int Level { get; set; }
        public static int EnemiesAlive { get; set; }
        public static int CannonSelected { get; set; }
    }
}
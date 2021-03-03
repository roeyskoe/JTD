using Jypeli;

namespace JTD
{
    /// <summary>
    /// Keeps track of a bunch of things (eventually)
    /// </summary>
    public static class GameManager
    {
        public static IntMeter Money { get; set; }
        public static IntMeter KillCount { get; set; }
        public static int Level { get; set; }
        public static int EnemiesAlive { get; set; }
        public static string CannonSelected { get; set; }
        public static Images Images { get; set; }

        /// <summary>
        /// Add object to game
        /// </summary>
        /// <param name="obj"></param>
        public static void Add(IGameObject obj)
        {
            JTD.Instance.Add(obj);
        }

        /// <summary>
        /// Add collision handler for specific object colliding with a specifig tagged object
        /// </summary>
        /// <param name="obj">Who is colliding</param>
        /// <param name="tag">With what</param>
        /// <param name="handler">Handler</param>
        /// <typeparam name="T1">Colliders type</typeparam>
        /// <typeparam name="T2">Targets type</typeparam>
        public static void AddCollisionHandler<T1, T2>(T1 obj, object tag, CollisionHandler<T1, T2> handler)
            where T1: PhysicsObject where T2 : PhysicsObject
        {
            JTD.Instance.AddCollisionHandler(obj, tag, handler);
        }
    }
}
using Jypeli;

namespace JTD
{
    /// <summary>
    /// Extends collisionhandler possibilities
    /// </summary>
    public interface IPhysicsObjectExtension
    {
        public delegate void CollisionHandler<in T>(T otherObject) where T : PhysicsObject;
    }

    /// <summary>
    /// Extends PhysicsObject to have a possibility for collision handler with only
    ///  a collider -parameter.
    /// </summary>
    public static class PhysicsObjectExtension
    {
        public static void AddCollisionHandler<T1, T2>(this T1 who, object tag, IPhysicsObjectExtension.CollisionHandler<T2> handler)
        where T1 : PhysicsObject
        where T2 : PhysicsObject
        {
            void TargetHandler(PhysicsObject collider, PhysicsObject collidee)
            {
                handler((T2)collidee);
            }

            GameManager.AddCollisionHandler<T1, T2>(who, tag, TargetHandler);
        }
    }
}
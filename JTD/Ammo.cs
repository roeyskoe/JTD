using System;
using Jypeli;

namespace JTD
{
    public class Ammo : PhysicsObject
    {
        /// <summary>
        /// How much damage this does when hit
        /// </summary>
        public int Damage { get;}

        public Ammo(int damage, Color color) : base(5, 5, Shape.Circle)
        {
            Damage = damage;
            base.Color = color;
            
            LifetimeLeft = TimeSpan.FromSeconds(2);
            Tag = "Ammo";
        }
    }
}
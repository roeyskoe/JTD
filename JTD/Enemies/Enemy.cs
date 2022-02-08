using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Widgets;

namespace JTD
{
    /// <summary>
    /// Enemy which tries to get to the protectable target
    /// </summary>
    public class Enemy : PhysicsObject
    {
        public IntMeter Health { get; private set; }

        public int Value { get; set; }

        public Enemy(EnemyTemplate enemyTemplate, int level, SortedList<char, Vector> route) : base(15, 15)
        {
            int health = enemyTemplate.BaseHealth * level;
            Health = new IntMeter(health, 0, health);
            Health.LowerLimit += Destroy;

            Image = GameManager.Images[enemyTemplate.Image];
            Value = enemyTemplate.Value;
            Tag = "Enemy";
            CanRotate = false;
            IgnoresCollisionResponse = true;

            PathFollowerBrain brain = new PathFollowerBrain(new List<Vector>(route.Values));
            brain.Speed = enemyTemplate.Speed;
            Brain = brain;

            ProgressBar HealthBar = new ProgressBar(Width, 3, Health);
            HealthBar.BarColor = Color.DarkGreen;
            HealthBar.Color = Color.BloodRed;
            HealthBar.Bottom = Bottom - 5;

            Add(HealthBar);

            AddCollisionHandler<Ammo>("Ammo", AmmoHit);
            AddCollisionHandler<Target>("Target", delegate
            {
                Explosion explosion = new Explosion(50);
                explosion.Position = Position;
                //Add(explosion); // TODO: Sound is way too loud :(
                GameManager.Target.Health.Value -= 100;
                Destroy();
            });
        }

        private void AmmoHit(Ammo ammo)
        {
            ammo.Destroy();
            Health.Value -= ammo.Damage;
        }

        private void AddCollisionHandler<T>(object tag, IPhysicsObjectExtension.CollisionHandler<T> handler)
        where T : PhysicsObject
        {
            PhysicsObjectExtension.AddCollisionHandler(this, tag, handler);
        }
    }
}

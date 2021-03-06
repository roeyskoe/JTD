using System;
using Jypeli;

namespace JTD
{
    /// <summary>
    /// Cannon
    /// </summary>
    public class Cannon : GameObject
    {
        public int Price { get; set; }
        public int Damage { get; set; }
        public double Speed { get; set; }
        public int Level { get; set; }
        public Color AmmoColor { get; set; }

        public Timer ShootTimer { get; set; }
        public Timer TurnTimer { get; set; }

        public Cannon(int price, int damage, double speed, Image image)
            : base(35, 35)
        {
            Price = price;
            Damage = damage;
            Speed = speed;
            Image = image;
            Tag = "Cannon";
            Level = 0;
        }

        public Cannon(CannonTemplate cannonTemplate) : base(35,35)
        {
            Price = cannonTemplate.Price;
            Damage = cannonTemplate.Damage;
            Speed = cannonTemplate.Interval;
            Image = GameManager.Images[cannonTemplate.Image];
            Tag = "Cannon";

            AmmoColor = (Color)typeof(Color).GetField(cannonTemplate.AmmoColor).GetValue(null);
            
            ShootTimer = new Timer();
            ShootTimer.Interval = Speed;

            if (cannonTemplate.BurstCount == 0)
            {
                ShootTimer.Timeout += Shoot;
            }
            else
            {
                ShootTimer.Timeout += delegate
                {
                    Timer burstTimer = new Timer(cannonTemplate.BurstDelay);
                    burstTimer.Timeout += Shoot;
                    burstTimer.Start(cannonTemplate.BurstCount);
                };
            }
            
            ShootTimer.Start();

            Game.Instance.Mouse.ListenOn(this, MouseButton.Left, ButtonState.Pressed, Upgrade, null);
        }

        /// <summary>
        /// Turns the cannon towards the nearest enemy
        /// </summary>
        public void Aim()
        {
            PhysicsObject nearestEnemy = ((JTD) JTD.Instance).FindEnemy(this);
            if (nearestEnemy != null)
            {
                Vector direction = (nearestEnemy.Position - Position).Normalize();
                Angle = direction.Angle;
            }
        }

        /// <summary>
        /// Shoot
        /// </summary>
        public void Shoot()
        {
            PhysicsObject nearestEnemy = ((JTD) JTD.Instance).FindEnemy(this); // TODO: nearest enemy is searched twice

            if (nearestEnemy != null)
            {
                Ammo ammo = new Ammo(Damage, AmmoColor);
                ammo.Position = Position;
                GameManager.Add(ammo);

                // Minor fix for ammo direction, needs more tweaking.
                Vector enemySpeed = nearestEnemy.Velocity;
                Vector enemyDist = nearestEnemy.Position - Position;
                Vector dirFix = enemyDist * 0.2 + enemySpeed * RandomGen.NextDouble(0.05, 2);

                double power = 500;
                Vector direction = (nearestEnemy.Position - Position).Normalize();
                ammo.Hit(ammo.Mass * direction * power + dirFix);
            }
        }
        
        
        public void BurstFire(double speed)
        {
            ShootTimer = new Timer();
            ShootTimer.Interval = speed;
            ShootTimer.Timeout += delegate { Shoot(); };
            ShootTimer.Start(3);
        }


        /// <summary>
        /// Upgrade cannon for more power.
        /// </summary>
        /// <param name="money">Players money</param>
        public void Upgrade()
        {
            Color[] colors = {Color.Red, Color.Green, Color.Blue, Color.White};
            if (GameManager.Money.Value >= Price * 2 && Level < 4)
            {
                Level++;
                ShootTimer.Interval = ShootTimer.Interval * 0.9;
                Damage = Convert.ToInt32(Damage * 1.5);
                Image image = Image.Clone(); // Edit image pixels directly for a new texture.
                for (int i = 2; i < 4; i++)
                {
                    for (int j = 13; j < 20; j++)
                        image[j, image.Height - i] = colors[Level - 1];
                }

                Image = image;
                Price = Price * 2;
                GameManager.Money.Value -= Convert.ToInt32(Price);
            }
        }
    }
}
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

        public Enemy(double width, double heigh, int health, int value, Image image, int speed,
            SortedList<char, Vector> route)
            : base(width, heigh)
        {
            Value = value;

            Health = new IntMeter(health, 0, health);
            Image = image;
            Health.LowerLimit += Destroy;
            Tag = "Enemy";
            CanRotate = false;
            IgnoresCollisionResponse = true;

            //Vihollisen tekoäly, joka toimii ehkä vähän hassusti.
            PathFollowerBrain brain = new PathFollowerBrain(new List<Vector>(route.Values));
            brain.Speed = speed;
            Brain = brain;

            ProgressBar HealthBar = new ProgressBar(width, 3, Health);
            HealthBar.BarColor = Color.DarkGreen;
            HealthBar.Color = Color.BloodRed;
            HealthBar.Bottom = Bottom - 5;
            Add(HealthBar);
        }
    }
}

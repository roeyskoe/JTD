using Jypeli;
using Jypeli.Widgets;

/// <summary>
/// Target you must protect
/// </summary>
class Target : PhysicsObject
{
    public IntMeter Health { get; private set; }

    public Target (double width, double height, int health, Image image)
        : base (width, height)
    {
        Health = new IntMeter (health, 0, health);
        Image = image;
        Health.LowerLimit += Destroy;
        Tag = "Target";
        CanRotate = false;
        IgnoresCollisionResponse = true;
        IgnoresExplosions = true;

        ProgressBar HealthBar = new ProgressBar (width, 3, Health);
        HealthBar.BarColor = Color.DarkGreen;
        HealthBar.Color = Color.BloodRed;
        HealthBar.Bottom = Bottom - 5;
        Add(HealthBar);
    }
}


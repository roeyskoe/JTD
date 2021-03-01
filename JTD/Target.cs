using Jypeli;
using Jypeli.Widgets;

/// <summary>
/// Suojeltavan kohteen luokka, siltä varalta jos joskus haluaisi useampia.
/// </summary>
class Target : PhysicsObject
{
    public IntMeter Elamalaskuri { get; private set; }

    public Target (double leveys, double korkeus, int elamaa, Image kuva)
        : base (leveys, korkeus)
    {
        Elamalaskuri = new IntMeter (elamaa, 0, elamaa);
        Image = kuva;
        Elamalaskuri.LowerLimit += Destroy;
        Tag = "JYFL";
        CanRotate = false;
        IgnoresCollisionResponse = true;
        IgnoresExplosions = true;

        ProgressBar ElamaPalkki = new ProgressBar (leveys, 3, Elamalaskuri);
        ElamaPalkki.BarColor = Color.DarkGreen;
        ElamaPalkki.Color = Color.BloodRed;
        ElamaPalkki.Bottom = Bottom - 5;
        Add (ElamaPalkki);
    }
}


using Jypeli;
using Jypeli.Widgets;

/// <summary>
/// Vihollisen luokka.
/// Siältää kaikki vihollisten ominaisuuksista vastaavat toiminnot.
/// </summary>
public class Enemy : PhysicsObject
{
	public IntMeter Elamalaskuri { get; private set; }

	public int Arvo { get; set; }

	public Enemy (double leveys, double korkeus, int elamaa, int arvo, Image kuva)
		: base (leveys, korkeus)
	{
		Arvo = arvo;

		Elamalaskuri = new IntMeter (elamaa, 0, elamaa);
		Image = kuva;
		Elamalaskuri.LowerLimit += Destroy;
		Tag = "Vihollinen";
		CanRotate = false;

		ProgressBar ElamaPalkki = new ProgressBar (leveys, 3, Elamalaskuri);
		ElamaPalkki.BarColor = Color.DarkGreen;
		ElamaPalkki.Color = Color.BloodRed;
		ElamaPalkki.Bottom = Bottom - 5;
		Add (ElamaPalkki);
	}
}


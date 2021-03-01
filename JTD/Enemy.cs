using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Widgets;

/// <summary>
/// Vihollisen luokka.
/// Siältää kaikki vihollisten ominaisuuksista vastaavat toiminnot.
/// </summary>
public class Enemy : PhysicsObject
{
	public IntMeter Elamalaskuri { get; private set; }

	public int Arvo { get; set; }

	public Enemy (double leveys, double korkeus, int elamaa, int arvo, Image kuva, int speed, SortedList<char, Vector> reitti)
		: base (leveys, korkeus)
	{
		Arvo = arvo;

		Elamalaskuri = new IntMeter (elamaa, 0, elamaa);
		Image = kuva;
		Elamalaskuri.LowerLimit += Destroy;
		Tag = "Vihollinen";
		CanRotate = false;
		IgnoresCollisionResponse = true;
		
		//Vihollisen tekoäly, joka toimii ehkä vähän hassusti.
		PathFollowerBrain polkuAivot = new PathFollowerBrain (new List<Vector> (reitti.Values));
		polkuAivot.Speed = speed;
		Brain = polkuAivot;
		
		ProgressBar ElamaPalkki = new ProgressBar (leveys, 3, Elamalaskuri);
		ElamaPalkki.BarColor = Color.DarkGreen;
		ElamaPalkki.Color = Color.BloodRed;
		ElamaPalkki.Bottom = Bottom - 5;
		Add (ElamaPalkki);
	}
}


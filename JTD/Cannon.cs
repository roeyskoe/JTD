using System;
using Jypeli;

/// <summary>
/// Tykkien luokka. 
/// Asettaa tykeille niille kuuluvan tekstuurin, nopeuden yms.
/// </summary>
public class Cannon : GameObject
{
	public double Hinta { get; set; }
	public int Vahinko { get; set; }
	public double Nopeus { get; set; }
	public Image kuva { get; set; }
	public int Versio { get; set; }
	public Color AmmuksenVari { get; set; }

	public Timer AmpumisAjastin { get; set; }
	public Timer Burst { get; set; }

	public Timer KaantymisAjastin { get; set; }

	public Cannon (double hinta, int vahinko, double nopeus, Image kuva)
		: base (35, 35)
	{
		Hinta = hinta;
		Vahinko = vahinko;
		Nopeus = nopeus;
		Image = kuva;
		Tag = "Tykki";
		AmmuksenVari = AmmuksenVari;
	}

	/// <summary>
	/// Käsittelee ammuksen osuman viholliseen.
	/// </summary>
	/// <param name="ammus">Ammus.</param>
	/// <param name="vihollinen">Vihollinen.</param>
	public void OsuuViholliseen (PhysicsObject ammus, PhysicsObject vihollinen)
	{
		ammus.Destroy ();
		((Enemy)vihollinen).Elamalaskuri.Value -= Vahinko;
	}
	
	/// <summary>
	/// Kääntää tornin vihollista kohti.
	/// Muutos on ainoastaan kosmeettinen eikä vaikuta mitenkään sen toimintaan.
	/// </summary>
	/// <param name="Torni">Torni.</param>
	public void KaannaTykki ()
	{
		PhysicsObject kohde = ((JTD)JTD.Instance).EtsiVihollinen(this);
		if (kohde != null) {
			Vector Suunta = (kohde.Position - Position).Normalize ();
			Angle = Suunta.Angle;
		}
	}
	
	/// <summary>
	/// Aliohjelma joka vastaa tykkien ampumisesta
	/// </summary>
	/// <param name="Torni">Torni.</param>
	public void Ammu ()
	{
		PhysicsObject kohde = ((JTD)JTD.Instance).EtsiVihollinen(this);

		if (kohde != null) // Tarkistetaan löytyikö vihollista
		{
			PhysicsObject ammus = new PhysicsObject (5, 5, Shape.Circle);
			ammus.Position = Position;
			ammus.Color = AmmuksenVari;
			ammus.LifetimeLeft = TimeSpan.FromSeconds (2);

			JTD.Instance.Add (ammus);

			//Luodaan pieni "korjauskerroin" jotta tykit osaavat tähdätä ennakkoon, mutta
			//kerrotaan se pienellä satunnaisuudella jotta tykit eivät olisi aivan liian tarkkoja.
			Vector nopeus = kohde.Velocity;
			Vector etaisyys = kohde.Position - Position;
			Vector tahtayskorjain = etaisyys * 0.1 + nopeus * RandomGen.NextDouble (0.05, 2);

			//Ammukset saattavat mennä suurilla nopueksilla vihollisesta läpi jos siltä tuntuu
			double ammuksenNopeus = 500;
			Vector Suunta = (kohde.Position - Position).Normalize ();
			ammus.Hit (ammus.Mass * Suunta * ammuksenNopeus + tahtayskorjain);

			JTD.Instance.AddCollisionHandler (ammus, "Vihollinen", OsuuViholliseen);
		}
	}
	
	
	public void BurstFire(double nopeus)
	{
		AmpumisAjastin = new Timer ();
		AmpumisAjastin.Interval = nopeus;
		AmpumisAjastin.Timeout += delegate { Ammu(); };
		AmpumisAjastin.Start (3);
	}
	
	
	/// <summary>
	/// Päivittää tornin ominaisuuksia paremmiksi.
	/// </summary>
	/// <param name="Torni"></param>
	public int PaivitaTykki (int raha)
	{
		Color [] varit = { Color.Red, Color.Green, Color.Blue, Color.White };
		if (raha >= Hinta * 2 && Versio < 4) {
			Versio++;
			AmpumisAjastin.Interval = AmpumisAjastin.Interval * 0.9;
			Vahinko = Convert.ToInt32 (Vahinko * 1.5);
			Image kuva = Image.Clone ();
			for (int i = 2; i < 4; i++) {
				for (int j = 13; j < 20; j++)
					kuva [j, kuva.Height - i] = varit [Versio - 1]; // Muutetaan kuvan pikseleitä.
			}
			Image = kuva;
			Hinta = Hinta * 2;
			raha -= Convert.ToInt32 (Hinta);
		}

		return raha;
	}

}



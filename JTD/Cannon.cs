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

}



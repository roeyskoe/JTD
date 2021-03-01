﻿using System;
using Jypeli;

/// <summary>
/// Cannon
/// </summary>
public class Cannon : GameObject
{
	public double Price { get; set; }
	public int Damage { get; set; }
	public double speed { get; set; }
	public int Level { get; set; }
	public Color AmmoColor { get; set; }

	public Timer ShootTimer { get; set; }
	public Timer BurstTimer { get; set; }
	public Timer TurnTimer { get; set; }

	public Cannon (double price, int damage, double speed, Image image)
		: base (35, 35)
	{
		Price = price;
		Damage = damage;
		this.speed = speed;
		Image = image;
		Tag = "Cannon";
		AmmoColor = AmmoColor;
	}

	/// <summary>
	/// When ammo hits an enemy.
	/// </summary>
	/// <param name="ammo">Ammo.</param>
	/// <param name="enemy">Enemy.</param>
	public void TargetHit(PhysicsObject ammo, PhysicsObject enemy)
	{
		ammo.Destroy();
		((Enemy)enemy).Health.Value -= Damage;
	}
	
	/// <summary>
	/// Turns the cannon towards the nearest enemy
	/// </summary>
	public void Aim()
	{
		PhysicsObject nearestEnemy = ((JTD)JTD.Instance).FindEnemy(this);
		if (nearestEnemy != null) {
			Vector direction = (nearestEnemy.Position - Position).Normalize();
			Angle = direction.Angle;
		}
	}
	
	/// <summary>
	/// Shoot
	/// </summary>
	public void Shoot()
	{
		PhysicsObject nearestEnemy = ((JTD)JTD.Instance).FindEnemy(this); // TODO: nearest enemy is searched twice

		if (nearestEnemy != null)
		{
			PhysicsObject ammo = new PhysicsObject (5, 5, Shape.Circle);
			ammo.Position = Position;
			ammo.Color = AmmoColor;
			ammo.LifetimeLeft = TimeSpan.FromSeconds(2);

			JTD.Instance.Add(ammo);

			// Minor fix for ammo direction, needs more tweaking.
			Vector enemySpeed = nearestEnemy.Velocity;
			Vector enemyDist = nearestEnemy.Position - Position;
			Vector dirFix = enemyDist * 0.2 + enemySpeed * RandomGen.NextDouble (0.05, 2);

			double power = 500;
			Vector direction = (nearestEnemy.Position - Position).Normalize ();
			ammo.Hit (ammo.Mass * direction * power + dirFix);

			JTD.Instance.AddCollisionHandler (ammo, "Enemy", TargetHit);
		}
	}
	
	
	public void BurstFire(double speed)
	{
		ShootTimer = new Timer ();
		ShootTimer.Interval = speed;
		ShootTimer.Timeout += delegate { Shoot(); };
		ShootTimer.Start (3);
	}


	/// <summary>
	/// Upgrade cannon for more power.
	/// </summary>
	/// <param name="money">Players money</param>
	public void Upgrade(IntMeter money)
	{
		Color[] colors = { Color.Red, Color.Green, Color.Blue, Color.White };
		if (money.Value >= Price * 2 && Level < 4) {
			Level++;
			ShootTimer.Interval = ShootTimer.Interval * 0.9;
			Damage = Convert.ToInt32 (Damage * 1.5);
			Image image = Image.Clone(); // Edit image pixels directly for a new texture.
			for (int i = 2; i < 4; i++) {
				for (int j = 13; j < 20; j++)
					image [j, image.Height - i] = colors[Level - 1];
			}
			Image = image;
			Price = Price * 2;
			money.Value -= Convert.ToInt32(Price);
		}
	}

}



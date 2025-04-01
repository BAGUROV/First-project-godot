using Godot;
using System;

public partial class Villager : CharacterBody3D
{
	[Export]
	public float MaxHealth = 20.0f;

	public Rig Rig;
	public HealthComponent Health;
	public CollisionShape3D Collision;
	public ShapeCast3D Detector;

	public override void _Ready()
	{
		base._Ready();

		Rig = GetNode<Rig>("Rig");
		Health = GetNode<HealthComponent>("HealthComponent");
		Collision = GetNode<CollisionShape3D>("CollisionShape3D");
		Detector = GetNode<ShapeCast3D>("PlayerDetector");

		var randomNumber = new Random();
		Rig.SetActiveMesh(Rig.VillagerMesh[randomNumber.Next(Rig.VillagerMesh.Length)]);

		Health.UpdateMaxHealth(MaxHealth);
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (Rig.IsIdle())
			CheckForAttacks();
	}

	private void CheckForAttacks()
	{
		for (int i=0; i<Detector.GetCollisionCount(); i++)
		{
			if (Detector.GetCollider(i) is MainCharacter main)
			{
				Rig.Travel("Overhead");
			}
		}
	}

	public void OnHealthComponentDefeat()
	{
		Rig.Travel("Defeat");
		Collision.Disabled = true;
		SetPhysicsProcess(false);
	}

	public void OnRigHeavyAttack()
	{
		GD.Print("Heave Attack");
	}
}

using Godot;
using System;

public partial class Villager : CharacterBody3D
{
	[Export]
	public float MaxHealth = 20.0f;

	public Rig Rig;
	public HealthComponent Health;

	public override void _Ready()
	{
		base._Ready();

		Rig = GetNode<Rig>(@"Rig");
		Health = GetNode<HealthComponent>("HealthComponent");

		var randomNumber = new Random();
		Rig.SetActiveMesh(Rig.VillagerMesh[randomNumber.Next(Rig.VillagerMesh.Length)]);

		Health.UpdateMaxHealth(MaxHealth);
	}

	public void OnHealthComponentDefeat()
	{
		Rig.Travel("Defeat");
	}
}

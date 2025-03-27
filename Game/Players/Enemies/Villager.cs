using Godot;
using System;

public partial class Villager : CharacterBody3D
{
	public Rig Rig;
	public override void _Ready()
	{
		base._Ready();

		Rig = GetNode<Rig>(@"Rig"); 
		var randomNumber = new Random();
		Rig.SetActiveMesh(Rig.VillagerMesh[randomNumber.Next(Rig.VillagerMesh.Length)]);
	}
}

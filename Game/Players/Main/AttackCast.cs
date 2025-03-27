using Godot;
using System;

public partial class AttackCast : RayCast3D
{
	public void DealDamage()
	{
		if(!IsColliding())
			return;

		var collider = GetCollider();
		GD.Print(collider);
		AddException((CollisionObject3D)collider);
	}
}

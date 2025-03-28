using Godot;
using System;

public partial class AttackCast : RayCast3D
{
	public void DealDamage()
	{
		if(!IsColliding())
			return;

		var collider = GetCollider();

		if (collider is Villager villager)
		{
			villager.Health.TakeDamage(15.0f);
			AddException((CollisionObject3D)collider);
		}
	}
}

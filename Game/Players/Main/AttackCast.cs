using Godot;
using System;

public partial class AttackCast : RayCast3D
{
	public void DealDamage(float damageIn)
	{
		if(!IsColliding())
			return;

		var collider = GetCollider();

		if (collider is Villager villager)
		{
			villager.Health.TakeDamage(damageIn);
			AddException((CollisionObject3D)collider);
		}
		else if (collider is MainCharacter main)
		{
			main.Health.TakeDamage(damageIn);
			AddException((CollisionObject3D)collider);
		}
	}
}

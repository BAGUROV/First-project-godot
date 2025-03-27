using Godot;
using System;

public partial class SmoothCameraArm : SpringArm3D
{
	[Export]
	public Node3D Target;
	[Export]
	public double Decay = 20.0;

	public override void _PhysicsProcess(double delta)
	{
		GlobalTransform = GlobalTransform.InterpolateWith(Target.GlobalTransform, 1.0f - (float)Math.Exp(-Decay * delta));
	}
}

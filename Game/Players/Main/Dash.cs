using Godot;
using System;

public partial class Dash : Node3D
{
	private Vector3 _direction = Vector3.Zero;
	private double _timeRemaining = 0.0;
	private double _dashDuration = 0.1;

	public Timer TimerDash;
	public GpuParticles3D Gpu;

	[Export]
	public MainCharacter Main;
	[Export]
	public float SpeedMultiplier = 2.0f;

	public override void _Ready()
	{
		base._Ready();

		TimerDash = GetNode<Timer>("Timer");
		Gpu = GetNode<GpuParticles3D>("GPUParticles3D");
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		base._UnhandledInput(@event);

		if (!TimerDash.IsStopped())
			return;

		if (@event.IsActionPressed("Dash"))
		{
			_direction = Main.GetMovementDirection();

			if (!_direction.IsZeroApprox())
			{
				Main.Rig.Travel("Dash");
				Gpu.Emitting = true;
				TimerDash.Start(1.0);
				_timeRemaining = _dashDuration;
			}
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (_direction.IsZeroApprox())
			return;

		Main.Velocity = _direction * Main.Speed * SpeedMultiplier;
		_timeRemaining -= delta;
		if (_timeRemaining <= 0)
		{
			_direction = Vector3.Zero;
			Gpu.Emitting = false;
		}
	}

}

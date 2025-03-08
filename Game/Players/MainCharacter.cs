using Godot;
using System;
using static Godot.Input;

public partial class MainCharacter : CharacterBody3D
{
	private Vector2 _look = Vector2.Zero;
	
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;
	public Variant Gravity = ProjectSettings.GetSetting("physics/3d/default_gravity");
	public Node3D HorizontalPivot;
	public Node3D VerticalPivot;
	
	[Export]
	public float MouseSensitivity = 0.00075f;
	[Export]
	public float MinBoundary = -60f;
	[Export]
	public float MaxBoundary = 10f;

	public override void _Ready()
	{
		Input.MouseMode = MouseModeEnum.Captured;
		HorizontalPivot = GetNode<Node3D>("HorizontalPivot");
		VerticalPivot = GetNode<Node3D>("HorizontalPivot/VerticalPivot");
	}

	public override void _PhysicsProcess(double delta)
	{
		FrameCameraRotation();
		
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		Vector3 direction = GetMovementDirection();
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel"))
			Input.MouseMode = MouseModeEnum.Visible;
		if (Input.MouseMode == MouseModeEnum.Captured)
			if (@event is InputEventMouseMotion)
				{
					_look = -(@event as InputEventMouseMotion).Relative * MouseSensitivity;
					//GD.Print(_look);
				}
	}

	private Vector3 GetMovementDirection()
	{
		Vector2 inputDir = Input.GetVector("MoveLeft", "MoveRight", "MoveForward", "MoveBack");
		var input_vector =  new Vector3(inputDir.X, 0, inputDir.Y).Normalized();

		return HorizontalPivot.GlobalTransform.Basis * input_vector;
	}

	private void FrameCameraRotation()
	{
		HorizontalPivot.RotateY(_look.X);
		VerticalPivot.RotateX(_look.Y);

		var rotation = VerticalPivot.Rotation;
		rotation.X = (float)Math.Clamp(rotation.X, MinBoundary * (Math.PI/180.0), MaxBoundary * (Math.PI/180.0));
		VerticalPivot.Rotation = rotation;
		
		GetNode<SpringArm3D>("SA3DMain").GlobalTransform = VerticalPivot.GlobalTransform;
		_look = Vector2.Zero;
	}
}

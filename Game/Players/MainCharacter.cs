using Godot;
using System;
using static Godot.Input;

public partial class MainCharacter : CharacterBody3D
{
	// Направление x/y, в котором игрок смотрит
	private Vector3 _look = Vector3.Zero;
	// Направление игрока при атаке
	private Vector3 _attackDirection = Vector3.Zero;
	
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;
	// Синхронизация гравитации из настроек проекта с узлами RigidBody.
	public Variant Gravity = ProjectSettings.GetSetting("physics/3d/default_gravity");
	public Node3D HorizontalPivot;
	public Camera3D Camera;
	public Node3D VerticalPivot;
	public Node3D RigPivot;
	public Rig Rig;
	
	[Export]
	public float MouseSensitivity = 0.00075f;
	[Export]
	public float MinBoundary = -60f;
	[Export]
	public float MaxBoundary = 10f;
	[Export]
	public float AttackMoveSpeed = 3.0f;
	[Export]
	public double AnimationDecay = 15.0;
	
	public override void _Ready()
	{
		Input.MouseMode = MouseModeEnum.Captured;
		HorizontalPivot = GetNode<Node3D>("HorizontalPivot");
		RigPivot = GetNode<Node3D>("RigPivot");
		Rig = GetNode<Rig>("RigPivot/Rig");
		VerticalPivot = GetNode<Node3D>("HorizontalPivot/VerticalPivot");
		Camera = GetNode<Camera3D>("SmoothCameraArm/C3DMain");
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
		Rig.UpdateAnimationTree(direction);
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
			LookTowardDirection(direction, delta);
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		HandleSlashingPhysicsFrame(delta, ref velocity);

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
				_look = new Vector3(-(@event as InputEventMouseMotion).Relative.X * MouseSensitivity,
								-(@event as InputEventMouseMotion).Relative.Y * MouseSensitivity,
								0);
				//GD.Print(_look);
			}
		if (Rig.IsIdle())
		{
			if(@event.IsActionPressed("Click"))
				SlashAttack();
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
		
		GetNode<SpringArm3D>("SmoothCameraArm").GlobalTransform = VerticalPivot.GlobalTransform;
		_look = Vector3.Zero;
	}

	private void LookTowardDirection(Vector3 direction, double delta)
	{
		var targetTransform = RigPivot.GlobalTransform.LookingAt(RigPivot.GlobalPosition + direction, Vector3.Up, true);
		
		RigPivot.GlobalTransform = RigPivot.GlobalTransform.InterpolateWith(targetTransform, 1.0f - (float)Math.Exp(-AnimationDecay * delta));
	}
	
	private void SlashAttack()
	{
		Rig.Travel("Slash");

		// Атака в направление камеры
		Vector3 cameraDirection = -Camera.GlobalBasis.Z.Normalized();
		_attackDirection = new Vector3(cameraDirection.X, 0, cameraDirection.Z).Normalized();

		// Атака в направление модельки
		// _attackDirection = GetMovementDirection();

		// if (_attackDirection.IsZeroApprox())
		// {	
		// 	_attackDirection = Rig.GlobalBasis * new Vector3(0, 0, 1);
		// }
	}

	private void HandleSlashingPhysicsFrame(double delta, ref Vector3 velocity)
	{
		if(!Rig.IsSlashing())
			return;
		
		velocity.X = _attackDirection.X * AttackMoveSpeed;
		velocity.Z = _attackDirection.Z * AttackMoveSpeed;

		LookTowardDirection(_attackDirection, delta);
	}
}

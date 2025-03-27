using Godot;
using System;
using static Godot.Input;

public partial class MainCharacter : CharacterBody3D
{
	// Направление мыши игрока x/y
	private Vector2 _look = Vector2.Zero;
	// Направление игрока при атаке
	private Vector3 _attackDirection = Vector3.Zero;
	
	public float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;
	// Синхронизация гравитации из настроек проекта с узлами RigidBody.
	public Variant Gravity = ProjectSettings.GetSetting("physics/3d/default_gravity");
	public Node3D HorizontalPivot;
	public Camera3D Camera;
	public Node3D VerticalPivot;
	public Node3D RigPivot;
	public Rig Rig;
	public AttackCast Attack;
	
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
		Attack = GetNode<AttackCast>("RigPivot/Rig/RayAttachment/AttackCast");
	}

	public override void _PhysicsProcess(double delta)
	{
		FrameCameraRotation();
		
		Vector3 velocity = Velocity;
		Vector3 direction = GetMovementDirection();
		Rig.UpdateAnimationTree(ref direction);

		HandleMovingPhysicsFrame(delta, ref velocity, ref direction);
		HandleSlashingPhysicsFrame(delta, ref velocity);
		HandleJumpingPhysicsFrame(delta, ref velocity);

		Velocity = velocity;
		MoveAndSlide();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel"))
			Input.MouseMode = MouseModeEnum.Visible;
		if (Input.MouseMode == MouseModeEnum.Captured)
		{
			if (@event is InputEventMouseMotion)
				_look = -(@event as InputEventMouseMotion).Relative * MouseSensitivity;
		}
		if (@event.IsActionPressed("Click"))
		{
			if(Rig.IsIdle())
				SlashAttack();
		}
	}

	private void FrameCameraRotation()
	{
		HorizontalPivot.RotateY(_look.X);
		VerticalPivot.RotateX(_look.Y);

		var rotation = VerticalPivot.Rotation;
		rotation.X = (float)Math.Clamp(rotation.X, MinBoundary * (Math.PI/180.0), MaxBoundary * (Math.PI/180.0));
		VerticalPivot.Rotation = rotation;
		
		GetNode<SpringArm3D>("SmoothCameraArm").GlobalTransform = VerticalPivot.GlobalTransform;
		_look = Vector2.Zero;
	}

	private void HandleMovingPhysicsFrame(double delta, ref Vector3 velocity, ref Vector3 direction)
	{
		if(!Rig.IsIdle())
			return;

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
	}
	
	private void HandleJumpingPhysicsFrame(double delta, ref Vector3 velocity)
	{
		// Гравитации
		if (!IsOnFloor())
			velocity += GetGravity() * (float)delta;

		// Прыжок.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
			velocity.Y = JumpVelocity;
	}

	private void HandleSlashingPhysicsFrame(double delta, ref Vector3 velocity)
	{
		if(!Rig.IsSlashing())
			return;

		// При ударе модель движется вперёд
		velocity.X = _attackDirection.X * AttackMoveSpeed;
		velocity.Z = _attackDirection.Z * AttackMoveSpeed;

		LookTowardDirection(_attackDirection, delta);
		Attack.DealDamage();
	}

	private void LookTowardDirection(Vector3 direction, double delta)
	{
		var targetTransform = RigPivot.GlobalTransform.LookingAt(RigPivot.GlobalPosition + direction, Vector3.Up, true);
		
		RigPivot.GlobalTransform = RigPivot.GlobalTransform.InterpolateWith(targetTransform, 1.0f - (float)Math.Exp(-AnimationDecay * delta));
	}

	private Vector3 GetMovementDirection()
	{
		Vector2 inputDir = Input.GetVector("MoveLeft", "MoveRight", "MoveForward", "MoveBack");
		var input_vector =  new Vector3(inputDir.X, 0, inputDir.Y).Normalized();

		return HorizontalPivot.GlobalTransform.Basis * input_vector;
	}

	private void SlashAttack()
	{
		Rig.Travel("Slash");

		if(!Rig.IsMoving())
		{
			// Атака в направление камеры
			Vector3 cameraDirection = -Camera.GlobalBasis.Z.Normalized();
			_attackDirection = new Vector3(cameraDirection.X, 0, cameraDirection.Z).Normalized();
		}
		else
		{
			// Атака в направление модели
			_attackDirection = GetMovementDirection();

			if (_attackDirection.IsZeroApprox())
			{	
				_attackDirection = Rig.GlobalBasis * new Vector3(0, 0, 1);
			}
		}
		Attack.ClearExceptions();
	}
}

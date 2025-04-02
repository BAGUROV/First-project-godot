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
	public float JumpVelocity = 4.5f;
	public double Decay = 8.0;
	// Синхронизация гравитации из настроек проекта с узлами RigidBody
	public Variant Gravity = ProjectSettings.GetSetting("physics/3d/default_gravity");
	public Node3D HorizontalPivot;
	public Camera3D Camera;
	public Node3D VerticalPivot;
	public Node3D RigPivot;
	public Rig Rig;
	public AttackCast Attack;
	public HealthComponent Health;
	public CollisionShape3D Collision;
	
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
	[Export]
	public float MaxHealth = 10.0f;
	[Export]
	public float Damage = 15.0f;
	
	public override void _Ready()
	{
		Input.MouseMode = MouseModeEnum.Captured;
		HorizontalPivot = GetNode<Node3D>("HorizontalPivot");
		RigPivot = GetNode<Node3D>("RigPivot");
		Rig = GetNode<Rig>("RigPivot/Rig");
		VerticalPivot = GetNode<Node3D>("HorizontalPivot/VerticalPivot");
		Camera = GetNode<Camera3D>("SmoothCameraArm/C3DMain");
		Attack = GetNode<AttackCast>("RigPivot/Rig/RayAttachment/AttackCast");
		Health = GetNode<HealthComponent>("HealthComponent");
		Collision = GetNode<CollisionShape3D>("CollisionShape3D");

		Health.UpdateMaxHealth(MaxHealth);
	}

	public override void _PhysicsProcess(double delta)
	{
		FrameCameraRotation();
		
		Vector3 velocity = Velocity;
		Vector3 direction = GetMovementDirection();
		Rig.UpdateAnimationTree(ref direction);

		HandleMovingPhysicsFrame(delta, ref velocity, ref direction);
		HandleSlashingPhysicsFrame(delta, ref velocity);
		HandleOverheadPhysicsFrame(delta, ref velocity);
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
		if (@event.IsActionPressed("RightClick"))
		{
			if(Rig.IsIdle())
				OverheadAttack();
		}
	}

	public void OnHealthComponentDefeat()
	{
		Rig.Travel("Defeat");
		Collision.Disabled = true;
		SetPhysicsProcess(false);
	}

	public Vector3 GetMovementDirection()
	{
		Vector2 inputDir = Input.GetVector("MoveLeft", "MoveRight", "MoveForward", "MoveBack");
		var input_vector =  new Vector3(inputDir.X, 0, inputDir.Y).Normalized();

		return HorizontalPivot.GlobalTransform.Basis * input_vector;
	}

	public float ExponentialDecay(float a, float b, double decay, double delta)
		=> (float)(b + (a - b) * Math.Exp(-decay * delta));

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
		if(!Rig.IsIdle() && !Rig.IsDashing())
			return;

		velocity.X = ExponentialDecay(velocity.X, direction.X * Speed, Decay, delta);
		velocity.Z = ExponentialDecay(velocity.Z, direction.Z * Speed, Decay, delta);

		if (direction != Vector3.Zero)
			LookTowardDirection(direction, delta);
	}
	
	private void HandleJumpingPhysicsFrame(double delta, ref Vector3 velocity)
	{
		// Гравитации
		if (!IsOnFloor())
			velocity += GetGravity() * (float)delta;

		// Прыжок
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
		Attack.DealDamage(Damage);
	}

	private void HandleOverheadPhysicsFrame(double delta, ref Vector3 velocity)
	{
		if(!Rig.IsOverhead())
			return;

		// При ударе модель не движется
		velocity.X = 0;
		velocity.Z = 0;

		LookTowardDirection(_attackDirection, delta);
		Attack.DealDamage(Damage);
	}

	private void LookTowardDirection(Vector3 direction, double delta)
	{
		var targetTransform = RigPivot.GlobalTransform.LookingAt(RigPivot.GlobalPosition + direction, Vector3.Up, true);
		
		RigPivot.GlobalTransform = RigPivot.GlobalTransform.InterpolateWith(targetTransform, 1.0f - (float)Math.Exp(-AnimationDecay * delta));
	}

	private void SlashAttack()
	{
		Rig.Travel("Slash");
		Damage = 10.0f;
		ChangeCharacterDirection();
		Attack.ClearExceptions();
	}

	private void OverheadAttack()
	{
		Rig.Travel("Overhead");
		Damage = 20.0f;
		ChangeCharacterDirection();
		Attack.ClearExceptions();
	}

	private void ChangeCharacterDirection()
	{
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
				_attackDirection = Rig.GlobalBasis * new Vector3(0, 0, 1);
		}
	}
}

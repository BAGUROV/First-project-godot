using Godot;
using System;

public partial class Rig : Node3D
{
	private string _runPath;
	private float _runWeightTarget;

	[Signal]
	public delegate void HeavyAttackEventHandler();

	public AnimationTree ATree;
	public AnimationNodeStateMachinePlayback Playback;
	public Skeleton3D SkeletonRig;
	public MeshInstance3D[] VillagerMesh;

	[Export]
	public double AnimationSpeed = 20.0f;

	public override void _Ready()
	{
		ATree = GetNode<AnimationTree>("AnimationTree");
		SkeletonRig = GetNode<Skeleton3D>("CharacterRig/GameRig/Skeleton3D");
		VillagerMesh = [ 
			GetNode<MeshInstance3D>("CharacterRig/GameRig/Skeleton3D/Villager_01"), 
			GetNode<MeshInstance3D>("CharacterRig/GameRig/Skeleton3D/Villager_02") 
		];

		Playback = (AnimationNodeStateMachinePlayback)ATree.Get("parameters/playback");
		_runPath = "parameters/MoveSpace/blend_position";
		_runWeightTarget = -1.0f;
	}
	
	public override void _PhysicsProcess(double delta)
	{
		ATree.Set(_runPath, Mathf.MoveToward((float)ATree.Get(_runPath), _runWeightTarget, delta * AnimationSpeed));
	}

	public void UpdateAnimationTree(ref Vector3 direction)
	{
		if(direction.IsZeroApprox())
			_runWeightTarget = -1.0f;
		else
			_runWeightTarget = 1.0f;
	}

	public void Travel(string AnimationName)
		=> Playback.Travel(AnimationName);
	public bool IsMoving()
		=> (float)ATree.Get(_runPath) == 1.0f;
	public bool IsIdle()
		=> Playback.GetCurrentNode() == "MoveSpace";
	public bool IsSlashing()
		=> Playback.GetCurrentNode() == "Slash";
	public bool IsOverhead()
		=> Playback.GetCurrentNode() == "Overhead";
	public bool IsDashing()
		=> Playback.GetCurrentNode() == "Dash";

	public void SetActiveMesh(MeshInstance3D activeMesh)
	{
		foreach(MeshInstance3D item in SkeletonRig.GetChildren())
			item.Visible = false;

		activeMesh.Visible = true;
	}
	
	public void OnAnimationTreeAnimationFinished(string AnimationName)
	{
		if (AnimationName == "Overhead")
			EmitSignal(SignalName.HeavyAttack);
	}
}

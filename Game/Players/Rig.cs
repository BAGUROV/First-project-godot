using Godot;
using System;

public partial class Rig : Node3D
{
	private string _runPath;
	private float _runWeightTarget;

	public AnimationTree ATree;
	public AnimationNodeStateMachinePlayback Playback;

	[Export]
	public double AnimationSpeed = 20.0f;

	public override void _Ready()
	{
		ATree = GetNode<AnimationTree>("AnimationTree");
		Playback = (AnimationNodeStateMachinePlayback)ATree.Get("parameters/playback");
		_runPath = "parameters/MoveSpace/blend_position";
		_runWeightTarget = -1.0f;
	}
	
	public override void _PhysicsProcess(double delta)
	{
		ATree.Set(_runPath, Mathf.MoveToward((float)ATree.Get(_runPath), _runWeightTarget, delta * AnimationSpeed));
	}

	public void UpdateAnimationTree(Vector3 direction)
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
}

using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystem : MonoBehaviour
{
	[Header("Character Input Values")]
	public Vector2 move;
	public Vector2 look;
	public bool jump;
	public bool sprint;
	public bool shoot;
	public bool pass;
	public bool test;
	public bool diveLeft;
	public bool diveRight;

	[Header("Movement Settings")]
	public bool analogMovement;
	
	public void MoveInput(Vector2 newMoveDirection)
	{
		move = newMoveDirection;
	} 

	public void LookInput(Vector2 newLookDirection)
	{
		look = newLookDirection;
	}

	public void JumpInput(bool newJumpState)
	{
		jump = newJumpState;
	}

	public void SprintInput(bool newSprintState)
	{
		sprint = newSprintState;
	}

	public void ShootInput(bool newShootState)
	{
		shoot = newShootState;
	}
	public void PassInput(bool newPassState)
	{
		pass = newPassState;
	}

	public void TestInput(bool newTestState)
	{
		test = newTestState;
	}

	public void DiveLeftInput(bool newDiveLeftState)
	{
		diveLeft = newDiveLeftState;
	}

	public void DiveRightInput(bool newDiveRightState)
	{
		diveRight = newDiveRightState;
	}	

}

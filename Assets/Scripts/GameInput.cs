using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    [SerializeField] Game scriptGame;
	[SerializeField] private GameObject standStadiumCamera;

#if !UNITY_IOS || !UNITY_ANDROID
	[Header("Mouse Cursor Settings")]
	public bool cursorLocked = true;
	public bool cursorInputForLook = true;
#endif

	private void OnViewThirdPerson()
    {
		standStadiumCamera.SetActive(true);
		scriptGame.StadiumCamera.enabled = false;
	}

	private void OnViewStadium()
    {
		standStadiumCamera.SetActive(false);
		scriptGame.StadiumCamera.enabled = true;
    }

    private void OnChangeShirt()
    {
        scriptGame.ChangeShirt();
    }

	public void OnMove(InputValue value)
	{
		scriptGame.ActiveHumanPlayer.GetComponent<InputSystem>().MoveInput(value.Get<Vector2>());
	}

	public void OnLook(InputValue value)
	{
		if (cursorInputForLook)
		{
			scriptGame.ActiveHumanPlayer.GetComponent<InputSystem>().LookInput(value.Get<Vector2>());
		}
	}

	public void OnJump(InputValue value)
	{
		scriptGame.ActiveHumanPlayer.GetComponent<InputSystem>().JumpInput(value.isPressed);
	}

	public void OnShoot(InputValue value)
	{
		scriptGame.ActiveHumanPlayer.GetComponent<InputSystem>().ShootInput(value.isPressed);
	}

	public void OnPass(InputValue value)
	{
		scriptGame.ActiveHumanPlayer.GetComponent<InputSystem>().PassInput(value.isPressed);
	}

	public void OnSprint(InputValue value)
	{
		scriptGame.ActiveHumanPlayer.GetComponent<InputSystem>().SprintInput(value.isPressed);
	}

	public void OnTest(InputValue value)
	{
		scriptGame.ActiveHumanPlayer.GetComponent<InputSystem>().TestInput(value.isPressed);
	}

	public void OnDiveRight(InputValue value)
	{
		scriptGame.ActiveHumanPlayer.GetComponent<InputSystem>().DiveRightInput(value.isPressed);
	}

	public void OnDiveLeft(InputValue value)
	{
		scriptGame.ActiveHumanPlayer.GetComponent<InputSystem>().DiveLeftInput(value.isPressed);
	}

	public void OnSelectNextPlayer(InputValue value)
	{
		scriptGame.SetNextHumanPlayer();
	}

#if !UNITY_IOS || !UNITY_ANDROID
	private void OnApplicationFocus(bool hasFocus)
	{
		SetCursorState(cursorLocked);
	}

	private void SetCursorState(bool newState)
	{
		Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
	}
#endif
}

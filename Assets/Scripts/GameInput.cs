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
		scriptGame.SetMessage("Changed to Player view");
	}

	private void OnViewStadium()
    {
		standStadiumCamera.SetActive(false);
		scriptGame.StadiumCamera.enabled = true;
		scriptGame.SetMessage("Changed to Stadium view");
	}

	private void OnChangeShirt()
    {
        scriptGame.ChangeShirt();
		scriptGame.SetMessage("Changed shirt colors");
	}

	private void OnFormation_343()
	{
		Formation.Set(scriptGame.Teams[0], Formation_._343);
		scriptGame.SetMessage("Changed formation to 3-4-3");
	}

	private void OnFormation_433()
	{
		Formation.Set(scriptGame.Teams[0], Formation_._433);
		scriptGame.SetMessage("Changed formation to 4-3-3");
	}

	private void OnFormation_442()
	{
		Formation.Set(scriptGame.Teams[0], Formation_._442);
		scriptGame.SetMessage("Changed formation to 4-4-2");
	}

	private void OnFormation_532()
	{
		Formation.Set(scriptGame.Teams[0], Formation_._532);
		scriptGame.SetMessage("Changed formation to 5-3-2");
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

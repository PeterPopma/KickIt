using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameInput : MonoBehaviour
{
#if !UNITY_IOS || !UNITY_ANDROID
	[Header("Mouse Cursor Settings")]
	public bool cursorLocked = true;
	public bool cursorInputForLook = true;
#endif

	private void OnViewThirdPerson()
    {
		Game.Instance.ActiveHumanPlayer.transform.Find("SelectedMarker").gameObject.SetActive(false);
		Game.Instance.StadiumCamera.enabled = false;
		Game.Instance.SetMessage("Changed to Player view");
	}

	private void OnViewStadium()
    {
		Game.Instance.ActiveHumanPlayer.transform.Find("SelectedMarker").gameObject.SetActive(true);
		Game.Instance.StadiumCamera.enabled = true;
		Game.Instance.SetMessage("Changed to Stadium view");
	}

	private void OnChangeShirt()
    {
		Game.Instance.ChangeShirt();
		Game.Instance.SetMessage("Changed shirt colors");
	}

	private void OnFormation_343()
	{
		Formation.Set(Game.Instance.Teams[0], Formation_._343);
		Game.Instance.SetMessage("Changed formation to 3-4-3");
	}

	private void OnFormation_433()
	{
		Formation.Set(Game.Instance.Teams[0], Formation_._433);
		Game.Instance.SetMessage("Changed formation to 4-3-3");
	}

	private void OnFormation_442()
	{
		Formation.Set(Game.Instance.Teams[0], Formation_._442);
		Game.Instance.SetMessage("Changed formation to 4-4-2");
	}

	private void OnFormation_532()
	{
		Formation.Set(Game.Instance.Teams[0], Formation_._532);
		Game.Instance.SetMessage("Changed formation to 5-3-2");
	}

	public void OnMove(InputValue value)
	{
		Game.Instance.ActiveHumanPlayer.GetComponent<InputSystem>().MoveInput(value.Get<Vector2>());
	}

	public void OnLook(InputValue value)
	{
		if (cursorInputForLook)
		{
			Game.Instance.ActiveHumanPlayer.GetComponent<InputSystem>().LookInput(value.Get<Vector2>());
		}
	}

	public void OnJump(InputValue value)
	{
		Game.Instance.ActiveHumanPlayer.GetComponent<InputSystem>().JumpInput(value.isPressed);
	}

	public void OnShoot(InputValue value)
	{
		Game.Instance.ActiveHumanPlayer.GetComponent<InputSystem>().ShootInput(value.isPressed);
	}

	public void OnPass(InputValue value)
	{
		Game.Instance.ActiveHumanPlayer.GetComponent<InputSystem>().PassInput(value.isPressed);
	}

	public void OnSprint(InputValue value)
	{
		Game.Instance.ActiveHumanPlayer.GetComponent<InputSystem>().SprintInput(value.isPressed);
	}

	public void OnTest(InputValue value)
	{
		//Game.Instance.ActiveHumanPlayer.GetComponent<InputSystem>().TestInput(value.isPressed);
		Game.Instance.GiveYellowCard(null);
	}

	public void OnDiveRight(InputValue value)
	{
		Game.Instance.ActiveHumanPlayer.GetComponent<InputSystem>().DiveRightInput(value.isPressed);
	}

	public void OnDiveLeft(InputValue value)
	{
		Game.Instance.ActiveHumanPlayer.GetComponent<InputSystem>().DiveLeftInput(value.isPressed);
	}

	public void OnSelectNextPlayer(InputValue value)
	{
		Game.Instance.SetNextHumanPlayer();
	}
	public void OnContinue()
	{
        if (Game.Instance.GameState.Equals(GameState_.MatchOver))
        {
			Game.Instance.SetGameState(GameState_.NewMatch);
		}
	}

	public void OnBack()
	{
		if (Game.Instance.GameState.Equals(GameState_.MatchOver))
		{
			SceneManager.LoadSceneAsync("Menu");
		}
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

using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HumanPlayer : Player
{
    private ThirdPersonController scriptThirdPersonController;
    private CinemachineVirtualCamera playerFollowCamera;
    protected InputSystem inputSystem;
    private PlayerInput playerInput;
    public PlayerInput PlayerInput { get => playerInput; set => playerInput = value; }
    public ThirdPersonController ScriptThirdPersonController { get => scriptThirdPersonController; set => scriptThirdPersonController = value; }


    protected void Awake()
    {
        base.Awake();

        playerFollowCamera = GameObject.Find("PlayerFollowCamera").GetComponent<CinemachineVirtualCamera>();
        scriptThirdPersonController = GetComponent<ThirdPersonController>();
        inputSystem = GetComponent<InputSystem>(); 
        playerInput = GetComponent<PlayerInput>();
    }
    protected void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected void Update()
    {
        base.Update();

        if (Game.Instance.GameState == GameState_.Replay)
        {
            if (inputSystem.shoot)
            {
                Game.Instance.Recorder.EndReplay();
            }
        }
    }
    public void Activate()
    {
        Game.Instance.ActiveHumanPlayer = this;
        playerInput.enabled = true;
    }

    public void SwitchActivePlayer()
    {
        PlayerInput.enabled = false;
        ((HumanPlayer)fellowPlayer).Activate();
        playerFollowCamera.Follow = fellowPlayer.PlayerCameraRoot;
    }

}

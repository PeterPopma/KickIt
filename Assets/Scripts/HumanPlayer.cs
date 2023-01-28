using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HumanPlayer : Player
{
    public PlayerInput PlayerInput { get => playerInput; set => playerInput = value; }
    public ThirdPersonController ScriptThirdPersonController { get => scriptThirdPersonController; set => scriptThirdPersonController = value; }

    protected InputSystem inputSystem;
    protected float shootingPower;
    private ThirdPersonController scriptThirdPersonController;
    private CinemachineVirtualCamera playerFollowCamera;
    private PlayerInput playerInput;


    protected new void Awake()
    {
        base.Awake();

        playerFollowCamera = GameObject.Find("PlayerFollowCamera").GetComponent<CinemachineVirtualCamera>();
        scriptThirdPersonController = GetComponent<ThirdPersonController>();
        inputSystem = GetComponent<InputSystem>(); 
        playerInput = GetComponent<PlayerInput>();
    }
    protected new void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected new void Update()
    {
        base.Update();

        if (Game.Instance.GameState == GameState_.Playing ||
     (Game.Instance.GameState == GameState_.BringingBallIn && DoingKick))
        {
            if (inputSystem.pass)
            {
                inputSystem.pass = false;
                if (HasBall)
                {
                    SetPlayerAction(ActionType_.Pass);
                }
                else
                {
                    SwitchActivePlayer();
                }
            }

            if (inputSystem.shoot)
            {
                if (HasBall)
                {
                    {
                        shootingPower += 1.5f * Time.deltaTime;
                        Game.Instance.SetPowerBar(shootingPower);
                        if (shootingPower > 1)
                        {
                            shootingPower = 1;
                        }
                    }
                }
                else
                {
                    inputSystem.shoot = false;
                    SetPlayerAction(ActionType_.Tackle);
                }
            }

            if (inputSystem.shoot == false && HasBall && shootingPower > 0)
            {
                SetPlayerAction(ActionType_.Shot, shootingPower);
                shootingPower = 0;
            }

            if (Game.Instance.GameState == GameState_.Replay)
            {
                if (inputSystem.shoot)
                {
                    Game.Instance.Recorder.EndReplay();
                }
            }
        }

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
        Player nextPlayer = Game.Instance.FindNextFieldPLayer(this);
        ((HumanPlayer)nextPlayer).Activate();
        playerFollowCamera.Follow = nextPlayer.PlayerCameraRoot;
    }

}

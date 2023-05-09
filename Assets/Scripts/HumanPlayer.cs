using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HumanPlayer : Player
{
    protected InputSystem inputSystem;
    protected float shootingPower;
    protected CinemachineVirtualCamera playerFollowCamera;

    protected new void Awake()
    {
        base.Awake();

        playerFollowCamera = GameObject.Find("VCam_PlayerFollow").GetComponent<CinemachineVirtualCamera>();
        inputSystem = GetComponent<InputSystem>(); 
    }
    protected new void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected new void Update()
    {
        base.Update();

        if (Game.Instance.GameState != GameState_.Replay)
        {
            if (inputSystem.test)
            {
                inputSystem.test = false;
                Game.Instance.TerminateAllRunningActions();
                Game.Instance.PlayerTakingPenalty = Game.Instance.Teams[1].Players[0];
                Game.Instance.NextGameState = GameState_.Penalty;
                Game.Instance.SetGameState(GameState_.WaitingForWhistle);
                ((HumanPlayer)Game.Instance.Teams[0].GoalKeeper).DoingKick = false;
            }
        }

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
                    Game.Instance.ActivateHumanPlayer(Game.Instance.HumanPlayerDestination);
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
                    if (!(this.GetType() == typeof(HumanGoalkeeper)))
                    {
                        inputSystem.shoot = false;
                        SetPlayerAction(ActionType_.Tackle);
                    }
                }
            }

            if (inputSystem.shoot == false && HasBall && shootingPower > 0)
            {
                SetPlayerAction(ActionType_.Shot, shootingPower);
                shootingPower = 0;
            }
        }

        if (Game.Instance.GameState == GameState_.Replay)
        {
            if (inputSystem.shoot)
            {
                inputSystem.shoot = false;
                Game.Instance.Recorder.EndReplay();
                Game.Instance.Recorder.EndReplay();
            }
        }
    }

}

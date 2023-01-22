using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanFieldPlayer : HumanPlayer
{
    private float shootingPower;

    void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        base.Start();
    }

    void Update()
    {
        base.Update();

        if (DoingThrow && Game.Instance.GameState == GameState_.BringingBallIn)
        {
            if (inputSystem.pass)
            {
                inputSystem.pass = false;
                SetPlayerAction(ActionType_.ThrowinPass);
            }

            if (inputSystem.shoot)
            {
                shootingPower += 1.5f * Time.deltaTime;
                Game.Instance.SetPowerBar(shootingPower);
                if (shootingPower > 1)
                {
                    shootingPower = 1;
                }
            }
            else
            {
                if (shootingPower > 0)
                {
                    SetPlayerAction(ActionType_.ThrowinShot, shootingPower);
                }
                shootingPower = 0;
            }
        }

        if (Game.Instance.GameState == GameState_.Playing ||
            (Game.Instance.GameState == GameState_.BringingBallIn && DoingKick))
        {
            if (HasBall)
            {
                if (inputSystem.pass)
                {
                    inputSystem.pass = false;
                    SetPlayerAction(ActionType_.Pass);
                }

                if (inputSystem.shoot)
                {
                    shootingPower += 1.5f * Time.deltaTime;
                    Game.Instance.SetPowerBar(shootingPower);
                    if (shootingPower > 1)
                    {
                        shootingPower = 1;
                    }
                }
                else
                {
                    if (shootingPower > 0)
                    {
                        SetPlayerAction(ActionType_.Shot, shootingPower);
                    }
                    shootingPower = 0;
                }
            }
            else
            {
                if (inputSystem.pass)
                {
                    inputSystem.pass = false;
                    SwitchActivePlayer();
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
}

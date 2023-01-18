using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanPlayer : MonoBehaviour
{
    Player scriptPlayer;
    private InputSystem inputSystem;
    private float shootingPower;

    void Awake()
    {
        inputSystem = GetComponent<InputSystem>();
        scriptPlayer = GetComponent<Player>();
    }

    void Update()
    {
        if (scriptPlayer.DoingThrow && Game.Instance.GameState == GameState_.BringingBallIn)
        {
            if (inputSystem.pass)
            {
                inputSystem.pass = false;
                scriptPlayer.SetPlayerAction(ActionType_.ThrowinPass);
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
                    scriptPlayer.SetPlayerAction(ActionType_.ThrowinShot, shootingPower);
                }
                shootingPower = 0;
            }
        }

        if (Game.Instance.GameState == GameState_.Playing ||
            (Game.Instance.GameState == GameState_.BringingBallIn && scriptPlayer.DoingKick))
        {
            if (scriptPlayer.HasBall)
            {
                if (inputSystem.pass)
                {
                    inputSystem.pass = false;
                    scriptPlayer.SetPlayerAction(ActionType_.Pass);
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
                        scriptPlayer.SetPlayerAction(ActionType_.Shot, shootingPower);
                    }
                    shootingPower = 0;
                }
            }
            else
            {
                if (inputSystem.pass)
                {
                    inputSystem.pass = false;
                    scriptPlayer.SwitchActivePlayer();
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

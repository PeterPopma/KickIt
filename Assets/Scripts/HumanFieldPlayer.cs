using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanFieldPlayer : HumanPlayer
{
    new void Awake()
    {
        base.Awake();
    }

    new void Start()
    {
        base.Start();
    }

    new void Update()
    {
        base.Update();

        if (inputSystem.test)
        {
            inputSystem.test = false;
            Game.Instance.Teams[1].Players[0].transform.position = new Vector3(16.2700005f, 0.5f, 0.100000001f);
            Game.Instance.Teams[1].Players[0].LookAt(Game.Instance.Goals[0]);
            ScriptBall.transform.position = Game.Instance.Teams[1].Players[0].PlayerBallPosition.position;
            Game.Instance.Teams[1].Players[0].CheckTakeBall();
            Game.Instance.Teams[1].Players[0].SetPlayerAction(ActionType_.Shot, 1f);
        }

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
    }
}

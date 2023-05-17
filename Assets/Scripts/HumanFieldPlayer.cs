using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanFieldPlayer : HumanPlayer
{
    private ThirdPersonController scriptThirdPersonController;
    public ThirdPersonController ScriptThirdPersonController { get => scriptThirdPersonController; set => scriptThirdPersonController = value; }

    
    new void Awake()
    {
        base.Awake();

        scriptThirdPersonController = GetComponent<ThirdPersonController>();
    }

    new void Start()
    {
        base.Start();
    }

    new void Update()
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

        if (Game.Instance.GameState != GameState_.Playing)
        {
            return;
        }
        
        if (Game.Instance.ActiveHumanPlayer != this)
        {
            if (Game.Instance.TeamWithBall != team)
            {
                MoveBehindEnemyPlayer();
            }
            else
            {
                MoveWithAttackingPlayer();
            }
        }

    }
}

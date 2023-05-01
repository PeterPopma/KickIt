using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HumanFieldPlayer : HumanPlayer
{
    private ThirdPersonController scriptThirdPersonController;
    public ThirdPersonController ScriptThirdPersonController { get => scriptThirdPersonController; set => scriptThirdPersonController = value; }

    public void OnJump(InputValue value)
    {
        int pp = 0;
    }
    public void OnShoot(InputValue value)
    {
        int pp = 0;
    }
    
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
    }
}

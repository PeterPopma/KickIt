using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanPlayer : MonoBehaviour
{
    Player scriptPlayer;
    private InputSystem inputSystem;

    void Awake()
    {
        inputSystem = GetComponent<InputSystem>();
        scriptPlayer = GetComponent<Player>();
    }

    void Update()
    {
        if (scriptPlayer.HasBall &&
            (Game.Instance.GameState == GameState_.Playing ||
            (Game.Instance.GameState == GameState_.BringingBallIn && 
            (scriptPlayer.DoingKick || scriptPlayer.DoingThrow))))
        {
            if (inputSystem.pass)
            {
                inputSystem.pass = false;
                scriptPlayer.Pass();
            }

            if (inputSystem.shoot)
            {
                scriptPlayer.ShootingPower += 1.5f * Time.deltaTime;
                Game.Instance.SetPowerBar(scriptPlayer.ShootingPower);
                if (scriptPlayer.ShootingPower > 1)
                {
                    scriptPlayer.ShootingPower = 1;
                }
            }
            else if (scriptPlayer.ShootingPower > 0)
            {
                scriptPlayer.Shoot();
            }
        }
    }
}

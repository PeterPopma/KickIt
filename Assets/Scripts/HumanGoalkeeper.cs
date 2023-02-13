using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanGoalkeeper : HumanPlayer
{

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();

        GetComponent<ThirdPersonController>().IsGoalKeeper = true;
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();

        if (PlayerInput.enabled == false)
        {
            // skip checks if goalkeeper is not active (for performance)
            return;
        }

        if (DoingKick)
        {
            return;
        }

        if (HasBall)
        {
            transformBall.position = PlayerBallPosition.position;
            return;
        }
        else
        {
            if (TakeBallDelay <= 0 && CheckTakeBall(1.2f))
            {
                Team.Stats.ShotsOnGoal++;
                Debug.Log("Goalkeeper took ball");
                playerFollowCamera.Follow = PlayerCameraRoot;
            }
        }

        if (inputSystem.move.x > 0 && inputSystem.move.y <= 0)
        {
            // right block
            SetPlayerAction(ActionType_.GoalKeeperBlockRight);
        }
        else if (inputSystem.move.x < 0 && inputSystem.move.y <= 0)
        {
            // left block
            SetPlayerAction(ActionType_.GoalKeeperBlockLeft);
        }
        else if (inputSystem.move.x == 0 && inputSystem.move.y > 0)
        {
            // jump up
            SetPlayerAction(ActionType_.GoalKeeperJump);
        }
        else if (inputSystem.move.x == 0 && inputSystem.move.y < 0)
        {
            // catch
            SetPlayerAction(ActionType_.GoalKeeperCatch);
        }
        if (inputSystem.diveLeft)
        {
            inputSystem.diveLeft = false;
            SetPlayerAction(ActionType_.GoalKeeperDiveLeft);
        }
        if (inputSystem.diveRight)
        {
            inputSystem.diveRight = false;
            SetPlayerAction(ActionType_.GoalKeeperDiveRight);
        }

    }

}

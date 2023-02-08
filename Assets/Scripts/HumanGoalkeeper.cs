using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanGoalkeeper : HumanPlayer
{

    // Start is called before the first frame update
    new void Start()
    {
        base.Start(); 
        
//        Game.Instance.GoalKeeperCameraTeam0.enabled = true;
//        ((HumanPlayer)Game.Instance.Teams[0].GoalKeeper).Activate();
        
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();

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

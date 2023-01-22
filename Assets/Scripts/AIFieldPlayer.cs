using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIFieldPlayer : AIPlayer
{
    [SerializeField] private float movementSpeed = 4.0f;
    private Vector3 targetGoalPosition;
    private Vector3 ownGoalPosition;
    private Vector3[] attackTargetLocation = new Vector3[2];

    void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        base.Start();

        targetGoalPosition = new Vector3(51.93f, Game.PLAYER_Y_POSITION, 0.24f);
        ownGoalPosition = new Vector3(-52.37f, Game.PLAYER_Y_POSITION, -0.22f);
        attackTargetLocation[0] = new Vector3(38f, Game.PLAYER_Y_POSITION, 10f);
        attackTargetLocation[1] = new Vector3(38f, Game.PLAYER_Y_POSITION, -10f);
    }

    void Update()
    {
        base.Update();

        return;
        if (Game.Instance.GameState != GameState_.Playing)
        {
            return;
        }

        if (team==null)
        {
            Debug.Log("AI player does not belong to a team!");
        }
        if (Game.Instance.TeamWithBall != team.Number)
        {
            DefendMode();
        }
        else
        {
            AttackMode();
        }
    }

    // players move towards enemy goal, one to z=12, one to z=-12
    private void AttackMode()
    {
        // move to target goal
        float speed = movementSpeed;
        Vector3 movedirection = new Vector3(0.1f, 0, 0);
        float distanceToGoal = 20;
        //        Vector3 movedirection = attackTargetLocation[scriptPlayer.Number] - new Vector3(playerBallPosition.position.x, 0, playerBallPosition.position.z);
        //float distanceToGoal = movedirection.magnitude;
        if (HasBall)
        {
            speed *= Game.HAVING_BALL_SLOWDOWN_FACTOR;
        }
        Vector3 moveSpeed = new Vector3(movedirection.normalized.x * speed * Time.deltaTime, 0, movedirection.normalized.z * speed * Time.deltaTime);
        transform.position += moveSpeed;
        transform.LookAt(targetGoalPosition);
        
        // shoot
        if (HasBall && distanceToGoal < 15)
        {
            speed = 0;
            SetPlayerAction(ActionType_.Shot, 0.7f);
        }

        animator.SetFloat("Speed", speed * 2);
    }

    // player closest to ball tries to steal it
    // player farthest from ball moves between goal and player closest to goal
    private void DefendMode()
    {
        if (Game.Instance.PlayerClosestToBall(1) == this)
        {
            MoveToBall();
        }
        else
        {
            MoveToBetweenGoalAndPlayerClosestToGoal();
        }
    }

    private void MoveToBetweenGoalAndPlayerClosestToGoal()
    {
        Vector3 mostDangerousEnemyPlayer = Game.Instance.PlayerClosestToLocation(0, ownGoalPosition).transform.position;
        Vector3 targetLocation = Vector3.Lerp(ownGoalPosition, mostDangerousEnemyPlayer, 0.5f);
        Vector3 movedirection = targetLocation - playerBallPosition.position;
        Vector3 moveSpeed = new Vector3(movedirection.normalized.x * movementSpeed * Time.deltaTime, 0, movedirection.normalized.z * movementSpeed * Time.deltaTime);
        transform.position += moveSpeed;

        if (moveSpeed.magnitude > 0.005)
        {
            transform.LookAt(targetLocation);
        }
        else
        {
            transform.LookAt(mostDangerousEnemyPlayer);
        }

        animator.SetFloat("Speed", movementSpeed * 2);
    }

    private void MoveToBall()
    {
        Vector3 lookAtPosition = transformBall.position;
        lookAtPosition.y = transform.position.y;
        transform.LookAt(lookAtPosition);
        Vector3 movedirection = transformBall.position - playerBallPosition.position;
        Vector3 moveSpeed = new Vector3(movedirection.normalized.x * movementSpeed * Time.deltaTime, 0, movedirection.normalized.z * movementSpeed * Time.deltaTime);
        transform.position += moveSpeed;
        animator.SetFloat("Speed", movementSpeed * 2);
    }
}

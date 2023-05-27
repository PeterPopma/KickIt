using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AIFieldPlayer : AIPlayer
{
    private Vector2 targetGoalPosition;
    private Vector2 ownGoalPosition;
    private const float DELAY_AFTER_ACTION = 8.0f;
    private float delayAfterAction;

    new void Awake()
    {
        base.Awake();
    }

    new void Start()
    {
        base.Start();

        targetGoalPosition = new Vector2(51.93f, 0.24f);
        ownGoalPosition = new Vector2(-52.37f, -0.22f);
    }

    new void Update()
    {
        base.Update();

        if (delayAfterAction > 0)
        {
            delayAfterAction -= Time.deltaTime;
            return;
        }

        if (PlayerAction.Running && PlayerAction.ActionType.Equals(ActionType_.Fall))
        {
            // immobilyze player during fall
            return;
        }

        if (Game.Instance.GameState != GameState_.Playing)
        {
            return;
        }

        if (team==null)
        {
            Utilities.Log("AI player does not belong to a team!", Utilities.DEBUG_TOPIC_UNEXPECTED_SITUATIONS);
        }

        if (Game.Instance.TeamWithBall != team)
        {
            MakeDefensiveMovement();
        }
        else
        {
            MakeAttackingMovement();
        }
    }

    private void MakeAttackingMovement()
    {
        if (HasBall)
        {
            if (Game.Instance.GameState != GameState_.BringingBallIn)
            {
//                MainPlayerAttackMovement();
            }
        }
        else
        {
            MoveWithAttackingPlayer();
        }
    }

    private void MainPlayerAttackMovement()
    {
        Vector3 attackTargetLocation = new Vector3(33f - 66f * team.PlayingSide, Game.PLAYER_Y_POSITION, transform.position.z);
        Vector3 movedirection = attackTargetLocation - new Vector3(playerBallPosition.position.x, 0, playerBallPosition.position.z);
        float distanceToTarget = movedirection.magnitude;

        if (distanceToTarget > 1)
        {
            // move to target location
            float speed = NORMAL_MOVEMENT_SPEED;

            speed *= Game.HAVING_BALL_SLOWDOWN_FACTOR;
            Vector3 moveSpeed = new Vector3(movedirection.normalized.x * speed * Time.deltaTime, 0, movedirection.normalized.z * speed * Time.deltaTime);
            transform.position += moveSpeed;
            transform.LookAt(targetGoalPosition);
            animator.SetFloat("Speed", speed * 2);
        }
        else if (distanceToTarget < 2)
        {
            // shoot
            SetPlayerAction(ActionType_.Shot, (Random.value * 0.4f) + 0.5f);
            delayAfterAction = DELAY_AFTER_ACTION;
            animator.SetFloat("Speed", 0);
        }
    }

    // player closest to ball tries to steal it
    // player farthest from ball moves between goal and player closest to goal
    private void MakeDefensiveMovement()
    {
        if (Game.Instance.FieldPlayerClosestToBall(1) == this)
        {
            MoveToBall();
        }
        else
        {
            MoveBehindEnemyPlayer();
        }
    }

    private void MoveToBetweenGoalAndPlayerClosestToGoal()
    {
        Vector3 mostDangerousEnemyPlayer = Game.Instance.PlayerClosestToLocation(0, ownGoalPosition).transform.position;
        Vector3 targetLocation = Vector3.Lerp(ownGoalPosition, mostDangerousEnemyPlayer, 0.5f);
        Vector3 movedirection = targetLocation - playerBallPosition.position;
        Vector3 moveSpeed = new Vector3(movedirection.normalized.x * NORMAL_MOVEMENT_SPEED * Time.deltaTime, 0, movedirection.normalized.z * NORMAL_MOVEMENT_SPEED * Time.deltaTime);
        transform.position += moveSpeed;

        if (moveSpeed.magnitude > 0.005)
        {
            transform.LookAt(targetLocation);
        }
        else
        {
            transform.LookAt(mostDangerousEnemyPlayer);
        }

        animator.SetFloat("Speed", NORMAL_MOVEMENT_SPEED * 2);
    }

    private void MoveToBall()
    {
        return;
        Vector3 lookAtPosition = new Vector3(transformBall.position.x, transform.position.y, transformBall.position.z);
        transform.LookAt(lookAtPosition);
        Vector3 movedirection = transformBall.position - playerBallPosition.position;
        Vector3 moveSpeed = new Vector3(movedirection.normalized.x * NORMAL_MOVEMENT_SPEED * Time.deltaTime, 0, movedirection.normalized.z * NORMAL_MOVEMENT_SPEED * Time.deltaTime);
        transform.position += moveSpeed;
        animator.SetFloat("Speed", NORMAL_MOVEMENT_SPEED * 2);
    }

    public void PrepareForPenalty()
    {
        Utilities.Log("prepare for penalty", Utilities.DEBUG_TOPIC_PLAYER_EVENTS);
        animator.SetFloat("Speed", 0);
//        transform.position = new Vector3(41.0040016f, 0.5f, -0.0216503143f);
        transform.position = new Vector3(30f/* + Random.value * 16.0f*/, 0.5f, -0.0216503143f);
        transform.LookAt(Game.Instance.Goals[0]);
        ScriptBall.transform.position = PlayerBallPosition.position;
        CheckTakeBall();
        if (!HasBall)
        {
            Utilities.Log("Expect to have ball!  ball pos.:" + ScriptBall.transform.position + " player pos.:" + Game.Instance.Teams[1].Players[0].transform.position,  Utilities.DEBUG_TOPIC_UNEXPECTED_SITUATIONS);
        }
        Game.Instance.ActivateHumanPlayer((HumanPlayer)Game.Instance.Teams[0].GoalKeeper);
        Game.Instance.GoalKeeperCameraTeam0.enabled = true;
        Game.Instance.Teams[0].GoalKeeper.transform.position = Game.Instance.SpawnPositionGoalkeeper(Game.Instance.Teams[0]);
    }


    public void TakePenalty()
    {
        float yRotation = Random.value * 35f + 70f;
        Game.Instance.Teams[1].Players[0].transform.rotation = Quaternion.Euler(0, 95f, 0);
        Game.Instance.Teams[1].Players[0].SetPlayerAction(ActionType_.Shot, 0.7f);
//        Game.Instance.Teams[1].Players[0].SetPlayerAction(ActionType_.Shot, (Random.value * 0.5f) + 0.6f);
    }
}

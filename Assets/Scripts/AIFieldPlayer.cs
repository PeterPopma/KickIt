using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIFieldPlayer : AIPlayer
{
    [SerializeField] private float movementSpeed = 4.0f;
    private Vector3 targetGoalPosition;
    private Vector3 ownGoalPosition;
    private const float DELAY_AFTER_ACTION = 8.0f;
    private float delayAfterAction;

    new void Awake()
    {
        base.Awake();
    }

    new void Start()
    {
        base.Start();

        targetGoalPosition = new Vector3(51.93f, Game.PLAYER_Y_POSITION, 0.24f);
        ownGoalPosition = new Vector3(-52.37f, Game.PLAYER_Y_POSITION, -0.22f);
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
            Debug.Log("AI player does not belong to a team!");
        }
        if (Game.Instance.TeamWithBall != team)
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
        Vector3 attackTargetLocation = new Vector3(38f, Game.PLAYER_Y_POSITION, transform.position.z<0 ? -10f : 10f);
        Vector3 movedirection = attackTargetLocation - new Vector3(playerBallPosition.position.x, 0, playerBallPosition.position.z);
        float distanceToGoal = movedirection.magnitude;
        if (HasBall)
        {
            speed *= Game.HAVING_BALL_SLOWDOWN_FACTOR;
        }

        if (distanceToGoal > 10)
        {
            Vector3 moveSpeed = new Vector3(movedirection.normalized.x * speed * Time.deltaTime, 0, movedirection.normalized.z * speed * Time.deltaTime);
            transform.position += moveSpeed;
            transform.LookAt(targetGoalPosition);
        }

        // shoot
        if (HasBall && distanceToGoal < 15)
        {
            speed = 0;
            SetPlayerAction(ActionType_.Shot, (Random.value * 0.4f) + 0.5f);
            delayAfterAction = DELAY_AFTER_ACTION;
        }

        animator.SetFloat("Speed", speed * 2);
    }

    // player closest to ball tries to steal it
    // player farthest from ball moves between goal and player closest to goal
    private void DefendMode()
    {
        if (Game.Instance.FieldPlayerClosestToBall(1) == this)
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

    public void PrepareForPenalty()
    {
        Debug.Log("prepare for penalty");
        animator.SetFloat("Speed", 0);
        transform.position = new Vector3(41.0040016f, 0.5f, -0.0216503143f);
        transform.LookAt(Game.Instance.Goals[0]);
        ScriptBall.transform.position = PlayerBallPosition.position;
        CheckTakeBall();
        if (!HasBall)
        {
            Debug.Log("Expect to have ball!  ball pos.:" + ScriptBall.transform.position + " player pos.:" + Game.Instance.Teams[1].Players[0].transform.position);
        }
        ((HumanPlayer)Game.Instance.Teams[0].GoalKeeper).Activate();
        Game.Instance.GoalKeeperCameraTeam0.enabled = true;
        Game.Instance.Teams[0].GoalKeeper.transform.position = Game.Instance.SpawnPositionGoalkeeperRed.transform.position;
    }


    public void TakePenalty()
    {
        float yRotation = Random.value * 35f + 70f;
        Debug.Log("take penalty at direction: " + yRotation);
        Game.Instance.Teams[1].Players[0].transform.rotation = Quaternion.Euler(0, yRotation, 0);
        Game.Instance.Teams[1].Players[0].SetPlayerAction(ActionType_.Shot, (Random.value * 0.5f) + 0.6f);
    }
}

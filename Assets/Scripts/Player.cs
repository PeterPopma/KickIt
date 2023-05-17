using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    protected Team team;
    protected Animator animator;

    public const float NORMAL_MOVEMENT_SPEED = 8f; 
    public const float MAX_ACTION_DURATION = 8.0f;
    [SerializeField] private Transform ballHandPosition;
    private int number;
    protected Ball scriptBall;
    private Rigidbody rigidbodyBall;
    protected Transform transformBall;
    protected Transform playerBallPosition;
    private Transform playerCameraRoot;
    private AudioSource soundDribble;
    private AudioSource soundShoot;
    private AudioSource soundSteal;
    private AudioSource soundSlide;
    private AudioSource soundOuch;
    private PlayerAction playerAction;
    private float distanceSinceLastDribble;
    private float takeBallDelay;       // after the player has lost the ball, he cannot steal it back for some time
    private float timePlayerActionRequested;
    private bool hasBall;
    private bool doingThrow;
    private bool doingKick;
    private bool movementDisabled;
    private Vector2 spawnPosition;
    private float speed, targetSpeed;
    Quaternion targetRotation;

    public bool HasBall { get => hasBall; set => hasBall = value; }
    public Transform PlayerBallPosition { get => playerBallPosition; set => playerBallPosition = value; }
    public Transform PlayerCameraRoot { get => playerCameraRoot; set => playerCameraRoot = value; }
    public float TimePlayerActionRequested { get => timePlayerActionRequested; set => timePlayerActionRequested = value; }
    public bool DoingThrow { get => doingThrow; set => doingThrow = value; }
    public bool DoingKick { get => doingKick; set => doingKick = value; }
    public Animator Animator { get => animator; set => animator = value; }
    public Transform BallHandPosition { get => ballHandPosition; set => ballHandPosition = value; }
    public Transform TransformBall { get => transformBall; set => transformBall = value; }
    public Ball ScriptBall { get => scriptBall; set => scriptBall = value; }
    public Team Team { get => team; set => team = value; }
    public bool MovementDisabled { get => movementDisabled; set => movementDisabled = value; }
    public AudioSource SoundSlide { get => soundSlide; set => soundSlide = value; }
    public PlayerAction PlayerAction { get => playerAction; set => playerAction = value; }
    public float TakeBallDelay { get => takeBallDelay; set => takeBallDelay = value; }
    public int Number { get => number; set => number = value; }
    public Vector2 SpawnPosition { get => spawnPosition; set => spawnPosition = value; }
    public float Speed { get => speed; set => speed = value; }

    protected void Awake()
    {
        animator = transform.Find("Geometry/Root").GetComponent<Animator>();
        transformBall = GameObject.Find("Ball").transform;
        scriptBall = transformBall.GetComponent<Ball>();
        soundDribble = GameObject.Find("Sound/dribble").GetComponent<AudioSource>();
        soundShoot = GameObject.Find("Sound/shoot").GetComponent<AudioSource>();
        soundSteal = GameObject.Find("Sound/takeball").GetComponent<AudioSource>();
        soundSlide = GameObject.Find("Sound/slide").GetComponent<AudioSource>();
        soundOuch = GameObject.Find("Sound/ouch").GetComponent<AudioSource>();
        PlayerBallPosition = transform.Find("BallPosition");
        rigidbodyBall = transformBall.gameObject.GetComponent<Rigidbody>();
        PlayerCameraRoot = transform.Find("PlayerCameraRoot");
        playerAction = new PlayerAction(this, animator);
    }
    protected void Start()
    {
    }

    protected void Update()
    {
        if (this != Game.Instance.ActiveHumanPlayer)
        {
            if (speed > targetSpeed)
            {
                speed -= Time.deltaTime * 40;
                if (speed < 0)
                {
                    speed = 0;
                }
            }
            if (speed < targetSpeed)
            {
                speed += Time.deltaTime * 40;
            }

            animator.SetFloat("Speed", speed);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * 300f);
        }
        
        if (!Game.Instance.GameState.Equals(GameState_.Replay))
        {
            playerAction.Update();
        }

        if (Game.Instance.GameState.Equals(GameState_.Playing))
        {
            UpdateGame();
        }

        if (Game.Instance.GameState.Equals(GameState_.BringingBallIn))
        {
            if (doingKick)
            {
                if (Time.time - timePlayerActionRequested > MAX_ACTION_DURATION && !playerAction.Running)
                {
                    Utilities.Log("timeout kick", Utilities.DEBUG_TOPIC_MATCH_EVENTS);
                    // Player did not respond, so execute kick in for him
                    SetPlayerAction(ActionType_.Shot, 0.2f);
                }
            }

            if (doingThrow)
            {
                animator.Play("ThrowIn", PlayerAnimation.LAYER_THROW_IN, 0.4f);
                animator.SetLayerWeight(PlayerAnimation.LAYER_THROW_IN, 1);
                scriptBall.transform.position = BallHandPosition.position;
                if (Time.time - timePlayerActionRequested > MAX_ACTION_DURATION && !playerAction.Running)
                {
                    Utilities.Log("timeout throw", Utilities.DEBUG_TOPIC_MATCH_EVENTS);
                    // Player did not respond, so execute throw in for him
                    SetPlayerAction(ActionType_.ThrowinShot, 0.2f);
                }
            }
        }
    }


    void UpdateGame()
    {
        if (takeBallDelay > 0)
        {
            takeBallDelay -= Time.deltaTime;
        }

        if (HasBall)
        {
            DribbleWithBall();
        }
        else if (Game.Instance.PlayerReceivingPass == null && takeBallDelay <= 0)
        {
            CheckTakeBall();
        }
    }

    // Special lookAt function that also moves the camera behind the player
    public void LookAt(Transform transformLookAt)
    {
        transform.LookAt(transformLookAt);
        if (GetComponent<ThirdPersonController>() != null)
        {
            GetComponent<ThirdPersonController>().LookAt(transform);
        }
    }

    public void TakeBall()
    {
        Utilities.Log("player taking ball: " + name, Utilities.DEBUG_TOPIC_PLAYER_EVENTS);

        // this makes the ball stop rotating from physics.
        scriptBall.GetComponent<Rigidbody>().isKinematic = true;
        scriptBall.GetComponent<Rigidbody>().isKinematic = false;
        if (Game.Instance.PlayerWithBall != null)
        {
            takeBallDelay = 2.0f;
            soundSteal.Play();
            if (Game.Instance.PlayerWithBall != this)
            {
                Game.Instance.PlayerWithBall.LooseBall();
            }
        }
        Game.Instance.SetPlayerWithBall(this);
    }

    public void CheckTakeBall()
    {
        float distanceToBall = Vector3.Distance(transformBall.position, PlayerBallPosition.position);
        if (distanceToBall < 0.6f)
        {
            TakeBall();
        }
    }

    public void TacklePlayers()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 2.0f);

        foreach (Collider collider in colliders)
        {
            Player player = collider.gameObject.GetComponent<HumanFieldPlayer>();
            if (player == null)
            {
                player = collider.gameObject.GetComponent<AIFieldPlayer>();
            }
            if (player != null && player != this)
            {
                soundOuch.Play();
                player.SetPlayerAction(ActionType_.Fall);
                if (player.Speed == 0)
                {
                    Game.Instance.GiveYellowCard(this);
                }
            }
        }
    }

    private void DribbleWithBall()
    {
        transformBall.position = PlayerBallPosition.position;
        distanceSinceLastDribble += scriptBall.Speed.magnitude * Time.deltaTime;
        if (distanceSinceLastDribble > 3)
        {
            soundDribble.Play();
            distanceSinceLastDribble = 0;
        }
    }

    public void LooseBall()
    {
        takeBallDelay = 2.0f;
        Utilities.Log("loose ball :" + name, Utilities.DEBUG_TOPIC_PLAYER_EVENTS);
        HasBall = false;
        Game.Instance.RemovePowerBar();
    }


    public void SetPlayerAction(ActionType_ actionType, float power = 0, bool replaceRunningAction = false)
    {
        // Any running action must be finished before you can order a new action
        if (!playerAction.Running || replaceRunningAction)
        {
            playerAction.StartAction(actionType, power);
        }
        else
        {
            Utilities.Log("action " + playerAction.ActionType.ToString() + " still running! animation layer weight: " +
                animator.GetLayerWeight(playerAction.PlayerAnimation.Layer) + " new action: " + actionType.ToString(), Utilities.DEBUG_TOPIC_PLAYERACTION);
        }
    }

    public void TakeShot(float shootingPower)
    {
        soundShoot.Play();
        takeBallDelay = 0.2f;
        Game.Instance.SetPlayerWithBall(null);
        LooseBall();
        Vector3 shootdirection = transform.forward;
        shootdirection.y += 0.3f;
        //Debug.DrawLine(transform.position, transform.position + shootdirection * 10f, Color.white, 2.5f);
        if (team.Number==0 && transform.position.x<-20)
        {
            if (shootdirection.x < 0)     // shooting in direction of opponent goal
            {
                team.Stats.Shots++;
                Utilities.Log("shots team 0 increased", Utilities.DEBUG_TOPIC_MATCH_EVENTS);
            }
        }
        if (team.Number == 1 && transform.position.x > 20)
        {
            if (shootdirection.x > 0)     // shooting in direction of opponent goal
            {
                team.Stats.Shots++;
                Utilities.Log("shots team 1 increased", Utilities.DEBUG_TOPIC_MATCH_EVENTS);
            }
        }
        Utilities.Log("shoot player " + name, Utilities.DEBUG_TOPIC_PLAYER_EVENTS);
        rigidbodyBall.AddForce(shootdirection * (5 + shootingPower * 25f), ForceMode.VelocityChange);
    }

    public void PassBallToPlayer()
    {
        soundShoot.Play();
        LooseBall();
        if (Team.IsHuman)
        {
            Game.Instance.PlayerReceivingPass = Game.Instance.HumanPlayerDestination;
            transform.LookAt(Game.Instance.PlayerReceivingPass.transform.position);
        }
        else
        {
            Game.Instance.PlayerReceivingPass = Game.Instance.AIPlayerDestination;
        }
        scriptBall.ExecutePass(this);
    }

    public void SetPosition(Vector3 position)
    {
        GetComponent<CharacterController>().enabled = false;
        transform.position = position;
        GetComponent<CharacterController>().enabled = true;
    }

    //  moves the player to the best defensive position: between enemy player and own goal
    protected void MoveBehindEnemyPlayer()
    {
        Vector3 enemyPosition = Game.Instance.OtherTeam(team).Players[Number].transform.position + new Vector3(2 - 4 * team.PlayingSide, 0, 0);
        if ((enemyPosition - transform.position).magnitude < 1)
        {
            // already close enough, so stop moving
            targetSpeed = 0;
            return;
        }
        Vector3 moveDirection = new Vector3(enemyPosition.x, 0, enemyPosition.z).normalized;
        targetRotation = Quaternion.LookRotation(moveDirection); 
        transform.position += moveDirection * NORMAL_MOVEMENT_SPEED * Time.deltaTime;
        targetSpeed = NORMAL_MOVEMENT_SPEED * 2;
    }

    // Move along on the x-axis with the player who has the ball, but on the same relative position as the formation
    protected void MoveWithAttackingPlayer()
    {
        Vector2 targetPosition = new Vector2(spawnPosition.x + Game.Instance.PlayerWithBall.GetDeltaXFromSpawnPosition(), spawnPosition.y);
        Vector2 movedirection = targetPosition - new Vector2(playerBallPosition.position.x, playerBallPosition.position.z);
        float distanceToTarget = movedirection.magnitude;

        if (distanceToTarget > 1)
        {
            Vector3 moveSpeed = new Vector3(movedirection.normalized.x * NORMAL_MOVEMENT_SPEED * Time.deltaTime, 0, movedirection.normalized.y * NORMAL_MOVEMENT_SPEED * Time.deltaTime);
            targetRotation = Quaternion.LookRotation(new Vector3(movedirection.x, 0, movedirection.y));
            transform.position += moveSpeed;
            targetSpeed = NORMAL_MOVEMENT_SPEED * 2;
        }
        else
        {
            targetSpeed =  0;
        }
    }

    public float GetDeltaXFromSpawnPosition()
    { 
        return transform.position.x - spawnPosition.x; 
    }

}

using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public const float MAX_ACTION_DURATION = 8.0f;

    [SerializeField] private Transform ballHandPosition;
    private ThirdPersonController scriptThirdPersonController;
    private AIPlayer scriptAIPlayer;
    private CinemachineVirtualCamera playerFollowCamera;
    private Team team;
    private int number;
    private Player fellowPlayer;
    private Ball scriptBall;
    private Rigidbody rigidbodyBall;
    private PlayerInput playerInput;
    private Transform transformBall;
    private Transform playerBallPosition;
    private Transform playerCameraRoot;
    private Vector3 initialPosition;
    private AudioSource soundDribble;
    private AudioSource soundShoot;
    private AudioSource soundSteal;
    private PlayerAction playerAction;
    private Animator animator;
    private float distanceSinceLastDribble;
    private float takeBallDelay;       // after the player has lost the ball, he cannot steal it back for some time
    private float timePlayerActionRequested;
    private bool hasBall;
    private bool isHuman;
    private bool doingThrow;
    private bool doingKick;

    public bool HasBall { get => hasBall; set => hasBall = value; }
    public Transform PlayerBallPosition { get => playerBallPosition; set => playerBallPosition = value; }
    public Transform PlayerCameraRoot { get => playerCameraRoot; set => playerCameraRoot = value; }
    public Team Team { get => team; set => team = value; }
    public Player FellowPlayer { get => fellowPlayer; set => fellowPlayer = value; }
    public Vector3 InitialPosition { get => initialPosition; set => initialPosition = value; }
    public int Number { get => number; set => number = value; }
    public PlayerInput PlayerInput { get => playerInput; set => playerInput = value; }
    public float TimePlayerActionRequested { get => timePlayerActionRequested; set => timePlayerActionRequested = value; }
    public ThirdPersonController ScriptThirdPersonController { get => scriptThirdPersonController; set => scriptThirdPersonController = value; }
    public bool IsHuman { get => isHuman; set => isHuman = value; }
    public bool DoingThrow { get => doingThrow; set => doingThrow = value; }
    public bool DoingKick { get => doingKick; set => doingKick = value; }
    public Animator Animator { get => animator; set => animator = value; }
    public Transform BallHandPosition { get => ballHandPosition; set => ballHandPosition = value; }
    public Transform TransformBall { get => transformBall; set => transformBall = value; }
    public Ball ScriptBall { get => scriptBall; set => scriptBall = value; }

    void Awake()
    {
        playerFollowCamera = GameObject.Find("PlayerFollowCamera").GetComponent<CinemachineVirtualCamera>();
        transformBall = GameObject.Find("Ball").transform;
        scriptBall = transformBall.GetComponent<Ball>();
        soundDribble = GameObject.Find("Sound/dribble").GetComponent<AudioSource>();
        soundShoot = GameObject.Find("Sound/shoot").GetComponent<AudioSource>();
        soundSteal = GameObject.Find("Sound/woosh").GetComponent<AudioSource>();
        PlayerBallPosition = transform.Find("BallPosition");
        rigidbodyBall = transformBall.gameObject.GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        PlayerCameraRoot = transform.Find("PlayerCameraRoot");
        initialPosition = transform.position;
        scriptThirdPersonController = GetComponent<ThirdPersonController>();
        scriptAIPlayer = GetComponent<AIPlayer>();
        animator = GetComponent<Animator>();
        playerAction = new PlayerAction(this, animator);
    }

    public float Speed()
    {
        if (isHuman)
        {
            return scriptThirdPersonController.AnimationBlend;
        }
        else
        {
            return scriptAIPlayer.Speed;
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
        else if (Game.Instance.PassDestinationPlayer != fellowPlayer && takeBallDelay <= 0)
        {
            CheckTakeBall();
        }
    }

    void Update()
    {
        if (!Game.Instance.GameState.Equals(GameState_.Replay))
        {
            playerAction.Update();
        }

        if (Game.Instance.GameState.Equals(GameState_.Playing))
        {
            UpdateGame();
        }

        if (doingKick)
        {
            transformBall.parent = transform;
            transformBall.position = PlayerBallPosition.position; 
            if (Time.time - timePlayerActionRequested > MAX_ACTION_DURATION && !playerAction.Running)
            {
                // Player did not respond, so execute kick in for him
                SetPlayerAction(ActionType_.Shot, 0.1f);
            }
        }

        if (doingThrow)
        {
            animator.Play("ThrowIn", PlayerAnimation.LAYER_THROW_IN, 0.4f);
            animator.SetLayerWeight(PlayerAnimation.LAYER_THROW_IN, 1);
            // note: we set the position on every frame, because the rigidbody physics is on.
            transformBall.position = BallHandPosition.position; 
            transformBall.parent = transform;
            if (Time.time - timePlayerActionRequested > MAX_ACTION_DURATION && !playerAction.Running)
            {
                // Player did not respond, so execute throw in for him
                SetPlayerAction(ActionType_.ThrowinShot, 0.1f);
            }
        }
    }

    private void CheckTakeBall()
    {
        float distanceToBall = Vector3.Distance(transformBall.position, PlayerBallPosition.position);
        if (distanceToBall < 0.6)
        {
            // this makes the ball stop rotating from physics.
            scriptBall.GetComponent<Rigidbody>().isKinematic = true;
            scriptBall.GetComponent<Rigidbody>().isKinematic = false;
            if (Game.Instance.PlayerWithBall != null)
            {
                takeBallDelay = 2.0f;
                soundSteal.Play();
                Game.Instance.PlayerWithBall.LooseBall();
            }
            Game.Instance.SetPlayerWithBall(this);
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
        HasBall = false;
        Game.Instance.RemovePowerBar();
    }

    // Special lookAt function that also moves the camera behind the player
    public void LookAt(Transform transformLookAt)
    {
        transform.LookAt(transformLookAt);
        if (scriptThirdPersonController != null)
        {
            scriptThirdPersonController.LookAt(transform);
        }
    }

    public void SetPlayerAction(ActionType_ actionType, float power = 0) {
        // Any running action must be finished before you can order a new action
        if (!playerAction.Running)
        {
            playerAction.Set(actionType, power);
        }
        else
        {
            Debug.Log("action still running!");
        }
    }

    public void TakeShot(float shootingPower)
    {
        soundShoot.Play();
        takeBallDelay = 0.2f;
        Game.Instance.SetPlayerWithBall(null);
        Vector3 shootdirection = transform.forward;
        shootdirection.y += 0.3f;
        Debug.DrawLine(transform.position, transform.position + shootdirection * 10f, Color.white, 2.5f);
        rigidbodyBall.AddForce(shootdirection * (10 + shootingPower * 25f), ForceMode.VelocityChange);
 //       rigidbodyBall.AddForce(new Vector3(0,0.3f,0)*20f, ForceMode.VelocityChange);
        LooseBall();
    }

    public void TakePass()
    {
        soundShoot.Play();
        transform.LookAt(fellowPlayer.transform.position);
        LooseBall();
        scriptBall.Pass(this);
    }

    public void Activate()
    {
        Game.Instance.ActiveHumanPlayer = this;
        playerInput.enabled = true;
    }

    public void SwitchActivePlayer()
    {
        PlayerInput.enabled = false;
        fellowPlayer.Activate();
        playerFollowCamera.Follow = fellowPlayer.PlayerCameraRoot;
    }
    public void SetPosition(Vector3 position)
    {
        if (Team.IsHuman)
        {
            GetComponent<CharacterController>().enabled = false;
            transform.position = position;
            GetComponent<CharacterController>().enabled = true;
        }
        else
        {
            transform.position = position;
        }
    }
}

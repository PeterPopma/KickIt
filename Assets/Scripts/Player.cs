using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public const float KICK_DURATION = 0.5f;
    public const float THROWIN_DURATION = 3.0f;
    public const float MAX_ACTION_DURATION = 8.0f;

    [SerializeField] private Transform ballHandPosition;
    private ThirdPersonController scriptThirdPersonController;
    private AIPlayer scriptAIPlayer;
    private Team team;
    int number;
    private Player fellowPlayer;
    private Ball scriptBall;
    private Rigidbody rigidbodyBall;
    private PlayerInput playerInput;
    private Transform transformBall;
    private Transform playerBallPosition;
    private Transform playerCameraRoot;
    private Vector3 initialPosition;
    private Animator animator;
    private AudioSource soundDribble;
    private AudioSource soundShoot;
    private AudioSource soundSteal;
    private Animation animationPlaying;
    private float distanceSinceLastDribble;
    private float takeBallDelay;       // after the player has lost the ball, he cannot steal it back for some time
    private float timePlayerActionRequested;
    private float timeLeftAnimation;
    private float shootingPower;
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
    public float ShootingPower { get => shootingPower; set => shootingPower = value; }
    public float TimePlayerActionRequested { get => timePlayerActionRequested; set => timePlayerActionRequested = value; }
    public Animator Animator { get => animator; set => animator = value; }
    public ThirdPersonController ScriptThirdPersonController { get => scriptThirdPersonController; set => scriptThirdPersonController = value; }
    public bool IsHuman { get => isHuman; set => isHuman = value; }
    public bool DoingThrow { get => doingThrow; set => doingThrow = value; }
    public bool DoingKick { get => doingKick; set => doingKick = value; }

    void Awake()
    {
        transformBall = GameObject.Find("Ball").transform;
        scriptBall = transformBall.GetComponent<Ball>();
        soundDribble = GameObject.Find("Sound/dribble").GetComponent<AudioSource>();
        soundShoot = GameObject.Find("Sound/shoot").GetComponent<AudioSource>();
        soundSteal = GameObject.Find("Sound/woosh").GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        PlayerBallPosition = transform.Find("BallPosition");
        rigidbodyBall = transformBall.gameObject.GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        PlayerCameraRoot = transform.Find("PlayerCameraRoot");
        initialPosition = transform.position;
        scriptThirdPersonController = GetComponent<ThirdPersonController>();
        scriptAIPlayer = GetComponent<AIPlayer>();
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
    
    private void HandleAnimations()
    {
        if (animationPlaying != null)
        {
            if (animationPlaying.FadeIn)
            {
                animator.SetLayerWeight(animationPlaying.Layer, Mathf.Lerp(animator.GetLayerWeight(animationPlaying.Layer), 1f, Time.deltaTime * 10f));
            }

            if (Time.time - animationPlaying.TimeStarted > animationPlaying.Duration)
            {
                if (animationPlaying.FadeOut)
                {
                    animator.SetLayerWeight(animationPlaying.Layer, Mathf.Lerp(animator.GetLayerWeight(animationPlaying.Layer), 0f, Time.deltaTime * 10f));
                }
                else
                {
                    animator.SetLayerWeight(animationPlaying.Layer, 0);
                }

                if (animator.GetLayerWeight(animationPlaying.Layer)==0)
                {
                    animationPlaying = null;
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
        else if (Game.Instance.PassDestinationPlayer != fellowPlayer && takeBallDelay <= 0)
        {
            CheckTakeBall();
        }
    }

    void Update()
    {
        if (!Game.Instance.GameState.Equals(GameState_.Replay))
        {
            HandleAnimations();
        }

        if (Game.Instance.GameState.Equals(GameState_.Playing))
        {
            UpdateGame();
        }

        if (Game.Instance.GameState.Equals(GameState_.BringingBallIn) && (doingKick || doingThrow))
        {
            if (timeLeftAnimation>0)
            {
                timeLeftAnimation -= Time.deltaTime;
                if (timeLeftAnimation <= 0)
                {
                    doingKick = doingThrow = false;
                    Game.Instance.SetGameState(GameState_.Playing);
                    TakeShot();
                }
            }
            else if (Time.time - timePlayerActionRequested > MAX_ACTION_DURATION && timeLeftAnimation <= 0)
            {
                // Player did not respond, so execute kick/throw in for him
                shootingPower = 0.1f;
                Shoot();
            }
        }
    }

    private void CheckTakeBall()
    {
        float distanceToBall = Vector3.Distance(transformBall.position, PlayerBallPosition.position);
        if (distanceToBall < 0.6)
        {
            if (Game.Instance.PlayerWithBall != null)
            {
                soundSteal.Play();
                Game.Instance.PlayerWithBall.LooseBall(true);
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

    public void LooseBall(bool stolen = false)
    {
        if (stolen)
        {
            takeBallDelay = 2.0f;
        }
        HasBall = false;
        shootingPower = 0;
        Game.Instance.RemovePowerBar();
    }

    public void ScoreGoal()
    {
        animationPlaying = new Animation(animator, Game.CHEERING_DURATION, false, true, Animation.LAYER_CHEER, "Cheer");
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

    public void Pass()
    {
        if (Game.Instance.PassDestinationPlayer == null)
        {
            transform.LookAt(fellowPlayer.transform.position);
            soundShoot.Play();
            LooseBall();
            animationPlaying = new Animation(animator, KICK_DURATION, false, true, Animation.LAYER_SHOOT, "Shoot");
            scriptBall.Pass(this);
            if (Game.Instance.GameState.Equals(GameState_.BringingBallIn))
            {
                Game.Instance.SetGameState(GameState_.Playing);
            }
        }
    }

    public void Shoot()
    {
        if (doingKick)
        {
            HasBall = false;
            animationPlaying = new Animation(animator, KICK_DURATION, false, true, Animation.LAYER_SHOOT, "Shoot");
            timeLeftAnimation = KICK_DURATION;
        }
        else if (doingThrow)
        {
            HasBall = false;
            animationPlaying = new Animation(animator, THROWIN_DURATION, false, true, Animation.LAYER_THROW_IN, "ThrowIn");
            timeLeftAnimation = THROWIN_DURATION;
        }
        else
        {
            TakeShot();
        }
    }

    private void TakeShot()
    {
        takeBallDelay = 0.2f;
        soundShoot.Play();
        Game.Instance.SetPlayerWithBall(null);
        Vector3 shootdirection = transform.forward;
        shootdirection.y += 0.3f;
        rigidbodyBall.AddForce(shootdirection * (10 + shootingPower * 25f), ForceMode.VelocityChange);
        LooseBall();
    }

    public void Activate()
    {
        Game.Instance.ActiveHumanPlayer = this;
        playerInput.enabled = true;
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

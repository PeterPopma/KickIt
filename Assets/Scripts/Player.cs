using Cinemachine;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public const float SHOOTING_DURATION = 0.5f;
    public const float THROWIN_DURATION = 3.0f;
    
    [SerializeField] private Transform ballHandPosition;
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
    private bool hasBall;
    private float timeSincePlayerAction;        // time since this player had performed an action (to keep track of animations, etc.)
    private float stealDelay;       // after the player has lost the ball, he cannot steal it back for some time
    private Team team;
    private bool takingFreeKick;
    private bool takingThrowIn;
    private float shootingPower;


    public bool HasBall { get => hasBall; set => hasBall = value; }
    public Transform PlayerBallPosition { get => playerBallPosition; set => playerBallPosition = value; }
    public Transform PlayerCameraRoot { get => playerCameraRoot; set => playerCameraRoot = value; }
    public Team Team { get => team; set => team = value; }
    public Player FellowPlayer { get => fellowPlayer; set => fellowPlayer = value; }
    public Vector3 InitialPosition { get => initialPosition; set => initialPosition = value; }
    public bool TakeFreeKick { get => takingFreeKick; set => takingFreeKick = value; }
    public bool TakeThrowIn { get => takingThrowIn; set => takingThrowIn = value; }
    public int Number { get => number; set => number = value; }
    public PlayerInput PlayerInput { get => playerInput; set => playerInput = value; }
    public float ShootingPower { get => shootingPower; set => shootingPower = value; }

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

    // Update is called once per frame
    void Update()
    {
        HandleAnimations();

        if (stealDelay > 0) {
            stealDelay -= Time.deltaTime;
        }

        if (HasBall)
        {
            DribbleWithBall();
        }
        else if (Game.Instance.PassDestinationPlayer != fellowPlayer && timeSincePlayerAction == 0 && stealDelay <= 0)
        {
            CheckTakeBall();
        }
        if (timeSincePlayerAction > 0)      // ball has been shot or thrown
        {
            if (takingThrowIn)
            {
                // release ball for throw-in
                if (Time.time - timeSincePlayerAction > 10.5)
                {
                    Game.Instance.SetGameState(GameState_.Playing);
                    TakeShot();
                }
                // finished throwing ball
                if (Time.time - timeSincePlayerAction > THROWIN_DURATION)
                {
                    timeSincePlayerAction = 0;
                }
            }
            else
            {
                // shoot ball
                if (HasBall/* && Time.time - timeShot > 0.2*/)
                {
                    TakeShot();
                }
                // finished shooting ball
                if (Time.time - timeSincePlayerAction > SHOOTING_DURATION)
                {
                    timeSincePlayerAction = 0;
                }
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
            stealDelay = 2.0f;
        }
        HasBall = false;
        shootingPower = 0;
        Game.Instance.RemovePowerBar();
    }

    public void ScoreGoal()
    {
        animationPlaying = new Animation(animator, Game.CHEERING_DURATION, false, true, Animation.LAYER_CHEER, "Cheer");
    }

    public void Shoot()
    {
        if (HasBall)
        {
            timeSincePlayerAction = Time.time;
            if (takingThrowIn)
            {
                animationPlaying = new Animation(animator, THROWIN_DURATION, false, true, Animation.LAYER_THROW_IN, "ThrowIn");
            }
            else
            {
                animationPlaying = new Animation(animator, SHOOTING_DURATION, false, true, Animation.LAYER_SHOOT, "Shoot");
            }
        }
    }

    private void TakeShot()
    {
        soundShoot.Play();
        Game.Instance.SetPlayerWithBall(null);
        Vector3 shootdirection = transform.forward;
        shootdirection.y += 0.2f;
        rigidbodyBall.AddForce(shootdirection * (10 + shootingPower * 20f), ForceMode.Impulse);
        LooseBall();
    }

    public void Pass()
    {
        if (HasBall && Game.Instance.PassDestinationPlayer == null)
        {
            transform.LookAt(fellowPlayer.transform.position);
            timeSincePlayerAction = Time.time;
            soundShoot.Play();
            LooseBall();
            animationPlaying = new Animation(animator, SHOOTING_DURATION, false, true, Animation.LAYER_SHOOT, "Shoot");
            scriptBall.Pass(this);
        }
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

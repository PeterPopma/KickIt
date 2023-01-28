using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    protected Team team;
    protected Animator animator;

    public const float MAX_ACTION_DURATION = 8.0f;
    [SerializeField] private Transform ballHandPosition;
    private Ball scriptBall;
    private Rigidbody rigidbodyBall;
    protected Transform transformBall;
    protected Transform playerBallPosition;
    private Transform playerCameraRoot;
    private Vector3 initialPosition;
    private AudioSource soundDribble;
    private AudioSource soundShoot;
    private AudioSource soundSteal;
    private AudioSource soundSlide;
    private AudioSource soundOuch;
    private PlayerAction playerAction;
    private Player nextPlayer;
    private float distanceSinceLastDribble;
    private float takeBallDelay;       // after the player has lost the ball, he cannot steal it back for some time
    private float timePlayerActionRequested;
    private bool hasBall;
    private bool doingThrow;
    private bool doingKick;
    private bool movementDisabled;

    public bool HasBall { get => hasBall; set => hasBall = value; }
    public Transform PlayerBallPosition { get => playerBallPosition; set => playerBallPosition = value; }
    public Transform PlayerCameraRoot { get => playerCameraRoot; set => playerCameraRoot = value; }
    public Vector3 InitialPosition { get => initialPosition; set => initialPosition = value; }
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
    public Player NextPlayer { get => nextPlayer; set => nextPlayer = value; }

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
        initialPosition = transform.position;
        playerAction = new PlayerAction(this, animator);
    }
    protected void Start()
    {
    }

    protected void Update()
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
            if (Time.time - timePlayerActionRequested > MAX_ACTION_DURATION && !playerAction.Running)
            {
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
                // Player did not respond, so execute throw in for him
                SetPlayerAction(ActionType_.ThrowinShot, 0.2f);
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
        else if (Game.Instance.PassDestinationPlayer == null && takeBallDelay <= 0)
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

    public void CheckTakeBall()
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
        HasBall = false;
        Game.Instance.RemovePowerBar();
    }


    public void SetPlayerAction(ActionType_ actionType, float power = 0)
    {
        // Any running action must be finished before you can order a new action
        if (!playerAction.Running)
        {
            playerAction.StartAction(actionType, power);
        }
        else
        {
            Debug.Log("action still running! animation layer weight: " + animator.GetLayerWeight(playerAction.PlayerAnimation.Layer));
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
        rigidbodyBall.AddForce(shootdirection * (5 + shootingPower * 25f), ForceMode.VelocityChange);
        //       rigidbodyBall.AddForce(new Vector3(0,0.3f,0)*20f, ForceMode.VelocityChange);
        LooseBall();
    }

    public void TakePass()
    {
        soundShoot.Play();
        Game.Instance.PassDestinationPlayer = Game.Instance.FindNextFieldPLayer(this);
        transform.LookAt(Game.Instance.PassDestinationPlayer.transform.position);
        LooseBall();
        if (Game.Instance.PassDestinationPlayer is HumanPlayer)
        {
            ((HumanPlayer)this).PlayerInput.enabled = false;
        }
        scriptBall.ExecutePass(this);
    }

    public void SetPosition(Vector3 position)
    {
        GetComponent<CharacterController>().enabled = false;
        transform.position = position;
        GetComponent<CharacterController>().enabled = true;
    }

}

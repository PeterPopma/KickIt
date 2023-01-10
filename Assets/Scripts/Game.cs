using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum GameMode_
{
    PlayerVsPlayer,
    PlayerVsPc,
    TwoPlayersVsPc
}

public enum GameState_
{
    Playing,
    WaitingForWhistle,
    BringingBallIn,
    Cheering,
    Replay,
    MatchOver
}

public class Game : MonoBehaviour
{
    public static Game Instance;

    public const float HAVING_BALL_SLOWDOWN_FACTOR = 0.8f;
    public const float PLAYER_Y_POSITION = 0.5f;
    public const float FIELD_BOUNDARY_LOWER_X = -52.6f;
    public const float FIELD_BOUNDARY_UPPER_X = 52.3f;
    public const float FIELD_BOUNDARY_LOWER_Z = -25f;
    public const float FIELD_BOUNDARY_UPPER_Z = 25f;
    public const float MINIMUM_DISTANCE_FREEKICK = 18f;

    public const float CHEERING_DURATION = 4.0f;
    public const float WAITING_FOR_WHISTLE_DURATION = 2.0f;

    [SerializeField] private Recorder recorder;
    [SerializeField] private GameObject playerSpawnPosition1;
    [SerializeField] private GameObject playerSpawnPosition2;
    [SerializeField] private GameObject playerSpawnPosition3;
    [SerializeField] private GameObject playerSpawnPosition4;
    [SerializeField] private GameObject pfPlayer1;
    [SerializeField] private GameObject pfPlayer2;
    [SerializeField] private GameObject pfPlayer3;
    [SerializeField] private GameObject pfPlayer4;
    [SerializeField] private TextMeshProUGUI textScore;
    [SerializeField] private TextMeshProUGUI textGoal;
    [SerializeField] private TextMeshProUGUI textPlayer;
    [SerializeField] private TextMeshProUGUI textReplay;
    [SerializeField] private Transform[] goals;
    private GameMode_ gameMode;
    private GameState_ gameState;
    private GameState_ nextGameState;
    private Slider sliderPowerBar;
    private GameObject powerBar;
    private Ball scriptBall;
    private AudioSource soundCheer;
    private AudioSource soundWhistle;
    private Player playerLastTouchedBall;
    private Player playerWithBall;
    private Player activeHumanPlayer;
    private Player passDestinationPlayer;
    private CinemachineVirtualCamera playerFollowCamera;
    private int teamWithBall;
    private int teamLastTouched;
    private int teamKickOff;
    private float timeGameStateStarted;
    private float goalTextColorAlpha;
    private List<Team> teams = new();
    private Vector3 kickOffPosition = new Vector3(0, PLAYER_Y_POSITION, 0.1f);
    private Image crosshairAim;
    private int goalOfTeamLastScored;

    public Player PassDestinationPlayer { get => passDestinationPlayer; set => passDestinationPlayer = value; }
    public Player PlayerWithBall { get => playerWithBall; }
    public int TeamWithBall { get => teamWithBall; set => teamWithBall = value; }
    public int TeamLastTouched { get => teamLastTouched; set => teamLastTouched = value; }
    public Player ActiveHumanPlayer { get => activeHumanPlayer; set => activeHumanPlayer = value; }
    public List<Team> Teams { get => teams; set => teams = value; }
    public Vector3 KickOffPosition { get => kickOffPosition; set => kickOffPosition = value; }
    public GameMode_ GameMode { get => gameMode; set => gameMode = value; }
    public GameState_ GameState { get => gameState; set => gameState = value; }
    public GameState_ NextGameState { get => nextGameState; set => nextGameState = value; }
    public Ball ScriptBall { get => scriptBall; set => scriptBall = value; }

    private void InitGamePlayerVsPC()
    {
        Team newTeam = new(0, true);
        teams.Add(newTeam);

        GameObject spawnedPlayer = Instantiate(pfPlayer1, playerSpawnPosition1.transform.position, Quaternion.identity);
        spawnedPlayer.name = "Peter";
        spawnedPlayer.GetComponent<Player>().IsHuman = true;
        spawnedPlayer.GetComponent<Player>().Number = 0;
        spawnedPlayer.GetComponent<Player>().Team = newTeam;
        spawnedPlayer.GetComponent<Player>().Activate();
        newTeam.Players.Add(spawnedPlayer.GetComponent<Player>());
        playerFollowCamera.Follow = spawnedPlayer.transform.Find("PlayerCameraRoot").transform;

        GameObject spawnedPlayer2 = Instantiate(pfPlayer2, playerSpawnPosition2.transform.position, Quaternion.identity);
        spawnedPlayer2.name = "Mark";
        spawnedPlayer2.GetComponent<Player>().IsHuman = true;
        spawnedPlayer2.GetComponent<Player>().Number = 1;
        spawnedPlayer2.GetComponent<Player>().Team = newTeam;
        spawnedPlayer2.GetComponent<PlayerInput>().enabled = false;
        newTeam.Players.Add(spawnedPlayer2.GetComponent<Player>());

        spawnedPlayer.GetComponent<Player>().FellowPlayer = spawnedPlayer2.GetComponent<Player>();
        spawnedPlayer2.GetComponent<Player>().FellowPlayer = spawnedPlayer.GetComponent<Player>();

        newTeam = new Team(1, false);
        teams.Add(newTeam);

        spawnedPlayer = Instantiate(pfPlayer3, playerSpawnPosition3.transform.position, Quaternion.identity);
        spawnedPlayer.name = "Arnoud";
        spawnedPlayer.GetComponent<Player>().IsHuman = false;
        spawnedPlayer.GetComponent<Player>().Number = 0;
        spawnedPlayer.GetComponent<Player>().Team = newTeam;
        newTeam.Players.Add(spawnedPlayer.GetComponent<Player>());

        spawnedPlayer2 = Instantiate(pfPlayer4, playerSpawnPosition4.transform.position, Quaternion.identity);
        spawnedPlayer2.name = "Thirza";
        spawnedPlayer2.GetComponent<Player>().IsHuman = false;
        spawnedPlayer2.GetComponent<Player>().Number = 1;
        spawnedPlayer2.GetComponent<Player>().Team = newTeam;
        newTeam.Players.Add(spawnedPlayer2.GetComponent<Player>());

        spawnedPlayer.GetComponent<Player>().FellowPlayer = spawnedPlayer2.GetComponent<Player>();
        spawnedPlayer2.GetComponent<Player>().FellowPlayer = spawnedPlayer.GetComponent<Player>();
    }

    public void Awake()
    {
        Instance = this;
        crosshairAim = GameObject.Find("Canvas/CrosshairAim").GetComponent<Image>();
        playerFollowCamera = GameObject.Find("PlayerFollowCamera").GetComponent<CinemachineVirtualCamera>();
        soundCheer = GameObject.Find("Sound/cheer").GetComponent<AudioSource>();
        soundWhistle = GameObject.Find("Sound/whistle").GetComponent<AudioSource>();
        scriptBall = GameObject.Find("Ball").GetComponent<Ball>();
        sliderPowerBar = GameObject.Find("Canvas/Panel/PowerBar").GetComponent<Slider>();
        powerBar = GameObject.Find("Canvas/Panel/PowerBar");
        powerBar.SetActive(false);

        if (GlobalParams.GameMode.Equals(GameMode_.PlayerVsPc))
        {
            InitGamePlayerVsPC();
        }
    }
    public void Start()
    {
        KickOff();
    }

    public void ResetPlayersAndBall()
    {
        foreach(Team team in teams)
        {
            foreach(Player player in team.Players)
            {
                player.SetPosition(player.InitialPosition);
                player.LookAt(goals[OtherTeam(player.Team.Number)]);
            }
        }
        // Set player to kick off
        Vector3 position = new(0.5f - teamKickOff, KickOffPosition.y, kickOffPosition.z);
        teams[teamKickOff].Players[0].SetPosition(position);
        SetPlayerWithBall(teams[teamKickOff].Players[0]);
        teams[teamKickOff].Players[0].DoingKick = true;
        scriptBall.BallOutOfFieldTimeOut = 0;
        scriptBall.PutOnCenterSpot();
        scriptBall.Rigidbody.velocity = Vector3.zero;
        scriptBall.Rigidbody.angularVelocity = Vector3.zero;
    }

    public Player PlayerClosestToBall(int teamNumber)
    {
        Player closestPlayer = null;
        float closestDistance = float.MaxValue;
        foreach(Player player in teams[teamNumber].Players)
        {
            float distance = (player.transform.position - scriptBall.transform.position).magnitude;
            if(distance < closestDistance)
            {
                closestPlayer = player;
            }
        }

        return closestPlayer;
    }

    public Player PlayerClosestToLocation(int teamNumber, Vector3 location)
    {
        Player closestPlayer = null;
        float closestDistance = float.MaxValue;
        foreach (Player player in teams[teamNumber].Players)
        {
            float distance = (player.transform.position - location).magnitude;
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player;
            }
        }

        return closestPlayer;
    }

    public void ScoreGoal(int goalOfTeam)
    {
        goalOfTeamLastScored = goalOfTeam;
        int teamScored = goalOfTeam==0 ? 1 : 0;
        teams[teamScored].Score++;
        teamKickOff = OtherTeam(teamScored);
        goalTextColorAlpha = 1;
        soundCheer.Play();
        textScore.text = "Score: " + teams[0].Score + "-" + teams[1].Score;
        playerLastTouchedBall.ScoreGoal();

        SetGameState(GameState_.Cheering);
    }

    public void KickOff()
    {
        ResetPlayersAndBall();
        ActiveHumanPlayer.GetComponent<ThirdPersonController>().MoveSpeed = 0;
        ActiveHumanPlayer.GetComponent<ThirdPersonController>().SprintSpeed = 0;
        nextGameState = GameState_.BringingBallIn;
        SetGameState(GameState_.WaitingForWhistle);
    }

    public void SetGameState(GameState_ newGameState)
    {
        gameState = newGameState;
        timeGameStateStarted = Time.time;

        switch (newGameState)
        {
            case GameState_.WaitingForWhistle:
                break;
            case GameState_.Cheering:
                break;
        }
    }

    public void Update()
    {
        if (gameState == GameState_.Replay && Time.time%1<0.5)
        {
            textReplay.text = GameState_.Replay.ToString();
        }
        else
        {
            textReplay.text = "";
        }

        if (gameState == GameState_.WaitingForWhistle)
        {
            if (Time.time - timeGameStateStarted > WAITING_FOR_WHISTLE_DURATION)
            {
                ActiveHumanPlayer.GetComponent<ThirdPersonController>().MoveSpeed = 8;
                ActiveHumanPlayer.GetComponent<ThirdPersonController>().SprintSpeed = 15;
                soundWhistle.Play();
                SetGameState(nextGameState);
            }
        }

        if (gameState == GameState_.Cheering)
        {
            if (Time.time - timeGameStateStarted > CHEERING_DURATION)
            {
                playerLastTouchedBall.Animator.SetLayerWeight(Animation.LAYER_CHEER, 0);
                SetGameState(GameState_.Replay);
            }
        }

        if (goalTextColorAlpha > 0)
        {
            goalTextColorAlpha -= Time.deltaTime;
            textGoal.alpha = goalTextColorAlpha;
            textGoal.fontSize = 350 - (goalTextColorAlpha * 250);
        }

//        PerformSanityChecks();
    }

    private void PerformSanityChecks()
    {
        Player playerWithInputEnabled = null;

        foreach (Team team in teams)
        {
            foreach (Player player in team.Players)
            {
                if (transform.position.y < 0)
                {
                    DisplayErrorMessage("players cannot be lower than the field");
                }
                if (player.GetComponent<PlayerInput>()!=null && player.GetComponent<PlayerInput>().enabled == true)
                {
                    playerWithInputEnabled = player;
                }
            }
        }

        if (playerWithInputEnabled==null)
        {
            DisplayErrorMessage("there must always be a player with input enabled");
        }

        if (ActiveHumanPlayer!=playerWithInputEnabled)
        {
            DisplayErrorMessage("active player must be player with input enabled");
        }

        if (!playerWithInputEnabled.transform.Find("PlayerCameraRoot").transform.parent.name.Equals(playerFollowCamera.Follow.parent.name))
        {
            DisplayErrorMessage("camera must follow player with input enabled");
        }

        if (scriptBall.transform.position.y < 0 && 
            scriptBall.transform.position.x > FIELD_BOUNDARY_LOWER_X &&
            scriptBall.transform.position.x < FIELD_BOUNDARY_UPPER_X && 
            scriptBall.transform.position.z > FIELD_BOUNDARY_LOWER_Z &&
            scriptBall.transform.position.z < FIELD_BOUNDARY_UPPER_Z)
        {
            DisplayErrorMessage("ball in the field cannot be lower than 0");
        }
    }

    private void DisplayErrorMessage(string message)
    {
        Debug.Log("An error has occurred: " + message);
    }

    private int OtherTeam(int team)
    {
        if (team == 0)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    public void SetPowerBar(float value)
    {
        powerBar.SetActive(true);
        sliderPowerBar.value = value;
        //crosshairAim.enabled = true;
    }

    public void RemovePowerBar()
    {
        powerBar.SetActive(false);
        crosshairAim.enabled = false;
    }

    // Move players that are too close in the direction of the center of the field
    public void SetMinimumDistanceOtherPlayers(Player playerToTakeDistanceFrom)
    {
        foreach (Team team in teams)
        {
            foreach (Player player in team.Players)
            {
                if (player != playerToTakeDistanceFrom)
                {
                    float distance = (player.transform.position-playerToTakeDistanceFrom.transform.position).magnitude;
                    if (distance < 8)
                    {
                        Vector3 moveToDistanceDirection = (Vector3.zero - playerToTakeDistanceFrom.transform.position).normalized * MINIMUM_DISTANCE_FREEKICK;
                        player.SetPosition(new Vector3(player.transform.position.x + moveToDistanceDirection.x,
                                                player.transform.position.y, player.transform.position.z + moveToDistanceDirection.z));
                    }
                }
            }
        }
    }

    public void SetPlayerWithBall(Player player)
    {
        playerWithBall = player;
        PassDestinationPlayer = null;
        if (playerWithBall != null)
        {
            if (player.Team.IsHuman && !player.PlayerInput.enabled)
            {
                playerFollowCamera.Follow = player.PlayerCameraRoot;
                activeHumanPlayer.PlayerInput.enabled = false;
                activeHumanPlayer = player;
                player.PlayerInput.enabled = true;
            }
            scriptBall.PutOnGround();
            player.HasBall = true;
            playerLastTouchedBall = playerWithBall;
            teamLastTouched = teamWithBall = playerWithBall.Team.Number;
            textPlayer.text = playerWithBall.name;
        }
        else
        {
            teamWithBall = -1;
            textPlayer.text = "";
        }
    }

    public Player GetPlayerToThrowIn()
    {
        if (teamLastTouched==0)
        {
            return PlayerClosestToBall(1);
        }
        else
        {
            return PlayerClosestToBall(0);
        }
    }


}

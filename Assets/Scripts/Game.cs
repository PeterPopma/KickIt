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

    public const float DELAY_SCOREGOAL = 0.5f;                  // Wait a little bit after scoring to show at playback
    public const float HAVING_BALL_SLOWDOWN_FACTOR = 0.8f;
    public const float PLAYER_Y_POSITION = 0.5f;
    public const float FIELD_BOUNDARY_LOWER_X = -52.894f;
    public const float FIELD_BOUNDARY_UPPER_X = 52.725f;
    public const float FIELD_BOUNDARY_LOWER_Z = -25.37f;
    public const float FIELD_BOUNDARY_UPPER_Z = 25.552f;
    public const float MINIMUM_DISTANCE_FREEKICK = 18f;

    public const float WAITING_FOR_WHISTLE_DURATION = 2.0f;

    [SerializeField] private Recorder recorder;
    [SerializeField] private GameObject spawnPositionPlayerRed1;
    [SerializeField] private GameObject spawnPositionPlayerRed2;
    [SerializeField] private GameObject spawnPositionGoalkeeperRed;
    [SerializeField] private GameObject spawnPositionPlayerBlue1;
    [SerializeField] private GameObject spawnPositionPlayerBlue2;
    [SerializeField] private GameObject spawnPositionGoalkeeperBlue;
    [SerializeField] private GameObject pfPlayerRed1;
    [SerializeField] private GameObject pfPlayerRed2;
    [SerializeField] private GameObject pfGoalkeeperRed;
    [SerializeField] private GameObject pfPlayerBlue1;
    [SerializeField] private GameObject pfPlayerBlue2;
    [SerializeField] private GameObject pfGoalkeeperBlue;
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
    private HumanPlayer activeHumanPlayer;
    private Player passDestinationPlayer;
    private CinemachineVirtualCamera playerFollowCamera;
    private int teamWithBall;
    private int teamLastTouched;
    private int teamKickOff;
    private float timeGameStateStarted;
    private float goalTextColorAlpha;
    private float delayScoreGoal;
    private List<Team> teams = new();
    private Vector3 kickOffPosition = new Vector3(0, PLAYER_Y_POSITION, 0.1f);
    private Image crosshairAim;
    private int goalOfTeamLastScored;

    public Player PassDestinationPlayer { get => passDestinationPlayer; set => passDestinationPlayer = value; }
    public Player PlayerWithBall { get => playerWithBall; }
    public int TeamWithBall { get => teamWithBall; set => teamWithBall = value; }
    public int TeamLastTouched { get => teamLastTouched; set => teamLastTouched = value; }
    public HumanPlayer ActiveHumanPlayer { get => activeHumanPlayer; set => activeHumanPlayer = value; }
    public List<Team> Teams { get => teams; set => teams = value; }
    public Vector3 KickOffPosition { get => kickOffPosition; set => kickOffPosition = value; }
    public GameMode_ GameMode { get => gameMode; set => gameMode = value; }
    public GameState_ GameState { get => gameState; set => gameState = value; }
    public GameState_ NextGameState { get => nextGameState; set => nextGameState = value; }
    public Ball ScriptBall { get => scriptBall; set => scriptBall = value; }
    public Recorder Recorder { get => recorder; set => recorder = value; }

    private void InitGamePlayerVsPC()
    {
        Team team1 = new(0);
        teams.Add(team1);

        GameObject playerRed1 = Instantiate(pfPlayerRed1, spawnPositionPlayerRed1.transform.position, Quaternion.identity);
        playerRed1.name = "Peter";
        playerRed1.GetComponent<HumanFieldPlayer>().Team = team1;
        playerRed1.GetComponent<HumanFieldPlayer>().Activate();
        team1.Players.Add(playerRed1.GetComponent<HumanFieldPlayer>());
        playerFollowCamera.Follow = playerRed1.transform.Find("PlayerCameraRoot").transform;

        GameObject playerRed2 = Instantiate(pfPlayerRed2, spawnPositionPlayerRed2.transform.position, Quaternion.identity);
        playerRed2.name = "Mark";
        playerRed2.GetComponent<HumanFieldPlayer>().Team = team1;
        playerRed2.GetComponent<PlayerInput>().enabled = false;
        team1.Players.Add(playerRed2.GetComponent<HumanFieldPlayer>());

        playerRed1.GetComponent<HumanFieldPlayer>().FellowPlayer = playerRed2.GetComponent<HumanFieldPlayer>();
        playerRed2.GetComponent<HumanFieldPlayer>().FellowPlayer = playerRed1.GetComponent<HumanFieldPlayer>();

        GameObject goalkeeperRed = Instantiate(pfGoalkeeperRed, spawnPositionGoalkeeperRed.transform.position, spawnPositionGoalkeeperRed.transform.rotation);
        goalkeeperRed.name = "Jaden";
        goalkeeperRed.GetComponent<HumanGoalkeeper>().Team = team1;
        team1.Players.Add(goalkeeperRed.GetComponent<HumanGoalkeeper>());

        Team team2 = new Team(1);
        teams.Add(team2);

        GameObject playerBlue1 = Instantiate(pfPlayerBlue1, spawnPositionPlayerBlue1.transform.position, Quaternion.identity);
        playerBlue1.name = "Arnoud";
        playerBlue1.GetComponent<AIPlayer>().Team = team2;
        team2.Players.Add(playerBlue1.GetComponent<AIPlayer>());

        GameObject playerBlue2 = Instantiate(pfPlayerBlue2, spawnPositionPlayerBlue2.transform.position, Quaternion.identity);
        playerBlue2.name = "Thirza";
        playerBlue2.GetComponent<AIPlayer>().Team = team2;
        team2.Players.Add(playerBlue2.GetComponent<AIPlayer>());

        GameObject goalkeeperBlue = Instantiate(pfGoalkeeperBlue, spawnPositionGoalkeeperBlue.transform.position, spawnPositionGoalkeeperBlue.transform.rotation);
        goalkeeperBlue.name = "Maaike";
        goalkeeperBlue.GetComponent<AIGoalkeeper>().Team = team2;
        team2.Players.Add(goalkeeperBlue.GetComponent<AIGoalkeeper>());

        playerBlue1.GetComponent<AIPlayer>().FellowPlayer = playerBlue2.GetComponent<AIPlayer>();
        playerBlue2.GetComponent<AIPlayer>().FellowPlayer = playerBlue1.GetComponent<AIPlayer>();
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

    public HumanPlayer PlayerClosestToBall(int teamNumber)
    {
        HumanPlayer closestPlayer = null;
        float closestDistance = float.MaxValue;
        foreach(HumanPlayer player in teams[teamNumber].Players)
        {
            float distance = (player.transform.position - scriptBall.transform.position).magnitude;
            if(distance < closestDistance)
            {
                closestPlayer = player;
            }
        }

        return closestPlayer;
    }

    public HumanPlayer PlayerClosestToLocation(int teamNumber, Vector3 location)
    {
        HumanPlayer closestPlayer = null;
        float closestDistance = float.MaxValue;
        foreach (HumanPlayer player in teams[teamNumber].Players)
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
        playerLastTouchedBall.SetPlayerAction(ActionType_.Cheer);

        delayScoreGoal = DELAY_SCOREGOAL;
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
                ActiveHumanPlayer.GetComponent<ThirdPersonController>().MoveSpeed = 0;
                ActiveHumanPlayer.GetComponent<ThirdPersonController>().SprintSpeed = 0;
                break;
            case GameState_.Replay:
                ActiveHumanPlayer.GetComponent<ThirdPersonController>().MoveSpeed = 8;
                ActiveHumanPlayer.GetComponent<ThirdPersonController>().SprintSpeed = 15;
                recorder.PlayBack(goalOfTeamLastScored);
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
                scriptBall.IsOutOfField = false;
            }
        }

        if (goalTextColorAlpha > 0)
        {
            goalTextColorAlpha -= Time.deltaTime;
            textGoal.alpha = goalTextColorAlpha;
            textGoal.fontSize = 350 - (goalTextColorAlpha * 250);
        }

        if (delayScoreGoal>0)
        {
            delayScoreGoal -= Time.deltaTime;
            if (delayScoreGoal <= 0)
            {
                SetGameState(GameState_.Cheering);
            }
        }

//        PerformSanityChecks();
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
    public void SetMinimumDistanceOtherPlayers(HumanPlayer playerToTakeDistanceFrom)
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
            if (player is HumanPlayer && !((HumanPlayer)player).PlayerInput.enabled)
            {
                playerFollowCamera.Follow = player.PlayerCameraRoot;
                activeHumanPlayer.PlayerInput.enabled = false;
                activeHumanPlayer = (HumanPlayer)player;
                activeHumanPlayer.PlayerInput.enabled = true;
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

    public HumanPlayer GetPlayerToThrowIn()
    {
        return PlayerClosestToBall(0);
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
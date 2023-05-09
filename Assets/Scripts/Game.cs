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
    NewMatch,
    Playing,
    WaitingForWhistle,
    BringingBallIn,
    Cheering,
    Penalty,
    Replay,
    MatchOver
}

public enum Formation_
{
    _343,
    _433,
    _442,
    _532
}

public class Game : MonoBehaviour
{
    public static Game Instance;

    public const int NUM_FIELDPLAYERS = 10;          // between 1 and 10
    public const float DELAY_SCOREGOAL = 0.5f;                  // Wait a little bit after scoring to show at playback
    public const float HAVING_BALL_SLOWDOWN_FACTOR = 0.8f;
    public const float PLAYER_Y_POSITION = 0.5f;
    public const float FIELD_BOUNDARY_LOWER_X = -53.047f;
    public const float FIELD_BOUNDARY_UPPER_X = 52.873f;
    public const float FIELD_BOUNDARY_LOWER_Z = -25.37f;
    public const float FIELD_BOUNDARY_UPPER_Z = 25.552f;
    public const float MINIMUM_DISTANCE_FREEKICK = 18f;
    public const float WAITING_FOR_WHISTLE_DURATION = 2.0f;

    [SerializeField] private Recorder recorder;
    [SerializeField] private Material[] materialHair;
    [SerializeField] private Material[] materialBody;
    [SerializeField] private GameObject spawnPositionGoalkeeperLeftSide;
    [SerializeField] private GameObject spawnPositionGoalkeeperRightSide;
    [SerializeField] private GameObject pfPlayerHuman;
    [SerializeField] private GameObject pfPlayerAI;
    [SerializeField] private GameObject pfGoalkeeperHuman;
    [SerializeField] private GameObject pfGoalkeeperAI;
    [SerializeField] private TextMeshProUGUI textScore;
    [SerializeField] private TextMeshProUGUI textGoal;
    [SerializeField] private TextMeshProUGUI textPlayer;
    [SerializeField] private TextMeshProUGUI textReplay;
    [SerializeField] private TextMeshProUGUI textMessage;
    [SerializeField] private Canvas canvasGame;
    [SerializeField] private Canvas canvasGameOver;
    [SerializeField] private Transform[] goals;
    [SerializeField] private CinemachineVirtualCamera goalKeeperCameraTeam0;
    [SerializeField] private CinemachineVirtualCamera goalKeeperCameraTeam1;
    [SerializeField] private GameTimer gameTimer;
    private GameMode_ gameMode;
    private GameState_ gameState;
    private GameState_ nextGameState;
    private Slider sliderPowerBar;
    private GameObject powerBar;
    private Ball scriptBall;
    private AudioSource soundCheer1;
    private AudioSource soundCheer2;
    private AudioSource soundWhistle;
    private Player playerLastTouchedBall;
    private Player playerWithBall;
    private Player playerTakingPenalty;
    private Player playerReceivingPass;
    private HumanPlayer humanPlayerDestination;
    private HumanPlayer activeHumanPlayer;
    private AIPlayer aIPlayerDestination;
    private CinemachineVirtualCamera playerFollowCamera;
    private CinemachineVirtualCamera stadiumCamera;
    private Team teamWithBall;
    private Team teamLastTouched;
    private Team teamKickOff;
    private float timeGameStateStarted;
    private float timeShowMessageStarted;
    private float goalTextColorAlpha;
    private float delayScoreGoal;
    private List<Team> teams = new();
    private Vector3 kickOffPosition = new Vector3(0, PLAYER_Y_POSITION, 0.1f);
    private int goalOfTeamLastScored;

    public Player PlayerReceivingPass { get => playerReceivingPass; set => playerReceivingPass = value; }
    public Player PlayerWithBall { get => playerWithBall; }
    public Team TeamWithBall { get => teamWithBall; set => teamWithBall = value; }
    public Team TeamLastTouched { get => teamLastTouched; set => teamLastTouched = value; }
    public HumanPlayer ActiveHumanPlayer { get => activeHumanPlayer; set => activeHumanPlayer = value; }
    public HumanPlayer HumanPlayerDestination { get => humanPlayerDestination; set => humanPlayerDestination = value; }
    public List<Team> Teams { get => teams; set => teams = value; }
    public Vector3 KickOffPosition { get => kickOffPosition; set => kickOffPosition = value; }
    public GameMode_ GameMode { get => gameMode; set => gameMode = value; }
    public GameState_ GameState { get => gameState; set => gameState = value; }
    public GameState_ NextGameState { get => nextGameState; set => nextGameState = value; }
    public Ball ScriptBall { get => scriptBall; set => scriptBall = value; }
    public Recorder Recorder { get => recorder; set => recorder = value; }
    public Transform[] Goals { get => goals; set => goals = value; }
    public CinemachineVirtualCamera GoalKeeperCameraTeam0 { get => goalKeeperCameraTeam0; set => goalKeeperCameraTeam0 = value; }
    public CinemachineVirtualCamera GoalKeeperCameraTeam1 { get => goalKeeperCameraTeam1; set => goalKeeperCameraTeam1 = value; }
    public GameObject SpawnPositionGoalkeeperRed { get => spawnPositionGoalkeeperLeftSide; set => spawnPositionGoalkeeperLeftSide = value; }
    public Player PlayerTakingPenalty { get => playerTakingPenalty; set => playerTakingPenalty = value; }
    public CinemachineVirtualCamera StadiumCamera { get => stadiumCamera; set => stadiumCamera = value; }
    public AIPlayer AIPlayerDestination { get => aIPlayerDestination; set => aIPlayerDestination = value; }

    private void InitGamePlayerVsPC()
    {
        Team team0 = new(0, 0, true);
        teams.Add(team0);

        GameObject newPlayer = null;

        for (int playerNumber = 0; playerNumber < NUM_FIELDPLAYERS; playerNumber++)
        {
            newPlayer = Instantiate(pfPlayerHuman);
            newPlayer.name = "Player 0-" + (playerNumber+1);
            newPlayer.GetComponent<HumanFieldPlayer>().Number = playerNumber;
            newPlayer.GetComponent<HumanFieldPlayer>().Team = team0;
            newPlayer.transform.Find("Geometry/Root/Ch38_Hair").GetComponent<Renderer>().material = materialHair[Random.Range(0, materialHair.Length)];
            newPlayer.transform.Find("SelectedMarker").gameObject.SetActive(false);
            team0.Players.Add(newPlayer.GetComponent<HumanFieldPlayer>());
        }

        // player 0 is the kickoff player, so after activating we will have the next fieldplayer to be designated as "next"
        humanPlayerDestination = team0.Players[0].GetComponent<HumanFieldPlayer>();

        // the last human player is used as the active one
        ActiveHumanPlayer = team0.Players[0].GetComponent<HumanFieldPlayer>();
        playerFollowCamera.Follow = team0.Players[0].GetComponent<HumanFieldPlayer>().PlayerCameraRoot;

        GameObject goalkeeperTeam0 = Instantiate(pfGoalkeeperHuman, spawnPositionGoalkeeperLeftSide.transform.position, spawnPositionGoalkeeperLeftSide.transform.rotation);
        goalkeeperTeam0.name = "Goalie 0";
        goalkeeperTeam0.GetComponent<HumanGoalkeeper>().Team = team0;
        team0.Players.Add(goalkeeperTeam0.GetComponent<HumanGoalkeeper>());
        team0.GoalKeeper = goalkeeperTeam0.GetComponent<HumanGoalkeeper>();
        Formation.Set(team0, Formation_._433);

        Team team1 = new Team(1, 1, false);
        teams.Add(team1);

        for (int playerNumber = 0; playerNumber < NUM_FIELDPLAYERS; playerNumber++)
        {
            newPlayer = Instantiate(pfPlayerAI);
            newPlayer.name = "Player 1-" + (playerNumber+1);
            newPlayer.GetComponent<AIFieldPlayer>().Number = playerNumber;
            newPlayer.GetComponent<AIFieldPlayer>().Team = team1;
            newPlayer.transform.Find("Geometry/Root/Ch38_Hair").GetComponent<Renderer>().material = materialHair[Random.Range(0, materialHair.Length)];
            team1.Players.Add(newPlayer.GetComponent<AIFieldPlayer>());
        }

        Formation.Set(team1, Formation_._433);

        GameObject goalkeeperTeam1 = Instantiate(pfGoalkeeperAI, spawnPositionGoalkeeperRightSide.transform.position, spawnPositionGoalkeeperRightSide.transform.rotation);
        goalkeeperTeam1.name = "Goalie 1";
        goalkeeperTeam1.GetComponent<AIGoalkeeper>().Team = team1;
        team1.Players.Add(goalkeeperTeam1.GetComponent<AIGoalkeeper>());
        team1.GoalKeeper = goalkeeperTeam1.GetComponent<AIGoalkeeper>();

        teamKickOff = teams[1];
        
        ChangeShirt();
        
        ResetMatchStats();
    }

    public void SetMessage(string message)
    {
        textMessage.text = message;
        timeShowMessageStarted = Time.time;
    }

    private void ResetMatchStats()
    {
        for (int team = 0; team < 2; team++)
        {
            Teams[team].Stats.BallPosession = 0;
            Teams[team].Stats.BallOnHalf = 0;
            Teams[team].Stats.Shots = 0;
            Teams[team].Stats.ShotsOnGoal = 0;
            Teams[team].Stats.Corners = 0;
            Teams[team].Stats.Goals = new List<Goal>();
        }
    }

    public void Awake()
    {
        Instance = this;
        playerFollowCamera = GameObject.Find("VCam_PlayerFollow").GetComponent<CinemachineVirtualCamera>();
        stadiumCamera = GameObject.Find("VCam_Stadium").GetComponent<CinemachineVirtualCamera>();
        soundCheer1 = GameObject.Find("Sound/cheer1").GetComponent<AudioSource>();
        soundCheer2 = GameObject.Find("Sound/cheer2").GetComponent<AudioSource>();
        soundWhistle = GameObject.Find("Sound/whistle").GetComponent<AudioSource>();
        scriptBall = GameObject.Find("Ball").GetComponent<Ball>();
        sliderPowerBar = GameObject.Find("CanvasGame/Panel/PowerBar").GetComponent<Slider>();
        powerBar = GameObject.Find("CanvasGame/Panel/PowerBar");
        powerBar.SetActive(false);

        if (Settings.GameMode.Equals(GameMode_.PlayerVsPc))
        {
            InitGamePlayerVsPC();
        }
    }
    public void Start()
    {
        SetGameState(GameState_.NewMatch);
    }

    public void ResetPlayersAndBall()
    {
        foreach(Team team in teams)
        {
            foreach(Player player in team.Players)
            {
                Vector3 spawnPosition = team.PlayingSide == 0 ? new Vector3(player.SpawnPosition.x, PLAYER_Y_POSITION, player.SpawnPosition.y) : new Vector3(-player.SpawnPosition.x, PLAYER_Y_POSITION, player.SpawnPosition.y);
                player.SetPosition(spawnPosition);
                player.LookAt(goals[OtherTeam(player.Team).Number]);
            }
            GameObject spawnPositionGoalkeeper = team.PlayingSide == 0 ? spawnPositionGoalkeeperLeftSide : spawnPositionGoalkeeperRightSide;
            team.GoalKeeper.SetPosition(spawnPositionGoalkeeper.transform.position);
        }       

        // Set player to kick off
        Vector3 position = new(0.5f - teamKickOff.Number, KickOffPosition.y, kickOffPosition.z);
        teamKickOff.Players[0].SetPosition(position);
        SetPlayerWithBall(teamKickOff.Players[0]);
        teamKickOff.Players[0].DoingKick = true;
        scriptBall.BallOutOfFieldTimeOut = 0;
        scriptBall.PutOnCenterSpot();
        scriptBall.Rigidbody.velocity = Vector3.zero;
        scriptBall.Rigidbody.angularVelocity = Vector3.zero;
    }

    public Player FieldPlayerClosestToBall(int teamNumber)
    {
        Player closestPlayer = null;
        float closestDistance = float.MaxValue;
        foreach(Player player in teams[teamNumber].Players)
        {
            if (player == teams[teamNumber].GoalKeeper)
            {
                continue;
            }
            float distance = (player.transform.position - scriptBall.transform.position).magnitude;
            if (distance < closestDistance)
            {
                closestDistance = distance;
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
        teams[teamScored].Stats.Score++;
        teamKickOff = Teams[goalOfTeam];
        goalTextColorAlpha = 1;
        if(Random.value < 0.5)
        {
            soundCheer1.Play();
        }
        else
        {
            soundCheer2.Play();
        }
        textScore.text = "Score: " + teams[0].Stats.Score + "-" + teams[1].Stats.Score;
        playerLastTouchedBall.SetPlayerAction(ActionType_.Cheer, 0 , true);
        playerLastTouchedBall.Team.Stats.ShotsOnGoal++;
        playerLastTouchedBall.Team.Stats.Goals.Add(new Goal(playerLastTouchedBall.name, gameTimer.TimePassedAs90Minutes()));

        delayScoreGoal = DELAY_SCOREGOAL;
    }

    public void KickOff()
    {
        Utilities.Log("Kickoff", Utilities.DEBUG_TOPIC_MATCH_EVENTS);
        ResetPlayersAndBall();
        ActiveHumanPlayer.MovementDisabled = true;
        Utilities.Log("Movement disabled for player: " + ActiveHumanPlayer.name, Utilities.DEBUG_TOPIC_PLAYER_EVENTS);
        nextGameState = GameState_.BringingBallIn;
        SetGameState(GameState_.WaitingForWhistle);
    }

    public void TerminateAllRunningActions()
    {
        Utilities.Log("TerminateAllRunningActions", Utilities.DEBUG_TOPIC_PLAYERACTION);
        foreach (Team team in teams)
        {
            foreach (Player player in team.Players)
            {
                player.PlayerAction.TerminateRunningAction();
            }
        }
    }

    public void SetGameState(GameState_ newGameState)
    {
        Utilities.Log("Changed gamestate to: " + newGameState.ToString(), Utilities.DEBUG_TOPIC_MATCH_EVENTS);
        gameState = newGameState;
        timeGameStateStarted = Time.time;

        switch (newGameState)
        {
            case GameState_.NewMatch:
                canvasGameOver.enabled = false;
                canvasGame.enabled = true;
                KickOff();
                break;
            case GameState_.WaitingForWhistle:
                TerminateAllRunningActions();
                if (NextGameState==GameState_.Penalty)
                {
                    if (playerTakingPenalty.Team.Number == 1)
                    {
                        ((AIFieldPlayer)playerTakingPenalty).PrepareForPenalty();
                    }
                }
                break;
            case GameState_.Cheering:
                ActiveHumanPlayer.MovementDisabled = true;
                Utilities.Log("Movement disabled for player: " + ActiveHumanPlayer.name, Utilities.DEBUG_TOPIC_PLAYER_EVENTS);
                break;
            case GameState_.Replay:
                playerLastTouchedBall.PlayerAction.TerminateRunningAction();        // Stop cheering
                ActiveHumanPlayer.MovementDisabled = false;
                Utilities.Log("Movement enabled for player: " + ActiveHumanPlayer.name, Utilities.DEBUG_TOPIC_PLAYER_EVENTS);
                recorder.PlayBack(goalOfTeamLastScored);
                break;
            case GameState_.Penalty:
                if (playerTakingPenalty.Team.Number==1)
                {
                    ((AIFieldPlayer)playerTakingPenalty).TakePenalty();
                }
                break;
            case GameState_.MatchOver:
                DisplayMatchStats();
                canvasGame.enabled = false;
                canvasGameOver.enabled = true;
                break;
        }
    }

    private void DisplayMatchStats()
    {
        string textWon = "It's a draw!";
        if(teams[0].Stats.Score> teams[1].Stats.Score)
        {
            textWon = "Team 0 won!";
        }
        if (teams[1].Stats.Score > teams[0].Stats.Score)
        {
            textWon = "Team 1 won!";
        }
        GameObject.Find("CanvasGameOver/Panel/TextWon").GetComponent<TextMeshProUGUI>().SetText(textWon);
        GameObject.Find("CanvasGameOver/Panel/TextGoals0").GetComponent<TextMeshProUGUI>().SetText(teams[0].Stats.Score.ToString());
        GameObject.Find("CanvasGameOver/Panel/TextGoals1").GetComponent<TextMeshProUGUI>().SetText(teams[1].Stats.Score.ToString());
        float ballpossessionTeam0 = 50, ballpossessionTeam1 = 50;
        if (teams[0].Stats.BallPosession + teams[1].Stats.BallPosession>0)
        { 
            ballpossessionTeam0 = 100 * teams[0].Stats.BallPosession / (teams[0].Stats.BallPosession + teams[1].Stats.BallPosession);
            ballpossessionTeam1 = 100 * teams[1].Stats.BallPosession / (teams[0].Stats.BallPosession + teams[1].Stats.BallPosession);
        }
        GameObject.Find("CanvasGameOver/Panel/TextBallPosession0").GetComponent<TextMeshProUGUI>().SetText(ballpossessionTeam0.ToString("0") + " %");
        GameObject.Find("CanvasGameOver/Panel/TextBallPosession1").GetComponent<TextMeshProUGUI>().SetText(ballpossessionTeam1.ToString("0") + " %");
        float ballOnHalf = 100 * teams[0].Stats.BallOnHalf / (teams[0].Stats.BallOnHalf + teams[1].Stats.BallOnHalf);
        GameObject.Find("CanvasGameOver/Panel/TextBallOnHalf0").GetComponent<TextMeshProUGUI>().SetText(ballOnHalf.ToString("0") + " %");
        ballOnHalf = 100 * teams[1].Stats.BallOnHalf / (teams[0].Stats.BallOnHalf + teams[1].Stats.BallOnHalf);
        GameObject.Find("CanvasGameOver/Panel/TextBallOnHalf1").GetComponent<TextMeshProUGUI>().SetText(ballOnHalf.ToString("0") + " %");
        GameObject.Find("CanvasGameOver/Panel/TextShots0").GetComponent<TextMeshProUGUI>().SetText(teams[0].Stats.Shots.ToString());
        GameObject.Find("CanvasGameOver/Panel/TextShots1").GetComponent<TextMeshProUGUI>().SetText(teams[1].Stats.Shots.ToString());
        GameObject.Find("CanvasGameOver/Panel/TextShotsOnGoal0").GetComponent<TextMeshProUGUI>().SetText(teams[0].Stats.ShotsOnGoal.ToString());
        GameObject.Find("CanvasGameOver/Panel/TextShotsOnGoal1").GetComponent<TextMeshProUGUI>().SetText(teams[1].Stats.ShotsOnGoal.ToString());
        GameObject.Find("CanvasGameOver/Panel/TextScorers0").GetComponent<TextMeshProUGUI>().SetText(teams[0].Stats.GoalsAsText());
        GameObject.Find("CanvasGameOver/Panel/TextScorers1").GetComponent<TextMeshProUGUI>().SetText(teams[1].Stats.GoalsAsText());
        GameObject.Find("CanvasGameOver/Panel/TextCorners0").GetComponent<TextMeshProUGUI>().SetText(teams[0].Stats.Corners.ToString());
        GameObject.Find("CanvasGameOver/Panel/TextCorners1").GetComponent<TextMeshProUGUI>().SetText(teams[1].Stats.Corners.ToString());
    }

    public void FixedUpdate()
    {
        if (gameState == GameState_.Playing || gameState == GameState_.Penalty)
        {
            if (scriptBall.transform.position.x > 0)
            {
                teams[0].Stats.BallOnHalf++;
            }
            else
            {
                teams[1].Stats.BallOnHalf++;
            }
            if (PlayerWithBall != null && PlayerWithBall.Team.Number == 0)
            {
                teams[0].Stats.BallPosession++;
            }
            if (PlayerWithBall != null && PlayerWithBall.Team.Number == 1)
            {
                teams[1].Stats.BallPosession++;
            }
        }
    }

    public void Update()
    {
        if (textMessage.text.Length>0)
        {
            if (Time.time - timeShowMessageStarted > 1)
            {
                textMessage.text = "";
            }
        }


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
                ActiveHumanPlayer.MovementDisabled = false;
                Utilities.Log("Movement enabled for player: " + ActiveHumanPlayer.name, Utilities.DEBUG_TOPIC_PLAYER_EVENTS);
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

    public Team OtherTeam(Team team)
    {
        if (team.Number == 0)
        {
            return teams[1];
        }
        else
        {
            return teams[0];
        }
    }

    public void SetPowerBar(float value)
    {
        powerBar.SetActive(true);
        sliderPowerBar.value = value;
    }

    public void RemovePowerBar()
    {
        powerBar.SetActive(false);
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
        PlayerReceivingPass = null;
        if (playerWithBall != null)
        {
            if (player.Team.IsHuman)
            {
                ActivateHumanPlayer((HumanPlayer)player);
            }
            GoalKeeperCameraTeam0.enabled = false;
            if (player is HumanPlayer)
            {
                playerFollowCamera.Follow = player.PlayerCameraRoot;
                activeHumanPlayer = (HumanPlayer)player;
            }
            scriptBall.PutOnGround();
            Utilities.Log("has ball :" + player.name, Utilities.DEBUG_TOPIC_PLAYER_EVENTS);
            player.HasBall = true;
            playerLastTouchedBall = playerWithBall;
            teamLastTouched = teamWithBall = playerWithBall.Team;
            textPlayer.text = playerWithBall.name;
        }
        else
        {
            teamWithBall = null;
            textPlayer.text = "";
        }
    }

    public void ChangeShirt()
    {
        Material shirtTeam0 = materialBody[Random.Range(0, materialBody.Length)];
        foreach (Player player in Teams[0].Players)
        {
            if (!player.Equals(Teams[0].GoalKeeper))
            {
                player.transform.Find("Geometry/Root/Ch38_Shirt").GetComponent<Renderer>().material = shirtTeam0;
            }
        }

        Material shirtTeam1;
        do
        {
            shirtTeam1 = materialBody[Random.Range(0, materialBody.Length)];
        } while (shirtTeam0.Equals(shirtTeam1));
        foreach (Player player in Teams[1].Players)
        {
            if (!player.Equals(Teams[1].GoalKeeper))
            {
                player.transform.Find("Geometry/Root/Ch38_Shirt").GetComponent<Renderer>().material = shirtTeam1;
            }
        }
    }

    public Player GetPlayerToThrowIn()
    {
        return FieldPlayerClosestToBall(0);
        //return FieldPlayerClosestToBall(OtherTeam(teamLastTouched));
    }

    public Player GetPlayerToGoalKick()
    {
        return teams[0].GoalKeeper;
        //return teams[OtherTeam(teamLastTouched)].GoalKeeper;
    }

    public void ActivateHumanPlayer(HumanPlayer player)
    {
        ActiveHumanPlayer = player;
        playerFollowCamera.Follow = player.PlayerCameraRoot;
        SetNextHumanPlayer();
    }

    public void SetNextHumanPlayer()
    {
        humanPlayerDestination.transform.Find("SelectedMarker").gameObject.SetActive(false);
        if (humanPlayerDestination.Number<humanPlayerDestination.Team.Players.Count-1)
        {
            humanPlayerDestination = (HumanPlayer)humanPlayerDestination.Team.Players[humanPlayerDestination.Number+1];
        }
        else
        {
            humanPlayerDestination = (HumanPlayer)humanPlayerDestination.Team.Players[0];
        }
        humanPlayerDestination.transform.Find("SelectedMarker").gameObject.SetActive(true);
    }

    public Player GetClosestPlayerOfSameTeam(Player currentPlayer)
    {
        Player closestPlayer = null;
        float closestDistance = Mathf.Infinity;
        foreach (Player player in currentPlayer.Team.Players)
        {
            if (player == currentPlayer)
            {
                continue;
            }
            Vector3 distanceToCurrentPlayer = player.transform.position - currentPlayer.transform.position;
            float sqrDistanceToCurrentPlayer = distanceToCurrentPlayer.sqrMagnitude;
            if (sqrDistanceToCurrentPlayer < closestDistance)
            {
                closestDistance = sqrDistanceToCurrentPlayer;
                closestPlayer = player;
            }
        }

        return closestPlayer;
    }

}
using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct RecorderFrame
{
    public Vector3 position;
    public Quaternion rotation;
    public float[] layers;
    public bool jump;
    public bool grounded;
    public bool freefall;
    public float speed;
}

class Recording
{
    public RecorderFrame[] frames;

    public Recording(int num_frames)
    {
        this.frames = new RecorderFrame[num_frames];
    }
}

class RecorderTeam
{
    public List<Recording> players = new List<Recording>();
}


public class Recorder : MonoBehaviour
{
    [SerializeField] Ball ball;
    [SerializeField] private CinemachineVirtualCamera[] cameraReplayGoal;
    [SerializeField] private CinemachineVirtualCamera[] cameraReplayField;

    // animation IDs
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;

    private const int NUM_RECORDED_FRAMES = 250;        // 5 seconds
    private const int NUM_RECORDED_FRAMES_WITHOUT_SLOWMOTION = 200;

    List<RecorderTeam> recordedTeams = new List<RecorderTeam>();
    Recording recordedBall = new Recording(NUM_RECORDED_FRAMES);
    int numTeams;
    int numPlayersPerTeam;
    int currentRecordingFrame;

    int currentPlayingFrame;
    int framesPlayed;
    int replaysPlayed;
    int goalOfTeam;

    int slowdownCount;

    // Start is called before the first frame update
    void Start()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");

        numTeams = Game.Instance.Teams.Count;
        numPlayersPerTeam = Game.Instance.Teams[0].Players.Count;
        recordedBall.frames = new RecorderFrame[NUM_RECORDED_FRAMES];
        for (int teamNumber = 0; teamNumber < numTeams; teamNumber++)
        {
            RecorderTeam newTeam = new RecorderTeam();
            for (int playerNumber = 0; playerNumber < numPlayersPerTeam; playerNumber++)
            {
                Recording recording = new Recording(NUM_RECORDED_FRAMES);
                for (int i=0; i< NUM_RECORDED_FRAMES; i++)
                {
                    recording.frames[i].layers = new float[Game.Instance.Teams[teamNumber].Players[playerNumber].Animator.layerCount];
                }
                newTeam.players.Add(recording);
            }
            recordedTeams.Add(newTeam);
        }
    }

    private int GetNextFrame(int frameNumber)
    {
        frameNumber++;
        if (frameNumber >= NUM_RECORDED_FRAMES)
        {
            frameNumber = 0;
        }

        return frameNumber;
    }

    private void Record()
    {
        currentRecordingFrame = GetNextFrame(currentRecordingFrame);

        for (int teamNumber = 0; teamNumber < recordedTeams.Count; teamNumber++)
        {
            for (int playerNumber = 0; playerNumber < numPlayersPerTeam; playerNumber++)
            {
                recordedTeams[teamNumber].players[playerNumber].frames[currentRecordingFrame].position = Game.Instance.Teams[teamNumber].Players[playerNumber].transform.position;
                recordedTeams[teamNumber].players[playerNumber].frames[currentRecordingFrame].rotation = Game.Instance.Teams[teamNumber].Players[playerNumber].transform.rotation;
                recordedTeams[teamNumber].players[playerNumber].frames[currentRecordingFrame].jump = Game.Instance.Teams[teamNumber].Players[playerNumber].Animator.GetBool(_animIDJump);
                recordedTeams[teamNumber].players[playerNumber].frames[currentRecordingFrame].grounded = Game.Instance.Teams[teamNumber].Players[playerNumber].Animator.GetBool(_animIDGrounded);
                recordedTeams[teamNumber].players[playerNumber].frames[currentRecordingFrame].freefall = Game.Instance.Teams[teamNumber].Players[playerNumber].Animator.GetBool(_animIDFreeFall);
                recordedTeams[teamNumber].players[playerNumber].frames[currentRecordingFrame].speed = Game.Instance.Teams[teamNumber].Players[playerNumber].Animator.GetFloat(_animIDSpeed);
                for (int layer = 0; layer < Game.Instance.Teams[teamNumber].Players[playerNumber].Animator.layerCount; layer++)
                {
                    recordedTeams[teamNumber].players[playerNumber].frames[currentRecordingFrame].layers[layer] = Game.Instance.Teams[teamNumber].Players[playerNumber].Animator.GetLayerWeight(layer);
//                    Utilities.LogToFile(@"F:\projects\kickit\recording.txt", "write frame" + currentRecordingFrame + " player[" + teamNumber + "][" + playerNumber + "].layer[" + layer + "] = " + Game.Instance.Teams[teamNumber].Players[playerNumber].Animator.GetLayerWeight(layer));
                }
            }
        }
        recordedBall.frames[currentRecordingFrame].position = ball.transform.position;
        recordedBall.frames[currentRecordingFrame].rotation = ball.transform.rotation;
    }

    private void Play()
    {
        if (framesPlayed < NUM_RECORDED_FRAMES_WITHOUT_SLOWMOTION || slowdownCount > 1)
        {
            slowdownCount = 0;
            currentPlayingFrame = GetNextFrame(currentPlayingFrame);
            framesPlayed++;

            ball.transform.position = recordedBall.frames[currentPlayingFrame].position;
            ball.transform.rotation = recordedBall.frames[currentPlayingFrame].rotation;
        }
        else
        {
            // interpolate between this and next frame
            Vector3 nextPosition = recordedBall.frames[GetNextFrame(currentPlayingFrame)].position;
            ball.transform.position = Vector3.Lerp(recordedBall.frames[currentPlayingFrame].position, nextPosition, 0.5f);
            Quaternion nextRotation = recordedBall.frames[GetNextFrame(currentPlayingFrame)].rotation;
            ball.transform.rotation = Quaternion.Lerp(recordedBall.frames[currentPlayingFrame].rotation, nextRotation, 0.5f);

            slowdownCount++;
        }

        ball.transform.position = recordedBall.frames[currentPlayingFrame].position;
        ball.transform.rotation = recordedBall.frames[currentPlayingFrame].rotation;

        for (int teamNumber = 0; teamNumber < numTeams; teamNumber++)
        {
            for (int playerNumber = 0; playerNumber < numPlayersPerTeam; playerNumber++)
            {
                Game.Instance.Teams[teamNumber].Players[playerNumber].transform.position = recordedTeams[teamNumber].players[playerNumber].frames[currentPlayingFrame].position;
                Game.Instance.Teams[teamNumber].Players[playerNumber].transform.rotation = recordedTeams[teamNumber].players[playerNumber].frames[currentPlayingFrame].rotation;
                Game.Instance.Teams[teamNumber].Players[playerNumber].Animator.SetBool(_animIDJump, recordedTeams[teamNumber].players[playerNumber].frames[currentPlayingFrame].jump);
                Game.Instance.Teams[teamNumber].Players[playerNumber].Animator.SetBool(_animIDGrounded, recordedTeams[teamNumber].players[playerNumber].frames[currentPlayingFrame].grounded);
                Game.Instance.Teams[teamNumber].Players[playerNumber].Animator.SetBool(_animIDFreeFall, recordedTeams[teamNumber].players[playerNumber].frames[currentPlayingFrame].freefall);
                Game.Instance.Teams[teamNumber].Players[playerNumber].Animator.SetFloat(_animIDSpeed, recordedTeams[teamNumber].players[playerNumber].frames[currentPlayingFrame].speed);
                for (int layer = 0; layer < Game.Instance.Teams[teamNumber].Players[playerNumber].Animator.layerCount; layer++)
                {
                    Game.Instance.Teams[teamNumber].Players[playerNumber].Animator.SetLayerWeight(layer, recordedTeams[teamNumber].players[playerNumber].frames[currentPlayingFrame].layers[layer]);
                    //                    Utilities.LogToFile(@"F:\projects\kickit\recording.txt", "read frame" + currentRecordingFrame + " player[" + teamNumber + "][" + playerNumber + "].layer[" + layer + "] = " + recordedTeams[teamNumber].players[playerNumber].frames[currentPlayingFrame].layers[layer]);
                }
            }
        }

        if (framesPlayed >= NUM_RECORDED_FRAMES)
        {
            replaysPlayed++;
            if (replaysPlayed == 1)
            {
                framesPlayed = 0;
                cameraReplayField[goalOfTeam].enabled = false;
                cameraReplayGoal[goalOfTeam].enabled = true;
            }
            else
            {
                EndReplay();
            }
        }
    }

    private void ResetAllAnimationLayers()
    {
        for (int teamNumber = 0; teamNumber < numTeams; teamNumber++)
        {
            for (int playerNumber = 0; playerNumber < numPlayersPerTeam; playerNumber++)
            {
                for (int layer = 1; layer < Game.Instance.Teams[teamNumber].Players[playerNumber].Animator.layerCount; layer++)
                {
                    Game.Instance.Teams[teamNumber].Players[playerNumber].Animator.SetLayerWeight(layer, 0);
                }
            }
        }
    }

    public void EndReplay()
    {
        Utilities.Log("end replay", Utilities.DEBUG_TOPIC_REPLAY);
        ResetAllAnimationLayers();
        cameraReplayGoal[0].enabled = false;
        cameraReplayGoal[1].enabled = false;
        cameraReplayField[0].enabled = false;
        cameraReplayField[1].enabled = false;
        Game.Instance.ScriptBall.Rigidbody.isKinematic = false;
        Game.Instance.ScriptBall.Rigidbody.useGravity = true;
        Game.Instance.KickOff();
    }

    void FixedUpdate()
    {
        if (!Game.Instance.GameState.Equals(GameState_.Replay) && !Game.Instance.GameState.Equals(GameState_.Cheering))
        {
            Record();
        }
        if (Game.Instance.GameState.Equals(GameState_.Replay))
        {
            Play();
        }
    }

    public void PlayBack(int goalOfTeam)
    {
        Utilities.Log("begin replay", Utilities.DEBUG_TOPIC_REPLAY);
        this.goalOfTeam = goalOfTeam;

        framesPlayed = replaysPlayed = 0;
        currentPlayingFrame = currentRecordingFrame;
        cameraReplayField[goalOfTeam].enabled = true;
    }
}

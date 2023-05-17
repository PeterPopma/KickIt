using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActionType_
{
    ThrowinPass,
    ThrowinShot,
    Pass,
    Shot,
    Cheer,
    Tackle,
    Fall,
    GoalKeeperDiveLeft,
    GoalKeeperDiveRight,
    GoalKeeperBlockLeft,
    GoalKeeperBlockRight,
    GoalKeeperJump,
    GoalKeeperCatch,
}

public class PlayerAction
{
    public const float ANIMATION_DURATION_KICK = 0.5f;
    public const float ANIMATION_DURATION_THROWIN = 2.0f;
    public const float ANIMATION_DURATION_CHEERING = 4.0f;
    public const float ANIMATION_DURATION_TACKLE = 0.5f;
    public const float ANIMATION_DURATION_FALL = 1.7f;
    public const float ANIMATION_DURATION_DIVE = 1.0f;
    public const float ANIMATION_DURATION_BLOCK = 1.0f;
    public const float ANIMATION_DURATION_CATCH = 1.7f;
    public const float ANIMATION_DURATION_JUMP = 1.7f;

    public const float DELAY_KICK = 0.0f;
    public const float DELAY_THROW = 0.1f;
    public const float DELAY_TACKLE = 0.2f;

    public const float FADE_IN_TIME = 0.2f;
    public const float FADE_OUT_TIME = 0.2f;

    private float timeStarted;
    private float durationActionDelay;
    private ActionType_ actionType;
    private PlayerAnimation playerAnimation;
    private float power;
    private bool running;
    private bool excuted;
    private Player player;
    private Animator animator;


    public ActionType_ ActionType { get => actionType; set => actionType = value; }
    public float Power { get => power; set => power = value; }
    public PlayerAnimation PlayerAnimation { get => playerAnimation; set => playerAnimation = value; }
    public float TimeStarted { get => timeStarted; set => timeStarted = value; }
    public bool Running { get => running; set => running = value; }

    public PlayerAction(Player player, Animator animator)
    {
        this.player = player;
        this.animator = animator;
    }

    private void HandleAnimation()
    {
        if (playerAnimation != null)
        {
            if (playerAnimation.FadeIn)
            {
                animator.SetLayerWeight(playerAnimation.Layer, Mathf.Lerp(animator.GetLayerWeight(playerAnimation.Layer), 1f, (Time.time - timeStarted) / FADE_IN_TIME));
            }

            if (Time.time - timeStarted > playerAnimation.Duration)
            {
                if (playerAnimation.FadeOut)
                {
                    float timeSinceEnded = (Time.time - timeStarted) - playerAnimation.Duration;
                    animator.SetLayerWeight(playerAnimation.Layer, Mathf.Lerp(animator.GetLayerWeight(playerAnimation.Layer), 0f, timeSinceEnded / FADE_OUT_TIME));
                }
                else
                {
                    animator.SetLayerWeight(playerAnimation.Layer, 0);
                }

                if (animator.GetLayerWeight(playerAnimation.Layer) < 0.05)
                {
                    OnActionFinished();
                }
            }
        }
    }

    private void OnActionFinished()
    {
        Utilities.Log("finished animation: " + playerAnimation.Layer.ToString() + " time: " + Time.time, Utilities.DEBUG_TOPIC_PLAYERACTION);
        running = false;
        playerAnimation = null;
        switch (actionType)
        {
            case ActionType_.Cheer:
                Utilities.Log("End Cheering", Utilities.DEBUG_TOPIC_MATCH_EVENTS);
                break;
            case ActionType_.Tackle:
                player.MovementDisabled = false;
                break;
            case ActionType_.Fall:
                player.MovementDisabled = false;
                break;
            case ActionType_.Shot:
            case ActionType_.Pass:
                if (player is HumanGoalkeeper)      // goalkick
                {
                    player.transform.position = Game.Instance.SpawnPositionGoalkeeper(player.Team);
                    //Game.Instance.ActivateHumanPlayer((HumanPlayer)Game.Instance.PlayerReceivingPass);
                }
                break;
            case ActionType_.GoalKeeperDiveLeft:                
            case ActionType_.GoalKeeperDiveRight:
            case ActionType_.GoalKeeperBlockLeft:
            case ActionType_.GoalKeeperBlockRight:
            case ActionType_.GoalKeeperJump:
            case ActionType_.GoalKeeperCatch:
                player.transform.position = Game.Instance.SpawnPositionGoalkeeper(player.Team);
                break;
        }
    }

    public void StartAction(ActionType_ actionType, float power)
    {
        Utilities.Log("start action: " + actionType.ToString() + " time: " + Time.time, Utilities.DEBUG_TOPIC_PLAYERACTION);
        this.actionType = actionType;
        this.power = power;
        excuted = false;
        running = true;
        timeStarted = Time.time;
        switch (actionType)
        {
            case ActionType_.Pass:
                player.DoingKick = false;
                durationActionDelay = DELAY_KICK;
                playerAnimation = new PlayerAnimation(animator, ANIMATION_DURATION_KICK, false, true, PlayerAnimation.LAYER_SHOOT, "Shoot");
                break;

            case ActionType_.Shot:
                player.DoingKick = false;
                durationActionDelay = DELAY_KICK;
                playerAnimation = new PlayerAnimation(animator, ANIMATION_DURATION_KICK, false, true, PlayerAnimation.LAYER_SHOOT, "Shoot");
                break;

            case ActionType_.ThrowinPass:
                player.DoingThrow = false;
                durationActionDelay = DELAY_THROW;
                playerAnimation = new PlayerAnimation(animator, ANIMATION_DURATION_THROWIN, false, true, PlayerAnimation.LAYER_THROW_IN, "ThrowIn", 0.4f);
                break;

            case ActionType_.ThrowinShot:
                player.DoingThrow = false;
                durationActionDelay = DELAY_THROW;
                playerAnimation = new PlayerAnimation(animator, ANIMATION_DURATION_THROWIN, false, true, PlayerAnimation.LAYER_THROW_IN, "ThrowIn", 0.4f);
                break;

            case ActionType_.Cheer:
                Utilities.Log("Begin Cheering", Utilities.DEBUG_TOPIC_MATCH_EVENTS);
                durationActionDelay = ANIMATION_DURATION_CHEERING;
                playerAnimation = new PlayerAnimation(animator, ANIMATION_DURATION_CHEERING, false, false, PlayerAnimation.LAYER_CHEER, "Cheer");
                break;

            case ActionType_.Tackle:
                player.SoundSlide.Play();
                durationActionDelay = DELAY_TACKLE;
                playerAnimation = new PlayerAnimation(animator, ANIMATION_DURATION_TACKLE, true, true, PlayerAnimation.LAYER_TACKLE, "Tackle");
                break;

            case ActionType_.Fall:
                playerAnimation = new PlayerAnimation(animator, ANIMATION_DURATION_FALL, true, true, PlayerAnimation.LAYER_FALL, "Fall", 0.4f);
                break;

            case ActionType_.GoalKeeperDiveLeft:
                playerAnimation = new PlayerAnimation(animator, ANIMATION_DURATION_DIVE, false, false, PlayerAnimation.LAYER_DIVE_LEFT, "DiveLeft");
                break;

            case ActionType_.GoalKeeperDiveRight:
                playerAnimation = new PlayerAnimation(animator, ANIMATION_DURATION_DIVE, false, false, PlayerAnimation.LAYER_DIVE_RIGHT, "DiveRight");
                break;

            case ActionType_.GoalKeeperBlockLeft:
                playerAnimation = new PlayerAnimation(animator, ANIMATION_DURATION_BLOCK, false, false, PlayerAnimation.LAYER_BLOCK_LEFT, "BlockLeft");
                break;

            case ActionType_.GoalKeeperBlockRight:
                playerAnimation = new PlayerAnimation(animator, ANIMATION_DURATION_BLOCK, false, false, PlayerAnimation.LAYER_BLOCK_RIGHT, "BlockRight");
                break;

            case ActionType_.GoalKeeperJump:
                playerAnimation = new PlayerAnimation(animator, ANIMATION_DURATION_JUMP, false, false, PlayerAnimation.LAYER_JUMP, "Jump");
                break;

            case ActionType_.GoalKeeperCatch:
                playerAnimation = new PlayerAnimation(animator, ANIMATION_DURATION_CATCH, false, false, PlayerAnimation.LAYER_CATCH, "Catch");
                break;
        }
    }

    public void TerminateRunningAction()
    {
        if (running)
        {
            running = false;
            if (playerAnimation != null)
            {
                animator.SetLayerWeight(playerAnimation.Layer, 0);
            }
            OnActionFinished();
        }
    }

    private void ExecuteAction()
    {
        excuted = true;
        Utilities.Log("executed action: " + actionType.ToString() + " time: " + Time.time, Utilities.DEBUG_TOPIC_PLAYERACTION);
        player.TransformBall.parent = null;

        switch (actionType)
        {
            case ActionType_.Pass:
                player.PassBallToPlayer();
                Game.Instance.SetGameState(GameState_.Playing);
                break;

            case ActionType_.Shot:
                player.TakeShot(power);
                Game.Instance.SetGameState(GameState_.Playing);
                break;

            case ActionType_.ThrowinPass:
                player.ScriptBall.DelayCheckOutOfField = 0.5f;
                player.ScriptBall.Rigidbody.isKinematic = false;
                player.PassBallToPlayer();
                Game.Instance.SetGameState(GameState_.Playing);
                break;

            case ActionType_.ThrowinShot:
                player.ScriptBall.DelayCheckOutOfField = 0.5f;
                player.ScriptBall.Rigidbody.isKinematic = false;
                player.TakeShot(power);
                Game.Instance.SetGameState(GameState_.Playing);
                break;

            case ActionType_.Cheer:
                Game.Instance.SetGameState(GameState_.Replay);
                break;

            case ActionType_.Tackle:
                player.TacklePlayers();
                player.MovementDisabled = true;
                Utilities.Log("Movement disabled for player: " + player.name, Utilities.DEBUG_TOPIC_PLAYER_EVENTS);
                break;

            case ActionType_.Fall:
                player.MovementDisabled = true;
                Utilities.Log("Movement enabled for player: " + player.name, Utilities.DEBUG_TOPIC_PLAYER_EVENTS);
                break;
        }
    }


    public void Update()
    {
        if (!running)
        {
            return;
        }
        if (!Game.Instance.GameState.Equals(GameState_.Replay))
        {
            HandleAnimation();
        }
        if (!excuted && Time.time - timeStarted >= durationActionDelay)
        {
            ExecuteAction();
        }

        switch (actionType)
        {
            case ActionType_.ThrowinShot:
            case ActionType_.ThrowinPass:
                if (!excuted)
                {
                    // ball follow hand as long as throw in has not been performed
                    player.TransformBall.position = player.BallHandPosition.position;
                }
                break;
            case ActionType_.GoalKeeperDiveLeft:
                if (Time.time - timeStarted < 0.4f)
                {
                    player.transform.Translate(new Vector3(0, 0, -8f) * Time.deltaTime, Space.World);
                }
                if (Time.time - timeStarted < 0.25f)
                {
                    player.transform.Translate(new Vector3(0, 6, 0f) * Time.deltaTime, Space.World);
                }
                else if (player.transform.position.y > Game.PLAYER_Y_POSITION)
                {
                    player.transform.Translate(new Vector3(0, -6, 0) * Time.deltaTime, Space.World);
                }
                break;

            case ActionType_.GoalKeeperDiveRight:
                if (Time.time - timeStarted < 0.4f)
                {
                    player.transform.Translate(new Vector3(0, 0, 8f) * Time.deltaTime, Space.World);
                }
                if (Time.time - timeStarted < 0.25f)
                {
                    player.transform.Translate(new Vector3(0, 6, 0f) * Time.deltaTime, Space.World);
                }
                else if (player.transform.position.y > Game.PLAYER_Y_POSITION)
                {
                    player.transform.Translate(new Vector3(0, -6, 0) * Time.deltaTime, Space.World);
                }
                break;

            case ActionType_.GoalKeeperBlockLeft:
                if (Time.time - timeStarted < 0.25f)
                {
                    player.transform.Translate(new Vector3(0, 0, -8f) * Time.deltaTime, Space.World);
                }
                break;

            case ActionType_.GoalKeeperBlockRight:
                if (Time.time - timeStarted < 0.25f)
                {
                    player.transform.Translate(new Vector3(0, 0, 8f) * Time.deltaTime, Space.World);
                }
                break;

            case ActionType_.GoalKeeperJump:
                if (Time.time - timeStarted < 0.2f)
                {
                    player.transform.Translate(new Vector3(0, 6, 0) * Time.deltaTime, Space.World);
                }
                else if(player.transform.position.y > Game.PLAYER_Y_POSITION)
                {
                    player.transform.Translate(new Vector3(0, -6, 0) * Time.deltaTime, Space.World);
                }
                break;

            case ActionType_.GoalKeeperCatch:
                break;
        }
    }
}

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
    Fall
}

public class PlayerAction
{
    public const float ANIMATION_DURATION_KICK = 0.5f;
    public const float ANIMATION_DURATION_THROWIN = 2.0f;
    public const float ANIMATION_DURATION_CHEERING = 4.0f;
    public const float ANIMATION_DURATION_TACKLE = 0.5f;
    public const float ANIMATION_DURATION_FALL = 1.7f;

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
        Debug.Log("finished animation: " + playerAnimation.Layer.ToString() + " time: " + Time.time);
        running = false;
        playerAnimation = null;
        switch (actionType)
        {
            case ActionType_.Tackle:
                player.MovementDisabled = false;
                break;
            case ActionType_.Fall:
                player.MovementDisabled = false;
                break;
        }
    }

    public void StartAction(ActionType_ actionType, float power)
    {
        Debug.Log("start action: " + actionType.ToString() + " time: " + Time.time);
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
        }
    }

    private void ExecuteAction()
    {
        excuted = true;
        Debug.Log("executed action: " + actionType.ToString() + " time: " + Time.time);
        player.TransformBall.parent = null;

        switch (actionType)
        {
            case ActionType_.Pass:
                player.TakePass();
                Game.Instance.SetGameState(GameState_.Playing);
                break;

            case ActionType_.Shot:
                player.TakeShot(power);
                Game.Instance.SetGameState(GameState_.Playing);
                break;

            case ActionType_.ThrowinPass:
                player.ScriptBall.DelayCheckOutOfField = 0.5f;
                player.ScriptBall.Rigidbody.isKinematic = false;
                player.TakePass();
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
                break;

            case ActionType_.Fall:
                player.MovementDisabled = true;
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
        if (running && (actionType.Equals(ActionType_.ThrowinShot) || actionType.Equals(ActionType_.ThrowinPass)))
        {
            player.TransformBall.position = player.BallHandPosition.position;
        }
    }
}

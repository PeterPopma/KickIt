using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActionType_
{
    ThrowinPass,
    ThrowinShot,
    Pass,
    Shot,
    Cheer
}

public class PlayerAction
{
    public const float ANIMATION_DURATION_KICK = 0.5f;
    public const float ANIMATION_DURATION_THROWIN = 2.0f;
    public const float ANIMATION_DURATION_CHEERING = 4.0f;

    public const float DELAY_KICK = 0.0f;
    public const float DELAY_THROW = 0.5f;

    private float timeStarted;
    private float durationActionDelay;
    private ActionType_ actionType;
    private PlayerAnimation playerAnimation;
    private float power;
    private bool running;
    private Player scriptPlayer;
    private Animator animator;


    public ActionType_ ActionType { get => actionType; set => actionType = value; }
    public float Power { get => power; set => power = value; }
    public PlayerAnimation PlayerAnimation { get => playerAnimation; set => playerAnimation = value; }
    public float TimeStarted { get => timeStarted; set => timeStarted = value; }
    public bool Running { get => running; set => running = value; }

    public PlayerAction(Player scriptPlayer, Animator animator)
    {
        this.scriptPlayer = scriptPlayer;
        this.animator = animator;
    }

    private void HandleAnimation()
    {
        if (playerAnimation != null)
        {
            if (playerAnimation.FadeIn)
            {
                animator.SetLayerWeight(playerAnimation.Layer, Mathf.Lerp(animator.GetLayerWeight(playerAnimation.Layer), 1f, Time.deltaTime * 10f));
            }

            if (Time.time - timeStarted > playerAnimation.Duration)
            {
                if (playerAnimation.FadeOut)
                {
                    animator.SetLayerWeight(playerAnimation.Layer, Mathf.Lerp(animator.GetLayerWeight(playerAnimation.Layer), 0f, Time.deltaTime * 10f));
                }
                else
                {
                    animator.SetLayerWeight(playerAnimation.Layer, 0);
                }

                if (animator.GetLayerWeight(playerAnimation.Layer) == 0)
                {
                    playerAnimation = null;
                }
            }
        }
    }

    public void Set(ActionType_ actionType, float power)
    {
        this.actionType = actionType;
        this.power = power;
        running = true;
        timeStarted = Time.time;
        switch (actionType)
        {
            case ActionType_.Pass:
                scriptPlayer.DoingKick = false;
                durationActionDelay = DELAY_KICK;
                playerAnimation = new PlayerAnimation(animator, ANIMATION_DURATION_KICK, false, true, PlayerAnimation.LAYER_SHOOT, "Shoot");
                break;

            case ActionType_.Shot:
                scriptPlayer.DoingKick = false;
                durationActionDelay = DELAY_KICK;
                playerAnimation = new PlayerAnimation(animator, ANIMATION_DURATION_KICK, false, true, PlayerAnimation.LAYER_SHOOT, "Shoot");
                break;

            case ActionType_.ThrowinPass:
                scriptPlayer.DoingThrow = false;
                durationActionDelay = DELAY_THROW;
                playerAnimation = new PlayerAnimation(animator, ANIMATION_DURATION_THROWIN, false, true, PlayerAnimation.LAYER_THROW_IN, "ThrowIn");
                break;

            case ActionType_.ThrowinShot:
                scriptPlayer.DoingThrow = false;
                durationActionDelay = DELAY_THROW;
                playerAnimation = new PlayerAnimation(animator, ANIMATION_DURATION_THROWIN, false, true, PlayerAnimation.LAYER_THROW_IN, "ThrowIn");
                break;

            case ActionType_.Cheer:
                durationActionDelay = ANIMATION_DURATION_CHEERING;
                playerAnimation = new PlayerAnimation(animator, ANIMATION_DURATION_CHEERING, false, false, PlayerAnimation.LAYER_CHEER, "Cheer");
                break;
        }
    }

    private void Execute()
    {
        running = false;
        scriptPlayer.TransformBall.parent = null;

        switch (actionType)
        {
            case ActionType_.Pass:
                scriptPlayer.TakePass();
                Game.Instance.SetGameState(GameState_.Playing);
                break;

            case ActionType_.Shot:
                scriptPlayer.TakeShot(power);
                Game.Instance.SetGameState(GameState_.Playing);
                break;

            case ActionType_.ThrowinPass:
                scriptPlayer.ScriptBall.Rigidbody.isKinematic = false;
                scriptPlayer.TakePass();
                Game.Instance.SetGameState(GameState_.Playing);
                break;

            case ActionType_.ThrowinShot:
                scriptPlayer.ScriptBall.Rigidbody.isKinematic = false;
                scriptPlayer.TakeShot(power);
                Game.Instance.SetGameState(GameState_.Playing);
                break;

            case ActionType_.Cheer:
                Game.Instance.SetGameState(GameState_.Replay);
                break;
        }
    }


    public void Update()
    {
        if (!Game.Instance.GameState.Equals(GameState_.Replay))
        {
            HandleAnimation();
        }
        if (running && Time.time - timeStarted >= durationActionDelay)
        {
            Execute();
        }
        if (running && (actionType.Equals(ActionType_.ThrowinShot) || actionType.Equals(ActionType_.ThrowinPass)))
        {
            scriptPlayer.TransformBall.position = scriptPlayer.BallHandPosition.position;
        }
    }
}

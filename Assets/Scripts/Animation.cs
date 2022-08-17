using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animation
{
    public const int LAYER_SHOOT = 1;
    public const int LAYER_CHEER = 2;
    public const int LAYER_THROW_IN = 3;

    private Animator animator;
    float timeStarted;
    float duration;
    bool fadeOut;
    bool fadeIn;
    int layer;
    string stateName;

    public Animation(Animator animator, float duration, bool fadeIn, bool fadeOut, int layer, string stateName)
    {
        this.animator = animator;
        timeStarted = Time.time;
        this.duration = duration;
        this.fadeIn = fadeIn;
        this.fadeOut = fadeOut;
        this.layer = layer;
        this.stateName = stateName;

        if (!fadeIn)
        {
            animator.SetLayerWeight(layer, 1);
        }

        animator.Play(stateName, layer, 0f);
    }

    public Animator Animator { get => animator; set => animator = value; }
    public float TimeStarted { get => timeStarted; set => timeStarted = value; }
    public float Duration { get => duration; set => duration = value; }
    public bool FadeOut { get => fadeOut; set => fadeOut = value; }
    public int Layer { get => layer; set => layer = value; }
    public string StateName { get => stateName; set => stateName = value; }
    public bool FadeIn { get => fadeIn; set => fadeIn = value; }
}

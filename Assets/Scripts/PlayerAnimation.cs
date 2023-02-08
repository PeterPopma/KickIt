using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation
{
    public const int LAYER_SHOOT = 1;
    public const int LAYER_CHEER = 2;
    public const int LAYER_THROW_IN = 3;
    public const int LAYER_TACKLE = 4;
    public const int LAYER_FALL = 5;
    public const int LAYER_DIVE_LEFT = 2;
    public const int LAYER_DIVE_RIGHT = 3;
    public const int LAYER_BLOCK_LEFT = 4;
    public const int LAYER_BLOCK_RIGHT = 5;
    public const int LAYER_JUMP = 6;
    public const int LAYER_CATCH = 7;

    private Animator animator;
    float duration;
    bool fadeOut;
    bool fadeIn;
    int layer;
    string name;

    public PlayerAnimation(Animator animator, float duration, bool fadeIn, bool fadeOut, int layer, string name, float startTime=0f)
    {
        this.animator = animator;
        this.duration = duration;
        this.fadeIn = fadeIn;
        this.fadeOut = fadeOut;
        this.layer = layer;
        this.name = name;

        if (!fadeIn)
        {
            animator.SetLayerWeight(layer, 1);
        }

        animator.Play(name, layer, startTime);
    }

    public Animator Animator { get => animator; set => animator = value; }
    public float Duration { get => duration; set => duration = value; }
    public bool FadeOut { get => fadeOut; set => fadeOut = value; }
    public int Layer { get => layer; set => layer = value; }
    public string Name { get => name; set => name = value; }
    public bool FadeIn { get => fadeIn; set => fadeIn = value; }
}

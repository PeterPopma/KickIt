using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Post : MonoBehaviour
{
    private AudioSource soundHitPost;

    private void Awake()
    {
        soundHitPost = GameObject.Find("Sound/hitpost").GetComponent<AudioSource>();
    }

    public void OnCollisionEnter(Collision collision)
    {
        soundHitPost.Play();
    }
}

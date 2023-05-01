using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Post : MonoBehaviour
{
    private AudioSource soundHitPost;

    void Awake()
    {
        soundHitPost = GameObject.Find("Sound/hitpost").GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void OnCollisionEnter(Collision collision)
    {
        soundHitPost.Play();
    }
}

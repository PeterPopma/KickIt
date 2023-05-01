using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StadiumCamera : MonoBehaviour
{
    [SerializeField] Transform ball;


    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(ball.position.x, transform.position.y, ball.position.z + 25);
    }
}

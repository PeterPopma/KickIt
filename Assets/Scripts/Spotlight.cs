using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spotlight : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float x = (Time.time*10) %40 - 20;
        transform.position = new Vector3(x, transform.position.y, transform.position.z);
    }
}

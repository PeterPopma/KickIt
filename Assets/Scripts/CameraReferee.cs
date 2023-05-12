using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraReferee : MonoBehaviour
{
    [SerializeField] GameObject cameraRoot;

    void LateUpdate()
    {
        // make sure the camera is always positioned before the referee
        transform.position = cameraRoot.transform.position;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxRotation : MonoBehaviour
{
public Material skybox;
public GameObject cam;
    void Update()
    {
        cam.transform.Rotate(Vector3.up * Time.deltaTime);
    }
}

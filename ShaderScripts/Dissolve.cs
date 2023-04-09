using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dissolve : MonoBehaviour
{
    Material material;
    public bool isDissolving = false;
    public float fade = 1f;
    bool isForming;
    public bool dissolveDone = false;
    public float colorIntensity = 0.5f;
    public bool charging = false;
    void awake()
    {
        isForming = false;
        fade = 0;
    }

    void Start()
    {
        material = GetComponent<SpriteRenderer>().material;
        isForming = true;
    }

    void OnEnable()
    {
        dissolveDone = false;
        fade = 0;
        isForming = true;
    }

    // void Melting() {
    //     isDissolving = true;
    // }
    void FixedUpdate()
    {
        if (isForming)
            isDissolving = false;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isDissolving = true;
        }
        if (isDissolving)
        {
            fade -= Time.deltaTime;

            if (fade <= 0f)
            {
                fade = 0f;
                dissolveDone = true;
            
                isDissolving = false;
            }

            
        }
        if (isForming)
        {
            fade += Time.deltaTime;
            if (fade >= 1)
            {
                fade = 1;
                isForming = false;
            }
        
        }
        if(charging){
            colorIntensity += Time.deltaTime;
            
        }
        material.SetFloat("_ColorIntensity", colorIntensity);
         material.SetFloat("_Fade", fade);
    }
}

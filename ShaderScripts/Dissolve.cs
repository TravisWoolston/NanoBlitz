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
    void Update()
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
                material.SetFloat("_Fade", fade);
                isDissolving = false;
            }

            material.SetFloat("_Fade", fade);
        }
        if (isForming)
        {
            fade += Time.deltaTime;
            if (fade >= 1)
            {
                fade = 1;
                isForming = false;
            }
            material.SetFloat("_Fade", fade);
        }
         material.SetFloat("_Fade", fade);
    }
}

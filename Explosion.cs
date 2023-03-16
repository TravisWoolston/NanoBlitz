using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float explosionRadius = 5f;
    public float explosionForce = 100f;

    public AudioSource explosionSFX;
    public AudioClip[] explosions;
    public bool sploded = false;

    void OnEnable()
    {
        Debug.Log("Explosion object enabled");
        //  int indexE = Random.Range(0, explosions.Length);
        //         explosionSFX.clip = explosions[indexE];
        //         explosionSFX.pitch = Random.Range(.9f, 1.1f);
        //         explosionSFX.Play();
        //         // Destroy(gameObject);
    }

    void OnStart()
    {
        int indexE = Random.Range(0, explosions.Length);
        explosionSFX.clip = explosions[indexE];
        explosionSFX.pitch = Random.Range(.9f, 1.1f);

        // Destroy(this.gameObject, 2f);
    }
    // void Update() {
    //     if(!sploded)
    //     {
    // explosionSFX.Play();
    // sploded = true;
    //     }

    // }
}

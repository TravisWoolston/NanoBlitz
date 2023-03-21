using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Pathfinding;
using UnityEngine.AI;

public class UM : MonoBehaviour
{
    public float VGZ = 10;
    public static UM Instance;
    public float AICount;
    public VectorGrid VG;
    public float currentTask;
    private float timer = 0;

    // public float AIRemoved = float.MaxValue;
    public bool updateNeeded = false;
    private float delayTime = .7f;
    public float VGForce = .03f;
    public float VGRadius = .03f;
    public float xGrav = 0;
    public float yGrav = 0;
    public AudioClip[] explosions;
    private AudioClip exp;

    public int allies = 0;
    public GameObject[] allyArray;
    public Transform[] allyArrayTransform;
    public GameObject[] enemies;
    public GameObject[] captains;
    public GameObject player;
    public GameObject[] cloneArray;
    public GameObject[] playerArray;
    public GameObject[] combinedAllies;
    public Color color1;
    public Color color2;
    public Color color3;
    public Color color4;
    Camera[] playerCams;
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        playerArray = GameObject.FindGameObjectsWithTag("Player");
        AICount = 1;
        currentTask = 0;
        InvokeRepeating("UpdateGlobal", 0f, 1f);
        VGForce = .03f;
        VGRadius = .03f;
        Physics2D.gravity = new Vector2(xGrav, yGrav);
        cloneArray = GameObject.FindGameObjectsWithTag("Clone");
    }

    public void TaskComplete()
    {
        currentTask++;
    }

    public void NewAI()
    {
        AICount++;
    }

    public void Explosion(Transform splodePlace)
    {
        int indexE = UnityEngine.Random.Range(0, explosions.Length);
        exp = explosions[indexE];

        //      GameObject explosionObject = Instantiate(explosionSource, splodePlace);
        //      explosionObject.SetActive(true);
        // AudioSource explosionAudio = explosionObject.GetComponent<AudioSource>();
        // explosionAudio.clip = exp;
        // explosionAudio.PlayOneShot(exp, 1f);
        // Destroy(explosionObject, 3f);
        AudioSource.PlayClipAtPoint(exp, splodePlace.position);
    }

    // public void RemoveAI(float id){
    //     AICount--;
    //     AIRemoved = id;
    // }
    
    public GameObject GetClosestPlayerGameObject(Vector3 position)
    {
        float distance = Mathf.Infinity;
        Transform closestTransform = null;
        float closestDistance = float.MaxValue;
        GameObject closestGameObject = null;
        foreach (GameObject gameObject in playerArray)
        {
            Transform transform = gameObject.transform;
            distance = (transform.position - position).sqrMagnitude;
            if (distance < closestDistance)
            {
                closestTransform = transform;
                closestDistance = distance;
                closestGameObject = gameObject;
            }
        }
        return closestGameObject;
    }
    public Transform GetClosestTransform(GameObject[] gameObjects, Vector3 position)
    {
        float distance = Mathf.Infinity;
        Transform closestTransform = null;
        float closestDistance = float.MaxValue;
        foreach (GameObject gameObject in gameObjects)
        {
            Transform transform = gameObject.transform;
            distance = (transform.position - position).sqrMagnitude;
            if (distance < closestDistance)
            {
                closestTransform = transform;
                closestDistance = distance;
                // break;
            }
        }
        return closestTransform;
    }

    public GameObject GetClosestGameObject(GameObject[] gameObjects, Vector3 position)
    {
        float distance = Mathf.Infinity;
        Transform closestTransform = null;
        float closestDistance = float.MaxValue;
        GameObject closestGameObject = null;
        foreach (GameObject gameObject in gameObjects)
        {
            Transform transform = gameObject.transform;
            distance = (transform.position - position).sqrMagnitude;
            if (distance < closestDistance)
            {
                closestTransform = transform;
                closestDistance = distance;
                closestGameObject = gameObject;
            }
        }
        return closestGameObject;
    }

    public void AddGridForce(
        Vector3 position,
        float magnitude,
        float radius,
        Color rippleColor,
        bool hasColor
    )
    {
        position.z = VGZ;
        VG.AddGridForce(position, magnitude, radius, rippleColor, hasColor);
    }

    private void UpdateGlobal()
    {
        // GameObject[] alliesArray = GameObject.FindGameObjectsWithTag("Clone");
        cloneArray = GameObject.FindGameObjectsWithTag("Clone");
        
        combinedAllies = new GameObject[cloneArray.Length + playerArray.Length];
        Array.Copy(playerArray, combinedAllies, playerArray.Length);
        Array.Copy(cloneArray, 0, combinedAllies, playerArray.Length, cloneArray.Length);

        allyArray = combinedAllies;
        captains = GameObject.FindGameObjectsWithTag("EnemyCaptain");
        enemies = GameObject.FindGameObjectsWithTag("BasicEnemy");
        allies = allyArray.Length;
        // Create a new array to hold the combined contents
        GameObject[] combinedEnemies = new GameObject[captains.Length + enemies.Length];

        // Copy the contents of the captains array to the combinedEnemies array
        Array.Copy(captains, combinedEnemies, captains.Length);

        // Copy the contents of the enemies array to the combinedEnemies array, starting at the end of the captains array
        Array.Copy(enemies, 0, combinedEnemies, captains.Length, enemies.Length);

        // Use the combinedEnemies array as the new enemies array
        enemies = combinedEnemies;
        allyArrayTransform = new Transform[allies];
        for (int i = 0; i < allies; i++)
        {
            allyArrayTransform[i] = allyArray[i].transform;
        }
    }

    public float GetPlayerID(){
        playerArray = GameObject.FindGameObjectsWithTag("Player");
        return playerArray.Length;
    }
    void FixedUpdate()
    {

        VGZ = VG.transform.position.z;
    }
}

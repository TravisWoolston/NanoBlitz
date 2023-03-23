using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using UnityEngine.Networking;
public class EnemyBasic : NetworkBehaviour
{
    public GameObject player;
    Transform playerT;
    PlayerController playerC;
    public GameObject basicAllyPrefab;

    public float maxSpeed = 100f;
    public float moveSpeed = 40f;
    public float acceleration = 5f;
    public float accTimer;

    public Rigidbody2D rb;
    Vector3 positionWithMostAllies;
    Vector2 moveDirection;
    public Vector2 target;
    Vector2 DSTarget;
    Vector2 fireTarget;
    Vector2 mousePosition;
    Vector3 playerPosition;
    public float timer = 0.0f;
    public float hp = 0;
    private float hpCheck = 0;
    public GameObject[] playerAllies;
    private float dmgRate = .05f;
    private bool isCaptain = false;
    private GameObject[] captains;

    // private GameObject[] enemies;
    public bool rallied = false;

    SpriteRenderer sprite;
    private float cDist;
    private float pDist;
    public ParticleSystem sparks;
    public float AIID;
    private float AIRemoved = float.MaxValue;
    UM uM;

    private bool queUpdate = false;
    float AIQueLength = 0;
    float lastRemoved = 0;
    public Transform closestPlayerAlly;
    float distanceThreshold = 50f;
    bool relocating = false;
    float fireRange = 100;
    public GameObject closestCaptain;
    private Transform closestCaptainT;

    public EnemyWeapon weapon;
    private float fireRate;
    private GameObject closestAllyObject;
    float rotationSpeed = 2f;
    public AIDestinationSetter DS;
    public AIPath aIPath;
    Path path;
    public float nextWaypointDistance = 8f;
    int currentWaypoint = 0;
    bool reachedEndOfPath = false;
    Seeker seeker;

    public Vector2 goTo;
    private GridGraph gridGraph;
    public GameObject engineParticles;
    GameObject engine;
    public Transform exhaustPoint;

    public AudioSource bulletAudio;
    public AudioClip[] lasers;
    Color color;
    public float magnitude = 0;
    float thrustPower;
    float VGZ;
    Vector3 VGPos;
    float VGRadius = 1;
    Transform[] playerAlliesT;
    float pathTimer = 0;
    public Dissolve Dissolve;
    Transform rbTransform;
    float allySearchRadius = 10;
    public GameObject rallyCpt;
    Transform cptTransform;
    Vector2 velocity;
    public GameObject prefab;
    void Start()
    {
        uM = UM.Instance;
        // player = GameObject.FindGameObjectsWithTag("Player")[0];
        // playerT = player.transform;
        // playerC = player.GetComponent<PlayerController>();
        VGZ = uM.VGZ;
        playerAllies = uM.allyArray;
        gridGraph = AstarPath.active.data.gridGraph;
        if (this.gameObject.tag == "EnemyCaptain")
        {
            VGRadius = 5;
            rb.angularDrag = 1000;
            dmgRate = .01f;
            rb.drag = 3;
            moveSpeed = .8f;
            rb.mass = 999999;
            isCaptain = true;
            fireRate = 2;
            rotationSpeed = .5f;
            InvokeRepeating("UpdateBounds", 0f, 1f);
            InvokeRepeating("UpdateCaptainTarget", 0f, 2f);
            fireRange = 100;
        }
        else
        {
            rb.angularDrag = 100;
            rb.drag = 1f;
            rb.gravityScale = 1;
            rb.mass = 50;
            fireRate = .3f;
            seeker = GetComponent<Seeker>();
            InvokeRepeating("UpdatePath", 0f, 1f);
            InvokeRepeating("UpdateBounds", 0f, 5f);

            DS = GetComponent<AIDestinationSetter>();
            engine = Instantiate(engineParticles, exhaustPoint);
            bulletAudio.pitch = Random.Range(.9f, 1.1f);
            InvokeRepeating("UpdateTarget", 0f, .7f);
        }
        InvokeRepeating("UpdateAllyArrays", 0f, 1f);
        thrustPower = rb.mass * 100;
        rbTransform = rb.transform;
        sprite = this.GetComponent<SpriteRenderer>();
        sprite.color = new Color(1, 0, 0);
        color = sprite.color;
        // enemies = uM.enemies;
    }

    void OnEnable()
    {
        //         if (uM.playerArray.Length > 0){
        //  player = uM.GetClosestPlayerGameObject(rbTransform.position);
        //         playerT = player.transform;
        //         playerC = player.GetComponent<PlayerController>();
        //         }

        if (!isCaptain)
        {
            rb.drag = 1;
            rallied = false;
        }
    }

    void UpdateAllyArrays()
    {
        uM = UM.Instance;
        playerAllies = FilterEnemiesByDistance(uM.allyArray, transform.position, fireRange);
        playerAlliesT = FilterEnemiesByDistanceTransform(
            uM.allyArrayTransform,
            transform.position,
            fireRange
        );
    }

    void UpdateBounds()
    {
        Bounds bounds = GetComponent<Collider2D>().bounds;
        AstarPath.active.UpdateGraphs(bounds, 2f);
    }

    void UpdatePath()
    {
        if (seeker.IsDone() || pathTimer > 10)
        {
            pathTimer = 0;
            seeker.StartPath(transform.position, target, OnPathComplete);
        }
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Bullet")
        {
            hp += dmgRate;
        }
    }

    GameObject[] FilterEnemiesByDistance(GameObject[] enemies, Vector3 origin, float distance)
    {
        List<GameObject> filteredEnemies = new List<GameObject>();

        for (int i = 0; i < enemies.Length; i++)
        {
            if (Vector3.Distance(enemies[i].transform.position, origin) <= distance)
            {
                filteredEnemies.Add(enemies[i]);
            }
        }

        return filteredEnemies.ToArray();
    }

    Transform[] FilterEnemiesByDistanceTransform(
        Transform[] enemies,
        Vector3 origin,
        float distance
    )
    {
        List<Transform> filteredEnemies = new List<Transform>();

        for (int i = 0; i < enemies.Length; i++)
        {
            if (Vector3.Distance(enemies[i].transform.position, origin) <= distance)
            {
                filteredEnemies.Add(enemies[i]);
            }
        }

        return filteredEnemies.ToArray();
    }

    IEnumerator DelayedDisable()
    {
        
        yield return new WaitForSeconds(0f);
        if (gameObject.tag == "EnemyCaptain")
        {
            for (int i = 0; i < 5; i++)
            {
                uM.spawnCloneServerRpc(basicAllyPrefab, transform);
            }
        }
        else
        {
            uM.spawnCloneServerRpc(basicAllyPrefab, transform);
            
        }

        hp = 0;

        // ParticleSystem spark = Instantiate(sparks, this.transform.position, Quaternion.identity);
        // Destroy(spark, 2f);
        Bounds bounds = GetComponent<Collider2D>().bounds;

        GridGraph graph = AstarPath.active.graphs[0] as GridGraph;
        // uM.AddGridForce(transform.position, 30, Color.red);
        // uM.Explosion(transform);
        UpdateBounds();
        GraphUpdateObject guo = new GraphUpdateObject(bounds);
        guo.modifyTag = true;
        guo.setTag = 0; // set the tag to the default value to mark the nodes as walkable

        AstarPath.active.UpdateGraphs(guo);
         NetworkObjectPool.Singleton.ReturnNetworkObject(NetworkObject, prefab);
        // gameObject.SetActive(false);
        
    }

    GameObject GetClosestGameObject(GameObject[] gameObjects, Vector3 position)
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

    Transform GetClosestTransform(GameObject[] gameObjects, Vector3 position)
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

    Vector3 Accelerator(Vector3 inputDirection)
    {
        moveDirection = inputDirection;
        if (accTimer < 5)
        {
            accTimer += Time.deltaTime;
        }
        if (accTimer < 2)
        {
            accTimer++;
        }
        float targetSpeed = moveSpeed * accTimer;
        float clampedSpeed = Mathf.Clamp(targetSpeed, 0, maxSpeed);
        float smoothAccel = Mathf.SmoothStep(0, clampedSpeed, acceleration * Time.deltaTime);

        return moveDirection * (smoothAccel / 5);
    }

    void UpdateCaptainTarget()
    {
        if (player == null)
            return;
        captains = uM.captains;
        playerAllies = FilterEnemiesByDistance(uM.allyArray, transform.position, fireRange);
        playerPosition = playerC.position;
        closestPlayerAlly = GetClosestTransform(playerAllies, transform.position);

        // Count the number of player allies within a certain radius of each position
        Dictionary<Vector3, int> allyCountByPosition = new Dictionary<Vector3, int>();
        foreach (Transform ally in playerAlliesT)
        {
            Vector3 allyPos = ally.position;
            int allyCount = 0;

            foreach (Transform otherAlly in playerAlliesT)
            {
                if (otherAlly == ally)
                    continue;
                if (Vector3.Distance(otherAlly.position, allyPos) <= allySearchRadius)
                {
                    allyCount++;
                }
            }
            allyCountByPosition[allyPos] = allyCount;
        }

        // Find the position with the most player allies around it
        positionWithMostAllies = transform.position;
        int maxAllyCount = 0;
        foreach (KeyValuePair<Vector3, int> pair in allyCountByPosition)
        {
            if (pair.Value > maxAllyCount)
            {
                maxAllyCount = pair.Value;
                positionWithMostAllies = pair.Key;
            }
        }

        if (closestPlayerAlly == null)
        {
            target = playerPosition;
        }
        else
        {
            target = closestPlayerAlly.position;
            fireTarget = positionWithMostAllies; // Set fireTarget to position with most allies
        }
        Vector2 fireDirection = fireTarget - rb.position;
        fireDirection.Normalize();
        float dot = Vector3.Dot(transform.up, fireDirection.normalized);
        if (timer > fireRate && dot > .5 && playerAllies.Length > 0 && isCaptain)
        {
            closestAllyObject = GetClosestGameObject(playerAllies, transform.position);
            weapon.FireEM(positionWithMostAllies);
            timer = 0;
        }
    }

    void UpdateTarget()
    {
        if (player == null) return;
        captains = uM.captains;

        playerPosition = playerC.position;
        playerAllies = FilterEnemiesByDistance(uM.allyArray, transform.position, fireRange);
        closestPlayerAlly = GetClosestTransform(playerAllies, transform.position);

        if (captains.Length > 0 && !rallied && !isCaptain)
        {
            float playerDistance = ((Vector3)playerPosition - transform.position).sqrMagnitude;
            if (closestCaptain == null || !closestCaptain.activeSelf)
            {
                closestCaptain = GetClosestGameObject(captains, transform.position);
                closestCaptainT = closestCaptain.GetComponent<Rigidbody2D>().transform;
            }

            if (playerDistance < (closestCaptainT.position - transform.position).sqrMagnitude)
            {
                target = playerPosition;
            }
            else
            {
                target = closestCaptainT.position;
                rallyCpt = closestCaptain;
                cptTransform = rallyCpt.transform;
                rallied = true;
            }
        }
        if (rallied)
        {
            float playerDistance = ((Vector3)playerPosition - transform.position).sqrMagnitude;
            if (!closestCaptain.activeSelf)
            {
                rallied = false;
            }
            else
            {
                target = closestCaptainT.position;
                DS.target = closestCaptainT;
                if (closestPlayerAlly != null)
                {
                    fireTarget = closestPlayerAlly.position;
                }
            }
        }
        else
        {
            if (closestPlayerAlly == null)
            {
                target = playerPosition;
            }
            else
            {
                target = closestPlayerAlly.position;
                fireTarget = closestPlayerAlly.position;
            }
        }
    }

    void FixedUpdate()
    {
        if (player == null && uM.playerArray.Length > 0)
        {
            player = uM.GetClosestPlayerGameObject(gameObject.transform.position);

            playerT = player.transform;
            playerC = player.GetComponent<PlayerController>();
        }
    
        velocity = rb.velocity;
        if (Dissolve.dissolveDone)
        {
            StartCoroutine(DelayedDisable());
        }
        if (hp > 1)
        {
            Dissolve.isDissolving = true;
        }
        pathTimer += Time.deltaTime;
        fireRange = (100f + uM.allies);
        if (!isCaptain && path != null)
        {
            if (path.vectorPath != null)
            {
                if (currentWaypoint >= path.vectorPath.Count)
                {
                    currentWaypoint = 0;
                    return;
                }
                if (path.vectorPath.Count > 2 && rallied)
                    aIPath.canMove = true;
                else
                    aIPath.canMove = false;
                float nextDistance = Vector2.Distance(
                    rb.position,
                    path.vectorPath[currentWaypoint]
                );
                if (nextDistance < nextWaypointDistance)
                {
                    currentWaypoint++;
                }
                else
                {
                    reachedEndOfPath = false;
                }
            }
        }

        if (hp != hpCheck)
        {
            hpCheck = hp;
            sprite.color = new Color(255, hp, 0);
        }
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;

        if (!isCaptain && path != null)
        {
            if (path.vectorPath != null)
            {
                if (path.vectorPath != null)
                {
                    if (currentWaypoint >= path.vectorPath.Count)
                    {
                        currentWaypoint = 0;
                    }
                    goTo = path.vectorPath[currentWaypoint];
                }
            }
        }
        else
        {
            goTo = target;
        }
        timer += Time.deltaTime;
        var offset = 90f;
        Vector2 targetDirection = goTo - rb.position;
        targetDirection.Normalize();
        float targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;

        Vector2 fireDirection = fireTarget - rb.position;
        fireDirection.Normalize();
        float fireAngle = Mathf.Atan2(fireDirection.y, fireDirection.x) * Mathf.Rad2Deg;

        Quaternion targetRotation;
        if (
            Vector2.Distance(rb.position, target) > distanceThreshold
            && Vector2.Distance(rb.position, fireTarget) > fireRange
        )
        {
            targetRotation = Quaternion.Euler(Vector3.forward * (targetAngle - offset));
        }
        else
        {
            targetRotation = Quaternion.Euler(Vector3.forward * (fireAngle - offset));
        }

        if (transform.rotation != targetRotation)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed
            );
        }
        else
        {
            if (Vector2.Distance(rb.position, target) > distanceThreshold)
            {
                if (rallied)
                {
                    if (Vector2.Distance(rb.position, cptTransform.position) > distanceThreshold)
                    {
                        rb.velocity = rallyCpt.GetComponent<Rigidbody2D>().velocity;
                    }
                }
                else
                    rb.AddForce(Accelerator(rbTransform.up) * thrustPower);
            }
            else
            {
                rb.drag = 4;
                if (accTimer > 0)
                {
                    accTimer -= Time.deltaTime;
                }
            }
        }
        float dot = Vector3.Dot(transform.up, fireDirection.normalized);
        if (
            timer > fireRate
            && dot > .9
            && playerAllies.Length > 0
            && gameObject.tag == "BasicEnemy"
        )
        {
            int index = Random.Range(0, lasers.Length);
            bulletAudio.clip = lasers[index];
            bulletAudio.pitch = Random.Range(.9f, 1.1f);
            bulletAudio.Play();
            weapon.Fire();
            timer = 0;
        }
    }

    void Update()
    {
        if(uM.playerArray.Length == 0) return;
        magnitude = rb.velocity.magnitude;

        if (magnitude > 30)
        {
            VGPos = rb.position;
            VGPos.z = VGZ;
            if (magnitude > 80)
                magnitude = 80;
            uM.VG.AddGridForce(VGPos, magnitude * uM.VGForce, 1, color, true);
        }
    }
}

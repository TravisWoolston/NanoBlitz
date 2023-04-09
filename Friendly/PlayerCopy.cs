using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Networking;

public class PlayerCopy : NetworkBehaviour
{
    public GameObject player;
    Transform playerT;
    public PlayerController playerC;
    public float maxSpeed = 100f;
    public float moveSpeed = 40f;
    public float acceleration = 5f;
    public float accTimer;
    public Quaternion targetRotation;
    public Vector2 target;
    public Vector2 fireTarget;
    public Rigidbody2D rb;
    public Weapon weapon;
    Vector2 moveDirection;
    Vector2 mousePosition;

    private float timer = 0.0f;
    public float hp = 1;
    public float hpCheck;
    public float allies;
    public float allyCheck = 0;
    public UM uM;
    public float cloneNumber = 0;
    private bool hasMomentum = false;
    private GameObject[] enemies;

    SpriteRenderer sprite;

    float curDistance = 0;
    public GameObject closestBasicEnemy;
    float distanceThreshold;
    float fireThreshold;
    public GameObject engineParticles;
    GameObject engine;
    public Transform exhaustPoint;
    public ParticleSystem explosion;
    private float offset = 90f;
    public Vector2 predictedTarget;
    private Vector2 targetDirection;
    float distance = 0;
    float time;
    float fireRange = 120f;
    public AudioSource bulletAudio;
    public AudioClip[] lasers;
    Color color;
    public float magnitude = 0;
    Transform rbTransform;
    float thrustPower;
    float VGZ;
    Vector3 VGPos;
    public bool rallied = true;
    Transform enemyRBTransform;
    public Dissolve Dissolve;
    public VectorGrid VGShield;

    // VectorGrid VGShield;

    bool activeShield = true;
    float VGShieldTimer = 0;
    GameObject[] playerArray;
    public GameObject prefab;
    public Transform firePoint;
    public bool reclaimed = false;

    void Awake()
    {
        uM = UM.Instance;
    }

    public override void OnNetworkSpawn()
    {
        //  uM = UM.Instance;
        player = uM.GetClosestPlayerGameObject(transform.position);
        playerT = player.transform;
        playerC = player.GetComponent<PlayerController>();
        uM.UpdateTeams(playerC, gameObject);
        VGShieldTimer = 0;
        activeShield = true;
        // rallied = false;
        hp = 1;
    }
    public override void OnNetworkDespawn(){
        uM.RemoveUpdateTeams(playerC, gameObject);
    }
    void Start()
    {
        uM = UM.Instance;
        // activeShield = true;
        // player = GameObject.FindGameObjectsWithTag("Player")[0];
        // playerT = player.transform;
        // playerC = player.GetComponent<PlayerController>();
        weapon.hasMissiles = false;
        VGZ = uM.VGZ;
        allies = uM.allies;
        enemies = uM.enemies;
        sprite = this.GetComponent<SpriteRenderer>();
        sprite.color = new Color(1 - hp, 1 - hp, 1 - hp);
        color = Color.blue;
        cloneNumber = uM.allies;
        rb.angularDrag = 10;
        rb.drag = 1.5f;
        rb.gravityScale = 1;
        rb.mass = 100;
        thrustPower = rb.mass * 100;
        // engine = Instantiate(engineParticles, exhaustPoint);
        InvokeRepeating("UpdateTarget", 0f, .2f);
        rbTransform = rb.transform;

        // bulletAudio = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        
        VGShieldTimer = 0;
        // activeShield = true;
        // rallied = false;
        hp = 1;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "EnemyBullet" && !activeShield)
        {
            hp -= .0314f;
        }
        if (collision.gameObject.tag == "HammerHead" && !activeShield)
        {
            hp -= 1f;
        }
        if(collision.gameObject.tag == "Bullet" ){
            if(collision.gameObject.GetComponent<Bullet>().teamID != playerC.playerID.Value){
                hp -= .0314f;
            }
        }
    }

    Vector3 Accelerator(Vector3 inputDirection)
    {
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

        return inputDirection * ((Vector2.Distance(rb.position, target) / 10) + smoothAccel / 5);
    }

    IEnumerator DelayedDisable()
    {
        yield return new WaitForSeconds(.2f);
        uM.Explosion(transform);
        hp = 1;
        ParticleSystem spark = Instantiate(explosion, this.transform.position, Quaternion.identity);
        Destroy(spark, 2f);
        //  uM.despawn(prefab);
        // gameObject.SetActive(false);
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

    void UpdateTarget()
    {
        allies = uM.allies;
        distanceThreshold = playerC.distanceThreshold;

        fireThreshold = (70f + allies) * playerC.hp;

        distance = Mathf.Infinity;
        Vector3 position = transform.position;
        closestBasicEnemy = uM.GetClosestEnemyGameObjectDic(playerC.enemyDic,transform.position);

        if (closestBasicEnemy != null)
        {
            enemyRBTransform = closestBasicEnemy.transform;
            if (!closestBasicEnemy.activeSelf)
            {
                closestBasicEnemy = uM.GetClosestEnemyGameObjectDic(playerC.enemyDic, transform.position);
            }

            fireTarget = enemyRBTransform.position;

            target = playerT.position;
        }
        else
        {
            // sprite.color = playerC.playerColor;
            rallied = true;
            target = playerT.position;
        }
        engineParticles.GetComponent<ParticleSystem>().startColor = playerC.playerColor.Value;
    }

    [ClientRpc]
    public void despawnClientRpc()
    {
        NetworkObjectPool.Singleton.ReturnNetworkObject(NetworkObject, prefab);
        if (NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }

    [ServerRpc(RequireOwnership = false)]
    public void despawnServerRpc()
    {
        if (NetworkObject.IsSpawned)
            NetworkObject.Despawn();
        NetworkObjectPool.Singleton.ReturnNetworkObject(NetworkObject, prefab);
        despawnClientRpc();
    }

    void FixedUpdate()
    {
        VGShieldTimer += Time.deltaTime;
        if (VGShieldTimer > 3 && activeShield)
        {
            VGShield.GetComponent<MeshRenderer>().enabled = false;
            activeShield = false;
        }
        else if (!VGShield && activeShield)
        {
            VGShield.GetComponent<MeshRenderer>().enabled = true;
        }
        if (!NetworkManager.Singleton.IsServer)
            return;
        if (player == null && uM.playerArray.Length > 0 && reclaimed)
        {
            player = uM.GetClosestPlayerGameObject(gameObject.transform.position);
            playerT = player.transform;
            playerC = player.GetComponent<PlayerController>();
            allies = playerC.allies;
        }
        if (player == null)
            return;

        if (Vector2.Distance(playerT.position, rbTransform.position) < distanceThreshold + 5)
        {
            if (rb.velocity.magnitude < playerC.velocity.magnitude)
                rb.velocity = (Vector2)playerC.velocity;
        }
        magnitude = rb.velocity.magnitude;

        if (Dissolve.dissolveDone)
        {
            StartCoroutine(DelayedDisable());

            despawnServerRpc();
            // despawnClientRpc();
            // NetworkObjectPool.Singleton.ReturnNetworkObject(NetworkObject, prefab);
        }

        if (hp <= 0)
        {
            Dissolve.isDissolving = true;
        }
        if (hp != hpCheck)
        {
            hpCheck = hp;
            // Dissolve.fade = hp;
            // sprite.color = new Color(1 - hp, 1 - hp, 1 - hp);
        }
        if (rallied)
            sprite.color = Color.blue;
        timer += Time.deltaTime;

        distance = Vector2.Distance(rbTransform.position, target);
        Vector2 targetDirection = target - rb.position;
        targetDirection.Normalize();
        float targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
        if (rb.velocity.magnitude == 0)
        {
            time = distance / 0.01f;
        }
        else
            distance = Vector2.Distance(rbTransform.position, fireTarget);
        if (enemies != null)
            if (playerC.firing)
            {
                predictedTarget = playerC.VGMouse;
            }
            else
            {
                time = distance / 80;
                if (closestBasicEnemy != null)
                    predictedTarget =
                        fireTarget + closestBasicEnemy.GetComponent<Rigidbody2D>().velocity * time;
            }

        Vector2 fireDirection = predictedTarget - rb.position;
        fireDirection.Normalize();
        float fireAngle = Mathf.Atan2(fireDirection.y, fireDirection.x) * Mathf.Rad2Deg;
        // Quaternion targetRotation;

        if (Vector2.Distance(rb.position, target) > distanceThreshold)
        {
            if (Vector2.Distance(playerT.position, transform.position) > distanceThreshold * .9)
            {
                rb.AddForce(Accelerator(rbTransform.up) * (thrustPower / 10));
            }
            targetRotation = Quaternion.Euler(Vector3.forward * (targetAngle - offset));
        }
        else
        {
            targetRotation = Quaternion.Euler(Vector3.forward * (fireAngle - offset));
        }
        if (transform.rotation != targetRotation)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 2f);
        }
        else
        {
            if (Vector2.Distance(rb.position, target) > distanceThreshold)
            {
                if (Vector2.Distance(playerT.position, transform.position) > distanceThreshold * .9)
                {
                    rb.AddForce(Accelerator(rbTransform.up) * (thrustPower / 10));
                }
            }
            else
            {
                if (accTimer > 0)
                {
                    accTimer -= Time.deltaTime;
                }
            }
        }

        float dot = Vector3.Dot(transform.up, fireDirection.normalized);

        if (timer > .3f && dot > .9 && enemies.Length > 0)
        {
            // bulletAudio.loop = true;
            int index = Random.Range(0, lasers.Length);
            bulletAudio.clip = lasers[index];
            bulletAudio.pitch = Random.Range(.9f, 1.1f);
            bulletAudio.Play();
            timer = 0;
            uM.spawnBulletServerRpc(firePoint.position, firePoint.rotation, playerC.playerID.Value);
        }
    }

    void Update()
    {
        if (!IsServer)
            return;
        if (
            magnitude > 30
            && Vector2.Distance(playerT.position, rbTransform.position) > distanceThreshold
        )
        {
            VGPos = rb.position;
            VGPos.z = VGZ;
            // uM.VG.AddGridForce(VGPos, 0, 3, color, true);
        }
    }

    void LateUpdate()
    {
        if (!IsServer)
            return;
        if (VGShield.GetComponent<MeshRenderer>().enabled)
        {
            VGShield.transform.rotation = Quaternion.Euler(
                0.0f,
                0.0f,
                gameObject.transform.rotation.z * -1.0f
            );
            // Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 5);

            // foreach (Collider2D hit in colliders)
            // {
            //     Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
            //     if (rb != null)
            //     {
            //         if (rb.tag != this.tag || rb.tag != "Bullet")
            //         {
            //             Transform colTransform = rb.transform;
            //             Vector3 colVec = new Vector3(
            //                 colTransform.position.x,
            //                 colTransform.position.y,
            //                 VGShield.transform.position.z
            //             );
            //             VGShield.AddGridForce(colVec, 1f, .3f, Color.red, false);
            //         }
            //     }
            // }
        }
    }
}

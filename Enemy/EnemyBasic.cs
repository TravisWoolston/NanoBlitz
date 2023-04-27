using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

using Unity.Netcode;
using UnityEngine.Networking;

public class EnemyBasic : NetworkBehaviour
{
    public Material shaderMaterial;
    public GameObject player;
    public Vector3 fireDirection;
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

    public Vector2 fireTarget;
    Vector2 mousePosition;
    Vector3 playerPosition;
    public float timer = 0.0f;
    public float hp = 1;
    private float hpCheck = 1;
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

    private float AIRemoved = float.MaxValue;
    UM uM;
  
Collider2D[] targets;
    private bool queUpdate = false;
    float AIQueLength = 0;
    float lastRemoved = 0;
    public GameObject closestPlayerObject;
    float distanceThreshold = 50f;
    bool relocating = false;
    float fireRange = 100;
    public GameObject closestCaptain;
    private Transform closestCaptainT;

    // public EnemyWeapon weapon;
    private float fireRate;
    private GameObject closestAllyObject;
    float rotationSpeed = 2f;

    // Path path;
    public float nextWaypointDistance = 8f;
    int currentWaypoint = 0;
    // bool reachedEndOfPath = false;
    // Seeker seeker;
    public float targetDirectionDot;
    public Vector2 goTo;
    private GridGraph gridGraph;
    public GameObject engineParticles;
    GameObject engine;
    public Transform exhaustPoint;
    public AudioSource chargeAudio;
    public AudioSource bulletAudio;
    public AudioClip[] lasers;
    Color color;
    public float magnitude = 0;
    float thrustPower;
    float VGZ;
    Vector3 VGPos;
    float VGRadius = 1;
    Transform[] playerAlliesT;
    public Dissolve Dissolve;
    Transform rbTransform;
    float allySearchRadius = 10;
    public GameObject rallyCpt;
    Transform cptTransform;
    public Vector2 velocity;
    public GameObject prefab;
    public Transform firePoint;
    public float enemyID = 0;
    Vector3 initialScale;
      public LineRenderer hpDrain;
      public bool absorbed = false;
    void Awake(){
        uM = UM.Instance;
    }
  public override void OnNetworkSpawn() {
    if(IsServer)
        uM.UpdateEnemyTeam(gameObject);
                //  uM = UM.Instance;
        
        // rallied = false;
        if(this.gameObject.tag == "HammerHead"){
            hp = 5;
        }
        if(this.gameObject.tag == "BasicEnemy"){
            hp = 1;
        }
       }
       public override void OnNetworkDespawn(){
        if(IsServer)
        uM.RemoveUpdateEnemyTeam(gameObject);
       }
    void Start()
    {
        
        // player = GameObject.FindGameObjectsWithTag("Player")[0];
        // playerT = player.transform;
        // playerC = player.GetComponent<PlayerController>();
        VGZ = uM.VGZ;
        playerAllies = uM.allyArray;
        
        if (this.gameObject.tag == "EnemyCaptain")
        {   
            
            VGRadius = 5;
            rb.angularDrag = 1000;
            dmgRate = .01f;
            rb.drag = 3;
            moveSpeed = 20f;
            rb.mass = 500;
            isCaptain = true;
            fireRate = 2;
            rotationSpeed = .5f;
            hp = 3;
            InvokeRepeating("UpdateCaptainTarget", .1f, .5f);
            fireRange = 100;
        }
        else if(this.gameObject.tag == "HammerHead"){
            dmgRate = .01f;
            moveSpeed = 80;
            rotationSpeed = .7f;
            // InvokeRepeating("UpdateHHTarget", .1f, .7f);
            fireRate = 5;
        }
        else
        {
            rb.angularDrag = 100;
            rb.drag = 1f;
            rb.gravityScale = 1;
            rb.mass = 50;
            fireRate = .3f;
            // seeker = GetComponent<Seeker>();



            bulletAudio.pitch = Random.Range(.9f, 1.1f);
            InvokeRepeating("UpdateTarget", .1f, .7f);
        }
        InvokeRepeating("UpdateAllyArrays", 0f, 1f);
        thrustPower = rb.mass * 10;
        rbTransform = rb.transform;
        sprite = this.GetComponent<SpriteRenderer>();
        // sprite.color = new Color(1, 0, 0);
        color = sprite.color;
        // weapon.parent = this;
        initialScale = rbTransform.localScale;
    }

    void OnEnable()
    {
        
                if (uM.playerArray.Length > 0){
         player = uM.GetClosestPlayerGameObject(transform.position);
                playerT = player.transform;
                playerC = player.GetComponent<PlayerController>();
                }

        if (!isCaptain)
        {
            rb.drag = 1;
            rallied = false;
        }
    }

    void UpdateAllyArrays()
    {
        uM = UM.Instance;
        // playerAllies = FilterEnemiesByDistance(uM.allyArray, transform.position, fireRange);
        playerAllies = uM.allyArray;
        playerAlliesT = uM.allyArrayTransform;
            
        
        // playerAlliesT = FilterEnemiesByDistanceTransform(
        //     uM.allyArrayTransform,
        //     transform.position,
        //     fireRange
        // );
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(gameObject.tag == "HammerHead"){
            collision.gameObject.GetComponent<Rigidbody2D>().AddForce(rb.velocity);
            
            
        }
        if (collision.gameObject.tag == "Bullet")
        {
            hp -= .01f;
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

    public void Despawn()
    {
        if (gameObject.tag == "EnemyCaptain")
        {
            for (int i = 0; i < 5; i++)
            {
                uM.spawnCloneServerRpc(transform.position, transform.rotation);
            }
        }
        else
        {
            if(!absorbed)
            uM.spawnCloneServerRpc(transform.position, transform.rotation);
        }

        despawnServerRpc();

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
            fireRange = (150f + uM.allies);
        captains = uM.captains;
        playerAllies = uM.allyArray;
        // playerAllies = FilterEnemiesByDistance(uM.allyArray, transform.position, fireRange);
        playerPosition = playerC.position;
        closestPlayerObject = GetClosestGameObject(playerAllies, transform.position);

        // Count the number of player allies within a certain radius of each position
        Dictionary<Vector3, int> allyCountByPosition = new Dictionary<Vector3, int>();
       if(playerAlliesT.Length > 0)
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

        if (closestPlayerObject == null)
        {
            target = playerPosition;
        }
        else
        {
            target = closestPlayerObject.transform.position;
            fireTarget = positionWithMostAllies; // Set fireTarget to position with most allies
        }
        Vector2 fireDirection = fireTarget - rb.position;
        fireDirection.Normalize();
        float dot = Vector3.Dot(transform.up, fireDirection.normalized);
        if (timer > fireRate && dot > .5 && playerAllies.Length > 0 && isCaptain)
        {
            fireTarget = positionWithMostAllies;
            closestAllyObject = GetClosestGameObject(playerAllies, transform.position);
            uM.spawnEM(firePoint.position, transform.rotation, NetworkObject);
            timer = 0;
        }
    }

    void UpdateTarget()
    {
        
        if (player == null)
            return;
            fireRange = (100f + uM.allies);
        captains = uM.captains;

        playerPosition = playerC.position;
        playerAllies = uM.allyArray;
        // playerAllies = FilterEnemiesByDistance(uM.allyArray, transform.position, fireRange);
        closestPlayerObject = GetClosestGameObject(playerAllies, transform.position);
        if(this.gameObject.tag != "HammerHead" && this.gameObject.tag != "EnemyCaptain")
        if (captains.Length > 0 && !rallied)
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

                if (closestPlayerObject != null)
                {
                    fireTarget = closestPlayerObject.transform.position;
                }
            }
        }
        else
        {
            if (closestPlayerObject == null)
            {
                target = playerPosition;
            }
            else
            {
                target = closestPlayerObject.transform.position;
                fireTarget = closestPlayerObject.transform.position;
            }
        }
                float dot = Vector3.Dot(transform.up, fireDirection.normalized);
        if (
            timer > fireRate
            && dot > .9
            && playerAllies.Length > 0
        )
        {
            int index = Random.Range(0, lasers.Length);
            bulletAudio.clip = lasers[index];
            bulletAudio.pitch = Random.Range(.9f, 1.1f);
            bulletAudio.Play();
            uM.spawnEnemyBulletServerRpc(firePoint.position, transform.rotation);
            timer = 0;
        }
    }
void UpdateHHTarget()
    {
        
        if (player == null)
            return;
            fireRange = (300f);
        captains = uM.captains;

        playerPosition = playerC.position;
        // playerAllies = FilterEnemiesByDistance(uM.allyArray, transform.position, fireRange);
        closestPlayerObject = GetClosestGameObject(uM.allyArray, transform.position);
        // playerAllies = FilterEnemiesByDistance(uM.allyArray, transform.position, fireRange);
        // closestPlayerObject = GetClosestGameObject(playerAllies, transform.position);

        {
            if (closestPlayerObject == null)
            {
                target = playerPosition;
            }
            else
            {
                target = closestPlayerObject.transform.position;
                fireTarget = closestPlayerObject.transform.position;
            }
        }
                float dot = Vector3.Dot(transform.up, fireDirection.normalized);
        if (
            timer > fireRate
            && dot > .9
            && playerAllies.Length > 0

        )
        {
            if(!Dissolve.charging){
                Dissolve.charging = true;
                chargeAudio.loop = true;
            chargeAudio.Play();
            } 
            
            
  
        }

            if(Dissolve.colorIntensity > 2 || !Dissolve.charging){
                chargeAudio.volume = Mathf.MoveTowards(chargeAudio.volume, 0f, Time.deltaTime * 8f);
            } 
            if(Dissolve.charging) {
                chargeAudio.volume = Mathf.MoveTowards(chargeAudio.volume, .2f, Time.deltaTime *5f);
            }
        
    }
    void Upgrade()
    {
        // Debug.DrawLine(
        //     rbTransform.position,
        //     new Vector3(rbTransform.position.x, rbTransform.position.y + distanceThreshold, 0)
        // );
        targets = Physics2D.OverlapCircleAll(rbTransform.position, distanceThreshold);
        hpDrain.enabled = true;
        NetworkObject targetObject = null;
        float mostHp = -1;
        foreach (Collider2D tgt in targets)
        {
            NetworkObject go = tgt.gameObject.GetComponent<NetworkObject>();
            if (go.tag == "BasicEnemy")
            {
                EnemyBasic playerCopy = go.GetComponent<EnemyBasic>();
                if (playerCopy.hp > mostHp)
                {
                    mostHp = playerCopy.hp;
                    targetObject = go;
                }
            }
        }

        if (targetObject != null)
        {
            EnemyBasic enemyBasic = targetObject.GetComponent<EnemyBasic>();
            enemyBasic.hp -= 0.005f;
            enemyBasic.absorbed = true;
            targetObject
                .GetComponent<Rigidbody2D>()
                .AddForce((rbTransform.position - targetObject.transform.position) * 500);
            hp += .005f;
            rb.mass = 500 * hp;

            if(IsServer){
                hpDrain.SetPosition(0, rbTransform.position);
            hpDrain.SetPosition(1, targetObject.transform.position);
            
            }
        }
        // if(IsServer){
            // if(hp > 1)
            // rbTransform.localScale = initialScale * hp;
        // }
        

    }
    void FixedUpdate()
    {
if(!NetworkManager.Singleton.IsServer) return;

if(gameObject.tag == "EnemyCaptain" & hp < 8){
    Upgrade();
}
if(gameObject.tag == "HammerHead"){
              if(Dissolve.colorIntensity > 6){
                //Charge!
                Debug.Log("Charge!");
                
                rb.AddForce(Accelerator(rbTransform.up) * (thrustPower * 400));
                timer=0;
                Dissolve.charging = false;
                chargeAudio.Stop();
            chargeAudio.loop = false;
                Dissolve.colorIntensity = 0;
            }
    UpdateHHTarget();
}
        if (player == null && uM.playerArray.Length > 0)
        {
            player = uM.GetClosestPlayerGameObject(gameObject.transform.position);

            playerT = player.transform;
            playerC = player.GetComponent<PlayerController>();
        }

        velocity = rb.velocity;
        if (Dissolve.dissolveDone)
        {
            Despawn();
            return;
        }
        if (hp < 0)
        {
            Dissolve.isDissolving = true;
            
        }



        if (hp != hpCheck)
        {
            hpCheck = hp;
        }
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;

        timer += Time.deltaTime;
        var offset = 90f;
        Vector2 targetDirection = target - rb.position;
        targetDirection.Normalize();
        float targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;

        fireDirection = fireTarget - rb.position;
        fireDirection.Normalize();
        float fireAngle = Mathf.Atan2(fireDirection.y, fireDirection.x) * Mathf.Rad2Deg;

        Quaternion targetRotation;
        if (
            Vector2.Distance(rb.position, target) > distanceThreshold
           
        )
        {
            targetDirectionDot = Vector3.Dot(transform.up, targetDirection);
            if(targetDirectionDot > .9){
                rb.AddForce(Accelerator(rbTransform.up) * (thrustPower));
            }
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
                    if (Vector2.Distance(rb.position, cptTransform.position) < distanceThreshold)
                    {
                        rb.velocity = rallyCpt.GetComponent<Rigidbody2D>().velocity;
                    }
                    else
                    {
                        rb.AddForce(Accelerator(rbTransform.up) * (thrustPower));
                    }
                }
                else if (isCaptain)
                    rb.AddForce(
                        Accelerator(rbTransform.up)
                            * (thrustPower)
                    );
                else
                {
                    rb.AddForce(Accelerator(rbTransform.up) * (thrustPower));
                }
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
        
    }

    void Update()
    {
        if(!IsServer) return;
        if (uM.playerArray.Length == 0)
            return;
        magnitude = rb.velocity.magnitude;

        if (magnitude > 30)
        {
            VGPos = rb.position;
            VGPos.z = VGZ;
            if (magnitude > 80)
                magnitude = 80;
            // uM.VG.AddGridForce(VGPos, magnitude * uM.VGForce, 1, color, true);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Missile : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        rbTransform = transform;
    }

    public Weapon parent;
    private float maxSpeed = 1000f;
    private float moveSpeed = 20f;
    private float acceleration = 200;
    private float accTimer;
    private float accTimerBase = 20;
    private float accTimerBaseReset = 20;

    public ParticleSystem explosion;
    public Rigidbody2D rb;
    public GameObject engineParticles;
    public Transform exhaustPoint;

    public Quaternion targetRotation;
    private float offset = 90f;
    Vector2 moveDirection;
    public Vector2 target;
    public GameObject targetGameObject;
    private float explosionRadius = 50f;
    private float explosionForce = 50f;
    private float timer = 0;
    private float targetAngle;
    private Vector2 predictedTarget;
    private Vector2 targetDirection;
    private float time;
    private float distance;
    public AudioSource rocketBoostSFX;
    public AudioClip[] rocketSounds;
    private Transform rbTransform;
    UM uM;
    Color rippleColor = Color.blue;
    public GameObject prefab;
    private bool boosted = false;
    float thrustPower;
public Dictionary<ulong, GameObject> enemyDic = new Dictionary<ulong, GameObject>();
    //     public AudioSource explosionSFX;
    //     private AudioClip exp;
    // public AudioClip[] explosions;

    void Start()
    {
        uM = UM.Instance;
        if (gameObject.tag == "Missile")
        {
            engineParticles.GetComponent<ParticleSystem>().startColor = Color.blue;
            // targetGameObject = uM.fireTarget;
            rippleColor = Color.blue;
        }
        rbTransform = transform;
        rb.angularDrag = 1;
        rb.drag = 2f;
        rb.mass = 1000;
        thrustPower = rb.mass * 20;
        rb.gravityScale = 1;

        // InvokeRepeating("StartAudio", 0f, 3f);
    }

    // void Awake() {
    //     DontDestroyOnLoad(gameObject);
    // }

    void OnEnable()
    {
        uM = UM.Instance;
        int index = Random.Range(0, rocketSounds.Length);
        rocketBoostSFX.clip = rocketSounds[index];
        rocketBoostSFX.pitch = Random.Range(.9f, 1.1f);
        rocketBoostSFX.Play();
        //  int indexE = Random.Range(0, explosions.Length);
        //         // exp = explosions[indexE];
        //         explosionSFX.pitch = Random.Range(.9f, 1.1f);


        if (gameObject.tag == "Missile")
        {
            engineParticles.GetComponent<ParticleSystem>().startColor = Color.blue;
            // targetGameObject = uM.fireTarget;
            rippleColor = Color.blue;
        }
        else if (gameObject.tag == "EnemyMissile")
        {
            rippleColor = Color.red;
        }
    }

    public void SetTarget(Vector3 EMTarget)
    {
        
        target = EMTarget;
    }

    public void SetPlayerTarget(GameObject MTarget, Dictionary<ulong, GameObject> parentEnemyDic)
    {
        enemyDic = parentEnemyDic;
        // targetGameObject = EMTarget;
        targetGameObject = MTarget;
    }
public bool DicCheck(GameObject gameObjectToCheck)
    {
        foreach (var kvp in enemyDic)
        {
            if (kvp.Value == gameObjectToCheck)
            {
                return true;
            }
        }
        return false;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (gameObject.tag == "Missile")
        {
            if (
                DicCheck(collision.gameObject)
            )
            {
                Explode();
                timer = 0;
                // uM.despawnServerObject(prefab, NetworkObject);
            }
        }
        else
        {
            if (collision.gameObject.tag == "Clone" || collision.gameObject.tag == "Player")
            {
                Explode();
                timer = 0;
                // uM.despawnServerObject(prefab, NetworkObject);
            }
        }
    }

    private void Explode()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        if (gameObject.tag == "Missile")
        {
            float effectMod = 1;

            foreach (Collider2D hit in colliders)
            {
                bool contact = false;
                Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 direction = rb.transform.position - transform.position;
                    float distance = direction.magnitude;
                    float effect = (1 - (distance / explosionRadius)) * effectMod;
                    if (effect < 0)
                    {
                        effect = 0;
                    }
                    Vector2 force = direction.normalized * explosionForce * effect;

                    if (hit.tag == "BasicEnemy" || hit.tag == "HammerHead")
                    {
                        contact = true;
                        rb.gameObject.GetComponent<EnemyBasic>().hp -= effect * .4f;
                        // rb.AddForce(force, ForceMode2D.Impulse);
                        rb.AddForce(force * rb.mass * 50);
                    }
                    if (hit.tag == "EnemyCaptain")
                    {
                        contact = true;
                        rb.gameObject.GetComponent<EnemyBasic>().hp -= effect * .2f;
                    }
                    if (contact)
                    {
                        if (effectMod > 0)
                        {
                            effectMod -= .1f;
                        }
                        else
                        {
                            effectMod = .1f;
                        }
                    }
                }
            }
        }
        else
        {
            float effectMod = 1;
            foreach (Collider2D hit in colliders)
            {
                bool contact = false;
                Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 direction = rb.transform.position - transform.position;
                    float distance = direction.magnitude;
                    float effect = 1 - (distance / explosionRadius);
                    Vector2 force = direction.normalized * explosionForce * effect;

                    if (rb.tag == "Clone")
                    {
                        contact = true;
                        rb.gameObject.GetComponent<PlayerCopy>().hp -= effect * .4f;
                        // rb.AddForce(force, ForceMode2D.Impulse);
                        rb.AddForce(force * rb.mass * 50);
                    }
                    if (rb.tag == "Player")
                    {
                        contact = true;
                        rb.GetComponent<PlayerController>().ApplyDmg(.1f);
                        // rb.AddForce(force, ForceMode2D.Impulse);
                        rb.AddForce(force * rb.mass * 50);
                    }
                }
                if (contact)
                {
                    if (effectMod > 0)
                    {
                        effectMod -= .1f;
                    }
                    else
                    {
                        effectMod = .1f;
                    }
                    if (effectMod > 0)
                    {
                        effectMod = 0;
                    }
                }
            }
        }

        // ParticleSystem spark = Instantiate(explosion, this.transform.position, Quaternion.identity);
        // Destroy(spark, 2f);

        uM.AddGridForce(transform.position, -5, 8, rippleColor, true);
        uM.Explosion(transform);
        uM.spawnExplosionServerRpc(rbTransform.position, rbTransform.rotation);
        // uM.despawn(prefab);

        if (!IsServer)
            return;
        boosted = false;
        // despawnServerRpc();
if (NetworkObject.IsSpawned)
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

    Vector3 Accelerator()
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

        return transform.up * (smoothAccel / 5);
    }

    void FixedUpdate()
    {
        if (!IsOwner)
            return;
        if (!boosted)
        {
            rb.AddForce(Accelerator() * thrustPower * 10);
            boosted = true;
        }
        if (rbTransform.position.z != -1)
        {
            rbTransform.position = new Vector3(rbTransform.position.x, rbTransform.position.y, -1);
        }
        timer += Time.deltaTime;
        if (timer > 8)
        {
            Explode();
            timer = 0;
            // uM.despawnServerObject(prefab, NetworkObject);
        }

        // engine.transform.position = exhaustPoint.position;

        if (targetGameObject != null && targetGameObject.activeInHierarchy)
        {
            target = targetGameObject.GetComponent<Rigidbody2D>().transform.position;
        }
        else
        {
            if (gameObject.tag == "Missile" && UM.Instance.enemies.Length > 0)
            {
                // targetGameObject = UM.Instance.GetClosestGameObject(
                //     UM.Instance.enemies,
                //     transform.position
                // );
                // if(targetGameObject == null){
                //     targetGameObject = transform.parent.gameObject.GetComponent<Missile>().parent.fireTarget;
                // }
                target = uM
                    .GetClosestGameObject(UM.Instance.enemies, transform.position)
                    .transform.position;
            }
            // else Explode();
        }

        distance = Vector2.Distance(rb.transform.position, target);
        if (rb.velocity.magnitude == 0)
        {
            time = distance / 0.01f;
        }
        else
            time = distance / rb.velocity.magnitude;
        if (gameObject.tag != "EnemyMPF" && targetGameObject != null)
        {
            predictedTarget = target + targetGameObject.GetComponent<Rigidbody2D>().velocity * time;
        }
        else
        {
            // if (timer > 5)
            // {
            //     predictedTarget = uM.position;
            // }
            // else
            if (timer > 3)
            {
                predictedTarget = uM.allyArray[0].transform.position;
            }
            else
                predictedTarget = target;
        }
        targetDirection = predictedTarget - rb.position;
        targetDirection.Normalize();
        // if (Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg > 0.1f)
        targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;

        targetRotation = Quaternion.Euler(Vector3.forward * (targetAngle - offset));
        float ddot = Vector3.Dot(transform.up, targetDirection);
        if (ddot > .7)
        {
            rb.AddForce(Accelerator() * thrustPower / 3);
        }
        if (ddot > .98)
        {
            rb.AddForce(Accelerator() * thrustPower / 2);
        }

        if (transform.rotation != targetRotation)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 100f);
        }
    }
}

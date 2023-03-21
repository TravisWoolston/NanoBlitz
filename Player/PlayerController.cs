using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Pathfinding;
using UnityEngine.AI;
using Unity.Netcode;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{
       public override void OnNetworkSpawn() {
        if(!IsOwner) return;
       }
    public float maxSpeed = 100f;
    public float moveSpeed = 40f;
    public float acceleration = 5f;
    public float accTimer;
    public Rigidbody2D rb;
    public Weapon weapon;

    Vector2 moveDirection;
    public Vector2 mousePosition;
    public Vector3 VGMouse;
    Vector2 target;
    Transform from;
    Transform to;
    public float movementMultiplier = 5;
    public float timer = 0.0f;
    public float hp;
    public float maxHp = 1;
    public float moveX;
    public float moveY;
    private float velocityX;
    private float velocityXBrake;
    private float velocityY;
    private float velocityYBrake;
    public int allies = 0;
    public bool loaded = true;
    public bool firing = false;
    Vector3 skyboxRotationAxis;
    float skyboxRotationAngle = 0;
    float rotationDampener = 0.1f;
    public GameObject[] allyArray;
    public Transform[] allyArrayTransform;
    public GameObject[] enemies;
    public GameObject[] captains;
    public Vector3 position;
    public GameObject fireTarget;
    public GameObject engineParticles;
    GameObject engine;
    public Transform exhaustPoint;
    public int missiles = 5;
    public float rocketChargeTime = 5f;
    private float rocketTimer = 0;
    public AudioSource bulletAudio;
    public AudioSource missileLaunch;
    private float explosionRadius = 5f;
    private float explosionForce = 100f;
    public GameObject explosionSource;
    private GameObject splode;

    public float magnitude;
    public float VGForce = .03f;
    public float VGRadius = .03f;
    private Color color;

    // public VectorGrid VG;
    float thrustPower;
    public float VGZ = 10;
    Vector3 VGPos;
    public Transform rbTransform;
    Color rippleColor = Color.blue;
    public VectorGrid VGShield;
    VectorGrid shield;
    bool activeShield = false;
    private HealthBar HPBar;
    private ShieldBar ShieldMeter;
    float sHPMax = .5f;
    float sHP;
    public LineRenderer hpDrain;
    float dmgRate = .0314f;
    float sTimer = 0;
    public Vector3 velocity;
    UM uM;
    float dodgeCD = 0;
    public float distanceThreshold = 0;
    Collider2D[] targets;
    Vector3 initialScale;
    Collider2D[] colliders;
    public GameObject playerCam;
    public CamMovement playerCamC;
    private float maxZoom;
    public float playerID = 0;

    void Awake()
    {
        rb.gravityScale = 1;
        // Instance = this;
        color = new Color(hp, 1 - hp, 1 - hp);
        this.GetComponent<SpriteRenderer>().color = color;
    }

    void Start()
    {
        if(!IsOwner) return;
        hp = maxHp;
        sHP = sHPMax;
        rbTransform = rb.transform;
        uM = UM.Instance;

        rb.angularDrag = 100;
        rb.drag = 1f;
        rb.gravityScale = 1;
        rb.mass = 99999;
        thrustPower = rb.mass * 50;
        engine = Instantiate(engineParticles, exhaustPoint);
        missileLaunch.time = .5f;
        missileLaunch.pitch = UnityEngine.Random.Range(.9f, 1.1f);
        bulletAudio.pitch = UnityEngine.Random.Range(.9f, 1.8f);
        allies = uM.allies;
        // InvokeRepeating("Movement", 0f, .05f);
        shield = Instantiate(VGShield, rbTransform);
        // shield.transform.SetParent(gameObject.transform);
        // HPBar = HealthBar.Instance;
       
        InvokeRepeating("UpdateGlobal", 0f, .5f);
        initialScale = rbTransform.localScale;
        playerID = uM.GetPlayerID();

            playerCam = GameObject.Find("Main Camera");
            HPBar = HealthBar.Instance;
            ShieldMeter = ShieldBar.Instance;
            playerCamC = playerCam.GetComponent<CamMovement>();
            playerCamC.player = rbTransform;

         HPBar.SetMaxHealth(hp);

         HPBar.SetHealth(hp);
        ShieldMeter.SetMaxHealth(sHP);

        ShieldMeter.SetHealth(sHP);
    }

    private void UpdateGlobal()
    {
        enemies = uM.enemies;
        targets = Physics2D.OverlapCircleAll(rbTransform.position, distanceThreshold);
        rbTransform.localScale = initialScale * maxHp;
        colliders = Physics2D.OverlapCircleAll(transform.position, maxHp * 5);
        if (playerCamC != null)
            playerCamC.maxZoom = maxHp * 10;
        // shield.m_GridWidth = 8 + (int)Mathf.Round(maxHp);
        // shield.m_GridHeight = 13 + (int)Mathf.Round(maxHp);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "EnemyBullet")
        {
            ApplyDmg(dmgRate);
        }
    }

    public void ApplyDmg(float dmg)
    {
        if(!IsOwner) return;
        if (sHP > 0)
        {
            sHP -= dmg;
            ShieldMeter.SetHealth(sHP);
            sTimer = 0;
        }
        else
        {
            hp -= dmg;
            HPBar.SetHealth(hp);
        }
    }

    void Movement()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            rb.AddForce(Accelerator(rbTransform.up) * thrustPower);
        }
        else
        {
            if (accTimer > 0)
            {
                accTimer -= Time.deltaTime;
            }
        }
        if (Input.GetKey(KeyCode.A) && dodgeCD > 2)
        {
            dodgeCD = 0;
            rb.AddForce(-rbTransform.right * thrustPower * 150);
        }
        if (Input.GetKey(KeyCode.D) && dodgeCD > 2)
        {
            dodgeCD = 0;
            rb.AddForce(rbTransform.right * thrustPower * 150);
        }
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

    void recoverHP()
    {
        hpDrain.enabled = true;
        GameObject targetObject = null;
        float mostHp = -1;
        foreach (Collider2D tgt in targets)
        {
            GameObject go = tgt.gameObject;
            if (go.tag == "Clone")
            {
                PlayerCopy playerCopy = go.GetComponent<PlayerCopy>();
                if (playerCopy.hp > mostHp)
                {
                    mostHp = playerCopy.hp;
                    targetObject = go;
                }
            }
        }

        if (targetObject != null)
        {
            targetObject.GetComponent<PlayerCopy>().hp -= 0.005f;
            hp += .002f;
            HPBar.SetHealth(hp);
            hpDrain.SetPosition(0, rbTransform.position);
            hpDrain.SetPosition(1, targetObject.transform.position);
        }
    }

    void Upgrade()
    {
        hpDrain.enabled = true;
        GameObject targetObject = null;
        float mostHp = -1;
        foreach (Collider2D tgt in targets)
        {
            GameObject go = tgt.gameObject;
            if (go.tag == "Clone")
            {
                PlayerCopy playerCopy = go.GetComponent<PlayerCopy>();
                if (playerCopy.hp > mostHp)
                {
                    mostHp = playerCopy.hp;
                    targetObject = go;
                }
            }
        }

        if (targetObject != null)
        {
            targetObject.GetComponent<PlayerCopy>().hp -= 0.005f;
            maxHp += .002f;
            HPBar.SetMaxHealth(maxHp);
            hpDrain.SetPosition(0, rbTransform.position);
            hpDrain.SetPosition(1, targetObject.transform.position);
        }
    }

    private void FixedUpdate()
    {
        if(!IsOwner) return;
        velocity = rb.velocity;
        distanceThreshold = (10f + allies / 6) * (maxHp / hp) + maxHp;
        if (sHP <= 0)
        {
            shield.GetComponent<MeshRenderer>().enabled = false;
        }
        else
        {
            shield.GetComponent<MeshRenderer>().enabled = true;
        }
        Movement();
        VGZ = uM.VG.transform.position.z;
        magnitude = rb.velocity.magnitude;
        rocketTimer += Time.deltaTime * maxHp;
        dodgeCD += Time.deltaTime;
        if (rocketTimer > rocketChargeTime)
        {
            rocketTimer = 0;
            missiles++;
        }
        timer += Time.deltaTime;
        firing = Input.GetMouseButton(0);

        moveX = Input.GetAxisRaw("Horizontal");
        moveY = Input.GetAxisRaw("Vertical");

        GameObject closest = null;

        position = transform.position;

        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        var offset = 90f;

        Vector2 direction = mousePosition - rb.position;
        direction.Normalize();
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(Vector3.forward * (angle - offset));

        if (transform.rotation != targetRotation)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 3);
        }
        // else {
        //     rb.velocity = transform.up * moveSpeed;
        // }

        else
        {
            if (closest == null)
                target = mousePosition;
            else
                target = closest.transform.position;
        }

        float dot = Vector3.Dot(transform.up, direction.normalized);
        if (Input.GetKey(KeyCode.F))
        {
            Upgrade();
        }
        else
        {
            if (hp < maxHp)
            {
                recoverHP();
            }
            else
                hpDrain.enabled = false;
        }
        if (timer > .3f && dot > .9)
        {
            if (Input.GetMouseButton(1) && missiles > 0)
            {
                fireTarget = uM.GetClosestGameObject(enemies, mousePosition);
                missiles--;
                weapon.FireMissile(fireTarget);
                timer = 0;
                missileLaunch.Play();
            }
            // else {

            // // bulletAudio.Play();
            // // weapon.Fire();
            // }
        }
    }

    void Update()
    {
        if(!IsOwner) return;
        if (playerCam == null)
        {
            if (IsOwner)
            {
                playerCam = GameObject.Find("Main Camera");
                playerCamC = playerCam.GetComponent<CamMovement>();
                playerCamC.player = rbTransform;
            }
        }
        //         playerCam.transform.rotation = Quaternion.Euler(
        //     0.0f,
        //     0.0f,
        //     gameObject.transform.rotation.z * -1.0f
        // );
        if (allies < uM.allies)
        {
            allies = uM.allies;
            sHPMax = .5f + (allies / 15);
            ShieldMeter.SetMaxHealth(sHPMax);
        }
        if (sTimer >= 5 && sHP < sHPMax)
        {
            sHP += sHPMax / 1000;
            if (sHP > sHPMax)
                sHP = sHPMax;
            ShieldMeter.SetHealth(sHP);
        }
        sTimer += Time.deltaTime;

        if (Input.GetMouseButton(0))
        {
            fireTarget = uM.GetClosestGameObject(enemies, mousePosition);

            VGMouse = (Vector3)mousePosition;
            VGMouse.z = VGZ;
            LayerMask targetMask = 1;
            // Collider2D[] targets = Physics2D.OverlapCircleAll(mousePosition, 30);
            uM.VG.AddGridForce(VGMouse, 3, 2, rippleColor, false);
            // foreach (Collider2D tgt in targets)
            // {
            //     GameObject go = tgt.gameObject;
            //     if (go.tag == "Clone")
            //     {
            //         go.GetComponent<PlayerCopy>().rallied = true;
            //     }
            // }
        }

        VGPos = rb.position;
        VGPos.z = uM.VGZ;
        uM.VG.AddGridForce(VGPos, 5, maxHp * 1.2f, uM.color1, true);

        if (colliders != null)
            foreach (Collider2D hit in colliders)
            {
                Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    if (rb.tag != this.tag && rb.tag != "Bullet")
                    {
                        Transform colTransform = rb.transform;
                        Vector3 colVec = new Vector3(
                            colTransform.position.x,
                            colTransform.position.y,
                            shield.transform.position.z
                        );
                        shield.AddGridForce(colVec, 1f, .3f, Color.red, false);
                    }
                }
            }
    }

    void LateUpdate()
    {
        if(!IsOwner) return;
        shield.transform.rotation = Quaternion.Euler(
            0.0f,
            0.0f,
            gameObject.transform.rotation.z * -1.0f
        );
    }
}



// sticky code

// public class StickyObject : MonoBehaviour
// {
//     private bool isStuck = false;
//     private Transform targetTransform;

//     private void OnCollisionEnter(Collision collision)
//     {
//         if (!isStuck && collision.gameObject.tag == "Sticky")
//         {
//             isStuck = true;
//             targetTransform = collision.gameObject.transform;
//             GetComponent<Rigidbody>().isKinematic = true;
//         }
//     }

//     private void FixedUpdate()
//     {
//         if (isStuck)
//         {
//             transform.position = Vector3.Lerp(transform.position, targetTransform.position, Time.deltaTime * 10f);
//             transform.rotation = Quaternion.Lerp(transform.rotation, targetTransform.rotation, Time.deltaTime * 10f);
//         }
//     }
// }

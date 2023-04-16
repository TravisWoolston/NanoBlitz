using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{
    public float maxSpeed = 100f;
    public float moveSpeed = 40f;
    public float acceleration = 5f;
    public float accTimer;

    [SerializeField]
    private Rigidbody2D rb;

    [SerializeField]
    private Weapon weapon;
    public Transform firePoint;
    Vector2 moveDirection;

    public Vector2 mousePosition;
    public Vector3 VGMouse;
    Vector2 target;
    Transform from;
    Transform to;
    public float movementMultiplier = 5;
    public float timer = 0.0f;

    // public NetworkVariable<float> hp = new NetworkVariable<float>();
    public float hp = 1;
    public float maxHp = 1;
    float sHPMax = .5f;
    public float sHP;
    public int allies = 0;
    public bool loaded = true;
    public bool firing = false;
    Vector3 skyboxRotationAxis;
    float skyboxRotationAngle = 0;
    float rotationDampener = 0.1f;
    public GameObject[] enemies;
    public GameObject[] captains;
    public Vector3 position;
    public GameObject fireTarget;
    public GameObject engineParticles;
    GameObject engine;
    public Transform exhaustPoint;
    public float missiles = 5;
    public float rocketChargeTime = 5f;
    private float rocketTimer = 0;
    public AudioSource bulletAudio;
    public AudioSource missileLaunch;
    public AudioSource thrustAudio;
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
    public NetworkVariable<Color> playerColor = new NetworkVariable<Color>();
    public VectorGrid VGShield;

    // VectorGrid VGShield;
    bool activeShield = false;
    private HealthBar HPBar;
    private ShieldBar ShieldMeter;

    public GameObject goLR;
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
    public GameObject playerMM;
    public CamMovement playerMMC;
    public CamMovement playerCamC;
    private float maxZoom;

    public NetworkVariable<int> playerID = new NetworkVariable<int>();
    bool isThrusting = false;
    private GameObject playerUIGO;
    UIPlayerValues playerUI;
    private GameObject missileCountUI;
    private UIMissileDisplay missileCount;

    public GameObject renderCam;
    public CamMovement renderCamC;
    public Dictionary<ulong, GameObject> allyDic = new Dictionary<ulong, GameObject>();
    public Dictionary<ulong, GameObject> enemyDic = new Dictionary<ulong, GameObject>();
    float boostMod = 100;
    void Awake()
    {
        uM = UM.Instance;
        rb.gravityScale = 1;
    }
    public override void OnNetworkSpawn(){
        AssignPlayerColorServerRpc();
    }
    [ServerRpc(RequireOwnership = false)]
    public void AssignPlayerColorServerRpc()
    {
        playerID.Value = uM.GetPlayerID();

        for (int i = 0; i < uM.playerArray.Length; i++)
        {
            uM.playerArray[i].GetComponent<PlayerController>().playerColor.Value = uM.colorArray[i];
            uM.playerArray[i].GetComponent<PlayerController>().playerID.Value = i;
        
        }
    }

    void Start()
    {
        
        hp = maxHp;

        
        rb.mass = 1000;
        rbTransform = rb.transform;
        rb.angularDrag = 100;
        rb.drag = 1f;
        rb.gravityScale = 1;
        initialScale = rbTransform.localScale;
        engineParticles.GetComponent<ParticleSystem>().startColor = playerColor.Value;
        // hpDrain = goLR.GetComponent<LineRenderer>();
        sHP = sHPMax;
        if (!IsOwner)
            return;

        
        playerUIGO = GameObject.Find("UIPlayerValues");
        playerUI = playerUIGO.GetComponent<UIPlayerValues>();

        thrustPower = rb.mass * 50;

        missileLaunch.time = .5f;
        missileLaunch.pitch = UnityEngine.Random.Range(.9f, 1.1f);
        bulletAudio.pitch = UnityEngine.Random.Range(.9f, 1.8f);
        thrustAudio.pitch = UnityEngine.Random.Range(.9f, 1.8f);
        allies = uM.allies;

        // VGShield = Instantiate(VGShield, rbTransform);

        InvokeRepeating("UpdateGlobal", 0f, .5f);

        playerMM = GameObject.Find("MiniMap");
        playerCam = GameObject.Find("Main Camera");
        playerCamC = playerCam.GetComponent<CamMovement>();
        playerCamC.player = rbTransform;

        renderCam = GameObject.Find("Render Cam");
        renderCamC = renderCam.GetComponent<CamMovement>();
        renderCamC.player = rbTransform;

        HPBar = HealthBar.Instance;
        ShieldMeter = ShieldBar.Instance;
        playerMMC = playerMM.GetComponent<CamMovement>();
        playerMMC.player = rbTransform;
        HPBar.SetMaxHealth(hp);

        HPBar.SetHealth(hp);
        ShieldMeter.SetMaxHealth(sHP);

        ShieldMeter.SetHealth(sHP);
    }

    private void UpdateGlobal()
    {
        // loop through player array and assign enemies array in UM?
        enemies = uM.enemies;

        colliders = Physics2D.OverlapCircleAll(transform.position, maxHp * 5);
        if (playerCamC != null)
            playerCamC.maxZoom = maxHp * 10;
        // VGShield.m_GridWidth = 8 + (int)Mathf.Round(maxHp);
        // VGShield.m_GridHeight = 13 + (int)Mathf.Round(maxHp);
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "HammerHead"){
            ApplyDmg(collision.gameObject.GetComponent<Rigidbody2D>().velocity.magnitude * .1f);
        }
        if (collision.gameObject.tag == "EnemyBullet")
        {
            ApplyDmg(dmgRate);
        }
    }

    public void ApplyDmg(float dmg)
    {
        // ApplyDmgServerRpc(dmg);
        if (sHP > 0)
        {
            sHP -= dmg;

            sTimer = 0;
        }
        else
        {
            hp -= dmg;
        }
        if (hp < 0)
            hp = 0;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ApplyDmgServerRpc(float dmg)
    {
        if (sHP > 0)
        {
            sHP -= dmg;

            sTimer = 0;
        }
        else
        {
            hp -= dmg;
        }
        if (hp < 0)
            hp = 0;
    }

    void Movement()
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.W))
        {
            if (!isThrusting)
            {
                isThrusting = true;
                thrustAudio.loop = true;
                thrustAudio.Play();
            }

            if (IsServer)
                rb.AddForce(Accelerator(rbTransform.up) * (thrustPower*maxHp));
            else
            {
                uM.moveServerRpc(playerID.Value, Accelerator(rbTransform.up) * thrustPower);
            }
            thrustAudio.volume = Mathf.MoveTowards(thrustAudio.volume, .2f, Time.deltaTime * .6f);
        }
        else
        {
            isThrusting = false;
            thrustAudio.volume = Mathf.MoveTowards(thrustAudio.volume, 0f, Time.deltaTime * .8f);
            if (thrustAudio.volume == 0f)
            {
                thrustAudio.Stop();
                thrustAudio.loop = false;
            }
            if (accTimer > 0)
            {
                accTimer -= Time.deltaTime;
            }
        }
        if (Input.GetKey(KeyCode.A) && dodgeCD > 2)
        {
            if (IsServer && IsOwner)
            {
                rb.AddForce(Accelerator(-rbTransform.right) * (thrustPower * boostMod * maxHp));
            }
            else
                uM.moveServerRpc(playerID.Value, Accelerator(-rbTransform.right) * (thrustPower * boostMod * maxHp));

            dodgeCD = 0;
        }
        if (Input.GetKey(KeyCode.D) && dodgeCD > 2)
        {
            dodgeCD = 0;
            if (IsServer)
            {
                rb.AddForce(Accelerator(rbTransform.right) * (thrustPower * boostMod * maxHp));
            }
            else
                uM.moveServerRpc(playerID.Value, Accelerator(rbTransform.right) * (thrustPower * boostMod * maxHp));
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

        targets = Physics2D.OverlapCircleAll(rbTransform.position, distanceThreshold);
        hpDrain.enabled = true;
        float healRate = .002f * maxHp;
        NetworkObject targetObject = null;
        float mostHp = -1;
        foreach (Collider2D tgt in targets)
        {
            NetworkObject go = tgt.gameObject.GetComponent<NetworkObject>();
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
            Transform targetObjT = targetObject.transform;
            targetObject.GetComponent<PlayerCopy>().hp -= healRate;
            if (Vector2.Distance(rbTransform.position, targetObjT.position) > distanceThreshold /4)
                targetObject
                    .GetComponent<Rigidbody2D>()
                    .AddForce((rbTransform.position - targetObjT.position) * 500);
            // UpdateHpServerRpc(healRate);
            hp += healRate;
            HPBar.SetHealth(hp);
            hpDrain.SetPosition(0, rbTransform.position);
            hpDrain.SetPosition(1, targetObjT.position);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateHpServerRpc(float hpAdd)
    {
        hp += hpAdd;
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
            targetObject
                .GetComponent<Rigidbody2D>()
                .AddForce((rbTransform.position - targetObject.transform.position) * 500);
            maxHp += .002f;
            rb.mass = 1000 * maxHp;
            HPBar.SetMaxHealth(maxHp);
            if(IsServer){
                hpDrain.SetPosition(0, rbTransform.position);
            hpDrain.SetPosition(1, targetObject.transform.position);
            
            }
            else {
                uM.UpgradeServerRpc(playerID.Value, rbTransform.position, targetObject.transform.position);
            }
        }
        // if(IsServer)
        // rbTransform.localScale = initialScale * maxHp;
        // else
        // uM.scaleServerRpc(playerID.Value, initialScale * maxHp);
    }

    private void FixedUpdate()
    {
        magnitude = rb.velocity.magnitude;
        distanceThreshold = (30);
        // distanceThreshold = (30 + maxHp);
        velocity = rb.velocity;
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        firing = Input.GetMouseButton(0);
    dodgeCD += Time.deltaTime;
        fireTarget = uM.GetClosestEnemyGameObjectDic(enemyDic, mousePosition);
        if (!IsOwner)
            return;
        ShieldMeter.SetHealth(sHP);
        HPBar.SetHealth(hp);
        playerUI.clones = allies;
        playerUI.missiles = missiles;
        playerUI.rocketRate = maxHp;

        if (sHP <= 0)
        {
            VGShield.GetComponent<MeshRenderer>().enabled = false;
        }
        else
        {
            VGShield.GetComponent<MeshRenderer>().enabled = true;
        }
        Movement();
        VGZ = uM.VG.transform.position.z;

        rocketTimer += Time.deltaTime * maxHp;

        missiles += (Time.deltaTime * maxHp) / 3;

        timer += Time.deltaTime;

        GameObject closest = null;

        position = transform.position;

        var offset = 90f;

        Vector2 direction = mousePosition - rb.position;
        direction.Normalize();
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(Vector3.forward * (angle - offset));

        if (transform.rotation != targetRotation)
        {
            if (IsServer)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    3
                );
            }
            else
                uM.rotateServerRpc(playerID.Value, targetRotation);
        }
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
                missiles--;

                uM.spawnMissileServerRpc(firePoint.position, transform.rotation);

                timer = 0;
                missileLaunch.Play();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateCServerRpc()
    {
        UM.Instance.playerArray[playerID.Value] = gameObject;
    }

    void Update()
    {
        if (!IsOwner)
            return;
        if (playerCam == null)
        {
            if (IsOwner || IsServer)
            {
                playerCam = GameObject.Find("Main Camera");
                playerCamC = playerCam.GetComponent<CamMovement>();
                playerCamC.player = rbTransform;
                renderCam = GameObject.Find("Render Camera");
                renderCamC = renderCam.GetComponent<CamMovement>();
                renderCamC.player = rbTransform;
                playerMM = GameObject.Find("MiniMap");
                playerMMC = playerMMC.GetComponent<CamMovement>();
                playerMMC.player = rbTransform;
            }
        }

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
            VGMouse = (Vector3)mousePosition;
            VGMouse.z = VGZ;
            LayerMask targetMask = 1;
            uM.VG.AddGridForce(VGMouse, 3, 2, playerColor.Value, false);
        }

        VGPos = rb.position;
        VGPos.z = uM.VGZ;
        uM.VG.AddGridForce(VGPos, 5, hp * 1.2f, playerColor.Value, true);

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
                            VGShield.transform.position.z
                        );
                        VGShield.AddGridForce(colVec, 1f, .3f, Color.red, false);
                    }
                }
            }
    }

    void LateUpdate()
    {
        if (!IsOwner)
            return;
        VGShield.transform.rotation = Quaternion.Euler(
            0.0f,
            0.0f,
            gameObject.transform.rotation.z * -1.0f
        );
    }
}

struct PlayerData : INetworkSerializable
{
    public ulong id;
    public ushort Length;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref id);
        serializer.SerializeValue(ref Length);
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

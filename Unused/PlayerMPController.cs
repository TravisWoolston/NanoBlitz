// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using System;
// using Pathfinding;
// using UnityEngine.AI;

// public class PlayerMPController : MonoBehaviour
// {
//     // public static PlayerController Instance;
//     public float maxSpeed = 100f;
//     public float moveSpeed = 40f;
//     public float acceleration = 5f;
//     public float accTimer;
//     public Rigidbody2D rb;
//     public Weapon weapon;
//     public float xGrav = 0;
//     public float yGrav = 0;
//     Vector2 moveDirection;
//     public Vector2 mousePosition;
//     private Vector3 VGMouse;
//     Vector2 target;
//     Transform from;
//     Transform to;
//     public float movementMultiplier = 5;
//     public float timer = 0.0f;
//     public float hp;
//     float maxHp = 1;
//     public float moveX;
//     public float moveY;
//     private float velocityX;
//     private float velocityXBrake;
//     private float velocityY;
//     private float velocityYBrake;
//     public int allies = 0;
//     public bool loaded = true;
//     public bool firing = false;
//     Vector3 skyboxRotationAxis;
//     float skyboxRotationAngle = 0;
//     float rotationDampener = 0.1f;
//     public GameObject[] allyArray;
//     public Transform[] allyArrayTransform;
//     public GameObject[] enemies;
//     public GameObject[] captains;
//     public Vector3 position;
//     public GameObject fireTarget;
//     public GameObject engineParticles;
//     GameObject engine;
//     public Transform exhaustPoint;
//     public int missiles = 5;
//     public float rocketChargeTime = 5f;
//     private float rocketTimer = 0;
//     public AudioSource bulletAudio;
//     public AudioSource missileLaunch;
//     private float explosionRadius = 5f;
//     private float explosionForce = 100f;
//     public GameObject explosionSource;
//     private GameObject splode;
//     public AudioClip[] explosions;
//     private AudioClip exp;
//     public float magnitude;
//     public float VGForce = 10f;
//     public float VGRadius = 20f;
//     private Color color;
//     public VectorGrid VG;
//     float thrustPower;
//     public float VGZ = 10;
//     Vector3 VGPos;
//     Transform rbTransform;
//     Color rippleColor = Color.blue;
//     public VectorGrid VGShield;
//     VectorGrid shield;
//     bool activeShield = false;
//     public HealthBar HPBar;
//     public HealthBar ShieldMeter;
//     float sHPMax = .5f;
//     float sHP;
//     public LineRenderer hpDrain;
//     float dmgRate = .0314f;
//     float sTimer = 0;

//     void Awake()
//     {
//         rb.gravityScale = 1;
//         // Instance = this;
//         color = new Color(hp, 1 - hp, 1 - hp);
//         this.GetComponent<SpriteRenderer>().color = color;
//     }

//     void Start()
//     {
//         hp = maxHp;
//         sHP = sHPMax;
//         rbTransform = rb.transform;
//         VGForce = .03f;
//         VGRadius = .03f;
//         Physics2D.gravity = new Vector2(xGrav, yGrav);
//         rb.angularDrag = 100;
//         rb.drag = 1f;
//         rb.gravityScale = 1;
//         rb.mass = 99999;
//         thrustPower = rb.mass * 50;
//         engine = Instantiate(engineParticles, exhaustPoint);
//         missileLaunch.time = .5f;
//         missileLaunch.pitch = UnityEngine.Random.Range(.9f, 1.1f);
//         bulletAudio.pitch = UnityEngine.Random.Range(.9f, 1.8f);
//         InvokeRepeating("UpdateGlobal", 0f, 1f);
//         // InvokeRepeating("Movement", 0f, .05f);
//         shield = Instantiate(VGShield, rbTransform);
//         // shield.transform.SetParent(gameObject.transform);
//         // HPBar = HealthBar.Instance;
//         HPBar.SetMaxHealth(hp);
//         ShieldMeter.SetMaxHealth(sHP);
//     }

//     public void ApplyDmg(float dmg)
//     {
//         if (sHP > 0)
//         {
//             sHP -= dmg;
//             ShieldMeter.SetHealth(sHP);
//             sTimer = 0;
//         }
//         else
//         {
//             hp -= dmg;
//             HPBar.SetHealth(hp);
//         }
//     }

//     void Movement()
//     {
//         if (Input.GetKey(KeyCode.LeftShift))
//         {
//             rb.AddForce(Accelerator(rbTransform.up) * thrustPower);
//         }
//         else
//         {
//             if (accTimer > 0)
//             {
//                 accTimer -= Time.deltaTime;
//             }
//         }
//         if (Input.GetKey(KeyCode.A))
//         {
//             rb.AddForce(-rbTransform.right * thrustPower);
//         }
//         if (Input.GetKey(KeyCode.D))
//         {
//             rb.AddForce(rbTransform.right * thrustPower);
//         }
//     }

//     public void AddGridForce(
//         Vector3 position,
//         float magnitude,
//         float radius,
//         Color rippleColor,
//         bool hasColor
//     )
//     {
//         position.z = VGZ;
//         VG.AddGridForce(position, magnitude, radius, rippleColor, hasColor);
//     }

//     public void Explosion(Transform splodePlace)
//     {
//         int indexE = UnityEngine.Random.Range(0, explosions.Length);
//         exp = explosions[indexE];

//         //      GameObject explosionObject = Instantiate(explosionSource, splodePlace);
//         //      explosionObject.SetActive(true);
//         // AudioSource explosionAudio = explosionObject.GetComponent<AudioSource>();
//         // explosionAudio.clip = exp;
//         // explosionAudio.PlayOneShot(exp, 1f);
//         // Destroy(explosionObject, 3f);
//         AudioSource.PlayClipAtPoint(exp, splodePlace.position);
//     }

//     private void UpdateGlobal()
//     {
//         // GameObject[] alliesArray = GameObject.FindGameObjectsWithTag("Clone");
//         allyArray = GameObject.FindGameObjectsWithTag("Clone");
//         Array.Resize(ref allyArray, allyArray.Length + 1);
//         allyArray[allyArray.Length - 1] = gameObject;
//         captains = GameObject.FindGameObjectsWithTag("EnemyCaptain");
//         enemies = GameObject.FindGameObjectsWithTag("BasicEnemy");
//         allies = allyArray.Length;
//         // Create a new array to hold the combined contents
//         GameObject[] combinedEnemies = new GameObject[captains.Length + enemies.Length];

//         // Copy the contents of the captains array to the combinedEnemies array
//         Array.Copy(captains, combinedEnemies, captains.Length);

//         // Copy the contents of the enemies array to the combinedEnemies array, starting at the end of the captains array
//         Array.Copy(enemies, 0, combinedEnemies, captains.Length, enemies.Length);

//         // Use the combinedEnemies array as the new enemies array
//         enemies = combinedEnemies;
//         allyArrayTransform = new Transform[allies];
//         for (int i = 0; i < allies; i++)
//         {
//             allyArrayTransform[i] = allyArray[i].transform;
//         }
//     }

//     private void OnCollisionEnter2D(Collision2D collision)
//     {
//         if (collision.gameObject.tag == "EnemyBullet")
//         {
//             ApplyDmg(dmgRate);
//         }
//     }

//     public Transform GetClosestTransform(GameObject[] gameObjects, Vector3 position)
//     {
//         float distance = Mathf.Infinity;
//         Transform closestTransform = null;
//         float closestDistance = float.MaxValue;
//         foreach (GameObject gameObject in gameObjects)
//         {
//             Transform transform = gameObject.transform;
//             distance = (transform.position - position).sqrMagnitude;
//             if (distance < closestDistance)
//             {
//                 closestTransform = transform;
//                 closestDistance = distance;
//                 // break;
//             }
//         }
//         return closestTransform;
//     }

//     public GameObject GetClosestGameObject(GameObject[] gameObjects, Vector3 position)
//     {
//         float distance = Mathf.Infinity;
//         Transform closestTransform = null;
//         float closestDistance = float.MaxValue;
//         GameObject closestGameObject = null;
//         foreach (GameObject gameObject in gameObjects)
//         {
//             Transform transform = gameObject.transform;
//             distance = (transform.position - position).sqrMagnitude;
//             if (distance < closestDistance)
//             {
//                 closestTransform = transform;
//                 closestDistance = distance;
//                 closestGameObject = gameObject;
//             }
//         }
//         return closestGameObject;
//     }

//     Vector3 Accelerator(Vector3 inputDirection)
//     {
//         moveDirection = inputDirection;
//         if (accTimer < 5)
//         {
//             accTimer += Time.deltaTime;
//         }
//         if (accTimer < 2)
//         {
//             accTimer++;
//         }
//         float targetSpeed = moveSpeed * accTimer;
//         float clampedSpeed = Mathf.Clamp(targetSpeed, 0, maxSpeed);
//         float smoothAccel = Mathf.SmoothStep(0, clampedSpeed, acceleration * Time.deltaTime);

//         return moveDirection * (smoothAccel / 5);
//     }

//     private void FixedUpdate()
//     {
//         if (sHP <= 0)
//         {
//             shield.GetComponent<MeshRenderer>().enabled = false;
//         }
//         else
//         {
//             shield.GetComponent<MeshRenderer>().enabled = true;
//         }
//         Movement();
//         VGZ = VG.transform.position.z;
//         magnitude = rb.velocity.magnitude;
//         rocketTimer += Time.deltaTime;
//         if (rocketTimer > rocketChargeTime)
//         {
//             rocketTimer = 0;
//             missiles++;
//         }
//         timer += Time.deltaTime;
//         firing = Input.GetMouseButton(0);

//         moveX = Input.GetAxisRaw("Horizontal");
//         moveY = Input.GetAxisRaw("Vertical");

//         GameObject closest = null;

//         position = transform.position;

//         mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

//         var offset = 90f;

//         Vector2 direction = mousePosition - rb.position;
//         direction.Normalize();
//         float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
//         Quaternion targetRotation = Quaternion.Euler(Vector3.forward * (angle - offset));

//         if (transform.rotation != targetRotation)
//         {
//             transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 3);
//         }
//         // else {
//         //     rb.velocity = transform.up * moveSpeed;
//         // }

//         else
//         {
//             if (closest == null)
//                 target = mousePosition;
//             else
//                 target = closest.transform.position;
//         }

//         float dot = Vector3.Dot(transform.up, direction.normalized);
//         fireTarget = GetClosestGameObject(enemies, mousePosition);
//         if (timer > .3f && dot > .9)
//         {
//             if (Input.GetMouseButton(1) && missiles > 0)
//             {
//                 missiles--;
//                 weapon.FireMissile();
//                 timer = 0;
//                 missileLaunch.Play();
//             }
//             // else {

//             // // bulletAudio.Play();
//             // // weapon.Fire();
//             // }
//         }
//     }

//     void Update()
//     {
//         if (sTimer >= 5 && sHP < sHPMax)
//         {
//             sHP += .001f;
//             if (sHP > sHPMax)
//                 sHP = sHPMax;
//             ShieldMeter.SetHealth(sHP);
//         }
//         sTimer += Time.deltaTime;
//         float mostHp = -1;
//         if (hp < maxHp)
//         {
//             hpDrain.enabled = true;
//             Collider2D[] targets = Physics2D.OverlapCircleAll(rbTransform.position, 30);
//             GameObject targetObject = null;

//             foreach (Collider2D tgt in targets)
//             {
//                 GameObject go = tgt.gameObject;
//                 if (go.tag == "Clone")
//                 {
//                     PlayerCopy playerCopy = go.GetComponent<PlayerCopy>();
//                     if (playerCopy.hp > mostHp)
//                     {
//                         mostHp = playerCopy.hp;
//                         targetObject = go;
//                     }
//                 }
//             }

//             if (targetObject != null)
//             {
//                 targetObject.GetComponent<PlayerCopy>().hp -= 0.005f;
//                 hp += .002f;
//                 HPBar.SetHealth(hp);
//                 hpDrain.SetPosition(0, rbTransform.position);
//                 hpDrain.SetPosition(1, targetObject.transform.position);
//             }
//         }
//         else
//             hpDrain.enabled = false;

//         if (Input.GetMouseButton(0))
//         {
//             VGMouse = (Vector3)mousePosition;
//             VGMouse.z = VGZ;
//             LayerMask targetMask = 1;
//             // Collider2D[] targets = Physics2D.OverlapCircleAll(mousePosition, 30);
//             VG.AddGridForce(VGMouse, 3, 2, rippleColor, false);
//             // foreach (Collider2D tgt in targets)
//             // {
//             //     GameObject go = tgt.gameObject;
//             //     if (go.tag == "Clone")
//             //     {
//             //         go.GetComponent<PlayerCopy>().rallied = true;
//             //     }
//             // }
//         }

//         VGPos = rb.position;
//         VGPos.z = VGZ;
//         VG.AddGridForce(VGPos, magnitude * VGForce, 2, rippleColor, true);
//     }

//     void LateUpdate()
//     {
//         shield.transform.rotation = Quaternion.Euler(
//             0.0f,
//             0.0f,
//             gameObject.transform.rotation.z * -1.0f
//         );
//         Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 5);

//         foreach (Collider2D hit in colliders)
//         {
//             Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
//             if (rb != null)
//             {
//                 if (rb.tag != this.tag)
//                 {
//                     Transform colTransform = rb.transform;
//                     Vector3 colVec = new Vector3(
//                         colTransform.position.x,
//                         colTransform.position.y,
//                         shield.transform.position.z
//                     );
//                     shield.AddGridForce(colVec, 1f, .3f, Color.red, false);
//                 }
//             }
//         }
//     }
// }



// // sticky code

// // public class StickyObject : MonoBehaviour
// // {
// //     private bool isStuck = false;
// //     private Transform targetTransform;

// //     private void OnCollisionEnter(Collision collision)
// //     {
// //         if (!isStuck && collision.gameObject.tag == "Sticky")
// //         {
// //             isStuck = true;
// //             targetTransform = collision.gameObject.transform;
// //             GetComponent<Rigidbody>().isKinematic = true;
// //         }
// //     }

// //     private void FixedUpdate()
// //     {
// //         if (isStuck)
// //         {
// //             transform.position = Vector3.Lerp(transform.position, targetTransform.position, Time.deltaTime * 10f);
// //             transform.rotation = Quaternion.Lerp(transform.rotation, targetTransform.rotation, Time.deltaTime * 10f);
// //         }
// //     }
// // }

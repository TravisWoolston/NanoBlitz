using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
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
    GameObject engine;
    public Quaternion targetRotation;
    private float offset = 90f;
    Vector2 moveDirection;
    public Vector2 target;
    public GameObject targetGameObject;
    private float explosionRadius = 50f;
    private float explosionForce = 100f;
    private float timer = 0;
    private float targetAngle;
    private Vector2 predictedTarget;
    private Vector2 targetDirection;
    private float time;
    private float distance;
    public AudioSource rocketBoostSFX;
    public AudioClip[] rocketSounds;
    UM uM;
    Color rippleColor = Color.blue;

    //     public AudioSource explosionSFX;
    //     private AudioClip exp;
    // public AudioClip[] explosions;

    void Start()
    {
        uM = UM.Instance;
        rb.angularDrag = 100;
        rb.drag = 1f;
        rb.mass = 1000;
        rb.gravityScale = 1;
        engine = Instantiate(engineParticles, exhaustPoint);

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
        // targetGameObject = EMTarget;
        target = EMTarget;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (gameObject.tag == "Missile")
        {
            if (
                collision.gameObject.tag == "BasicEnemy"
                || collision.gameObject.tag == "EnemyCaptain"
            )
            {
                Explode();
                timer = 0;
                gameObject.SetActive(false);
            }
        }
        else
        {
            if (collision.gameObject.tag == "Clone" || collision.gameObject.tag == "Player")
            {
                Explode();
                timer = 0;
                gameObject.SetActive(false);
            }
        }
    }

    private void Explode()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        if (gameObject.tag == "Missile")
        {
            foreach (Collider2D hit in colliders)
            {
                Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 direction = rb.transform.position - transform.position;
                    float distance = direction.magnitude;
                    float effect = 1 - (distance / explosionRadius);
                    Vector2 force = direction.normalized * explosionForce * effect;

                    if (hit.tag == "BasicEnemy")
                    {
                        rb.gameObject.GetComponent<EnemyBasic>().hp += effect * .4f;
                        // rb.AddForce(force, ForceMode2D.Impulse);
                        rb.AddForce(force * rb.mass * 50);
                    }
                    if (hit.tag == "EnemyCaptain")
                    {
                        rb.gameObject.GetComponent<EnemyBasic>().hp += effect * .2f;
                    }
                }
            }
        }
        else
        {
            foreach (Collider2D hit in colliders)
            {
                Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 direction = rb.transform.position - transform.position;
                    float distance = direction.magnitude;
                    float effect = 1 - (distance / explosionRadius);
                    Vector2 force = direction.normalized * explosionForce * effect;

                    if (rb.tag == "Clone")
                    {
                        rb.gameObject.GetComponent<PlayerCopy>().hp -= effect * .4f;
                        // rb.AddForce(force, ForceMode2D.Impulse);
                        rb.AddForce(force * rb.mass * 50);
                    }
                    if (rb.tag == "Player")
                    {
                        rb.GetComponent<PlayerController>().ApplyDmg(.1f);
                        // rb.AddForce(force, ForceMode2D.Impulse);
                        rb.AddForce(force * rb.mass * 50);
                    }
                }
            }
        }

        ParticleSystem spark = Instantiate(explosion, this.transform.position, Quaternion.identity);
        Destroy(spark, 2f);
        uM.AddGridForce(transform.position, -5, 8, rippleColor, true);
        uM.Explosion(transform);

        gameObject.SetActive(false);
    }

    Vector3 Accelerator()
    {
        moveDirection = rb.transform.up;
        accTimerBase += Time.deltaTime;
        accTimer = accTimerBase * Time.deltaTime * 10;
        float targetSpeed = moveSpeed * accTimer;
        float clampedSpeed = Mathf.Clamp(targetSpeed, 0, maxSpeed);
        float smoothAccel = Mathf.SmoothStep(0, clampedSpeed, acceleration * Time.deltaTime);

        return moveDirection * smoothAccel;
    }

    void FixedUpdate()
    {
        // engine.transform.position = exhaustPoint.position;
        if (targetGameObject != null && targetGameObject.activeInHierarchy)
        {
            target = targetGameObject.GetComponent<Rigidbody2D>().transform.position;
        }
        else
        {
            if (gameObject.tag == "Missile")
            {
                targetGameObject = UM.Instance.GetClosestGameObject(
                    UM.Instance.enemies,
                    transform.position
                );
                target = targetGameObject.GetComponent<Rigidbody2D>().transform.position;
            }
            // else Explode();
        }

        timer += Time.deltaTime;

        distance = Vector2.Distance(rb.transform.position, target);
        if (rb.velocity.magnitude == 0)
        {
            time = distance / 0.01f;
        }
        else
            time = distance / rb.velocity.magnitude;
        if (gameObject.tag != "EnemyMPF")
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
        if (ddot > .8)
        {
            rb.velocity = (Vector2)Accelerator();
        }
        else
        {
            accTimerBase = accTimerBaseReset;
            accTimer = accTimerBase;
            float decel = Mathf.Lerp(1, 0, Mathf.Pow(rb.velocity.magnitude / maxSpeed, 20));
            rb.velocity *= decel;
        }
        if (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 3f);
        }

        if (timer > 8)
        {
            Explode();
            timer = 0;
            gameObject.SetActive(false);
        }
    }
}

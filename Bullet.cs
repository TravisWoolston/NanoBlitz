using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float expiration = 0;

    // Start is called before the first frame update
    // void Start()
    // {

    // }

    // void OnEnable()
    // {
    //     if (this.gameObject.tag == "Bullet")
    //     {
    // GameObject player =
    // Physics2D.IgnoreCollision(
    //     GameObject.FindGameObjectWithTag("Player").GetComponent<Collider2D>(),
    //     GetComponent<Collider2D>()
    // );

    // GameObject[] allies = GameObject.FindGameObjectsWithTag("Clone");
    // foreach (GameObject obj in allies)
    // {
    //     Physics2D.IgnoreCollision(
    //         obj.GetComponent<Collider2D>(),
    //         GetComponent<Collider2D>()
    //     );
    // }
    // }
    // if (this.gameObject.tag == "EnemyBullet")
    // {
    //     GameObject[] allies = GameObject.FindGameObjectsWithTag("BasicEnemy");
    //     foreach (GameObject obj in allies)
    //     {
    //         Physics2D.IgnoreCollision(
    //             obj.GetComponent<Collider2D>(),
    //             GetComponent<Collider2D>()
    //         );
    //     }
    // }
    // }

    // private void OnBecameInvisible()
    // {
    //     gameObject.SetActive(false);
    // }
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (this.gameObject.tag == "Bullet")
        {
            if (col.gameObject.tag == "EnemyBasic")
                gameObject.SetActive(false);
            // if(col.gameObject.tag == "Clone"){
            //         gameObject.SetActive(false);

            // }
        }
        if (this.gameObject.tag == "EnemyBullet")
        {
            if (col.gameObject.tag == "Clone")
                gameObject.SetActive(false);
        }
        if (this.gameObject.tag != col.gameObject.tag)
            gameObject.SetActive(false);
    }

    private void OnCollissionExit2D(Collision2D col) { }

    void Update()
    {
        expiration += Time.deltaTime;
        if (expiration > 3)
        {
            gameObject.SetActive(false);
            expiration = 0;
        }
    }
}

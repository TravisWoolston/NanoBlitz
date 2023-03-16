using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool SharedInstance;
    public List<GameObject> bullets;
    public List<GameObject> enemyBullets;
    public List<GameObject> basicEnemies;
    public List<GameObject> basicAllies;
    public List<GameObject> enemyCpts;
    public List<GameObject> missiles;
    public List<GameObject> enemyMissiles;
    public GameObject bulletPrefab;
    public GameObject enemyBulletPrefab;
    public GameObject basicEnemyPrefab;
    public GameObject basicAllyPrefab;
    public GameObject enemyCptPrefab;
    public GameObject missilePrefab;
    public GameObject enemyMPF;
    public int amountToPool;
    UM unitManager;

    void Awake()
    {
        SharedInstance = this;
    }

    void Start()
    {
        unitManager = UM.Instance;
        bullets = new List<GameObject>();
        GameObject tmp;
        for (int i = 0; i < amountToPool; i++)
        {
            tmp = Instantiate(enemyCptPrefab);
            tmp.SetActive(false);
            enemyCpts.Add(tmp);
        }
        for (int i = 0; i < amountToPool; i++)
        {
            tmp = Instantiate(missilePrefab);
            tmp.SetActive(false);
            missiles.Add(tmp);
        }
        for (int i = 0; i < amountToPool; i++)
        {
            tmp = Instantiate(enemyMPF);
            tmp.SetActive(false);
            enemyMissiles.Add(tmp);
        }
        {
            tmp = Instantiate(missilePrefab);
            tmp.SetActive(false);
            enemyCpts.Add(tmp);
        }
        for (int i = 0; i < amountToPool; i++)
        {
            tmp = Instantiate(basicAllyPrefab);
            tmp.SetActive(false);
            basicAllies.Add(tmp);
        }
        for (int i = 0; i < amountToPool; i++)
        {
            tmp = Instantiate(bulletPrefab);
            tmp.SetActive(false);
            Physics2D.IgnoreCollision(
                GameObject.FindGameObjectWithTag("Player").GetComponent<Collider2D>(),
                GetComponent<Collider2D>()
            );

            //     GameObject[] allies = GameObject.FindGameObjectsWithTag("Clone");
            //     foreach (GameObject obj in basicAllies)
            //     {
            //         Debug.Log(obj);
            //         Physics2D.IgnoreCollision(
            //             obj.GetComponent<Collider2D>(),
            //             GetComponent<Collider2D>()
            //         );
            //     }
            bullets.Add(tmp);
        }
        for (int i = 0; i < amountToPool; i++)
        {
            tmp = Instantiate(enemyBulletPrefab);
            tmp.SetActive(false);
            enemyBullets.Add(tmp);
        }
        for (int i = 0; i < amountToPool; i++)
        {
            tmp = Instantiate(basicEnemyPrefab);
            tmp.SetActive(false);
            basicEnemies.Add(tmp);
        }
    }

    public GameObject GetBullet()
    {
        for (int i = 0; i < amountToPool; i++)
        {
            if (!bullets[i].activeInHierarchy)
            {
                return bullets[i];
            }
        }
        return null;
    }

    public GameObject GetEnemyBullet()
    {
        for (int i = 0; i < amountToPool; i++)
        {
            if (!enemyBullets[i].activeInHierarchy)
            {
                return enemyBullets[i];
            }
        }
        return null;
    }

    public GameObject GetBasicEnemy()
    {
        for (int i = 0; i < amountToPool; i++)
        {
            if (!basicEnemies[i].activeInHierarchy)
            {
                unitManager.NewAI();
                return basicEnemies[i];
            }
        }
        return null;
    }

    public GameObject GetBasicAlly()
    {
        for (int i = 0; i < amountToPool; i++)
        {
            if (!basicAllies[i].activeInHierarchy)
            {
                return basicAllies[i];
            }
        }
        return null;
    }

    public GameObject GetEnemyCpt()
    {
        for (int i = 0; i < amountToPool; i++)
        {
            if (!enemyCpts[i].activeInHierarchy)
            {
                return enemyCpts[i];
            }
        }
        return null;
    }

    public GameObject GetMissile()
    {
        for (int i = 0; i < amountToPool; i++)
        {
            if (!missiles[i].activeInHierarchy)
            {
                return missiles[i];
            }
        }
        return null;
    }

    public GameObject GetEM()
    {
        for (int i = 0; i < amountToPool; i++)
        {
            if (!enemyMissiles[i].activeInHierarchy)
            {
                return enemyMissiles[i];
            }
        }
        return null;
    }
}

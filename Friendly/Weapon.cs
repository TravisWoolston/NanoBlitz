using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireForce = 120f;

    // public GameObject sparkPrefab;
    void Start()
    {
        fireForce = 80f;
    }

    public void Fire()
    {
        GameObject bullet = ObjectPool.SharedInstance.GetBullet();
        if (bullet != null)
        {
            bullet.transform.position = firePoint.position;
            // bullet.transform.rotation = firePoint.rotation;

            bullet.SetActive(true);
            bullet
                .GetComponent<Rigidbody2D>()
                .AddForce(firePoint.up * fireForce, ForceMode2D.Impulse);
        }
        // GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);


        // Object.Destroy(this.gameObject, 1);
    }

    public void FireMissile(PlayerController playerC)
    {
        GameObject missile = ObjectPool.SharedInstance.GetMissile();
        if (missile != null)
        {
            missile.transform.position = firePoint.position;
            missile.transform.rotation = transform.rotation;

            missile.GetComponent<Missile>().SetPlayerTarget(playerC);

            missile.SetActive(true);
            missile.GetComponent<Rigidbody2D>().velocity += (Vector2)(firePoint.up * 100);
            //  missile.GetComponent<Rigidbody2D>().AddForce(firePoint.up * 1000);
        }
    }

    public void FireEM()
    {
        GameObject missile = ObjectPool.SharedInstance.GetEM();
        if (missile != null)
        {
            missile.transform.position = firePoint.position;
            missile.transform.rotation = transform.rotation;

            // missile.GetComponent<Missile>().SetTarget(fireTarget);

            missile.SetActive(true);
            missile.GetComponent<Rigidbody2D>().velocity += (Vector2)(firePoint.up * 100);
            //  missile.GetComponent<Rigidbody2D>().AddForce(firePoint.up * 1000);
        }
    }
}

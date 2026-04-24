using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class BlunderBuss : Gun
{
    [SerializeField] private GameObject bullets;
    [SerializeField] private GameObject transvormPivit;
    [SerializeField] private float bulletSpeed = 1.0f;
    [SerializeField] private int bulletAmount = 18;
    [SerializeField] private float damage = 3;
    [SerializeField] private float spread = 10f; 
    private float ammoLife = 1f;
    private void Start()
    {
        Kind = KindofGun.BunderBuss;
    }
    public override void Schoot()
    {
        base.Schoot();
        if (shootRate<=cooldown)
        {
            for (int i = 0; i < bulletAmount; i++)
            {
                GameObject bullet = Instantiate(bullets, transvormPivit.transform.position, transvormPivit.transform.rotation);
                Rigidbody rb = bullet.GetComponent<Rigidbody>();
                Bullet amunition = bullet.GetComponent<Bullet>();
                amunition.damage = damage;
                amunition.timeTillDeath = ammoLife;
                amunition.type = BulletType.Normal;
                bullet.transform.rotation = Quaternion.RotateTowards(bullet.transform.rotation, Random.rotation, spread);
                rb.AddForce(bullet.transform.forward*100* bulletSpeed);
            }
            cooldown = 0f;
        }
    }
    private void Update()
    {
        if (shootRate >= cooldown)
        {
            cooldown += Time.deltaTime;
        }
    }
}

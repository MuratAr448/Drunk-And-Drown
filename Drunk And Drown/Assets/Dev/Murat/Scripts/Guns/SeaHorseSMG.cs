using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SeaHorseSMG : Gun
{
    [SerializeField] private GameObject bullets;
    [SerializeField] private GameObject transvormPivit;
    [SerializeField] private float bulletSpeed = 1.0f;
    [SerializeField] private int bulletAmount = 18;
    [SerializeField] private float damage = 3;
    [SerializeField] private float spread = 10f; 
    private float ammoLife = 1f;
    [SerializeField] private Movement player;
    private void Start()
    {
        player = FindFirstObjectByType<Movement>();
        Kind = KindofGun.BunderBuss;
    }
    public override void Schoot()
    {
        base.Schoot();
        if (shootRate1 <= cooldown1 && Input.GetKey(KeyCode.Mouse0))
        {
            GameObject bullet = Instantiate(bullets, transvormPivit.transform.position, transvormPivit.transform.rotation);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            Bullet amunition = bullet.GetComponent<Bullet>();
            amunition.damage = damage;
            amunition.timeTillDeath = ammoLife;
            amunition.type = BulletType.Normal;
            amunition.Lanched();
            RaycastHit hit;
            GameObject Origin = player.GetComponent<MainPlayer>().rayOrigin;
            if (Physics.Raycast(Origin.transform.position, Origin.transform.forward, out hit))
            {
                rb.transform.LookAt(hit.point);
            }
            rb.AddForce(Origin.transform.forward * 100 * bulletSpeed);
            cooldown1 = 0f;
        }
    }
    public override void SecondDairy()
    {
        base.SecondDairy();
        if (shootRate2 <= cooldown2&& Input.GetKeyDown(KeyCode.Mouse1))
        {
            for (int i = 0; i < bulletAmount; i++)
            {
                GameObject bullet = Instantiate(bullets, transvormPivit.transform.position, transvormPivit.transform.rotation);
                Rigidbody rb = bullet.GetComponent<Rigidbody>();
                Bullet amunition = bullet.GetComponent<Bullet>();
                amunition.damage = damage;
                amunition.timeTillDeath = ammoLife;
                amunition.type = BulletType.Normal;
                amunition.Lanched();
                bullet.transform.rotation = Quaternion.RotateTowards(bullet.transform.rotation, Random.rotation, spread);
                rb.AddForce(bullet.transform.forward * 100 * bulletSpeed);
            }
            cooldown2 = 0f;
        }
    }
    private void Update()
    {
        if (shootRate1 >= cooldown1)
        {
            cooldown1 += Time.deltaTime;
        }
        if (shootRate2 >= cooldown2)
        {
            cooldown2 += Time.deltaTime;
        }
    }
}

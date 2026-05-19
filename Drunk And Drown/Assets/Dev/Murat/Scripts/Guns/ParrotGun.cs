using UnityEngine;
using UnityEngine.InputSystem;

public class ParrotGun : Gun
{
    [SerializeField] private GameObject Parrot;
    private GameObject bullet;
    [SerializeField] private GameObject transvormPivit;
    [SerializeField] private float bulletSpeed = 1.0f;
    [SerializeField] private float damage = 3;
    [SerializeField] private float radius = 4;
    [SerializeField] private float force = 5;
    [SerializeField] private Movement player;
    private float ammoLife = 3f;
    void Start()
    {
        player = FindFirstObjectByType<Movement>();
        Kind = KindofGun.ParrotGun;
    }
    public override void Schoot()
    {
        base.Schoot();
        if (shootRate <= cooldown && bullet != null)
        {

            bullet.GetComponent<Rigidbody>().isKinematic = false;
            bullet.transform.parent = null;
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            Bullet amunition = bullet.GetComponent<Bullet>();
            amunition.GetComponent<SphereCollider>().enabled = true;
            amunition.damage = damage;
            amunition.timeTillDeath = ammoLife;
            amunition.type = BulletType.Explosive;
            amunition.radius = radius;
            amunition.force = force;
            RaycastHit hit;
            GameObject Origin = player.GetComponent<MainPlayer>().rayOrigin;
            if (Physics.Raycast(Origin.transform.position, Origin.transform.forward, out hit))
            {
                rb.transform.LookAt(hit.point);
            }
            rb.AddForce(Origin.transform.forward * 100 * bulletSpeed);
            cooldown = 0f;
            player.Exposion();
            player.GetComponent<Rigidbody>().AddForce(-Origin.transform.forward * force*2.5f, ForceMode.Impulse);
            bullet = null;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (shootRate >= cooldown)
        {
            cooldown += Time.deltaTime;
        }
        if (bullet==null&& shootRate <= cooldown+Time.deltaTime*3)
        {
            bullet = Instantiate(Parrot, transvormPivit.transform);
            bullet.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

}

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
        if (shootRate1 <= cooldown1 && bullet != null && Input.GetKeyDown(KeyCode.Mouse0))
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
            cooldown1 = 0f;
            player.Exposion();
            player.GetComponent<Rigidbody>().AddForce(-Origin.transform.forward * force*2.5f, ForceMode.Impulse);
            bullet = null;
        }
    }
    public override void SecondDairy()
    {
        base.SecondDairy();
        if (shootRate2 <= cooldown1 && bullet != null&& Input.GetKeyDown(KeyCode.Mouse1))
        {
            cooldown1 = 0f;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (shootRate1 >= cooldown1)
        {
            cooldown1 += Time.deltaTime;
        }
        if (bullet==null&& shootRate1 <= cooldown1+Time.deltaTime*3)
        {
            bullet = Instantiate(Parrot, transvormPivit.transform);
            bullet.GetComponent<Rigidbody>().isKinematic = true;
            bullet.GetComponent<SphereCollider>().enabled = false;
        }
    }

}

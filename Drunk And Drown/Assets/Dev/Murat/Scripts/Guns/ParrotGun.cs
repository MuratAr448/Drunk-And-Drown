using UnityEngine;

public class ParrotGun : Gun
{
    [SerializeField] private GameObject Parrot;
    [SerializeField] private GameObject bullet;
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
            amunition.damage = damage;
            amunition.timeTillDeath = ammoLife;
            amunition.type = BulletType.Exsplosive;
            amunition.radius = radius;
            amunition.force = force;
            rb.AddForce(bullet.transform.forward * 100 * bulletSpeed);
            cooldown = 0f;
            StartCoroutine(player.Impact(-bullet.transform.forward * bulletSpeed * Time.deltaTime));
            //player.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * bulletSpeed*Time.deltaTime);
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

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class SquidRayGun : Gun
{
    [SerializeField] private float Basedamage = 1;
    [SerializeField] private float size = 0.01f;
    private MainPlayer player;
    [SerializeField] private GameObject inkPrefab;
    private GameObject inkRay;
    [SerializeField] private LayerMask layerMask;
    private Collider previosOponent;
    private bool ableToHit = false;
    public Enemy enemy;

    void Start()
    {
        player = FindFirstObjectByType<MainPlayer>();
        Kind = KindofGun.SquidRayGun;
    }

    public override void Shoot()
    {
        base.Shoot();
        if (Input.GetKey(KeyCode.Mouse0))
        {
            cooldown1 += Time.deltaTime*2;
        }
    }
    private void CoolingDown()
    {
        if(!Input.GetKey(KeyCode.Mouse0))
        {
            cooldown1 -= Time.deltaTime;

            if (cooldown1 < 0f)
            {
                cooldown1 = 0;
                ableToHit = false;
                if (inkRay != null)
                {
                    Destroy(inkRay);
                }
            }
        }
        else
        {
            if (inkRay == null)
            {
                inkRay = Instantiate(inkPrefab, gameObject.transform);
            }
            ableToHit = true;
            if (cooldown1>= shootRate1)
            {
                cooldown1 = shootRate1;
            }
        }
        size = cooldown1 * 0.5f;
        RaycastHit hit;
        GameObject Origin = player.rayOrigin;
        if (Physics.Raycast(Origin.transform.position, Origin.transform.forward, out hit, 20, layerMask) && inkRay != null)
        {
            if (hit.collider != inkRay.GetComponent<Collider>())
            {
                float distance = Vector3.Distance(transform.position, hit.point);
                transform.LookAt(hit.point);
                inkRay.transform.rotation = transform.rotation;
                inkRay.transform.localScale = new Vector3(size, size, distance);
                inkRay.transform.localPosition = Vector3.forward * (distance * 0.3f);
                Collider Oponent = hit.collider;
                if (Oponent.TryGetComponent(out IDamageable damageable) && ableToHit)
                {
                    if (Oponent.gameObject.TryGetComponent(out RayDamage RD))
                    {
                        if (RD.Iframe <= 0)
                        {
                            RD.RayIframe();
                            if (previosOponent != Oponent)
                            {
                                RD.Multiplyer = 0;
                            }
                            RD.Multiplyer += Basedamage;
                            damageable.TakeDamage(RD.Multiplyer);
                        }
                    }
                    else
                    {
                        Oponent.gameObject.AddComponent<RayDamage>();
                        damageable.TakeDamage(Basedamage);
                    }
                    previosOponent = Oponent;
                }
            }
        }
        else if(inkRay != null)
        {
            inkRay.transform.localPosition = Vector3.forward * (20 * 0.3f);
            inkRay.transform.rotation = transform.rotation;
            inkRay.transform.localScale = new Vector3(size, size, 20);
        }
    }
    public override void Secondary()
    {
        base.Secondary();
        if (shootRate2 <= cooldown2 && Input.GetKeyDown(KeyCode.Mouse1))
        {
            RaycastHit hit;
            GameObject Origin = player.rayOrigin;
            if (Physics.Raycast(Origin.transform.position, Origin.transform.forward, out hit, 20, layerMask))
            {
                if (hit.collider.TryGetComponent(out IDamageable damageable))
                {
                    enemy = hit.transform.GetComponent<Enemy>();
                    StartCoroutine(ToEnemy(enemy));
                }
            }
        }
    }
    private IEnumerator ToEnemy(Enemy enemyPos)
    {
        float distance = Vector3.Distance(player.transform.position, enemyPos.transform.position);
        float damage = distance;
        while (distance > 1.5f)
        {
            //stun enemy
            distance = Vector3.Distance(player.transform.position, enemyPos.transform.position);
            player.transform.position = Vector3.MoveTowards(player.transform.position, enemyPos.transform.position, 0.2f* distance);
            yield return new WaitForSeconds(Time.deltaTime);
            cooldown2 = 0f;
        }
        enemy.TakeDamage(damage);
        enemy = null;
    }
    void Update()
    {
        if (shootRate2 >= cooldown2)
        {
            cooldown2 += Time.deltaTime;
        }
        CoolingDown();
    }
}

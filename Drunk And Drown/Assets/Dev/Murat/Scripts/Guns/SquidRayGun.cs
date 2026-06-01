using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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
    [SerializeField] private float test = 0;

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
            cooldown1 += Time.deltaTime;
            if (size < 1f)
            {
                size += Time.deltaTime;
            }
        }
    }
    private void CoolingDown()
    {
        if(!Input.GetKey(KeyCode.Mouse0))
        {
            cooldown1 -= Time.deltaTime;
            if (size > 0.1f)
            {
                size -= Time.deltaTime;
            }

            if (cooldown1 < 1f)
            {
                cooldown1 = 0;
                ableToHit = false;
                if (inkRay != null)
                {
                    Destroy(inkRay);
                }
            }
        }
        RaycastHit hit;
        GameObject Origin = player.rayOrigin;
        if (Physics.Raycast(Origin.transform.position, Origin.transform.forward, out hit, test, layerMask) && inkRay != null)
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
            inkRay.transform.localPosition = Vector3.forward * (test * 0.3f);
            inkRay.transform.rotation = transform.rotation;
            inkRay.transform.localScale = new Vector3(size, size, test);
        }
        Debug.DrawLine(Origin.transform.position, Origin.transform.position + Origin.transform.forward * test);
        if (shootRate1 < cooldown1)
        {
            if (inkRay == null)
            {
                inkRay = Instantiate(inkPrefab, gameObject.transform);
            }
            ableToHit = true;
            cooldown1 = shootRate1;
        }
    }
    public override void Secondary()
    {
        base.Secondary();
        if (shootRate2 <= cooldown2 && Input.GetKeyDown(KeyCode.Mouse1))
        {
            RaycastHit hit;
            GameObject Origin = player.rayOrigin;
            if (Physics.Raycast(Origin.transform.position, Origin.transform.forward, out hit))
            {

            }
            cooldown2 = 0f;
        }
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

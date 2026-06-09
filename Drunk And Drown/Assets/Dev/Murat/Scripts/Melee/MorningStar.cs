using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class MorningStar : Melee
{
    [SerializeField] private float damage = 10;
    [SerializeField] private float hitDuration = 0;
    [SerializeField] private SphereCollider colliderTrig;
    [SerializeField] private GameObject pufferFish;
    [SerializeField] private GameObject smokePref;
    private List<Enemy> hitEnemys = new List<Enemy>();
    [SerializeField] private ExplosiveHammer explosive;
    public bool Attacking = false;
    void Start()
    {
        Kind = KindofMelee.MorningStar;
    }
    public override void Swing()
    {
        base.Swing();
        if(swingRate1 < cooldown1&& Input.GetKeyDown(KeyCode.Mouse0)&&!Attacking)
        {
            cooldown1 = 0f;
            StartCoroutine(NormalAttack());
            Expand();
        }
    }
    public override void SecondDairy()
    {
        base.SecondDairy();
        if (swingRate2 < cooldown2 && Input.GetKeyDown(KeyCode.Mouse1)&& !Attacking)
        {
            Expand();
            StartCoroutine(PowerSlam());
            cooldown2 = 0f;
        }
    }
    private IEnumerator NormalAttack()
    {
        //movement
        yield return new WaitForSeconds(1f);
        Deflate();
    }
    private IEnumerator PowerSlam()
    {
        //movement
        yield return new WaitForSeconds(1f);
        explosive.damagedObjects.Clear();
        explosive.Explode();
        GameObject smoke = Instantiate(smokePref,pufferFish.transform.position, pufferFish.transform.rotation,null);
        Destroy(smoke,3f);
        StartCoroutine(Smoke(smoke, explosive.radius));
        yield return new WaitForSeconds (1f);
        Deflate();
        //return
    }
    private IEnumerator Smoke(GameObject Smoke, float size)
    {
        Smoke.transform.localScale = Vector3.one * size;
        while (Smoke)
        {
            Smoke.transform.Rotate(Vector3.right);
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }
    void Update()
    {
        if (cooldown1>=swingRate1&&cooldown2>=swingRate2&&Attacking)
        {
            Deflate();
        }
        if (swingRate1 >= cooldown1)
        {
            cooldown1 += Time.deltaTime;
        }

        if (swingRate2 >= cooldown2)
        {
            cooldown2 += Time.deltaTime;
        }
    }
    private void Expand()
    {
        colliderTrig.enabled = true;
        colliderTrig.radius = 0.3f;
        pufferFish.transform.localScale = Vector3.one * 0.6f;
        Attacking = true;
    }
    private void Deflate()
    {
        colliderTrig.enabled = false;
        colliderTrig.radius = 0.15f;
        pufferFish.transform.localScale = Vector3.one * 0.3f;
        hitEnemys.Clear();
        Attacking = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Enemy>())
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            if (!hitEnemys.Contains(enemy)&& Attacking)
            {
                enemy.TakeDamage(damage);
                hitEnemys.Add(enemy);
            }
        }
    }
}

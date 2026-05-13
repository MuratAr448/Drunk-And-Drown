using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class MorningStar : Melee
{
    [SerializeField] private float damage = 10;
    [SerializeField] private float hitDuration = 0;
    [SerializeField] private SphereCollider colliderTrig;
    [SerializeField] private GameObject PufferFish;
    private List<Enemy> hitEnemys = new List<Enemy>();  
    void Start()
    {
        Kind = KindofMelee.MorningStar;
    }
    public override void Swing()
    {
        base.Swing();
        if(swingRate < cooldown)
        {
            cooldown = 0f;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (swingRate >= cooldown)
        {
            cooldown += Time.deltaTime;
        }
        if (cooldown >= hitDuration)
        {
            colliderTrig.enabled = false;
            colliderTrig.radius = 0.15f;
            PufferFish.transform.localScale = Vector3.one*0.3f;
            hitEnemys.Clear();
        }else
        {
            colliderTrig.enabled = true;
            colliderTrig.radius = 0.3f;
            PufferFish.transform.localScale = Vector3.one * 0.6f;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject);
        if (other.CompareTag("Enemy") && other.GetComponent<Enemy>())
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            if (!hitEnemys.Contains(enemy))
            {
                enemy.TakeDamage(damage);
                hitEnemys.Add(enemy);
            }
        }
    }
}

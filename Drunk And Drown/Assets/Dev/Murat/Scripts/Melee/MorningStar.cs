using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


public class MorningStar : Melee
{
    [SerializeField] private float damage = 10;
    [SerializeField] private SphereCollider colliderTrig;
    [SerializeField] private GameObject smokePref;
    private List<IDamageable> hitEnemys = new List<IDamageable>();
    [SerializeField] private ExplosiveHammer explosive;
    public bool Attacking = false;
    private Movement player;
    [SerializeField] private Animation _currentAnimation;
    [SerializeField] private List<AnimationClip> _currentAnimationClip;
    public override float GetDamage() { return damage; }
    public override void ApplyRarityScaling(float multiplier)
    {
        base.ApplyRarityScaling(multiplier);
        damage *= multiplier;
        if (explosive != null)
        {
            explosive.damage *= multiplier;
        }
    }

    void Start()
    {
        explosive.damage = damage*0.5f;
        player = FindFirstObjectByType<Movement>();
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

            if (shootSound != null && audioSource != null)
            {
                shootSound.Play(audioSource);
            }
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

            if (shootSound != null && audioSource != null)
            {
                shootSound.Play(audioSource);
            }
        }
    }
    private IEnumerator NormalAttack()
    {
        //movement
        _currentAnimation.Play(_currentAnimationClip[0].name);
        yield return new WaitForSeconds(1f);
        Deflate();
    }
    private IEnumerator PowerSlam()
    {
        //movement
        while(!player.isGrounded)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            cooldown2 = 0f;
            //falling
        }
        _currentAnimation.Play(_currentAnimationClip[2].name);
        yield return new WaitForSeconds(0.5f);
        explosive.damagedObjects.Clear();
        explosive.Explode();
        GameObject smoke = Instantiate(smokePref,colliderTrig.transform.position, colliderTrig.transform.rotation,null);
        Destroy(smoke,3f);
        StartCoroutine(Smoke(smoke, explosive.radius));
        yield return new WaitForSeconds (0.5f);
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
        Attacking = true;
    }
    private void Deflate()
    {
        _currentAnimation.Play(_currentAnimationClip[1].name);
        colliderTrig.enabled = false;
        hitEnemys.Clear();
        Attacking = false;
    }
    public void HitEnemy(Collider other)
    {
        if (other.TryGetComponent(out IDamageable Damageable))
        {

            if (!hitEnemys.Contains(Damageable) && Attacking)
            {
                Damageable.TakeDamage(damage);
                hitEnemys.Add(Damageable);
            }
        }
    }
}

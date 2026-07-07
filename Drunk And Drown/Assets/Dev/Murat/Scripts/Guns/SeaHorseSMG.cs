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
    [SerializeField] private ParticleSystem shootParticles;
    private float ammoLife = 1f;
    private MainPlayer player;
    [SerializeField] private Animation _currentAnimation;
    [SerializeField] private List<AnimationClip> _currentAnimationClip;

    public override float GetDamage() { return damage; }
    public override void ApplyRarityScaling(float multiplier)
    {
        base.ApplyRarityScaling(multiplier);
        damage *= multiplier;
        for (int i = 0; i < _currentAnimationClip.Count; i++)
        {
            _currentAnimation[_currentAnimationClip[i].name].speed *= multiplier;
        }
    }

    private void Start()
    {
        player = FindFirstObjectByType<MainPlayer>();
        Kind = KindofGun.BunderBuss;
    }
    public override void Shoot()
    {
        base.Shoot();
        if (shootRate1 <= cooldown1 && Input.GetKey(KeyCode.Mouse0))
        {
            if (!_currentAnimation.IsPlaying(_currentAnimationClip[1].name))
            {
                _currentAnimation.Play(_currentAnimationClip[0].name);
            }

            GameObject bullet = Instantiate(bullets, transvormPivit.transform.position, transvormPivit.transform.rotation);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            Bullet amunition = bullet.GetComponent<Bullet>();
            amunition.damage = damage;
            amunition.timeTillDeath = ammoLife;
            amunition.type = BulletType.Normal;
            amunition.Lanched();

            GameObject Origin = player.rayOrigin;
            Vector3 targetPoint;
            int mask = ~(1 << player.gameObject.layer);
            if (Physics.Raycast(Origin.transform.position, Origin.transform.forward, out RaycastHit hit, 1000f, mask, QueryTriggerInteraction.Ignore))
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = Origin.transform.position + Origin.transform.forward * 100f;
            }

            Vector3 fireDirection = (targetPoint - bullet.transform.position).normalized;
            bullet.transform.forward = fireDirection;
            rb.AddForce(fireDirection * 100 * bulletSpeed);
            cooldown1 = 0f;

            if (shootSound != null && audioSource != null)
            {
                shootSound.PlayOneShot(audioSource);
            }
        }
    }
    public override void Secondary()
    {
        base.Secondary();
        if (shootRate2 <= cooldown2 && Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (shootParticles != null)
            {
                shootParticles.Play();
            }
            _currentAnimation.Play(_currentAnimationClip[1].name);
            GameObject Origin = player.rayOrigin;
            Vector3 targetPoint;
            int mask = ~(1 << player.gameObject.layer);
            if (Physics.Raycast(Origin.transform.position, Origin.transform.forward, out RaycastHit hit, 1000f, mask, QueryTriggerInteraction.Ignore))
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = Origin.transform.position + Origin.transform.forward * 100f;
            }

            for (int i = 0; i < bulletAmount; i++)
            {
                GameObject bullet = Instantiate(bullets, transvormPivit.transform.position, transvormPivit.transform.rotation);
                Rigidbody rb = bullet.GetComponent<Rigidbody>();
                Bullet amunition = bullet.GetComponent<Bullet>();
                amunition.damage = damage;
                amunition.timeTillDeath = ammoLife;
                amunition.type = BulletType.Normal;
                amunition.Lanched();

                Vector3 fireDirection = (targetPoint - bullet.transform.position).normalized;
                bullet.transform.forward = fireDirection;
                bullet.transform.rotation = Quaternion.RotateTowards(bullet.transform.rotation, Random.rotation, spread);
                rb.AddForce(bullet.transform.forward * 100 * bulletSpeed);
            }
            cooldown2 = 0f;

            if (shootSound != null && audioSource != null)
            {
                shootSound.PlayOneShot(audioSource);
            }

            if (player != null)
            {
                player.TriggerCameraShake(0.15f, 0.08f);
            }
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
        if (!Input.GetKey(KeyCode.Mouse0) && !Input.GetKey(KeyCode.Mouse1)&&!_currentAnimation.IsPlaying(_currentAnimationClip[1].name))
        {
            _currentAnimation.Play(_currentAnimationClip[2].name);
        }
        if (shootParticles != null && shootParticles.isPlaying)
        {
            if (!Input.GetKey(KeyCode.Mouse0) && !Input.GetKey(KeyCode.Mouse1))
            {
                shootParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }
}

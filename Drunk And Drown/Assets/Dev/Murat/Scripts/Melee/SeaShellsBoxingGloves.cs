using System.Collections;
using UnityEngine;
using static UnityEngine.UI.Image;

public class SeaShellsBoxingGloves : Melee
{
    [SerializeField] private float damage = 3;
    [SerializeField] private float lunge;
    public bool Lunging = false;
    private Movement movement;
    private MainPlayer player;

    public override float GetDamage() { return damage; }
    public override void ApplyRarityScaling(float multiplier)
    {
        base.ApplyRarityScaling(multiplier);
        damage *= multiplier;
    }

    void Start()
    {
        movement = FindFirstObjectByType<Movement>();
        player = movement.GetComponent<MainPlayer>();
        Kind = KindofMelee.SeaShellsBoxingGloves;
    }
    public override void Swing()
    {
        base.Swing();
        if (swingRate1 < cooldown1 && Input.GetKey(KeyCode.Mouse0) && !Lunging)
        {
            NormalAttack();
            cooldown1 = 0f;

            if (shootSound != null && audioSource != null)
            {
                shootSound.Play(audioSource);
            }
        }

    }
    public override void SecondDairy()
    {
        base.SecondDairy();
        if (swingRate2 < cooldown2 && Input.GetKeyDown(KeyCode.Mouse1)&& !Lunging)
        {
            cooldown2 = 0f;
            StartCoroutine(Lunge());
            Lunging = true;

            if (shootSound != null && audioSource != null)
            {
                shootSound.Play(audioSource);
            }
        }
    }
    private void NormalAttack()
    {
        Collider[] colliders = Physics.OverlapSphere(movement.transform.position + movement.transform.forward*2,2);
        foreach (Collider col in colliders)
        {
            if (col.TryGetComponent(out Enemy enemy))
            {
                enemy.TakeDamage(damage);
            }
        }
    }
    private IEnumerator Lunge()
    {
        GameObject origin = player.rayOrigin;
        Vector3 fireDirection = origin.transform.forward;
        movement.ApplyKnockback(fireDirection * lunge);

        Collider[] colliders = Physics.OverlapBox(movement.transform.position, new Vector3(1, 1, lunge) * 0.5f, origin.transform.rotation);

        yield return new WaitForSeconds(Time.deltaTime*2);

        foreach (Collider col in colliders)
        {
            if (col.TryGetComponent(out Enemy enemy))
            {
                enemy.TakeDamage(damage * 5);
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (swingRate1 >= cooldown1)
        {
            cooldown1 += Time.deltaTime;
        }

        if (swingRate2 >= cooldown2)
        {
            cooldown2 += Time.deltaTime;
        }
        if (cooldown2>=1)
        {
            Lunging = false;
        }
    }
}

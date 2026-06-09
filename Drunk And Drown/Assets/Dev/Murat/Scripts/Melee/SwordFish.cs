using UnityEngine;

public class SwordFish : Melee
{
    [SerializeField] private float damage = 10;
    void Start()
    {
        Kind = KindofMelee.SwordFish;
    }
    public override void Swing()
    {
        base.Swing();
        if (swingRate1 < cooldown1 && Input.GetKeyDown(KeyCode.Mouse0))
        {
            cooldown1 = 0f;
        }
    }
    public override void SecondDairy()
    {
        base.SecondDairy();
        if (swingRate2 < cooldown2 && Input.GetKeyDown(KeyCode.Mouse1))
        {
            cooldown2 = 0f;
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
    }
}

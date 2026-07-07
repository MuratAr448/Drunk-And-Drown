using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Lava : MonoBehaviour
{
    private float timeForDamage = 0;
    private void Update()
    {
        timeForDamage += Time.deltaTime;
        if (timeForDamage > 1)
        {
            float radius = transform.localScale.x;
            if (radius < transform.localScale.z)
            {
                radius = transform.localScale.z;
            }
            LavaCylinder(transform.position-Vector3.up* transform.lossyScale.y, transform.position + Vector3.up * transform.lossyScale.y, radius, transform.localScale.x, transform.localScale.z);
            timeForDamage = 0;
        }
    }
    public void LavaCylinder(Vector3 aStart, Vector3 aEnd, float Radius,float ScaleX, float ScaleZ)
    {
        Collider[] CapsuleCollider = Physics.OverlapCapsule(aStart, aEnd, Radius*0.5f);
        Vector3 dir = aEnd - aStart;
        Collider[] BoxCollider = Physics.OverlapBox(aStart + dir * 0.5f, new Vector3(ScaleX, dir.magnitude * 0.5f, ScaleZ));
        List<Collider> CapColliders = new List<Collider>();
        List<Collider> Colliders = new List<Collider>();
        for (int i =0;i<CapsuleCollider.Length ;i++ )
        {
            CapColliders.Add(CapsuleCollider[i]);
        }
        for (int i = 0; i < BoxCollider.Length; i++)
        {
            if (CapColliders.Contains(BoxCollider[i]))
            {
                Colliders.Add(BoxCollider[i]);
            }  
        }

        foreach (Collider col in Colliders)
        {
            if (col.TryGetComponent(out IDamageable Damaged))
            {
                Damaged.TakeDamage(5);
            }
        }
    }
}

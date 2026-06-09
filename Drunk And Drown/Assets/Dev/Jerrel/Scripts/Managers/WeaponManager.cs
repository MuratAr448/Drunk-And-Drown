using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Serializable]
    public struct Weapon
    {
        public string id;
        public GameObject weaponObject;
    }

    public List<Weapon> allWeapons = new List<Weapon>();
    
    public void AddWeapon(Weapon weaponData)
    {
        if (weaponData.weaponObject == null) return;
        MainPlayer.AddWeaponToList(weaponData.weaponObject);
    }

    [ContextMenu("Give Parrotgun")]
    public void GiveParrotgun()
    {
        if (allWeapons != null && allWeapons.Count > 0 && allWeapons[0].weaponObject != null)
        {
            MainPlayer.AddWeaponToList(allWeapons[0].weaponObject);
        }
    }
    public void GiveMorningStar()
    {
        if (allWeapons != null && allWeapons.Count > 0 && allWeapons[1].weaponObject != null)
        {
            MainPlayer.AddWeaponToList(allWeapons[1].weaponObject);
        }
    }
    public void GiveSeaHorseSMG()
    {
        if (allWeapons != null && allWeapons.Count > 0 && allWeapons[2].weaponObject != null)
        {
            MainPlayer.AddWeaponToList(allWeapons[2].weaponObject);
        }
    }
    public void GiveSquidRayGun()
    {
        if (allWeapons != null && allWeapons.Count > 0 && allWeapons[3].weaponObject != null)
        {
            MainPlayer.AddWeaponToList(allWeapons[3].weaponObject);
        }
    }
}
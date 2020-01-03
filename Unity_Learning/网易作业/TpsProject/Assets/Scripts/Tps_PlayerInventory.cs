using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tps_PlayerInventory : MonoBehaviour
{
    private List<int> keysArr;
    private List<Tps_Weapon> weaponContainer;

    private void Start()
    {
        keysArr = new List<int>();
        weaponContainer = new List<Tps_Weapon>();
        //初始有一把默认的枪支
        weaponContainer.Add(new Tps_Weapon_fal());
    }

    public void AddKey(int keyId)
    {
        if (!keysArr.Contains(keyId))
            keysArr.Add(keyId);
    }

    public bool HasKey(int doorId)
    {
        if (keysArr.Contains(doorId))
            return true;
        return false;
    }

    public void AddWeapon(Tps_Weapon weapon)
    {
        if (!weaponContainer.Contains(weapon))
        {
            weaponContainer.Add(weapon);
        }
    }

    public bool HasWeapon(Tps_Weapon weapon)
    {
        if (weaponContainer.Contains(weapon))
            return true;
        return false;
    }

    public int FindeWeaponIndex(Tps_Weapon weapon)
    {
        if (weaponContainer.Contains(weapon))
            return weaponContainer.IndexOf(weapon);
        return -1;
    }


    public int WeaponTypeNumber
    {
        get
        {
            return weaponContainer.Count;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Weapon : MonoBehaviour {

    public float damage;
    public float rateOfFire;
    public int ammo;
    public float reloadTime;
    public bool automatic;

    public float projectileSpeed;

    public void Fire() {

    }
}

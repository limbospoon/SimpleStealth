using UnityEngine;

public class Weapon : MonoBehaviour, IWeapon
{
    public int clipSize = 6;
    public int clipCount = 3;
    public float fireRange = 100.0f;
    public float fireRate = 0.3f;

    private float lastFireTime = 0.0f;

    public void Fire()
    {
        throw new System.NotImplementedException();
    }

    public void Reload()
    {
        throw new System.NotImplementedException();
    }
}

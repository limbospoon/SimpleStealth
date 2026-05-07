using System.Collections;
using UnityEngine;

public enum WeaponState
{
    IDLE,
    FIRING,
    RELOADING
}

public class Weapon : MonoBehaviour, IWeapon
{
    public int clipSize = 6;
    public int clipCount = 3;
    public float fireRange = 100.0f;
    public float fireRate = 0.3f;
    public float realoadSpeed = 0.3f;
    public WeaponState weaponState;
    public bool bShowDebugMsg = false;

    public int currentClipSize;
    private int currentClipCount;
    public int maxAmmo;
    public int currentAmmo;

    void Start()
    {
        weaponState = WeaponState.IDLE;
        currentClipSize = clipSize;
        currentClipCount = clipCount;
        maxAmmo = clipCount * clipSize;
        currentAmmo = maxAmmo;
    }

    void Update()
    {
        RaycastHit hit;
        Ray r = new Ray();
        Camera cam = GetComponentInChildren<Camera>();
        r.origin = cam.transform.position;
        r.direction = cam.transform.forward;

        if(Physics.Raycast(r.origin, r.direction, out hit, fireRange))
        {
            if(bShowDebugMsg)
                Debug.Log(gameObject.name + "Hit: " + hit.collider.name);
        }
    }

    public void Fire()
    {
        if(currentClipSize <= 0)
            return;
        
        if(weaponState != WeaponState.IDLE)
            return;
        Debug.Log("Started Firing! " + gameObject.name);
        StartCoroutine(Firing());
    }

    public void Reload()
    {
        if(currentAmmo <= 0)
            return;

        if(currentClipSize >= clipSize)
            return;

        if(weaponState == WeaponState.RELOADING)
            return; 
        Debug.Log("Started Reloading! " + gameObject.name);
        StartCoroutine(Reloading());
    }

    public void StopFire()
    {
        if(weaponState == WeaponState.RELOADING)
            return;
        Debug.Log("Stopped Firing! " + gameObject.name);
        weaponState = WeaponState.IDLE;
    }

    IEnumerator Firing()
    {
        Debug.Log("Firing! " + gameObject.name);
        weaponState = WeaponState.FIRING;
        currentClipSize--;
        currentAmmo--;
        float t = 0.0f;

        

        do
        {
            t += 1.0f * Time.deltaTime;
            yield return 0;
        }while(t < fireRate);

        weaponState = WeaponState.IDLE;
        yield return 0;
    }

    IEnumerator Reloading()
    {
        weaponState = WeaponState.RELOADING;
        float t = 0.0f;
        Debug.Log("Reloading! " + gameObject.name);
        do
        {
            t += 1.0f * Time.deltaTime;
            yield return 0;
        }while(t < realoadSpeed);

        if(currentAmmo < clipSize)
            currentClipSize = currentAmmo;
        else
            currentClipSize = clipSize;

        weaponState = WeaponState.IDLE;
        yield return 0;
    }
}

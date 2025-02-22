using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SniperShoot : MonoBehaviour
{
    [Header("Prefab Refrences")]
    public GameObject bulletPrefab;
    public GameObject casingPrefab;
    public GameObject muzzleFlashPrefab;

    [Header("Location Refrences")]
    [SerializeField] private Transform barrelLocation;
    [SerializeField] private Transform casingExitLocation;

    [Header("Settings")]
    [Tooltip("Specify time to destory the casing object")][SerializeField] private float destroyTimer = 2f;
    [Tooltip("Bullet Speed")][SerializeField] private float shotPower = 500f;
    [Tooltip("Casing Ejection Speed")][SerializeField] private float ejectPower = 150f;

    [Header("Bullet Management")]
    private MagazineManager magazine;
    private int currentBullets = 0;
    private bool canShoot = false;
    private bool hasShot = false;

    [Header("Audio")]
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip shootSFX;
    [SerializeField] private AudioClip noBulletsSFX;


    void Start()
    {
        if (barrelLocation == null)
            barrelLocation = transform;
    }

    public void OnShoot(BaseInteractionEventArgs args)
    {
        if (canShoot && currentBullets > 0)
        {
            canShoot = false;
            Shoot();
            hasShot = true;
            //Vibración:
            if (args.interactorObject is XRBaseControllerInteractor interactor)
            {
                interactor.SendHapticImpulse(1.0f, 0.3f);
            }
        }
        else
        {
            source.PlayOneShot(noBulletsSFX);
        }
    }


    //This function creates the bullet behavior
    void Shoot()
    {
        if (muzzleFlashPrefab)
        {
            //Create the muzzle flash
            GameObject tempFlash;
            tempFlash = Instantiate(muzzleFlashPrefab, barrelLocation.position, barrelLocation.rotation);

            //Destroy the muzzle flash effect
            Destroy(tempFlash, destroyTimer);
        }

        //cancels if there's no bullet prefeb
        if (!bulletPrefab)
        { return; }

        // Create a bullet and add force on it in direction of the barrel
        currentBullets--;
        if (magazine) magazine.DecreaseBullet();
        source.PlayOneShot(shootSFX);
        Instantiate(bulletPrefab, barrelLocation.position, barrelLocation.rotation).GetComponent<Rigidbody>().AddForce(barrelLocation.forward * shotPower);
    }

    //This function creates a casing at the ejection slot
    void CasingRelease()
    {
        //Cancels function if ejection slot hasn't been set or there's no casing
        if (!casingExitLocation || !casingPrefab)
        { return; }

        //Create the casing
        GameObject tempCasing;
        tempCasing = Instantiate(casingPrefab, casingExitLocation.position, casingExitLocation.rotation) as GameObject;
        //Add force on casing to push it out
        tempCasing.GetComponent<Rigidbody>().AddExplosionForce(Random.Range(ejectPower * 0.7f, ejectPower), (casingExitLocation.position - casingExitLocation.right * 0.3f - casingExitLocation.up * 0.6f), 1f);
        //Add torque to make casing spin in random direction
        tempCasing.GetComponent<Rigidbody>().AddTorque(new Vector3(0, Random.Range(100f, 500f), Random.Range(100f, 1000f)), ForceMode.Impulse);

        //Destroy casing after X seconds
        Destroy(tempCasing, destroyTimer);
    }

    public void SetMagazine(MagazineManager newMagazine)
    {
        magazine = newMagazine;
        currentBullets += magazine.GetBullets();
        canShoot = false;
    }

    public void RemoveMagazine()
    {
        if (currentBullets > 0)
        {
            currentBullets = 1;
            magazine.DecreaseBullet();
        }
        else currentBullets = 0;
        magazine = null;
    }

    public void EnableShoot()
    {
        canShoot = true;
        if (hasShot)
        {
            CasingRelease();
            hasShot = false;
        }
    }
}


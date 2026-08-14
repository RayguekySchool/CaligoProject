using UnityEngine;
using TMPro;
using System.Collections;

public class PistolCanvasGun : MonoBehaviour
{
    [Header("Pistol Config")]
    public Animator animator;
    public float range = 100f;
    public Camera fpsCam;
    public string bulletType = "Pistol";
    public float fireRate = 0.5f;

    [Header("Ammo")]
    public int maxAmmo = 12;
    public int reserveAmmo = 36;
    private int currentAmmo;

    [Header("UI")]
    public TextMeshProUGUI ammoText;

    [Header("Audio")]
    public AudioSource p_shootSound;

    [Header("Reload")]
    public float reloadTime = 1.5f;

    private static readonly int ShootTrigger = Animator.StringToHash("ShootPistol");
    private static readonly int ReloadBool = Animator.StringToHash("ReloadPistol");

    private float nextTimeToFire = 0f;
    private bool isReloading = false;
    private bool hasShotOnce = false;

    void Start()
    {
        currentAmmo = maxAmmo;

        if (ammoText != null)
            ammoText.gameObject.SetActive(false);

        if (p_shootSound == null)
            p_shootSound = GetComponent<AudioSource>();

        if (fpsCam == null)
            fpsCam = Camera.main;
    }

    void Update()
    {
        if (isReloading) return;

        if (FirstPersonController.instance != null && !FirstPersonController.instance.CanMove)
            return;

        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo && reserveAmmo > 0)
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetMouseButtonDown(0) && Time.time >= nextTimeToFire && currentAmmo > 0)
        {
            nextTimeToFire = Time.time + fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        currentAmmo--;

        if (animator != null)
        {
            animator.SetTrigger(ShootTrigger);
        }

        if (p_shootSound != null)
            p_shootSound.Play();

        if (!hasShotOnce)
        {
            hasShotOnce = true;
            if (ammoText != null)
                ammoText.gameObject.SetActive(true);
        }

        UpdateAmmoUI();

        if (fpsCam == null) return;

        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            EnemyHealth enemy = hit.transform.GetComponent<EnemyHealth>() ??
                                hit.transform.GetComponentInParent<EnemyHealth>() ??
                                hit.transform.GetComponentInChildren<EnemyHealth>();

            if (enemy != null)
            {
                enemy.TakeBullet(bulletType);
            }
        }
    }

    public void SetReloadTime(float newReloadTime)
    {
        reloadTime = newReloadTime;
    }

    IEnumerator Reload()
    {
        isReloading = true;

        if (animator != null)
        {
            animator.SetBool(ReloadBool, true);
        }

        yield return new WaitForSeconds(reloadTime);

        int neededAmmo = maxAmmo - currentAmmo;
        int ammoToReload = Mathf.Min(neededAmmo, reserveAmmo);

        currentAmmo += ammoToReload;
        reserveAmmo -= ammoToReload;

        UpdateAmmoUI();

        if (animator != null)
        {
            animator.SetBool(ReloadBool, false);
        }

        isReloading = false;
    }

    void UpdateAmmoUI()
    {
        if (ammoText != null)
            ammoText.text = $"{currentAmmo}";
    }
}
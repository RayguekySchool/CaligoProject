using UnityEngine;
using TMPro;
using System.Collections;

public class PistolCanvasGun : MonoBehaviour
{
    [Header("Pistol config")]
    public Animator animator;
    public float range = 100f;
    public Camera fpsCam;
    public string bulletType = "Pistol";
    public float fireRate = 0.5f;

    [Header("ammo")]
    public int maxAmmo = 12;
    public int reserveAmmo = 36;
    private int currentAmmo;

    [Header("UI")]
    public TextMeshProUGUI ammoText;

    [Header("audio")]
    public AudioSource p_shootSound;

    [Header("reload")]
    public float reloadTime = 1.5f;

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
        if (isReloading)
        {
            if (animator != null) animator.SetBool("ShootPistol", false);
            return;
        }

        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo && reserveAmmo > 0)
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetButtonDown("Fire1") && Time.time >= nextTimeToFire && currentAmmo > 0)
        {
            nextTimeToFire = Time.time + fireRate;
            Shoot();
        }
        else
        {
            if (animator != null)
            {
                animator.SetBool("ShootPistol", false);
                animator.SetBool("ReloadPistol", false);
            }
        }
    }

    void Shoot()
    {
        currentAmmo--;
        if (animator != null)
        {
            animator.SetBool("ShootPistol", true);
            animator.SetBool("ReloadPistol", false);
            animator.SetTrigger("ShootPistol");
        }

        if (p_shootSound != null)
            p_shootSound.Play();
        else
            Debug.Log("[PistolaCanvasGun] p_shootSound == null");

        if (!hasShotOnce)
        {
            hasShotOnce = true;
            if (ammoText != null)
                ammoText.gameObject.SetActive(true);
        }

        UpdateAmmoUI();

        if (fpsCam == null)
        {
            Debug.LogWarning("[PistolaCanvasGun] fpsCam é null — defina a câmera no inspector.");
            return;
        }

        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Debug.DrawRay(ray.origin, ray.direction * range, Color.red, 1f);

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            string hitName = hit.transform.name;
            Debug.Log($"[PistolaCanvasGun] Raycast acertou: {hitName} a {hit.distance:F2}m (collider: {hit.collider.name})");

            EnemyHealth enemy = hit.transform.GetComponent<EnemyHealth>();
            string foundOn = "self";
            if (enemy == null)
            {
                enemy = hit.transform.GetComponentInParent<EnemyHealth>();
                if (enemy != null) foundOn = "parent";
            }
            if (enemy == null)
            {
                enemy = hit.transform.GetComponentInChildren<EnemyHealth>();
                if (enemy != null) foundOn = "children";
            }
            if (enemy == null && hit.rigidbody != null)
            {
                enemy = hit.rigidbody.GetComponent<EnemyHealth>();
                if (enemy != null) foundOn = "rigidbody";
            }

            if (enemy != null)
            {
                Debug.Log($"[PistolaCanvasGun] Encontrado EnemyHealth ({foundOn}) em '{enemy.gameObject.name}'. Chamando TakeBullet({bulletType}).");
                enemy.TakeBullet(bulletType);
            }
            else
            {
                Debug.Log($"[PistolaCanvasGun] Não foi encontrado EnemyHealth no objeto atingido ({hitName}). Verifique se o componente está no mesmo GameObject ou em parents/children.");
            }
        }
        else
        {
            Debug.Log("[PistolaCanvasGun] Raycast não acertou nada.");
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
            animator.SetBool("ShootPistol", false);
            animator.SetBool("ReloadPistol", true);
            animator.ResetTrigger("ShootPistol");
            animator.SetTrigger("ReloadPistol");
        }

        yield return new WaitForSeconds(reloadTime);

        int neededAmmo = maxAmmo - currentAmmo;
        int ammoToReload = Mathf.Min(neededAmmo, reserveAmmo);

        currentAmmo += ammoToReload;
        reserveAmmo -= ammoToReload;

        UpdateAmmoUI();

        if (animator != null)
            animator.SetBool("ReloadPistol", false);

        isReloading = false;
    }

    void UpdateAmmoUI()
    {
        if (ammoText != null)
            ammoText.text = $"{currentAmmo}";
    }
}

using UnityEngine;
using Unity.Netcode;
using System.Collections;

public enum deviationTypes { None, Random, Gradual };
public enum projectileTypes { Bullet, Laser };

[CreateAssetMenu(fileName = "BulletPattern", menuName = "ScriptableObjects/BulletPattern", order = 1)]
public class BulletPattern : ScriptableObject
{
    public float canShootAfter;
    public int amountOfShotWaves = 1;
    public float delayBetweenShots = 0f;
    public KeyCode triggerButton;
    [SerializeField] private projectileTypes projectileType;
    [SerializeField] private int amountOfBullets = 1;
    [SerializeField] private deviationTypes deviationType;
    [SerializeField] private float shootingAngle = 0f;
    [SerializeField] private float cooldown = 3f;
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private GameObject projectile;
    [SerializeField] private float bulletMaxTravelDistance = 40f;
    public bool shouldLockRotation = false;
    float baseBulletAngleDeviation;
    float deltaAngle;
    int waveIdx = 0;
    LineRenderer lineRenderer;

    public void onAddSpell(LineRenderer _lineRenderer)
    {
        deltaAngle = shootingAngle / (amountOfBullets - 1);
        baseBulletAngleDeviation = -shootingAngle / 2;
        lineRenderer = _lineRenderer;
    }

    public IEnumerator onShoot(GameObject firePoint, Transform player, System.Action<bool> changeRotationAbility)
    {
        if (shouldLockRotation) changeRotationAbility(false);
        // TODO remove this logic if laser is not needed anymore
        switch (projectileType)
        {
            case projectileTypes.Laser:
                if (!lineRenderer)
                {
                    Debug.Log("Can't find line renderer");
                    break;
                }
                canShootAfter = Time.time + cooldown;
                lineRenderer.enabled = true;
                onLaserShot(firePoint, player);
                yield return new WaitForSeconds(delayBetweenShots);
                onLaserShot(firePoint, player, true);
                yield return new WaitForSeconds(.15f);
                lineRenderer.enabled = false;
                break;
            case projectileTypes.Bullet:
            default:
                waveIdx = 0;
                while (waveIdx < amountOfShotWaves)
                {
                    canShootAfter = Time.time + cooldown;

                    for (int bulletIndex = 0; bulletIndex < amountOfBullets; bulletIndex++)
                    {
                        float rotationDegree = player.rotation.eulerAngles.z;

                        switch (deviationType)
                        {
                            case deviationTypes.Random:
                                rotationDegree += Random.Range(-baseBulletAngleDeviation, baseBulletAngleDeviation);
                                break;
                            case deviationTypes.Gradual:
                                rotationDegree += baseBulletAngleDeviation + bulletIndex * deltaAngle;
                                break;
                            case deviationTypes.None:
                            default:
                                break;
                        }

                        // This is required since player's model is facing another direction
                        if (player.localScale.x < 0) rotationDegree += 180;

                        Vector3 bulletRotation = new Vector3(0f, 0f, rotationDegree);

                        GameObject bullet = Instantiate(
                            projectile,
                            firePoint.transform.position,
                            Quaternion.Euler(bulletRotation)
                        );
                        bullet.GetComponent<NetworkObject>().Spawn(true);

                        BulletBehaviour bulletBehaviour = bullet.GetComponent<BulletBehaviour>();
                        bulletBehaviour.parent = firePoint.transform.parent.gameObject;

                        Rigidbody2D bulletRB = bullet.GetComponent<Rigidbody2D>();

                        Vector2 shootingVector = new Vector2(
                            Mathf.Cos(Mathf.Deg2Rad * rotationDegree),
                            Mathf.Sin(Mathf.Deg2Rad * rotationDegree)
                        );

                        bulletRB.velocity = shootingVector * projectileSpeed;
                        bulletBehaviour.projectileSpeed = projectileSpeed;
                        bulletBehaviour.maxTravelDistance = bulletMaxTravelDistance;
                    }
                    waveIdx++;
                    yield return new WaitForSeconds(delayBetweenShots);
                }
                break;
        }
        if (shouldLockRotation) changeRotationAbility(true);
    }

    void onLaserShot(GameObject firePoint, Transform player, bool shouldDealDamage = false)
    {
        if (shouldDealDamage)
        {
            lineRenderer.startWidth = 0.3f;
            lineRenderer.endWidth = 0.3f;
            lineRenderer.startColor = Color.white;
            lineRenderer.endColor = Color.white;
        }
        else
        {
            lineRenderer.startWidth = 0.2f;
            lineRenderer.endWidth = 0.2f;
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
        }
        lineRenderer.SetPosition(0, firePoint.transform.localPosition);

        Vector3 laserDirection = Vector3.right;

        lineRenderer.SetPosition(1, laserDirection * bulletMaxTravelDistance);

        if (shouldDealDamage)
        {
            RaycastHit2D[] targets = Physics2D.RaycastAll(firePoint.transform.localPosition, firePoint.transform.right);
            // TODO deal damage to targets
        }
    }
}
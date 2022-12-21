using UnityEngine;
using Unity.Netcode;
using System.Collections;

public enum deviationTypes { None, Random, Gradual };

[CreateAssetMenu(fileName = "BulletPattern", menuName = "ScriptableObjects/BulletPattern", order = 1)]
public class BulletPattern : ScriptableObject
{
    public float canShootAfter;
    public int amountOfShotWaves = 1;
    public float delayBetweenShots = 0f;
    public KeyCode triggerButton;
    [SerializeField] private int amountOfBullets = 1;
    [SerializeField] private deviationTypes deviationType;
    [SerializeField] private float shootingAngle = 0f;
    [SerializeField] private float cooldown = 3f;
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private GameObject projectile;
    [SerializeField] private float bulletMaxTravelDistance = 40f;
    float baseBulletAngleDeviation;
    float deltaAngle;
    int waveIdx = 0;

    public void Start()
    {
        canShootAfter = 0f;
        deltaAngle = shootingAngle / (amountOfBullets - 1);
        baseBulletAngleDeviation = -shootingAngle / 2;
        waveIdx = 0;
    }

    public IEnumerator onShoot(GameObject firePoint, Transform player)
    {
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
    }

    private IEnumerator WaitAndPrint(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
    }
}
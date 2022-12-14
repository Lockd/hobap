using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[CreateAssetMenu(fileName = "BulletPattern", menuName = "ScriptableObjects/BulletPattern", order = 1)]
public class BulletPattern : ScriptableObject
{
    public float canShootAfter;
    public KeyCode triggerButton;
    [SerializeField] private int amountOfBullets = 1;
    [SerializeField] private float delayBetweenShots = 0f;
    [SerializeField] private float shootingAngle = 0f;
    [SerializeField] private float cooldown = 3f;
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private GameObject projectile;
    [SerializeField] private bool shouldDeviate = false;
    float baseBulletAngleDeviation;
    float deltaAngle;

    public void Start()
    {
        canShootAfter = 0f;
        deltaAngle = shootingAngle / (amountOfBullets - 1);
        baseBulletAngleDeviation = -shootingAngle / 2;
    }

    public void onShoot(GameObject firePoint, Transform player)
    {
        canShootAfter = Time.time + cooldown;

        for (int bulletIndex = 0; bulletIndex < amountOfBullets; bulletIndex++)
        {
            float rotationDegree = player.rotation.eulerAngles.z;
            if (shouldDeviate)
            {
                rotationDegree += baseBulletAngleDeviation + bulletIndex * deltaAngle;
            }

            Vector3 bulletRotation = new Vector3(0f, 0f, rotationDegree);

            GameObject bullet = Instantiate(
                projectile,
                firePoint.transform.position,
                Quaternion.Euler(bulletRotation)
            );
            bullet.GetComponent<NetworkObject>().Spawn(true);

            Rigidbody2D bulletRB = bullet.GetComponent<Rigidbody2D>();

            Vector2 shootingVector = new Vector2(
                Mathf.Cos(Mathf.Deg2Rad * rotationDegree),
                Mathf.Sin(Mathf.Deg2Rad * rotationDegree)
            );

            bulletRB.velocity = shootingVector * projectileSpeed;
        }
    }
}
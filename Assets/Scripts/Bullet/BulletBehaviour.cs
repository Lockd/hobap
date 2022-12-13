using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehaviour : MonoBehaviour
{
    [SerializeField] private float maxTravelDistance = 20f;
    Vector3 initialPosition;
    void Start()
    {
        initialPosition = transform.position;
    }

    void Update()
    {
        if (Vector3.Distance(initialPosition, transform.position) > maxTravelDistance)
        {
            Destroy(gameObject);
        }
    }
}

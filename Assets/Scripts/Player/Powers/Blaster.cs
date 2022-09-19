using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blaster : MonoBehaviour
{
    [SerializeField]
    private Transform gunTip, view;
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private float gunForce = 1000, cooldown = 5;
    private bool canShoot = true;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (canShoot)
            {
                Shoot();
            }
        }
    }

    void Shoot()
    {
        canShoot = false;
        rb.AddForce(-view.forward * gunForce);
        Invoke(nameof(ResetShoot), cooldown);
    }

    void ResetShoot()
    {
        canShoot = true;
    }
}

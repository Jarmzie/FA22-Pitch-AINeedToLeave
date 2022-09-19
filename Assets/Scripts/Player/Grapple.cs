using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : MonoBehaviour
{
    [SerializeField]
    private LineRenderer lr;
    [SerializeField]
    private LayerMask grappleStuff;
    private Vector3 grapplePoint;
    [SerializeField]
    private Transform grappleTip, view, player;
    private float maxDistance = 50f;
    private SpringJoint joint;
    [SerializeField]
    private GameObject hook;

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            DoGrapple();
        }
        if (Input.GetMouseButtonUp(1))
        {
            Detatch();
        }
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    private void DoGrapple()
    {
        Detatch();
        RaycastHit hit;
        if (Physics.Raycast(view.position, view.forward, out hit, maxDistance))
        {
            hook.SetActive(false);

            grapplePoint = hit.point;
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grapplePoint;

            float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);

            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;

            joint.spring = 100f; //4.5
            joint.damper = 10f; //7
            joint.massScale = 4.5f; //4.5

            lr.positionCount = 2;
        }
    }

    private void DrawRope()
    {
        if (!joint)
        {
            return;
        }
        lr.SetPosition(0, grappleTip.position);
        lr.SetPosition(1, grapplePoint);
    }

    private void Detatch()
    {
        hook.SetActive(true);
        lr.positionCount = 0;
        if (joint)
        {
            Destroy(joint);
        }
    }
}

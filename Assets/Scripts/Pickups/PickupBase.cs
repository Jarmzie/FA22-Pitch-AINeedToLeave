using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupBase : MonoBehaviour
{
    public float ReturnTime = 0.0f;
    public bool shouldSpin = false;

    private void Update()
    {
        if (shouldSpin)
        {
            transform.Rotate(0, 0, 1, Space.Self);
        }
    }

    void OnTriggerEnter(Collider other) {
        if(other.name=="PlayerBody")
        {
            PickupBehavior();
            if(ReturnTime==0.0f){
               Destroy(this.gameObject);
            }
            else{
                StartCoroutine(ReturninX());
                this.gameObject.GetComponent<Renderer>().enabled = false;
                this.gameObject.GetComponent<CapsuleCollider>().enabled = false;

            }
        }
    }

    IEnumerator ReturninX(){
        yield return new WaitForSeconds(ReturnTime);
        this.gameObject.GetComponent<Renderer>().enabled = true;
        this.gameObject.GetComponent<CapsuleCollider>().enabled = true;
    }


    public virtual void PickupBehavior() {
        Debug.Log("Unused pickup behavior");
        return;
    }

   
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup_Jump : PickupBase
{

    PlayerMovement player;

    public void Start(){
        player = GameObject.Find("PlayerBody").GetComponent<PlayerMovement>();
    }

    public override void PickupBehavior(){
        Debug.Log("Jumps modified");
        player.SetJumpsLeft(player.getJumpsLeft() + 1);
        player.SetMaxJumps(player.getJumpsLeft());
        StartCoroutine(ReturnJumps());
    }

    IEnumerator ReturnJumps(){
        yield return new WaitUntil(() => player.getJumpsLeft() == 0);
        player.SetMaxJumps(2);
    }
}

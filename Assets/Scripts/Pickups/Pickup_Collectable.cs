using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup_Collectable : PickupBase
{
    public override void PickupBehavior(){
      GameManager.Instance.CollectGotten += 1;
      GameManager.Instance.UIM.ColText.text = "" + GameManager.Instance.CollectGotten;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text ColText;
    private void Awake() {
        ColText = GameObject.Find("CollectableText").GetComponent<Text>();
    }


}

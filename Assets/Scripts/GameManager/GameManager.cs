using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager: MonoBehaviour
{
    public static GameManager Instance;
    public int CollectGotten;
    public UIManager UIM;
    private void Awake(){
        if(Instance){
            Debug.Log("GameManager already exists.");
        }
        Instance = this;

        UIM = GameObject.Find("Canvas").GetComponent<UIManager>();
        Debug.Log(UIM.ToString());
    }

  

}

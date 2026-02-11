using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGamePhotonManager : MonoBehaviourPunCallbacks
{
    public GameObject playerPref;
    public float timerforspawn;
    // Start is called before the first frame update
    void Start()
    {

        StartCoroutine(spawnpref());
        
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var item in PhotonNetwork.CurrentRoom.CustomProperties)
        {
            print(item.Key.ToString() + " value : " + item.Value.ToString());
           
        }
    }
   
    
    
        IEnumerator spawnpref()
        {
            while (!PhotonNetwork.InRoom)
                yield return null;

            while (SceneManager.GetActiveScene().name == "Menu")
                yield return null;

            PhotonNetwork.Instantiate(playerPref.name, new Vector3(0, 5, 0), Quaternion.identity);
        }
    
}

using UnityEngine;
using Photon.Pun;
using System.Linq;
using Unity.VisualScripting;
using System.Collections.Generic;

public class elevatorScript : MonoBehaviourPunCallbacks,IPunObservable
{
    public Animator animator;
    public bool containPlayerCollider;
    public bool canGo;
    public bool isGoing;
    public bool isSoloInElevator;
    public LayerMask layerPlayer;
    public List<Collider> collsplayers = new List<Collider>();
    public Vector3 vectorNewPosElevator;
    public float newposY;
    public int number;
    public GameObject prefTest;
    public BoxCollider CollPlayerInElevator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(photonView.IsMine == true)
        {
            if(isGoing)
            {
                transform.parent.position = Vector3.MoveTowards(transform.parent.position, new Vector3(vectorNewPosElevator.x, vectorNewPosElevator.y + newposY, vectorNewPosElevator.z), 20f * Time.deltaTime);
            }
            if (Input.GetKeyDown(KeyCode.Space))
            { 
                number = Random.Range(2, 8580);
            }
            if (Input.GetKeyDown(KeyCode.F))
            { 
                PhotonNetwork.Instantiate(prefTest.name,prefTest.transform.position,Quaternion.identity);
            }
        }

        if (isGoing)
        {
            return;
        }
        if (Physics.CheckBox(transform.TransformPoint(GetComponent<BoxCollider>().center), GetComponent<BoxCollider>().bounds.extents,transform.rotation,layerPlayer))
        {
            containPlayerCollider = true;
        }
        else
        {
            containPlayerCollider = false;
        }

        collsplayers = Physics.OverlapBox(transform.TransformPoint(GetComponent<BoxCollider>().center), GetComponent<BoxCollider>().bounds.extents, transform.rotation, layerPlayer).ToList();
        collsplayers.RemoveAll((coo) => coo == coo.gameObject.GetComponent<CharacterController>());

        if (collsplayers.Count == 1)
        {
            canGo = true;
        }
        else
        {
            canGo = false;
        }
        var playersoloinelevator = Physics.OverlapBox((CollPlayerInElevator.center), CollPlayerInElevator.bounds.extents, transform.rotation, layerPlayer).ToList();
        playersoloinelevator.RemoveAll((coo) => coo == coo.gameObject.GetComponent<CharacterController>());

        if (collsplayers.Count == 1)
        {
            isSoloInElevator = true;
        }
        else
        {
            isSoloInElevator = false;
        }
        
            if(canGo == true && collsplayers[0].GetComponent<PhotonView>().IsMine)
            {
                if(Input.GetKeyDown(KeyCode.E))
                {
                    photonView.RPC("goToNextLevel", RpcTarget.All);
                }
            }
        
       
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(number);
        }

        if(stream.IsReading)
        {
            number = (int)stream.ReceiveNext();
        }
    }

    [PunRPC] public void goToNextLevel()
    {
        print("dsqqdqqsd");
        collsplayers[0].transform.parent = transform.parent;
        if (photonView.IsMine)
        {
            print("toto");

            vectorNewPosElevator = transform.parent.position;
            isGoing = true;
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.tag == "Player")
        {
            animator.SetBool("Open",true);
        }
    }

    private void OnTriggerStay(Collider other)
    {
       
    }
    private void OnTriggerExit(Collider other)
    {
        
        if (other.tag == "Player" && !containPlayerCollider)
        {
            animator.SetBool("Open", false);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.TransformPoint( GetComponent<BoxCollider>().center), GetComponent<BoxCollider>().bounds.size);
    }
}

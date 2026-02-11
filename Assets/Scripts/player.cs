using JetBrains.Annotations;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class player : MonoBehaviourPunCallbacks
{
    public Vector3 move;
    public Vector3 playerVelocity;
    public Vector3 look;
    CharacterController characterController;
    public GameObject material;
    public float detectGroundRadius;
    public LayerMask ground;
    public bool isGrounded;
    public float fall, playerSpeed, jumpHeight;
    public float gravityValue;
    public Camera cam;
    public float lookx, looky;
    public List<GameObject> ascencors = new List<GameObject>();
    public Animator animatorperso;

    // Start is called before the first frame update
    void Start()
    {
        
        if (!photonView.IsMine) 
        {
           
            cam.enabled = false;
             Destroy(GetComponent<CharacterController>());
            return; 
        }
        List<GameObject> ezze = GameObject.FindGameObjectsWithTag("Ascenceur").ToList();
        ascencors = ezze;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if(photonView.IsMine)
        {
            characterController = GetComponent<CharacterController>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
        { 
           
            return;
        }
        
        lookx += Input.GetAxis("Mouse X");

        looky += Input.GetAxis("Mouse Y");
        looky = Mathf.Clamp(looky, -80, 80);
        cam.transform.rotation = Quaternion.Euler(-looky, lookx, 0);
        transform.rotation = Quaternion.Euler(0, lookx, 0);

        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }
        if( Mathf.Abs( Input.GetAxis("Horizontal"))>0 || Mathf.Abs( Input.GetAxis("Vertical"))>0)
        {
            animatorperso.SetBool("Run", true);
        }
        else
        {
            animatorperso.SetBool("Run", false);
        }   

        move = Quaternion.Euler(0, transform.eulerAngles.y, 0) * new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        characterController.Move(move * Time.deltaTime * playerSpeed);

        if (PhotonNetwork.IsMasterClient)
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                photonView.RPC("ActivateAscencor", RpcTarget.AllBufferedViaServer);
            }
        }

        // Makes the player jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -2.0f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        characterController.Move(playerVelocity * Time.deltaTime);


    }
   
    private void FixedUpdate()
    {
        if (!photonView.IsMine) { return; }
        isGrounded = Physics.CheckSphere(new Vector3(transform.position.x, transform.position.y - (GetComponent<Collider>().bounds.size.y / 2), transform.position.z), detectGroundRadius, ground);
    }

    [PunRPC]
    public void ActivateAscencor()
    {
        print("ogogogogogogogogogo");
        foreach (var item in ascencors)
        {
            item.GetComponent<Animator>().enabled = true;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, transform.position.y - (GetComponent<Collider>().bounds.size.y / 2), transform.position.z), detectGroundRadius);
        Gizmos.color = Color.yellow;
    }
}
 
   

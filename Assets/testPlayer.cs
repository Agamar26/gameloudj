using JetBrains.Annotations;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class testPlayer : MonoBehaviour
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
 
    // Start is called before the first frame update
    void Start()
    {
  
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        characterController = GetComponent<CharacterController>();
    }
  
    // Update is called once per frame
    void Update()
    {
   

        lookx += Input.GetAxis("Mouse X");

        looky += Input.GetAxis("Mouse Y");
        looky = Mathf.Clamp(looky, -80, 80);
        cam.transform.rotation = Quaternion.Euler(-looky, lookx, 0);

        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        move = Quaternion.Euler(0, cam.transform.localEulerAngles.y, 0) * new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        characterController.Move(move * Time.deltaTime * playerSpeed);



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

        isGrounded = Physics.CheckSphere(new Vector3(transform.position.x, transform.position.y - (GetComponent<Collider>().bounds.size.y / 2), transform.position.z), detectGroundRadius, ground);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, transform.position.y - (GetComponent<Collider>().bounds.size.y / 2), transform.position.z), detectGroundRadius);
        Gizmos.color = Color.yellow;
    }
}

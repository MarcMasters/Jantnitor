using Mono.Cecil;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class AntScript : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    private Rigidbody rb;
    //private Vector3 direction;

    [SerializeField] private GameObject pickUpArms;
    [SerializeField] private GameObject defaultArms;
    public bool pickingUp = false;
    public bool dropping = false;
    private bool onPickupRange = false;

    RaycastHit hitInfo;
    [SerializeField] LayerMask layerMask;
    private Vector3 rayPosition;
    [SerializeField] private Transform antHands; 
    [SerializeField] private Transform resourceContainer;
    private Transform resourceTrans;
    private Rigidbody resourceRb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        moveAnt();
    }

    void Update()
    {
        print(resourceTrans);
        managePickUp();
        manageRaycast();

        if (onPickupRange && Input.GetKeyDown(KeyCode.Space))
        {
            dropping = false;
            pickingUp = true;

            defaultArms.SetActive(false);
            pickUpArms.SetActive(true);
        }

        if (pickingUp && Input.GetKeyDown(KeyCode.E))
        {
            dropping = true;
            pickingUp = false;

            defaultArms.SetActive(true);
            pickUpArms.SetActive(false);
        }
        //print(onPickupRange);
    }

    //private void OnTriggerStay(Collider other)
    //{
    //    if (other.CompareTag("resource_leaf") || other.CompareTag("resource_water"))
    //    {
    //        //onPickupRange = true;

    //        if (pickingUp)
    //        {
    //            //print("hoja");
    //            defaultArms.SetActive(false);
    //            pickUpArms.SetActive(true);

    //            //pickUpLeaf.SetActive(true);
    //            //pickUpWater.SetActive(false);

    //            pickingUp = false;
    //        }
    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    onPickupRange = false;
    //}

    private void moveAnt()
    {
        Vector3 direction = Vector3.zero;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            direction.x = Input.GetAxis("Horizontal");
            direction.z = Input.GetAxis("Vertical");

            rb.linearDamping = 0f;

            // Movimiento en 8 direcciones (sin suavizado entre ellas)
            //if (Input.GetKey(KeyCode.W)) { direction.z = 1; }
            //if (Input.GetKey(KeyCode.S)) { direction.z = -1; }
            //if (Input.GetKey(KeyCode.A)) { direction.x = -1; }
            //if (Input.GetKey(KeyCode.D)) { direction.x = 1; }

            //rb.MovePosition(rb.position + direction.normalized * moveSpeed * Time.fixedDeltaTime);  // tp de personaje
            //rb.AddForce(direction.normalized * moveSpeed * Time.fixedDeltaTime, ForceMode.Impulse); // demasiada inercia
            rb.linearVelocity = direction.normalized * moveSpeed * Time.fixedDeltaTime;  // fisica no realista (movimiento instantaneo)

            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + 180;
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
        }
        else
        {
            //parar
            rb.linearDamping = 3f;
        }
        //print(direction);
    }

    private void managePickUp()
    {
        if (pickingUp)
        {
            //resource.transform.parent = antHands;
            resourceTrans.SetParent(antHands);
            resourceTrans.position = antHands.transform.position;
            resourceRb.isKinematic = true;
        }

        if (dropping)
        {
            resourceTrans.parent = resourceContainer;
            resourceRb.isKinematic = false;
        }
    }

    private void manageRaycast()
    {
        // Rayo desde el centro hacia delante
        rayPosition = new Vector3(transform.position.x, transform.position.y - 0.75f, transform.position.z);
        Ray ray = new Ray(rayPosition, transform.TransformDirection(Vector3.back));

        Debug.DrawRay(rayPosition, transform.TransformDirection(Vector3.back) * 1f, Color.red);

        if (Physics.Raycast(ray, out hitInfo, 1f, layerMask))
        {
            resourceTrans = hitInfo.transform;
            resourceRb = hitInfo.rigidbody;
            onPickupRange = true;
            print("Hit something");
        }
        else
        {
            onPickupRange = false;
            print("Hit nothing");
        }
    }
}
using UnityEngine;

public class AntScript : MonoBehaviour
{
    // Movement
    [SerializeField] private float moveSpeed;
    private Rigidbody rb;
    private Vector3 direction;
    [SerializeField] ParticleSystem moveParticles;

    // Pickup
    [SerializeField] private GameObject pickUpArms;
    [SerializeField] private GameObject defaultArms;
    public bool pickingUp = false;
    private bool onPickupRange = false;

    // Resources raycast
    RaycastHit resourceHitInfo;
    [SerializeField] LayerMask layerMask;
    private Vector3 rayPosition;
    [SerializeField] private Transform antHands;
    [SerializeField] private Transform resourceContainer; // objeto padre en drop
    private Transform resourceTrans;
    private Rigidbody resourceRb;
    private Collider resourceCol;

    // Throwing
    [SerializeField] private float throwForce;
    private bool hasThrown;

    // Enemies raycast
    [SerializeField] LayerMask enemyLayerMask;
    RaycastHit enemyHitInfo;
    private bool onHitRange;

    // Pulgon raycast
    [SerializeField] LayerMask pulgonLayerMask;
    public bool playerOnPulgon;

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
        //print(resourceTrans);
        //print(enemyTrans);
        
        rayPosition = new Vector3(transform.position.x, transform.position.y - 0.75f, transform.position.z);
        raycast();
        //detectEnemy();

        managePickup();

        if (Input.GetKeyDown(KeyCode.Q))
        {
            dropResource(true);
        }
    }

    private void moveAnt()
    {
        direction = Vector3.zero;
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

            if (!moveParticles.isPlaying)
            {
                moveParticles.Play();
            }
        }
        else
        {
            //parar
            rb.linearDamping = 3f;
            if (moveParticles.isPlaying)
            {
                moveParticles.Stop();
            }
        }
        //print(direction);
    }

    private void raycast()
    {
        // Rayo desde el centro hacia delante
        Ray ray = new Ray(rayPosition, transform.TransformDirection(Vector3.back));
        Debug.DrawRay(rayPosition, transform.TransformDirection(Vector3.back) * 1.5f, Color.green);
        if (Physics.Raycast(ray, out resourceHitInfo, 1.5f, layerMask))
        {
            onPickupRange = true;
            //print("Hit something");
        }
        else
        {
            onPickupRange = false;
            //print("Hit nothing");
        }


        // Detectar pulgon
        //Ray pulgonRay = new Ray(rayPosition, transform.TransformDirection(Vector3.back));
        if (Physics.Raycast(ray, 1.5f, pulgonLayerMask))
        {
            playerOnPulgon = true;
        }
        else
        {
            playerOnPulgon = false;
        }


        // Detectar enemigo
        if (Physics.Raycast(ray, out enemyHitInfo, 1.5f, enemyLayerMask))
        {
            //onHitRange = true;
            //print("Hit something");

            // animacion atacar
            // codigo atacar
        }
        else
        {
            onHitRange = false;
            //print("Hit nothing");
        }
    }

    //private void detectEnemy()
    //{
    //    // Rayo desde el centro hacia delante
    //    Ray ray = new Ray(rayPosition, transform.TransformDirection(Vector3.back));

    //    Debug.DrawRay(rayPosition, transform.TransformDirection(Vector3.back) * 1.5f, Color.red);

    //    if (Physics.Raycast(ray, out enemyHitInfo, 1.5f, enemyLayerMask))
    //    {
    //        //onHitRange = true;
    //        //print("Hit something");

    //        // animacion atacar
    //        // codigo atacar
    //    }
    //    else
    //    {
    //        onHitRange = false;
    //        //print("Hit nothing");
    //    }
    //}

    private void managePickup()
    {
        if (onPickupRange && Input.GetKeyDown(KeyCode.Space))
        {
            hasThrown = false;
            if (pickingUp) // si ya tiene un objeto
            {
                // soltar objeto 1
                resourceTrans.parent = resourceContainer;
                resourceRb.isKinematic = false;
                resourceCol.enabled = true;

                // establecer nuevo objeto
                resourceTrans = resourceHitInfo.transform;
                resourceRb = resourceHitInfo.rigidbody;
                resourceCol = resourceHitInfo.collider;

                // coger nuevo objeto
                resourceTrans.SetParent(antHands);
                resourceTrans.position = antHands.transform.position;
                resourceRb.isKinematic = true;
                resourceCol.enabled = false;
            }
            else
            {
                resourceTrans = resourceHitInfo.transform;
                resourceRb = resourceHitInfo.rigidbody;
                resourceCol = resourceHitInfo.collider;

                pickingUp = true;

                resourceTrans.SetParent(antHands);
                resourceTrans.position = antHands.transform.position;
                resourceRb.isKinematic = true;
                resourceCol.enabled = false;

                // animacion idle > pickup
                defaultArms.SetActive(false);
                pickUpArms.SetActive(true);
            }
        }

        if (pickingUp && Input.GetKeyDown(KeyCode.E))
        {
            dropResource();
        }
    }

    private void dropResource(bool throwing = false)
    {
        pickingUp = false;

        resourceTrans.parent = resourceContainer;
        resourceRb.isKinematic = false;
        resourceCol.enabled = true;

        // animacion pickup > idle 
        defaultArms.SetActive(true);
        pickUpArms.SetActive(false);

        if (throwing && !hasThrown)
        {
            resourceRb.AddForce(transform.TransformDirection(Vector3.back) * throwForce, ForceMode.Impulse);
            //resourceRb.AddForce(direction * throwForce, ForceMode.Impulse); // fuerza depende de velocidad
            hasThrown = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            print("DAMAGE");
        }
    }
}
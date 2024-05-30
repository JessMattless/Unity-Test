using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public GameObject[] inventory;

    public float moveSpeed = 5.0f;
    public float lookSpeed = 2.0f;
    public float jumpHeight = 4f;
    public float interactDistance = 1.0f;

    public GameObject objectHolder;
    public GameObject heldObject;

    private int inventoryIndex = 0;

    private new Camera camera;
    private new Rigidbody rigidbody;
    private new Collider collider;    

    private float yRot = 0.0f;
    private float distanceToGround;

    [SerializeReference]
    private Vector3 heldObjectOffset;
    [SerializeReference]
    private GameObject currentViewedObject;

    void Start()
    {
        // Initialize the inventory so we don't get an error trying to add something to a slot that doesn't exist
        inventory = new GameObject[2];

        camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();

        distanceToGround = collider.bounds.extents.y;
    }

    private void Update()
    {
        if (Input.GetButton("Jump") && IsGrounded())
        {
            /* This isn't perfect, it doesn't get *exactly* the same amount of height for the same amount of time,
             * But this is close enough that it works consistently.
             */
            rigidbody.velocity = new Vector3(0, jumpHeight, 0);
        }
        if (Input.GetButtonDown("Cancel")) Application.Quit();
        if (Input.GetButtonDown("Interact"))
        {
            if (currentViewedObject != null) {
                if (currentViewedObject.CompareTag("Key")) {
                    // The new object must be picked up first, otherwise the pedestal bugs out
                    GameObject oldObject = heldObject;
                    bool placed = (currentViewedObject.GetComponent<Key>().IsPlaced());
                    GameObject pedestal = currentViewedObject.GetComponent<Key>().currentPedestal;

                    // Pick up the new object
                    heldObject = currentViewedObject;
                    heldObject.GetComponent<GrabbableObject>().OnPickup(objectHolder);
                    inventory[inventoryIndex] = heldObject;

                    // If the player is already holding an object
                    if (oldObject != null)
                    {
                        // If the object the player is trying to pick up is placed,
                        // place the current held object on the pedestal and pick up the new object
                        if (placed && oldObject.GetComponent<Key>() != null) {
                            oldObject.GetComponent<Key>().OnPlace(pedestal);
                        }
                        // Else drop the current held object
                        else oldObject.GetComponent<GrabbableObject>().OnDrop();
                    }
                }
                else if (currentViewedObject.CompareTag("Grabbable"))
                {
                    // All of this is the sameas for key
                    // I'm not sure why you can't just add multiple tags to an object
                    if (heldObject != null)
                    {
                        heldObject.GetComponent<GrabbableObject>().OnDrop();
                        inventory[inventoryIndex] = null;
                    }
                    heldObject = currentViewedObject;
                    heldObject.GetComponent<GrabbableObject>().OnPickup(objectHolder);
                    inventory[inventoryIndex] = heldObject;
                }
                else if (currentViewedObject.CompareTag("Pedestal"))
                {
                    // If the player is holding an object, only allow key objects to interact with pedestals
                    if (heldObject != null && heldObject.GetComponent<Key>() != null)
                    {
                        // If the pedestal the player is looking at is not holding an object, place the currently
                        // held object on the pedestal
                        if (!currentViewedObject.GetComponent<Pedestal>().IsHoldingObject())
                        {
                            heldObject.GetComponent<Key>().OnPlace(currentViewedObject);
                            heldObject = null;
                            inventory[inventoryIndex] = null;
                        }
                        // Else if the pedestal is holding an object
                        else
                        {
                            GameObject oldObject = heldObject;

                            //Place the currently held object on the pedestal
                            heldObject = currentViewedObject.GetComponent<Pedestal>().heldObject;
                            heldObject.GetComponent<GrabbableObject>().OnPickup(objectHolder);
                            inventory[inventoryIndex] = heldObject;

                            //Pick up the object that was previously on the pedestal
                            oldObject.GetComponent<Key>().OnPlace(currentViewedObject);
                        }
                    }
                    // Else if the player is not holding an object, but the pedestal is, give the player the item.
                    else if (heldObject == null && currentViewedObject.GetComponent<Pedestal>().IsHoldingObject())
                    {
                        heldObject = currentViewedObject.GetComponent<Pedestal>().heldObject;
                        heldObject.GetComponent<GrabbableObject>().OnPickup(objectHolder);
                        inventory[inventoryIndex] = heldObject;

                    }
                }
            }
        }
        if (Input.GetButtonDown("Drop"))
        {
            // If the player is holding an item, drop the item
            if (heldObject != null)
            {
                heldObject.GetComponent<GrabbableObject>().OnDrop();
                heldObject = null;
                inventory[inventoryIndex] = null;
            }
        }
        if (Input.GetButtonDown("Inventory Slot 1"))
        {
            inventoryIndex = 0;
            heldObject = inventory[inventoryIndex];
        }
        if (Input.GetButtonDown("Inventory Slot 2"))
        {
            inventoryIndex = 1;
            heldObject = inventory[inventoryIndex];
        }
    }

    void FixedUpdate()
    {
        //Get position from player input
        float xMovement = Input.GetAxis("Horizontal");
        float zMovement = Input.GetAxis("Vertical");

        //Get y rotation from the camera
        yRot = camera.transform.rotation.eulerAngles.y;
        transform.eulerAngles = new Vector3(0.0f, yRot, 0.0f);

        // Get the next player position from the player input and direction
        Vector3 newPos = transform.position + moveSpeed * Time.deltaTime * 
            ((transform.forward * zMovement) + (transform.right * xMovement));


        // move the player according to inputs and rotation
        //Note: This works fine, except *sometimes* there is very slight movement when standing still on a ramp.
        rigidbody.MovePosition(newPos);
        rigidbody.position.Normalize();

        // Move the object that holds objects the player is holding
        objectHolder.transform.position = transform.position 
            + (transform.forward * heldObjectOffset.x) 
            + (transform.right * heldObjectOffset.z)
            + new Vector3(0, heldObjectOffset.y, 0);

        // Rotate the object holder, to make sure any items being held face the same way as the player
        objectHolder.transform.eulerAngles = new Vector3(0, yRot, 0);

        /* Use a raycast to detect if the player is looking at an object
         * Check whether the ray hit was within the interact distance of the player
         * if it is, set it as the actively viewed object
         */
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject reticle = GameObject.Find("Reticle");
            if (hit.distance <= interactDistance)
            {
                currentViewedObject = hit.transform.gameObject;
                if (hit.transform.gameObject.layer == 8 || hit.transform.gameObject.layer == 9)
                    reticle.GetComponent<Image>().color = Color.red;
                else reticle.GetComponent<Image>().color = Color.white;
            }
            else
            {
                currentViewedObject = null;
                reticle.GetComponent<Image>().color = Color.white;
            }
        }

        // Check each object in the players inventory
        for (int i = 0; i < inventory.Length; i++)
        {
            // Change the active inventory slot colour to a darker colour
            GameObject inventoryBackground = GameObject.Find("InventorySlot" + i);
            if (i == inventoryIndex)
                inventoryBackground.GetComponent<Image>().color = new Color(0.08490568f, 0.08490568f, 0.08490568f);
            else
                inventoryBackground.GetComponent<Image>().color = new Color(0.1698113f, 0.1698113f, 0.1698113f);

            GameObject inventoryText = GameObject.Find("InventorySlotText" + i);
            Debug.Log("InventorySlotText" + i);
            if (inventory[i] != null)
            {
                // If that inventory slot is holding an object, change the slot colour to orange
                inventoryText.GetComponent<TMP_Text>().color = new Color(0.8018868f, 0.4433302f, 0.1702118f);

                // Show the object the player is holding and hide the rest
                if (i == inventoryIndex)
                    inventory[i].SetActive(true);
                else inventory[i].SetActive(false);
            }
            else
            {
                inventoryText.GetComponent<TMP_Text>().color = Color.white;
            }
        }
    }

    // A simple raycast to check if the player is touching a surface below them
    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, distanceToGround + 0.1f);
    }
}

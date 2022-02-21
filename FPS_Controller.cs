/*
 * First Person Controller using CharacterController
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPS_Controller : MonoBehaviour
{
    [Header("Move")]
    private CharacterController cc;
    private float inputX, inputZ, inputSpeed;
    [SerializeField] private float moveSpeed, sideSpeed, gravitySpeed, jumpSpeed, pushSpeed;
    [SerializeField] private bool isGrounded, isJumping;

    private Vector3 move, velocity;
    public Transform foot;
    public LayerMask groundMask, footMask;

    [Header("Look")]
    private float mouseX, mouseY, rotateCam;
    [Range(0f, 1000f)]
    [SerializeField] private float mouseSensitivity = 300f;
    public Transform playerCamera;

    [Header("Audio")]
    private AudioSource footAudio;
    public float footTime, footCooldown;
    private Object_Base scrObject;
    [SerializeField] private List<AudioClip> clipConc = new List<AudioClip>();
    [SerializeField] private List<AudioClip> clipCarpet = new List<AudioClip>();
    [SerializeField] private List<AudioClip> clipMetal = new List<AudioClip>();
    [SerializeField] private List<AudioClip> clipWet = new List<AudioClip>();
    [SerializeField] private List<AudioClip> clipLeaves = new List<AudioClip>();
    [SerializeField] private List<AudioClip> clipLanding = new List<AudioClip>();

    void Start()
    {
        cc = GetComponent<CharacterController>();
        footAudio = foot.GetComponent<AudioSource>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        CheckGround();
        Move();
        Look();
    }

    private void CheckGround()
    {
        isGrounded = Physics.CheckSphere(foot.position, 0.15f, groundMask);

        if (isGrounded)
        {
            if (velocity.y < 0)
            {
                velocity.y = -2f;
            }

            if (isJumping)
            {
                FootLanding();
                isJumping = false;
            }

            if (Input.GetButtonDown("Jump"))
            {
                StartCoroutine(startJump());
            }

            footTime -= (Time.deltaTime * inputSpeed);
            if (inputSpeed > 0.2f && footTime < 0f)
            {
                footTime = footCooldown;
                Footsteps();
            }
        }
    }

    private void Move()
    {
        inputX = Input.GetAxis("Horizontal") * sideSpeed * Time.deltaTime;
        inputZ = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
        move = transform.right * inputX + transform.forward * inputZ;

        cc.Move(move);
        inputSpeed = cc.velocity.magnitude;

        velocity.y += gravitySpeed * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);
    }

    private void Look()
    {
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * 0.7f * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        rotateCam -= mouseY;
        rotateCam = Mathf.Clamp(rotateCam, -80f, 80f);
        playerCamera.localRotation = Quaternion.Euler(rotateCam, 0, 0);

        transform.Rotate(Vector3.up * mouseX);
    }

    IEnumerator startJump()
    {
        velocity.y = Mathf.Sqrt(jumpSpeed * -2f * gravitySpeed);
        yield return new WaitForSeconds(0.1f);
        isJumping = true;
    }

    private void Footsteps()
    {
        RaycastHit Hit;
        if (Physics.Raycast(foot.transform.position, -Vector3.up, out Hit, 1, footMask))
        {
            if (Hit.transform.GetComponent<Object_Base>() != null)
            {
                scrObject = Hit.transform.GetComponent<Object_Base>();
                footAudio.pitch = Random.Range(0.95f, 1.05f);

                switch (scrObject.mySound)
                {
                    case SoundType.Wet:
                        footAudio.PlayOneShot(clipWet[Random.Range(0, clipWet.Count)]);
                        break;
                    case SoundType.Carpet:
                        footAudio.PlayOneShot(clipCarpet[Random.Range(0, clipCarpet.Count)]);
                        break;
                    case SoundType.Metal:
                        footAudio.PlayOneShot(clipMetal[Random.Range(0, clipMetal.Count)]);
                        break;
                    case SoundType.Leaves:
                        footAudio.PlayOneShot(clipLeaves[Random.Range(0, clipLeaves.Count)]);
                        break;
                    case SoundType.Concrete:
                        footAudio.PlayOneShot(clipConc[Random.Range(0, clipConc.Count)]);
                        break;
                    default:
                        footAudio.PlayOneShot(clipConc[Random.Range(0, clipConc.Count)], 0.5f);
                        break;
                }
            }
        }
    }

    private void FootLanding()
    {
        RaycastHit Hit;
        if (Physics.Raycast(foot.transform.position, -Vector3.up, out Hit, 1, footMask))
        {
            if (Hit.transform.GetComponent<Object_Base>() != null)
            {
                scrObject = Hit.transform.GetComponent<Object_Base>();
                footAudio.pitch = Random.Range(0.95f, 1.05f);

                switch (scrObject.mySound)
                {
                    case SoundType.Concrete:
                        footAudio.PlayOneShot(clipLanding[0], 1f);
                        break;
                    case SoundType.Carpet:
                        footAudio.PlayOneShot(clipLanding[1], 1f);
                        break;
                    case SoundType.Metal:
                        footAudio.PlayOneShot(clipLanding[2], 1f);
                        break;
                    case SoundType.Wet:
                        footAudio.PlayOneShot(clipLanding[3], 1f);
                        break;
                    case SoundType.Leaves:
                        footAudio.PlayOneShot(clipLanding[4], 1f);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    //push rigidbodies
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody hitBody = hit.collider.attachedRigidbody;

        if (hitBody == null || isGrounded == false)
            return;

        if (hit.moveDirection.y < -0.3f)
            return;

        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
        hitBody.velocity = (pushDir * pushSpeed) / hitBody.mass;
    }
}

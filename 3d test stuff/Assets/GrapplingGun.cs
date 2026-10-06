using UnityEngine;

public class GrapplingGun : MonoBehaviour
{
    private LineRenderer lr;
    private Vector3 grapplePoint;
    public LayerMask whatIsGrappleable;
    public Transform gunTip, camera, player;
    private float maxDistance = 100f;
    private SpringJoint joint;

    [Header("Visual Mesh Settings")]
    [Tooltip("Assign your actual Gun Mesh child object here so recoil only affects visuals, not gunTip/physics")]
    [SerializeField] private Transform gunMesh;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip attachSound1;
    [SerializeField] private AudioClip attachSound2;

    [Header("Pitch Variation")]
    [SerializeField] private bool randomizePitch = true;
    [SerializeField] private float minPitch = 0.9f;
    [SerializeField] private float maxPitch = 1.1f;

    [Header("Recoil Settings")]
    [Tooltip("Distance the gun mesh kicks backward on fire")]
    [SerializeField] private float recoilDistance = 0.15f;
    [Tooltip("How fast the gun mesh returns to resting position")]
    [SerializeField] private float recoilReturnSpeed = 15f;

    [Header("Muzzle Flash Settings")]
    [SerializeField] private ParticleSystem muzzleFlashParticles;
    [SerializeField] private Light muzzleFlashLight;
    [SerializeField] private float lightFlashDuration = 0.05f;

    private Vector3 meshOriginalLocalPos;
    private float lightTimer = 0f;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // If gunMesh isn't assigned, default to transform so it doesn't crash
        if (gunMesh == null) gunMesh = transform;

        meshOriginalLocalPos = gunMesh.localPosition;

        if (muzzleFlashLight != null)
        {
            muzzleFlashLight.enabled = false;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartGrapple();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            StopGrapple();
        }

        // Smoothly return ONLY the visual mesh to resting position
        if (gunMesh != null)
        {
            gunMesh.localPosition = Vector3.Lerp(gunMesh.localPosition, meshOriginalLocalPos, Time.deltaTime * recoilReturnSpeed);
        }

        if (muzzleFlashLight != null && muzzleFlashLight.enabled)
        {
            lightTimer -= Time.deltaTime;
            if (lightTimer <= 0f)
            {
                muzzleFlashLight.enabled = false;
            }
        }
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    void StartGrapple()
    {
        RaycastHit hit;
        if (Physics.Raycast(camera.position, camera.forward, out hit, maxDistance, whatIsGrappleable))
        {
            grapplePoint = hit.point;
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grapplePoint;

            float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);

            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;

            joint.spring = 4.5f;
            joint.damper = 7f;
            joint.massScale = 4.5f;

            lr.positionCount = 2;
            currentGrapplePosition = gunTip.position;

            ApplyRecoil();
            TriggerMuzzleFlash();
            PlayAttachSound();
        }
    }

    void StopGrapple()
    {
        lr.positionCount = 0;
        Destroy(joint);
    }

    private Vector3 currentGrapplePosition;

    void DrawRope()
    {
        if (!joint) return;

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint, Time.deltaTime * 8f);

        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, currentGrapplePosition);
    }

    public bool IsGrappling()
    {
        return joint != null;
    }

    public Vector3 GetGrapplePoint()
    {
        return grapplePoint;
    }

    private void ApplyRecoil()
    {
        if (gunMesh != null)
        {
            gunMesh.localPosition -= Vector3.forward * recoilDistance;
        }
    }

    private void TriggerMuzzleFlash()
    {
        if (muzzleFlashParticles != null)
        {
            muzzleFlashParticles.Play();
        }

        if (muzzleFlashLight != null)
        {
            muzzleFlashLight.enabled = true;
            lightTimer = lightFlashDuration;
        }
    }

    private void PlayAttachSound()
    {
        if (audioSource == null || (attachSound1 == null && attachSound2 == null)) return;

        AudioClip clipToPlay = (Random.value > 0.5f) ? attachSound1 : attachSound2;

        if (clipToPlay == null)
        {
            clipToPlay = (attachSound1 != null) ? attachSound1 : attachSound2;
        }

        if (randomizePitch)
        {
            audioSource.pitch = Random.Range(minPitch, maxPitch);
        }

        audioSource.PlayOneShot(clipToPlay);
    }
}
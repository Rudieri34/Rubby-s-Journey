using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple boat controller using Unity's New Input System.
/// Requires Rigidbody and some drag settings for water resistance.
/// Combine with BuoyantObjectWaves.cs or BuoyantObject.cs for floating.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BoatController : MonoBehaviour
{
    [Header("Movement")]
    public float acceleration = 50f;    // forward thrust power
    public float steering = 30f;        // yaw torque
    public float maxSpeed = 10f;        // clamps top speed

    [Header("Water Resistance")]
    public float waterLinearDrag = 0.5f;
    public float waterAngularDrag = 0.3f;

    private Rigidbody rb;
    private BoatControls controls;
    private Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        controls = new BoatControls();
        controls.Boat.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Boat.Move.canceled += ctx => moveInput = Vector2.zero;
    }

    void OnEnable()
    {
        controls.Boat.Enable();
    }

    void OnDisable()
    {
        controls.Boat.Disable();
    }

    void FixedUpdate()
    {
        // Forward/backward throttle
        Vector3 forwardForce = transform.forward * (moveInput.y * acceleration);
        rb.AddForce(forwardForce, ForceMode.Force);

        // Steering (yaw rotation torque)
        float turn = moveInput.x * steering;
        rb.AddTorque(Vector3.up * turn, ForceMode.Force);

        // Limit max speed
        Vector3 flatVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        if (flatVel.magnitude > maxSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * maxSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }
}
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BuoyantObject))]
public class BoatController : MonoBehaviour
{
    public static BoatController Instance { get; private set; }

    private BuoyantObject _buoyantObject;

    [Header("Cargo Spawn Point")]
    [SerializeField] private Transform _cargoSpawnPoint;

    [Header("Movement")]
    [SerializeField] private float _acceleration = 30f;    // thrust strength
    [SerializeField] private float _reverseAcceleration = 15f; // weaker reverse power
    [SerializeField] private float _steering = 25f;        // turning torque
    [SerializeField] private float _maxSpeed = 12f;        // forward speed cap
    [SerializeField] private float _maxReverseSpeed = 5f;  // reverse speed cap
    [SerializeField] private float _throttleChangeRate = 2f; // how fast throttle changes
    [SerializeField] private float _brakeDrag = 0.8f;      // braking slowdown factor



    GameObject _currentCargo;

    private Rigidbody _rb;
    private BoatControls _controls;
    private Vector2 _moveInput;

    private float _currentThrottle = 0f; // -1 (full reverse) to 1 (full forward)

    void Awake()
    {
        Instance = this;

        _rb = GetComponent<Rigidbody>();
        _buoyantObject = GetComponent<BuoyantObject>();

        _controls = new BoatControls();
        _controls.Player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        _controls.Player.Move.canceled += ctx => _moveInput = Vector2.zero;
    }

    void OnEnable() => _controls.Player.Enable();
    void OnDisable() => _controls.Player.Disable();

    void FixedUpdate()
    {
        if(GameManager.Instance.IsGamePaused || !GameManager.Instance.IsMovementAllowed)
        {
            _currentThrottle = 0f; // reset throttle when game is paused or movement is not allowed
            return;
        }

        if (_buoyantObject.IsUnderWater)
        {
            HandleThrottle();
            ApplyForces();
        }
    }

    private void HandleThrottle()
    {
        // Smoothly change throttle toward input.y (-1 to 1)
        float targetThrottle = _moveInput.y;
        _currentThrottle = Mathf.MoveTowards(_currentThrottle, targetThrottle, Time.fixedDeltaTime * _throttleChangeRate);

        // Apply natural braking when no throttle input
        if (Mathf.Approximately(targetThrottle, 0f) && Mathf.Abs(_currentThrottle) < 0.05f)
        {
            _currentThrottle = 0f;
            _rb.linearVelocity *= (1f - Time.fixedDeltaTime * _brakeDrag); // smooth stop
        }
    }

    private void ApplyForces()
    {
        // Forward/reverse force
        float thrust = (_currentThrottle > 0f) ? _acceleration : _reverseAcceleration;
        Vector3 forwardForce = transform.forward * (_currentThrottle * thrust);
        _rb.AddForce(forwardForce, ForceMode.Force);

        // Steering (yaw torque only when moving)
        if (Mathf.Abs(_currentThrottle) > 0.05f)
        {
            float turn = _moveInput.x * _steering;
            _rb.AddTorque(Vector3.up * turn, ForceMode.Force);
        }

        // Limit max speed (different for forward/reverse)
        Vector3 flatVel = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
        float speedLimit = (_currentThrottle >= 0f) ? _maxSpeed : _maxReverseSpeed;

        if (flatVel.magnitude > speedLimit)
        {
            Vector3 limitedVel = flatVel.normalized * speedLimit;
            _rb.linearVelocity = new Vector3(limitedVel.x, _rb.linearVelocity.y, limitedVel.z);
        }
    }


    public void SetNewCargo(GameObject cargo)
    {
        GameObject newCargo = Instantiate(cargo, _cargoSpawnPoint.position, _cargoSpawnPoint.rotation);
        _currentCargo = newCargo;
    }
}

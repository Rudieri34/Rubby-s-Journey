using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BuoyantObject))]
[RequireComponent(typeof(AudioSource))]
public class BoatController : MonoBehaviour
{
    public static BoatController Instance { get; private set; }

    private BuoyantObject _buoyantObject;

    [Header("Cargo Spawn Point")]
    [SerializeField] private Transform _cargoSpawnPoint;
    [SerializeField] private float _cargoFailDistance = 5f; // how far cargo can drift before failing

    [Header("Movement")]
    [SerializeField] private float _acceleration = 30f;
    [SerializeField] private float _reverseAcceleration = 15f;
    [SerializeField] private float _steering = 25f;
    [SerializeField] private float _maxSpeed = 12f;
    [SerializeField] private float _maxReverseSpeed = 5f;
    [SerializeField] private float _throttleChangeRate = 2f;
    [SerializeField] private float _brakeDrag = 0.8f;

    [Header("Engine Sound")]
    [SerializeField] private AudioSource _engineAudioSource;
    [SerializeField] private float _minPitch = 0.8f;
    [SerializeField] private float _maxPitch = 2f;
    [SerializeField] private float _minVolume = 0.2f;
    [SerializeField] private float _maxVolume = .5f;
    [SerializeField] private float _engineLerpSpeed = 5f;

    private GameObject _currentCargo;

    private Rigidbody _rb;
    private BoatControls _controls;
    private Vector2 _moveInput;

    private float _currentThrottle = 0f;

    void Awake()
    {
        Instance = this;

        _rb = GetComponent<Rigidbody>();
        _buoyantObject = GetComponent<BuoyantObject>();

        if (_engineAudioSource == null)
            _engineAudioSource = GetComponent<AudioSource>();

        _engineAudioSource.loop = true;
        _engineAudioSource.playOnAwake = false;

        _controls = new BoatControls();
        _controls.Player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        _controls.Player.Move.canceled += ctx => _moveInput = Vector2.zero;
    }

    void OnEnable()
    {
        if(PlayerPrefs.GetInt("CurrentQuestIndex") != 0)
        {
            transform.position = QuestManager.Instance.Quests[PlayerPrefs.GetInt("CurrentQuestIndex") - 1].Destination;
        }


        _controls.Player.Enable();
        if (_engineAudioSource != null && !_engineAudioSource.isPlaying)
            _engineAudioSource.Play();
    }

    void OnDisable()
    {
        _controls.Player.Disable();
        if (_engineAudioSource != null && _engineAudioSource.isPlaying)
            _engineAudioSource.Stop();
    }

    void FixedUpdate()
    {
        if (GameManager.Instance.IsGamePaused || !GameManager.Instance.IsMovementAllowed)
        {
            _currentThrottle = 0f;
            UpdateEngineSound();
            return;
        }

        if (_buoyantObject.IsUnderWater)
        {
            HandleThrottle();
            ApplyForces();
        }

        UpdateEngineSound();
        CheckFailConditions();
    }

    private void HandleThrottle()
    {
        float targetThrottle = _moveInput.y;
        _currentThrottle = Mathf.MoveTowards(_currentThrottle, targetThrottle, Time.fixedDeltaTime * _throttleChangeRate);

        if (Mathf.Approximately(targetThrottle, 0f) && Mathf.Abs(_currentThrottle) < 0.05f)
        {
            _currentThrottle = 0f;
            _rb.linearVelocity *= (1f - Time.fixedDeltaTime * _brakeDrag);
        }
    }

    private void ApplyForces()
    {
        float thrust = (_currentThrottle > 0f) ? _acceleration : _reverseAcceleration;
        Vector3 forwardForce = transform.forward * (_currentThrottle * thrust);
        _rb.AddForce(forwardForce, ForceMode.Force);

        if (Mathf.Abs(_currentThrottle) > 0.05f)
        {
            float turn = _moveInput.x * _steering;
            _rb.AddTorque(Vector3.up * turn, ForceMode.Force);
        }

        Vector3 flatVel = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
        float speedLimit = (_currentThrottle >= 0f) ? _maxSpeed : _maxReverseSpeed;

        if (flatVel.magnitude > speedLimit)
        {
            Vector3 limitedVel = flatVel.normalized * speedLimit;
            _rb.linearVelocity = new Vector3(limitedVel.x, _rb.linearVelocity.y, limitedVel.z);
        }
    }

    private void UpdateEngineSound()
    {
        if (_engineAudioSource == null) return;

        // Use throttle to influence pitch and volume
        float targetPitch = Mathf.Lerp(_minPitch, _maxPitch, Mathf.Abs(_currentThrottle));
        float targetVolume = Mathf.Lerp(_minVolume, _maxVolume, Mathf.Abs(_currentThrottle));

        _engineAudioSource.pitch = Mathf.Lerp(_engineAudioSource.pitch, targetPitch, Time.fixedDeltaTime * _engineLerpSpeed);
        _engineAudioSource.volume = Mathf.Lerp(_engineAudioSource.volume, targetVolume, Time.fixedDeltaTime * _engineLerpSpeed);
    }

    public void SetNewCargo(GameObject cargo)
    {
        GameObject newCargo = Instantiate(cargo, _cargoSpawnPoint.position, _cargoSpawnPoint.rotation);
        _currentCargo = newCargo;
        _currentCargo.transform.DOScale(Vector3.one, .5f);
        SoundManager.Instance.PlaySFX("cargo_spawn", 1, 0.5f);
    }

    public void CleanCargo()
    {
        if (_currentCargo != null)
            _currentCargo.transform.DOScale(Vector3.zero, 1).OnComplete(() => Destroy(_currentCargo));
    }

    private void CheckFailConditions()
    {
        if (_currentCargo != null)
        {
            float dist = Vector3.Distance(_currentCargo.transform.position, _cargoSpawnPoint.position);
            if (dist > _cargoFailDistance)
            {
                SetMissionFailed();
            }
        }

        // Check if boat is flipped upside down
        float uprightDot = Vector3.Dot(transform.up, Vector3.up);
        if (uprightDot < 0.2f) // threshold, adjust as needed
        {
            SetMissionFailed();
        }
    }

    async void SetMissionFailed()
    {
        GameManager.Instance.SetMovementAllowed(false);

        DialogManager.Instance.SetTextColor(true);
        await DialogManager.Instance.SetDialog("Well, looks like I failed, let's try again.");
        if (_currentCargo) Destroy(_currentCargo);
        _currentCargo = null;

        await UniTask.WaitForSeconds(1f);
        GameManager.Instance.ReloadScene();
    }
}

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BuoyantObject : MonoBehaviour
{
    [SerializeField] private Transform[] _floatPoints;

    [SerializeField] private float _underWatrerDrag = 3f;
    [SerializeField] private float _underWaterAngularDrag = 3f;
    [SerializeField] private float _surfaceDrag = 1f;
    [SerializeField] private float _surfaceAngularDrag = 0.5f;
    [SerializeField] private float _floatingPower = 15f;
    [SerializeField] private float _waterLevel = 0f;

    Rigidbody _rigidbody;
    int _floatPointsUnderwater;

    public bool IsUnderWater;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        _floatPointsUnderwater = 0;
        foreach (Transform point in _floatPoints)
        {
            float difference = point.position.y - OceanManager.Instance.WaterHeightAtPosition(point.position);

            if (difference < 0)
            {
                _rigidbody.AddForceAtPosition(Vector3.up * _floatingPower * Mathf.Abs(difference), point.position, ForceMode.Force);
                _floatPointsUnderwater++;
                if (!IsUnderWater)
                {
                    IsUnderWater = true;
                    SwitchState(true);
                }
            }
        }

        if (IsUnderWater && _floatPointsUnderwater == 0)
        {
            IsUnderWater = false;
            SwitchState(false);
        }
    }

    void SwitchState(bool underwater)
    {
        if(IsUnderWater)
        {
            _rigidbody.linearDamping = _underWatrerDrag;
            _rigidbody.angularDamping = _underWaterAngularDrag;
        }
        else
        {
            _rigidbody.linearDamping = _surfaceDrag;
            _rigidbody.angularDamping = _surfaceAngularDrag;
        }
    }
}

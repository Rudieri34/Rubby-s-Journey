using UnityEngine;
using DG.Tweening; // ✅ Import DOTween

[System.Serializable] // ✅ makes it show in inspector
public class OceanConditions
{
    public float WaveHeight = 0.07f;
    public float WaveSpeed = 0.01f;
    public float WaveSpeed2 = 0.01f;
    public float WaveFrequency = 0.04f;
}

public class OceanManager : MonoBehaviour
{
    public static OceanManager Instance { get; private set; }

    public OceanConditions CurrentOceanCondition;
    public Transform Ocean;

    private Material _oceanMaterial;
    private Texture2D _wavesDisplacement;

    private void Start()
    {
        Instance = this;
        DOTween.Kill(this);

        CreateOcean();
        SetVariables();
        UpdateMaterial();
    }

    void CreateOcean()
    {
        if (Ocean == null)
        {
            Debug.LogWarning("Ocean transform not assigned!");
        }
    }

    void SetVariables()
    {
        _oceanMaterial = Ocean.GetComponent<Renderer>().sharedMaterial;
        _wavesDisplacement = _oceanMaterial.GetTexture("_WavesDisplacement") as Texture2D;
    }

    public float WaterHeightAtPosition(Vector3 position)
    {
        return Ocean.position.y + _wavesDisplacement.GetPixelBilinear(
            (position.x * CurrentOceanCondition.WaveFrequency + Time.time * CurrentOceanCondition.WaveSpeed2) * Ocean.localScale.x,
            (position.z * CurrentOceanCondition.WaveFrequency + Time.time * CurrentOceanCondition.WaveSpeed) * Ocean.localScale.z
        ).g * CurrentOceanCondition.WaveHeight;
    }

    private void OnValidate()
    {
        if (_oceanMaterial)
            SetVariables();

        UpdateMaterial();
    }

    void UpdateMaterial()
    {
        if (_oceanMaterial == null) return;

        _oceanMaterial.SetFloat("_WaveHeight", CurrentOceanCondition.WaveHeight);
        _oceanMaterial.SetFloat("_WaveSpeed", CurrentOceanCondition.WaveSpeed);
        _oceanMaterial.SetFloat("_WaveSpeed2", CurrentOceanCondition.WaveSpeed2);
        _oceanMaterial.SetFloat("_WaveFrequency", CurrentOceanCondition.WaveFrequency);
    }

    public void SetOceanCondition(OceanConditions newCondition, float transitionTime)
    {
        // Kill any previous tweens on this object to avoid conflicts
        DOTween.Kill(this);

        // Tween each property smoothly
        DOTween.To(() => CurrentOceanCondition.WaveHeight,
                   x => { CurrentOceanCondition.WaveHeight = x; UpdateMaterial(); },
                   newCondition.WaveHeight, transitionTime).SetTarget(this);

        CurrentOceanCondition.WaveSpeed = newCondition.WaveSpeed;
        CurrentOceanCondition.WaveSpeed2 = newCondition.WaveSpeed2;
        CurrentOceanCondition.WaveFrequency = newCondition.WaveFrequency;

    }
}

using UnityEngine;
using static UnityEditor.PlayerSettings;

public class OceanManager : MonoBehaviour
{
    public static OceanManager Instance { get; private set; }

    public float WaveHeight = 0.07f;
    public float WaveSpeed = 0.01f;
    public float WaveSpeed2 = 0.01f;
    public float WaveFrequency = 0.04f;

    public Transform Ocean;

    Material _oceanMaterial;
    Texture2D _wavesDisplacement;
    
    void Start()
    {
        Instance = this;

        CreateOcean();
        SetVariables();    
    }

    void CreateOcean()
    {
        if (Ocean == null)
        {
            
        }
    }

        void SetVariables() { 
        _oceanMaterial = Ocean.GetComponent<Renderer>().sharedMaterial;
        _wavesDisplacement = _oceanMaterial.GetTexture("_WavesDisplacement") as Texture2D;
    }

    public float WaterHeightAtPosition(Vector3 position)
    {
        return Ocean.position.y + _wavesDisplacement.GetPixelBilinear(
            (position.x * WaveFrequency + Time.time * WaveSpeed2) * Ocean.localScale.x,
            (position.z * WaveFrequency + Time.time * WaveSpeed) * Ocean.localScale.z
            ).g * WaveHeight;
        //return Ocean.position.y + _wavesDisplacement.GetPixelBilinear(
        // position.x * WaveFrequency + Time.time * WaveSpeed2,
        //    position.z * WaveFrequency + Time.time * WaveSpeed).g *
        //    WaveHeight * Ocean.localScale.x;
    }
    private void OnValidate()
    {
        if(!_oceanMaterial)
            SetVariables();

        UpdateMaterial();
    }

    void UpdateMaterial()
    {
        _oceanMaterial.SetFloat("_WaveHeight", WaveHeight);
        _oceanMaterial.SetFloat("_WaveSpeed", WaveSpeed);
        _oceanMaterial.SetFloat("_WaveSpeed2", WaveSpeed2);
        _oceanMaterial.SetFloat("_WaveFrequency", WaveFrequency);   
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

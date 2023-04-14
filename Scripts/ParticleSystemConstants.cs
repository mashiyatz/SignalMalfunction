using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemConstants : MonoBehaviour
{
    public int _rateOfEmissionBody;
    public int _rateOfEmissionJoints;
    public int _rateOfEmissionChange;
    public float _timeToTransition;
    public Transform _mainCamera;
    public Vector3 _cameraDefaultPos;
    public Material _jointsMaterialTransparent;
    public Material _bodyMaterialTransparent;
    public Material _jointsMaterialOpaque;
    public Material _bodyMaterialOpaque;

    public static int rateOfEmissionBody;
    public static int rateOfEmissionJoints;
    public static int rateOfEmissionChange;
    public static float timeToTransition;
    public static Transform mainCamera;
    public static Vector3 cameraDefaultPos;
    public static Material jointsMaterialTransparent;
    public static Material bodyMaterialTransparent;
    public static Material jointsMaterialOpaque;
    public static Material bodyMaterialOpaque;

    public static bool isListening;

    private void Start()
    {
        rateOfEmissionBody = _rateOfEmissionBody;
        rateOfEmissionJoints = _rateOfEmissionJoints;
        rateOfEmissionChange = _rateOfEmissionChange;
        timeToTransition = _timeToTransition;
        mainCamera = _mainCamera;
        cameraDefaultPos = _cameraDefaultPos;
        jointsMaterialTransparent = _jointsMaterialTransparent;
        bodyMaterialTransparent = _bodyMaterialTransparent;
        jointsMaterialOpaque = _jointsMaterialOpaque;
        bodyMaterialOpaque = _bodyMaterialOpaque;
        isListening = false; 
}
}

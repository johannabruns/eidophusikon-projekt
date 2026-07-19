using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public CinemachineCamera cam;

    [Header("Static Stage View Cam Configuration")]
    [SerializeField] private float stageSize = 8f;
    [SerializeField] private Vector3 stagePos = new(0f, 0.055f, -10f);

    [Header("Dynamic Player Follow Cam Configuration")]
    [SerializeField] private float followSize = 8f;
    public Transform trackingTarget = null;

    /*
    private CinemachineBasicMultiChannelPerlin noise;

    void Awake()
    {
        noise = GetComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void Shake(float intensity, float time)
    {
        StartCoroutine(CameraShakeCoroutine(intensity, time));
    }

    private IEnumerator CameraShakeCoroutine(float intensity, float duration)
    {
        noise.AmplitudeGain = intensity;
        yield return new WaitForSeconds(duration);
        noise.AmplitudeGain = 0f;
    }

    */




}

using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public CinemachineCamera cam;

    [Header("Dynamic Player Follow Cam Configuration")]
    public Transform trackingTarget = null;

    void Start()
    {
        if (cam == null)
        {
            cam = GetComponent<CinemachineCamera>();
        }
    }

    void Update()
    {
        // Wir suchen nach dem lokalen Spieler im Netzwerk
        if (trackingTarget == null)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            
            foreach (GameObject p in players)
            {
                // Wir schnappen uns nur den Klon, der im Netzwerk gespawnt wurde (ignoriere die Referenz)
                if (p.scene.name != "DontDestroyOnLoad" && p.name.Contains("Player"))
                {
                    trackingTarget = p.transform;
                    break;
                }
            }
        }

        // Zwinge die Cinemachine-Kamera permanent dazu, dem Target zu folgen
        if (cam != null && trackingTarget != null)
        {
            cam.Follow = trackingTarget;
        }
    }
}
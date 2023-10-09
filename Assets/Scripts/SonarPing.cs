using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SonarPing : MonoBehaviour
{
    [SerializeField]
    public Text sonarLabel;

    public float maxRange = 20.0f;
    public float minRange = 0.01f;
    public float noiseAmount = 1.0f;
    private bool hasHit = false;
    private float lastHit = 0.0f;

    void Start()
    {
        
    }

    void FixedUpdate() {
        // Raycast down up to a max range
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, maxRange)) {
            float hitDist = hit.distance + ((Random.value - 0.5f) * noiseAmount * 2.0f);
            if (hitDist >= minRange && hitDist <= maxRange) {
                hasHit = true;
                lastHit = hitDist;
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * hitDist, Color.green);
            } else {
                hasHit = false;
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * hitDist, Color.red);
            }
        }

        if (hasHit) {
            sonarLabel.text = lastHit + " m";
        } else {
            sonarLabel.text = "-- m";
        }
        
    }
}

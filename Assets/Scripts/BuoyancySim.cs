using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class BuoyancySim : MonoBehaviour
{
    public Rigidbody rb;

    public float depthBefSub;
    public float displacementAmt;

    public int floaters;
    public float waterDrag;
    public float waterAngularDrag;
    public WaterSurface water;
    WaterSearchParameters Search;
    WaterSearchResult SearchResult;

    private void FixedUpdate() {
        //rb.AddForceAtPosition(Physics.gravity / floaters, transform.position, ForceMode.Acceleration);

        Search.startPosition = transform.position;
        water.FindWaterSurfaceHeight(Search, out SearchResult);

        if (transform.position.y < SearchResult.height) {
            float displacementMulti = Mathf.Clamp01(SearchResult.height - transform.position.y / depthBefSub) * displacementAmt;

            rb.AddForceAtPosition(new Vector3(0f, 1f * displacementMulti, 0f), transform.position, ForceMode.Acceleration);

            rb.AddForce(displacementMulti * -rb.velocity * waterDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
            rb.AddTorque(displacementMulti * -rb.angularVelocity * waterAngularDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }
    }
}

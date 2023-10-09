using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class RoverPhysics : MonoBehaviour
{
    public Rigidbody rb;
    public GameObject leftThrusterPoint;
    public GameObject rightThrusterPoint;
    public GameObject leftPropeller;
    public GameObject rightPropeller;

    public UInt16[] rawPWM = new UInt16[16];

    public float thrustMax = 30.0f;
    public float leftThrottle = 0.0f;
    public float rightThrottle = 0.0f;
    
    public float fluidDensity = 1.0f;
    public float displacementVolume = 80.0f;
    public GameObject[] buoyancySamples;

    public float waterDrag = 0.5f;
    public float waterAngularDrag = 0.5f;

    public bool keyboardControl = false;
    public bool rcControl = false;

    public WaterSurface water;
    WaterSearchParameters searchParameters = new WaterSearchParameters();
    WaterSearchResult searchResult = new WaterSearchResult();

    void Start() {
        for (int i = 0; i < rawPWM.Length; i++) {
            rawPWM[i] = 1500;
        }
    }

    private float GetWaterHeight(Vector3 pos) {
        searchParameters.startPosition = transform.position + new Vector3(0.0f, 20.0f, 0.0f);
        searchParameters.targetPosition = transform.position - new Vector3(0.0f, 10.0f, 0.0f);
        searchParameters.error = 0.001f;
        searchParameters.maxIterations = 16;
        water.FindWaterSurfaceHeight(searchParameters, out searchResult);
        return searchResult.height;
    }

    float ScaleValue(float OldMin, float OldMax, float NewMin, float NewMax, float OldValue){
    
        float OldRange = (OldMax - OldMin);
        float NewRange = (NewMax - NewMin);
        float NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;
    
        return(NewValue);
    }

    void FixedUpdate() {
        // Keyboard control
        if (keyboardControl) {
            if (Input.GetKey("w")) {
                leftThrottle = 1.0f;
                rightThrottle = 1.0f;
            } else if (Input.GetKey("s")) {
                leftThrottle = -1.0f;
                rightThrottle = -1.0f;
            } else {
                leftThrottle = 0.0f;
                rightThrottle = 0.0f;
            }
            if (Input.GetKey("a")) {
                leftThrottle *= 0.5f;
                rightThrottle += 0.5f;
            }
            if (Input.GetKey("d")) {
                leftThrottle += 0.5f;
                rightThrottle *= 0.5f;
            }
        }
        if (rcControl) {
            leftThrottle = Mathf.Clamp(ScaleValue(900.0f, 2100.0f, -1.0f, 1.0f, rawPWM[0]), -1.0f, 1.0f);
            rightThrottle = Mathf.Clamp(ScaleValue(900.0f, 2100.0f, -1.0f, 1.0f, rawPWM[2]), -1.0f, 1.0f);
        }

        leftThrottle = Mathf.Clamp(leftThrottle, -1.0f, 1.0f);
        rightThrottle = Mathf.Clamp(rightThrottle, -1.0f, 1.0f); 

        if (leftPropeller) {
            leftPropeller.transform.Rotate(new Vector3(0.0f, 0.0f, 1.0f) * leftThrottle * 150.0f);
        }
        if (rightPropeller) {
            rightPropeller.transform.Rotate(new Vector3(0.0f, 0.0f, 1.0f) * rightThrottle * 150.0f);
        }

        // Compute global force from left and right thrusters.
        Vector3 leftForcePoint = leftThrusterPoint.transform.position;
        Vector3 rightForcePoint = rightThrusterPoint.transform.position;
        Vector3 leftForce = transform.TransformDirection(new Vector3(-leftThrottle * thrustMax, 0.0f, 0.0f));
        Vector3 rightForce = transform.TransformDirection(new Vector3(-rightThrottle * thrustMax, 0.0f, 0.0f));


        // Thrust only works if the thruster is underwater.
        if (leftForcePoint.y >= GetWaterHeight(leftForcePoint)) {
            leftForce = new Vector3(0.0f, 0.0f, 0.0f);
        }
        if (rightForcePoint.y >= GetWaterHeight(rightForcePoint)) {
            rightForce = new Vector3(0.0f, 0.0f, 0.0f);
        }

        rb.AddForceAtPosition(leftForce, leftForcePoint);
        rb.AddForceAtPosition(rightForce, rightForcePoint);


        Debug.DrawRay(leftForcePoint, leftForce * 0.1f);
        Debug.DrawRay(rightForcePoint, rightForce * 0.1f);

        for (int i = 0; i < buoyancySamples.Length; i++) {
            AddBuoyancyForce(buoyancySamples[i].transform.position);
        }
    }
    
    private void AddBuoyancyForce(Vector3 posGlobal) {
        // Compute depth below water at given point.
        float waterHeight = GetWaterHeight(posGlobal);
        float depth = waterHeight - posGlobal.y;
        if (depth >= 0.0f) {
            // Compute a rough approximation of the displacement of water.
            // Assumes the bouyancy volume is fully submerged at 0.15m
            float fractionSubmerged = Mathf.Clamp(depth / 0.3f, 0.0f, 1.0f);
            float displacedVolume = (displacementVolume / buoyancySamples.Length) * fractionSubmerged;

            // Compute buoyant force using Archimedes principle
            // Fb = p x g x V
            // Fb: Bouyancy force, Newtons
            // p: Fluid density, kg/m^3
            // V: Displaced volume, m^3

            // p = Mass / Volume
            Vector3 bouyantForce = new Vector3(0f, Mathf.Abs(Physics.gravity.y) * (displacedVolume) * fluidDensity, 0.0f);

            // Apply forces.
            rb.AddForceAtPosition(bouyantForce, posGlobal, ForceMode.Force);
            Debug.DrawRay(posGlobal, bouyantForce * 0.1f);

            // Apply drag due to water.            
            rb.AddForce((1.0f / buoyancySamples.Length) * fractionSubmerged * -rb.velocity * waterDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
            //rb.AddTorque((1.0f / buoyancySamples.Length) * fractionSubmerged * -rb.angularVelocity * waterAngularDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }
    }
}

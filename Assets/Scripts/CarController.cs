using UnityEngine;
using System;

public class CarController : MonoBehaviour {
    [Tooltip("Maximum steering angle of the wheels")]
    public float maxAngle = 30f;
    [Tooltip("Maximum torque applied to the driving wheels")]
    public float maxTorque = 300f;
    [Tooltip("Maximum brake torque applied to the driving wheels")]
    public float brakeTorque = 30000f;
    [Tooltip("If you need the visual wheels to be attached automatically, drag the wheel shape here.")]
    public GameObject wheelShape;

    [Tooltip("The vehicle's speed when the physics engine can use different amount of sub-steps (in m/s).")]
    public float criticalSpeed = 5f;
    [Tooltip("Simulation sub-steps when the speed is above critical.")]
    public int stepsBelow = 5;
    [Tooltip("Simulation sub-steps when the speed is below critical.")]
    public int stepsAbove = 1;

    [Range(0f, 10f)]
    public float sidewaysExtremumSlip = 1;

    [Range(0f, 10f)]
    public float sidewaysAsymptoteSlip = 2;

    [Range(0f, 10f)]
    public float sidewaysAsymptoteValue = 1;

    [Range(0f, 10f)]
    public float sidewaysExtremumValue = 2;

    [Range(0f, 10f)]
    public float sidewaysStiffness = 1;

    [Range(0f, 10f)]
    public float forwardExtremumSlip = 1;

    [Range(0f, 10f)]
    public float forwardAsymptoteSlip = 2;

    [Range(0f, 10f)]
    public float forwardAsymptoteValue = 1;

    [Range(0f, 10f)]
    public float forwardExtremumValue = 2;

    [Range(0f, 10f)]
    public float forwardStiffness = 1;

    public Transform centreOfMass;

    private WheelCollider[] m_Wheels;

    public float brakingMass;

    public Material wheelTouchMaterial;

    public Material wheelMaterial;

    private float initialMass;

    private float steerAngle;

    // Find all the WheelColliders down in the hierarchy.
    void Start() {

        public float GetSpeed() {
            return GetComponent<Rigidbody>().velocity.magnitude;
        }

        // This is a really simple approach to updating wheels.
        // We simulate a rear wheel drive car and assume that the car is perfectly symmetric at local zero.
        // This helps us to figure our which wheels are front ones and which are rear.
        void Update() {
        }
    }

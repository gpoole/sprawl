﻿using UnityEngine;
using System;

[Serializable]
public enum DriveType
{
	RearWheelDrive,
	FrontWheelDrive,
	AllWheelDrive
}

public class WheelDrive : MonoBehaviour
{
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

	[Tooltip("The vehicle's drive type: rear-wheels drive, front-wheels drive or all-wheels drive.")]
	public DriveType driveType;
	
	public float sidewaysExtremumSlip = 1;

	public float sidewaysAsymptoteSlip = 2;
	
	public float sidewaysAsymptoteValue = 10000;
	
	public float sidewaysExtremumValue = 20000;

	public float sidewaysStiffness = 1;

	public float forwardAsymptoteSlip = 2;
	
	public float forwardAsymptoteValue = 10000;


	public float forwardExtremumSlip = 1;
	
	public float forwardExtremumValue = 20000;

	public float forwardStiffness = 1;

    private WheelCollider[] m_Wheels;

    // Find all the WheelColliders down in the hierarchy.
	void Start()
	{
		WheelFrictionCurve sidewaysCurve = new WheelFrictionCurve();
		sidewaysCurve.asymptoteSlip = sidewaysAsymptoteSlip;
		sidewaysCurve.asymptoteValue = sidewaysAsymptoteValue;
		sidewaysCurve.extremumSlip = sidewaysExtremumSlip;
		sidewaysCurve.extremumValue = sidewaysExtremumValue;
		sidewaysCurve.stiffness = sidewaysStiffness;

		WheelFrictionCurve forwardCurve = new WheelFrictionCurve();
		forwardCurve.asymptoteSlip = forwardAsymptoteSlip;
		forwardCurve.asymptoteValue = forwardAsymptoteValue;
		forwardCurve.extremumSlip = forwardExtremumSlip;
		forwardCurve.extremumValue = forwardExtremumValue;
		forwardCurve.stiffness = forwardStiffness;

		m_Wheels = GetComponentsInChildren<WheelCollider>();

		for (int i = 0; i < m_Wheels.Length; ++i) 
		{
			var wheel = m_Wheels [i];
			wheel.sidewaysFriction = sidewaysCurve;
			wheel.forwardFriction = forwardCurve;

			// Create wheel shapes only when needed.
			if (wheelShape != null)
			{
				var ws = Instantiate (wheelShape);
				ws.transform.parent = wheel.transform;
			}
		}
	}

	// This is a really simple approach to updating wheels.
	// We simulate a rear wheel drive car and assume that the car is perfectly symmetric at local zero.
	// This helps us to figure our which wheels are front ones and which are rear.
	void Update()
	{
		m_Wheels[0].ConfigureVehicleSubsteps(criticalSpeed, stepsBelow, stepsAbove);

		float angle = maxAngle * Input.GetAxis("Horizontal");
		float torque = maxTorque * Input.GetAxis("Vertical");

		float handBrake = Input.GetKey(KeyCode.Space) ? brakeTorque : 0;

		foreach (WheelCollider wheel in m_Wheels)
		{
			// A simple car where front wheels steer while rear ones drive.
			if (wheel.transform.localPosition.z > 0)
				wheel.steerAngle = angle;

			if (wheel.transform.localPosition.z < 0)
			{
				wheel.brakeTorque = handBrake;
			}

			if (wheel.transform.localPosition.z < 0 && driveType != DriveType.FrontWheelDrive)
			{
				wheel.motorTorque = torque;
			}

			if (wheel.transform.localPosition.z >= 0 && driveType != DriveType.RearWheelDrive)
			{
				wheel.motorTorque = torque;
			}

			// Update visual wheels if any.
			if (wheelShape) 
			{
				Quaternion q;
				Vector3 p;
				wheel.GetWorldPose (out p, out q);

				// Assume that the only child of the wheelcollider is the wheel shape.
				Transform shapeTransform = wheel.transform.GetChild (0);
				shapeTransform.position = p;
				shapeTransform.rotation = q;
			}
		}
	}

	public void Reset() {
		GetComponent<Rigidbody>().velocity = Vector3.zero;
	}
}

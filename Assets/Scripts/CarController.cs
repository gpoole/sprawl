using UnityEngine;
using System;

public class CarController : MonoBehaviour
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
	
	private float initialMass;

	private float steerAngle;

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

		initialMass = GetComponent<Rigidbody>().mass;

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

	public float GetSpeed() {
		return GetComponent<Rigidbody>().velocity.magnitude;
	}

	// This is a really simple approach to updating wheels.
	// We simulate a rear wheel drive car and assume that the car is perfectly symmetric at local zero.
	// This helps us to figure our which wheels are front ones and which are rear.
	void Update()
	{
		m_Wheels[0].ConfigureVehicleSubsteps(criticalSpeed, stepsBelow, stepsAbove);

		float wantedAngle = Input.GetAxis("Horizontal") * maxAngle;
		steerAngle = Mathf.Lerp(steerAngle, wantedAngle, Time.deltaTime * 6f);
		Debug.Log(wantedAngle + "," + steerAngle);

		float torque = maxTorque * Input.GetAxis("Vertical");
		bool useHandBrake = Input.GetKey(KeyCode.Space);

		var rigidBody = GetComponent<Rigidbody>();
		var transform = GetComponent<Transform>();
		if (useHandBrake) {
			rigidBody.centerOfMass = centreOfMass.localPosition;
			rigidBody.mass = brakingMass;
		} else {
			rigidBody.centerOfMass = Vector3.zero;
			rigidBody.mass = initialMass;
		}

		foreach (WheelCollider wheel in m_Wheels)
		{
			var isFrontWheel = wheel.transform.localPosition.z > 0;

			// A simple car where front wheels steer while rear ones drive.
			if (isFrontWheel)
				wheel.steerAngle = steerAngle;
			
			if (useHandBrake) {
				wheel.brakeTorque = brakeTorque;
			} else {
				wheel.brakeTorque = 0;
			}

			wheel.motorTorque = torque;

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
}

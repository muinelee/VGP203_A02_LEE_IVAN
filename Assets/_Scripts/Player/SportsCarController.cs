using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SportsCarController : MonoBehaviour
{
    [Header("Performance Settings")]
    public float motorForce = 2000f;
    public float brakeForce = 4000f;
    public float maxSteerAngle = 35f;
    public float maxSpeed = 150.0f;
    public float downforce = 100f;

    [Header("Component References")]
    public WheelCollider[] wheelColliders = new WheelCollider[4];
    public Transform[] wheelTransforms = new Transform[4];

    private float steerInput;
    private float gasInput;
    private float currentSpeed;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        currentSpeed = rb.velocity.magnitude * 3.6f;
        HandleMovement();
        HandleSteering();
        UpdateWheelPoses();
    }

    public void SetSteerInput(float value)
    {
        steerInput = value;
    }

    public void SetGasInput(float value)
    {
        gasInput = value;
    }

    private void HandleMovement()
    {
        if (IsGrounded())
        {
            HandleGroundedMovement();
            ApplyDownforce();
        }
        else
        {
            rb.drag = 0.1f; // Airborne drag
        }
    }

    private bool IsGrounded()
    {
        return wheelColliders.Any(wheel => wheel.isGrounded);
    }

    private void HandleGroundedMovement()
    {
        bool isReversing = gasInput < 0;
        float targetTorque = gasInput * motorForce;

        if (isReversing && currentSpeed > 5f)
        {
            ApplyBrakeTorque();
        }
        else if (!isReversing && (gasInput == 0 || currentSpeed > maxSpeed))
        {
            ApplyBrakeTorque();
        }
        else
        {
            ApplyMotorTorque(targetTorque);
        }
    }

    private void ApplyDownforce()
    {
        rb.AddForce(-transform.up * downforce * rb.velocity.magnitude);
    }

    private void ApplyMotorTorque(float torque)
    {
        SetWheelTorques(torque, 0);
        rb.drag = 0.1f; // Normal driving drag
    }

    private void ApplyBrakeTorque()
    {
        SetWheelTorques(0, brakeForce);
        rb.drag = 1f; // Braking drag
    }

    private void SetWheelTorques(float motorTorque, float brakeTorque)
    {
        foreach (WheelCollider wheel in wheelColliders)
        {
            wheel.motorTorque = motorTorque;
            wheel.brakeTorque = brakeTorque;
        }
    }

    private void HandleSteering()
    {
        float steerAngle = maxSteerAngle * steerInput;
        wheelColliders[0].steerAngle = steerAngle;
        wheelColliders[1].steerAngle = steerAngle;
    }

    private void UpdateWheelPoses()
    {
        for (int i = 0; i < wheelColliders.Length; i++)
        {
            UpdateWheelPose(wheelColliders[i], wheelTransforms[i]);
        }
    }

    private void UpdateWheelPose(WheelCollider collider, Transform transform)
    {
        Vector3 pos;
        Quaternion quat;
        collider.GetWorldPose(out pos, out quat);
        transform.position = pos;
        transform.rotation = quat;
    }
}
using System;
using UnityEngine;
using World;

public class Player : MonoBehaviour
{
    public Transform sphere;
    public Transform cam;
    public float minSpeed = 1.0f,
        maxSpeed = 10f,
        rotationSpeed = 1f,
        maxStrafeAngle = 25,
        strafeSpeed = 1,
        minAngleX = -30,
        maxAngleX = 30,
        minAngleY = -85,
        maxAngleY = 60,
        mouseSensitivity = 100;

    public float speed { get; set; }
    private float _targetSpeed;
    private Vector3 _targetLookDir;
    private float _strafeAngle;
    private float _mouseLookAngleX,
        _mouseLookAngleY;
    private Vector3 _startCamAngles;

    void Start()
    {
        _startCamAngles = cam.localEulerAngles;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X"),
            mouseY = -Input.GetAxis("Mouse Y");
        if (mouseX == 0)
        {
            _mouseLookAngleX /= 1.001f;
            if (Mathf.Abs(_mouseLookAngleX) < 0.001f)
            {
                _mouseLookAngleX = 0;
            }
        }
        else
        {
            _mouseLookAngleX += mouseX * mouseSensitivity * Time.deltaTime;
            _mouseLookAngleX = Mathf.Clamp(_mouseLookAngleX, minAngleX, maxAngleX);
        }
        if (mouseY == 0)
        {
            _mouseLookAngleY /= 1.001f;
            if (Mathf.Abs(_mouseLookAngleY) < 0.001f)
            {
                _mouseLookAngleY = 0;
            }
        }
        else
        {
            _mouseLookAngleY += mouseY * mouseSensitivity * Time.deltaTime;
            _mouseLookAngleY = Mathf.Clamp(_mouseLookAngleY, minAngleY, maxAngleY);
        }
        cam.localEulerAngles = _startCamAngles + new Vector3(_mouseLookAngleY, _mouseLookAngleX, 0);

        Vector3 lookDir = Vector3.Lerp(transform.forward, _targetLookDir, rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.LookRotation(lookDir, sphere.position - transform.position);
    }

    void FixedUpdate()
    {
        SetSpeed();
        SetMovement();
    }

    private void SetSpeed()
    {
        if (speed < _targetSpeed)
        {
            if (speed < 0.00001f)
                speed = 0.001f;
            speed = Mathf.Min(speed * 1.1f, _targetSpeed);
        }
        else if (speed > _targetSpeed)
        {
            speed = Mathf.Max(speed / 1.1f, _targetSpeed);
            //if (speed < 0.001f)
            //    speed = 0;
        }

        sphere.transform.eulerAngles = new Vector3(0, 0, _strafeAngle);
    }

    private void SetMovement()
    {
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.Alpha6))
        {
            _strafeAngle = Mathf.Min(_strafeAngle + strafeSpeed * Time.fixedDeltaTime, maxStrafeAngle);
        }
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.Alpha4))
        {
            _strafeAngle = Mathf.Max(_strafeAngle - strafeSpeed * Time.fixedDeltaTime, -maxStrafeAngle);
        }
        else
        {
            _strafeAngle /= 1.01f;
            if (Mathf.Abs(_strafeAngle) < 0.001f)
            {
                _strafeAngle = 0;
            }
        }
    }

    public void EnterPlane(TerrainChunk plane, Vector3 moveDir)
    {
        float angle = plane.transform.localEulerAngles.x - 90,
            t = Mathf.Clamp(angle / 90f, 0, 1);
        _targetSpeed = t * maxSpeed + (1 - t) * minSpeed;

        _targetLookDir = moveDir;
    }
}

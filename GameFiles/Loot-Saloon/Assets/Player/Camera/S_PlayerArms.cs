using System;
using Unity.Netcode;
using UnityEngine;

public class S_PlayerArms : MonoBehaviour
{
    private Vector2 _lookInput;
    private float _sensitivity = 100f;
    float _xRotation = 0f;
    private Transform _playerTransform;
    private void Start()
    {
        _playerTransform = transform.parent.transform;
        if (_playerTransform.parent.GetComponent<NetworkObject>().IsOwner)
        {
            S_PlayerInputsReciever.OnLook += GetLookInput;
        }
    }

    void Update()
    {
        float mouseY = _lookInput.y * _sensitivity * Time.deltaTime;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
    }
    
    private void GetLookInput(Vector2 lookInput)
    {
        _lookInput = lookInput;
    }
}

using UnityEngine;

public class S_CameraFollower : MonoBehaviour
{
    [SerializeField] private Camera _camera;

    [SerializeField] private Vector3 _offset = new Vector3();

    void Start()
    {
        if (_camera == null)
            _camera = Camera.main;
    }

    void Update()
    {
        transform.position = _camera.transform.position
                           + _camera.transform.forward * _offset.z
                           + _camera.transform.right * _offset.x
                           + _camera.transform.up * _offset.y;
    }
}

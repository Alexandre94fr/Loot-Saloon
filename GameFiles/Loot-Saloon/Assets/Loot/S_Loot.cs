using TMPro;
using UnityEngine;

public class S_Loot : S_Pickable
{
    // this is set by the instantiator
    //[HideInInspector]
    public SO_LootProperties properties;

    [SerializeField] private Canvas _worldCanvas;
    private Camera _currentCamera;

    public void SetWorldTextVisibility(bool visible, Camera cam)
    {
        if (_worldCanvas == null)
            return;

        _worldCanvas.gameObject.SetActive(visible);
        _currentCamera = visible ? cam : null;
    }

    private void LateUpdate()
    {
        if (_worldCanvas != null && _currentCamera != null)
        {
            _worldCanvas.transform.position = transform.position + Vector3.up * 0.5f;

            Vector3 direction = _currentCamera.transform.position - _worldCanvas.transform.position;
            direction.y = 0f;
            _worldCanvas.transform.rotation = Quaternion.LookRotation(-direction);
        }
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        TextMeshProUGUI text = _worldCanvas.GetComponentInChildren<TextMeshProUGUI>();
        text.text = $"{properties.lootName} : {properties.moneyValue}$";
    }


}

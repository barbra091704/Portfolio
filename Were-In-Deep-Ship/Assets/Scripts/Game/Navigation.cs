using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct LocationItem
{
    public int locationID;
    public Image locationItem;
}


public class Navigation : NetworkBehaviour, IInteractable
{
    public NetworkVariable<LocationData> SelectedLocation = new();
    public NetworkVariable<int> SelectedLocationIndex = new(0);
    public NetworkVariable<bool> LeverState = new();
    public Transform leverTransform;
    public Canvas canvas;
    public LocationItem[] locationItems;

    public override void OnNetworkSpawn()
    {
        GameManager.Singleton.CurrentLocation.OnValueChanged += CurrentLocationValueChanged;
    }
    public override void OnNetworkDespawn()
    {
        GameManager.Singleton.CurrentLocation.OnValueChanged -= CurrentLocationValueChanged;
    }
    public void Interact<T>(RaycastHit hit, NetworkObject player, T value)
    {
        if (value is bool boolValue)
        {
            SelectLocationRpc(boolValue);
        }
        else if (!LeverState.Value)
        {
            PullLeverRpc();
        }
    }

    [Rpc(SendTo.Server)]
    private void SelectLocationRpc(bool value)
    {
        DisableAllLocationItems();

        SelectedLocationIndex.Value = value ? SelectedLocationIndex.Value + 1 : SelectedLocationIndex.Value - 1;

        SelectedLocationIndex.Value = Mathf.Clamp(SelectedLocationIndex.Value, 0, GameManager.Singleton.Locations.Length -1);

        SelectedLocation.Value = GameManager.Singleton.Locations[SelectedLocationIndex.Value];

        EnableLocationItemByID(SelectedLocationIndex.Value);
    }

    [Rpc(SendTo.Server)]
    public void PullLeverRpc()
    {
        StartCoroutine(PullLever());
    }

    public IEnumerator PullLever()
    {
        LeverState.Value = true;

        float duration = 0.5f;
        float elapsedTime = 0f;
        Quaternion startRotation = Quaternion.Euler(0, 0, 0);
        Quaternion endRotation = Quaternion.Euler(0, 0, 90);

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            
            leverTransform.localRotation = Quaternion.Lerp(startRotation, endRotation, t);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        leverTransform.localRotation = endRotation;

        yield return new WaitForEndOfFrame();

        GameManager.Singleton.CurrentLocation.Value = SelectedLocation.Value;

        yield return new WaitForSeconds(1f);

        leverTransform.localRotation = startRotation;

        LeverState.Value = false;
    }


    private void CurrentLocationValueChanged(LocationData old, LocationData newValue)
    {
        print($"Boat Received Callback From GameManager To Travel To: {newValue.SceneName} [ CurrentLocationValueChanged ] - Boat");

        StartCoroutine(TravelToNewLocation());
    }

    private IEnumerator TravelToNewLocation()
    {
        foreach (var item in GameManager.Singleton.PlayerDatas)
        {
            if (item.clientID == NetworkManager.LocalClientId){

                if(item.Reference.TryGet(out NetworkObject playerObj))
                {
                    GUIManager.Singleton.ToggleUI(default, false);

                    FadeBlack.Singleton.Fade(0.5f, 4, 0.5f);

                    yield return new WaitForSeconds(3f);

                    BoxCollider collider = GetComponent<BoxCollider>();
                    if (!collider.bounds.Contains(playerObj.transform.position + Vector3.up))
                    {
                        playerObj.transform.position = RandomPointInBounds(collider.bounds);
                        print("RandomBounds");
                    }

                    GUIManager.Singleton.ToggleUI(default, true);

                    break;
                }
            }
        }
    }
    public Vector3 RandomPointInBounds(Bounds bounds) {
        return new Vector3(
            UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
            UnityEngine.Random.Range(bounds.min.y, bounds.max.y),
            UnityEngine.Random.Range(bounds.min.z, bounds.max.z)
        );
    }

    public void DisableAllLocationItems()
    {
        foreach (var item in locationItems)
        {
            if (item.locationItem.isActiveAndEnabled)
            {
                item.locationItem.gameObject.SetActive(false);
            }
        }
    }
    public void EnableLocationItemByID(int id)
    {
        foreach (var item in locationItems)
        {
            if (item.locationID == id)
            {
                item.locationItem.gameObject.SetActive(true);
            }
        }
    }
}

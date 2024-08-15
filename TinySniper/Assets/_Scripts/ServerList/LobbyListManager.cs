using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Steamworks.Data;
using TMPro;

public class LobbyListManager : MonoBehaviour
{
    public GameObject lobbyDataItemPrefab;
    public GameObject lobbyListContent;
    public TMP_Dropdown dropdown;
    public List<GameObject> listOfLobbies = new();
    private Lobby[] lobbies;
    private void Start(){
        SearchForLobbies();
    }
    public void DestroyLobbies(){
        foreach(var lobbyItem in listOfLobbies){
            Destroy(lobbyItem);
        }
        listOfLobbies.Clear();
    }   
    public async void SearchForLobbies(){
        if (listOfLobbies.Count > 0) { DestroyLobbies(); }

        switch(dropdown.value){
            case 0:
                lobbies = await SteamMatchmaking.LobbyList.FilterDistanceWorldwide().WithSlotsAvailable(1).RequestAsync();
                break;
            case 1:
                lobbies = await SteamMatchmaking.LobbyList.FilterDistanceFar().WithSlotsAvailable(1).RequestAsync();
                break;
            case 2:
                lobbies = await SteamMatchmaking.LobbyList.FilterDistanceClose().WithSlotsAvailable(1).RequestAsync();
                break;
        }
        DisplayLobbies(lobbies);
    }
    public void DisplayLobbies(Lobby[] lobbies){
        for (int i = 0; i < lobbies.Length; i++)
        {
            GameObject createdItem = Instantiate(lobbyDataItemPrefab);
            LobbyDataEntry lobbyDataEntry = createdItem.GetComponent<LobbyDataEntry>();
            lobbyDataEntry.lobbyData.lobbyId = lobbies[i].Id;
            lobbyDataEntry.lobbyData.lobbyName = lobbies[i].GetData("LobbyName");
            lobbyDataEntry.lobbyData.currentPlayers = lobbies[i].MemberCount;
            lobbyDataEntry.lobbyData.maxPlayers = lobbies[i].MaxMembers;
            lobbyDataEntry.SetLobbyData();
            createdItem.transform.SetParent(lobbyListContent.transform);
            createdItem.transform.localScale = Vector3.one;

            listOfLobbies.Add(createdItem);
        }
    }
}

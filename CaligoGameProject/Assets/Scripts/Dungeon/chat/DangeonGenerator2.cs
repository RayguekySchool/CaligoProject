using System.Collections.Generic;
using UnityEngine;

public class DangeonGenerator2 : MonoBehaviour
{
    public GameObject[] roomPrefabs;
    public int maxRooms = 10;
    private List<Transform> openSockets = new List<Transform>();

    void Start()
    {
        GenerateDungeon();
    }

    void GenerateDungeon()
    {
        // Cria a primeira sala
        GameObject startRoom = Instantiate(roomPrefabs[0], Vector3.zero, Quaternion.identity);
        AddSockets(startRoom);

        for (int i = 1; i < maxRooms; i++)
        {
            if (openSockets.Count == 0) break;

            Transform socket = openSockets[0];
            openSockets.RemoveAt(0);

            GameObject roomPrefab = roomPrefabs[Random.Range(0, roomPrefabs.Length)];
            GameObject newRoom = Instantiate(roomPrefab);

            // Alinha a nova sala no socket
            Transform newSocket = GetRandomSocket(newRoom);
            if (newSocket == null) continue;

            AlignToSocket(newRoom, newSocket, socket);
            AddSockets(newRoom);
        }
    }

    void AddSockets(GameObject room)
    {
        Room roomScript = room.GetComponent<Room>();
        foreach (Transform socket in roomScript.sockets)
        {
            if (!socket.GetComponent<RoomSocket>().isConnected)
            {
                openSockets.Add(socket);
            }
        }
    }

    Transform GetRandomSocket(GameObject room)
    {
        Room roomScript = room.GetComponent<Room>();
        if (roomScript.sockets.Length == 0) return null;
        return roomScript.sockets[Random.Range(0, roomScript.sockets.Length)];
    }

    void AlignToSocket(GameObject room, Transform roomSocket, Transform targetSocket)
    {
        Vector3 offset = targetSocket.position - roomSocket.position;
        room.transform.position += offset;

        roomSocket.GetComponent<RoomSocket>().isConnected = true;
        targetSocket.GetComponent<RoomSocket>().isConnected = true;
    }
}

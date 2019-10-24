using System.Net;
using System.Runtime.InteropServices;
using UnityEngine;

using Morko.Network;

public class ServerController : MonoBehaviour
{
	public enum StatusType {Idle, Broadcasting, RunningGame};
	public StatusType Status { get; private set; }

	private Server server;

	public string [] players;

	public void CreateServer(ServerCreateInfo createInfo)
	{
		server = Server.Create(createInfo);
		server.OnPlayerAdded += () => players = server.PlayersNames;
	}

	public void CloseServer()
	{
		server.StopBroadcasting();
		server.AbortGame();
		server.Close();
	}

	public void StartBroadcast()
	{
		server.StartBroadcasting();
		Status = StatusType.Broadcasting;
	}

	public void StopBroadcast()
	{
		server.StopBroadcasting();
		Status = StatusType.Idle;
	}

	public void StartGame()
	{
		Debug.Log("[SERVER] start game");
		server.StartGame();
		Status = StatusType.RunningGame;
	}

	public void AbortGame()
	{
		Debug.Log("[SERVER] abort game");
		server.AbortGame();
		Status = StatusType.Idle;
	}

	public void OnDisable()
	{
		server?.StopBroadcasting();
		server?.AbortGame();
		server?.Close();
	}

	public void AddHostingPlayer(string name, IPEndPoint endPoint)
	{
		server.AddHostingPlayer(name, endPoint);
	}

}
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class UIController
{
	[Serializable]
	private struct HostView : IMenuLayout
	{
		public int serverIndex;

		public MenuView view;

		MenuView IMenuLayout.View => view;
		bool IMenuLayout.BelongsToMainMenu => true;

		public const string defaultServerName = "Default Server";
		public const string defaultPlayerName = "Hosting Player";

		public Button cancelButton;
		public Button createRoomButton;
		public InputField playerNameField;
		public InputField serverNameField;
		public DiscreteInputField playerCountField;
		public DiscreteInputField gameDurationField;
		public CircularScroll mapSelectionList;
	}
	[SerializeField] private HostView hostView;

	private void InitializeHostView()
	{
		hostView.view.OnShow += () => hostView.mapSelectionList.listItemContainer.SetActive(true);

		hostView.createRoomButton.onClick.AddListener(() =>
		{
			var info = new ServerInfo
			{
				hostingPlayerName = hostView.playerNameField.text,
				serverName = hostView.serverNameField.text,
				mapIndex = 0, //hostView.mapSelectionList.currentItem.ID,
				maxPlayers = hostView.playerCountField.IntValue,
				gameDurationSeconds = hostView.gameDurationField.IntValue,
				joinedPlayers = new JoinInfo[hostView.playerCountField.IntValue],
			};

			serverControls.CreateServer(info);
			EventSystem.current.SetSelectedGameObject(hostLobbyView.startGameButton.gameObject);
			SetView(hostLobbyView);
			DisplayJoinedPlayersList(info);
		});
		hostView.cancelButton.onClick.AddListener(() =>
		{
			EventSystem.current.SetSelectedGameObject(mainView.hostViewButton.gameObject);
			SetMainView();
		});

		hostView.playerNameField.text = HostView.defaultPlayerName;
		hostView.serverNameField.text = HostView.defaultServerName;
	}
}
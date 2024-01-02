using Alteruna;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomPreview(typeof(Multiplayer))]
public class NetworkManagerPreview : ObjectPreview
{
    private bool m_IsConnected = false;
    public NetworkManagerPreview()
    {
        if (!m_IsConnected)
        {
            GameObject.FindObjectOfType<Multiplayer>().TryGetComponent(out MultiplayerManagerExtension extension);
            if (extension == null)
            {
                GameObject.FindObjectOfType<Multiplayer>().AddComponent<MultiplayerManagerExtension>();
                m_IsConnected = true;

                Debug.Log("MultiplayerManagerExtension added to components.");
            }
        }
    }
    public override bool HasPreviewGUI()
    {
        return true;
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        if(Multiplayer.Instance == null)
        {
            GUILayout.Label("No active instance of Multiplayer manager found.");
            return;
        }

        if (Multiplayer.Instance.CurrentRoom == null)
        {
            GUILayout.Label("Not in a room...");
            return;
        }

        GUILayout.BeginVertical();
        GUILayout.Label($"{ Multiplayer.Instance.CurrentRoom.GetUserCount() } user(s) in the room");

        foreach (var client in Multiplayer.Instance.CurrentRoom.Users)
        {
            GUILayout.BeginHorizontal();

            var role = client.IsHost ? "Host" : "Client";
            GUILayout.Label($"{ client.Name } ({ role })");

            if (GUILayout.Button("Disconnect"))
            {
                MultiplayerManagerExtension.Instance.RequestKick(client);
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
    }
}
#endif


public class MultiplayerManagerExtension : AttributesSync
{
    public static MultiplayerManagerExtension Instance { get; private set; }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void RequestKick(User user)
    {
        Debug.Log($"Attempt to kick {user.Name}");
        InvokeRemoteMethod(nameof(Kick), user);
    }

    [SynchronizableMethod]
    private void Kick()
    {
        Multiplayer.Instance.CurrentRoom.Leave();
    }
}


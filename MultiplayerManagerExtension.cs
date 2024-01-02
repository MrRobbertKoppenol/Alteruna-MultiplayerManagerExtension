using Alteruna;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomPreview(typeof(Multiplayer))]
public class NetworkManagerPreview : ObjectPreview
{
    public NetworkManagerPreview()
    {
        GameObject.FindObjectOfType<Multiplayer>().TryGetComponent(out MultiplayerManagerExtension extension);
        if (extension == null)
        {
            Debug.Log("MultiplayerManagerExtension added to components.");
            GameObject.FindObjectOfType<Multiplayer>().AddComponent<MultiplayerManagerExtension>();
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
        Debug.Log($"Attempt to kick { user.Name }");
        InvokeRemoteMethod(nameof(Kick), user);
    }

    [SynchronizableMethod]
    private void Kick()
    {
        Multiplayer.Instance.CurrentRoom.Leave();
    }
}



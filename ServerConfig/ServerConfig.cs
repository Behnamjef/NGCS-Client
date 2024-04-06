using UnityEngine;

[CreateAssetMenu(fileName = "Config", order = 0)]
public class ServerConfig : ScriptableObject
{
    public string BaseUrl;
    public string RoomUrl;
    public string GameCenterUrl;

    public static ServerConfig Instance
    {
        get
        {
            if (_instance == null)
                _instance = UnityEngine.Resources.Load<ServerConfig>("Config");

            return _instance;
        }
    }

    private static ServerConfig _instance;
}

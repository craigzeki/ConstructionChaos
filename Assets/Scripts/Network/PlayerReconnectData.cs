/// <summary>
/// Class to store reconnection data to send to event subscribers
/// </summary>
public class PlayerReconnectData
{
    public ulong PreviousClientID { get; private set; }
    public ulong CurrentClientID { get; private set; }

    public PlayerReconnectData(ulong previousClientID, ulong currentClientID)
    {
        PreviousClientID = previousClientID;
        CurrentClientID = currentClientID;
    }
}

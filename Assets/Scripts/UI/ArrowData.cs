using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Collections;
using UnityEngine;

public class ArrowData : INetworkSerializable
{
    public uint ObjectToFollowId;
    public Zone.ZONE ZoneType;
    public FixedString64Bytes ZoneName; //! This may need to be changed to FixedString128Bytes or just keep zone names short
    public Color Colour;
    public bool IsGoalArrow;

    //public ArrowData(uint id, Zone.ZONE type, FixedString64Bytes name, Color colour, bool isGoalArrow)
    //{
        //ObjectToFollowId = id;
        //ZoneType = type;
        //ZoneName = name;
        //Colour = colour;
        //IsGoalArrow = isGoalArrow;
    //}

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ObjectToFollowId);
        serializer.SerializeValue(ref ZoneType);
        serializer.SerializeValue(ref ZoneName);
        serializer.SerializeValue(ref Colour);
        serializer.SerializeValue(ref IsGoalArrow);
    }
}

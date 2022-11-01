using System;
using System.IO;
using System.Runtime.Serialization;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Rejuvena.Gimmick.RecipeShuffler;

public sealed class PacketHandler
{
    [Serializable]
    public class ShufflerSyncException : Exception
    {
        public ShufflerSyncException() { }
        public ShufflerSyncException(string message) : base(message) { }
        public ShufflerSyncException(string message, Exception inner) : base(message, inner) { }

        protected ShufflerSyncException(
            SerializationInfo info,
            StreamingContext context
        ) : base(info, context) { }
    }
        
    public enum PacketType : byte
    {
        RequestSeedFromServer,
        SendSeedToClient
    }

    public RecipeShufflerMod Mod { get; }

    public PacketHandler(RecipeShufflerMod mod) {
        Mod = mod;
    }

    public void HandlePacket(BinaryReader reader, int whoAmI) {
        PacketType packetType = (PacketType) reader.ReadByte();

        switch (Main.netMode) {
            // Handle incoming packets from the server.
            case NetmodeID.MultiplayerClient:
                HandlePacketFromServer(packetType, reader, whoAmI);
                break;
                
            // Handle incoming packets from the client.
            case NetmodeID.Server:
                HandlePacketFromClient(packetType, reader, whoAmI);
                break;
                
            // On singleplayer, this should never be reached.
            case NetmodeID.SinglePlayer:
                throw new ShufflerSyncException("Somehow tried to sync in singleplayer!");
        }
    }

    private void HandlePacketFromClient(PacketType packetType, BinaryReader reader, int whoAmI) {
        switch (packetType) {
            case PacketType.RequestSeedFromServer:
                // Write the world seed and send it to the requester.
                // No reading needed since the client just sends a packet with the byte denoting the packet type.
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte) PacketType.SendSeedToClient);
                packet.Write(WorldGen.currentWorldSeed);
                packet.Send(whoAmI);
                break;

            default:
                throw new ShufflerSyncException($"Cannot handle packet type: {packetType}");
        }
    }

    private void HandlePacketFromServer(PacketType packetType, BinaryReader reader, int whoAmI) {
        WorldGen.currentWorldSeed = packetType switch
        {
            // Only need to read this data.
            PacketType.SendSeedToClient => reader.ReadString(),
            _ => throw new ShufflerSyncException($"Cannot handle packet type: {packetType}")
        };
    }
}
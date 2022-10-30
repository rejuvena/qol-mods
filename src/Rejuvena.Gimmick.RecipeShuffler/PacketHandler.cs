using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Rejuvena.Gimmick.RecipeShuffler
{
    public sealed class PacketHandler
    {
        public enum PacketType : byte
        {
            RequestSeedFromServer,
            SendSeedToClient
        }

        public RecipeShufflerMod Mod { get; }

        public PacketHandler(RecipeShufflerMod mod)
        {
            Mod = mod;
        }

        public void HandlePacket(BinaryReader reader, int whoAmI)
        {
            PacketType packetType = (PacketType)reader.ReadByte();

            // Handle incoming packets from the server.
            if (Main.netMode == NetmodeID.MultiplayerClient) HandlePacketFromServer(packetType, reader, whoAmI);
            // Handle incoming packets from the client.
            else if (Main.netMode == NetmodeID.Server) HandlePacketFromClient(packetType, reader, whoAmI);
            // On singleplayer, this should never be reached.
            else if (Main.netMode == NetmodeID.SinglePlayer) throw new Exception("Somehow tried to sync in singleplayer!");
        }

        private void HandlePacketFromClient(PacketType packetType, BinaryReader reader, int whoAmI)
        {
            switch (packetType)
            {
                case PacketType.RequestSeedFromServer:
                    // Write the world seed and send it to the requester.
                    // No reading needed since the client just sends a packet with the byte denoting the packet type.
                    ModPacket packet = Mod.GetPacket();
                    packet.Write((byte)PacketType.SendSeedToClient);
                    packet.Write(WorldGen.currentWorldSeed);
                    packet.Send(whoAmI);
                    break;

                default:
                    throw new Exception($"Cannot handle packet type: {packetType}");
            }
        }

        private void HandlePacketFromServer(PacketType packetType, BinaryReader reader, int whoAmI)
        {
            switch (packetType)
            {
                case PacketType.SendSeedToClient:
                    // Only need to read this data.
                    WorldGen.currentWorldSeed = reader.ReadString();
                    break;

                default:
                    throw new Exception($"Cannot handle packet type: {packetType}");
            }
        }
    }
}
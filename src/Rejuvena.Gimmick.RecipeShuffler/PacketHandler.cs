using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RecipeShuffler
{
    public class PacketHandler
    {
        public enum PacketType : byte
        {
            RequestSeedFromServer,
            SendSeedToClient
        }

        protected readonly RecipeShuffler Mod;

        public PacketHandler(RecipeShuffler mod)
        {
            Mod = mod;
        }

        public virtual void HandlePacket(BinaryReader reader, int whoAmI)
        {
            PacketType packetType = (PacketType)reader.ReadByte();

            // Handle incoming packets from the server.
            if (Main.netMode == NetmodeID.MultiplayerClient) HandlePacketFromServer(packetType, reader, whoAmI);
            // Handle incoming packets from the client.
            else if (Main.netMode == NetmodeID.Server) HandlePacketFromClient(packetType, reader, whoAmI);
            // On singleplayer, this should never be reached.
            else if (Main.netMode == NetmodeID.SinglePlayer) throw new Exception("Somehow tried to sync in singleplayer!");
        }

        protected virtual void HandlePacketFromClient(PacketType packetType, BinaryReader reader, int whoAmI)
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

        protected virtual void HandlePacketFromServer(PacketType packetType, BinaryReader reader, int whoAmI)
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
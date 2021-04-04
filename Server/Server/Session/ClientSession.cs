using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;
using Server.Game;
using System.Net;
using Google.Protobuf.Protocol;
using Google.Protobuf;
using Server.Data;

namespace Server
{
    public partial class ClientSession : PacketSession
    {
        public PlayerServerState ServerState { get; private set; } = PlayerServerState.ServerStateLogin;
        public Player MyPlayer { get; set; }
        public int SessionId { get; set; }

        // 패킷 임시저장
        List<ArraySegment<byte>> _reserveQueue = new List<ArraySegment<byte>>();
        object _lock = new object();

        #region NETWORK
        public void Send(IMessage packet)
        {
            // 패킷 이름을 이용해서 ID를 정함
            string msgName = packet.Descriptor.Name.Replace("_", string.Empty); // S_Chat => SChat
            MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), msgName);

            ushort size = (ushort)packet.CalculateSize();
            byte[] sendBuffer = new byte[size + 4];
            Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));
            Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));
            Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);


            lock (_lock)
            {
                // 예약만하고 보내지는 않음
                _reserveQueue.Add(sendBuffer);
            }
                //Send(new ArraySegment<byte>(sendBuffer));
        }

        // 실제 보내는 부분
        public void FlushSend()
        {
            List<ArraySegment<byte>> sendList = null;
            lock (_lock)
            {
                if (_reserveQueue.Count == 0)
                    return;
                sendList = _reserveQueue;
                _reserveQueue = new List<ArraySegment<byte>>();
            }

            Send(sendList);
        }

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected : {endPoint}");

            {
                S_Connected connectedPacket = new S_Connected();
                Send(connectedPacket);
            }
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            // TEMP 1번룸만 존재
            //RoomManager.Instance.Find(1).LeaveGame(MyPlayer.Info.ObjectId);
            GameLogic.Instance.Push(() =>
            {
                GameRoom room = GameLogic.Instance.Find(1);
                room.Push(room.LeaveGame, MyPlayer.Info.ObjectId);
            });
            SessionManager.Instance.Remove(this);

            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override void OnSend(int numOfBytes)
        {
            //Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
        #endregion
    }
}

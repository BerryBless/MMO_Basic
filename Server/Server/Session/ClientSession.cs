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
	public class ClientSession : PacketSession
	{
		public Player MyPlayer { get; set; }
		public int SessionId { get; set; }

		public void Send(IMessage packet)
        {
			// 패킷 이름을 이용해서 ID를 정함
			string msgName = packet.Descriptor.Name.Replace("_", string.Empty); // S_Chat => SChat
			MsgId msgId = (MsgId) Enum.Parse(typeof(MsgId), msgName);

			ushort size = (ushort)packet.CalculateSize();
			byte[] sendBuffer = new byte[size + 4];
			Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));
			Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));
			Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);

			Send(new ArraySegment<byte>(sendBuffer));

		}

		public override void OnConnected(EndPoint endPoint)
		{
			Console.WriteLine($"OnConnected : {endPoint}");

			// TODO DB에서 데이터 뽑아오기
			// Test코드
			MyPlayer = ObjectManager.Instance.Add<Player>();
            {
				MyPlayer.Info.Name = $"Player_{MyPlayer.Info.ObjectId}";
				MyPlayer.Info.PosInfo.State = CreatureState.Idle;
				MyPlayer.Info.PosInfo.MoveDir = MoveDir.Down;
				MyPlayer.Info.PosInfo.PosX =0;
				MyPlayer.Info.PosInfo.PosY =0;

				StatInfo stat = null;
				DataManager.StatDict.TryGetValue(1, out stat);
				MyPlayer.Stat.MergeFrom(stat);

				MyPlayer.Session = this;
            }

			//RoomManager.Instance.Find(1).EnterGame(MyPlayer);
			GameRoom room = RoomManager.Instance.Find(1);
			room.Push(room.EnterGame, MyPlayer);
		}

		public override void OnRecvPacket(ArraySegment<byte> buffer)
		{
			PacketManager.Instance.OnRecvPacket(this, buffer);
		}

		public override void OnDisconnected(EndPoint endPoint)
		{
			// TEMP 1번룸만 존재
			//RoomManager.Instance.Find(1).LeaveGame(MyPlayer.Info.ObjectId);
			GameRoom room = RoomManager.Instance.Find(1);
			room.Push(room.LeaveGame, MyPlayer.Info.ObjectId);

			SessionManager.Instance.Remove(this);

			Console.WriteLine($"OnDisconnected : {endPoint}");
		}

		public override void OnSend(int numOfBytes)
		{
			//Console.WriteLine($"Transferred bytes: {numOfBytes}");
		}
	}
}

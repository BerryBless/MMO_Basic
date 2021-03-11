using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using Server.Game;
using ServerCore;
using Server.Data;

namespace Server
{
	class Program
	{
		static Listener _listener = new Listener();

		static void FlushRoom()
		{
			JobTimer.Instance.Push(FlushRoom, 250);
		}

		static void Main(string[] args)
		{
			// "게임기획" 데이타 불러오기
			ConfigManager.LoadConfig();
			DataManager.LoadData();

			RoomManager.Instance.Add(2); // TEST 1은 맵ID

			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[0];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			_listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
			Console.WriteLine("Listening...");

			//FlushRoom();
			//JobTimer.Instance.Push(FlushRoom);


			while (true)
			{
				//JobTimer.Instance.Flush();
				// TODO 무식하게 짠 업데이트 테스트 후 잡타이머로 고쳐야함

				//RoomManager.Instance.Find(1).Update();
				GameRoom room = RoomManager.Instance.Find(1);
				room.Push(room.Update);

				Thread.Sleep(100);
			}
		}
	}
}

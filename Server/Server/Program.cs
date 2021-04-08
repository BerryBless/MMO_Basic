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
using Server.DB;
using System.Linq;

namespace Server
{
    class Program
    {
        static Listener _listener = new Listener();

        static void GameLogicTask()
        {
            while (true)
            {
                GameLogic.Instance.Update();
                Thread.Sleep(0);
            }
        }

        static void DbTask()
        {
            while (true)
            {
                DbTransaction.Instance.Flush();
                Thread.Sleep(0);
            }
        }


        static void NetworkTask()
        {
            while (true)
            {
                // 모든 플레이어를 순회
                List<ClientSession> sessions = SessionManager.Instance.GetSessions();
                foreach (ClientSession session in sessions)
                {
                    // Flush
                    session.FlushSend();
                }
                Thread.Sleep(0);
            }
        }

        static void Main(string[] args)
        {
            // "게임기획" 데이타 불러오기
            ConfigManager.LoadConfig();
            DataManager.LoadData();

            // 룸 생성
            GameLogic.Instance.Push(() =>
            {
                GameLogic.Instance.Add(3);
            });

            // DNS (Domain Name System)
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
            Console.WriteLine("Listening...");

            // NetworkTask
            {
                //Task networkTask = new Task(NetworkTask, TaskCreationOptions.LongRunning);
                //networkTask.Start();
                Thread t = new Thread(NetworkTask);
                t.Name = "Network Send";
                t.Start();
            }

            // DbTask
            {
                //Task dbTask = new Task(DbTask, TaskCreationOptions.LongRunning);
                //dbTask.Start();
                Thread t = new Thread(DbTask);
                t.Name = "DB";
                t.Start();
            }
            // GameLogicTask
            {
                //Task gameLogicTask = new Task(GameLogicTask, TaskCreationOptions.LongRunning);
                //gameLogicTask.Start();
                Thread.CurrentThread.Name = "GameLogic";
                GameLogicTask();
            }
        }
    }
}

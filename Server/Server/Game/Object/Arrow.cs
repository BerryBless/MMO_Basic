using System;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using System.Text;

namespace Server.Game
{
    class Arrow : Projectile
    {
        public GameObject Owner { get; set; }

        long _nextMoveTick = 0; // 틱카운터

        public override void Update() {
            // 유효성 검사
            if (Data == null || Data.projectile == null || Owner == null || Room == null) return;

            if (_nextMoveTick >= Environment.TickCount64) {
                return;
            }

            long tick = (long)(1000 / Data.projectile.speed); // 1초에 speed칸만큼 움직임
            _nextMoveTick = Environment.TickCount64 + tick;

            // TODO 앞으로 나가기 / 뿌려주기
            Vector2Int destPos = GetFrontCellPos();
            if (Room.Map.CanGo(destPos) == true)
            {
                CellPos = destPos;

                S_Move movePacket = new S_Move();
                movePacket.ObjectId = Id;
                movePacket.PosInfo = PosInfo;
                Room.Broadcast(movePacket);

                //Console.WriteLine("Move Arrow");
            }
            else
            {
                GameObject target = Room.Map.Find(destPos);
                if (target != null)
                {
                    // 피격판정
                    Console.WriteLine($"{target.Info.Name}  Damaged : {Data.damage}");
                    target.OnDamaged(this, Data.damage + Owner.Stat.Attack);
                }
                // 소멸
                //Room.LeaveGame(Id);
                Room.Push(Room.LeaveGame, Id);
            }

        }

    }
}

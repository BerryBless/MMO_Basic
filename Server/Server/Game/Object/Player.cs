using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class Player : GameObject
    {
        public int PlayerDbId { get; set; }

        public ClientSession Session { get; set; }

        public Player()
        {
            ObjectType = GameObjectType.Player;
            Speed = 30f; //TODO 데이터 시트로 바꾸기
        }

        public override void OnDamaged(GameObject attecker, int damage)
        {
            base.OnDamaged(attecker, damage);
        }
        public void OnLeaveGame()
        {
            DbTransaction.SavePlayerStatus_AllInOne(this, Room);
        }
    }
}

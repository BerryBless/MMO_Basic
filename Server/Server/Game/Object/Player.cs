using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class Player : GameObject
    {
        public ClientSession Session { get; set; }

        public Player()
        {
            ObjectType = GameObjectType.Player;
            Speed = 30f; //TEMP 데이터 시트로 바꾸기
        }

        public override void OnDamaged(GameObject attecker, int damage)
        {
            base.OnDamaged(attecker, damage);
        }
    }
}

using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public partial class GameRoom : JobSerializer
    {

        // C_EquipItem
        public void HandleEquipItem(Player player, C_EquipItem equipPacket)
        {
            if (player == null) return;

            // 아이템 있나 체크
            Item item = player.Inven.Get(equipPacket.ItemDbId);
            if (item == null) return;


            // 메모리 선적용
            item.Equipped = equipPacket.Equipped;

            // DB에 Noti
            DbTransaction.EquipItemNoti(player, item);

            // 클라에 통보
            S_EquipItem equipOkItem = new S_EquipItem();
            equipOkItem.ItemDbId = equipPacket.ItemDbId;
            equipOkItem.Equipped = equipPacket.Equipped;
            player.Session.Send(equipOkItem);
        }
    }
}

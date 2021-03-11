using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.DB;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    public partial class ClientSession : PacketSession
    {
        public void HandleLogin(C_Login loginPacket)
        {
            // TODO 보안체크 더 강하게
            if (ServerState != PlayerServerState.ServerStateLogin) return;
                 
            // TODO 각종 상황에 대비
            // -동시?
            // -악질 패킷
            // -이상한 타이밍
            using (AppDbContext db = new AppDbContext())
            {
                AccountDb findAccount = db.Accounts
                    .Include(a => a.Players)
                    .Where(a => a.AccountName == loginPacket.UniqueId)
                    .FirstOrDefault();

                if (findAccount != null)
                { // 로그인 성공
                    S_Login loginOk = new S_Login() { LoginOk = 1 };
                    this.Send(loginOk);
                }
                else
                { // 실패
                  // TEMP_ 새계정으로
                    AccountDb newAccount = new AccountDb() { AccountName = loginPacket.UniqueId };
                    db.Accounts.Add(newAccount);
                    db.SaveChanges();   // TODO : Exception

                    // 로그인 성공
                    S_Login loginOk = new S_Login() { LoginOk = 1 };
                    this.Send(loginOk);
                }
            }
        }
    }
}

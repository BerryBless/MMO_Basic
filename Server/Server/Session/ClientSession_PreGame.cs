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
        public int AccountDbId { get; private set; }
        public List<LobbyPlayerInfo> LobbyPlayers { get; set; } = new List<LobbyPlayerInfo>();

        public void HandleLogin(C_Login loginPacket)
        {
            // TODO 보안체크 더 강하게
            if (ServerState != PlayerServerState.ServerStateLogin) return;

            // TODO 각종 상황에 대비
            // -동시?
            // -악질 패킷
            // -이상한 타이밍
            LobbyPlayers.Clear();
            using (AppDbContext db = new AppDbContext())
            {
                AccountDb findAccount = db.Accounts
                    .Include(a => a.Players)
                    .Where(a => a.AccountName == loginPacket.UniqueId)
                    .FirstOrDefault();

                if (findAccount != null)
                { // 로그인 성공
                    AccountDbId = findAccount.AccountId;        // Id는 자주쓰니 기억
                    S_Login loginOk = new S_Login() { LoginOk = 1 };
                    foreach(PlayerDb playerDb in findAccount.Players)
                    {
                        LobbyPlayerInfo lobbyPlayer = new LobbyPlayerInfo() {
                            Name = playerDb.PlayerName,
                            StatInfo = new StatInfo()
                            {
                                Level = playerDb.Level,
                                Hp = playerDb.Hp,
                                MaxHp = playerDb.MaxHp,
                                Attack = playerDb.Attack,
                                Speed = playerDb.Speed,
                                TotalExp = playerDb.TotalExp
                            }
                        };

                        // 메모리에 들고있는다 DB접근 최소화
                        LobbyPlayers.Add(lobbyPlayer);
                        // 패킷을 넣어준다
                        loginOk.Players.Add(lobbyPlayer);

                    }
                    this.Send(loginOk);

                    // 로비로 이동
                    ServerState = PlayerServerState.ServerStateLobby;
                }
                else
                { // 실패
                  // TEMP_ 새계정으로
                    AccountDb newAccount = new AccountDb() { AccountName = loginPacket.UniqueId };
                    db.Accounts.Add(newAccount);
                    db.SaveChanges();   // TODO : Exception

                    AccountDbId = findAccount.AccountId;        // Id는 자주쓰니 기억

                    // 로그인 실패
                    S_Login loginOk = new S_Login() { LoginOk = 1 };
                    this.Send(loginOk);

                    // 로비로 이동
                    ServerState = PlayerServerState.ServerStateLobby;
                }
            }
        }
    }
}

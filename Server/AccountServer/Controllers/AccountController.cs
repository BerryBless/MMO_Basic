using AccountServer.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // 계정 생성
        [HttpPost]
        [Route("create")]
        public CreateAccountPacketRes CreateAccount([FromBody] CreateAccountPacketReq req)
        {
            CreateAccountPacketRes res = new CreateAccountPacketRes();

            // ID 겹치는지 확인
            AccountDb account = _context.Accounts
                                    .AsNoTracking()
                                    .Where(a => a.AccountName == req.AccountName)
                                    .FirstOrDefault();

            if (account == null)
            {
                // 가능

                _context.Accounts.Add(new AccountDb()
                {
                    AccountName = req.AccountName,
                    Password = req.Password // TODO Hash로 해주기
                });

                bool success = _context.SaveChangesEx();
                res.CreateOk = success;
            }
            else
            {
                // 불가능
                res.CreateOk = false;
            }
            return res;
        }
        // 로그인
        [HttpPost]
        [Route("login")]
        public LoginAccountPacketRes LoginAccount([FromBody] LoginAccountPacketReq req)
        {
            LoginAccountPacketRes res = new LoginAccountPacketRes();

            AccountDb account = _context.Accounts
                                    .AsNoTracking()
                                    .Where(a => a.AccountName == req.AccountName
                                        && a.Password == req.Password)
                                    .FirstOrDefault();


            if(account == null)
            {
                // 문제있음
                res.LoginOk = false;
            }
            else
            {
                // 로그인
                res.LoginOk = true;

                // TODO 서버목록
                res.ServerList = new List<ServerInfo>()
                {
                    new ServerInfo () {Name = "칼페온", Ip ="127.0.0.1", CrowdedLevel = 1000 },
                    new ServerInfo () {Name = "카마실비아", Ip ="127.0.0.1", CrowdedLevel = 50 },
                    new ServerInfo () {Name = "발레리아", Ip ="127.0.0.1", CrowdedLevel = 1 },
                };
            }

            return res;
        }
    }
}

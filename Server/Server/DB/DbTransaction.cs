using System;
using System.Collections.Generic;
using System.Text;
using Server;
using Microsoft.EntityFrameworkCore;
using Server.Game;

namespace Server.DB
{
    public class DbTransaction : JobSerializer
    {
        public static DbTransaction Instance { get; } = new DbTransaction();

		// Me (GameRoom) -> You (Db) -> Me (GameRoom)
		public static void SavePlayerStatus_AllInOne(Player player, GameRoom room)
		{
			if (player == null || room == null)
				return;

			// Me (GameRoom)
			PlayerDb playerDb = new PlayerDb();
			playerDb.PlayerDbId = player.PlayerDbId;
			playerDb.Hp = player.Stat.Hp;

			// You
			Instance.Push(() =>
			{
				using (AppDbContext db = new AppDbContext())
				{
					db.Entry(playerDb).State = EntityState.Unchanged;
					db.Entry(playerDb).Property(nameof(PlayerDb.Hp)).IsModified = true;
					bool success = db.SaveChangesEx();
					if (success)
					{
						// Me
						room.Push(() => Console.WriteLine($"Hp Saved({playerDb.Hp})"));
					}
				}
			});
		}
		// Me (GameRoom) -> You (Db) -> Me (GameRoom)
		public static void SavePlayerStatus_Step1(Player player, GameRoom room)
		{
			if (player == null || room == null)
				return;

			// Me (GameRoom)
			PlayerDb playerDb = new PlayerDb();
			playerDb.PlayerDbId = player.PlayerDbId;
			playerDb.Hp = player.Stat.Hp;

			
			Instance.Push<PlayerDb,GameRoom>(SavePlayerStatus_Step2,playerDb,room);
		}
		// You
		public static void SavePlayerStatus_Step2(PlayerDb playerDb, GameRoom room)
        {
			using (AppDbContext db = new AppDbContext())
			{
				db.Entry(playerDb).State = EntityState.Unchanged;
				db.Entry(playerDb).Property(nameof(PlayerDb.Hp)).IsModified = true;
				bool success = db.SaveChangesEx();
				if (success)
				{
					
					room.Push<int>(SavePlayerStatus_Step3, playerDb.Hp);
				}
			}
		}
			// Me
		public static void SavePlayerStatus_Step3(int Hp)
		{
			Console.WriteLine($"Hp Saved({Hp})");

		}

	}
}

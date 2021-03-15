using System;
using System.Collections.Generic;
using System.Text;
using Server.DB;

namespace Server
{
    public static class Extensions
    {
        internal static bool SaveChangesEx(this AppDbContext db)
        {
            try
            {
                db.SaveChanges();
                return true;
            }
            catch
            {
                // SaveChanges 실패했을때 예외
                return false;
            }
        }
    }
}

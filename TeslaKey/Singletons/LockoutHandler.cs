using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeslaKey.Singletons
{
    public interface ILockoutHandler
    {
        bool LockedOut(string email);
    }
    public class LockoutHandler : ILockoutHandler
    {
        private Dictionary<string, List<DateTime>> requests = new Dictionary<string, List<DateTime>>();
        private object lockobj = new object();

        bool ILockoutHandler.LockedOut(string key)
        {
            lock (lockobj)
            {
                if (!requests.TryGetValue(key, out List<DateTime> l))
                {
                    l = new List<DateTime>();
                    requests[key] = l;
                }
                var tt = DateTime.Now.AddMinutes(-5);
                for (int nit = 0; nit < l.Count;)
                {
                    if (l[nit] < tt)
                    {
                        l.RemoveAt(nit);
                    }
                    else
                    {
                        nit++;
                    }
                }
                if (l.Count >= 5)
                {
                    return true;
                }
                l.Add(DateTime.Now);
                return false;
            }
        }
    }
}

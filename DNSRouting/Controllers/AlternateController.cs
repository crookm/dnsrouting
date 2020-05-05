using System;
using Microsoft.AspNetCore.Mvc;

namespace DNSRouting.Controllers
{
  public class AlternateController : Controller
    {
        // GET: /<controller>/
        public string Index(string Host) // Host is set in CustomRouter
        {
            return String.Format("Alternate page, host = {0}", Host);
        }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace DNSRouting.Controllers
{
  public class HomeController : Controller
    {
        public string Index()
        {
            return "Your regular home page.";
        }
    }
}

//using Microsoft.AspNetCore.Mvc;

//namespace PresentationMVC.Controllers;

//public class HomeController : Controller
//{
//    public IActionResult Index()
//    {
//        if (User.Identity?.IsAuthenticated == true)
//            return RedirectToAction("Index", "Rooms");

//        return RedirectToAction("Login", "Auth");
//    }
//}

using Microsoft.AspNetCore.Mvc;

namespace PresentationMVC.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
}
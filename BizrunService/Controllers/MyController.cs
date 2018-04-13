using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace BizrunService.Controllers
{
    class myClass
    {
        public string name { get; set; }
        public int age { get; set; }
    }

    [Route("api/[controller]")]
    public class MyController : Controller
    {
        
        List<myClass> list = new List<myClass>();
        // GET: /<controller>/
        public IActionResult Index()
        {

            list.Add(new myClass() { name = "Razi", age = 25 });
            list.Add(new myClass() { name = "yash", age = 22 });
            return new JsonResult(list);
        }
    }
}

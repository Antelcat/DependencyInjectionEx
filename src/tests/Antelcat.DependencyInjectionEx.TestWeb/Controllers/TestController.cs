using Antelcat.DependencyInjectionEx.Autowired;
using Microsoft.AspNetCore.Mvc;

namespace Antelcat.DependencyInjectionEx.TestWeb.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class TestController : ControllerBase
{
    public TestController()
    {
        
    }
    [Autowired(typeof(IB))]
    public required B B { get; set; }
    
    [HttpGet]
    public IActionResult Index()
    {
        return Ok((object?) B ?? new { });
    } 
}
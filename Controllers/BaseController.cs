using Microsoft.AspNetCore.Mvc;

namespace PersonalManagerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseController : ControllerBase
    {
    }
}
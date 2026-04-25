using Microsoft.AspNetCore.Mvc;

namespace Kesa.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class ApiV1ControllerBase : ControllerBase
{
}

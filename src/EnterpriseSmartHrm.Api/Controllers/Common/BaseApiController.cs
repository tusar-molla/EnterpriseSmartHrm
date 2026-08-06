using EnterpriseSmartHrm.Application.Common.Models;
using EnterpriseSmartHrm.Application.Contracts.Common;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseSmartHrm.Api.Controllers.Common;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected IActionResult FromResult(Result result)
    {
        var traceId = HttpContext.TraceIdentifier;

        if (result.IsSuccess)
        {
            return Ok(ApiResponse<object>.Ok(null, result.Message, traceId));
        }

        return ToErrorResponse<object>(result, traceId);
    }

    protected IActionResult FromResult<T>(Result<T> result)
    {
        var traceId = HttpContext.TraceIdentifier;

        if (result.IsSuccess)
        {
            return Ok(ApiResponse<T>.Ok(result.Value, result.Message, traceId));
        }

        return ToErrorResponse<T>(result, traceId);
    }

    private IActionResult ToErrorResponse<T>(Result result, string traceId)
    {
        var response = ApiResponse<T>.Fail(result.Message, result.Errors, traceId);

        return result.Status switch
        {
            ResultStatus.ValidationError => BadRequest(response),
            ResultStatus.NotFound => NotFound(response),
            ResultStatus.Unauthorized => Unauthorized(response),
            ResultStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, response),
            ResultStatus.Conflict => Conflict(response),
            _ => BadRequest(response)
        };
    }
}

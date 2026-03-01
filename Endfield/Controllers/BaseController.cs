using Endfield.Api.Share.Enums;
using Endfield.Api.Share.Models;
using Microsoft.AspNetCore.Mvc;

namespace Endfield.Api.Controllers;

/// <summary>
/// 控制器基类
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// 返回统一格式响应
    /// </summary>
    protected ActionResult<string> ReturnInfo(bool success, ReturnDataCode code, string? message, object? data)
        => ReturnDataResponse.ReturnInfo(success, code, message, data);

    /// <summary>
    /// 成功响应
    /// </summary>
    protected ActionResult<string> ReturnSuccess(object? data, string? message = null)
        => ReturnDataResponse.ReturnSuccess(data, message);

    /// <summary>
    /// 失败响应
    /// </summary>
    protected ActionResult<string> ReturnFail(string message, ReturnDataCode code = ReturnDataCode.BusinessError)
        => ReturnDataResponse.ReturnFail(message, code);
}

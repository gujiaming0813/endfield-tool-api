using Endfield.Api.Share.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Endfield.Api.Share.Models;

/// <summary>
/// 统一响应格式
/// </summary>
/// <param name="Success">是否成功</param>
/// <param name="Message">消息</param>
/// <param name="Data">数据</param>
/// <param name="Code">状态码</param>
public record ReturnDataResponse(bool Success = true, string? Message = null, object? Data = null, ReturnDataCode Code = ReturnDataCode.Success)
{
    /// <summary>
    /// 返回统一格式响应
    /// </summary>
    public static ActionResult<string> ReturnInfo(bool success, ReturnDataCode code, string? message, object? data)
        => new JsonResult(new ReturnDataResponse(success, message, data, code));

    /// <summary>
    /// 成功响应
    /// </summary>
    public static ActionResult<string> ReturnSuccess(object? data, string? message = null)
        => ReturnInfo(true, ReturnDataCode.Success, message, data);

    /// <summary>
    /// 失败响应
    /// </summary>
    public static ActionResult<string> ReturnFail(string message, ReturnDataCode code = ReturnDataCode.BusinessError)
        => ReturnInfo(false, code, message, null);
}

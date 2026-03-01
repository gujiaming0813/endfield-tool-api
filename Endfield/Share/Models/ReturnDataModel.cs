using Endfield.Api.Share.Enums;

namespace Endfield.Api.Share.Models;

/// <summary>
/// 通用返回数据模型
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
/// <param name="Success">是否成功</param>
/// <param name="Code">状态码</param>
/// <param name="Message">消息</param>
/// <param name="Data">数据</param>
public record ReturnDataModel<T>(bool Success, ReturnDataCode Code, string? Message, T? Data)
{
    /// <summary>
    /// 创建成功响应
    /// </summary>
    public static ReturnDataModel<T> SuccessResult(T data, string? message = null)
        => new(true, ReturnDataCode.Success, message, data);

    /// <summary>
    /// 创建失败响应
    /// </summary>
    public static ReturnDataModel<T> FailResult(string message, ReturnDataCode code = ReturnDataCode.BusinessError)
        => new(false, code, message, default);
}

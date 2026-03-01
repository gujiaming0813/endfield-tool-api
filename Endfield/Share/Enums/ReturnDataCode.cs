namespace Endfield.Api.Share.Enums;

/// <summary>
/// 返回数据状态码
/// </summary>
public enum ReturnDataCode
{
    /// <summary>
    /// 成功
    /// </summary>
    Success = 200,

    /// <summary>
    /// 参数错误
    /// </summary>
    BadRequest = 400,

    /// <summary>
    /// 未授权
    /// </summary>
    Unauthorized = 401,

    /// <summary>
    /// 禁止访问
    /// </summary>
    Forbidden = 403,

    /// <summary>
    /// 资源不存在
    /// </summary>
    NotFound = 404,

    /// <summary>
    /// 业务错误
    /// </summary>
    BusinessError = 500
}

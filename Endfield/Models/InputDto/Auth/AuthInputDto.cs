using System.ComponentModel.DataAnnotations;

namespace Endfield.Api.Models.InputDto.Auth;

/// <summary>
/// 登录输入参数
/// </summary>
public record LoginInputDto
{
    /// <summary>
    /// 用户名
    /// </summary>
    [Required(ErrorMessage = "用户名不能为空")]
    public required string Username { get; init; }

    /// <summary>
    /// 密码
    /// </summary>
    [Required(ErrorMessage = "密码不能为空")]
    public required string Password { get; init; }
}

/// <summary>
/// 注册输入参数
/// </summary>
public record RegisterInputDto
{
    /// <summary>
    /// 用户名
    /// </summary>
    [Required(ErrorMessage = "用户名不能为空")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "用户名长度需在3-50个字符之间")]
    public required string Username { get; init; }

    /// <summary>
    /// 密码
    /// </summary>
    [Required(ErrorMessage = "密码不能为空")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "密码长度需在6-100个字符之间")]
    public required string Password { get; init; }

    /// <summary>
    /// 昵称
    /// </summary>
    [StringLength(50, ErrorMessage = "昵称长度不能超过50个字符")]
    public string? Nickname { get; init; }

    /// <summary>
    /// 邮箱
    /// </summary>
    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    public string? Email { get; init; }
}

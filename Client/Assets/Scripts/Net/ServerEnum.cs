/// <summary>
/// 注册类型
/// </summary>
public enum RegisterType
{
    ///手机注册
    Phone,
    ///邮箱注册
    Mail,
}

/// <summary>
/// 注册结果
/// </summary>
public enum RegisterResult
{
    ///成功
    Success,
    ///注册失败
    Failed,
    ///该帐户已存在
    AlreadyExist,
    ///验证码错误
    WrongCode,
    ///禁止注册和禁止登录（被封）
    Forbidden,
}

/// <summary>
/// 登录类型
/// </summary>
public enum LoginType
{
    Phone,
    Mail,
    WX,
    QQ,
    Token,
}

public enum LoginResult
{
    ///登录成功
    Success,
    ///登录错误
    Failed,
    ///密码错误
    WrongPwd,
    ///用户不存在
    UserNotExist,
    ///Token超时
    TimeoutToken,
}

using MySql;
using MySql.SQLData;
using ServerBase;
using System;
namespace SimpleServer.Business
{
    public class UserManager : Singleton<UserManager>
    {
        /// <summary>
        /// 注册用户 （正常情况下还要包括检测验证码是否正确）
        /// </summary>
        /// <param name="registerType"></param>
        /// <param name="userName"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public RegisterResult Register(RegisterType registerType, string userName, string pwd)
        {
            try
            {
                int count = MySqlMgr.Instance.SqlSugarDB.Queryable<User>().Where(it => it.Username == userName).Count();
                if (count > 0)
                {
                    return RegisterResult.AlreadyExist;
                }
                User user = new User();
                switch (registerType)
                {
                    //手机或邮箱注册，在注册前会有一个协议申请验证码，申请的验证码生成后在数据库储存一份，然后注册的时候把客户端传入的验证码和数据库的验证码比较，如果不一致，注册错误返回RegisterResult.WrongCode
                    case RegisterType.Phone:
                        user.LoginType = LoginType.Phone.ToString();
                        break;
                    case RegisterType.Mail:
                        user.LoginType = LoginType.Mail.ToString();
                        break;
                }
                user.Username = userName;
                user.Password = pwd;
                user.LoginDate = DateTime.Now;
                //数据插入
                MySqlMgr.Instance.SqlSugarDB.Insertable(user).ExecuteCommand();
                return RegisterResult.Success;
            }
            catch (Exception e)
            {
                Debug.LogError("注册失败：" + e);
                return RegisterResult.Failed;
            }
        }

        public LoginResult Login(LoginType loginType, string userName, string pwd, out int userId,out string token)
        {
            userId = 0;
            token = "";
            try
            {
                User user = null;
                switch (loginType)
                {
                    case LoginType.Phone:
                    case LoginType.Mail:
                        user = MySqlMgr.Instance.SqlSugarDB.Queryable<User>().Where(it => it.Username == userName).Single();
                        break;
                    //如果是QQ或微信，再User里面要多存一个UnionId，判断时候是it => it.再User里面要多存一个UnionId == userName
                    case LoginType.WX:
                    case LoginType.QQ:
                        break;
                    case LoginType.Token:
                        user = MySqlMgr.Instance.SqlSugarDB.Queryable<User>().Where(it => it.Username == userName).Single();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(loginType), loginType, "登录类型错误");
                }

                if (user == null)
                {
                    //QQ和微信首次登录时，相当于注册
                    if (loginType == LoginType.QQ || loginType == LoginType.WX)
                    {
                        //在数据库注册
                        return LoginResult.Success;
                    }
                    else
                    {
                        return LoginResult.UserNotExist;
                    }
                }
                else
                {
                    if (loginType != LoginType.Token)
                    {
                        if (loginType == LoginType.Phone)
                        {
                            if (user.Password != pwd)
                                return LoginResult.WrongPwd;
                        }else if (loginType == LoginType.Mail)
                        {
                            if (user.Password != pwd)
                                return LoginResult.WrongPwd;
                        }
                    }
                    else
                    {
                        if (user.Token != pwd)
                        {
                            return LoginResult.TimeoutToken;
                        }
                    }
                    user.Token = Guid.NewGuid().ToString();
                    user.LoginDate = DateTime.Now;
                    token = user.Token;
                    MySqlMgr.Instance.SqlSugarDB.Updateable(user).ExecuteCommand();
                    userId = user.Id;
                    return LoginResult.Success;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("登录失败: {0}",e.ToString());
                return LoginResult.Failed;
            }
        }
    }
}
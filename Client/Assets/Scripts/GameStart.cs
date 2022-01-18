using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    private NetManager mNetManager = NetManager.Instance;
    void Start()
    {
        mNetManager.Connect("172.12.12.30", 8011);
    }

    void Update()
    {
        mNetManager.Update();
        if (Input.GetKeyDown(KeyCode.A))
        {
            ProtocolMgr.SocketTest();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            ProtocolMgr.Register(RegisterType.Phone,"13534489765","Ocean", "123456",(res) =>
            {
                switch (res)
                {
                    case RegisterResult.AlreadyExist:
                        Debug.LogError("该手机号已经注册过了");
                        break;
                    case RegisterResult.WrongCode:
                        Debug.LogError("验证吗错误");
                        break;
                    case RegisterResult.Forbidden:
                        Debug.LogError("改账号被封");
                        break;
                    case RegisterResult.Success:
                        Debug.LogError("注册成功");
                        break;
                    case RegisterResult.Failed:
                        Debug.LogError("注册失败");
                        break;
                }
            });
        }
        
        if (Input.GetKeyDown(KeyCode.D))
        {
            ProtocolMgr.Login(LoginType.Phone,"13534489765","Ocean", (res, resToken) =>
            {
                switch (res)
                {
                    case LoginResult.Success:
                        Debug.LogError("登录成功");
                        break;
                    case LoginResult.Failed:
                        Debug.LogError("登录失败");
                        break;
                    case LoginResult.WrongPwd:
                        Debug.LogError("密码错误");
                        break;
                    case LoginResult.TimeoutToken:
                        Debug.LogError("Token失效");
                        break;
                    case LoginResult.UserNotExist:
                        Debug.LogError("用户不存在");
                        break;
                }
            });
        }
    }

    private void OnApplicationQuit()
    {
        mNetManager.Close();
    }
}

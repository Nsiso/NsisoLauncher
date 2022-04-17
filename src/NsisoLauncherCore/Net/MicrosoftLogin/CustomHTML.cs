using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace NsisoLauncherCore.Net.MicrosoftLogin
{
    public static class CustomHTML
    {
        public static SystemWebViewOptions GetCustomHTML()
        {
            return new SystemWebViewOptions
            {
                HtmlMessageSuccess =
                @"<html style='font-family: sans-serif;'>
                    <head>
                        <title>微软验证成功</title>
                        <meta charset='utf-8'>
                    </head>
                    <body style='text-align: center;'>
                        <header>
                            <h1>Nsiso启动器微软验证</h1>
                        </header>
                        <main style='border: 1px solid lightgrey; margin: auto; width: 600px; padding-bottom: 15px;'>
                            <h2 style='color: limegreen;'>验证成功</h2>
                            <div>您现在可以返回Nsiso启动器继续完成您的登录。您可以自由关闭此页面。</div>
                        </main>

                    </body>
                </html>",

                HtmlMessageError =
                @"<html style='font-family: sans-serif;'>
                    <head>
                        <title>微软验证失败</title>
                        <meta charset='utf-8'>
                    </head>
                    <body style='text-align: center;'>
                        <header>
                            <h1>Nsiso启动器微软验证</h1>
                        </header>
                        <main style='border: 1px solid lightgrey; margin: auto; width: 600px; padding-bottom: 15px;'>
                            <h2 style='color: salmon;'>验证失败</h2>
                            <div><b>错误详细信息:</b> 错误： {0} 错误描述： {1}</div>
                            <br>
                            <div>您现在可以返回Nsiso启动器继续完成您的登录。您可以自由关闭此页面。</div>
                        </main>
                   </body>
                </html>"
            };
        }
    }
}

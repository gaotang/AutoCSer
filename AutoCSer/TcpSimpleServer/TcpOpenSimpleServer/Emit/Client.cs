﻿using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using AutoCSer.Net.TcpSimpleServer.Emit;

namespace AutoCSer.Net.TcpOpenSimpleServer.Emit
{
    /// <summary>
    /// TCP 客户端
    /// </summary>
    internal static class Client
    {
        /// <summary>
        /// TCP 客户端元数据
        /// </summary>
        internal static readonly ClientMetadata Metadata = new ClientMetadata(typeof(TcpOpenSimpleServer.Client), typeof(MethodClient));
    }
    /// <summary>
    /// TCP 客户端
    /// </summary>
    /// <typeparam name="interfaceType">接口类型</typeparam>
    [AutoCSer.IOS.Preserve(AllMembers = false)]
    public static class Client<interfaceType>
    {
        /// <summary>
        /// 默认 TCP 服务配置
        /// </summary>
        private static readonly ServerAttribute defaultServerAttribute;
        /// <summary>
        /// 错误字符串提示信息
        /// </summary>
        private static readonly string errorString;
        /// <summary>
        /// TCP 客户端类型
        /// </summary>
        private static readonly Type clientType;
        /// <summary>
        /// 客户端命令信息集合
        /// </summary>
        private static readonly TcpServer.CommandInfoBase[] commands;
        /// <summary>
        /// 获取客户端命令信息
        /// </summary>
        /// <param name="index"></param>
        /// <returns>客户端命令信息</returns>
        [MethodImpl(AutoCSer.MethodImpl.AggressiveInlining)]
        [AutoCSer.IOS.Preserve]
        public static TcpServer.CommandInfoBase GetCommand(int index)
        {
            return commands[index];
        }
        /// <summary>
        /// 创建 TCP 客户端
        /// </summary>
        /// <param name="attribute">TCP 调用服务器端配置信息</param>
        /// <param name="verifyMethod">TCP 验证方法</param>
        /// <param name="log">日志接口</param>
        /// <returns>TCP 客户端</returns>
        public static interfaceType Create(ServerAttribute attribute = null, Func<interfaceType, bool> verifyMethod = null, AutoCSer.Log.ILog log = null)
        {
            if (errorString != null) throw new Exception(errorString);
            if (clientType == null) throw new InvalidCastException();
            MethodClient client = (MethodClient)Activator.CreateInstance(clientType);
            interfaceType interfaceClient = (interfaceType)(object)client;
            if (attribute == null) attribute = defaultServerAttribute;
            client._TcpClient_ = new AutoCSer.Net.TcpOpenSimpleServer.Client<interfaceType>(interfaceClient, attribute, log, verifyMethod);
            if (attribute.IsAutoClient) client._TcpClient_.TryCreateSocket();
            return interfaceClient;
        }

        static Client()
        {
            Type type = typeof(interfaceType);
            MethodGetter builder = new MethodGetter { IsClient = true };
            bool isMethod = builder.Get(type);
            errorString = builder.ErrorString;
            if (isMethod)
            {
                defaultServerAttribute = builder.DefaultServerAttribute;

                Method<ServerAttribute, MethodAttribute, ServerSocket>.ClientBuilder clientBuilder = new Method<ServerAttribute, MethodAttribute, ServerSocket>.ClientBuilder { Metadata = Client.Metadata };
                clientType = clientBuilder.Build(type, defaultServerAttribute, builder.Methods, typeof(Client<interfaceType>).GetMethod("GetCommand", BindingFlags.Static | BindingFlags.Public));
                commands = clientBuilder.Commands;
            }
        }
    }
}

﻿using System;

namespace AutoCSer.Example.Xml
{
    /// <summary>
    /// 入侵派生类型 示例
    /// </summary>
    [AutoCSer.Xml.Serialize(IsBaseType = true)]
    class BaseType
    {
        /// <summary>
        /// 基类数据字段
        /// </summary>
        public int Value;

        /// <summary>
        /// 入侵派生类型 测试
        /// </summary>
        /// <returns></returns>
        [AutoCSer.Metadata.TestMethod]
        internal static bool TestCase()
        {
            SonType value = new SonType { Value = 1, SonValue = 2 };
            string xml = AutoCSer.Xml.Serializer.Serialize(value);

            SonType newValue = AutoCSer.Xml.Parser.Parse<SonType>(xml);
            if (newValue == null || newValue.Value != 1 || newValue.SonValue != 0)
            {
                return false;
            }

            SonType2 newValue2 = AutoCSer.Xml.Parser.Parse<SonType2>(xml);
            return newValue2 != null && newValue2.Value == 1 && newValue2.SonValue == 0;
        }
    }
    /// <summary>
    /// 派生类型
    /// </summary>
    class SonType : BaseType
    {
        /// <summary>
        /// 派生类型数据字段
        /// </summary>
        public int SonValue;
    }
    /// <summary>
    /// 派生类型
    /// </summary>
    class SonType2 : BaseType
    {
        /// <summary>
        /// 派生类型数据字段
        /// </summary>
        public int SonValue;
    }
}

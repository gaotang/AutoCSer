﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace AutoCSer.Tool.OpenPack
{
    class Program
    {
        /// <summary>
        /// ZIP 文件名称
        /// </summary>
        private const string zipFileName = "AutoCSer.Example.zip";
        /// <summary>
        /// 第三方 dll 文件名称集合
        /// </summary>
        private static readonly HashSet<string> thirdPartyFileNames = new HashSet<string>();
        /// <summary>
        /// ZIP 压缩包
        /// </summary>
        private static ZipArchive zipArchive;
        static void Main(string[] args)
        {
            string zipPath = new DirectoryInfo(@"..\..\..\..\Web\www.AutoCSer.com\Download\").FullName;
            DirectoryInfo githubDirectory = new DirectoryInfo(@"..\..\..\..\Github\AutoCSer\");
            if (!githubDirectory.Exists) githubDirectory.Create();
            //if (File.Exists(zipPath + zipFileName)) File.Delete(zipPath + zipFileName);
            foreach (FileInfo file in new DirectoryInfo(@"..\..\..\..\ThirdParty\").GetFiles()) thirdPartyFileNames.Add(file.Name.ToLower());
            //using (MemoryStream stream = new MemoryStream())
            //{
            //    using (zipArchive = new ZipArchive(stream, ZipArchiveMode.Create, true)) boot(new DirectoryInfo(@"..\..\..\..\"), githubDirectory.FullName);
            //    using (FileStream packFile = new FileStream(zipPath + zipFileName, FileMode.Create)) packFile.Write(stream.GetBuffer(), 0, (int)stream.Position);
            //}
            using (FileStream stream = new FileStream(zipPath + zipFileName, FileMode.Create))
            using (zipArchive = new ZipArchive(stream, ZipArchiveMode.Create, true)) 
            {
                boot(new DirectoryInfo(@"..\..\..\..\"), githubDirectory.FullName);
            }
            Console.WriteLine(zipPath + zipFileName);
            Console.ReadKey();
        }
        /// <summary>
        /// 根目录处理
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="githubPath"></param>
        private static void boot(DirectoryInfo directory, string githubPath)
        {
            foreach (FileInfo file in directory.GetFiles())
            {
                string fileName = file.Name;
                if (fileName.IndexOf(".example", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    using (Stream entryStream = zipArchive.CreateEntry(fileName).Open()) githubFile(file, entryStream, githubPath);
                }
            }
            foreach (DirectoryInfo nextDircectory in directory.GetDirectories())
            {
                switch (nextDircectory.Name.ToLower())
                {
                    case ".vs": vs(nextDircectory, githubPath); break;
                    case "autocser": AutoCSer(nextDircectory, githubPath); break;
                    case "packet": case "thirdparty": case "example": case "testcase": case "web": copy(nextDircectory, nextDircectory.Name + @"\", githubPath); break;
                }
            }
        }
        /// <summary>
        /// github 文件处理
        /// </summary>
        /// <param name="file"></param>
        /// <param name="entryStream"></param>
        /// <param name="githubPath"></param>
        private unsafe static void githubFile(FileInfo file, Stream entryStream, string githubPath)
        {
            githubFile(file.Name, File.ReadAllBytes(file.FullName), entryStream, githubPath);
        }
        /// <summary>
        /// github 文件处理
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        /// <param name="entryStream"></param>
        /// <param name="githubPath"></param>
        private unsafe static void githubFile(string fileName, byte[] data, Stream entryStream, string githubPath)
        {
            entryStream.Write(data, 0, data.Length);
            FileInfo githubFile = new FileInfo(githubPath + fileName);
            bool isFile = true;
            if (githubFile.Exists && githubFile.Length == data.Length)
            {
                byte[] githubData = File.ReadAllBytes(githubFile.FullName);
                isFile = false;
                fixed (byte* dataFixed = data, githubDataFixed = githubData)
                {
                    byte* start = dataFixed, end = dataFixed + (data.Length & (int.MaxValue - 7)), githubStart = githubDataFixed;
                    while (start != end)
                    {
                        if (*(ulong*)start != *(ulong*)githubStart)
                        {
                            isFile = true;
                            break;
                        }
                        start += sizeof(ulong);
                        githubStart += sizeof(ulong);
                    }
                    end += data.Length & 7;
                    while (start != end)
                    {
                        if (*start++ != *githubStart++) isFile = true;
                    }
                }
            }
            if (isFile) File.WriteAllBytes(githubFile.FullName, data);
        }
        /// <summary>
        /// github 目录处理
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="githubPath"></param>
        /// <returns></returns>
        private static string checkGithubPath(DirectoryInfo directory, string githubPath)
        {
            if (!Directory.Exists(githubPath += directory.Name + @"\")) Directory.CreateDirectory(githubPath);
            return githubPath;
        }
        /// <summary>
        /// VS 目录处理
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="githubPath"></param>
        private static void vs(DirectoryInfo directory, string githubPath)
        {
            githubPath = checkGithubPath(directory, githubPath);
            string path = directory.Name + @"\";
            foreach (DirectoryInfo nextDircectory in directory.GetDirectories())
            {
                if (nextDircectory.Name.IndexOf(".example", StringComparison.OrdinalIgnoreCase) > 0) copy(nextDircectory, path + nextDircectory.Name + @"\", githubPath);
            }
        }
        /// <summary>
        /// AutoCSer 项目目录处理
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="githubPath"></param>
        private static void AutoCSer(DirectoryInfo directory, string githubPath)
        {
            githubPath = checkGithubPath(directory, githubPath);
            string path = directory.Name + @"\";
            foreach (DirectoryInfo nextDircectory in directory.GetDirectories())
            {
                int isDircectory;
                switch (nextDircectory.Name.Length)
                {
                    case 2: isDircectory = string.Compare(nextDircectory.Name, "js", true); break;
                    case 3: isDircectory = string.Compare(nextDircectory.Name, "Sql", true); break;
                    case 5: isDircectory = string.Compare(nextDircectory.Name, "MySql", true); break;
                    case 6: isDircectory = string.Compare(nextDircectory.Name, "Deploy", true); break;
                    case 7: isDircectory = string.Compare(nextDircectory.Name, "Drawing", true); break;
                    case 8: isDircectory = string.Compare(nextDircectory.Name, "HtmlNode", true); break;
                    case 9:
                        isDircectory = string.Compare(nextDircectory.Name, "DiskBlock", true);
                        if (isDircectory != 0) isDircectory = string.Compare(nextDircectory.Name, "HtmlTitle", true);
                        break;
                    case 10: isDircectory = string.Compare(nextDircectory.Name, "Properties", true); break;
                    case 11: isDircectory = string.Compare(nextDircectory.Name, "FieldEquals", true); break;
                    case 12: isDircectory = string.Compare(nextDircectory.Name, "RandomObject", true); break;
                    case 13: isDircectory = string.Compare(nextDircectory.Name, "CodeGenerator", true); break;
                    case 15:
                        isDircectory = string.Compare(nextDircectory.Name, "TcpStreamServer", true);
                        if (isDircectory != 0) isDircectory = string.Compare(nextDircectory.Name, "TcpSimpleServer", true);
                        break;
                    case 17: isDircectory = string.Compare(nextDircectory.Name, "RawSocketListener", true); break;
                    default: isDircectory = 1; break;
                }
                if (isDircectory == 0) copy(nextDircectory, path + nextDircectory.Name + @"\", githubPath);
            }
        }
        /// <summary>
        /// 复制文件
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="path"></param>
        /// <param name="githubPath"></param>
        private static void copy(DirectoryInfo directory, string path, string githubPath)
        {
            githubPath = checkGithubPath(directory, githubPath);
            if (string.Compare(path, @"Web\www.AutoCSer.com\Download\", true) != 0)
            {
                foreach (FileInfo file in directory.GetFiles())
                {
                    string fileName = file.Name;
                    bool isDelete = false;
                    if (fileName[0] == '%') isDelete = true;
                    else
                    {
                        switch (file.Extension)
                        {
                            case ".png": fileName = null; break;
                            case ".cs":
                                if (string.Compare(fileName, "Pub.cs", true) == 0 && string.Compare(path, @"Web\Config\", true) == 0)
                                {
                                    using (Stream entryStream = zipArchive.CreateEntry(path + fileName).Open())
                                    {
                                        string code = File.ReadAllText(file.FullName).Replace(@" public static readonly bool IsLocal = false;", @" public static readonly bool IsLocal = true;");
                                        code = "000" + new Regex(@" public const string TcpVerifyString = ""([^""]+)"";").Replace(code, match => @" public const string TcpVerifyString = ""XXX"";");
                                        byte[] data = System.Text.Encoding.UTF8.GetBytes(code);
                                        data[0] = 0xef;
                                        data[1] = 0xbb;
                                        data[2] = 0xbf;
                                        githubFile(fileName, data, entryStream, githubPath);
                                    }
                                    fileName = null;
                                }
                                break;
                            case ".dll":
                                if (string.Compare(path, @"ThirdParty\", true) != 0 && thirdPartyFileNames.Contains(fileName.ToLower())) fileName = null;
                                break;
                            case ".txt":
                                if (string.Compare(fileName, "ip.txt", true) == 0) fileName = null;
                                else if (fileName.StartsWith("log_default", StringComparison.OrdinalIgnoreCase)) isDelete = true;
                                break;
                            case ".json":
                                if (fileName.EndsWith(".runtimeconfig.dev.json", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".runtimeconfig.json", StringComparison.OrdinalIgnoreCase)) fileName = null;
                                break;
                        }
                    }
                    if (isDelete)
                    {
                        file.Attributes = 0;
                        file.Delete();
                    }
                    else if (fileName != null)
                    {
                        using (Stream entryStream = zipArchive.CreateEntry(path + fileName).Open()) githubFile(file, entryStream, githubPath);
                    }
                }
                foreach (DirectoryInfo nextDircectory in directory.GetDirectories())
                {
                    switch (nextDircectory.Name.ToLower())
                    {
                        case "bin": case "obj": break;
                        default: copy(nextDircectory, path + nextDircectory.Name + @"\", githubPath); break;
                    }
                }
            }
        }
    }
}

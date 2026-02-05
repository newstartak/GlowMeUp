/*
 * 
 * 사용 예시
 * 
 * NLogManager.Debug("msg");
 * 
 */

using NLog;
using NLog.Config;
using System.IO;
using UnityEngine;
using System.Threading.Tasks;
using System;

public static class NLogManager
{
    private static NLog.Logger logger = null;

    static public async Task InitNLog()
    {
        string configPath = await StreamingWorker.GetFile("NLog.config");

        LogManager.Configuration = new XmlLoggingConfiguration(configPath);


#if UNITY_ANDROID
        LogManager.Configuration.Variables["pathVar"] = Application.persistentDataPath;
#else
        LogManager.Configuration.Variables["pathVar"] = Directory.GetCurrentDirectory();
#endif

        logger = LogManager.GetCurrentClassLogger();

        logger.Info("NLog init completed.");
    }

    static public void Debug(string msg)
    {
        if(logger == null)  return;

        logger.Debug(msg);
    }

    static public void Info(string msg)
    {
        if (logger == null) return;

        logger.Info(msg);
    }

    static public void Warn(string msg)
    {
        if (logger == null) return;

        logger.Warn(msg);
    }

    static public void Error(string msg)
    {
        if (logger == null) return;

        logger.Error(msg);
    }

    static public void Fatal(string msg)
    {
        if (logger == null) return;

        logger.Fatal(msg);
    }
}
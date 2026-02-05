using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

public static class XmlManager
{
    public class DeviceConfig
    {
        public int Index_cam;
        public int Id_device;
    }

    public class HttpConfig
    {
        public List<string> Endpoint;
    }

    public class RedisConfig
    {
        public List<string> Endpoint;
        public int Port;
        public string Password;
        public string Channel;
    }

    public class SettingConfig
    {
        public int Rotate_angle;
        public bool Flip;
        public int Timer_capture;
        public int Timer_name;
        public int Timer_complete;

        public List<string> Random_name;
    }

    public static DeviceConfig Device;
    public static HttpConfig Http;
    public static RedisConfig Redis;
    public static SettingConfig Setting;

    private static string _xmlPath;
    private static XDocument _doc;
    private static XElement _root;

    static public async Task InitXml()
    {
        _xmlPath = await StreamingWorker.GetFile("AppData.xml");

        InitConfig();
    }

    public static void InitConfig()
    {
        try
        {
            _doc = XDocument.Load(_xmlPath);
            _root = _doc.Element("Root");

            Device = new DeviceConfig
            {
                Index_cam = int.Parse(_root.Element("Device").Element("Index_cam").Value),
                Id_device = int.Parse(_root.Element("Device").Element("Id_device").Value)
            };

            Http = new HttpConfig
            {
                Endpoint = _root.Element("Http").Element("Endpoint").Elements().Select(o => o.Value).ToList()
            };

            Redis = new RedisConfig
            {
                Endpoint = _root.Element("Redis").Element("Endpoint").Elements().Select(o => o.Value).ToList(),
                Port = int.Parse(_root.Element("Redis").Element("Port").Value),
                Password = _root.Element("Redis").Element("Password").Value ?? null,
                Channel = _root.Element("Redis").Element("Channel").Value
            };

            Setting = new SettingConfig
            {
                Rotate_angle = int.Parse(_root.Element("Setting").Element("Rotate_angle").Value),
                Flip = bool.TryParse(_root.Element("Setting").Element("Flip").Value, out bool tmp) ? tmp : false,
                Timer_capture = int.Parse(_root.Element("Setting").Element("Timer_capture").Value),
                Timer_name = int.Parse(_root.Element("Setting").Element("Timer_name").Value),
                Timer_complete = int.Parse(_root.Element("Setting").Element("Timer_complete").Value),

                Random_name = _root.Element("Setting").Element("Random_name").Elements().Select(o => o.Value).ToList()
            };

            NLogManager.Info("XML init completed.");
        }
        catch (Exception ex)
        {
            NLogManager.Error($"Error occured during init xml: {ex}");

            throw;
        }
    }
}
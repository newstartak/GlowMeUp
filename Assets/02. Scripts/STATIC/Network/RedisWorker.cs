using StackExchange.Redis;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public static class RedisWorker
{
    private static ConnectionMultiplexer _redis;

    private static ISubscriber _sub;

    public static async Task InitRedis()
    {
        bool isInitFailed = true;

        foreach (var ip in XmlManager.Redis.Endpoint)
        {
            var config = new ConfigurationOptions
            {
                AbortOnConnectFail = true,
                ConnectTimeout = 2000,
                EndPoints = { $"{ip}:{XmlManager.Redis.Port}" }
            };

            if (string.IsNullOrEmpty(XmlManager.Redis.Password) == false)
            {
                config.Password = XmlManager.Redis.Password;
            }

            try
            {
                _redis = await ConnectionMultiplexer.ConnectAsync(config);

                NLogManager.Info($"Redis server connected: {ip}:{XmlManager.Redis.Port}");

                _sub = _redis.GetSubscriber();
                _sub.Subscribe(RedisChannel.Literal(XmlManager.Redis.Channel), async (ch, cmd) =>
                {
                    await HandleCommand(cmd);
                });

                _redis.ConnectionFailed += (_, ex) =>
                {
                    NLogManager.Warn($"Redis disconnected: {ex.Exception}");

                    _redis = null;
                    _sub = null;
                };

                isInitFailed = false;
                break;
            }
            catch (Exception ex)
            {
                NLogManager.Error($"Error occured during connecting to redis server: {ex.Message}");
            }
        }

        if(isInitFailed)
        {
            throw new Exception("No Alive Redis Server");
        }
    }

    private static async Task HandleCommand(string cmd)
    {
        JObject jsonObj = JObject.Parse(cmd);

        string step = null;
        string data = null;
        int id_request = -1;
        int id_tablet = -1;
        //string error_msg = null;

        if (jsonObj.ContainsKey("step"))
        {
            step = jsonObj["step"].ToString();
        }

        var ignoreSteps = new List<string>
        {
            "AI_START",
            "AI_WAITING_LIST",
            "ai_done"
        };

        foreach(var ignoreStep in ignoreSteps)
        {
            if (string.Equals(step.ToLower(), ignoreStep.ToLower()))
            {
                return;
            }
        }

        NLogManager.Info($"Command received: {cmd}");

        if (jsonObj.ContainsKey("data"))
        {
            JObject dataObj = JObject.Parse(jsonObj["data"].ToString());

            if (dataObj.ContainsKey("id_request"))
            {
                id_request = (int)dataObj["id_request"];
            }

            if (dataObj.ContainsKey("id_tablet"))
            {
                id_tablet = (int)dataObj["id_tablet"];
            }

            //if (dataObj.ContainsKey("error_msg"))
            //{
            //    error_msg = dataObj["error_msg"].ToString();
            //}
        }

        switch (step)
        {
            case "ai_error":
                try
                {
                    NLogManager.Error($"Error Occured during AI converting: {cmd}");

                    UnityMainThreadDispatcher.Instance.Enqueue(async () =>
                    {
                        await SceneWorker.ChangeSceneAsync("scError", "The AI conversion was not successful.\n\nPlease try again shortly, or use the other scanner.");
                    });
                }
                catch (Exception ex)
                {
                    NLogManager.Error($"Error occured during handling ai_error cmd: {ex.Message}");
                }

                break;

            case "capture_error":
                try
                {
                    NLogManager.Error($"Mark not recognized: {cmd}");

                    UnityMainThreadDispatcher.Instance.Enqueue(async () =>
                    {
                        await SceneWorker.ChangeSceneAsync("scError", "We couldn't detect a valid drawing.\n\nPlease place your drawing correctly and try scanning again.");
                    });
                }
                catch (Exception ex)
                {
                    NLogManager.Error($"Error occured during handling capture_error cmd: {ex.Message}");
                }

                break;

            default:
                NLogManager.Warn($"Unknown command: {cmd}");
                break;
        }
    }

    public static async Task PublishRedisAsync(string step, Dictionary<string, object> data)
    {
        JObject jsonObj = new JObject
        {
            ["step"] = step,
            ["data"] = JObject.FromObject(data)
        };

        string json = jsonObj.ToString();

        try
        {
            await _sub.PublishAsync(RedisChannel.Literal(XmlManager.Redis.Channel), json);
        }
        catch(Exception ex)
        {
            NLogManager.Error($"Error occured during sending cmd: {ex.Message}");

            throw;
        }

        NLogManager.Info($"Command sent: {json}");
    }
}
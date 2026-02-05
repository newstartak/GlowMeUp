using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine;

public class UnityMainThreadDispatcher : Singleton<UnityMainThreadDispatcher>
{
    private readonly ConcurrentQueue<Func<Task>> _jobs = new ConcurrentQueue<Func<Task>>();

    public void Enqueue(Func<Task> job)
    {
        if (job == null) return;
        _jobs.Enqueue(job);
    }

    private async void Update()
    {
        while (_jobs.TryDequeue(out var job))
        {
            try
            {
                await job();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}

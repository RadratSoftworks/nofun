using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Nofun
{
    public class JobScheduler : MonoBehaviour
    {
        private class Job
        {
            public Action caller;
            public AutoResetEvent evt;

            public Job(Action caller, AutoResetEvent waiter = null)
            {
                this.caller = caller;
                this.evt = waiter;
            }
        };

        public static JobScheduler Instance { get; private set; }
        public static bool Paused { get; set; }

        private Thread unityThread;
        private Queue<Job> jobs;
        private List<Job> postponedJobs;
        private List<Job> postponedJobs2;
        private Queue<Job> jobPool;
        private AutoResetEvent postponeDoneFlushed;

        private bool flushablePostponed = false;

        private void Start()
        {
            unityThread = Thread.CurrentThread;
            Paused = false;
            jobs = new();
            jobPool = new();

            postponedJobs = new();
            postponedJobs2 = new();
            Instance = this;

            postponeDoneFlushed = new AutoResetEvent(false);
            postponeDoneFlushed.Set();
        }

        private void Update()
        {
            if (Paused)
            {
                return;
            }

            lock (jobs)
            {
                while (jobs.Count != 0)
                {
                    Job job = jobs.Dequeue();
                    job.caller();

                    if (job.evt != null)
                    {
                        job.evt.Set();
                    }

                    lock (jobPool)
                    {
                        jobPool.Enqueue(job);
                    }
                }
            }

            if (flushablePostponed)
            {
                lock (postponedJobs2)
                {
                    postponedJobs2.ForEach(job =>
                    {
                        job.caller();
                        job.evt?.Set();
                    });

                    lock (jobPool)
                    {
                        foreach (var job in postponedJobs2)
                        {
                            jobPool.Enqueue(job);
                        }
                    }

                    postponedJobs2.Clear();

                    flushablePostponed = false;
                    postponeDoneFlushed.Set();
                }
            }
        }

        private Job RetrieveOrCreateJob(Action act, bool useEvent = false)
        {
            Job job = null;

            lock (jobPool)
            {
                if (jobPool.Count != 0)
                {
                    job = jobPool.Dequeue();
                    job.caller = act;

                    if (useEvent)
                    {
                        if (job.evt == null)
                        {
                            job.evt = new AutoResetEvent(false);
                        }
                        else
                        {
                            job.evt.Reset();
                        }
                    }
                }
                else
                {
                    job = new Job(act, useEvent ? new AutoResetEvent(false) : null);
                }
            }

            return job;
        }

        public void PostponeToUnityThread(Action act, bool toBeginning = false)
        {
            lock (postponedJobs)
            {
                if (toBeginning)
                {
                    postponedJobs.Insert(0, RetrieveOrCreateJob(act));
                }
                else
                {
                    postponedJobs.Add(RetrieveOrCreateJob(act));
                }
            }
        }

        public void FlushPostponed()
        {
            postponeDoneFlushed.WaitOne();

            (postponedJobs2, postponedJobs) = (postponedJobs, postponedJobs2);
            flushablePostponed = true;
        }

        public void RunOnUnityThread(Action act)
        {
            if (Thread.CurrentThread != unityThread)
            {
                lock (jobs)
                {
                    jobs.Enqueue(RetrieveOrCreateJob(act));
                }
            }
            else
            {
                act();
            }
        }

        public void RunOnUnityThreadSync(Action act)
        {
            if (Thread.CurrentThread != unityThread)
            {
                AutoResetEvent evt;

                lock (jobs)
                {
                    Job job = RetrieveOrCreateJob(act, true);
                    evt = job.evt;

                    jobs.Enqueue(job);
                }

                evt.WaitOne();
            }
            else
            {
                act();
            }
        }
    }
}

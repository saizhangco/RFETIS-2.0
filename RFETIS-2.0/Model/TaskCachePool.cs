using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static RFETIS_2._0.Model.TaskCache;
using System.Configuration;
using RFETIS_2._0.SIL;

namespace RFETIS_2._0.Model
{
    public class TaskCachePool
    {
        public Dictionary<int, TaskCache> pool = new Dictionary<int, TaskCache>();
        private EleTagResponseHandler ResponseHandler;

        private int timeout = 15; // seconds
        private bool wlock = false;

        public TaskCachePool()
        {
            string timeout = ConfigurationManager.AppSettings.Get("timeout");
            if (timeout != null)
            {
                try
                {
                    this.timeout = int.Parse(timeout);
                    Console.WriteLine("Timeout : " + timeout);
                }
                catch (Exception)
                {

                }
            }
        }

        public void setEleTagResponseHandler(EleTagResponseHandler handler)
        {
            ResponseHandler += handler;
        }

        /// <summary>
        /// 创建任务缓存
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <param name="a"></param>
        /// <returns>
        ///   0  创建成功
        ///   1  取药任务已经存在
        ///   2  补药任务已经存在
        ///   3  其他错误
        /// </returns>
        public int create(TaskCacheType type, int id, int a)
        {
            if (pool.ContainsKey(id) )
            {
                if( !pool[id].Active)
                {
                    pool[id].update(type, id, a);
                }
                else
                {
                    switch (pool[id].Type)
                    {
                        case TaskCacheType.TAKE: return 1;
                        case TaskCacheType.ADD: return 2;
                        default: return 3;
                    }
                }
            }
            else
            {
                while (wlock) ;
                wlock = true;
                TaskCache cache = new TaskCache(type, id, a);
                pool.Add(id, cache);
                wlock = false;
                Thread td = new Thread(() => {
                    Thread.Sleep(timeout * 1000);
                    if( pool.ContainsKey(id) )
                    {
                        if( pool[id].Active && !pool[id].Exelock )
                        {
                            pool[id].Active = false;
                            if (pool[id].Type == TaskCacheType.TAKE)
                            {
                                ResponseHandler.Invoke(id, EleTagResponseState.TAKE_TIMEOUT, "");
                            }
                            else if (pool[id].Type == TaskCacheType.ADD)
                            {
                                ResponseHandler.Invoke(id, EleTagResponseState.ADD_TIMEOUT, "");
                            }
                        }
                    }
                });
                td.IsBackground = true;
                td.Start();
            }
            return 0;
        }

        public void destory(int id)
        {
            if( pool.ContainsKey(id) )
            {
                if(pool[id].Exelock)
                {
                    pool[id].Active = false;
                    pool[id].Exelock = false;
                }
            }
        }

        public TaskCache getTaskCache(int id)
        {
            if( pool.ContainsKey(id))
            {
                if( pool[id].Active && !pool[id].Exelock )
                {
                    pool[id].Exelock = true;
                    return pool[id];
                }
            }
            return null;
        }
    }
}

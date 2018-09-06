using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFETIS_2._0.Model
{
    public class TaskCache
    {
        public enum TaskCacheType
        {
            NONE,
            TAKE,
            ADD
        }
        private TaskCacheType taskCacheType;
        private int guid;
        private int amount;

        public TaskCacheType Type
        {
            get { return taskCacheType;  }
        }

        public int Guid
        {
            get { return guid; }
        }

        public int Amount
        {
            get { return amount; }
        }
        
        private bool active = false;

        public bool Active
        {
            get { return active; }
            set { active = value; }
        }

        private bool exelock = false;

        public bool Exelock
        {
            get { return exelock; }
            set { exelock = value; }
        }

        private TaskCache()
        {
            taskCacheType = TaskCacheType.NONE;
            guid = 0;
            amount = -1;
        }

        public TaskCache(TaskCacheType type, int id, int a)
        {
            taskCacheType = type;
            guid = id;
            amount = a;
            active = true;
        }

        public void update(TaskCacheType type, int id, int a)
        {
            taskCacheType = type;
            guid = id;
            amount = a;
            active = true;
        }

    }
}

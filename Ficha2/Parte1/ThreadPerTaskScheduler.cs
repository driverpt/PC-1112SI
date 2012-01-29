using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Parte1
{
    public class ThreadPerTaskScheduler : TaskScheduler
    {

        protected override void QueueTask(Task task)
        {
            throw new NotImplementedException();
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            
            throw new NotImplementedException();
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            
            throw new NotImplementedException();
        }
    }
}
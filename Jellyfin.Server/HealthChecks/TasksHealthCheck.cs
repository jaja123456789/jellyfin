using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Jellyfin.Server.HealthChecks;

/// <summary>
/// Implementation of the <see cref="TasksHealthCheck"/>.
/// </summary>
public class TasksHealthCheck : IHealthCheck
{
    private readonly ITaskManager _taskManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="TasksHealthCheck"/> class.
    /// </summary>
    /// <param name="taskManager">Instance of the <see cref="ITaskManager"/> interface.</param>
    public TasksHealthCheck(ITaskManager taskManager)
    {
        _taskManager = taskManager;
    }

    /// <inheritdoc />
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        IEnumerable<IScheduledTaskWorker> tasks = _taskManager.ScheduledTasks.OrderBy(o => o.Name);

        foreach (var task in tasks)
        {
            if (task.ScheduledTask is IConfigurableScheduledTask scheduledTask)
            {
                if (!scheduledTask.IsEnabled)
                {
                    continue;
                }
            }

            var taskInfo = ScheduledTaskHelpers.GetTaskInfo(task);

            if (taskInfo != null && taskInfo.LastExecutionResult?.Status == TaskCompletionStatus.Failed)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy());
            }
        }

        return Task.FromResult(HealthCheckResult.Healthy());
    }
}

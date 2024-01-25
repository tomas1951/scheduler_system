namespace SharedResources.Enums;

public enum SchedulerTaskStatus
{
    NoAssignedTask,  // how can task status be no assigned task, this belongs to client status
    Waiting,
    InProgress,
    Done,
    Error,
    Stopped
}

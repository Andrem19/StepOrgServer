using System;

namespace StepOrg.Entities.ModulesStruct.Payloads
{
    public enum TYPE
    {
        EVERYDAY,
        ALLWAYS,
        ONCE
    }
    public enum STEP
    {
        CREATE,
        InProgress,
        DONE
    }
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public TYPE Type { get; set; }
        public bool Complete { get; set; } = false;
        public string? NameWhoCompletLast { get; set; }

        public bool IsSteps { get; set; }
        public STEP? Step { get; set; }
        public string? Creator { get; set; }
        public string? Executor { get; set; }

        public int PayloadId { get; set; }
        public Payload? Payload { get; set; }
    }
}

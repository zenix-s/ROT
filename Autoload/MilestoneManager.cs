using System.Collections.Generic;
using Godot.NativeInterop;

namespace RotOfTime.Autoload;

public enum Milestone
{
    GameStarted,
    TutorialStarted
}

public static class MilestoneExtensions
{
    public static bool IsCompleted(this Milestone milestone)
    {
        return GameManager.Instance.Milestones.CompletedMilestones.Contains(milestone);
    }

    public static void Complete(this Milestone milestone)
    {
        GameManager.Instance.Milestones.CompleteMilestone(milestone);
    }
}

public class MilestoneManager
{
    public HashSet<Milestone> CompletedMilestones { get; private set; } = [];

    public void CompleteMilestone(Milestone milestone)
    {
        CompletedMilestones.Add(milestone);
    }

    public void LoadMilestones(IEnumerable<Milestone> milestones)
    {
        CompletedMilestones = new HashSet<Milestone>(milestones);
    }
}

using System.Collections.Generic;
using Godot;

namespace RotOfTime.Autoload;

public enum Milestone
{
    TutorialStarted
}

public static class MilestoneExtensions
{
    public static bool IsCompleted(this Milestone milestone)
    {
        return MilestoneManager.instance.CompletedMilestones.Contains(milestone);
    }

    public static void Complete(this Milestone milestone)
    {
        MilestoneManager.instance.CompleteMilestone(milestone);
    }
}

public partial class MilestoneManager : Node
{
    public static MilestoneManager instance { get; private set; }

    public HashSet<Milestone> CompletedMilestones { get; private set; } = [];

    public override void _Ready()
    {
        instance = this;
    }

    public void CompleteMilestone(Milestone milestone)
    {
        CompletedMilestones.Add(milestone);
    }

    public void LoadMilestones(IEnumerable<Milestone> milestones)
    {
        CompletedMilestones = new HashSet<Milestone>(milestones);
    }
}

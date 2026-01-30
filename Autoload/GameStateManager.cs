using System.Collections.Generic;

namespace RotOfTime.Autoload;

#region Milestones management

public enum Milestone
{
    GameStarted,
    TutorialStarted
}

public static class MilestoneExtensions
{
    public static bool IsCompleted(this Milestone milestone)
    {
        return GameManager.Instance.GameStateManager.CompletedMilestones.Contains(milestone);
    }

    public static void Complete(this Milestone milestone)
    {
        GameManager.Instance.GameStateManager.CompleteMilestone(milestone);
    }
}

public static class GameStateManagerMilestoneExtensions
{
    extension(GameStateManager manager)
    {
        public void CompleteMilestone(Milestone milestone)
        {
            manager.CompletedMilestones.Add(milestone);
        }

        public void LoadMilestones(IEnumerable<Milestone> milestones)
        {
            manager.CompletedMilestones = new HashSet<Milestone>(milestones);
        }
    }
}

#endregion

public class GameStateManager
{
    public HashSet<Milestone> CompletedMilestones { get; set; } = [];
}

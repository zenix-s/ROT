using System.Collections.Generic;
using RotOfTime.Autoload;

namespace RotOfTime.Core.GameData;

/// <summary>
///     Permanent progression that persists across all runs.
///     Saved to meta.json file.
/// </summary>
public class MetaData
{
    public List<Milestone> CompletedMilestones { get; set; } = [];
}

using RotOfTime.Core.Entities.StateMachine;
using RotOfTime.Scenes.Bosses.SoulFragment1.StateMachine.States;

namespace RotOfTime.Scenes.Bosses.SoulFragment1.StateMachine;

public partial class SoulFragmentStateMachine : StateMachine<SoulFragment1>
{
    public override void _Ready()
    {
        base._Ready();
        ChangeState<IdleState>();
    }
}

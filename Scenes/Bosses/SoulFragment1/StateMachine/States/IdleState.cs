using RotOfTime.Core.Entities.StateMachine;

namespace RotOfTime.Scenes.Bosses.SoulFragment1.StateMachine.States;

public partial class IdleState : State<SoulFragment1>
{
    public override void PhysicsProcess(double delta)
    {
        if (TargetEntity.Target != null)
            StateMachine.ChangeState<ChasingState>();
    }
}

using RotOfTime.Core.Entities.StateMachine;

namespace RotOfTime.Scenes.Bosses.SoulFragment1.StateMachine.States;

public partial class DashingState : State<SoulFragment1>
{
    private float _traveled;

    public override void Enter()
    {
        _traveled = 0f;
    }

    public override void PhysicsProcess(double delta)
    {
        TargetEntity.Velocity = TargetEntity.DashDirection * TargetEntity.DashSpeed;
        TargetEntity.MoveAndSlide();

        _traveled += TargetEntity.DashSpeed * (float)delta;

        bool hitWall = TargetEntity.GetSlideCollisionCount() > 0;

        if (_traveled >= TargetEntity.DashDistance || hitWall)
            StateMachine.ChangeState<ChasingState>();
    }
}

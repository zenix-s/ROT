using RotOfTime.Core.Entities.StateMachine;
using RotOfTime.Scenes.Enemies.BasicEnemy.StateMachine.States;

namespace RotOfTime.Scenes.Enemies.BasicEnemy.StateMachine;

public partial class EnemyStateMachine : StateMachine<BasicEnemy>
{
    public override void _Ready()
    {
        base._Ready();
        ChangeState<IdleState>();
    }
}

using RotOfTime.Core.Entities.StateMachine;
using RotOfTime.Scenes.Player.StateMachine.States;

namespace RotOfTime.Scenes.Player.StateMachine;

public partial class PlayerStateMachine : StateMachine<Player>
{
    public override void _Ready()
    {
        base._Ready();
        ChangeState<IdleState>();
    }
}
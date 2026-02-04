namespace RotOfTime.Core.Entities.StateMachine;

public interface IState
{
    string StateId { get; }
    void Enter();
    void Exit();
    void PhysicsProcess(double delta);
    void Process(double delta);
}
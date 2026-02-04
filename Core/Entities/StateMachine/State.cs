using Godot;

namespace RotOfTime.Core.Entities.StateMachine;

public abstract partial class State<T> : Node, IState
    where T : Node
{
    public string StateId => GetType().Name;
    protected T TargetEntity;
    protected StateMachine<T> StateMachine;

    public void Init(T targetEntity, StateMachine<T> stateMachine)
    {
        TargetEntity = targetEntity;
        StateMachine = stateMachine;
    }

    public virtual void Enter() { }

    public virtual void Exit() { }

    public virtual void PhysicsProcess(double delta) { }

    public virtual void Process(double delta) { }
}

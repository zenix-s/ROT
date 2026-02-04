using System;
using System.Collections.Generic;
using Godot;

namespace RotOfTime.Core.Entities.StateMachine;

public abstract partial class StateMachine<T> : Node
    where T : Node
{
    protected T TargetEntity;
    private readonly Dictionary<string, IState> _stateRegistry = [];
    private IState _currentState;

    public override void _Ready()
    {
        TargetEntity = GetParent<T>();

        foreach (var child in GetChildren())
        {
            if (child is not State<T> state)
                continue;

            string stateId = state.StateId;
            _stateRegistry[stateId] = state;
            state.Init(TargetEntity, this);
        }
    }

    public void ChangeState<TState>() 
        where TState : State<T>
    {
        string newStateId = typeof(TState).Name;
        
        if (!_stateRegistry.TryGetValue(newStateId, out var newState))
            throw new InvalidOperationException($"State {newStateId} not found in state registry");
            
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
    }

    public override void _Process(double delta)
    {
        _currentState?.Process(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        _currentState?.PhysicsProcess(delta);
    }
}

using Godot;
using System;

public interface ITransition
{
    public void Transition(Action midTransition, Action postTransition);
    public void TransitionIn(Action midTransition = null);
    public void TransitionOut(Action postTransition = null);
}

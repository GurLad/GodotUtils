using Godot;
using System;

public partial class FadeTransition : ColorRect, ITransition
{
    [Export] private float transitionTime;

    private Interpolator interpolator = new Interpolator();

    public override void _Ready()
    {
        base._Ready();
        AddChild(interpolator);
        Modulate = new Color(Modulate, 0);
        Visible = false;
    }

    public void Transition(Action midTransition, Action postTransition)
    {
        TransitionIn(() =>
        {
            midTransition?.Invoke();
            TransitionOut(postTransition);
        });
    }

    public void TransitionIn(Action midTransition = null)
    {
        Visible = true;
        MouseFilter = MouseFilterEnum.Stop;
        Modulate = new Color(Modulate, 0);
        interpolator.Interpolate(transitionTime, new Interpolator.InterpolateObject(
            a => Modulate = new Color(Modulate, a),
            0,
            1));
        interpolator.OnFinish = midTransition;
    }

    public void TransitionOut(Action postTransition = null)
    {
        Modulate = new Color(Modulate, 1);
        interpolator.Interpolate(transitionTime, new Interpolator.InterpolateObject(
            a => Modulate = new Color(Modulate, a),
            1,
            0));
        interpolator.OnFinish = () =>
        {
            MouseFilter = MouseFilterEnum.Ignore;
            Visible = false;
            postTransition?.Invoke();
        };
    }
}

using Godot;
using Godot.Collections;
using System;

public partial class SceneController : Node
{
    private enum State { Idle, FadeOut, FadeIn }
    public static SceneController Current;

    [Export]
    private string FirstScene;
    [Export]
    private Dictionary<string, PackedScene> Scenes;
    [Export]
    private Timer Timer;
    [Export]
    private Control BlackScreen;
    [Export]
    private Node ScenesNode;

    private State state;
    private Node currentScene = null;
    private Action midTransition;
    private Action postTransition;

    public override void _Ready()
    {
        base._Ready();
        Current = this;
        TransitionToScene(FirstScene);
        FinishFadeOut();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        switch (state)
        {
            case State.Idle:
                break;
            case State.FadeOut:
                BlackScreen.Modulate = new Color(BlackScreen.Modulate, Timer.Percent());
                if (Timer.TimeLeft <= 0)
                {
                    FinishFadeOut();
                }
                break;
            case State.FadeIn:
                BlackScreen.Modulate = new Color(BlackScreen.Modulate, 1 - Timer.Percent());
                if (Timer.TimeLeft <= 0)
                {
                    FinishFadeIn();
                }
                break;
            default:
                break;
        }
    }

    private void FinishFadeOut()
    {
        BlackScreen.Modulate = new Color(BlackScreen.Modulate, 1);
        state = State.FadeIn;
        midTransition?.Invoke();
        Timer.Start();
    }

    private void FinishFadeIn()
    {
        BlackScreen.Modulate = new Color(BlackScreen.Modulate, 0);
        state = State.Idle;
        BlackScreen.MouseFilter = Control.MouseFilterEnum.Ignore;
        postTransition?.Invoke();
    }

    public void Transition(Action midTransition, Action postTransition)
    {
        BlackScreen.MouseFilter = Control.MouseFilterEnum.Stop;
        this.midTransition = midTransition;
        this.postTransition = postTransition;
        state = State.FadeOut;
        Timer.Start();
    }

    public void TransitionToScene(string name, Action midTransition = null)
    {
        Transition(() =>
        {
            ClearCurrentScene();
            ScenesNode.AddChild(currentScene = Scenes[name].Instantiate<Node>());
            midTransition?.Invoke();
        }, null);
    }

    private void ClearCurrentScene()
    {
        if (currentScene != null)
        {
            currentScene.QueueFree();
            currentScene = null;
        }
    }
}

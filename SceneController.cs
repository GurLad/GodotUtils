using Godot;
using System;

public class SceneController : Node
{
    private enum State { Idle, FadeOut, FadeIn }
    public static SceneController Current;

    [Export]
    public NodePath PathTimer;
    [Export]
    public NodePath PathBlackScreen;
    [Export]
    public NodePath PathScenesNode;
    [Export]
    public PackedScene StartScene;
    [Export]
    public PackedScene GameScene;

    private State state;
    private Timer transitionTimer;
    private Control blackScreen;
    private Node scenesNode;
    private Node currentScene = null;
    private Action midTransition;
    private Action postTransition;

    public override void _Ready()
    {
        base._Ready();
        Current = this;
        transitionTimer = GetNode<Timer>(PathTimer);
        blackScreen = GetNode<Control>(PathBlackScreen);
        scenesNode = GetNode<Node>(PathScenesNode);
        TransitionToScene(StartScene);
        FinishFadeOut();
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        switch (state)
        {
            case State.Idle:
                break;
            case State.FadeOut:
                blackScreen.Modulate = new Color(blackScreen.Modulate, transitionTimer.Percent());
                if (transitionTimer.TimeLeft <= 0)
                {
                    FinishFadeOut();
                }
                break;
            case State.FadeIn:
                blackScreen.Modulate = new Color(blackScreen.Modulate, 1 - transitionTimer.Percent());
                if (transitionTimer.TimeLeft <= 0)
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
        blackScreen.Modulate = new Color(blackScreen.Modulate, 1);
        state = State.FadeIn;
        midTransition?.Invoke();
        transitionTimer.Start();
    }

    private void FinishFadeIn()
    {
        blackScreen.Modulate = new Color(blackScreen.Modulate, 0);
        state = State.Idle;
        blackScreen.MouseFilter = Control.MouseFilterEnum.Ignore;
        postTransition?.Invoke();
    }

    public void Transition(Action midTransition, Action postTransition)
    {
        blackScreen.MouseFilter = Control.MouseFilterEnum.Stop;
        this.midTransition = midTransition;
        this.postTransition = postTransition;
        state = State.FadeOut;
        transitionTimer.Start();
    }

    public void TransitionToScene(PackedScene scene)
    {
        Transition(() =>
        {
            ClearCurrentScene();
            scenesNode.AddChild(currentScene = scene.Instance<Node>());
        }, null);
    }

    public void TransitionToGame()
    {
        Transition(BeginGame, null);
    }

    private void ClearCurrentScene()
    {
        if (currentScene != null)
        {
            currentScene.QueueFree();
            currentScene = null;
        }
    }

    private void BeginGame()
    {
        ClearCurrentScene();
        scenesNode.AddChild(currentScene = GameScene.Instance<GameController>());
        currentScene.Connect("OnRestart", this, "TransitionToGame");
    }
}

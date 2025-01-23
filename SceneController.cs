using Godot;
using Godot.Collections;
using System;

public partial class SceneController : Node
{
    public static SceneController Current { get; private set; }

    [Export]
    private string firstScene;
    [Export]
    private Dictionary<string, PackedScene> scenes;
    [Export]
    private NodePath pathTransition;
    [Export]
    private Node scenesNode;

    private ITransition transition;
    private Node currentScene = null;

    public override void _Ready()
    {
        base._Ready();
        transition = GetNode(pathTransition) is ITransition t ? t : null;
        if (transition == null)
        {
            GD.PushError("SceneController: Invalid transition node! Got " + GetNode(pathTransition).GetType() + " instead of ITransition");
        }
        Current = this;
        scenesNode.AddChild(currentScene = scenes[firstScene].Instantiate<Node>());
        transition?.TransitionOut();
    }

    public void TransitionToScene(string name, Action midTransition = null)
    {
        Transition(() =>
        {
            ClearCurrentScene();
            scenesNode.AddChild(currentScene = scenes[name].Instantiate<Node>());
            midTransition?.Invoke();
        }, null);
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event is InputEventKey keyEvent && !keyEvent.IsEcho() && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.Escape || keyEvent.Keycode == Key.F5)
            {
                DisplayServer.WindowSetMode(DisplayServer.WindowGetMode() == DisplayServer.WindowMode.ExclusiveFullscreen ?
                    DisplayServer.WindowMode.Windowed :
                    DisplayServer.WindowMode.ExclusiveFullscreen);
            }
        }
    }

    private void Transition(Action midTransition, Action postTransition)
    {
        if (transition != null)
        {
            transition.Transition(midTransition, postTransition);
        }
        else
        {
            midTransition?.Invoke();
            postTransition?.Invoke();
        }
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

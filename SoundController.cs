using Godot;
using System;

public partial class SoundController : Node
{
    public static SoundController Current { get; private set; }
    [Export]
    private AudioStreamPlayer player;

    public override void _Ready()
    {
        base._Ready();
        Current = this;
    }

    public void PlaySFX(AudioStream sfx)
    {
        if (player.Playing)
        {
            GD.PushWarning("SFX overlap!");
        }
        player.Stream = sfx;
        player.Play();
    }
}

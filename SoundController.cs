using Godot;
using System;

public partial class SoundController : Node
{
    public static SoundController Current { get; private set; }
    
    [Export] private Godot.Collections.Dictionary<string, AudioStream> sfxDict;
    
    private AudioStreamPlayer player;

    public override void _Ready()
    {
        base._Ready();
        Current = this;
        AddChild(player = new AudioStreamPlayer());
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

    public void PlaySFX(string name)
    {
    	if (sfxDict.ContainsKey(name))
        {
            PlaySFX(sfxDict[name]);
        }
        else
        {
            GD.PushError("SFX error: no SFX named " + name + "!");
        }
    }
}

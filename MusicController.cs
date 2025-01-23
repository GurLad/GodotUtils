using Godot;
using System;
using System.Collections.Generic;

public partial class MusicController : Node
{
    public enum TransitionMode { FadeInOut, Crossfade }
    // TBA: Find better names & descriptions
    public enum MemoryMode { None, KeepTimestamp, UseTimestampFromThePreviousTimeThisTrackWasPlayed }

    private static MusicController current;

    [ExportCategory("Tracks")]
    [Export] private string firstTrack;
    [Export] private string[] trackNames;
    [Export] private AudioStream[] trackStreams;
    [ExportCategory("Configs")]
    [Export] MemoryMode memoryMode = MemoryMode.None;
    [Export] TransitionMode transitionMode = TransitionMode.Crossfade;
    [Export] private float transitionTime = 1;

    private Dictionary<string, TrackInfo> tracks = new Dictionary<string, TrackInfo>();
    private Interpolator interpolator = new Interpolator();
    private AudioStreamPlayer currentPlayer = new AudioStreamPlayer();
    private AudioStreamPlayer previousPlayer = new AudioStreamPlayer();

    private string currentTrack = "";

    public override void _Ready()
    {
        base._Ready();
        AddChild(interpolator);
        AddChild(currentPlayer);
        AddChild(previousPlayer);
        for (int i = 0; i < Mathf.Min(trackNames.Length, trackStreams.Length); i++)
        {
            tracks.Add(trackNames[i], new TrackInfo(trackStreams[i]));
        }
        if (tracks.ContainsKey(firstTrack))
        {
            currentPlayer.Stream = tracks[currentTrack = firstTrack].Stream;
            currentPlayer.Play(tracks[currentTrack].Position);
        }
        else
        {
            GD.PushError("MusicPlayer: Invalid first track! \"" + firstTrack + "\" doesn't exist");
        }
        current = this;
    }

    public static void Play(string name, MemoryMode? memoryModeOverride = null)
    {
        if (current != null)
        {
            current.PlayInternal(name, memoryModeOverride);
        }
        else
        {
            GD.PushError("Missing music controller!");
        }
    }

    private void PlayInternal(string name, MemoryMode? memoryModeOverride = null)
    {
        if (currentTrack == name)
        {
            return;
        }

        MemoryMode currentMemoryMode = memoryModeOverride ?? memoryMode;

        switch (transitionMode)
        {
            case TransitionMode.FadeInOut:
                interpolator.Interpolate(transitionTime, new Interpolator.InterpolateObject(
                    a => currentPlayer.VolumeDb = Mathf.LinearToDb(a),
                    Mathf.DbToLinear(currentPlayer.VolumeDb),
                    0));
                interpolator.OnFinish = () =>
                {
                    tracks[currentTrack].Position = currentPlayer.GetPlaybackPosition();
                    float nextPos = GetNextPosition(currentMemoryMode);
                    SwitchTrack(currentPlayer, tracks[currentTrack = name], nextPos);
                    interpolator.Interpolate(transitionTime, new Interpolator.InterpolateObject(
                        a => currentPlayer.VolumeDb = Mathf.LinearToDb(a),
                        Mathf.DbToLinear(currentPlayer.VolumeDb),
                        1));
                };
                break;
            case TransitionMode.Crossfade:
                tracks[currentTrack].Position = currentPlayer.GetPlaybackPosition();
                float nextPos = GetNextPosition(currentMemoryMode);
                SwitchTrack(previousPlayer, tracks[name], nextPos);
                previousPlayer.VolumeDb = Mathf.LinearToDb(0);
                interpolator.Interpolate(transitionTime,
                    new Interpolator.InterpolateObject(
                        a => currentPlayer.VolumeDb = Mathf.LinearToDb(a),
                        Mathf.DbToLinear(currentPlayer.VolumeDb),
                        0),
                    new Interpolator.InterpolateObject(
                        a => previousPlayer.VolumeDb = Mathf.LinearToDb(a),
                        Mathf.DbToLinear(previousPlayer.VolumeDb),
                        1));
                interpolator.OnFinish = () =>
                {
                    AudioStreamPlayer temp = previousPlayer;
                    previousPlayer = currentPlayer;
                    currentPlayer = previousPlayer;
                    previousPlayer.Stop();
                    currentTrack = name;
                };
                break;
            default:
                break;
        }
    }

    private float GetNextPosition(MemoryMode currentMemoryMode)
    {
        return currentMemoryMode switch
        {
            MemoryMode.None => 0,
            MemoryMode.KeepTimestamp => currentPlayer.GetPlaybackPosition(),
            MemoryMode.UseTimestampFromThePreviousTimeThisTrackWasPlayed => tracks[currentTrack].Position,
            _ => throw new Exception("Impossible!"),
        };
    }

    private void SwitchTrack(AudioStreamPlayer player, TrackInfo track, float position)
    {
        player.Stream = track.Stream;
        player.Play(position);
    }

    private class TrackInfo
    {
        public AudioStream Stream;
        public float Position = 0;

        public TrackInfo(AudioStream stream)
        {
            Stream = stream;
        }
    }
}

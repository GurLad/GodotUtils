using Godot;
using System;
using System.Collections.Generic;

public partial class MusicController : AudioStreamPlayer
{
    private static MusicController current;

    [Export] private float transitionTime = 1;
    [Export] private string firstTrack;
    [Export] private string[] trackNames;
    [Export] private AudioStream[] trackStreams;

    private Dictionary<string, TrackInfo> tracks = new Dictionary<string, TrackInfo>();
    private Interpolator interpolator = new Interpolator();

    private string currentTrack = "";

    public override void _Ready()
    {
        base._Ready();
        AddChild(interpolator);
        for (int i = 0; i < Mathf.Min(trackNames.Length, trackStreams.Length); i++)
        {
            tracks.Add(trackNames[i], new TrackInfo(trackStreams[i]));
        }
        Stream = tracks[currentTrack = firstTrack].Stream;
        Play(tracks[currentTrack].Position);
        current = this;
    }

    public static void Play(string name, bool keepPreviousTimestamp = true)
    {
        if (current != null)
        {
            if (current.currentTrack == name)
            {
                return;
            }
            current.interpolator.Interpolate(current.transitionTime, new Interpolator.InterpolateObject(
                a => current.VolumeDb = Mathf.LinearToDb(a),
                Mathf.DbToLinear(current.VolumeDb),
                0));
            current.interpolator.OnFinish = () =>
            {
                current.tracks[current.currentTrack].Position = current.GetPlaybackPosition();
                current.Stream = current.tracks[current.currentTrack = name].Stream;
                current.Play(current.tracks[current.currentTrack].Position);
                current.interpolator.Interpolate(current.transitionTime, new Interpolator.InterpolateObject(
                    a => current.VolumeDb = Mathf.LinearToDb(a),
                    Mathf.DbToLinear(current.VolumeDb),
                    1));
            };
        }
        else
        {
            GD.PrintErr("Missing music controller!");
        }
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

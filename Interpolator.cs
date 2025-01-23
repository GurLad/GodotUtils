using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Interpolator : Node
{
    public enum Mode { Allowed, Warning, Error, ErrorAndIgnore }

    // Properties
    public Action OnFinish { private get; set; } = null;
    public bool Active { get; private set; }
    public Mode InterruptMode { private get; set; } = Mode.Warning; 

    private Timer timer = new Timer();
    private List<InterpolateObject> objects = new List<InterpolateObject>();

    public override void _Ready()
    {
        base._Ready();
        timer.OneShot = true;
        AddChild(timer);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Active)
        {
            objects.ForEach(a => a.SetValue(a.LerpFunc(a.BaseValue, a.TargetValue, a.EasingFunction(timer.Percent()))));
            if (timer.TimeLeft <= 0)
            {
                objects.Clear();
                Active = false;
                Action onFinish = OnFinish;
                OnFinish = null;
                onFinish?.Invoke();
            }
        }
    }

    public void Stop(bool triggerOnFinish)
    {
        objects.Clear();
        Active = false;
        if (triggerOnFinish)
        {
            Action onFinish = OnFinish;
            OnFinish = null;
            onFinish?.Invoke();
        }
        else
        {
            OnFinish = null;
        }
    }

    public void Interpolate(float time, params InterpolateObject[] objects)
    {
        if (Active)
        {
            switch (InterruptMode)
            {
                case Mode.Allowed:
                    break;
                case Mode.Warning:
                    GD.PushWarning("Interpolator is active!");
                    break;
                case Mode.Error:
                    GD.PushError("Interpolator is active!");
                    break;
                case Mode.ErrorAndIgnore:
                    GD.PushError("Interpolator is active!");
                    return;
                default:
                    break;
            }
        }
        this.objects = objects.ToList();
        timer.WaitTime = time;
        timer.Start();
        Active = true;
    }

    public void Delay(float time)
    {
        if (Active)
        {
            switch (InterruptMode)
            {
                case Mode.Allowed:
                    break;
                case Mode.Warning:
                    GD.PushWarning("Interpolator is active!");
                    break;
                case Mode.Error:
                    GD.PushError("Interpolator is active!");
                    break;
                case Mode.ErrorAndIgnore:
                    GD.PushError("Interpolator is active!");
                    return;
                default:
                    break;
            }
        }
        objects.Clear();
        timer.WaitTime = time;
        timer.Start();
        Active = true;
    }

    public class InterpolateObject
    {
        public Action<object> SetValue;
        public Func<object, object, float, object> LerpFunc;
        public object BaseValue;
        public object TargetValue;
        public Func<float, float> EasingFunction;

        protected InterpolateObject(Action<object> setValue, Func<object, object, float, object> lerpFunc, object baseValue, object targetValue, Func<float, float> easingFunction = null)
        {
            SetValue = setValue;
            LerpFunc = lerpFunc;
            BaseValue = baseValue;
            TargetValue = targetValue;
            EasingFunction = easingFunction ?? ((a) => a);
        }

        protected InterpolateObject(Action<object> setValue, Func<object, float, object> mulFunc, Func<object, object, object> addFunc, object baseValue, object targetValue, Func<float, float> easingFunction = null) :
            this(
                setValue,
                (a, b, t) => addFunc(mulFunc(a, 1 - t), mulFunc(b, t)),
                baseValue,
                targetValue,
                easingFunction)
        { }

        public InterpolateObject(Action<Vector3> setValue, Vector3 baseValue, Vector3 targetValue, Func<float, float> easingFunction = null) :
            this(
                (a) => setValue((Vector3)a),
                (a, t) => (Vector3)a * t,
                (a, b) => (Vector3)a + (Vector3)b,
                baseValue,
                targetValue,
                easingFunction)
        { }

        public InterpolateObject(Action<Vector2> setValue, Vector2 baseValue, Vector2 targetValue, Func<float, float> easingFunction = null) :
            this(
                (a) => setValue((Vector2)a),
                (a, t) => (Vector2)a * t,
                (a, b) => (Vector2)a + (Vector2)b,
                baseValue,
                targetValue,
                easingFunction)
        { }

        public InterpolateObject(Action<float> setValue, float baseValue, float targetValue, Func<float, float> easingFunction = null) :
            this(
                (a) => setValue((float)a),
                (a, t) => (float)a * t,
                (a, b) => (float)a + (float)b,
                baseValue,
                targetValue,
                easingFunction)
        { }

        public InterpolateObject(Action<Quaternion> setValue, Quaternion baseValue, Quaternion targetValue, Func<float, float> easingFunction = null) :
            this(
                (a) => setValue((Quaternion)a),
                (a, b, t) => ((Quaternion)a).Slerp((Quaternion)b, t),
                baseValue,
                targetValue,
                easingFunction)
        { }

        public InterpolateObject(Action<Color> setValue, Color baseValue, Color targetValue, Func<float, float> easingFunction = null) :
            this(
                (a) => setValue((Color)a),
                (a, t) => (Color)a * t,
                (a, b) => (Color)a + (Color)b,
                baseValue,
                targetValue,
                easingFunction)
        { }
    }

    public class InterpolateObject<T> : InterpolateObject
    {
        public InterpolateObject(Action<T> setValue, Func<T, float, T> mulFunc, Func<T, T, T> addFunc, T baseValue, T targetValue, Func<float, float> easingFunction = null) :
            base((a) => setValue((T)a), (a, t) => mulFunc((T)a, t), (a, b) => addFunc((T)a, (T)b), baseValue, targetValue, easingFunction)
        { }

        public InterpolateObject(Action<T> setValue, Func<T, T, float, T> lerpFunc, T baseValue, T targetValue, Func<float, float> easingFunction = null) :
            base((a) => setValue((T)a), (a, b, t) => lerpFunc((T)a, (T)b, t), baseValue, targetValue, easingFunction)
        { }
    }
}

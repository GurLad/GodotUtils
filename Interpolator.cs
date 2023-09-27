using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Interpolator : Node
{
    // Exports
    [Export]
    private Timer timer;
    // Properties
    private bool active;
    private List<InterpolateObject> objects;

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (active)
        {
            objects.ForEach(a => a.SetValue(a.LerpFunc(a.BaseValue, a.TargetValue, a.EasingFunction(timer.Percent()))));
            if (timer.TimeLeft <= 0)
            {
                active = false;
            }
        }
    }

    public void Interpolate(float time, params InterpolateObject[] objects)
    {
        this.objects = objects.ToList();
        timer.WaitTime = time;
        timer.Start();
        active = true;
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
                easingFunction) { }

        public InterpolateObject(Action<Vector3> setValue, Vector3 baseValue, Vector3 targetValue, Func<float, float> easingFunction = null) :
            this(
                (a) => setValue((Vector3)a),
                (a, t) => (Vector3)a * t,
                (a, b) => (Vector3)a + (Vector3)b,
                baseValue,
                targetValue,
                easingFunction) { }

        public InterpolateObject(Action<float> setValue, float baseValue, float targetValue, Func<float, float> easingFunction = null) :
            this(
                (a) => setValue((float)a),
                (a, t) => (float)a * t,
                (a, b) => (float)a + (float)b,
                baseValue,
                targetValue,
                easingFunction) { }

        public InterpolateObject(Action<Quaternion> setValue, Quaternion baseValue, Quaternion targetValue, Func<float, float> easingFunction = null) :
            this(
                (a) => setValue((Quaternion)a),
                (a, b, t) => ((Quaternion)a).Slerp((Quaternion)b, t),
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

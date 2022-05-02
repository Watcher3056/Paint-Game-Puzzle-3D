using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pixeye.Actors;
using UnityEngine;
using static TeamAlpha.Source.ProcessorTweens.Tween;

namespace TeamAlpha.Source
{
    public class ProcessorTweens : Processor, ITick, ITickLate, ITickFixed
    {
        public class TweenFloat : Tween
        {
            public AnimationCurve curve;
            public TweenFloat(System.Object target, Action<object> setter, float timeEnd,
                AnimationCurve curve, Action onWin = null, Action onUpdateBefore = null,
                Action onUpdateAfter = null)
            {
                this.target = target;
                this.setter = setter;
                this.startValue = curve.Evaluate(0f);
                this.endValue = curve.keys[curve.keys.Length - 1].value;
                this.timeEnd = timeEnd;
                this.curve = curve;
                this.onWin = onWin;
                this.onUpdateBefore = onUpdateBefore;
                this.onUpdateAfter = onUpdateAfter;
            }
            protected override void HandleUpdate(float delta)
            {
                float totalCurveLength = curve.keys[curve.keys.Length - 1].time;
                setter(curve.Evaluate(TimePassed * (totalCurveLength / timeEnd)));
            }
        }
        public class Tween
        {
            public enum UpdateType { Tick, TickLate, TickFixed }
            public System.Object target;
            public UpdateType updateType;
            public bool unscaledDelta;
            public float timeEnd;
            public System.Object startValue;
            public System.Object endValue;
            public Action<System.Object> setter;
            public Action onWin;
            public Action onUpdateBefore;
            public Action onUpdateAfter;

            public float TimePassed
            {
                get => timePassed;
                set
                {
                    timePassed = value;
                    if (timePassed > timeEnd)
                        timePassed = timeEnd;
                }
            }
            private float timePassed;
            public bool Killed => killed;
            private bool killed;

            public void Update(float delta)
            {
                if (onUpdateBefore != null)
                    onUpdateBefore.Invoke();
                TimePassed += delta;
                HandleUpdate(delta);
                if (onUpdateAfter != null)
                    onUpdateAfter.Invoke();

                if (timePassed == timeEnd)
                    Kill(true);
            }
            protected virtual void HandleUpdate(float delta) { }
            public void Kill(bool Win)
            {
                if (Win)
                {
                    setter(endValue);
                    if (onWin != null)
                        onWin.Invoke();
                }
                killed = true;
            }
        }

        public static ProcessorTweens Default => _default;
        private static ProcessorTweens _default;

        private List<Tween> tweens = new List<ProcessorTweens.Tween>();

        public ProcessorTweens() { _default = this; }
        public void Tick(float delta)
        {
            foreach (Tween t in tweens)
                if (t.updateType == UpdateType.Tick)
                    t.Update(t.unscaledDelta ? Time.deltaTimeFixed : delta);
        }
        public void TickLate(float delta)
        {
            foreach (Tween t in tweens)
                if (t.updateType == UpdateType.TickLate)
                    t.Update(t.unscaledDelta ? Time.deltaTimeFixed : delta);
            tweens.RemoveAll(t => t.updateType != UpdateType.TickFixed && t.Killed);
        }

        public void TickFixed(float delta)
        {
            foreach (Tween t in tweens)
                if (t.updateType == UpdateType.TickFixed)
                    t.Update(t.unscaledDelta ? Time.deltaTimeFixed : delta);
            tweens.RemoveAll(t => t.updateType == UpdateType.TickFixed && t.Killed);
        }

        public static Tween DO(System.Object target, Action<object> setter, float timeEnd,
                AnimationCurve curve, Action onWin = null, Action onUpdateBefore = null,
                Action onUpdateAfter = null)
        {
            Tween tween = new TweenFloat(target, setter, timeEnd, curve,
                onWin, onUpdateBefore, onUpdateAfter);
            _default.tweens.Add(tween);

            return tween;
        }
        public static void DOKill(System.Object target, bool Win)
        {
            Tween tween = _default.tweens.Find(t => t.target == target);
            if (tween != null)
                tween.Kill(Win);
        }
    }
}

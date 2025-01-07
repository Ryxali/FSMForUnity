using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace FSMForUnity.Editor
{
    internal sealed class PulseElement : VisualElement
    {
        private static readonly StyleWithDefault<int> pulseDuration = new StyleWithDefault<int>("--fsmforunity-arrow-pulse-duration", 500);

        private static readonly ObjectPool<PulseElement> pulseElements = new ObjectPool<PulseElement>(() => new PulseElement(), e => e.Reset());

        private readonly IVisualElementScheduledItem pulse;

        private static event System.Action onPulseScheduled = delegate { };

        private const long BasePulseRate = 5;

        private long duration = pulseDuration.defaultValue * BasePulseRate;

        private Func<float, Vector2> interpolator;
        private Func<float> scale;
        private long progress;

        private long pulseRate = BasePulseRate;
        private bool isActive = true;

        private PulseElement()
        {
            style.position = Position.Absolute;
            style.backgroundColor = Color.white;
            AddToClassList("fsmforunity-arrow-pulse");
            pulse = schedule.Execute(Update).Every(16).Until(IsDone);
            pulse.Pause();
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStylesResolved);
        }

        private void OnCustomStylesResolved(CustomStyleResolvedEvent evt)
        {
            duration = evt.customStyle.ValueOrDefault(pulseDuration) * BasePulseRate;
        }

        public bool IsDone()
        {
            return !isActive;
        }

        public void Reset()
        {
            RemoveFromHierarchy();
            interpolator = null;
            progress = 0;
            pulseRate = BasePulseRate;
            isActive = false;
            onPulseScheduled -= OnPulseScheduled;
        }

        private void OnPulseScheduled()
        {
            pulseRate++;
        }

        public static void Run(VisualElement parent, Func<float, Vector2> interpolator, Func<float> scale)
        {
            var pulse = pulseElements.Take();
            parent.Add(pulse);
            pulse.interpolator = interpolator;
            pulse.scale = scale;
            pulse.isActive = true;
            pulse.pulse.Resume();
            onPulseScheduled();
            onPulseScheduled += pulse.OnPulseScheduled;
            pulse.Update(default);
        }

        public void ForceUpdate() => Update(default);

        public void Update(TimerState timerState)
        {
            progress += timerState.deltaTime * pulseRate;
            var dt = progress / (float)duration;
            var point = interpolator(Mathf.Clamp01(dt));
            style.left = new StyleLength(point.x);
            style.top = new StyleLength(point.y);
            style.scale = new Scale(new Vector2(scale(), scale()));
            if (dt >= 1f)
            {
                isActive = false;
                pulseElements.Return(this);
            }
        }
    }
}

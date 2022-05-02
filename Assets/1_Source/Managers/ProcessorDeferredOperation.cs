using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace TeamAlpha.Source
{
    public class ProcessorDeferredOperation : MonoBehaviour
    {
        private class Item
        {
            public Action action;
            public bool triggerOnce;
            public bool isUnscaledDelta;
            public float triggerPeriod;
            public int triggerPeriodFrames;

            public float curPeriodBuffer;
            public int curFramesBuffer;
        }

        public static ProcessorDeferredOperation Default => _default;
        private static ProcessorDeferredOperation _default;

        private List<Item> items = new List<Item>();

        public ProcessorDeferredOperation() => _default = this;
        public void Update()
        {
            float unscaledDeltaTime = Time.unscaledDeltaTime;
            for (int i = 0; i < items.Count; i++)
            {
                Item item = items[i];
                try
                {
                    if (item.triggerPeriod > item.curPeriodBuffer || item.triggerPeriodFrames > item.curFramesBuffer)
                    {
                        item.curPeriodBuffer += item.isUnscaledDelta ? unscaledDeltaTime : LayerDefault.DeltaTime;
                        item.curFramesBuffer++;
                        continue;
                    }
                    else
                    {
                        item.curPeriodBuffer = 0f;
                        item.curFramesBuffer = 0;
                    }
                    item.action.Invoke();
                    if (item.triggerOnce)
                    {
                        items.RemoveAt(i);
                        i--;
                    }
                }
                catch (Exception ex)
                {
                    this.LogError(ex.Message + " ::: " + ex.StackTrace);
                    items.RemoveAt(i);
                    i--;
                }
            }
        }
        public void Add(Action action, bool triggerOnce = false, bool isUnscaledDelta = false, float triggerPeriod = 0f)
        {
            items.Add(new Item { action = action, triggerOnce = triggerOnce, isUnscaledDelta = isUnscaledDelta, triggerPeriod = triggerPeriod });
        }
        public void Add(Action action, bool triggerOnce = false)
        {
            items.Add(new Item { action = action, triggerOnce = triggerOnce, triggerPeriodFrames = 1 });
        }
        public void Add(Action action, bool triggerOnce = false, int triggerPeriodFrames = 1)
        {
            items.Add(new Item { action = action, triggerOnce = triggerOnce, triggerPeriodFrames = triggerPeriodFrames });
        }
        public void Remove(Action action)
        {
            items.Remove(items.Find((x) => { return x.action == action; }));
        }
    }
}

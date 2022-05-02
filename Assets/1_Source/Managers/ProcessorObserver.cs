using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamAlpha.Source
{
    public class ProcessorObserver : MonoBehaviour
    {
        public class Observer
        {
            public Action<object> callback;
            public Func<object> propertySelector;
            public object prevValue;
            public bool invokeOnceOnUpdate;

            public void Kill()
            {
                ProcessorObserver.Default.Kill(this);
            }
        }
        public static ProcessorObserver Default { private set; get; }

        public ProcessorObserver() => Default = this;

        private List<Observer> observers = new List<Observer>();
        private int curIndex = 0;
        // Use this for initialization
        private void Start()
        {

        }

        // Update is called once per frame
        private void Update()
        {
            curIndex = 0;
            while (curIndex < observers.Count)
            {
                Observer observer = observers[curIndex];
                object curValue = observer.propertySelector.Invoke();
                if (!curValue.Equals(observer.prevValue) || observer.invokeOnceOnUpdate)
                {
                    observer.prevValue = curValue;
                    observer.callback.Invoke(curValue);
                    observer.invokeOnceOnUpdate = false;
                }
                curIndex++;
            }
        }
        public Observer Add(Func<object> propertySelector, Action<object> callback, bool invokeOnceOnUpdate)
        {
            Observer observer = new Observer();
            observer.propertySelector = propertySelector;
            observer.prevValue = propertySelector.Invoke();
            observer.callback = callback;
            observer.invokeOnceOnUpdate = invokeOnceOnUpdate;
            observers.Add(observer);
            return observer;
        }
        public void Kill(Observer observer)
        {
            int indexOf = observers.IndexOf(observer);
            if (indexOf <= curIndex)
                curIndex--;
            observer.callback = null;
            observer.prevValue = null;
            observer.propertySelector = null;
            observers.Remove(observer);
        }
    }
}
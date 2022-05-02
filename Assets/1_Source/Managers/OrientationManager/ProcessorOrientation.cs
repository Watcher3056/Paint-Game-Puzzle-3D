using Pixeye.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamAlpha.Source
{
    public partial class ProcessorOrientation : Processor, ITick
    {
        public enum GameMode { None, Vertical, Horizontal }

        public static ProcessorOrientation Default => _default;
        private static ProcessorOrientation _default;
        public bool AutoRotationOn { get { return autoRotationOn; } }
        private bool autoRotationOn;

        public Dictionary<GameMode, StateDefault> statesMap = new Dictionary<GameMode, StateDefault>();

        public static event Action OnOrientationChanged = delegate { };
        public GameMode CurState { get { return curState; } set { curState = value; CheckState(); } }
        private GameMode curState = GameMode.None;
        private GameMode PrevState { get; set; }

        public ProcessorOrientation()
        {
            _default = this;
            statesMap = new Dictionary<GameMode, StateDefault>();
            SetupStateVertical();
            SetupStateHorizontal();
        }

        private void CheckState()
        {
            if (PrevState != CurState)
            {
                this.Log(" CurState: " + CurState + " PrevState: " + PrevState);
                if (statesMap.ContainsKey(CurState) &&
                    statesMap[CurState].Condition((int)PrevState))
                {
                    if (statesMap.ContainsKey(PrevState))
                        statesMap[PrevState].OnEnd((int)CurState);
                    statesMap[CurState].OnStart();

                    PrevState = CurState;
                    OnOrientationChanged.Invoke();
                }
                else
                    CurState = PrevState;
            }
        }
        public void Tick(float delta)
        {
            if (autoRotationOn)
                UpdateOrientation();
        }

        //Legacy indian
        public void UpdateOrientation()
        {
            Vector3 gyro;
            ScreenOrientation curOrientation;

            gyro = Input.acceleration;
            gyro.x = (float)Math.Round((decimal)(gyro.x), 1);
            gyro.y = (float)Math.Round((decimal)(gyro.y), 1);
            gyro.z = (float)Math.Round((decimal)(gyro.z), 1);
            curOrientation = Screen.orientation;


            if (gyro.x >= 0.8f) // Tilt Right
            {
                if (curOrientation == ScreenOrientation.Portrait)
                    curOrientation = ScreenOrientation.LandscapeRight;
                else if (curOrientation == ScreenOrientation.LandscapeRight)
                    curOrientation = ScreenOrientation.PortraitUpsideDown;
                else if (curOrientation == ScreenOrientation.PortraitUpsideDown)
                    curOrientation = ScreenOrientation.LandscapeLeft;
                else if (curOrientation == ScreenOrientation.LandscapeLeft)
                    curOrientation = ScreenOrientation.Portrait;
            }
            else if (gyro.x <= -0.8f) // Tilt Left
            {
                if (curOrientation == ScreenOrientation.Portrait)
                    curOrientation = ScreenOrientation.LandscapeLeft;
                else if (curOrientation == ScreenOrientation.LandscapeLeft)
                    curOrientation = ScreenOrientation.PortraitUpsideDown;
                else if (curOrientation == ScreenOrientation.PortraitUpsideDown)
                    curOrientation = ScreenOrientation.LandscapeRight;
                else if (curOrientation == ScreenOrientation.LandscapeRight)
                    curOrientation = ScreenOrientation.Portrait;
            }
            else if (gyro.y >= 0.6f) // Tilt Forward
            {
                if (curOrientation == ScreenOrientation.Portrait)
                    curOrientation = ScreenOrientation.PortraitUpsideDown;
                else if (curOrientation == ScreenOrientation.PortraitUpsideDown)
                    curOrientation = ScreenOrientation.Portrait;
                else if (curOrientation == ScreenOrientation.LandscapeRight)
                    curOrientation = ScreenOrientation.LandscapeLeft;
                else if (curOrientation == ScreenOrientation.LandscapeLeft)
                    curOrientation = ScreenOrientation.LandscapeRight;
            }
            Screen.orientation = curOrientation;
            CurState = curOrientation == ScreenOrientation.Portrait ||
                curOrientation == ScreenOrientation.PortraitUpsideDown ?
                GameMode.Vertical : GameMode.Horizontal;
        }
        private void HandleOnApplicationFocused(bool focused)
        {
            if (focused) autoRotationOn = DeviceAutoRotationIsOn();
        }
        private bool DeviceAutoRotationIsOn()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
    using (var actClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            var context = actClass.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass systemGlobal = new AndroidJavaClass("android.provider.Settings$System");
            var rotationOn = systemGlobal.CallStatic<int>("getInt", context.Call<AndroidJavaObject>("getContentResolver"), "accelerometer_rotation");
 
            return rotationOn==1;
        }
#endif
            return true;
        }
    }
}

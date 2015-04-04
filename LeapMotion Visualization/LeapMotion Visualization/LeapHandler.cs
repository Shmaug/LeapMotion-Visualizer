using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Leap;

namespace LeapMotion_Visualization
{
    class LeapHandler : ILeapEventDelegate
    {
        private Controller controller;
        private LeapEventListener listener;

        public LeapHandler()
        {
            this.controller = new Controller();
            this.listener = new LeapEventListener(this);
            controller.AddListener(listener);
        }

        delegate void LeapEventDelegate(string EventName);
        public void LeapEventNotification(string EventName)
        {
            switch (EventName)
            {
                case "onInit":
                    break;
                case "onConnect":
                    this.connectHandler();
                    break;
                case "onFrame":

                    break;
            }
        }

        public Frame getFrame()
        {
            Frame frame = controller.Frame();
            return frame;
        }

        void connectHandler()
        {
            this.controller.EnableGesture(Gesture.GestureType.TYPE_CIRCLE);
            this.controller.Config.SetFloat("Gesture.Circle.MinRadius", 40.0f);
            this.controller.EnableGesture(Gesture.GestureType.TYPE_SWIPE);
        }

        public void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    this.controller.RemoveListener(this.listener);
                    this.controller.Dispose();
                }
            }
            finally
            {

            }
        }
    }

    public interface ILeapEventDelegate
    {
        void LeapEventNotification(string EventName);
    }

    public class LeapEventListener : Listener
    {
        ILeapEventDelegate eventDelegate;

        public LeapEventListener(ILeapEventDelegate delegateObject)
        {
            this.eventDelegate = delegateObject;
        }
        public override void OnInit(Controller controller)
        {
            this.eventDelegate.LeapEventNotification("onInit");
        }
        public override void OnConnect(Controller controller)
        {
            this.eventDelegate.LeapEventNotification("onConnect");
        }
        public override void OnFrame(Controller controller)
        {
            this.eventDelegate.LeapEventNotification("onFrame");
        }
        public override void OnExit(Controller controller)
        {
            this.eventDelegate.LeapEventNotification("onExit");
        }
        public override void OnDisconnect(Controller controller)
        {
            this.eventDelegate.LeapEventNotification("onDisconnect");
        }
    }
}

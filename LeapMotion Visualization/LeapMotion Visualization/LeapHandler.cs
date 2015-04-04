using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Leap;

namespace LeapMotion_Visualization
{
    class LeapHandler
    {
        private Controller controller;

        public LeapHandler()
        {
            this.controller = new Controller();

            this.controller.EnableGesture(Gesture.GestureType.TYPE_CIRCLE);
            this.controller.Config.SetFloat("Gesture.Circle.MinRadius", 40.0f);
            this.controller.EnableGesture(Gesture.GestureType.TYPE_SWIPE);
            this.controller.EnableGesture(Gesture.GestureType.TYPE_SCREEN_TAP);
        }

        public Frame getFrame()
        {
            Frame frame = controller.Frame();
            return frame;
        }
    }
}

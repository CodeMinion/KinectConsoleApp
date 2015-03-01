using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KinectConsoleApp
{
    public class MouseController
    {
        // Reference to the current sensor used.
        private KinectSensor m_currSensor;
        
        // We'll need it for mapping the data received
        // to other spaces.
        private CoordinateMapper m_coorMapper;
        
        // This will holds all the bodies tracked.
        private Body[] m_bodies;

        // Used for cooldown between clicks.
        private long m_leftClickDelayMillis = 1000;
        private long m_rightClickDelayMillis = 1000;

        // Used to reduce how ofter we signal for 
        // a left or right click. 
        private Stopwatch m_leftStopWatch;
        private Stopwatch m_rightStopWatch;

        // We'll need these for scaling our hand
        // position to our screen coordinates.
        private int m_frameWidth;
        private int m_frameHeight;

        public MouseController(KinectSensor sensor)
        {
            m_currSensor = sensor;
            m_coorMapper = m_currSensor.CoordinateMapper;

            FrameDescription frameDes = m_currSensor.DepthFrameSource.FrameDescription;
            m_frameWidth = frameDes.Width;
            m_frameHeight = frameDes.Height;

            m_leftStopWatch = new Stopwatch();
            m_leftStopWatch.Start();

            m_rightStopWatch = new Stopwatch();
            m_rightStopWatch.Start();
        }

        public void BR_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            Console.WriteLine("Frame Received");
                
            // Using block automatically disposes the frame.
            // If the frame is not disposed of you won't be getting any
            // more frames of this type. 
            using (BodyFrame frame = e.FrameReference.AcquireFrame())
            {
                // Only perform the minimum operations needed and 
                // release the frame. 
                if (frame != null)
                {
                    if (m_bodies == null)
                    {
                        m_bodies = new Body[frame.BodyCount];
                    }
                
                    frame.GetAndRefreshBodyData(m_bodies);
                    dataReceived = true;
                }
            }

            if (!dataReceived)
                return;

            foreach (Body mainBody in m_bodies)
            {
                if (mainBody == null)
                    continue;

                if (!mainBody.IsTracked)
                    continue;

                Joint leftHand = mainBody.Joints[JointType.HandLeft];

                if (leftHand.TrackingState == TrackingState.Tracked)
                {
                    HandleLeftHand(mainBody, leftHand);
                }
            }
        }

        private void HandleLeftHand(Body mainBody, Joint leftHand)
        {
            CameraSpacePoint handPosition = leftHand.Position;
            DepthSpacePoint dsp = m_coorMapper.MapCameraPointToDepthSpace(leftHand.Position);

            float x = dsp.X / m_frameWidth * Screen.PrimaryScreen.Bounds.Right;
            float y = dsp.Y / m_frameHeight * Screen.PrimaryScreen.Bounds.Bottom;

            Console.WriteLine("Depth X:" + dsp.X + " Depth Y:" + dsp.Y);
            Console.WriteLine("Mouse X:" + x + " Mouse Y:" + y);

            if (mainBody.HandLeftConfidence != TrackingConfidence.High)
                return;

            if (mainBody.HandLeftState == HandState.Open)
            {
                Console.WriteLine("Hand Open");
                Cursor.Position = new Point((int)x, (int)y);

            }
            else if (mainBody.HandLeftState == HandState.Closed)
            {
                Console.WriteLine("Hand Closed");
                if (m_leftStopWatch.ElapsedMilliseconds >= m_leftClickDelayMillis)
                {
                    MouseHandler.PerformLeftClick();
                    m_leftStopWatch.Restart();
                }
            }
            else if (mainBody.HandLeftState == HandState.Lasso)
            {
                Console.WriteLine("Hand Lasso");
                if (m_rightStopWatch.ElapsedMilliseconds >= m_rightClickDelayMillis)
                {
                    MouseHandler.PerformRightClick();
                    m_rightStopWatch.Restart();
                }
            }

        }
    }
}

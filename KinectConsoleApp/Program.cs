using Microsoft.Kinect;
using System;
namespace KinectConsoleApp
{
    class Program{
        static void Main(string[] args){
            KinectSensor sensor = KinectSensor.GetDefault();
            if (sensor != null){
                sensor.Open();
                MouseController mouse = new MouseController(sensor);
                BodyFrameReader bodyReader = sensor.BodyFrameSource.OpenReader();
                bodyReader.FrameArrived += mouse.BR_FrameArrived;
        
                Console.WriteLine("Press Enter To Finish");
                Console.ReadLine();
                sensor.Close();
            }
            
        }
    }
}

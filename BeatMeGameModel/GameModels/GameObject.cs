using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatMeGameModel.EitorModels
{
    public class GameObject
    {
        public double XPosition
        {
            get => xPosition;
            set
            {
                xPosition = value;
                Relocate?.Invoke();
            }
        }

        public double YPosition
        {
            get => yPosition;
            set
            {
                yPosition = value;
                Relocate?.Invoke();
            }
        }
        public double Angle 
        {
            get => angle;
            set
            {
                angle = value;
                Rotate?.Invoke();
            }
        }
        public double XSize 
        {
            get => xSize;
            set
            {
                xSize = value;
                Resize?.Invoke();
            }
        }
        public double YSize 
        {
            get => ySize;
            set
            {
                ySize = value;
                Resize?.Invoke();
            }
        }
        public string Name { get; }

        public event Action Relocate;
        public event Action Rotate;
        public event Action Resize;

        private double xPosition;
        private double yPosition;
        private double angle;
        private double xSize;
        private double ySize;

        public GameObject(double x, double y, double angle, double xSize, double ySize, string name)
        {
            xPosition = x;
            yPosition = y;
            this.angle = angle;
            this.xSize = xSize;
            this.ySize = ySize;
            Name = name;
        }
    }
}

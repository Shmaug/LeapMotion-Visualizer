using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LeapMotion_Visualization.Graphics;

namespace LeapMotion_Visualization.Sim
{
    public enum VirusShape
    {
        Complex,
        Icosahedral
    }
    public enum EnterType
    {
        Trojan,
        Ingestion,
        Injection
    }
    public enum ExitType
    {
        Lysis,
        PinchBud
    }
    public enum VirusType
    {
        RNA,
        Lysogenic
    }
    public class Virus
    {
        public VirusType type;
        public VirusShape shape;
        public EnterType enterType;
        public ExitType exitType;
        public Model model;
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 angularVelocity;
        public Vector3 rotation;
        public Cell target;
        public bool attached;
        public bool awake;
        public bool injecting;
        public Vector3 offset;
        public float timer;
        public object data;
        public bool cameraSubject;

        public Virus(VirusType type, VirusShape shape, EnterType enterType, ExitType exitType, Vector3 pos, World world)
        {
            this.type = type;
            this.shape = shape;
            this.enterType = enterType;
            this.exitType = exitType;
            this.position = pos;
            this.model = (shape == VirusShape.Complex ? Main.main.comVirusModel : Main.main.icoVirusModel);
            world.viruses.Add(this);
        }

        public void reproduce()
        {
            MovableObject obj = new MovableObject(Main.main.cellModel, .075f);
            obj.color = Color.Crimson;
            obj.position = this.target.position;
            this.data = obj;
            this.target.virusTimer = 5f;
            this.target.infected = true;
            this.target.host = this;
        }
    }
}

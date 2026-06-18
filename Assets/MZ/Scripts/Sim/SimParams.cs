using System;

namespace MZ.Sim
{
    [Serializable]
    public class SimParams
    {
        public int FieldLength;
        public int AnimalsCount;
        public int AnimalSpeed;
        public EntityState[] Animals;
        public EntityState[] Feeds;

        public SimParams() { }

        public SimParams(int fieldLength, int animalsCount, int animalSpeed)
        {
            FieldLength = fieldLength;
            AnimalsCount = animalsCount;
            AnimalSpeed = animalSpeed;
        }
    }

    [Serializable]
    public class EntityState
    {
        public int Id;
        public int PositionX;
        public int PositionY;
        public float ColorR;
        public float ColorG;
        public float ColorB;
    }
}

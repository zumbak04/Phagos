[System.Serializable]
public class Neuron
{
    public float value;
    public float[] weights;

    public Neuron(int weightsSize)
    {
        weights = new float[weightsSize];
    }
}

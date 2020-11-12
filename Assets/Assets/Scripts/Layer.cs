[System.Serializable]
public class Layer
{
    public int size;
    public Neuron[] neurons;

    public float[] neuronValues
    {
        get
        {
            float[] outputs = new float[size];
            for (int i = 0; i < size; i++)
            {
                outputs[i] = neurons[i].value;
            }
            return outputs;
        }
        set
        {
            for (int i = 0; i < size; i++)
            {
                neurons[i].value = value[i];
            }
        }
    }

    public Layer(int size, int nextSize)
    {
        this.size = size;
        neurons = new Neuron[size];
        for (int i = 0; i < neurons.Length; i++)
        {
            neurons[i] = new Neuron(nextSize);
        }
    }
}

public class Layer
{
    public int size;
    public float[] neurons;
    public float[,] weights;

    public Layer(int size, int nextSize)
    {
        this.size = size;
        neurons = new float[size];
        weights = new float[size, nextSize];
    }
}

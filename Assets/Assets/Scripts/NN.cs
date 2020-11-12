using System;
using UnityEngine;

[System.Serializable]
public class NN
{
    public Layer[] layers;

    public Layer lastLayer { get { return layers[layers.Length - 1]; } }

    public NN(params int[] sizes)
    {
        layers = new Layer[sizes.Length];
        for (int i = 0; i < sizes.Length; i++)
        {
            int nextSize = 0;
            if(i < sizes.Length - 1) nextSize = sizes[i + 1];
            layers[i] = new Layer(sizes[i], nextSize);
            for (int j = 0; j < sizes[i]; j++)
            {
                for (int k = 0; k < nextSize; k++)
                {
                    layers[i].neurons[i].weights[k] = UnityEngine.Random.Range(-1f, 1f);
                }
            }
        }
    }

    public float[] Move(float[] inputs)
    {
        //Passes inputs to the first layer
        layers[0].neuronValues = inputs;

        for (int i = 1; i < layers.Length; i++) 
        {
            float min = 0f;
            if(i == layers.Length - 1) min = -1f;
            Layer prevLayer = layers[i - 1];
            Layer thisLayer = layers[i];
            for (int j = 0; j < thisLayer.size; j++)
            {
                thisLayer.neurons[j].value = 0;
                for (int k = 0; k < prevLayer.size; k++)
                {
                    thisLayer.neurons[j].value += prevLayer.neurons[k].value * prevLayer.neurons[k].weights[j];
                }
                thisLayer.neurons[j].value = Mathf.Min(1f, Mathf.Max(min, thisLayer.neurons[j].value));
            }
        }

        return lastLayer.neuronValues;
    }

}
/// Bird by Example Neural Net (Recursive Capabilities)
/// 
/// Noah James Burkholder 2020 (MIT License)
/// 
/// Made using the help of various people including:
/// 
/// Andrej Karpathy — "The Unreasonable Effectiveness of Recurrent Neural Networks"
/// Grant Sanderson (3Blue1Brown) — "Deep Learning" series on Youtube.
/// Sebastian Lague — "Neural Networks" series on Youtube.
/// Cary Huang (CaryKH) — Inspiring me to get into machine learning with his project "Evolv.io".

using System;
using UnityEngine;
using UnityEngine.Profiling;
/// <summary>
/// C# Recurrent Neural Net for Bird by Example
/// </summary>
[System.Serializable]
public class NeuralNet {
    // Number of inputs on a bird.
    public static int BirdInputNum = 32;

    // Tags for readability of input integers.
    public enum Perception {
        Clock,
        Health,
        DeltaHealth,
        Hunger,
        DeltaHunger,
        SocietyHoming,
        SquawkMemory,
        GrabbedMemory,
        PunchedMemory,
        FocusDistance,
        FocusDirection,
        FocusHoming,
        FocusEvokes // Must be last. Consists of 20 perceptrons. Resume at n+20.

    }

    // Lets me write to the input layer of the RNN elsewhere in code.
    public void Perceive (Perception perceptionSense, float value) {
        // Write to input layer. 0 = Input.
        layers[0].activations[(int)perceptionSense] = value;

    }

    // Macro process done every frame for processing the RNN I/O.
    public void Think () {
        Profiler.BeginSample ("Thinking");
        netOutput = FeedForward (layers[0]); // Fetch output by passing forward the 0th layer.
        Profiler.EndSample ();

    }

    // Reads output of the bird. For inter-agent learning.
    public float[] GetOutput () {
        if (isFullyInitialized) {
            return layers[layers.Length - 1].activations;

        } else {
            Debug.LogError ("A bird tried to read the outputs too quickly!");
            return new float[layers.Length];
        }
    }

    // Number of bird outputs.
    public static int BirdOutputNum = 10;

    // Names of bird outputs. (Actions)
    public enum Action {
        MoveZ,
        MoveX,
        Rotate,
        Squawk,
        Eat,
        Punch,
        Jump,
        Crouch,
        Use,
        Drop,
        Kiss,
        Birth,
        Heal
    }

    // Fetches various disparate player control intents and writes them to the input layer.
    public void Control () {
        Profiler.BeginSample ("Controlling");

        netOutput = new float[BirdOutputNum];

        netOutput[(int) Action.MoveZ] = PlayerManager.PlayerBird.PlayerIntendedWalk ();
        netOutput[(int) Action.MoveX] = PlayerManager.PlayerBird.PlayerIntendedStrafe ();
        netOutput[(int) Action.Rotate] = PlayerManager.PlayerBird.PlayerIntendedRotation ();
        netOutput[(int) Action.Punch] = PlayerManager.PlayerBird.PlayerIntendedPunch ();
        netOutput[(int) Action.Jump] = PlayerManager.PlayerBird.PlayerIntendedJump ();
        netOutput[(int) Action.Crouch] = PlayerManager.PlayerBird.PlayerIntendedCrouch ();
        netOutput[(int) Action.Use] = PlayerManager.PlayerBird.PlayerIntendedUse ();
        netOutput[(int) Action.Drop] = PlayerManager.PlayerBird.PlayerIntendedDrop ();

        layers[layers.Length - 1].activations = netOutput;

        Profiler.EndSample ();
    }

    // Writes action intents back into the bird.
    public void Intent (Bird bird) {
        if (float.IsNaN (netOutput[0])) return;

        Profiler.BeginSample ("Intending");
        bird.intendedMove.z = netOutput[(int) Action.MoveZ];
        bird.intendedMove.x = netOutput[(int) Action.MoveX];
        bird.intendedRotation = netOutput[(int) Action.Rotate];
        bird.intendedPunch = netOutput[(int) Action.Punch];
        bird.intendedJump = netOutput[(int) Action.Jump];
        bird.intendedCrouch = netOutput[(int) Action.Crouch];
        bird.intendedUse = netOutput[(int) Action.Use];
        bird.intendedDrop = netOutput[(int) Action.Drop];
        Profiler.EndSample ();
    }

    // Fetches the number of layers within the RNN.
    public int GetLayerQuantity () {
        return layers.Length;
    }

    public Layer GetLayer(int index)
    {
        return layers[index];
    }

    // Gets the number of activation nodes within a layer within the RNN.
    public int GetNumNodesAtLayer (int index) {
        return layers[index].activations.Length;
    }

    // Gets the actual activation value at a particular
    public float GetValueAtNeuron (int layerIndex, int neuronIndex) {
        return layers[layerIndex].activations[neuronIndex];
    }
    public float GetValueAtWeight(int layerIndex, int forwardIndex, int backIndex)
    {
        return layers[layerIndex].weights[forwardIndex, backIndex];
    }

    public Bird parentBird; // The bird this RNN belongs to.
    public float[] netOutput; // Cache for always-accessible output.
    public int[] layerDimensions; // Array with number of activations in each layer.
    private Layer[] layers; //layers in the network
    public bool isFullyInitialized = false;
    public bool isFirstHidden = false;
    /// <summary>
    /// Constructor setting up layers
    /// </summary>
    /// <param name="layerDimensions">Layers of this network</param>
    public NeuralNet (int[] layerDimensions, Bird parentBird) {
        this.parentBird = parentBird;
        // Makes a deep copy of layers.
        this.layerDimensions = new int[layerDimensions.Length];
        isFirstHidden = true;
        for (int i = 0; i < layerDimensions.Length; i++) {
            this.layerDimensions[i] = layerDimensions[i];
        }

        // Creates neural layers, starting with input, and ending with output.
        layers = new Layer[layerDimensions.Length];

        for (int i = 0; i < layers.Length; i++) {

            if (i == 0) {
                layers[i] = new Layer (this, layerDimensions[i]); // Input layer.
            } else if (i == layers.Length - 1) {
                layers[i] = new Layer (this, layerDimensions[i - 1], layerDimensions[i], i); // Output layer.
            } else {
                if (isFirstHidden) {
                    layers[i] = new RecurrentLayer (this, layerDimensions[i - 1], layerDimensions[i], i); // Recurrent hidden layers.
                    isFirstHidden = false; // Only the first hidden layer is recurrent.
                } else {
                    layers[i] = new Layer (this, layerDimensions[i - 1], layerDimensions[i], i); // Hidden layers.
                }
            }
        }
        isFullyInitialized = true; // For confirmation.
    }

    /// <summary>
    /// High level feed-forward method for this network. This does one cycle of input-output processing.
    /// </summary>
    /// <param name="inputs">Inputs to be fed forward.</param>
    public float[] FeedForward (Layer inputLayer) {

        // Feed forward input layer.
        layers[1].FeedForward (inputLayer.activations);

        // Feed forward the remaining layers, until output.
        for (int i = 1; i < layers.Length; i++) {
            layers[i].FeedForward (layers[i - 1].activations);

        }

        return layers[layers.Length - 1].activations; // Returns output of output layer.
    }

    // For success metrics.
    private float rewardFactor = 0;

    /// <summary>
    /// Compares the last feedfoward to a target output.
    /// Back-propagates from outputs using derivative Chain Rule to tune weights.
    /// </summary>
    /// <param name="expected">The expected output from the last feed-forward.</param>
    /// <param name="rewardFactor">Precalculated value denoting the interest the other bird has in this bird.</param>
    public void BackPropagate (float[] expected, float rewardFactor) {
        Profiler.BeginSample ("BackPropagation");
        this.rewardFactor = rewardFactor;
        if (isFullyInitialized) {

            // Begin back-propagation.
            for (int i = layers.Length - 1; i >= 0; i--) {
                if (i == layers.Length - 1) {
                    layers[i].BackPropOutput (expected); // Back-propagate the last (output) layer.
                } else {
                    layers[i].BackPropHidden (layers[i + 1].errorGamma, layers[i + 1].weights); // Back-propagate all the hidden layers.
                }
            }

            // For each layer...
            for (int i = 0; i < layers.Length; i++) {
                // Update the weights based on backprop's write to the weightDeltas.
                layers[i].UpdateWeights ();
            }
        }
        Profiler.EndSample ();
    }

    /// <summary>
    /// Each individual layer in the neural network.
    /// </summary>
    public class Layer {
        public NeuralNet parentNet;
        public int numPrecedingActivations; // The number of neurons in the previous layer.
        public int numActivations; // The number of neurons in this layer.

        public int biasIndex; // The index within activations which holds the bias.
        public float[] activations; // The values output by this layer, run through an activation function.
        public float[] precedingActivations; // Activations of preceding layer. Think of these as the 'inputs'.
        public float[, ] weights; // Weights to previous layer from this layer
        public float[, ] weightsDelta; // Change in weights assigned by backprop.
        public float[] errorDelta; // The calculated error from a layer minus the expected output.
        public float[] errorGamma; // The slope or gradient of the error in n-dimensional space. This will create a gradient which can be navigated by the weights.

        public int layerIndex = 0;

        /// <summary>
        /// Constructs input layer.
        /// </summary>
        public Layer () { }

        /// <summary>
        /// Constructs input layer.
        /// </summary>
        /// <param name="parentNet">The network this layer belongs to.</param>
        /// <param name="numActivations">Number of neurons in the input layer.</param>
        public Layer (NeuralNet parentNet, int numActivations) {
            this.parentNet = parentNet;
            this.numActivations = numActivations;
            activations = new float[numActivations];
            errorGamma = new float[numActivations];
            errorDelta = new float[numActivations];
            biasIndex = numActivations - 1; // Get last node.
            activations[biasIndex] = 1f; // Last node is always 1. Never re-assign this!
        }

        /// <summary>
        /// Constructs any layer which isn't the input layer.
        /// </summary>
        /// <param name="parentNet">Reference for ECS connectivity.</param>
        /// <param name="numPrecedingActivations">Number of neurons in the previous layer.</param>
        /// <param name="numActivations">Number of neurons in the current layer.</param>
        /// <param name="layerIndex">The index of the layer, starting from 0.</param>
        public Layer (NeuralNet parentNet, int numPrecedingActivations, int numActivations, int layerIndex) {
            this.parentNet = parentNet;
            this.numPrecedingActivations = numPrecedingActivations;
            this.numActivations = numActivations;
            this.layerIndex = layerIndex;

            // Array initialization.
            activations = new float[numActivations];
            precedingActivations = new float[numPrecedingActivations];
            weights = new float[numActivations, numPrecedingActivations];
            weightsDelta = new float[numActivations, numPrecedingActivations];
            errorGamma = new float[numActivations];
            errorDelta = new float[numActivations];
            biasIndex = numActivations - 1; // Get last node.
            activations[biasIndex] = 1f; // Last node is always 1. Never re-assign this!

            InitilizeWeights (); // Randomize weights.
        }

        public static float Radicalness = 0.5f;
        /// <summary>
        /// Initilize weights between random values.
        /// </summary>
        public virtual void InitilizeWeights () {
            for (int i = 0; i < numActivations; i++) {
                for (int j = 0; j < numPrecedingActivations; j++) {
                    weights[i, j] = (float) UnityEngine.Random.Range (-Radicalness, Radicalness);

                }
            }
        }

        /// <summary>
        /// Feed-forward this layer with a given input.
        /// </summary>
        /// <param name="precedingActivations">The output values of the previous layer.</param>
        public virtual float[] FeedForward (float[] precedingActivations) // Inputs previous layer's neurons.
        {

            this.precedingActivations = precedingActivations; // Shallow copy which can be used for back-propagation.
            // Feed forward.
            for (int i = 0; i < numActivations; i++) // For each neuron in this layer.
            {
                if (i != biasIndex) {
                    activations[i] = 0; // Reset to 0.
                    for (int j = 0; j < numPrecedingActivations; j++) // Then go through every subneuron.
                    {
                        // Take the activation of the previous neuron,
                        // and multiply it by the weight between this neuron and the previous neuron.
                        activations[i] += precedingActivations[j] * weights[i, j];
                    }

                    activations[i] = (float) Math.Tanh (activations[i]); // And put through activation function.
                }
            }

            return activations; // Return layer float array..
        }

        /// <summary>
        /// Return the slope of tanh(x) This is used for gradient descent.
        /// </summary>
        /// <param name="tanhOfX">The tanh(x) for the slope calculation.</param>
        public virtual float DerivativeOfTanH (float tanhOfX) {
            // The derivative of tanh(x) == (1 - (tanh(x) * tanh(x))).
            return Mathf.Clamp (1 - (tanhOfX * tanhOfX), -1, 1);
        }

        /// <summary>
        /// Back propagation for the output layer. This is a simpler version of the BackPropHidden function.
        /// </summary>
        /// <param name="expected">The expected output.</param>
        public virtual void BackPropOutput (float[] expected) {
            //if (expected.Length < error.Length) Debug.LogError("Smaller expected than expected!");
            // Calculate Error by comparing expected result and actual result.
            for (int i = 0; i < numActivations; i++) {
                // At the output layer, calculate the amount of error that exists with the current output.
                errorDelta[i] = activations[i] - expected[i];
            }

            // Calculate errorDelta, (gamma) using derivative of the tanh(x) function.
            for (int i = 0; i < numActivations; i++) {
                errorGamma[i] = errorDelta[i] * DerivativeOfTanH (activations[i]);
            }

            // Use derivative of error and the preceding node to re-assign neural weights.
            for (int i = 0; i < numActivations; i++) {
                for (int j = 0; j < numPrecedingActivations; j++) {
                    weightsDelta[i, j] = errorGamma[i] * precedingActivations[j]; // Change weight deltas for later reassignment.
                }
            }
        }

        /// <summary>
        /// Back propagation for the hidden layers.
        /// </summary>
        /// <param name="errorGammaForward">The gamma of the layer in front of this one (closer to the output). We only need the most immediate layer because of the chain rule.</param>
        /// <param name="weightsFoward">The weights of the layer in front of this one (closer to the output). We only need the most immediate layer because of the chain rule.</param>
        public virtual void BackPropHidden (float[] errorGammaForward, float[, ] weightsFoward) {
            // Calculate new errorDelta using gamma sums of the forward layer.
            for (int i = 0; i < numActivations; i++) {
                errorGamma[i] = 0; // Zero the gamma for this neuron.

                for (int j = 0; j < errorGammaForward.Length; j++) // For all error slopes in the layer ahead of this one. (Could be output layer.)
                {
                    errorGamma[i] += errorGammaForward[j] * weightsFoward[j, i]; // Use the errorSlope and weights of forward (ahead) layer to calculate *this* errorSlope.
                }

                errorGamma[i] *= DerivativeOfTanH (activations[i]); // And put through a sigmoid-type function.
            }

            // Use Gamma + the inputs to re-assign neural weights.
            for (int i = 0; i < numActivations; i++) {
                for (int j = 0; j < numPrecedingActivations; j++) {
                    // Use derivative of error and the preceding node to re-assign neural weights.
                    weightsDelta[i, j] = errorGamma[i] * precedingActivations[j]; // Change weight deltas for later reassignment.

                }
            }
        }

        /// <summary>
        /// Updates weights with values output by the backpropagation sums.
        /// </summary>
        public virtual void UpdateWeights () {
            // Iterate all weights on this layer.
            for (int forwardIndex = 0; forwardIndex < numActivations; forwardIndex++) {
                for (int backIndex = 0; backIndex < numPrecedingActivations; backIndex++) {

                    weights[forwardIndex, backIndex] -= weightsDelta[forwardIndex, backIndex] * (OptionsManager.GlobalNeuroplasticity * parentNet.rewardFactor); // Learn at rate of neuroPlasticity variable.
                }
            }
        }
    }

    public class RecurrentLayer : Layer {

        public float[] recurrentValues; // The values which are ready for comparison in the recurrent nodes.
        public float[] recurrentCache; // Cache for node values in layer before comparison.
        public float[, ] recurrentWeights; // Weights to recurrent layer from this layer
        public float[, ] recurrentWeightsDelta; // Change in weights assigned by backprop.

        /// <summary>
        /// Constructs any layer which isn't the input layer.
        /// </summary>
        /// <param name="parentNet">Reference for ECS connectivity.</param>
        /// <param name="numPrecedingActivations">Number of neurons in the previous layer.</param>
        /// <param name="numActivations">Number of neurons in the current layer.</param>
        /// <param name="layerIndex">The index of the layer, starting from 0.</param>
        public RecurrentLayer (NeuralNet parentNet, int numPrecedingActivations, int numActivations, int layerIndex) {
            this.parentNet = parentNet;
            this.numPrecedingActivations = numPrecedingActivations;
            this.numActivations = numActivations;
            this.layerIndex = layerIndex;

            // Array initialization.
            activations = new float[numActivations];
            precedingActivations = new float[numPrecedingActivations];
            weights = new float[numActivations, numPrecedingActivations];
            weightsDelta = new float[numActivations, numPrecedingActivations];
            errorGamma = new float[numActivations];
            errorDelta = new float[numActivations];
            recurrentValues = new float[numActivations];
            recurrentCache = new float[numActivations];
            recurrentWeights = new float[numActivations, numActivations];
            recurrentWeightsDelta = new float[numActivations, numActivations];
            biasIndex = numActivations - 1; // Get last node.
            activations[biasIndex] = 1f; // Last node is always 1f. Never re-assign this!
            InitilizeWeights (); // Randomize weights.

        }

        /// <summary>
        /// Initilize weights between random values.
        /// </summary>
        public override void InitilizeWeights () {
            for (int forwardIndex = 0; forwardIndex < numActivations; forwardIndex++) {
                for (int backIndex = 0; backIndex < numPrecedingActivations; backIndex++) // Normal weights.
                {
                    weights[forwardIndex, backIndex] = (float) UnityEngine.Random.Range (-Radicalness, Radicalness);

                }
                for (int recurrentIndex = 0; recurrentIndex < numActivations; recurrentIndex++) // Recurrent layer weights.
                {
                    recurrentWeights[forwardIndex, recurrentIndex] = (float) UnityEngine.Random.Range (0, Radicalness);

                }
            }
        }

        /// <summary>
        /// Feed-forward this layer with a given input, and the recurrent (t-1) layer.
        /// </summary>
        /// <param name="precedingActivations">The output values of the previous layer.</param>
        /// <returns></returns>
        public override float[] FeedForward (float[] precedingActivations) // Inputs previous layer's neurons.
        {

            this.precedingActivations = precedingActivations; // Shallow copy which can be used for back-propagation.

            // Feed forward.
            for (int forwardIndex = 0; forwardIndex < numActivations; forwardIndex++) // For each neuron in this layer.
            {
                recurrentCache[forwardIndex] = activations[forwardIndex];
                if (forwardIndex != biasIndex) {
                    activations[forwardIndex] = 0; // Reset to 0.
                    for (int backIndex = 0; backIndex < numPrecedingActivations; backIndex++) // Then go through every contributing previous neuron.
                    {
                        // Take the activation of the previous neuron,
                        // and multiply it by the weight between this neuron and the previous neuron.
                        activations[forwardIndex] += (precedingActivations[backIndex] * weights[forwardIndex, backIndex]);
                    }
                    for (int recurrentIndex = 0; recurrentIndex < numActivations; recurrentIndex++) // Then do the same for the recurrent layer.
                    {
                        // Take the activation of the previous neuron,
                        // and multiply it by the weight between this neuron and the previous neuron.
                        activations[forwardIndex] += (recurrentValues[recurrentIndex] * recurrentWeights[forwardIndex, recurrentIndex]);
                    }
                    activations[forwardIndex] = (float) Math.Tanh (activations[forwardIndex]); // And put through a sigmoid-type function.
                }
            }
            recurrentValues = recurrentCache;

            return activations; // Return layer float array..
        }

        /// <summary>
        /// Gets the TanH derivative of a given value. Recurrent modification.
        /// </summary>
        /// <param name="tanhOfX">The tanh(x) for the slope calculation.</param>
        public override float DerivativeOfTanH(float tanhOfX)
        {
            // The derivative of tanh(x) == (1 - (tanh(x) * tanh(x))).
            return Mathf.Clamp(1 - (tanhOfX * tanhOfX), -1, 1);
        }


        /// <summary>
        /// Back propagation for the output layer of a recurrent layer. This is a simpler version of the BackPropHidden function.
        /// </summary>
        /// <param name="expected">Expected output.</param>
        public override void BackPropOutput (float[] expected) {
            // Calculate Error by comparing expected result and actual result.
            for (int index = 0; index < numActivations; index++) {
                errorDelta[index] = activations[index] - expected[index];
            }

            // Calculate errorDelta, (gamma) using derivative of the tanh(x) function.
            for (int index = 0; index < numActivations; index++) {
                errorGamma[index] = errorDelta[index] * DerivativeOfTanH (activations[index]);
            }

            // Use Gamma + the inputs to re-assign neural weights.
            for (int forwardIndex = 0; forwardIndex < numActivations; forwardIndex++) {
                for (int backIndex = 0; backIndex < numPrecedingActivations; backIndex++) {

                    weightsDelta[forwardIndex, backIndex] = errorGamma[forwardIndex] * precedingActivations[backIndex]; // Change weight deltas for later reassignment.

                }
                for (int recurrentIndex = 0; recurrentIndex < numActivations; recurrentIndex++) {

                    recurrentWeightsDelta[forwardIndex, recurrentIndex] = errorGamma[forwardIndex] * recurrentValues[recurrentIndex]; // Change weight deltas for later reassignment.

                }
            }
        }

        /// <summary>
        /// Back propagation for the hidden layers.
        /// </summary>
        /// <param name="gammaForward">The gamma of the layer in front of this one (closer to the output). We only need the most immediate layer because of the chain rule.</param>
        /// <param name="weightsFoward">The weights of the layer in front of this one (closer to the output). We only need the most immediate layer because of the chain rule.</param>
        public override void BackPropHidden (float[] gammaForward, float[, ] weightsFoward) {
            // Calculate new gamma using gamma sums of the forward layer.
            for (int index = 0; index < numActivations; index++) {
                errorGamma[index] = 0; // Zero the gamma for this neuron.

                for (int forwardIndex = 0; forwardIndex < gammaForward.Length; forwardIndex++) // For all neurons in the layer ahead of this one. (Could be output layer.)
                {
                    errorGamma[index] += gammaForward[forwardIndex] * weightsFoward[forwardIndex, index]; // Use the Gamma and Weights of forward (ahead) layer to calculate *this* Gamma.
                }

                errorGamma[index] *= DerivativeOfTanH (activations[index]); // And put through a sigmoid-type function.
            }

            // Use Gamma + the inputs to re-assign neural weights.
            for (int forwardIndex = 0; forwardIndex < numActivations; forwardIndex++) {
                for (int backIndex = 0; backIndex < numPrecedingActivations; backIndex++) {
                    weightsDelta[forwardIndex, backIndex] = errorGamma[forwardIndex] * precedingActivations[backIndex]; // Change weight deltas for later reassignment.
                }

                for (int recurrentIndex = 0; recurrentIndex < numActivations; recurrentIndex++) {
                    recurrentWeightsDelta[forwardIndex, recurrentIndex] = errorGamma[forwardIndex] * recurrentValues[recurrentIndex]; // Change weight deltas for later reassignment.
                }
            }
        }

        /// <summary>
        /// Updates weights with values output by the backpropagation sums.
        /// </summary>
        public override void UpdateWeights () {
            // Iterate all weights on this layer.
            for (int forwardIndex = 0; forwardIndex < numActivations; forwardIndex++) {
                for (int backIndex = 0; backIndex < numPrecedingActivations; backIndex++) {

                    weights[forwardIndex, backIndex] -= weightsDelta[forwardIndex, backIndex] * (OptionsManager.GlobalNeuroplasticity * parentNet.rewardFactor); // Learn at rate of neuroPlasticity variable.
                }
            }
        }
    }
}
import json
import random
import tensorflow as tf
import numpy as np

def generate_random_color():
    return [random.random(), random.random(), random.random(), 1]

def create_neural_network_json_file(layers):
    architecture = {
        "nodes": {},
        "edges": [],
        "edge_thickness": [],
        "colors": []
    }

    for i in range(len(layers)):
        for j in range(len(layers[i])):
            curr_start = f"{i}_{j}_{j}"
            
            if i < len(layers) - 1:
                for k in range(len(layers[i + 1])):
                    curr_end = f"{i+1}_{j}_{k}"
                    architecture["edges"].append(["Node_" + curr_start,"Node_" +  curr_end])
                    architecture["edge_thickness"].append(layers[i][j][k])
    total_layers = len(layers)
    for layer_idx, layer_features in enumerate(layers):
        total_features = len(layer_features)
        layer_color = generate_random_color()
        
        feature_thickness = 1

        if layer_idx == total_layers - 1:
            layer_prev = len(layers[layer_idx - 1])
            feature_thickness = max(layer_prev, total_features)
        elif layer_idx > 0:
            layer_prev = len(layers[layer_idx - 1])
            layer_next = len(layers[layer_idx + 1])
            feature_thickness = max(layer_prev, layer_next, total_features)
        elif layer_idx == 0:
            feature_thickness = total_features

        architecture["colors"].extend([layer_color] * total_features * feature_thickness)

        for feature_idx in range(feature_thickness):
            node_name = f"Node_{layer_idx}_{feature_idx}"
            for i in range(total_features):
                architecture["nodes"][node_name + "_" + str(i)] = [layer_idx * 2, feature_idx, i + 1]

    return architecture

def create_train_neural_network(layers, input_data, target_data, epochs=100):
    model = tf.keras.Sequential(layers)

    model.compile(optimizer='adam', loss='mean_squared_error')

    # Train the model using the input_data and target_data
    model.fit(input_data, target_data, epochs=epochs, verbose=0)

    # Create an array to store weights
    weights_array = []

    for layer in model.layers:
        if hasattr(layer, 'weights'):
            weights = layer.get_weights()[0]
            weights_array.append(weights.tolist())

    # Add dummy weights for the final layer
    weights_array.append([[0]] * weights_array[-1][0].__len__())

    # Print and return the weights array
    print("Weights array:")
    for layer_idx, layer in enumerate(weights_array):
        print(f"Layer {layer_idx}:")
        for feature_idx, feature_list in enumerate(layer):
            print(f"  Feature {feature_idx}:")
            for weight_idx, weight in enumerate(feature_list):
                print(f"    Weight {weight_idx}: {weight}")

    return weights_array

def generate_json_file(architecture):
    architecture = create_neural_network_json_file(architecture)
    # Convert to JSON format
    json_string = json.dumps(architecture, indent=4)

    # Save the JSON data to a file
    output_file_path = "Datasets/DeepLearning.json"
    with open(output_file_path, "w") as output_file:
        output_file.write(json_string)

    print(f"JSON data saved to {output_file_path}")


if __name__ == "__main__":
    # Define the architecture layers
    layers = [
        tf.keras.layers.Dense(2, activation='relu', input_shape=(2,)),
        tf.keras.layers.Dense(4, activation='relu'),
        tf.keras.layers.Dense(4, activation='relu'),
        tf.keras.layers.Dense(1, activation='linear')
    ]

    # Generate example input and target data
    input_data = np.random.rand(100, 2)  # Replace this with your actual input data
    target_data = np.random.rand(100, 1)  # Replace this with your actual target data

    # Create the neural network architecture, train it, and get the weights
    trained_weights = create_train_neural_network(layers, input_data=input_data, target_data=target_data)
    generate_json_file(trained_weights)
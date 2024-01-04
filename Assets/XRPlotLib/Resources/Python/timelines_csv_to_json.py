import csv
import json
import ast
import argparse
import timelines_csv_to_json_scoringFunctions as scoringFunctions
from sklearn.preprocessing import minmax_scale
import numpy as np

def convert_csv_to_connected_json(csv_file, json_file, json_non_connected, json_white, z_multiplier, additional_csv_file):

    data = {'points': [], 'labels': [], 'colors': []}
    all_timeseries = []

    with open(csv_file, 'r') as file:
        csv_reader = csv.reader(file)
        next(csv_reader)  # Skip the header row

        for row in csv_reader:
            x, y, z, label, color = row
            z = float(z) * z_multiplier
            data['points'].append([float(x), float(y), z])
            data['labels'].append(label)
            data['colors'].append(ast.literal_eval(color))

    # Use json dump to write to file and re-read it to ensure the data is formatted correctly
    with open(json_non_connected, 'w') as file:
        json.dump(data, file)

    with open(json_non_connected, 'r') as f:
        data = json.load(f)

    # Create the nodes, edges and edge thicknesses
    nodes = {}
    edges = []
    edge_thickness = []

    # Colors for different uses, one for displaying the plot with box plots and one for not
    colors_quantiles = data['colors']
    colors_zscores = [[1, 1, 1, 1]] * len(colors_quantiles)

    # Create the labels included in the input file
    labels = data['labels']

    # Create a list of all the labels
    dataset_labels = []

    # Include all points from the data set
    points = data['points']

    # For splitting the time series and processing each one separately
    current_timeseries = []
    current_substring = ""


    for i in range(len(labels)):
        label = labels[i]
        point = points[i]

        # Split Experiments and Groups by looking at the label
        # Could be improved by looking at Z, but used this algorithm to split
        # the coloring too at some point so it stayed like this
        current_substring = label.split(' ')[0]
        
        if i > 1:
            prev_label = labels[i-1]
            prev_substring = prev_label.split(' ')[0]
            
            if current_substring != prev_substring:
                all_timeseries.append(current_timeseries.copy())
                current_timeseries = []
                dataset_labels.append(prev_substring)
        
        current_timeseries.append(point)

        # Connect the nodes to simulate time series
        nodes[label] = point

        if i < len(labels) - 1:
            next_label = labels[i+1]
            next_substring = next_label.split(' ')[0]
            if current_substring == next_substring:
                edge = [label, next_label]
                edges.append(edge)
                edge_thickness.append(0.2)

    dataset_labels.append(current_substring)
    all_timeseries.append(current_timeseries.copy())

    # Create a temporary list to hold the scores for the partial timelines
    partial_scores = []

    for i, timeseries in enumerate(all_timeseries):
        # Calculate the score up to each point in the time series
        for j in range(len(timeseries)):
            # Ensure that there are at least 3 points to perform KMeans
            if j+1 >= 3:
                partial_timeline = [sublist[:j] for sublist in all_timeseries]

                # Scoring results from weighted result of knn and novel algorithm
                score = scoringFunctions.score_sum(i, partial_timeline, 1, 0)

                partial_scores.append(score)
            else:
                # If we have less than 3 points, append a default score or skip
                partial_scores.append(0)  # Assuming 0 is the default or placeholder score
    
    partial_scores = normalize(partial_scores)

    k = 0
    # Now, color the nodes based on the partial scores
    for score in partial_scores:
        colors_zscores[k] = interpolate_color([1, 1, 0, 1], [1, 0, 0, 1], score)
        k += 1

    nodes_quantiles = nodes.copy()
    nodes_zscores = nodes.copy()

    edges_quantiles = edges.copy()
    edges_zscores = edges.copy()

    edge_thickness_quantiles = edge_thickness.copy()
    edge_thickness_zscores = edge_thickness.copy()

    colors_quantiles = colors_zscores.copy()

    # Import quantile data and generate box plots
    with open(additional_csv_file, 'r') as file:
        csv_reader = csv.reader(file)
        next(csv_reader)  # Skip the header row

        for row in csv_reader:
            x, min_val, first_val, median_val, fourth_val, max_val, iqr, geom_mean, z, dataset, color = row

            y_values = [float(min_val), float(first_val), float(median_val), float(fourth_val), float(max_val)]
            y_labels = ["Min", "1st", "Median", "4th", "Max"]

            for y, y_label in zip(y_values, y_labels):
                point_label = f"{dataset}_{y_label}"
                point = [float(x), y, float(z) * z_multiplier]
                nodes_quantiles[point_label] = point
                if y_label in ["1st", "4th"]:
                    original_color = ast.literal_eval(color)
                    lighter_color = [(c + 0.2) if c <= 0.8 else 1.0 for c in original_color]
                    colors_quantiles.append(lighter_color)
                else:
                    colors_quantiles.append(ast.literal_eval(color))
                
            edges_quantiles.append([f"{dataset}_Min", f"{dataset}_1st"])
            edges_quantiles.append([f"{dataset}_1st", f"{dataset}_4th"])
            edges_quantiles.append([f"{dataset}_4th", f"{dataset}_Max"])
            edge_thickness_quantiles.extend([0.1, 1, 0.1])

    # Generate Time series with Box plot Visualization
    output_data = {'nodes': nodes_quantiles, 'edges': edges_quantiles, 'edge_thickness': edge_thickness_quantiles, 'colors': colors_quantiles}
    with open(json_file, 'w') as f:
        json.dump(output_data, f)
    
    # Generate Time series with Score Coloring Visualization
    output_data_white = {'nodes': nodes_zscores, 'edges': edges_zscores, 'edge_thickness': edge_thickness_zscores, 'colors': colors_zscores}
    with open(json_white, 'w') as f:
        json.dump(output_data_white, f)

def normalize(lst):
    min_value = min(lst)
    max_value = max(lst)
    range_value = max_value - min_value
    return [(value - min_value) / range_value for value in lst]

def interpolate_color(start_color, end_color, factor):
    # Ensure factor is within [0, 1]
    factor = max(0.0, min(1.0, factor))
    
    # Interpolate each RGB component separately
    interpolated_color = [start + (end - start) * factor for start, end in zip(start_color, end_color)]
    
    return interpolated_color

def main():
    parser = argparse.ArgumentParser(description='Convert CSV to connected JSON with Z-axis multiplier and additional CSV data.')
    parser.add_argument('csv_file', help='Input CSV file name')
    parser.add_argument('json_file', help='Output connected JSON file name')
    parser.add_argument('json_non_connected', help='Output non-connected JSON file name')
    parser.add_argument('json_white', help='Output connected but white and highlighted JSON file name')
    parser.add_argument('z_multiplier', type=float, help='Multiplier for Z-axis values')
    parser.add_argument('additional_csv_file', help='Additional CSV file name')

    args = parser.parse_args()

    convert_csv_to_connected_json(args.csv_file, args.json_file, args.json_non_connected, args.json_white, args.z_multiplier, args.additional_csv_file)

if __name__ == '__main__':
    main()
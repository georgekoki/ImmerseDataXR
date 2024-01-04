import pandas as pd
import argparse
import numpy as np

def calculate_quantiles(input_file, output_file):
    # Load your data from the input CSV file
    data = pd.read_csv(input_file)

    # Create an empty DataFrame for the quantiles
    quantiles = pd.DataFrame(columns=['X', 'Min', '1st', 'Median', '4th', 'Max', 'IQR', 'GeometricMean', 'Z', 'dataset', 'color'])

    # Get unique IDs from the data
    unique_ids = data['Z'].unique()

    # Loop through each ID and calculate quantiles and other statistics
    for id in unique_ids:
        id_data = data[data['Z'] == id]
        dataset = id_data['label'].iloc[0]  # Assuming 'dataset' is a column in the input file
        color = id_data['color'].iloc[0]      # Assuming 'color' is a column in the input file
        quantiles.loc[id] = [-1,  # Constant value for X
                             id_data['Y'].min(),          # Min
                             id_data['Y'].quantile(0.25),  # 1st Quantile (25th percentile)
                             id_data['Y'].median(),       # Median (50th percentile)
                             id_data['Y'].quantile(0.75),  # 4th Quantile (75th percentile)
                             id_data['Y'].max(),          # Max
                             id_data['Y'].quantile(0.75) - id_data['Y'].quantile(0.25),  # IQR
                             np.exp(np.mean(np.log(id_data['Y']))),  # Geometric Mean
                             id,                          # Z
                             dataset.split(" ")[0],                     # dataset
                             color]                       # color

    # Save the results to the output CSV file
    quantiles.to_csv(output_file, index=False)

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description='Calculate quantiles and statistics from a CSV file.')
    parser.add_argument('input_file', help='Input CSV file')
    parser.add_argument('output_file', help='Output CSV file')
    args = parser.parse_args()

    calculate_quantiles(args.input_file, args.output_file)

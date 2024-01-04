import numpy as np
from sklearn.cluster import KMeans
from sklearn.metrics import pairwise_distances_argmin_min
import numpy as np

# Clustering based scoring
def score_clustering_based(target_index, all_series):
    # Ensure all series have the same length
    all_series = pad_series(all_series)
    
    # Extract only Y-values for KMeans
    all_values_y = [series[:,1] for series in all_series]

    # Check for consistent series lengths
    series_lengths = {len(series) for series in all_values_y}
    if len(series_lengths) > 1:
        raise ValueError("All series must be of same length for KMeans clustering.")
    
    kmeans = KMeans(n_clusters=3).fit(all_values_y)
    
    # Ensuring target_series Y-values are 2D array for pairwise_distances_argmin_min
    target_series_y = all_values_y[target_index].reshape(1, -1)
    
    closest, _ = pairwise_distances_argmin_min(kmeans.cluster_centers_, target_series_y)
    
    target_cluster = kmeans.labels_[closest[0]]
    
    within_cluster_distances = [calculate_distance(all_series[target_index], series) for idx, series in enumerate(all_series) if kmeans.labels_[idx] == target_cluster]
    
    outlier_score = np.mean(within_cluster_distances)

    return outlier_score / 100

def pad_series(all_series):
    max_length = max(len(series) for series in all_series)
    padded_series = []
    for series in all_series:
        series_length = len(series)
        # Ensure each series is a NumPy array
        series_np = np.array(series)
        # Generate the padding
        padding = np.zeros((max_length - series_length, series_np.shape[1]))
        # Add the padding to the original series
        padded = np.vstack([series_np, padding])
        padded_series.append(padded)
    return padded_series

def calculate_distance(series1, series2):
    return np.linalg.norm(series1-series2)

# Outlier based scoring
def score_outlier_based(target_index, all_series):
    # Create stats
    stats_series = []
    for series in all_series:
        scores = time_series_statistics(series)
        stats_series.append(scores)
    stats_series = np.array(stats_series)

    # Get low and upper quartiles of stats
    low_quartiles, upper_quartiles = evaluate_quartiles(stats_series)

    outlier_rank = check_for_outliers(stats_series, low_quartiles, upper_quartiles)

    return outlier_rank[target_index]

def check_for_outliers(data, low_quartile, upper_quartile, alpha=0.2):
    how_many_cols = len(data[0])

    final_col = [0] * len(data)

    for i in range(how_many_cols):
       curr_col = data[:,i]
       height_of_col = len(curr_col)
       for j in range(height_of_col):
           if(isOutlier(curr_col[j], upper_quartile[i], low_quartile[i], alpha)):
               final_col[j] += 1

    return final_col

def isOutlier(value, upper, low, alpha):
    iqr = upper - low
    lower_bound = low - alpha * iqr
    upper_bound = upper + alpha * iqr

    return (value < lower_bound) | (value > upper_bound)

def evaluate_quartiles(data_set):
    num_values_per_sample = len(data_set[0])
    low_quartiles = []
    upper_quartiles = []
    for i in range(num_values_per_sample):
        low_quartile, upper_quartile = np.percentile(data_set[:,i], [25, 75]) # for i col
        low_quartiles.append(low_quartile)
        upper_quartiles.append(upper_quartile)
    return low_quartiles, upper_quartiles

def time_series_statistics(time_series):
    # Convert the input to a numpy array to ensure compatibility with numpy functions
    time_series = np.array(time_series)
    
    # Calculate required statistics
    min_value = np.min(time_series)
    q1 = np.percentile(time_series, 25)
    median = np.median(time_series)
    q3 = np.percentile(time_series, 75)
    max_value = np.max(time_series)
    iqr = q3 - q1
    
    return [min_value, q1, median, q3, max_value, iqr]

# Sum of Clustering and outlier based scoring
def score_sum(target_series, all_series, w1, w2):
    assert w1 + w2 <= 1

    KMeansScore = score_clustering_based(target_series, all_series)
    OutlierScore = score_outlier_based(target_series, all_series)

    return w1 * KMeansScore + w2 * OutlierScore


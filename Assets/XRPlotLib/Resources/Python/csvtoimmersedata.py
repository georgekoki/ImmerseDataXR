import csv
import json
from datetime import datetime

def magnitude_to_color(magnitude):
    red = min(1.0, magnitude / 10.0)
    blue = 1.0 - red
    return [red, 0.0, blue, 1.0]

def convert_csv_to_json(input_file, output_file):
    data = {
        "points": [],
        "colors": [],
        "colorsOutline": [],
        "labels": []
    }
    
    with open(input_file, 'r') as infile:
        reader = csv.reader(infile)
        next(reader)  # Skip header
        
        for row in reader:
            time_str, lat, lon, depth, mag = row
            
            # Parse the timestamp
            timestamp = datetime.strptime(time_str.strip(), "%Y/%m/%d %H:%M:%S.%f")
            year, month, day = timestamp.year, timestamp.month, timestamp.day
            hour, minute, second = timestamp.hour, timestamp.minute, timestamp.second
            
            # Corrected for Unity (Lon -> X, Depth -> -Y, Lat -> -Z)
            point = [float(lon), -float(depth), float(lat)]
            color = magnitude_to_color(float(mag))
            outline = [0, 0, 0, 1]
            label = f"{day}/{month}/{year}, {hour}:{minute}:{second} Magnitude: {mag}"
            
            data["points"].append(point)
            data["colors"].append(color)
            data["colorsOutline"].append(outline)
            data["labels"].append(label)
    
    # Add predefined points with specified colors
    predefined_points = [
        [24.7192, -0, 35.9691],
        [24.7192, -0, 37.2872],
        [26.4331, -0, 37.2872],
        [26.4331, -0, 35.9691]
    ]
    predefined_colors = [
        [1.0, 0.0, 0.0, 1.0],  # Red
        [0.0, 1.0, 0.0, 1.0],  # Green
        [0.0, 0.0, 1.0, 1.0],  # Blue
        [1.0, 1.0, 1.0, 1.0]   # White
    ]
    
    for point, color in zip(predefined_points, predefined_colors):
        data["points"].append(point)
        data["colors"].append(color)
        data["colorsOutline"].append([0, 0, 0, 1])
        data["labels"].append("Predefined Point")
    
    with open(output_file, 'w') as outfile:
        json.dump(data, outfile, indent=4)

# Example usage
convert_csv_to_json("quakes.csv", "output.json")

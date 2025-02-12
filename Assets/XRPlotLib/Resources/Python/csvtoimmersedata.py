import csv
import json

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
            year, month, day, hour, minute, second, lat, lon, dep, mag, rms, *_ = row
            
            point = [float(lat), -float(dep), float(lon)]
            color = magnitude_to_color(float(mag))
            outline = [0, 0, 0, 1]
            label = f"{day}/{month}/{year}, {hour}:{minute}:{second} Magnitude: {mag}, RMS: {rms}"
            
            data["points"].append(point)
            data["colors"].append(color)
            data["colorsOutline"].append(outline)
            data["labels"].append(label)
    
    with open(output_file, 'w') as outfile:
        json.dump(data, outfile, indent=4)

# Example usage
convert_csv_to_json("quakes.csv", "output.json")

import csv

def convert_to_csv(input_file, output_file):
    with open(input_file, 'r') as infile, open(output_file, 'w', newline='') as outfile:
        writer = csv.writer(outfile)
        
        # Write header row
        header = ["Year", "Month", "Day", "Hour", "Minute", "Second", "Latitude", "Longitude", "Depth", "Magnitude", "RMS", "dx", "dy", "dz", "Np", "Na", "Gap"]
        
        for line in infile:
            parts = line.split()
            
            # Ensure we have the correct number of columns
            if len(parts) == len(header):
                writer.writerow(parts)
            else:
                print(f"Skipping malformed line: {line.strip()}")

# Example usage
convert_to_csv("quakes.txt", "quakes.csv")

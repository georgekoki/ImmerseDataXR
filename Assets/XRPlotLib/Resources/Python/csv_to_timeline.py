import pandas as pd

# Read the CSV file into a Pandas DataFrame
df = pd.read_csv('C:\\Users\\ZiKok\\Desktop\\n14data.csv')

# Group the DataFrame by both 'Z' and 'X' columns and sum the 'Y' values for each group
df['Y'] = df.groupby(['Z', 'X'])['Y'].transform('sum')

# Drop duplicate rows based on the 'Z' and 'X' columns, keeping the first occurrence
df = df.drop_duplicates(subset=['Z', 'X'], keep='first')

# Reset the index
df = df.reset_index(drop=True)

# Save the modified DataFrame to a new CSV file
df.to_csv('C:\\Users\\ZiKok\\Desktop\\n22_fixed.csv', index=False)
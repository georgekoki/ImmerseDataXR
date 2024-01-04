import networkx as nx
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D
import json
import random
import matplotlib.cm as cm
import matplotlib.colors as mcolors


def generate_random_3Dgraph(n_nodes, radius, percent_connected, seed=None):
    if seed is not None:
        random.seed(seed)

    # Generate a dict of positions
    pos = {i: (random.uniform(0, 1), random.uniform(0, 1), random.uniform(0, 1)) for i in range(n_nodes)}

    # Create random 3D network
    G = nx.random_geometric_graph(n_nodes, radius, pos=pos)

    edges = list(G.edges())

    # Shuffle the list of edges randomly
    random.shuffle(edges)

    # Choose a percentage of edges to delete
    percentage_to_delete = 1 - percent_connected
    num_edges_to_delete = int(len(edges) * percentage_to_delete)

    # Select a node for maximum edges
    max_edges_node = random.choice(list(G.nodes()))

    # Add edges
    for node in G.nodes():
        if node == max_edges_node:
            # Connect the maximum edges node to all other nodes
            G.add_edges_from([(node, other_node) for other_node in G.nodes() if other_node != node])

    # Delete random edges from the graph
    for edge in edges[:num_edges_to_delete]:
        G.remove_edge(*edge)

    return G


def interpolate_color(color1, color2, value):
    """
    Interpolates between two colors based on a given value.

    Args:
        color1 (str or tuple): The first color in RGB format (e.g., 'red', (1, 0, 0)).
        color2 (str or tuple): The second color in RGB format (e.g., 'blue', (0, 0, 1)).
        value (float): The interpolation value between 0 and 1.

    Returns:
        tuple: The interpolated color in RGB format as a tuple.
    """
    if isinstance(color1, str):
        color1 = mcolors.to_rgb(color1)
    if isinstance(color2, str):
        color2 = mcolors.to_rgb(color2)

    r = (1 - value) * color1[0] + value * color2[0]
    g = (1 - value) * color1[1] + value * color2[1]
    b = (1 - value) * color1[2] + value * color2[2]

    return r, g, b


# Create a directed graph
G = generate_random_3Dgraph(50, 10, 0.05)

# Get node positions
pos = nx.get_node_attributes(G, 'pos')

# Plotting the graph
fig = plt.figure()
ax = fig.add_subplot(111, projection='3d')
for node, position in pos.items():
    x, y, z = position
    ax.scatter(x, y, z, c='b', s=100, edgecolors='k')
    ax.text(x, y, z, node, fontsize=12, ha='center')

for edge in G.edges():
    u, v = edge
    x1, y1, z1 = pos[u]
    x2, y2, z2 = pos[v]
    ax.plot([x1, x2], [y1, y2], [z1, z2], 'r-')

# Set axis labels
ax.set_xlabel('X-axis')
ax.set_ylabel('Y-axis')
ax.set_zlabel('Z-axis')

# Calculate the minimum and maximum degrees of the original graph
original_degrees = dict(G.degree())
min_edges = min(original_degrees.values())
max_edges = max(original_degrees.values())

# Create a color gradient between light blue and soft red based on the number of edges each node has
color_map = cm.ScalarMappable(cmap='coolwarm')

# Normalize the number of edges for each node between 0 and 1
normalized_edges = [(original_degrees[node] - min_edges) / (max_edges - min_edges) for node in G.nodes()]

# Assign colors based on the normalized number of edges
colors = [interpolate_color((0.27, 0.79, 1), (1, 0.10, 0.41), val) for val in normalized_edges]

# Randomize the thickness of each edge from 0.2 to 1 with 2 decimal points
edge_thickness = [round(random.uniform(0.2, 1.0), 2) for _ in G.edges()]


# Add the colors and edge thickness to the data dictionary
data = {
    "nodes": {f"Edges connected: {node}": list(value) for node, value in pos.items()},
    "edges": [[f"Edges connected: {u}", f"Edges connected: {v}"] for u, v in G.edges()],
    "colors": [[c[0], c[1], c[2], 1] for c in colors],
    "edge_thickness": edge_thickness
}

# Convert the data dictionary to a JSON object
json_object = json.dumps(data, indent=4)

# Save the JSON object to a file
with open("Datasets/random_graph.json", "w") as outfile:
    outfile.write(json_object)

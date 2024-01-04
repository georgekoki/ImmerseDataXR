import numpy as np
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D
from sklearn import neighbors
from sklearn import datasets
import json
import numpy as np
from sklearn import datasets
from sklearn.neighbors import KNeighborsClassifier

# Load the iris dataset
iris = datasets.load_iris()

# Split the data into training and testing sets
X = iris.data
y = iris.target

# Create a random dataset
np.random.seed(0)
X_random = np.random.uniform(10, -10, (500, 4))

# Train the KNN model with the iris data
knn = KNeighborsClassifier(n_neighbors=3)
knn.fit(X, y)

# Predict the class labels for the random dataset
y_pred = knn.predict(X_random)

print("Predicted class labels for the random dataset:", y_pred)

colors_target = []
colorsOutline_target = []
for cl in y:
    if cl == 0:
        colors_target.append([1, 0, 0, 1])
        colorsOutline_target.append([0, 0, 0, 0])
    elif cl == 1:
        colors_target.append([0, 1, 0, 1])
        colorsOutline_target.append([0, 0, 0, 0])
    elif cl == 2:
        colors_target.append([0, 0, 1, 1])
        colorsOutline_target.append([0, 0, 0, 0])

colors_pred = []
for cl in y_pred:
    if cl == 0:
        colors_pred.append([1, 0, 0, 0.7])
    elif cl == 1:
        colors_pred.append([0, 1, 0, 0.7])
    elif cl == 2:
        colors_pred.append([0, 0, 1, 0.7])

colorsOutline_pred = [[0, 0, 0, 0]] * len(colors_pred)
        
data = {
    "points" :  X.tolist(), #+ X_random.tolist(),
    "colors" :  colors_target, #+ colors_pred,
    "colorsOutline":  colors_target, #+ colorsOutline_pred,
    "labels" : ["Class = " + str(num) for num in iris.target.tolist()] #+ (["Test Data"] * len(colors_pred))
}

json_object = json.dumps(data, indent=4)

with open("sample_data.json", "w") as outfile:
    outfile.write(json_object)
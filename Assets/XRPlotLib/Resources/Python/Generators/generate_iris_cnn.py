import numpy as np
import tensorflow as tf
from sklearn import datasets
from sklearn.model_selection import train_test_split
from tensorflow.keras.utils import to_categorical
from tensorflow.keras.models import Sequential
from tensorflow.keras.layers import Dense, Conv2D, Flatten
import json

# Load the iris dataset
iris = datasets.load_linnerud()

# Split the data into training and testing sets
X = iris.physiological
y = iris.target

# One-hot encode the target labels
y = to_categorical(y)

# Split the data into training and testing sets
X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2)
X_test_temp = X_test
# Reshape the data to fit the input shape of a CNN
X_train = np.reshape(X_train, (X_train.shape[0], X_train.shape[1], 1))
X_test = np.reshape(X_test, (X_test.shape[0], X_test.shape[1], 1))

# Define the model
model = Sequential()
model.add(Conv2D(64, kernel_size=3, activation='relu', input_shape=(X_train.shape[1], X_train.shape[2], 1), padding='same'))
model.add(Flatten())
model.add(Dense(3, activation='softmax'))

# Compile the model
model.compile(optimizer='adam', loss='categorical_crossentropy', metrics=['accuracy'])

# Use the model to predict the labels for the test data
y_pred = model.predict(X_test)

# Convert the one-hot encoded labels to their original form
y_pred = np.argmax(y_pred, axis=1)

# Train the model
model.fit(X_train, y_train, epochs=10)

data = {
    "points" : X_test_temp.tolist(),
    "colors" : y_pred.tolist()
}

json_object = json.dumps(data, indent=4)

with open("sample.json", "w") as outfile:
    outfile.write(json_object)

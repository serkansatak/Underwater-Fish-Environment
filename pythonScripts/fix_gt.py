import numpy as np

annotations = np.genfromtxt("data/gt.csv", delimiter=",")
annotations = annotations[:, :-1]
np.savetxt("data/gt.csv", annotations, delimiter=",")
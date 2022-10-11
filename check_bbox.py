from os import read
from re import A
from unittest.main import MAIN_EXAMPLES
import cv2 as cv
import numpy as np
import random as rnd

def read_annotations():
    annotations = np.genfromtxt("data/gt.csv", delimiter=",")
    #print(annotations)
    #print(np.shape(annotations))
    annotations = annotations[annotations[:,0] == 1]
    annotations = annotations[:, :-1]
    print(annotations)
    return annotations

def draw_rectangles(img, annotations):
    for i in range(0,np.shape(annotations)[0]):
        min_x = int(annotations[i, 3])
        min_y = int(annotations[i, 2])
        max_x = int(annotations[i, 3] + annotations[i, 5])
        max_y = int(annotations[i, 2] + annotations[i, 4])
        color = (rnd.randint(0,255), rnd.randint(0,255), rnd.randint(0,255))
        print("min " + str((min_x, min_y)))
        print("max " + str((max_x, max_y)))
        cv.rectangle(img, (min_x, min_y), (max_x, max_y), color, 2)
    cv.imshow("test", img)
    cv.waitKey(0)
    return 0

if __name__=="__main__":
    annotations = read_annotations()
    img = cv.imread("data/1.png")
    #cv.imshow("test", img)
    #cv.waitKey(0)
    draw_rectangles(img, annotations)


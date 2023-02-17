import numpy as np
import cv2 as cv
import os 
import random as rnd
from configparser import ConfigParser


dataFolder = "../synthData/"


def read_annotations(img_no):
    annotations = np.genfromtxt(gtFile, delimiter=",")
    annotations = annotations[:, :-1]
    annotations = annotations[annotations[:,0] == img_no]
    return annotations


def draw_rectangles(img, annotations, colors):
    for i in range(0,np.shape(annotations)[0]):
        id = annotations[i, 1]
        color = colors[ int(id -1) ]

        min_x = int(annotations[i, 2])
        min_y = int(annotations[i, 3])
        max_x = int(annotations[i, 2] + annotations[i, 4])
        if (max_x > 960):
            print("Width overflow")
        max_y = int(annotations[i, 3] + annotations[i, 5])
        if (max_y > 544):
            print("Height overflow")
        center_x = int((min_x + max_x)/2)
        center_y = int((min_y + max_y)/2)
        cv.rectangle(img, (min_x, min_y), (max_x, max_y), color, 2)
        cv.putText(img, str(id), (center_x, center_y), cv.FONT_HERSHEY_DUPLEX, 1, color, 1)
    return img


def orderImages(imageFiles):
    for i in range(0, len(imageFiles)):
        imageFiles[i] = imageFiles[i].replace(".jpg", "")
        imageFiles[i] = int (imageFiles[i])
    imageFiles.sort()
    return imageFiles        


def getRootFolders():
    return os.listdir(dataFolder)

def generateColors(_noOfColors):
    colors = []
    for i in range(int (_noOfColors)):
        colors.append((rnd.randint(0,255), rnd.randint(0,255), rnd.randint(0,255)))
    return colors


if __name__=="__main__":
    fourcc = cv.VideoWriter_fourcc(*'mp4v')
    config = ConfigParser()

    roots = getRootFolders()
    for root in roots:
        sequences = os.listdir(dataFolder + root + "/train/")
        for sequence in sequences:
            imgFolder = dataFolder + root + "/train/" + sequence + "/img1/"
            imageFiles = os.listdir(imgFolder)
            imagesSorted = orderImages(imageFiles)
            gtFile = dataFolder + root + "/train/" + sequence + "/gt/gt.txt"
            sequenceVideo = dataFolder + root + "/train/" + sequence + "/video.mp4"
            print("Savind video to", sequenceVideo)
            out = cv.VideoWriter(sequenceVideo, fourcc, 15.0, (960,544))
            iniFile = dataFolder + root + "/train/" + sequence + "/seqinfo.ini"
            config.read(iniFile)
            noOfColors = config.getint('Sequence', 'spawnedFish')

            idColors = []
            for img_no in imagesSorted:
                img_name = str(img_no)
                img_name = img_name.zfill(6) + ".jpg"
                img = cv.imread(imgFolder+"/"+img_name)
                #height, width, layers = img.shape
                #size = (width,height)
                #print(size)
                annotations = read_annotations(img_no)
                if (img_no == 1):
                    idColors = generateColors(noOfColors)
                img = draw_rectangles(img, annotations, idColors)
                out.write(img)

            out.release()


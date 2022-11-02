# ffmpeg -framerate 15 -i %06d.png output.avi
import cv2 as cv
import numpy as np
import os
import random as rnd
import sys

def read_annotations(img_no):
    #print(img_no)
    annotations = np.genfromtxt(gtFile, delimiter=",")
    annotations = annotations[:, :-1]
    annotations = annotations[annotations[:,0] == img_no]
    #print(annotations)
    return annotations

def draw_rectangles(img, annotations):
    for i in range(0,np.shape(annotations)[0]):
        min_x = int(annotations[i, 2])
        min_y = int(annotations[i, 3])
        max_x = int(annotations[i, 2] + annotations[i, 4])
        max_y = int(annotations[i, 3] + annotations[i, 5])
        center_x = int((min_x + max_x)/2)
        center_y = int((min_y + max_y)/2)
        color = (rnd.randint(0,255), rnd.randint(0,255), rnd.randint(0,255))
        #print("min " + str((min_x, min_y)))
        #print("max " + str((max_x, max_y)))
        cv.rectangle(img, (min_x, min_y), (max_x, max_y), color, 2)
        cv.putText(img, str(annotations[i, 1]), (center_x, center_y), cv.FONT_HERSHEY_DUPLEX, 1, color, 1)
    #cv.imshow("test", img)
    #cv.waitKey(0)
    #return 0
    return img

if __name__=="__main__":
    idx = int(sys.argv[1])
    if (idx < 10):
        idx = "0" + str(idx)

    rootDir = "synthData/brackishMOTSynth/train"
    imgFolder = rootDir + "/brackishMOTSynth-" + str(idx) + "/img1"
    gtFile = rootDir + "/brackishMOTSynth-" + str(idx) + "/gt/gt.txt"
    #outputFile = "annotatedVideos/video-" + str(idx) + ".avi"

    images = os.listdir(imgFolder)
    images_ordered = []
    img_array = []
    size = None

    for image in images:
        images_ordered.append( int(image.replace(".jpg", "")) )
    images_ordered.sort()
    print("Number of images in the sequence ", len(images_ordered))

    for img_no in images_ordered:
        img_name = str(img_no)
        img_name = img_name.zfill(6) + ".jpg"
        img = cv.imread(imgFolder+"/"+img_name)
        height, width, layers = img.shape
        size = (width,height)
        print(size)
        annotations = read_annotations(img_no)
        img = draw_rectangles(img, annotations)
        cv.imshow(img_name, img)
        cv.waitKey(0)
        cv.destroyAllWindows()
        #filename = "annotatedVideos/" + img_name
        #cv.imwrite(filename, img)

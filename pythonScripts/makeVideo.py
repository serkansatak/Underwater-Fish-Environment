import numpy as np
import cv2 as cv
import os 

dataFolder = "../synthData/"

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
        if (max_x > 960):
            print("Width overflow")
        max_y = int(annotations[i, 3] + annotations[i, 5])
        if (max_y > 544):
            print("Height overflow")
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

def orderImages(imageFiles):
    for i in range(0, len(imageFiles)):
        imageFiles[i] = imageFiles[i].replace(".jpg", "")
        imageFiles[i] = int (imageFiles[i])

    imageFiles.sort()

    for i in range(0, len(imageFiles)):
        imageFiles[i] = str (imageFiles[i])
        imageFiles[i] = imageFiles[i].zfill(6)
        imageFiles[i] = imageFiles[i] + ".jpg"
        
    return imageFiles        


        
def getRootFolders():
    return os.listdir(dataFolder)

if __name__=="__main__":
    fourcc = cv.VideoWriter_fourcc(*'mp4v')

    roots = getRootFolders()
    for root in roots:
        sequences = os.listdir(dataFolder + root + "/train/")
        for sequence in sequences:
            imgFolder = dataFolder + root + "/train/" + sequence + "/img1/"
            imageFiles = os.listdir(imgFolder)
            imageFilesSorted = orderImages(imageFiles)
            gtFile = dataFolder + root + "/train/" + sequence + "/gt/gt.txt"
            sequenceVideo = dataFolder + root + "/train/" + sequence + "/video.mp4"
            out = cv.VideoWriter(sequenceVideo, fourcc, 15.0, (960,544))

            for image in imageFilesSorted:
                img = cv.imread(imgFolder + image)
                out.write(img)
            
            out.release()


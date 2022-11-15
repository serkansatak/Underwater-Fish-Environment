import os
import cv2 as cv

path ="synthData"
#we shall store all the file names in this list
filelist = []

for root, dirs, files in os.walk(path):
	for file in files:
        #append the file name to the list
		filelist.append(os.path.join(root,file))

#print all the file names
for name in filelist:
    if "000001.jpg" in name:
        print(name)
        img = cv.imread(name)
        cv.imshow("temp", img)
        cv.waitKey(0)
        cv.destroyAllWindows()
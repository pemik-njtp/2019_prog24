import cv2
import numpy as np
import time

array_to_bytes = []
encoded_array = []
counter = 0
current_start = 0
index = 1

def encoding() :
    global array_to_bytes, encoded_array
    print("encoding")
    i = 0
    number = 0
    for idx in array_to_bytes :
        i += 1
        number += idx * 2**(8-i)
        if (i%8 == 0) :
            encoded_array.append(number)
            i = 0
            number = 0

def write_to_file() :
    global encoded_array, array_to_bytes
    encoding()
    byte_array_to_write = bytearray(encoded_array)
    new_file = open("mikro_out","wb")
    new_file.write(byte_array_to_write)
    new_file.close()
    count = 0
    for i in array_to_bytes :
        print(i,end='')
        count += 1
        if (count % 3 == 0) :
            print()

def setup_points(cam) :
    '''
    cv2.rectangle(cam, (280,260), (290, 270), (0,255,0), 2)
    cv2.rectangle(cam, (305,260), (315, 270), (0,255,0), 2)
    cv2.rectangle(cam, (330,260), (340, 270), (0,255,0), 2)
    '''
    cam[270,285] = [0,255,0]
    cam[270,311] = [0,255,0]
    cam[270,338] = [0,255,0]

def clickevent(event, x, y, flags, param) :
    if event == cv2.EVENT_LBUTTONDOWN:
        print(x,'-',y,' color: ',param[y,x])


current_millis = lambda : int(time.time()*1000)
camera = cv2.VideoCapture(1)
camera.set(cv2.CAP_PROP_EXPOSURE, -12)
camera.set(cv2.CAP_PROP_FRAME_WIDTH, 240)
camera.set(cv2.CAP_PROP_FRAME_HEIGHT, 180)

while camera.isOpened() :
    ret, frame = camera.read()
    setup_points(frame)
    cv2.imshow('asdf',frame)
    cv2.setMouseCallback('asdf',clickevent,frame)
    if(cv2.waitKey(1) & 0xFF == ord('q')) :
        break

    if (np.any(frame[266, 280] > 230) & np.any(frame[266,307] > 230) & np.all(frame[266,331] > 230)) :
        print("waiting")
    else:
        print("go")
        time.sleep(0.175)
        break

current_start = current_millis()

while camera.isOpened() :
    ret, frame = camera.read()
    led1=led2=led3=0
    if np.any(frame[266,280] > 240) :
        led1 = 1
    if np.any(frame[266,307] > 240) :
        led2 = 1
    if np.any(frame[266,331] > 240) :
        led3 = 1
    array_to_bytes.append(led3)
    array_to_bytes.append(led2)
    array_to_bytes.append(led1)
    time_to_sleep = index /35 - (current_millis() - current_start)*0.001
    #print(time_to_sleep)
    time.sleep(time_to_sleep)
    index += 1
    cv2.imshow('asdf',frame)
    if(cv2.waitKey(1) & 0xFF == ord('q')) :
        break

write_to_file()
camera.release()
cv2.destroyAllWindows()

import cv2

class Capture:

    def __init__(self, src, size):
        self.cap = cv2.VideoCapture(src)
        self.width = size[0]
        self.height = size[1]
        self.fps = (int)(self.cap.get(cv2.CAP_PROP_FPS))
        self.frameCount = (int)(self.cap.get(cv2.CAP_PROP_FRAME_COUNT))
    
    def read(self):
        ret, frame = self.cap.read()
        if ret == False: return None
        frame = cv2.resize(frame, (self.width, self.height))
        return frame
    
    def close(self):
        self.cap.release()

class Writer:

    def __init__(self, path, fps, size):
        self.wrt = cv2.VideoWriter(path, cv2.VideoWriter_fourcc(*'XVID'), fps, size)
    
    def write(self, frame):
        self.wrt.write(frame)

    def close(self):
        self.wrt.release()
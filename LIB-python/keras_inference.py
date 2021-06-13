import cv2 as cv
from tensorflow.keras import models

class KerasInference:

    def __init__(self, filename):
        self.model = models.load_model(filename)

    def run_img_1_to_img(self, frame):
        frame = frame.transpose((1, 0))
        frame = frame.reshape((1,) + frame.shape)
        frame = frame.reshape((1,) + frame.shape)
        frame = frame.astype('float32') / 255.0
        mask = self.model.predict(frame)
        mask = mask.reshape(mask.shape[1:3]).transpose(1, 0) * 255
        mask = mask.astype('uint8')
        return mask

    def run_img_3_to_img(self, frame):
        cv.cvtColor(frame, cv.COLOR_BGR2RGB)
        frame = frame.transpose((1, 0, 2))
        frame = frame.reshape((1,) + frame.shape)
        frame = frame.astype('float32') / 255.0
        mask = self.model.predict(frame)
        mask = mask.reshape(mask.shape[1:3]).transpose(1, 0) * 255
        mask = mask.astype('uint8')
        return mask

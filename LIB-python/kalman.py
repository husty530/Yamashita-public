import cv2
import numpy as np

class Kalman:

    # -------- Constructor -------- #

    # Design filter shape
    def __init__(self, initState, measureNoise, processNoise):
        self.k = len(initState)
        self.m = len(initState)
        self.filter = cv2.KalmanFilter(self.k, self.m, type = cv2.CV_64F)
        self.filter.statePre = np.array(initState).T
        self.filter.statePost = np.array(initState).T
        self.filter.measurementMatrix = np.eye(self.m, self.k, dtype = 'float64')
        self.filter.transitionMatrix = np.eye(self.k, self.k, dtype = 'float64')
        self.filter.processNoiseCov = processNoise * np.eye(self.k, self.k, dtype = 'float64')
        self.filter.measurementNoiseCov = measureNoise * np.eye(self.m, self.m, dtype = 'float64')
        self.filter.errorCovPost = 1. * np.eye(self.k, self.k, dtype = 'float64')
    

    # -------- Methods -------- #

    # arg    ... Measurement vector
    # return ... Correct & Predict vector
    def update(self, measurement):
        correct = self.filter.correct(measurement)
        predict = self.filter.predict()
        return correct, predict
    
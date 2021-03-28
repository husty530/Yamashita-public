#pragma once

#include <opencv2/opencv.hpp>

using namespace std;

class Kalman
{

private:

	int k = 0;
	int m = 0;
	int n = 0;
	unsigned int type = CV_64F;
	cv::Mat VecToMat(vector<double> vec, int rows, int cols);
	cv::KalmanFilter _kalman;

public:

	Kalman();
	Kalman(vector<double> initialStateVec, double measurementNoise = 0.01, double processNoise = 0.01, double preError = 1);
	Kalman(vector<double> initialStateVec, vector<double> transitionMatrix, vector<double> measurementMatrix, double measurementNoise = 0.01, double processNoise = 0.01, double preError = 1.0);
	Kalman(vector<double> initialStateVec, vector<double> transitionMatrix, vector<double> measurementMatrix, double measurementNoiseMatrix[], double processNoiseMatrix[], double preErrorMatrix[]);
	Kalman(vector<double> initialStateVec, vector<double> controlMatrix, double measurementNoise = 0.01, double processNoise = 0.01, double preError = 1.0);
	Kalman(vector<double> initialStateVec, vector<double> controlMatrix, vector<double> transitionMatrix, vector<double> measurementMatrix, double measurementNoise = 0.01, double processNoise = 0.01, double preError = 1.0);
	Kalman(vector<double> initialStateVec, vector<double> controlMatrix, vector<double> transitionMatrix, vector<double> measurementMatrix, double measurementNoiseMatrix[], double processNoiseMatrix[], double preErrorMatrix[]);
	void Update(vector<double>& measurementVec, double controlVec[] = nullptr);

};
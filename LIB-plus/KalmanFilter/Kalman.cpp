#include "Kalman.h"

Kalman::Kalman() {

}

Kalman::Kalman(std::vector<double> initialStateVec, double measurementNoise, double processNoise, double preError) {

	k = initialStateVec.size();
	m = initialStateVec.size();
	_kalman = cv::KalmanFilter(k, m, 0, type);
	_kalman.statePre = VecToMat(initialStateVec, k, 1);
	_kalman.statePost = VecToMat(initialStateVec, k, 1);
	_kalman.transitionMatrix = cv::Mat::zeros(cv::Size(k, k), type);
	_kalman.measurementMatrix = cv::Mat::zeros(cv::Size(m, k), type);
	_kalman.measurementNoiseCov = cv::Mat::zeros(cv::Size(m, m), type);
	_kalman.processNoiseCov = cv::Mat::zeros(cv::Size(k, k), type);
	_kalman.errorCovPre = cv::Mat::zeros(cv::Size(k, k), type);
	for (int i = 0; i < k; i++) {
		_kalman.transitionMatrix.at<double>(i, i) = 1;
		_kalman.measurementMatrix.at<double>(i, i) = 1;
		_kalman.measurementNoiseCov.at<double>(i, i) = measurementNoise;
		_kalman.processNoiseCov.at<double>(i, i) = processNoise;
		_kalman.errorCovPre.at<double>(i, i) = preError;
	}
}

Kalman::Kalman(std::vector<double> initialStateVec, std::vector<double> transitionMatrix, std::vector<double> measurementMatrix, double measurementNoise, double processNoise, double preError) {

	k = initialStateVec.size();
	m = measurementMatrix.size() / k;
	_kalman = cv::KalmanFilter(k, m, 0, type);
	_kalman.statePre = VecToMat(initialStateVec, k, 1);
	_kalman.statePost = VecToMat(initialStateVec, k, 1);
	_kalman.transitionMatrix = VecToMat(transitionMatrix, k, k);
	_kalman.measurementMatrix = VecToMat(measurementMatrix, m, k);
	_kalman.measurementNoiseCov = cv::Mat::zeros(cv::Size(m, m), type);
	for (int i = 0; i < m; i++) {
		_kalman.measurementNoiseCov.at<double>(i, i) = measurementNoise;
	}
	_kalman.processNoiseCov = cv::Mat::zeros(cv::Size(k, k), type);
	_kalman.errorCovPre = cv::Mat::zeros(cv::Size(k, k), type);
	for (int i = 0; i < k; i++) {
		_kalman.processNoiseCov.at<double>(i, i) = processNoise;
		_kalman.errorCovPre.at<double>(i, i) = preError;
	}

}
Kalman::Kalman(std::vector<double> initialStateVec, std::vector<double> transitionMatrix, std::vector<double> measurementMatrix, double measurementNoiseMatrix[], double processNoiseMatrix[], double preErrorMatrix[]) {

	k = initialStateVec.size();
	m = measurementMatrix.size() / k;
	_kalman = cv::KalmanFilter(k, m, 0, type);
	_kalman.statePre = VecToMat(initialStateVec, k, 1);
	_kalman.statePost = VecToMat(initialStateVec, k, 1);
	_kalman.transitionMatrix = VecToMat(transitionMatrix, k, k);
	_kalman.measurementMatrix = VecToMat(measurementMatrix, m, k);
	_kalman.measurementNoiseCov = cv::Mat(m, m, type, measurementNoiseMatrix);
	_kalman.processNoiseCov = cv::Mat(k, k, type, processNoiseMatrix);
	_kalman.errorCovPre = cv::Mat(k, k, type, preErrorMatrix);

}
Kalman::Kalman(std::vector<double> initialStateVec, std::vector<double> controlMatrix, double measurementNoise, double processNoise, double preError) {

	k = initialStateVec.size();
	m = initialStateVec.size();
	n = controlMatrix.size() / k;
	_kalman = cv::KalmanFilter(k, m, 0, type);
	_kalman.statePre = VecToMat(initialStateVec, k, 1);
	_kalman.statePost = VecToMat(initialStateVec, k, 1);
	_kalman.controlMatrix = VecToMat(controlMatrix, k, n);
	_kalman.transitionMatrix = cv::Mat::zeros(cv::Size(k, k), type);
	_kalman.measurementMatrix = cv::Mat::zeros(cv::Size(m, k), type);
	_kalman.measurementNoiseCov = cv::Mat::zeros(cv::Size(m, m), type);
	_kalman.processNoiseCov = cv::Mat::zeros(cv::Size(k, k), type);
	_kalman.errorCovPre = cv::Mat::zeros(cv::Size(k, k), type);
	for (int i = 0; i < k; i++) {
		_kalman.transitionMatrix.at<double>(i, i) = 1;
		_kalman.measurementMatrix.at<double>(i, i) = 1;
		_kalman.measurementNoiseCov.at<double>(i, i) = measurementNoise;
		_kalman.processNoiseCov.at<double>(i, i) = processNoise;
		_kalman.errorCovPre.at<double>(i, i) = preError;
	}

}
Kalman::Kalman(std::vector<double> initialStateVec, std::vector<double> controlMatrix, std::vector<double> transitionMatrix, std::vector<double> measurementMatrix, double measurementNoise, double processNoise, double preError) {

	k = initialStateVec.size();
	m = measurementMatrix.size() / k;
	n = controlMatrix.size() / k;
	_kalman = cv::KalmanFilter(k, m, 0, type);
	_kalman.statePre = VecToMat(initialStateVec, k, 1);
	_kalman.statePost = VecToMat(initialStateVec, k, 1);
	_kalman.controlMatrix = VecToMat(controlMatrix, k, n);
	_kalman.transitionMatrix = VecToMat(transitionMatrix, k, k);
	_kalman.measurementMatrix = VecToMat(measurementMatrix, m, k);
	_kalman.measurementNoiseCov = cv::Mat::zeros(cv::Size(m, m), type);
	for (int i = 0; i < m; i++) {
		_kalman.measurementNoiseCov.at<double>(i, i) = measurementNoise;
	}
	_kalman.processNoiseCov = cv::Mat::zeros(cv::Size(k, k), type);
	_kalman.errorCovPre = cv::Mat::zeros(cv::Size(k, k), type);
	for (int i = 0; i < k; i++) {
		_kalman.processNoiseCov.at<double>(i, i) = processNoise;
		_kalman.errorCovPre.at<double>(i, i) = preError;
	}

}
Kalman::Kalman(std::vector<double> initialStateVec, std::vector<double> controlMatrix, std::vector<double> transitionMatrix, std::vector<double> measurementMatrix, double measurementNoiseMatrix[], double processNoiseMatrix[], double preErrorMatrix[]) {

	k = initialStateVec.size();
	m = measurementMatrix.size() / k;
	n = controlMatrix.size() / k;
	_kalman.statePre = VecToMat(initialStateVec, k, 1);
	_kalman.statePost = VecToMat(initialStateVec, k, 1);
	_kalman.controlMatrix = VecToMat(controlMatrix, k, n);
	_kalman.transitionMatrix = VecToMat(transitionMatrix, k, k);
	_kalman.measurementMatrix = VecToMat(measurementMatrix, m, k);
	_kalman.measurementNoiseCov = cv::Mat(m, m, type, measurementNoiseMatrix);
	_kalman.processNoiseCov = cv::Mat(k, k, type, processNoiseMatrix);
	_kalman.errorCovPre = cv::Mat(k, k, type, preErrorMatrix);

}

void Kalman::Update(std::vector<double>& measurementVec, double controlVec[]) {

	_kalman.correct(VecToMat(measurementVec, m, 1));
	cv::Mat resultMat;
	if (controlVec == nullptr) {
		resultMat = _kalman.predict();
	}
	else {
		resultMat = _kalman.predict(cv::Mat(n, 1, type, controlVec));
	}
	for (int i = 0; i < m; i++) {
		measurementVec[i] = resultMat.at<double>(i);
	}
}

cv::Mat Kalman::VecToMat(std::vector<double> vec, int rows, int cols) {

	auto matrix = cv::Mat(rows, cols, type);
	for (int i = 0; i < rows; i++) {
		for (int j = 0; j < cols; j++) {
			matrix.at<double>(i, j) = vec[i * cols + j];
		}
	}
	return matrix;
}
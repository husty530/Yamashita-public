#pragma once

#include <opencv2/opencv.hpp>
#include <opencv2/dnn.hpp>

using namespace std;
using namespace cv;

struct YoloResults {

	int count;
	vector<int> classIds;
	vector<Point> centers;
	vector<float> confidences;
	vector<Rect2d> boxes;

};

class Yolo
{

public:

	enum GraphicMode { None, Points, Boxes, BoxesWithLabels };
	enum WithCUDA { On, Off };

	/// <summary>
	/// Initialize again using another constructor
	/// </summary>
	Yolo();

	/// <summary>
	/// Initialize YOLO Detector
	/// </summary>
	/// <param name="cfg">Path</param>
	/// <param name="names">Path</param>
	/// <param name="weights">Path</param>
	/// <param name="mode"></param>
	/// <param name="cuda">On or Off</param>
	/// <param name="size">Width and Height must be multiple of 32</param>
	/// <param name="confThresh"></param>
	/// <param name="nmsThresh"></param>
	Yolo(const char* cfg, const char* names, const char* weights, GraphicMode mode = BoxesWithLabels, WithCUDA cuda = Off, Size size = Size(384, 288), float confThresh = 0.5, float nmsThresh = 0.3);

	/// <summary>
	/// Inference and Store results in struct
	/// </summary>
	/// <param name="img">Input and Output matrix</param>
	/// <param name="results"></param>
	void Run(Mat& img, YoloResults& results);

private:

	float _confThresh;
	float _nmsThresh;
	Size _blob_size;
	vector<string> _labels;
	vector<Scalar> _colors;
	dnn::Net _net;
	GraphicMode _mode;
	void Process(Mat& img, YoloResults& results);
	void DrawPoints(Mat& img, YoloResults& results);
	void DrawRects(Mat& img, YoloResults& results);
	void DrawRectsWithLabels(Mat& img, YoloResults& results);

};
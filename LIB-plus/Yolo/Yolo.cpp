#include <fstream>
#include "Yolo.h"

using namespace std;
using namespace cv;
using namespace dnn;

Yolo::Yolo() {}

Yolo::Yolo(const char* cfg, const char* names, const char* weights, GraphicMode mode, WithCUDA cuda, Size size, float confThresh, float nmsThresh)
{

    _confThresh = confThresh;
    _nmsThresh = nmsThresh;
    _blob_size = size;
    _mode = mode;

    string buf;
    ifstream ifs(names);
    while (getline(ifs, buf))
    {
        _labels.push_back(buf);
        auto color = Scalar(rand() % 256, rand() % 256, rand() % 256);
        _colors.push_back(color);
    }

    _net = readNetFromDarknet(cfg, weights);
    if (cuda == On)
    {
        _net.setPreferableBackend(DNN_BACKEND_CUDA);
        _net.setPreferableTarget(DNN_TARGET_CUDA);
    }
    else
    {
        _net.setPreferableBackend(DNN_BACKEND_OPENCV);
        _net.setPreferableTarget(DNN_TARGET_CPU);
    }

}

void Yolo::Run(Mat& img, YoloResults& results)
{

    Process(img, results);

    switch (_mode)
    {
    case None:
        break;
    case Points:
        DrawPoints(img, results);
        break;
    case Boxes:
        DrawRects(img, results);
        break;
    case BoxesWithLabels:
        DrawRectsWithLabels(img, results);
        break;
    }

}

void Yolo::Process(Mat& img, YoloResults& results)
{

    results.count = 0;
    results.classIds.clear();
    results.centers.clear();
    results.confidences.clear();
    results.boxes.clear();

    vector<int> classIds;
    vector<Rect2d> boxes;
    vector<float> confidences;

    vector<Mat> outs;
    vector<String> outNames = _net.getUnconnectedOutLayersNames();
    auto blob = blobFromImage(img, 1.0f / 255, _blob_size, Scalar(), true, false);
    _net.setInput(blob);
    _net.forward(outs, outNames);

    for (Mat pred : outs)
    {
        float* data = (float*)pred.data;
        for (int i = 0; i < pred.rows; i++, data += pred.cols)
        {
            Point classIdPoint;
            double confidence = (double)data[4];
            if (confidence > _confThresh)
            {
                double probability;
                minMaxLoc(pred.row(i).colRange(5, pred.cols), 0, &probability, 0, &classIdPoint);
                if (probability > _confThresh)
                {
                    auto centerX = (int)(data[0] * img.cols);
                    auto centerY = (int)(data[1] * img.rows);
                    auto width = (int)(data[2] * img.cols);
                    auto height = (int)(data[3] * img.rows);
                    classIds.push_back(classIdPoint.x);
                    confidences.push_back(confidence);
                    boxes.push_back(Rect2d(centerX, centerY, width, height));
                }
            }
        }
    }

    MatShape indices;
    NMSBoxes(boxes, confidences, _confThresh, _nmsThresh, indices);

    for (int i : indices)
    {
        auto box = boxes[i];
        results.count++;
        results.classIds.push_back(classIds[i]);
        results.centers.push_back(Point(box.x, box.y));
        results.confidences.push_back(confidences[i]);
        results.boxes.push_back(box);
    }

}

void Yolo::DrawPoints(Mat& img, YoloResults& results)
{

    for (int i = 0; i < results.count; i++)
    {
        circle(img, results.centers[i], 3, _colors[results.classIds[i]], FILLED);
    }

}

void Yolo::DrawRects(Mat& img, YoloResults& results)
{

    for (int i = 0; i < results.count; i++)
    {
        auto box = results.boxes[i];
        int left = box.x - box.width / 2;
        int top = box.y - box.height / 2;
        rectangle(img, Point(left, top), Point(left + box.width, top + box.height), _colors[results.classIds[i]]);
    }

}

void Yolo::DrawRectsWithLabels(Mat& img, YoloResults& results)
{

    for (int i = 0; i < results.count; i++)
    {
        auto box = results.boxes[i];
        int left = box.x - box.width / 2;
        int top = box.y - box.height / 2;
        rectangle(img, Point(left, top), Point(left + box.width, top + box.height), _colors[results.classIds[i]]);
        auto label = _labels[results.classIds[i]] + format(" %d", (int)(results.confidences[i] * 100)) + "%";
        int baseLine;
        auto labelSize = getTextSize(label, FONT_HERSHEY_SIMPLEX, 0.3, 1, &baseLine);
        rectangle(img, Point(left, max(top, labelSize.height) - labelSize.height), Point(left + labelSize.width, max(top, labelSize.height) + baseLine), _colors[results.classIds[i]], FILLED);
        putText(img, label, Point(left, max(top, labelSize.height)), FONT_HERSHEY_SIMPLEX, 0.3, Scalar());
    }

}
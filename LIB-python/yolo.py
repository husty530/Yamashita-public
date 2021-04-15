import cv2
import enum
import numpy as np

class YoloResults:
    
    def __init__(self):
        self.labels = []
        self.confidences = []
        self.centers = []
        self.sizes = []
        self.count = 0

    def add(self, label, confidence, center, size):
        self.labels.append(label)
        self.confidences.append(confidence)
        self.centers.append(center)
        self.sizes.append(size)
        self.count += 1

    def clear(self):
        self.labels.clear()
        self.confidences.clear()
        self.centers.clear()
        self.sizes.clear()
        self.count = 0

class Drawmode(enum.Enum):

    rectangle = 0
    point = 1
    off = 2

class Yolo:


    # -------- Constructor -------- #

    # Load model
    def __init__(self, cfg, names, weights, blobsize, drawmode, conf_thresh, nms_thresh):
        self.net = cv2.dnn.readNetFromDarknet(cfg, weights)
        self.net.setPreferableBackend(cv2.dnn.DNN_BACKEND_OPENCV)
        self.net.setPreferableTarget(cv2.dnn.DNN_TARGET_CPU)
        self.blobsize = blobsize
        self.conf_thresh = conf_thresh
        self.nms_thresh = nms_thresh
        self.drawmode = drawmode
        self.classes = open(names).read().strip().split('\n')
        np.random.seed(1)
        self.colors = np.random.randint(0, 255, size=(len(self.classes), 3), dtype='uint8')


    # -------- Methods -------- #

    # arg    ... Input image
    # return ... List of detection result
    def run(self, frame):
        blob = cv2.dnn.blobFromImage(frame, 1.0 / 255, self.blobsize, 0, True, False)
        self.net.setInput(blob)
        ln = self.net.getLayerNames()
        ln = [ln[i[0] - 1] for i in self.net.getUnconnectedOutLayers()]
        outs = self.net.forward(ln)
        centers = []
        boxes = []
        confidences = []
        classIDs = []
        h, w = frame.shape[:2]

        for output in outs:
            for detection in output:
                scores = detection[5:]
                classID = np.argmax(scores)
                confidence = detection[4]
                if confidence > self.conf_thresh:
                    box = detection[:4] * np.array([w, h, w, h])
                    centerX, centerY, width, height = box.astype("int")
                    center = [centerX, centerY]
                    centers.append(center)
                    x = int(centerX - (width / 2))
                    y = int(centerY - (height / 2))
                    box = [x, y, int(width), int(height)]
                    boxes.append(box)
                    confidences.append(float(confidence))
                    classIDs.append(classID)
        
        results = YoloResults()
        indices = cv2.dnn.NMSBoxes(boxes, confidences, self.conf_thresh, self.nms_thresh)
        if len(indices) > 0:
            for i in indices.flatten():
                x, y = boxes[i][0], boxes[i][1]
                w, h = boxes[i][2], boxes[i][3]
                results.add(self.classes[classIDs[i]], confidences[i], centers[i], boxes[i])
                if self.drawmode == Drawmode.rectangle:
                    color = [int(c) for c in self.colors[classIDs[i]]]
                    cv2.rectangle(frame, (x, y), (x + w, y + h), color, 2)
                    text = "{}:{:.1f}%".format(self.classes[classIDs[i]], confidences[i] * 100)
                    cv2.putText(frame, text, (x, y - 5), cv2.FONT_HERSHEY_SIMPLEX, 0.3, (0, 0, 0), 1)
                elif self.drawmode == Drawmode.point:
                    color = [int(c) for c in self.colors[classIDs[i]]]
                    cv2.circle(frame, (int(x + w / 2), int(y + h / 2)), 3, color, 4)
        return results
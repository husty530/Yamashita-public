import numpy as np
import cv2
from nnabla.utils.nnp_graph import NnpLoader

class NnablaInference:


    # -------- Constructor -------- #

    # Load model
    def __init__(self, filename, input_layer_name, output_layer_name):
        nnp = NnpLoader(filename)
        net = nnp.get_network('MainRuntime', batch_size=1)
        self.input = net.inputs[input_layer_name]
        self.output = net.outputs[output_layer_name]


    # -------- Methods -------- #

    # arg    ... 1-dimensional vector
    # return ... Output layer label / vector array
    def run_vec_to_label(self, vec):
        vec = np.expand_dims(vec, 0)
        self.input.d = vec
        self.output.forward()
        return self.output.d[0]
    
    # arg    ... Gray-scale image
    # return ... Output layer label / vector array
    def run_img_1_to_label(self, frame):
        frame = np.expand_dims(frame, 0)
        frame = np.expand_dims(frame, 0)
        self.input.d = frame / 255.0
        self.output.forward()
        return self.output.d[0]

    # arg    ... Color image (BGR)
    # return ... Output layer label / vector array
    def run_img_3_to_label(self, frame):
        frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        frame = np.expand_dims(frame, 0)
        self.input.d = frame.transpose(0, 3, 1, 2) / 255.0
        self.output.forward()
        return self.output.d[0]

    # arg    ... Gray-scale image
    # return ... Gray-scale image
    def run_img_1_to_img(self, frame):
        frame = np.expand_dims(frame, 0)
        frame = np.expand_dims(frame, 0)
        self.input.d = frame / 255.0
        self.output.forward()
        return (self.output.d[0][0] * 255).astype('uint8')

    # arg    ... Color image (BGR)
    # return ... Gray-scale image / Color image (RGB)
    def run_img_3_to_img(self, frame):
        frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        frame = np.expand_dims(frame, 0)
        self.input.d = frame.transpose(0, 3, 1, 2) / 255.0
        self.output.forward()
        return (self.output.d[0].transpose(1, 2, 0) * 255).astype('uint8')
        
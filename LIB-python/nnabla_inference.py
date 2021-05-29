import numpy as np
import cv2 as cv
from nnabla.utils.nnp_graph import NnpLoader

class NnablaInference:

    def __init__(self, filename, input_layer_name, output_layer_name):
        nnp = NnpLoader(filename)
        net = nnp.get_network('MainRuntime', batch_size=1)
        self.input = net.inputs[input_layer_name]
        self.output = net.outputs[output_layer_name]

    def run_vec_to_label(self, vec):
        vec = np.expand_dims(vec, 0)
        self.input.d = vec
        self.output.forward()
        return self.output.d[0]
    
    def run_img_1_to_label(self, frame):
        frame = np.expand_dims(frame, 0)
        frame = np.expand_dims(frame, 0)
        self.input.d = frame / 255.0
        self.output.forward()
        return self.output.d[0]

    def run_img_3_to_label(self, frame):
        frame = np.expand_dims(cv.cvtColor(frame, cv.COLOR_BGR2RGB), 0)
        self.input.d = frame.transpose(0, 3, 1, 2) / 255.0
        self.output.forward()
        return self.output.d[0]

    def run_img_1_to_img(self, frame):
        frame = np.expand_dims(frame, 0)
        frame = np.expand_dims(frame, 0)
        self.input.d = frame / 255.0
        self.output.forward()
        return (self.output.d[0][0] * 255).astype('uint8')

    def run_img_3_to_img(self, frame):
        frame = np.expand_dims(cv.cvtColor(frame, cv.COLOR_BGR2RGB), 0)
        self.input.d = frame.transpose(0, 3, 1, 2) / 255.0
        self.output.forward()
        return (self.output.d[0].transpose(1, 2, 0) * 255).astype('uint8')
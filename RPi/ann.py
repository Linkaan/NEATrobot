import json
import math
import serial
import zmq

SPEED = 400
CHANGE_TOLERANCE = 3
SLOW_DOWN_FACTOR = 0.5
INPUT_COEFFICIENT = 1.0 / 1.0
FILTER_FACTOR = 0.7
 
class NeuronType:
    Input = 0
    Hidden = 1
    Bias = 2
    Output = 3

def sigmoid(activation, response):
    return 1.0 / (1.0 + math.exp(-activation / response))

def createNeuronLookupTable(neurons):
    neuronById = {}
    for neuron in neurons:
        neuronById[neuron['neuronID']] = neuron
    return neuronById

def updateANN(ann, inputs):
    outputs = []

    neurons = ann['neurons']
    neuronCount = len(neurons)

    if 'neuronById' not in ann:
        ann['neuronById'] = createNeuronLookupTable(neurons)

    neuronById = ann['neuronById']

    neuronIndex = 0

    # update input neurons
    while neurons[neuronIndex]['type'] == NeuronType.Input:
        neurons[neuronIndex]['output'] = inputs[neuronIndex]
        neuronIndex += 1
    
    # update bias to 1
    neurons[neuronIndex]['output'] = 1
    neuronIndex += 1

    # iterate through hidden and output neurons
    while neuronIndex < neuronCount:
        # the total sum of adding up all activations from neurons linking
        # to the current neuron.
        sum = 0.0

        neuron = neurons[neuronIndex]

        linkIndex = 0
        linkCount = len(neuron['linksTo'])

        # calculate sum of all activations for neurons linking to the current neuron
        while linkIndex < linkCount:
            link = neuron['linksTo'][linkIndex]
            sum += link['weight'] * neuronById[link['fromNeuron']]['output']
            linkIndex += 1

        # apply the activation function to get non-linear output
        neuron['output'] = sigmoid(sum, neuron['activationResponse'])

        if neuron['type'] == NeuronType.Output:
            outputs.append(neuron['output'])

        neuronIndex += 1
    
    return outputs

if __name__ == "__main__":
    ann = json.loads(r'{"neurons":[{"linksTo":[],"linksFrom":[{"fromNeuron":0,"toNeuron":6,"weight":0.3998056650161743,"isRecurrent":false},{"fromNeuron":0,"toNeuron":9,"weight":0.5525347590446472,"isRecurrent":false},{"fromNeuron":0,"toNeuron":39,"weight":-1.4688773155212403,"isRecurrent":false}],"sumActivation":0.0,"output":0.7231108546257019,"type":0,"neuronID":0,"activationResponse":1.0,"splitX":0.2857142984867096,"splitY":0.0},{"linksTo":[],"linksFrom":[{"fromNeuron":1,"toNeuron":6,"weight":1.3139477968215943,"isRecurrent":false},{"fromNeuron":1,"toNeuron":7,"weight":-1.841086983680725,"isRecurrent":false},{"fromNeuron":1,"toNeuron":9,"weight":0.33449316024780276,"isRecurrent":false}],"sumActivation":0.0,"output":1.1387290954589844,"type":0,"neuronID":1,"activationResponse":1.0778632164001465,"splitX":0.4285714626312256,"splitY":0.0},{"linksTo":[],"linksFrom":[{"fromNeuron":2,"toNeuron":6,"weight":0.09775090217590332,"isRecurrent":false},{"fromNeuron":2,"toNeuron":7,"weight":1.0683479309082032,"isRecurrent":false},{"fromNeuron":2,"toNeuron":9,"weight":2.705871105194092,"isRecurrent":false}],"sumActivation":0.0,"output":5.0,"type":0,"neuronID":2,"activationResponse":1.0,"splitX":0.5714285969734192,"splitY":0.0},{"linksTo":[],"linksFrom":[{"fromNeuron":3,"toNeuron":6,"weight":-0.4662399888038635,"isRecurrent":false},{"fromNeuron":3,"toNeuron":7,"weight":3.3073086738586427,"isRecurrent":false}],"sumActivation":0.0,"output":1.3970988988876343,"type":0,"neuronID":3,"activationResponse":0.9197852611541748,"splitX":0.7142857313156128,"splitY":0.0},{"linksTo":[],"linksFrom":[{"fromNeuron":4,"toNeuron":6,"weight":-0.8872920274734497,"isRecurrent":false},{"fromNeuron":4,"toNeuron":7,"weight":-0.9433706402778626,"isRecurrent":false},{"fromNeuron":4,"toNeuron":9,"weight":0.8649682402610779,"isRecurrent":false},{"fromNeuron":4,"toNeuron":39,"weight":-1.6333357095718384,"isRecurrent":false}],"sumActivation":0.0,"output":0.6981958746910095,"type":0,"neuronID":4,"activationResponse":1.0,"splitX":0.8571429252624512,"splitY":0.0},{"linksTo":[],"linksFrom":[{"fromNeuron":5,"toNeuron":6,"weight":0.8984636068344116,"isRecurrent":false},{"fromNeuron":5,"toNeuron":7,"weight":-0.8724162578582764,"isRecurrent":false}],"sumActivation":0.0,"output":1.0,"type":2,"neuronID":5,"activationResponse":1.0,"splitX":0.1428571492433548,"splitY":0.0},{"linksTo":[{"fromNeuron":0,"toNeuron":6,"weight":0.3998056650161743,"isRecurrent":false},{"fromNeuron":1,"toNeuron":6,"weight":1.3139477968215943,"isRecurrent":false},{"fromNeuron":2,"toNeuron":6,"weight":0.09775090217590332,"isRecurrent":false},{"fromNeuron":3,"toNeuron":6,"weight":-0.4662399888038635,"isRecurrent":false},{"fromNeuron":4,"toNeuron":6,"weight":-0.8872920274734497,"isRecurrent":false},{"fromNeuron":5,"toNeuron":6,"weight":0.8984636068344116,"isRecurrent":false},{"fromNeuron":7,"toNeuron":6,"weight":-0.5298008918762207,"isRecurrent":false},{"fromNeuron":9,"toNeuron":6,"weight":-0.15490210056304933,"isRecurrent":false}],"linksFrom":[{"fromNeuron":6,"toNeuron":7,"weight":-1.1464767456054688,"isRecurrent":false}],"sumActivation":0.0,"output":0.7721230983734131,"type":3,"neuronID":6,"activationResponse":1.0,"splitX":0.3333333432674408,"splitY":1.0},{"linksTo":[{"fromNeuron":1,"toNeuron":7,"weight":-1.841086983680725,"isRecurrent":false},{"fromNeuron":2,"toNeuron":7,"weight":1.0683479309082032,"isRecurrent":false},{"fromNeuron":3,"toNeuron":7,"weight":3.3073086738586427,"isRecurrent":false},{"fromNeuron":4,"toNeuron":7,"weight":-0.9433706402778626,"isRecurrent":false},{"fromNeuron":5,"toNeuron":7,"weight":-0.8724162578582764,"isRecurrent":false},{"fromNeuron":9,"toNeuron":7,"weight":-0.3332200050354004,"isRecurrent":false},{"fromNeuron":6,"toNeuron":7,"weight":-1.1464767456054688,"isRecurrent":false},{"fromNeuron":39,"toNeuron":7,"weight":0.11120444536209107,"isRecurrent":false}],"linksFrom":[{"fromNeuron":7,"toNeuron":6,"weight":-0.5298008918762207,"isRecurrent":false}],"sumActivation":0.0,"output":0.9941025376319885,"type":3,"neuronID":7,"activationResponse":1.0,"splitX":0.6666666865348816,"splitY":1.0},{"linksTo":[{"fromNeuron":0,"toNeuron":9,"weight":0.5525347590446472,"isRecurrent":false},{"fromNeuron":4,"toNeuron":9,"weight":0.8649682402610779,"isRecurrent":false},{"fromNeuron":2,"toNeuron":9,"weight":2.705871105194092,"isRecurrent":false},{"fromNeuron":1,"toNeuron":9,"weight":0.33449316024780276,"isRecurrent":false},{"fromNeuron":39,"toNeuron":9,"weight":0.16371417045593263,"isRecurrent":false}],"linksFrom":[{"fromNeuron":9,"toNeuron":7,"weight":-0.3332200050354004,"isRecurrent":false},{"fromNeuron":9,"toNeuron":6,"weight":-0.15490210056304933,"isRecurrent":false}],"sumActivation":0.0,"output":0.9999996423721314,"type":1,"neuronID":9,"activationResponse":1.0,"splitX":0.2857142984867096,"splitY":0.0},{"linksTo":[{"fromNeuron":4,"toNeuron":39,"weight":-1.6333357095718384,"isRecurrent":false},{"fromNeuron":0,"toNeuron":39,"weight":-1.4688773155212403,"isRecurrent":false}],"linksFrom":[{"fromNeuron":39,"toNeuron":9,"weight":0.16371417045593263,"isRecurrent":false},{"fromNeuron":39,"toNeuron":7,"weight":0.11120444536209107,"isRecurrent":false}],"sumActivation":0.0,"output":0.09952177852392197,"type":1,"neuronID":39,"activationResponse":1.0,"splitX":0.8571429252624512,"splitY":0.0}],"depth":2}')
    ser = serial.Serial('/dev/ttyACM0', 115200)
    last_sign = [1, 1]
    same = 2    

    context = zmq.Context()
    ipc_sock = context.socket(zmq.REQ)
    ipc_sock.connect("tcp://127.0.0.1:3000")

    sign = lambda x: (1, -1)[x < 0]
    has_first_measurement = False

    print("running ANN")
    while True:
        try:
            if has_first_measurement == False:
                has_first_measurement = True
                inputs = [i * INPUT_COEFFICIENT for i in json.loads(ser.readline())["sensors"]]                
            else:
                sensors = json.loads(ser.readline())["sensors"]
                for i in range(len(sensors)):
                    inputs[i] = FILTER_FACTOR * inputs[i] + (1.0 - FILTER_FACTOR) * (sensors[i] * INPUT_COEFFICIENT)
            outputs = updateANN(ann, inputs)[::-1]
        except (KeyboardInterrupt, SystemExit):
            raise            
        except:            
            print("failed to update ann")
            continue

        speed = SPEED
        sign_now = [sign(outputs[0] - 0.5), sign(outputs[1] - 0.5)]
        if last_sign == sign_now:
            same += 1
        else:
            if same < CHANGE_TOLERANCE or sign_now[::-1] == last_sign:
                print("slowing down...")
                speed *= SLOW_DOWN_FACTOR
            same = 0
        last_sign = sign_now

        print("outputs: " + str(["{0:0.2f}".format(i) for i in outputs]) + " \t inputs: " + str(["{0:0.2f}".format(i) for i in inputs]))        
        ser.write(json.dumps({"speeds":  [round((i - 0.5) * speed) for i in outputs]}).encode())
        ser.write("\n".encode())
        ipc_sock.send(json.dumps({"ann": {"inputs": inputs, "outputs": outputs}}))
        ipc_sock.recv()
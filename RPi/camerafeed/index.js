process.on('uncaughtException', function(err) {
  console.log('Caught exception: ' + err);
  console.log(err.stack);
});

const express = require('express');
const raspividStream = require('raspivid-stream');
const zmq = require('zmq');

const app = express();
const wss = require('express-ws')(app);

var ipc_sock = zmq.socket('rep');

ipc_sock.identity = 'server' + process.pid;

ipc_sock.bind("tcp://127.0.0.1:3000", (err) => {
    if (err) throw err;
    console.log("Bound IPC to port 3000");

    ipc_sock.on('message', (data) => {
        ipc_sock.send(0); //response
    });
});

app.get('/', (req, res) => res.sendFile(__dirname + '/index.html'));

app.ws('/video-stream', (ws, req) => {
    console.log('Client connected');    

    ws.send(JSON.stringify({
      action: 'init',
      width: '960',
      height: '540'
    }));

    var videoStream = raspividStream({ rotation: 180 });

    videoStream.on('data', (data) => {
        ws.send(data, { binary: true }, (error) => { if (error) console.error(error); });
    });

    ipc_sock.on('message', (data) => {
        parsed = JSON.parse(data);
        if (parsed['ann']) {
            console.log("send data to client");
            ws.send(JSON.stringify(parsed));
        }
    });

    ws.on('close', () => {
        console.log('Client left');
        videoStream.removeAllListeners('data');
    });
});

app.use(function (err, req, res, next) {
  console.error(err);
  next(err);
})

app.listen(80, () => console.log('Server started on 80'));

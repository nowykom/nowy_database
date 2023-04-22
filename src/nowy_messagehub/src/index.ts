import * as http from "http";
import * as socket_io from "socket.io";

const port = parseInt(String(process.argv[2] || 5000));

interface ServerToClientEvents {
  'v1:broadcast_event': (...data: any[]) => void;
}

interface ClientToServerEvents {
  'v1:broadcast_event': (...data: any[]) => void;
}

interface InterServerEvents {
  ping: () => void;
}

interface SocketData {
  name: string;
  age: number;
}


console.log(`Server: start on port ${port}`);

process.on('uncaughtException', function (exception) {
  // handle or ignore error
  console.log(exception);
});

const httpServer = http.createServer();
const io = new socket_io.Server<
        ClientToServerEvents,
        ServerToClientEvents,
        InterServerEvents,
        SocketData
>(httpServer, { /* options */});

io.on("connection", (socket) => {
  socket.on("v1:broadcast_event", function (...values) {
    const event_name = values[0];
    console.log(`event_name: ${JSON.stringify(event_name)}`);
    console.log(`values: ${JSON.stringify(values)}`);

    io.sockets.emit(`v1:broadcast_event`, ...values);
    socket.broadcast.emit(`v1:broadcast_event`, ...values);
  });
});

httpServer.listen(port);


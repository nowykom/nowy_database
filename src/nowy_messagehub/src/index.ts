import * as http from "http";
import * as socket_io from "socket.io";

const port = parseInt(String(process.argv[2] || 5000));

interface ServerToClientEvents {
  'v1:broadcast_event': (category: string, data: string) => void;
}

interface ClientToServerEvents {
  'v1:broadcast_event': (category: string, data: string) => void;
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
  socket.on("v1:broadcast_event", function (event_name, data) {
    console.log(`event_name: ${JSON.stringify(event_name)}`);
    console.log(`data: ${JSON.stringify(data)}`);
    // @ts-ignore
    socket.broadcast.emit(`v1:broadcast_event`, event_name, data);
  });
});

httpServer.listen(port);


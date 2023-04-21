import {createServer} from "http";
import {Server} from "socket.io";

const port = parseInt(String(process.argv[2] || 5000));

interface ServerToClientEvents {
  noArg: () => void;
  basicEmit: (a: number, b: string, c: Buffer) => void;
  withAck: (d: string, callback: (e: number) => void) => void;
}

interface ClientToServerEvents {
  hello: () => void;
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

const httpServer = createServer();
const io = new Server<
        ClientToServerEvents,
        ServerToClientEvents,
        InterServerEvents,
        SocketData
>(httpServer, { /* options */});

io.on("connection", (socket) => {
  // ...
});


httpServer.listen(port);


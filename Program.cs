using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

using System.Reflection;
using System.Linq;

public class ClientState{
    public Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    public byte[] readBuffer = new byte[1024];
    public int hp = -100;
    public float x = 0;
    public float y = 0;
    public float z = 0;
    public float eulY = 0;  
}

class MainClass{
    static Socket listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    public static Dictionary<Socket, ClientState> clientsDict = new Dictionary<Socket, ClientState>();
    public static void Main(string[] args){
        // Prepare for bind & linsten
		IPEndPoint ipEp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8890);
		listenfd.Bind(ipEp);

		listenfd.Listen(0);
		Console.WriteLine("[Server]initial succ");

        // Prepare a checklist for Select Socket
        List<Socket> checkList = new List<Socket>();
        while(true){
            // initial the socket checklist
            checkList.Clear();
            checkList.Add(listenfd);
            foreach(ClientState s in clientsDict.Values){
                checkList.Add(s.socket);
            }
            // select the socket which is fine
            Socket.Select(checkList, null, null, 1000);
            // handle the readable socket
            foreach(Socket s in checkList){
                if(s == listenfd){
                    ReadListenfd(s);
                }else{
                    ReadClientfd(s);
                }
            }
        }
    }
    public static void ReadListenfd(Socket listenfd){
        Socket clientfd = listenfd.Accept();
        ClientState state = new ClientState();
        state.socket = clientfd;
        clientsDict.Add(clientfd, state);
        Console.WriteLine("Accept Socket: " + clientfd);
    }
    public static void ReadClientfd(Socket clientfd){
        ClientState state = clientsDict[clientfd];
        int count = 0;
        try{
            count = clientfd.Receive(state.readBuffer);
        }catch(SocketException ex){
            MethodInfo mei = typeof(EventHandler).GetMethod("OnDisconnect")!;
            object[] ob = {state};
            mei.Invoke(null, ob);

            clientfd.Close();
            clientsDict.Remove(clientfd);
            Console.WriteLine("Receive SocketEx " + ex.ToString());
            return;
        }
        if(count == 0){
            MethodInfo mei = typeof(EventHandler).GetMethod("OnDisconnect")!;
            object[] ob = {state};
            mei.Invoke(null, ob);

            clientfd.Close();
            clientsDict.Remove(clientfd);
            Console.WriteLine("Socket close");
            return;
        }
        string recvStr = System.Text.Encoding.Default.GetString(state.readBuffer, 0, count);
        Console.WriteLine("Receive " + recvStr);
        string[] split = recvStr.Split('|');
        string msgName = split[0];
        string msgArgs = split[1];

        MethodInfo mi = typeof(MsgHandler).GetMethod("Msg" + msgName)!;
        object[] o = {state, msgArgs};
        mi.Invoke(null, o);
        
        byte[] sendBytes = System.Text.Encoding.Default.GetBytes(recvStr);
        foreach(ClientState cs in clientsDict.Values){
            cs.socket.Send(sendBytes);
        }
        return;
    }
}
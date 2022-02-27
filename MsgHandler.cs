using System;
using System.Collections.Generic;

class MsgHandler{
    public static void MsgEnter(ClientState c, string msgArgs){
        string[] split = msgArgs.Split(',');
        string decs = split[0];
        
        c.hp = 100;
        c.x = float.Parse(split[1]);
        c.y = float.Parse(split[2]);
        c.z = float.Parse(split[3]);
        c.eulY = float.Parse(split[4]);

        string sendStr = "Enter|" + msgArgs;
        byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
        foreach(ClientState cs in MainClass.clientsDict.Values){
            cs.socket.Send(sendBytes);
        }
    }
    public static void MsgList(ClientState c, string msgArgs){
        string sendStr = "List|";
        foreach(ClientState cs in MainClass.clientsDict.Values){
            sendStr += cs.socket.RemoteEndPoint!.ToString() +",";
            sendStr += cs.x.ToString() +",";
            sendStr += cs.y.ToString() +",";
            sendStr += cs.z.ToString() +",";
            sendStr += cs.eulY.ToString() +",";
            sendStr += cs.hp.ToString() +",";
        }
        byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
        c.socket.Send(sendBytes);
    }
    public static void MsgMove(ClientState c, string msgArgs){
        string sendStr = "Move|";

        string[] split = msgArgs.Split(',');
        string decs = split[0];
        c.x = float.Parse(split[1]);
        c.y = float.Parse(split[2]);
        c.z = float.Parse(split[3]);

        sendStr += msgArgs;
        byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);

        foreach(ClientState cs in MainClass.clientsDict.Values){
            cs.socket.Send(sendBytes);
        }
    }
    
}
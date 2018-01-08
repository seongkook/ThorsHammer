using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;


public class ForceSend : MonoBehaviour
{

    string IP = "127.0.0.1";
    int port = 8888;
    IPEndPoint remoteEndPoint;
    UdpClient client;

    public ForceSend(string ip)
    {
        //IP = ip;
        init();
    }



    void FixedUpdate()
    {
        // Dummy force
        //Vector3 force = new Vector3(0, 1.0f, 0f);
        //SendForce(transform.rotation, force);
    }

    public void init()
    {
        print("Initializing UDP");

        if (IP.Length < 7)
            IP = "127.0.0.1";

        if (port < 1)
            port = 8888;

        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
        client = new UdpClient();
    }



    public void SendForce(Quaternion device, Vector3 force)
    {
        Vector3[] deviceVector = getRotatedVectors(device);
        Vector3 newForce = new Vector3(force.x, force.z, force.y);
        Vector3 forceVector = getWindVector(deviceVector, newForce);
        sendString("F:" + forceVector.x + "," + forceVector.z + "," + forceVector.y + "\r\n");
    }

    private void sendString(string message)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            client.Send(data, data.Length, remoteEndPoint);
        }
        catch (Exception err)
        {
            print(err.ToString());
        }
    }

    private Vector3 getWindVector(Vector3[] vectors, Vector3 force)
    {
        // Distance from plane with normal vector n to a force vector f
        // d = (n dot f) / |n|
        float dx = Vector3.Dot(vectors[0], force) / vectors[0].magnitude;
        float dy = Vector3.Dot(vectors[1], force) / vectors[1].magnitude;
        float dz = Vector3.Dot(vectors[2], force) / vectors[2].magnitude;
        return new Vector3(dx, dy, dz);
    }    

    private Vector3[] getRotatedVectors(Quaternion q)
    {
        Vector3 x = new Vector3(0, 0, -1);
        Vector3 y = new Vector3(0, -1, 0);
        Vector3 z = new Vector3(-1, 0, 0);

        x = RotateVector(q, x);
        y = RotateVector(q, y);
        z = RotateVector(q, z);

        Vector3[] vectors = new Vector3[] { x, y, z };
        return vectors;
    }

    private Vector3 RotateVector(Quaternion quat, Vector3 vec)
    {
        float num = quat.x * 2;
        float num2 = quat.y * 2;
        float num3 = quat.z * 2;
        float num4 = quat.x * num;
        float num5 = quat.y * num2;
        float num6 = quat.z * num3;
        float num7 = quat.x * num2;
        float num8 = quat.x * num3;
        float num9 = quat.y * num3;
        float num10 = quat.w * num;
        float num11 = quat.w * num2;
        float num12 = quat.w * num3;
        Vector3 result = new Vector3();
        result.x = (1 - (num5 + num6)) * vec.x + (num7 - num12) * vec.y + (num8 + num11) * vec.z;
        result.y = (num7 + num12) * vec.x + (1 - (num4 + num6)) * vec.y + (num9 - num10) * vec.z;
        result.z = (num8 - num11) * vec.x + (num9 + num10) * vec.y + (1 - (num4 + num5)) * vec.z;
        return result;
    }

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

[Serializable]
public class SITLCommsJsonIMUData {
    public float[] gyro = new float[] {0.0f, 0.0f, 0.0f};
    public float[] accel_body = new float[] {0.0f, -9.8f, 0.0f};
}

[Serializable]
public class SITLCommsJsonOutputPacket {
    public float timestamp = 0;
    public SITLCommsJsonIMUData imu = new SITLCommsJsonIMUData();
    public float[] position = new float[] {0.0f, 0.0f, 0.0f};
    public float[] attitude = new float[] {0.0f, 0.0f, 0.0f};
    public float[] velocity = new float[] {0.0f, 0.0f, 0.0f};
}

public class RoverComms : MonoBehaviour {

    public int localPort = 9002;
    
    UdpClient socketReceive;
    UdpClient socketSend;
    Thread receiveThread;

    public Rigidbody roverRb;
    public RoverPhysics roverPhysics;

    private bool hasRemoteConnection;
    IPEndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);

    SITLCommsJsonOutputPacket data = new SITLCommsJsonOutputPacket();

    private long startTime;

    void Start() {
        Debug.Log("Starting rover comms thread on port " + localPort);
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();

        data.timestamp = 0.0f;
        data.imu = new SITLCommsJsonIMUData();
        data.imu.gyro = new float[] {0.0f, 0.0f, 0.0f};
        data.imu.accel_body = new float[] {0.0f, -9.8f, 0.0f};
        data.position = new float[] {0.0f, 0.0f, 0.0f};
        data.attitude = new float[] {0.0f, 0.0f, 0.0f};
        data.velocity = new float[] {0.0f, 0.0f, 0.0f};
        
        startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }

    void OnDisable() {
        if (receiveThread!= null) {
            receiveThread.Abort();
        }
        if (socketReceive != null) {
            socketReceive.Close();
        }
        Debug.Log("Stopping rover comms thread");
    }

    void Update()
    {
        data.timestamp = (DateTimeOffset.Now.ToUnixTimeMilliseconds() - startTime) / 1000.0f;
        data.imu.gyro = new float[] {-roverRb.angularVelocity.x* Mathf.Deg2Rad, -roverRb.angularVelocity.z* Mathf.Deg2Rad, roverRb.angularVelocity.y* Mathf.Deg2Rad};
        data.imu.accel_body = new float[] {0.0f, 0.0f, -9.8f};
        data.position = new float[] {-roverRb.position.x, roverRb.position.z, roverRb.position.y};
        data.attitude = new float[] {transform.eulerAngles.x* Mathf.Deg2Rad, -transform.eulerAngles.z* Mathf.Deg2Rad, transform.eulerAngles.y* Mathf.Deg2Rad};
        data.velocity = new float[] {-roverRb.velocity.x, roverRb.velocity.z, roverRb.velocity.y};

        // data.imu.gyro = new float[] {0.0f, 0.0f, 0.0f};
        // data.imu.accel_body = new float[] {0.0f, -9.8f, 0.0f};
        // data.position = new float[] {0.0f, 0.0f, 0.0f};
        // data.attitude = new float[] {0.0f, 0.0f, 0.0f};
        // data.velocity = new float[] {0.0f, 0.0f, 0.0f};
    }
    
    private void ReceiveData() {
        socketReceive = new UdpClient(localPort);
        socketSend = new UdpClient();

        while (true) {
            try {
                byte[] data = socketReceive.Receive(ref remoteEndpoint);
                if (!hasRemoteConnection) {
                    hasRemoteConnection = true;
                    Debug.Log("Received new connection from SITL: " + remoteEndpoint.ToString());
                } else {
                    //Debug.Log("Received data from SITL at addr " + remoteEndpoint.ToString());

                    var reader = new BinaryReader(new MemoryStream(data), Encoding.UTF8, false);

                    UInt16 magic = reader.ReadUInt16();
                    UInt16 frameRate = reader.ReadUInt16();
                    UInt32 frameCount = reader.ReadUInt32();
                    UInt16[] pwm = new UInt16[16];
                    for (var i = 0; i < 16; i++) {
                        pwm[i] = reader.ReadUInt16();
                    }
                    roverPhysics.rawPWM = pwm;
                }
            } catch (Exception err) {
                Debug.LogException(err);
            }

            // Send telemetry packet to remote endpoint
            if (hasRemoteConnection) {
                String telemStr = JsonUtility.ToJson(data) + "\n";
                byte[] byteData = Encoding.UTF8.GetBytes(telemStr);
                socketSend.Send(byteData, byteData.Length, remoteEndpoint);
            }
            
        }
    }
}

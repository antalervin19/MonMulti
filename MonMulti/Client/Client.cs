﻿using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

namespace MonMulti
{
    public class Client
    {
        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isConnected = false;

        public event Action<string> OnMessageReceived;

        public async Task<bool> ConnectToServerAsync(string ip, int port)
        {
            try
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(ip, port);
                _networkStream = _tcpClient.GetStream();
                _isConnected = true;
                _cancellationTokenSource = new CancellationTokenSource();

                Debug.Log("Connected to server!");

                _ = ListenForMessagesAsync(_cancellationTokenSource.Token);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Connection error: {ex.Message}");
                return false;
            }
        }

        private async Task ListenForMessagesAsync(CancellationToken cancellationToken)
        {
            try
            {
                byte[] buffer = new byte[1024];

                while (_isConnected && !cancellationToken.IsCancellationRequested)
                {
                    int bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    if (bytesRead == 0) break;

                    string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Debug.Log($"[RECEIVED] {message}");
                    OnMessageReceived?.Invoke(message);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Listening task canceled.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error while receiving JSON: {ex.Message}");
            }
        }

        public void Disconnect()
        {
            if (!_isConnected) return;
            _isConnected = false;

            _cancellationTokenSource?.Cancel();
            _networkStream?.Close();
            _tcpClient?.Close();

            Debug.Log("Disconnected from server.");
        }
    }
}

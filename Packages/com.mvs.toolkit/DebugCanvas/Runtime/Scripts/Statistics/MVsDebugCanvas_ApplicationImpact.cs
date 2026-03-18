using UnityEngine;
using TMPro;
using System;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace MVsToolkit.DebugCanvas.Statitics
{
    public class MVsDebugCanvas_ApplicationImpact : MonoBehaviour
    {
        [Header("TextMeshPro References")]
        public TextMeshProUGUI cpuText;
        public TextMeshProUGUI ramText;
        public TextMeshProUGUI netText;

        private TimeSpan lastCpuTime;
        private float lastCheckTime;

        private long lastBytesSent;
        private long lastBytesReceived;

        void Start()
        {
            lastCpuTime = Process.GetCurrentProcess().TotalProcessorTime;
            lastCheckTime = Time.time;

            lastBytesSent = GetTotalBytesSent();
            lastBytesReceived = GetTotalBytesReceived();
        }

        void Update()
        {
            UpdateCPU();
            UpdateRAM();
            UpdateNetwork();
        }

        // ---------------- CPU ----------------
        void UpdateCPU()
        {
            float now = Time.time;
            float delta = now - lastCheckTime;
            if (delta <= 0.2f) return;

            var proc = Process.GetCurrentProcess();
            TimeSpan newCpuTime = proc.TotalProcessorTime;

            double cpuUsedMs = (newCpuTime - lastCpuTime).TotalMilliseconds;

            int cpuCores = SystemInfo.processorCount;

            // Normalisation sur 100%
            double cpuPercent = (cpuUsedMs / (delta * 1000.0)) * 100.0 / cpuCores;

            cpuText.text = $"CPU : {cpuPercent:F1}%";

            lastCpuTime = newCpuTime;
            lastCheckTime = now;
        }

        // ---------------- RAM ----------------
        void UpdateRAM()
        {
            float totalMB = SystemInfo.systemMemorySize;
            float usedMB = (GC.GetTotalMemory(false) / (1024f * 1024f));
            float percent = (usedMB / totalMB) * 100f;

            ramText.text = $"RAM : {usedMB:F0} Mo / {totalMB} Mo ({percent:F1}%)";
        }

        // ---------------- NETWORK ----------------
        void UpdateNetwork()
        {
            float now = Time.time;
            float delta = now - lastCheckTime;
            if (delta <= 0) return;

            long sent = GetTotalBytesSent();
            long received = GetTotalBytesReceived();

            long deltaSent = sent - lastBytesSent;
            long deltaReceived = received - lastBytesReceived;

            float totalBits = (deltaSent + deltaReceived) * 8f;
            float mbitsPerSec = (totalBits / delta) / 1_000_000f;

            netText.text = $"Network : {mbitsPerSec:F2} Mbit/s";

            lastBytesSent = sent;
            lastBytesReceived = received;
        }

        // ---------------- NETWORK HELPERS ----------------
        long GetTotalBytesSent()
        {
            long total = 0;
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                total += ni.GetIPv4Statistics().BytesSent;
            return total;
        }

        long GetTotalBytesReceived()
        {
            long total = 0;
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                total += ni.GetIPv4Statistics().BytesReceived;
            return total;
        }
    }
}
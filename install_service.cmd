@echo off
sc.exe create "hyperv_exporter" DisplayName="Hyper-V Exporter for Prometheus" binPath="%~dp0\hyperv_exporter.exe" start="delayed-auto"
sc.exe start "hyperv_exporter"

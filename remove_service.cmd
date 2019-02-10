@echo off
sc.exe stop "hyperv_exporter"
sc.exe delete "hyperv_exporter"

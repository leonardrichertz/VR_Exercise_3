VR Übung 3 - Tracking

Es wurde ein (HMD) Tracking-System implementiert, welches Daten von Trägheitssensoren (IMU) mit optischem Marker-basiertem Kamera-Tracking kombiniert.

Die korrigierte APK sowie der Code befinden sich im Correction-Branch.

Beide Ausgabeteile wurden in der Szene „Aufgabe_A” gelöst.

ÜBERBLICK
=========
Diese VR-Anwendung demonstriert ein HMD-Tracking-System, das Daten von simulierten Trägheitssensoren (IMU) mit optischem Kamera-Tracking kombiniert. 

WAS WIRD GETRACKT?
==================
Das System trackt die Position und Orientierung des VR-Headsets (HMD) des Nutzers.

WAS SIEHT DER NUTZER?
====================
- ROTE KUGEL: Ihre berechnete HMD-Position 
- GRAUE QUADER: Tracking-Kameras (ändern Farbe je nach Status):
  * GRÜN: Kamera kann HMD erfolgreich tracken
  * ROT: Kamera kann HMD nicht sehen (verdeckt)
  * GELB: HMD außerhalb der Reichweite
  * GRAU: HMD außerhalb des Kamera-Sichtfelds
- WÄNDE: Hindernisse zum Verstecken vor den Kameras

WIE FUNKTIONIERT DIE APP?
========================
1. Bewegen Sie sich frei in der VR-Umgebung
2. Beobachten Sie, wie sich die Farben der Kameras ändern
3. Verstecken Sie sich hinter Wänden → Kameras werden rot
4. Hören Sie auf Audio-Signale bei Tracking-Verlust

TRACKING-MODI
=============
- OPTISCHES TRACKING (≥2 Kameras sehen HMD): Hohe Genauigkeit
- IMU-FALLBACK (Kameras blockiert): Weniger genau, akkumuliert Fehler
- AUDIO-WARNUNG: Ertönt bei Tracking-Verlust

INTERAKTIVE ELEMENTE
===================
- 4 Wände in verschiedenen Höhen zum Verstecken
- Mehrere Tracking-Kameras mit unterschiedlichen Blickwinkeln
- Echtzeit-Visualisierung des Tracking-Status

ZIEL DER DEMONSTRATION
=====================
Verstehen Sie die Grenzen verschiedener Tracking-Technologien:
- Optisches Tracking: Präzise aber anfällig für Verdeckung
- IMU-Tracking: Funktioniert immer, aber driftet über Zeit
- Sensor-Fusion: Kombiniert beide für optimales Ergebnis
